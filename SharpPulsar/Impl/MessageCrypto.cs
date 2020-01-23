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
namespace SharpPulsar.Impl
{
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;

	using ByteBuf = io.netty.buffer.ByteBuf;



	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using EncryptionKeyInfo = SharpPulsar.Api.EncryptionKeyInfo;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using CryptoException = SharpPulsar.Api.PulsarClientException.CryptoException;
	using PulsarByteBufAllocator = Org.Apache.Pulsar.Common.Allocator.PulsarByteBufAllocator;
	using EncryptionKeys = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.EncryptionKeys;
	using KeyValue = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;
	using ByteString = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.ByteString;
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
		private const string RsaTrans = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string AESGCM = "AES/GCM/NoPadding";

		private static KeyGenerator keyGenerator;
		private const int TagLen = 16 * 8;
		public const int IvLen = 12;
		private sbyte[] iv = new sbyte[IvLen];
		private Cipher cipher;
		internal MessageDigest Digest;
		private string logCtx;

		// Data key which is used to encrypt message
		private SecretKey dataKey;
		private LoadingCache<ByteBuffer, SecretKey> dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private ConcurrentDictionary<string, EncryptionKeyInfo> encryptedDataKeyMap;

		internal static readonly SecureRandom SecureRandom;
		static MessageCrypto()
		{

			Security.addProvider(new BouncyCastleProvider());
			SecureRandom Rand = null;
			try
			{
				Rand = SecureRandom.getInstance("NativePRNGNonBlocking");
			}
			catch (NoSuchAlgorithmException)
			{
				Rand = new SecureRandom();
			}

			SecureRandom = Rand;

			// Initial seed
			SecureRandom.NextBytes(new sbyte[IvLen]);
		}

		public MessageCrypto(string LogCtx, bool KeyGenNeeded)
		{

			this.logCtx = LogCtx;
			encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			dataKeyCache = CacheBuilder.newBuilder().expireAfterAccess(4, BAMCIS.Util.Concurrent.TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				cipher = Cipher.getInstance(AESGCM, BouncyCastleProvider.PROVIDER_NAME);
				// If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!KeyGenNeeded)
				{

					Digest = MessageDigest.getInstance("MD5");

					dataKey = null;
					return;
				}
				keyGenerator = KeyGenerator.getInstance("AES");
				int AesKeyLength = Cipher.getMaxAllowedKeyLength("AES");
				if (AesKeyLength <= 128)
				{
					log.warn("{} AES Cryptographic strength is limited to {} bits. Consider installing JCE Unlimited Strength Jurisdiction Policy Files.", LogCtx, AesKeyLength);
					keyGenerator.init(AesKeyLength, SecureRandom);
				}
				else
				{
					keyGenerator.init(256, SecureRandom);
				}

			}
			catch (Exception e) when (e is NoSuchAlgorithmException || e is NoSuchProviderException || e is NoSuchPaddingException)
			{

				cipher = null;
				log.error("{} MessageCrypto initialization Failed {}", LogCtx, e.Message);

			}

			// Generate data key to encrypt messages
			dataKey = keyGenerator.generateKey();

			iv = new sbyte[IvLen];
		}

		public class CacheLoaderAnonymousInnerClass : CacheLoader<ByteBuffer, SecretKey>
		{
			private readonly MessageCrypto outerInstance;

			public CacheLoaderAnonymousInnerClass(MessageCrypto OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}


			public override SecretKey load(ByteBuffer Key)
			{
				return null;
			}

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.PublicKey loadPublicKey(byte[] keyBytes) throws Exception
		private PublicKey LoadPublicKey(sbyte[] KeyBytes)
		{

			Reader KeyReader = new StringReader(StringHelper.NewString(KeyBytes));
			PublicKey PublicKey = null;
			try
			{
					using (PEMParser PemReader = new PEMParser(KeyReader))
					{
					object PemObj = PemReader.readObject();
					JcaPEMKeyConverter PemConverter = new JcaPEMKeyConverter();
					SubjectPublicKeyInfo KeyInfo = null;
					X9ECParameters EcParam = null;
        
					if (PemObj is ASN1ObjectIdentifier)
					{
        
						// make sure this is EC Parameter we're handling. In which case
						// we'll store it and read the next object which should be our
						// EC Public Key
        
						ASN1ObjectIdentifier EcOID = (ASN1ObjectIdentifier) PemObj;
						EcParam = ECNamedCurveTable.getByOID(EcOID);
						if (EcParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + ((ASN1ObjectIdentifier) PemObj).Id);
						}
        
						PemObj = PemReader.readObject();
					}
					else if (PemObj is X9ECParameters)
					{
						EcParam = (X9ECParameters) PemObj;
						PemObj = PemReader.readObject();
					}
        
					if (PemObj is org.bouncycastle.cert.X509CertificateHolder)
					{
						KeyInfo = ((org.bouncycastle.cert.X509CertificateHolder) PemObj).SubjectPublicKeyInfo;
					}
					else
					{
						KeyInfo = (SubjectPublicKeyInfo) PemObj;
					}
					PublicKey = PemConverter.getPublicKey(KeyInfo);
        
					if (EcParam != null && ECDSA.Equals(PublicKey.Algorithm))
					{
						ECParameterSpec EcSpec = new ECParameterSpec(EcParam.Curve, EcParam.G, EcParam.N, EcParam.H, EcParam.Seed);
						KeyFactory KeyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPublicKeySpec KeySpec = new ECPublicKeySpec(((BCECPublicKey) PublicKey).Q, EcSpec);
						PublicKey = (PublicKey) KeyFactory.generatePublic(KeySpec);
					}
					}
			}
			catch (Exception e) when (e is IOException || e is NoSuchAlgorithmException || e is NoSuchProviderException || e is InvalidKeySpecException)
			{
				throw new Exception(e);
			}
			return PublicKey;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.security.PrivateKey loadPrivateKey(byte[] keyBytes) throws Exception
		private PrivateKey LoadPrivateKey(sbyte[] KeyBytes)
		{

			Reader KeyReader = new StringReader(StringHelper.NewString(KeyBytes));
			PrivateKey PrivateKey = null;
			try
			{
					using (PEMParser PemReader = new PEMParser(KeyReader))
					{
					X9ECParameters EcParam = null;
        
					object PemObj = PemReader.readObject();
        
					if (PemObj is ASN1ObjectIdentifier)
					{
        
						// make sure this is EC Parameter we're handling. In which case
						// we'll store it and read the next object which should be our
						// EC Private Key
        
						ASN1ObjectIdentifier EcOID = (ASN1ObjectIdentifier) PemObj;
						EcParam = ECNamedCurveTable.getByOID(EcOID);
						if (EcParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + EcOID.Id);
						}
        
						PemObj = PemReader.readObject();
        
					}
					else if (PemObj is X9ECParameters)
					{
        
						EcParam = (X9ECParameters) PemObj;
						PemObj = PemReader.readObject();
					}
        
					if (PemObj is PEMKeyPair)
					{
        
						PrivateKeyInfo PKeyInfo = ((PEMKeyPair) PemObj).PrivateKeyInfo;
						JcaPEMKeyConverter PemConverter = new JcaPEMKeyConverter();
						PrivateKey = PemConverter.getPrivateKey(PKeyInfo);
        
					}
        
					// if our private key is EC type and we have parameters specified
					// then we need to set it accordingly
        
					if (EcParam != null && ECDSA.Equals(PrivateKey.Algorithm))
					{
						ECParameterSpec EcSpec = new ECParameterSpec(EcParam.Curve, EcParam.G, EcParam.N, EcParam.H, EcParam.Seed);
						KeyFactory KeyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPrivateKeySpec KeySpec = new ECPrivateKeySpec(((BCECPrivateKey) PrivateKey).S, EcSpec);
						PrivateKey = (PrivateKey) KeyFactory.generatePrivate(KeySpec);
					}
        
					}
			}
			catch (IOException E)
			{
				throw new Exception(E);
			}
			return PrivateKey;
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
//ORIGINAL LINE: public synchronized void addPublicKeyCipher(java.util.Set<String> keyNames, SharpPulsar.api.CryptoKeyReader keyReader) throws SharpPulsar.api.PulsarClientException.CryptoException
		public virtual void AddPublicKeyCipher(ISet<string> KeyNames, CryptoKeyReader KeyReader)
		{
			lock (this)
			{
        
				// Generate data key
				dataKey = keyGenerator.generateKey();
        
				foreach (string Key in KeyNames)
				{
					AddPublicKeyCipher(Key, KeyReader);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addPublicKeyCipher(String keyName, SharpPulsar.api.CryptoKeyReader keyReader) throws SharpPulsar.api.PulsarClientException.CryptoException
		private void AddPublicKeyCipher(string KeyName, CryptoKeyReader KeyReader)
		{

			if (string.ReferenceEquals(KeyName, null) || KeyReader == null)
			{
				throw new PulsarClientException.CryptoException("Keyname or KeyReader is null");
			}

			// Read the public key and its info using callback
			EncryptionKeyInfo KeyInfo = KeyReader.getPublicKey(KeyName, null);

			PublicKey PubKey;

			try
			{
				PubKey = LoadPublicKey(KeyInfo.Key);
			}
			catch (Exception E)
			{
				string Msg = logCtx + "Failed to load public key " + KeyName + ". " + E.Message;
				log.error(Msg);
				throw new PulsarClientException.CryptoException(Msg);
			}

			Cipher DataKeyCipher = null;
			sbyte[] EncryptedKey;

			try
			{

				// Encrypt data key using public key
				if (RSA.Equals(PubKey.Algorithm))
				{
					DataKeyCipher = Cipher.getInstance(RsaTrans, BouncyCastleProvider.PROVIDER_NAME);
				}
				else if (ECDSA.Equals(PubKey.Algorithm))
				{
					DataKeyCipher = Cipher.getInstance(ECIES, BouncyCastleProvider.PROVIDER_NAME);
				}
				else
				{
					string Msg = logCtx + "Unsupported key type " + PubKey.Algorithm + " for key " + KeyName;
					log.error(Msg);
					throw new PulsarClientException.CryptoException(Msg);
				}
				DataKeyCipher.init(Cipher.ENCRYPT_MODE, PubKey);
				EncryptedKey = DataKeyCipher.doFinal(dataKey.Encoded);

			}
			catch (Exception E) when (E is IllegalBlockSizeException || E is BadPaddingException || E is NoSuchAlgorithmException || E is NoSuchProviderException || E is NoSuchPaddingException || E is InvalidKeyException)
			{
				log.error("{} Failed to encrypt data key {}. {}", logCtx, KeyName, E.Message);
				throw new PulsarClientException.CryptoException(E.Message);
			}
			EncryptionKeyInfo Eki = new EncryptionKeyInfo(EncryptedKey, KeyInfo.Metadata);
			encryptedDataKeyMap[KeyName] = Eki;
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
		public virtual bool RemoveKeyCipher(string KeyName)
		{

			if (string.ReferenceEquals(KeyName, null))
			{
				return false;
			}
			encryptedDataKeyMap.Remove(KeyName);
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
//ORIGINAL LINE: public synchronized io.netty.buffer.ByteBuf encrypt(java.util.Set<String> encKeys, SharpPulsar.api.CryptoKeyReader keyReader, org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata.Builder msgMetadata, io.netty.buffer.ByteBuf payload) throws SharpPulsar.api.PulsarClientException
		public virtual ByteBuf Encrypt(ISet<string> EncKeys, CryptoKeyReader KeyReader, MessageMetadata.Builder MsgMetadata, ByteBuf Payload)
		{
			lock (this)
			{
        
				if (EncKeys.Count == 0)
				{
					return Payload;
				}
        
				// Update message metadata with encrypted data key
				foreach (string KeyName in EncKeys)
				{
					if (encryptedDataKeyMap[KeyName] == null)
					{
						// Attempt to load the key. This will allow us to load keys as soon as
						// a new key is added to producer config
						AddPublicKeyCipher(KeyName, KeyReader);
					}
					EncryptionKeyInfo KeyInfo = encryptedDataKeyMap[KeyName];
					if (KeyInfo != null)
					{
						if (KeyInfo.Metadata != null && KeyInfo.Metadata.Count > 0)
						{
							IList<KeyValue> KvList = new List<KeyValue>();
							KeyInfo.Metadata.forEach((key, value) =>
							{
							KvList.Add(KeyValue.newBuilder().setKey(key).setValue(value).build());
							});
							MsgMetadata.addEncryptionKeys(EncryptionKeys.newBuilder().setKey(KeyName).setValue(ByteString.copyFrom(KeyInfo.Key)).addAllMetadata(KvList).build());
						}
						else
						{
							MsgMetadata.addEncryptionKeys(EncryptionKeys.newBuilder().setKey(KeyName).setValue(ByteString.copyFrom(KeyInfo.Key)).build());
						}
					}
					else
					{
						// We should never reach here.
						log.error("{} Failed to find encrypted Data key for key {}.", logCtx, KeyName);
					}
        
				}
        
				// Create gcm param
				// TODO: Replace random with counter and periodic refreshing based on timer/counter value
				SecureRandom.NextBytes(iv);
				GCMParameterSpec GcmParam = new GCMParameterSpec(TagLen, iv);
        
				// Update message metadata with encryption param
				MsgMetadata.EncryptionParam = ByteString.copyFrom(iv);
        
				ByteBuf TargetBuf = null;
				try
				{
					// Encrypt the data
					cipher.init(Cipher.ENCRYPT_MODE, dataKey, GcmParam);
        
					ByteBuffer SourceNioBuf = Payload.nioBuffer(Payload.readerIndex(), Payload.readableBytes());
        
					int MaxLength = cipher.getOutputSize(Payload.readableBytes());
					TargetBuf = PulsarByteBufAllocator.DEFAULT.buffer(MaxLength, MaxLength);
					ByteBuffer TargetNioBuf = TargetBuf.nioBuffer(0, MaxLength);
        
					int BytesStored = cipher.doFinal(SourceNioBuf, TargetNioBuf);
					TargetBuf.writerIndex(BytesStored);
        
				}
				catch (Exception e) when (e is IllegalBlockSizeException || e is BadPaddingException || e is InvalidKeyException || e is InvalidAlgorithmParameterException || e is ShortBufferException)
				{
        
					TargetBuf.release();
					log.error("{} Failed to encrypt message. {}", logCtx, e);
					throw new PulsarClientException.CryptoException(e.Message);
        
				}
        
				Payload.release();
				return TargetBuf;
			}
		}

		private bool DecryptDataKey(string KeyName, sbyte[] EncryptedDataKey, IList<KeyValue> EncKeyMeta, CryptoKeyReader KeyReader)
		{

			IDictionary<string, string> KeyMeta = new Dictionary<string, string>();
			EncKeyMeta.ForEach(kv =>
			{
			KeyMeta[kv.Key] = kv.Value;
			});

			// Read the private key info using callback
			EncryptionKeyInfo KeyInfo = KeyReader.getPrivateKey(KeyName, KeyMeta);

			// Convert key from byte to PivateKey
			PrivateKey PrivateKey;
			try
			{
				PrivateKey = LoadPrivateKey(KeyInfo.Key);
				if (PrivateKey == null)
				{
					log.error("{} Failed to load private key {}.", logCtx, KeyName);
					return false;
				}
			}
			catch (Exception E)
			{
				log.error("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, KeyName, E.Message);
				return false;
			}

			// Decrypt data key to decrypt messages
			Cipher DataKeyCipher = null;
			sbyte[] DataKeyValue = null;
			sbyte[] KeyDigest = null;

			try
			{

				// Decrypt data key using private key
				if (RSA.Equals(PrivateKey.Algorithm))
				{
					DataKeyCipher = Cipher.getInstance(RsaTrans, BouncyCastleProvider.PROVIDER_NAME);
				}
				else if (ECDSA.Equals(PrivateKey.Algorithm))
				{
					DataKeyCipher = Cipher.getInstance(ECIES, BouncyCastleProvider.PROVIDER_NAME);
				}
				else
				{
					log.error("Unsupported key type {} for key {}.", PrivateKey.Algorithm, KeyName);
					return false;
				}
				DataKeyCipher.init(Cipher.DECRYPT_MODE, PrivateKey);
				DataKeyValue = DataKeyCipher.doFinal(EncryptedDataKey);

				KeyDigest = Digest.digest(EncryptedDataKey);

			}
			catch (Exception E) when (E is IllegalBlockSizeException || E is BadPaddingException || E is NoSuchAlgorithmException || E is NoSuchProviderException || E is NoSuchPaddingException || E is InvalidKeyException)
			{
				log.error("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, KeyName, E.Message);
				return false;
			}
			dataKey = new SecretKeySpec(DataKeyValue, "AES");
			dataKeyCache.put(ByteBuffer.wrap(KeyDigest), dataKey);
			return true;
		}

		private ByteBuf DecryptData(SecretKey DataKeySecret, MessageMetadata MsgMetadata, ByteBuf Payload)
		{

			// unpack iv and encrypted data
			ByteString IvString = MsgMetadata.EncryptionParam;
			IvString.copyTo(iv, 0);

			GCMParameterSpec GcmParams = new GCMParameterSpec(TagLen, iv);
			ByteBuf TargetBuf = null;
			try
			{
				cipher.init(Cipher.DECRYPT_MODE, DataKeySecret, GcmParams);

				ByteBuffer SourceNioBuf = Payload.nioBuffer(Payload.readerIndex(), Payload.readableBytes());

				int MaxLength = cipher.getOutputSize(Payload.readableBytes());
				TargetBuf = PulsarByteBufAllocator.DEFAULT.buffer(MaxLength, MaxLength);
				ByteBuffer TargetNioBuf = TargetBuf.nioBuffer(0, MaxLength);

				int DecryptedSize = cipher.doFinal(SourceNioBuf, TargetNioBuf);
				TargetBuf.writerIndex(DecryptedSize);

			}
			catch (Exception e) when (e is InvalidKeyException || e is InvalidAlgorithmParameterException || e is IllegalBlockSizeException || e is BadPaddingException || e is ShortBufferException)
			{
				log.error("{} Failed to decrypt message {}", logCtx, e.Message);
				if (TargetBuf != null)
				{
					TargetBuf.release();
					TargetBuf = null;
				}
			}

			return TargetBuf;
		}

		private ByteBuf GetKeyAndDecryptData(MessageMetadata MsgMetadata, ByteBuf Payload)
		{

			ByteBuf DecryptedData = null;

			IList<EncryptionKeys> EncKeys = MsgMetadata.EncryptionKeysList;

			// Go through all keys to retrieve data key from cache
			for (int I = 0; I < EncKeys.Count; I++)
			{

				sbyte[] MsgDataKey = EncKeys[I].Value.toByteArray();
				sbyte[] KeyDigest = Digest.digest(MsgDataKey);
				SecretKey StoredSecretKey = dataKeyCache.getIfPresent(ByteBuffer.wrap(KeyDigest));
				if (StoredSecretKey != null)
				{

					// Taking a small performance hit here if the hash collides. When it
					// retruns a different key, decryption fails. At this point, we would
					// call decryptDataKey to refresh the cache and come here again to decrypt.
					DecryptedData = DecryptData(StoredSecretKey, MsgMetadata, Payload);
					// If decryption succeeded, data is non null
					if (DecryptedData != null)
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
			return DecryptedData;

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
		public virtual ByteBuf Decrypt(MessageMetadata MsgMetadata, ByteBuf Payload, CryptoKeyReader KeyReader)
		{

			// If dataKey is present, attempt to decrypt using the existing key
			if (dataKey != null)
			{
				ByteBuf DecryptedData = GetKeyAndDecryptData(MsgMetadata, Payload);
				// If decryption succeeded, data is non null
				if (DecryptedData != null)
				{
					return DecryptedData;
				}
			}

			// dataKey is null or decryption failed. Attempt to regenerate data key
			IList<EncryptionKeys> EncKeys = MsgMetadata.EncryptionKeysList;
			EncryptionKeys EncKeyInfo = EncKeys.Where(kbv =>
			{
			sbyte[] EncDataKey = kbv.Value.toByteArray();
			IList<KeyValue> EncKeyMeta = kbv.MetadataList;
			return DecryptDataKey(kbv.Key, EncDataKey, EncKeyMeta, KeyReader);
			}).First().orElse(null);

			if (EncKeyInfo == null || dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return GetKeyAndDecryptData(MsgMetadata, Payload);

		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(MessageCrypto));

	}

}