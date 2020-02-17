
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Avro.Generic;
using BAMCIS.Util.Concurrent;
using crypto;
using DotNetty.Buffers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Security;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;

namespace SharpPulsar.Impl
{
	//https://pulsar.apache.org/docs/en/security-encryption/
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;

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

	//https://github.com/eaba/Bouncy-Castle-AES-GCM-Encryption/blob/master/EncryptionService.cs
	public class MessageCrypto
	{

		private const string ECDSA = "ECDSA";
		private const string RSA = "RSA";
		private const string ECIES = "ECIES";

		// Ideally the transformation should also be part of the message property. This will prevent client
		// from assuming hardcoded value. However, it will increase the size of the message even further.
		private const string RsaTrans = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string AESGCM = "AES/GCM/NoPadding";

		private static SymmetricAlgorithm keyGenerator;//https://www.c-sharpcorner.com/article/generating-symmetric-private-key-in-c-sharp-and-net/
		private const int TagLen = 16 * 8;
		public const int IvLen = 12;
		private sbyte[] iv = new sbyte[IvLen];
		private GcmBlockCipher cipher;
		internal MD5 Digest;
		private string logCtx;

		// Data key which is used to encrypt message
		private AesManaged dataKey;//https://stackoverflow.com/questions/39093489/c-sharp-equivalent-of-the-java-secretkeyspec-for-aes
		private LoadingCache<ByteBuffer, AesManaged> dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private ConcurrentDictionary<string, EncryptionKeyInfo> encryptedDataKeyMap;

		internal static readonly SecureRandom SecureRandom;
		static MessageCrypto()
		{
			SecureRandom rand = null;
			try
			{
				rand = SecureRandom.GetInstance("NativePRNGNonBlocking");
			}
			catch (Exception)
			{
				rand = new SecureRandom();
			}

			SecureRandom = rand;

			// Initial seed
			SecureRandom.NextBytes(new byte[IvLen]);
		}

		public MessageCrypto(string logCtx, bool keyGenNeeded)
		{

			this.logCtx = logCtx;
			encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			dataKeyCache = CacheBuilder.newBuilder().expireAfterAccess(4, TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				cipher = new GcmBlockCipher(new AesEngine());//https://stackoverflow.com/questions/34206699/bouncycastle-aes-gcm-nopadding-and-secretkeyspec-in-c-sharp
															 // If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					Digest = MD5.Create();//new MD5CryptoServiceProvider();

					dataKey = null;
					return;
				}
				keyGenerator = new AesCryptoServiceProvider();
				int aesKeyLength = keyGenerator.KeySize;
				if (aesKeyLength <= 128)
				{
					Log.LogWarning("{} AES Cryptographic strength is limited to {} bits. Consider installing JCE Unlimited Strength Jurisdiction Policy Files.", logCtx, aesKeyLength);
					keyGenerator.IV = SecureRandom.GetNextBytes(SecureRandom, aesKeyLength);
				}
				else
				{
					keyGenerator.IV = SecureRandom.GetNextBytes(SecureRandom, 256);
				}

			}
			catch (Exception e) 
			{

				cipher = null;
				Log.LogError("{} MessageCrypto initialization Failed {}", logCtx, e.Message);

			}

			// Generate data key to encrypt messages
            keyGenerator.GenerateIV();
			keyGenerator.GenerateKey();
            dataKey = new AesManaged {Key = keyGenerator.Key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7};

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

			Reader<> KeyReader = new StringReader(StringHelper.NewString(KeyBytes));
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

						ASN1ObjectIdentifier EcOID = (ASN1ObjectIdentifier)PemObj;
						EcParam = ECNamedCurveTable.getByOID(EcOID);
						if (EcParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + ((ASN1ObjectIdentifier)PemObj).Id);
						}

						PemObj = PemReader.readObject();
					}
					else if (PemObj is X9ECParameters)
					{
						EcParam = (X9ECParameters)PemObj;
						PemObj = PemReader.readObject();
					}

					if (PemObj is org.bouncycastle.cert.X509CertificateHolder)
					{
						KeyInfo = ((org.bouncycastle.cert.X509CertificateHolder)PemObj).SubjectPublicKeyInfo;
					}
					else
					{
						KeyInfo = (SubjectPublicKeyInfo)PemObj;
					}
					PublicKey = PemConverter.getPublicKey(KeyInfo);

					if (EcParam != null && ECDSA.Equals(PublicKey.Algorithm))
					{
						ECParameterSpec EcSpec = new ECParameterSpec(EcParam.Curve, EcParam.G, EcParam.N, EcParam.H, EcParam.Seed);
						KeyFactory KeyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPublicKeySpec KeySpec = new ECPublicKeySpec(((BCECPublicKey)PublicKey).Q, EcSpec);
						PublicKey = (PublicKey)KeyFactory.generatePublic(KeySpec);
					}
				}
			}
			catch (Exception e) when (e is IOException || e is NoSuchAlgorithmException || e is NoSuchProviderException || e is InvalidKeySpecException)
			{
				throw new Exception(e);
			}
			return PublicKey;
		}

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

						ASN1ObjectIdentifier EcOID = (ASN1ObjectIdentifier)PemObj;
						EcParam = ECNamedCurveTable.getByOID(EcOID);
						if (EcParam == null)
						{
							throw new PEMException("Unable to find EC Parameter for the given curve oid: " + EcOID.Id);
						}

						PemObj = PemReader.readObject();

					}
					else if (PemObj is X9ECParameters)
					{

						EcParam = (X9ECParameters)PemObj;
						PemObj = PemReader.readObject();
					}

					if (PemObj is PEMKeyPair)
					{

						PrivateKeyInfo PKeyInfo = ((PEMKeyPair)PemObj).PrivateKeyInfo;
						JcaPEMKeyConverter PemConverter = new JcaPEMKeyConverter();
						PrivateKey = PemConverter.getPrivateKey(PKeyInfo);

					}

					// if our private key is EC type and we have parameters specified
					// then we need to set it accordingly

					if (EcParam != null && ECDSA.Equals(PrivateKey.Algorithm))
					{
						ECParameterSpec EcSpec = new ECParameterSpec(EcParam.Curve, EcParam.G, EcParam.N, EcParam.H, EcParam.Seed);
						KeyFactory KeyFactory = KeyFactory.getInstance(ECDSA, BouncyCastleProvider.PROVIDER_NAME);
						ECPrivateKeySpec KeySpec = new ECPrivateKeySpec(((BCECPrivateKey)PrivateKey).S, EcSpec);
						PrivateKey = (PrivateKey)KeyFactory.generatePrivate(KeySpec);
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
		//ORIGINAL LINE: public synchronized void addPublicKeyCipher(java.util.Set<String> keyNames, org.apache.pulsar.client.api.CryptoKeyReader keyReader) throws org.apache.pulsar.client.api.PulsarClientException.CryptoException
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
		//ORIGINAL LINE: private void addPublicKeyCipher(String keyName, org.apache.pulsar.client.api.CryptoKeyReader keyReader) throws org.apache.pulsar.client.api.PulsarClientException.CryptoException
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
		//ORIGINAL LINE: public synchronized io.netty.buffer.ByteBuf encrypt(java.util.Set<String> encKeys, org.apache.pulsar.client.api.CryptoKeyReader keyReader, org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata.Builder msgMetadata, io.netty.buffer.ByteBuf payload) throws org.apache.pulsar.client.api.PulsarClientException
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

		private bool DecryptDataKey(string keyName, byte[] encryptedDataKey, IList<KeyValue> encKeyMeta, ICryptoKeyReader keyReader)
		{

			IDictionary<string, string> keyMeta = new Dictionary<string, string>();
			encKeyMeta.ToList().ForEach(kv =>
			{
				keyMeta[kv.Key] = kv.Value;
			});

			// Read the private key info using callback
			EncryptionKeyInfo keyInfo = keyReader.GetPrivateKey(keyName, keyMeta);

			// Convert key from byte to PivateKey
			PrivateKey privateKey;
			try
			{
				privateKey = LoadPrivateKey(keyInfo.Key);
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
					dataKeyCipher = Cipher.getInstance(RsaTrans, BouncyCastleProvider.PROVIDER_NAME);
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

				keyDigest = Digest.digest(encryptedDataKey);

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

		private IByteBuffer GetKeyAndDecryptData(MessageMetadata msgMetadata, IByteBuffer payload)
		{

            IByteBuffer decryptedData = null;

			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeys;

			// Go through all keys to retrieve data key from cache
			for (int i = 0; i < encKeys.Count; i++)
			{

				byte[] msgDataKey = encKeys[i].Value.ToByteArray();
				byte[] keyDigest = Digest.digest(msgDataKey);
				SecretKey storedSecretKey = dataKeyCache.getIfPresent(ByteBuffer.wrap(keyDigest));
				if (storedSecretKey != null)
				{

					// Taking a small performance hit here if the hash collides. When it
					// retruns a different key, decryption fails. At this point, we would
					// call decryptDataKey to refresh the cache and come here again to decrypt.
					decryptedData = DecryptData(storedSecretKey, msgMetadata, payload);
					// If decryption succeeded, data is non null
					if (decryptedData != null)
					{
						break;
					}
				}
				else
				{
					// First time, entry won't be present in cache
					Log.LogWarning("{} Failed to decrypt data or data key is not in cache. Will attempt to refresh", logCtx);
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
		public IByteBuffer Decrypt(MessageMetadata msgMetadata, IByteBuffer payload, ICryptoKeyReader keyReader)
		{

			// If dataKey is present, attempt to decrypt using the existing key
			if (dataKey != null)
			{
				var decryptedData = GetKeyAndDecryptData(msgMetadata, payload);
				// If decryption succeeded, data is non null
				if (decryptedData != null)
				{
					return decryptedData;
				}
			}

			// dataKey is null or decryption failed. Attempt to regenerate data key
			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeys;
			var encKeyInfo = encKeys.Where(kbv =>
			{
				var encDataKey = kbv.Value.ToByteArray();
				IList<KeyValue> encKeyMeta = kbv.Metadata;
				return DecryptDataKey(kbv.Key, encDataKey, encKeyMeta, keyReader);
			}).DefaultIfEmpty(null).First();

			if (encKeyInfo == null || dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return GetKeyAndDecryptData(msgMetadata, payload);

		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(MessageCrypto));

	}


}