using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace org.apache.pulsar.client.impl
{
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;

	using ByteBuf = io.netty.buffer.ByteBuf;



	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using EncryptionKeyInfo = org.apache.pulsar.client.api.EncryptionKeyInfo;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using CryptoException = org.apache.pulsar.client.api.PulsarClientException.CryptoException;
	using PulsarByteBufAllocator = org.apache.pulsar.common.allocator.PulsarByteBufAllocator;
	using EncryptionKeys = org.apache.pulsar.common.api.proto.PulsarApi.EncryptionKeys;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;
	using ASN1ObjectIdentifier = org.bouncycastle.asn1.ASN1ObjectIdentifier;
	using PrivateKeyInfo = org.bouncycastle.asn1.pkcs.PrivateKeyInfo;
	using SubjectPublicKeyInfo = org.bouncycastle.asn1.x509.SubjectPublicKeyInfo;
	using ECNamedCurveTable = org.bouncycastle.asn1.x9.ECNamedCurveTable;
	using X9ECParameters = org.bouncycastle.asn1.x9.X9ECParameters;
	using BCECPrivateKey = org.bouncycastle.jcajce.provider.asymmetric.ec.BCECPrivateKey;
	using BCECPublicKey = org.bouncycastle.jcajce.provider.asymmetric.ec.BCECPublicKey;
	using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
	using ECParameterSpec = org.bouncycastle.jce.spec.ECParameterSpec;
	using ECPrivateKeySpec = org.bouncycastle.jce.spec.ECPrivateKeySpec;
	using ECPublicKeySpec = org.bouncycastle.jce.spec.ECPublicKeySpec;
	using PEMException = org.bouncycastle.openssl.PEMException;
	using PEMKeyPair = org.bouncycastle.openssl.PEMKeyPair;
	using PEMParser = org.bouncycastle.openssl.PEMParser;
	using JcaPEMKeyConverter = org.bouncycastle.openssl.jcajce.JcaPEMKeyConverter;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class MessageCrypto
	{

		private const string ECDSA = "ECDSA";
		private const string RSA = "RSA";
		private const string ECIES = "ECIES";

		// Ideally the transformation should also be part of the message property. This will prevent client
		// from assuming hardcoded value. However, it will increase the size of the message even further.
		private const string RSA_TRANS = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string AESGCM = "AES/GCM/NoPadding";

		private static KeyGenerator keyGenerator;
		private const int tagLen = 16 * 8;
		public const int ivLen = 12;
		private sbyte[] iv = new sbyte[ivLen];
		private Cipher cipher;
		internal MessageDigest digest;
		private string logCtx;

		// Data key which is used to encrypt message
		private SecretKey dataKey;
		private LoadingCache<ByteBuffer, SecretKey> dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private ConcurrentDictionary<string, EncryptionKeyInfo> encryptedDataKeyMap;

		internal static readonly SecureRandom secureRandom;
		static MessageCrypto()
		{

			Security.addProvider(new BouncyCastleProvider());
			SecureRandom rand = null;
			try
			{
				rand = SecureRandom.getInstance("NativePRNGNonBlocking");
			}
			catch (NoSuchAlgorithmException)
			{
				rand = new SecureRandom();
			}

			secureRandom = rand;

			// Initial seed
			secureRandom.NextBytes(new sbyte[ivLen]);
		}

		public MessageCrypto(string logCtx, bool keyGenNeeded)
		{

			this.logCtx = logCtx;
			encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			dataKeyCache = CacheBuilder.newBuilder().expireAfterAccess(4, TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				cipher = Cipher.getInstance(AESGCM, BouncyCastleProvider.PROVIDER_NAME);
				// If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					digest = MessageDigest.getInstance("MD5");

					dataKey = null;
					return;
				}
				keyGenerator = KeyGenerator.getInstance("AES");
				int aesKeyLength = Cipher.getMaxAllowedKeyLength("AES");
				if (aesKeyLength <= 128)
				{
					log.warn("{} AES Cryptographic strength is limited to {} bits. Consider installing JCE Unlimited Strength Jurisdiction Policy Files.", logCtx, aesKeyLength);
					keyGenerator.init(aesKeyLength, secureRandom);
				}
				else
				{
					keyGenerator.init(256, secureRandom);
				}

			}
			catch (Exception e) when (e is NoSuchAlgorithmException || e is NoSuchProviderException || e is NoSuchPaddingException)
			{

				cipher = null;
				log.error("{} MessageCrypto initialization Failed {}", logCtx, e.Message);

			}

			// Generate data key to encrypt messages
			dataKey = keyGenerator.generateKey();

			iv = new sbyte[ivLen];
		}

		private class CacheLoaderAnonymousInnerClass : CacheLoader<ByteBuffer, SecretKey>
		{
			private readonly MessageCrypto outerInstance;

			public CacheLoaderAnonymousInnerClass(MessageCrypto outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override SecretKey load(ByteBuffer key)
			{
				return null;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.PublicKey loadPublicKey(byte[] keyBytes) throws Exception
		private PublicKey loadPublicKey(sbyte[] keyBytes)
		{

			Reader keyReader = new StringReader(StringHelper.NewString(keyBytes));
			PublicKey publicKey = null;
			try
			{
					using (PEMParser pemReader = new PEMParser(keyReader))
					{
					object pemObj = pemReader.readObject();
					JcaPEMKeyConverter pemConverter = new JcaPEMKeyConverter();
					SubjectPublicKeyInfo keyInfo = null;
					X9ECParameters ecParam = null;
        
					if (pemObj is ASN1ObjectIdentifier)
					{
        
						// make sure this is EC Parameter we're handling. In which case
						// we'll store it and read the next object which should be our
						// EC Public Key
        
						ASN1ObjectIdentifier ecOID = (ASN1ObjectIdentifier) pemObj;
						ecParam = ECNamedCurveTable.getByOID(ecOID);
						if (ecParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + ((ASN1ObjectIdentifier) pemObj).Id);
						}
        
						pemObj = pemReader.readObject();
					}
					else if (pemObj is X9ECParameters)
					{
						ecParam = (X9ECParameters) pemObj;
						pemObj = pemReader.readObject();
					}
        
					if (pemObj is org.bouncycastle.cert.X509CertificateHolder)
					{
						keyInfo = ((org.bouncycastle.cert.X509CertificateHolder) pemObj).SubjectPublicKeyInfo;
					}
					else
					{
						keyInfo = (SubjectPublicKeyInfo) pemObj;
					}
					publicKey = pemConverter.getPublicKey(keyInfo);
        
					if (ecParam != null && ECDSA.Equals(publicKey.Algorithm))
					{
						ECParameterSpec ecSpec = new ECParameterSpec(ecParam.Curve, ecParam.G, ecParam.N, ecParam.H, ecParam.Seed);
						KeyFactory keyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPublicKeySpec keySpec = new ECPublicKeySpec(((BCECPublicKey) publicKey).Q, ecSpec);
						publicKey = (PublicKey) keyFactory.generatePublic(keySpec);
					}
					}
			}
			catch (Exception e) when (e is IOException || e is NoSuchAlgorithmException || e is NoSuchProviderException || e is InvalidKeySpecException)
			{
				throw new Exception(e);
			}
			return publicKey;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.PrivateKey loadPrivateKey(byte[] keyBytes) throws Exception
		private PrivateKey loadPrivateKey(sbyte[] keyBytes)
		{

			Reader keyReader = new StringReader(StringHelper.NewString(keyBytes));
			PrivateKey privateKey = null;
			try
			{
					using (PEMParser pemReader = new PEMParser(keyReader))
					{
					X9ECParameters ecParam = null;
        
					object pemObj = pemReader.readObject();
        
					if (pemObj is ASN1ObjectIdentifier)
					{
        
						// make sure this is EC Parameter we're handling. In which case
						// we'll store it and read the next object which should be our
						// EC Private Key
        
						ASN1ObjectIdentifier ecOID = (ASN1ObjectIdentifier) pemObj;
						ecParam = ECNamedCurveTable.getByOID(ecOID);
						if (ecParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + ecOID.Id);
						}
        
						pemObj = pemReader.readObject();
        
					}
					else if (pemObj is X9ECParameters)
					{
        
						ecParam = (X9ECParameters) pemObj;
						pemObj = pemReader.readObject();
					}
        
					if (pemObj is PEMKeyPair)
					{
        
						PrivateKeyInfo pKeyInfo = ((PEMKeyPair) pemObj).PrivateKeyInfo;
						JcaPEMKeyConverter pemConverter = new JcaPEMKeyConverter();
						privateKey = pemConverter.getPrivateKey(pKeyInfo);
        
					}
        
					// if our private key is EC type and we have parameters specified
					// then we need to set it accordingly
        
					if (ecParam != null && ECDSA.Equals(privateKey.Algorithm))
					{
						ECParameterSpec ecSpec = new ECParameterSpec(ecParam.Curve, ecParam.G, ecParam.N, ecParam.H, ecParam.Seed);
						KeyFactory keyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPrivateKeySpec keySpec = new ECPrivateKeySpec(((BCECPrivateKey) privateKey).S, ecSpec);
						privateKey = (PrivateKey) keyFactory.generatePrivate(keySpec);
					}
        
					}
			}
			catch (IOException e)
			{
				throw new Exception(e);
			}
			return privateKey;
		}

		/*
		 * Encrypt data key using the public key(s) in the argument. <p> If more than one key name is specified, data key is
		 * encrypted using each of those keys. If the public key is expired or changed, application is responsible to remove
		 * the old key and add the new key <p>
		 *
		 * @param keyNames List of public keys to encrypt data key
		 *
		 * @param keyReader Implementation to read the key values
		 *
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void addPublicKeyCipher(java.util.Set<String> keyNames, org.apache.pulsar.client.api.CryptoKeyReader keyReader) throws org.apache.pulsar.client.api.PulsarClientException.CryptoException
		public virtual void addPublicKeyCipher(ISet<string> keyNames, CryptoKeyReader keyReader)
		{
			lock (this)
			{
        
				// Generate data key
				dataKey = keyGenerator.generateKey();
        
				foreach (string key in keyNames)
				{
					addPublicKeyCipher(key, keyReader);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addPublicKeyCipher(String keyName, org.apache.pulsar.client.api.CryptoKeyReader keyReader) throws org.apache.pulsar.client.api.PulsarClientException.CryptoException
		private void addPublicKeyCipher(string keyName, CryptoKeyReader keyReader)
		{

			if (string.ReferenceEquals(keyName, null) || keyReader == null)
			{
				throw new PulsarClientException.CryptoException("Keyname or KeyReader is null");
			}

			// Read the public key and its info using callback
			EncryptionKeyInfo keyInfo = keyReader.getPublicKey(keyName, null);

			PublicKey pubKey;

			try
			{
				pubKey = loadPublicKey(keyInfo.Key);
			}
			catch (Exception e)
			{
				string msg = logCtx + "Failed to load public key " + keyName + ". " + e.Message;
				log.error(msg);
				throw new PulsarClientException.CryptoException(msg);
			}

			Cipher dataKeyCipher = null;
			sbyte[] encryptedKey;

			try
			{

				// Encrypt data key using public key
				if (RSA.Equals(pubKey.Algorithm))
				{
					dataKeyCipher = Cipher.getInstance(RSA_TRANS, BouncyCastleProvider.PROVIDER_NAME);
				}
				else if (ECDSA.Equals(pubKey.Algorithm))
				{
					dataKeyCipher = Cipher.getInstance(ECIES, BouncyCastleProvider.PROVIDER_NAME);
				}
				else
				{
					string msg = logCtx + "Unsupported key type " + pubKey.Algorithm + " for key " + keyName;
					log.error(msg);
					throw new PulsarClientException.CryptoException(msg);
				}
				dataKeyCipher.init(Cipher.ENCRYPT_MODE, pubKey);
				encryptedKey = dataKeyCipher.doFinal(dataKey.Encoded);

			}
			catch (Exception e) when (e is IllegalBlockSizeException || e is BadPaddingException || e is NoSuchAlgorithmException || e is NoSuchProviderException || e is NoSuchPaddingException || e is InvalidKeyException)
			{
				log.error("{} Failed to encrypt data key {}. {}", logCtx, keyName, e.Message);
				throw new PulsarClientException.CryptoException(e.Message);
			}
			EncryptionKeyInfo eki = new EncryptionKeyInfo(encryptedKey, keyInfo.Metadata);
			encryptedDataKeyMap[keyName] = eki;
		}

		/*
		 * Remove a key <p> Remove the key identified by the keyName from the list of keys.<p>
		 *
		 * @param keyName Unique name to identify the key
		 *
		 * @return true if succeeded, false otherwise
		 */
		/*
		 */
		public virtual bool removeKeyCipher(string keyName)
		{

			if (string.ReferenceEquals(keyName, null))
			{
				return false;
			}
			encryptedDataKeyMap.Remove(keyName);
			return true;
		}

		/*
		 * Encrypt the payload using the data key and update message metadata with the keyname & encrypted data key
		 *
		 * @param encKeys One or more public keys to encrypt data key
		 *
		 * @param msgMetadata Message Metadata
		 *
		 * @param payload Message which needs to be encrypted
		 *
		 * @return encryptedData if success
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized io.netty.buffer.ByteBuf encrypt(java.util.Set<String> encKeys, org.apache.pulsar.client.api.CryptoKeyReader keyReader, org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata.Builder msgMetadata, io.netty.buffer.ByteBuf payload) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual ByteBuf encrypt(ISet<string> encKeys, CryptoKeyReader keyReader, MessageMetadata.Builder msgMetadata, ByteBuf payload)
		{
			lock (this)
			{
        
				if (encKeys.Count == 0)
				{
					return payload;
				}
        
				// Update message metadata with encrypted data key
				foreach (string keyName in encKeys)
				{
					if (encryptedDataKeyMap[keyName] == null)
					{
						// Attempt to load the key. This will allow us to load keys as soon as
						// a new key is added to producer config
						addPublicKeyCipher(keyName, keyReader);
					}
					EncryptionKeyInfo keyInfo = encryptedDataKeyMap[keyName];
					if (keyInfo != null)
					{
						if (keyInfo.Metadata != null && !keyInfo.Metadata.Empty)
						{
							IList<KeyValue> kvList = new List<KeyValue>();
							keyInfo.Metadata.forEach((key, value) =>
							{
							kvList.Add(KeyValue.newBuilder().setKey(key).setValue(value).build());
							});
							msgMetadata.addEncryptionKeys(EncryptionKeys.newBuilder().setKey(keyName).setValue(ByteString.copyFrom(keyInfo.Key)).addAllMetadata(kvList).build());
						}
						else
						{
							msgMetadata.addEncryptionKeys(EncryptionKeys.newBuilder().setKey(keyName).setValue(ByteString.copyFrom(keyInfo.Key)).build());
						}
					}
					else
					{
						// We should never reach here.
						log.error("{} Failed to find encrypted Data key for key {}.", logCtx, keyName);
					}
        
				}
        
				// Create gcm param
				// TODO: Replace random with counter and periodic refreshing based on timer/counter value
				secureRandom.NextBytes(iv);
				GCMParameterSpec gcmParam = new GCMParameterSpec(tagLen, iv);
        
				// Update message metadata with encryption param
				msgMetadata.EncryptionParam = ByteString.copyFrom(iv);
        
				ByteBuf targetBuf = null;
				try
				{
					// Encrypt the data
					cipher.init(Cipher.ENCRYPT_MODE, dataKey, gcmParam);
        
					ByteBuffer sourceNioBuf = payload.nioBuffer(payload.readerIndex(), payload.readableBytes());
        
					int maxLength = cipher.getOutputSize(payload.readableBytes());
					targetBuf = PulsarByteBufAllocator.DEFAULT.buffer(maxLength, maxLength);
					ByteBuffer targetNioBuf = targetBuf.nioBuffer(0, maxLength);
        
					int bytesStored = cipher.doFinal(sourceNioBuf, targetNioBuf);
					targetBuf.writerIndex(bytesStored);
        
				}
				catch (Exception e) when (e is IllegalBlockSizeException || e is BadPaddingException || e is InvalidKeyException || e is InvalidAlgorithmParameterException || e is ShortBufferException)
				{
        
					targetBuf.release();
					log.error("{} Failed to encrypt message. {}", logCtx, e);
					throw new PulsarClientException.CryptoException(e.Message);
        
				}
        
				payload.release();
				return targetBuf;
			}
		}

		private bool decryptDataKey(string keyName, sbyte[] encryptedDataKey, IList<KeyValue> encKeyMeta, CryptoKeyReader keyReader)
		{

			IDictionary<string, string> keyMeta = new Dictionary<string, string>();
			encKeyMeta.ForEach(kv =>
			{
			keyMeta[kv.Key] = kv.Value;
			});

			// Read the private key info using callback
			EncryptionKeyInfo keyInfo = keyReader.getPrivateKey(keyName, keyMeta);

			// Convert key from byte to PivateKey
			PrivateKey privateKey;
			try
			{
				privateKey = loadPrivateKey(keyInfo.Key);
				if (privateKey == null)
				{
					log.error("{} Failed to load private key {}.", logCtx, keyName);
					return false;
				}
			}
			catch (Exception e)
			{
				log.error("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, keyName, e.Message);
				return false;
			}

			// Decrypt data key to decrypt messages
			Cipher dataKeyCipher = null;
			sbyte[] dataKeyValue = null;
			sbyte[] keyDigest = null;

			try
			{

				// Decrypt data key using private key
				if (RSA.Equals(privateKey.Algorithm))
				{
					dataKeyCipher = Cipher.getInstance(RSA_TRANS, BouncyCastleProvider.PROVIDER_NAME);
				}
				else if (ECDSA.Equals(privateKey.Algorithm))
				{
					dataKeyCipher = Cipher.getInstance(ECIES, BouncyCastleProvider.PROVIDER_NAME);
				}
				else
				{
					log.error("Unsupported key type {} for key {}.", privateKey.Algorithm, keyName);
					return false;
				}
				dataKeyCipher.init(Cipher.DECRYPT_MODE, privateKey);
				dataKeyValue = dataKeyCipher.doFinal(encryptedDataKey);

				keyDigest = digest.digest(encryptedDataKey);

			}
			catch (Exception e) when (e is IllegalBlockSizeException || e is BadPaddingException || e is NoSuchAlgorithmException || e is NoSuchProviderException || e is NoSuchPaddingException || e is InvalidKeyException)
			{
				log.error("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, keyName, e.Message);
				return false;
			}
			dataKey = new SecretKeySpec(dataKeyValue, "AES");
			dataKeyCache.put(ByteBuffer.wrap(keyDigest), dataKey);
			return true;
		}

		private ByteBuf decryptData(SecretKey dataKeySecret, MessageMetadata msgMetadata, ByteBuf payload)
		{

			// unpack iv and encrypted data
			ByteString ivString = msgMetadata.EncryptionParam;
			ivString.copyTo(iv, 0);

			GCMParameterSpec gcmParams = new GCMParameterSpec(tagLen, iv);
			ByteBuf targetBuf = null;
			try
			{
				cipher.init(Cipher.DECRYPT_MODE, dataKeySecret, gcmParams);

				ByteBuffer sourceNioBuf = payload.nioBuffer(payload.readerIndex(), payload.readableBytes());

				int maxLength = cipher.getOutputSize(payload.readableBytes());
				targetBuf = PulsarByteBufAllocator.DEFAULT.buffer(maxLength, maxLength);
				ByteBuffer targetNioBuf = targetBuf.nioBuffer(0, maxLength);

				int decryptedSize = cipher.doFinal(sourceNioBuf, targetNioBuf);
				targetBuf.writerIndex(decryptedSize);

			}
			catch (Exception e) when (e is InvalidKeyException || e is InvalidAlgorithmParameterException || e is IllegalBlockSizeException || e is BadPaddingException || e is ShortBufferException)
			{
				log.error("{} Failed to decrypt message {}", logCtx, e.Message);
				if (targetBuf != null)
				{
					targetBuf.release();
					targetBuf = null;
				}
			}

			return targetBuf;
		}

		private ByteBuf getKeyAndDecryptData(MessageMetadata msgMetadata, ByteBuf payload)
		{

			ByteBuf decryptedData = null;

			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeysList;

			// Go through all keys to retrieve data key from cache
			for (int i = 0; i < encKeys.Count; i++)
			{

				sbyte[] msgDataKey = encKeys[i].Value.toByteArray();
				sbyte[] keyDigest = digest.digest(msgDataKey);
				SecretKey storedSecretKey = dataKeyCache.getIfPresent(ByteBuffer.wrap(keyDigest));
				if (storedSecretKey != null)
				{

					// Taking a small performance hit here if the hash collides. When it
					// retruns a different key, decryption fails. At this point, we would
					// call decryptDataKey to refresh the cache and come here again to decrypt.
					decryptedData = decryptData(storedSecretKey, msgMetadata, payload);
					// If decryption succeeded, data is non null
					if (decryptedData != null)
					{
						break;
					}
				}
				else
				{
					// First time, entry won't be present in cache
					log.debug("{} Failed to decrypt data or data key is not in cache. Will attempt to refresh", logCtx);
				}

			}
			return decryptedData;

		}

		/*
		 * Decrypt the payload using the data key. Keys used to encrypt data key can be retrieved from msgMetadata
		 *
		 * @param msgMetadata Message Metadata
		 *
		 * @param payload Message which needs to be decrypted
		 *
		 * @param keyReader KeyReader implementation to retrieve key value
		 *
		 * @return decryptedData if success, null otherwise
		 */
		public virtual ByteBuf decrypt(MessageMetadata msgMetadata, ByteBuf payload, CryptoKeyReader keyReader)
		{

			// If dataKey is present, attempt to decrypt using the existing key
			if (dataKey != null)
			{
				ByteBuf decryptedData = getKeyAndDecryptData(msgMetadata, payload);
				// If decryption succeeded, data is non null
				if (decryptedData != null)
				{
					return decryptedData;
				}
			}

			// dataKey is null or decryption failed. Attempt to regenerate data key
			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeysList;
			EncryptionKeys encKeyInfo = encKeys.Where(kbv =>
			{
			sbyte[] encDataKey = kbv.Value.toByteArray();
			IList<KeyValue> encKeyMeta = kbv.MetadataList;
			return decryptDataKey(kbv.Key, encDataKey, encKeyMeta, keyReader);
			}).First().orElse(null);

			if (encKeyInfo == null || dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return getKeyAndDecryptData(msgMetadata, payload);

		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(MessageCrypto));

	}

}