using DotNetty.Buffers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpPulsar.Common;
using SharpPulsar.Exception;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Common.Entity;
using SharpPulsar.Api;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using SharpPulsar.Shared;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.X509.Store;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Digests;

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
	/*using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;

	using ByteBuf = io.netty.buffer.ByteBuf;



	using CryptoKeyReader = SharpPulsar.Api.CryptoKeyReader;
	using EncryptionKeyInfo = SharpPulsar.Api.EncryptionKeyInfo;
	using ASN1ObjectIdentifier = org.bouncycastle.asn1.ASN1ObjectIdentifier;
	using PrivateKeyInfo = org.bouncycastle.asn1.pkcs.PrivateKeyInfo;
	using SubjectPublicKeyInfo = org.bouncycastle.asn1.x509.SubjectPublicKeyInfo;
	using ECNamedCurveTable = org.bouncycastle.asn1.x9.ECNamedCurveTable;
	using X9ECParameters = org.bouncycastle.asn1.x9.X9ECParameters;
	using BCECPrivateKey = org.bouncycastle.jcajce.provider.asymmetric.ec.BCECPrivateKey;
	using BCECPublicKey = org.bouncycastle.jcajce.provider.asymmetric.ec.BCECPublicKey;
	//using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;
	using ECParameterSpec = org.bouncycastle.jce.spec.ECParameterSpec;
	using ECPrivateKeySpec = org.bouncycastle.jce.spec.ECPrivateKeySpec;
	using ECPublicKeySpec = org.bouncycastle.jce.spec.ECPublicKeySpec;
	using PEMException = org.bouncycastle.openssl.PEMException;
	using PEMKeyPair = org.bouncycastle.openssl.PEMKeyPair;
	using PEMParser = org.bouncycastle.openssl.PEMParser;
	using JcaPEMKeyConverter = org.bouncycastle.openssl.jcajce.JcaPEMKeyConverter;*/

	public class MessageCrypto
	{

		private const string ECDSA = "ECDSA";
		private const string RSA = "RSA";
		private const string ECIES = "ECIES";

		// Ideally the transformation should also be part of the message property. This will prevent client
		// from assuming hardcoded value. However, it will increase the size of the message even further.
		private const string RsaTrans = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string AESGCM = "AES/GCM/NoPadding";

		private static CipherKeyGenerator keyGenerator;
		private const int TagLen = 16 * 8;
		public const int IvLen = 12;
		private sbyte[] iv = new sbyte[IvLen];
		private GcmBlockCipher cipher;
		internal MD5Digest Digest;
		private string logCtx;

		// Data key which is used to encrypt message
		private byte[] dataKey;
		//private LoadingCache<ByteBuffer, SecretKey> dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private ConcurrentDictionary<string, EncryptionKeyInfo> encryptedDataKeyMap;

		internal static readonly SecureRandom SecureRandom;
		static MessageCrypto()
		{
			SecureRandom Rand = null;
			try
			{
				Rand = SecureRandom.GetInstance("NativePRNGNonBlocking");
			}
			catch (NoSuchAlgorithmException)
			{
				Rand = new SecureRandom();
			}

			SecureRandom = Rand;

			// Initial seed
			SecureRandom.NextBytes(new byte[IvLen]);
		}

		public MessageCrypto(string LogCtx, bool KeyGenNeeded)
		{

			logCtx = LogCtx;
			encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			//dataKeyCache = CacheBuilder.newBuilder().expireAfterAccess(4, BAMCIS.Util.Concurrent.TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				cipher = new GcmBlockCipher(new AesEngine());
				// If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!KeyGenNeeded)
				{

					Digest = new MD5Digest();

					dataKey = null;
					return;
				}
				keyGenerator = new CipherKeyGenerator();//.GetInstance("AES");
				/*int AesKeyLength = Cipher.GetMaxAllowedKeyLength("AES");
				if (AesKeyLength <= 128)
				{
					log.LogWarning("{} AES Cryptographic strength is limited to {} bits. Consider installing JCE Unlimited Strength Jurisdiction Policy Files.", LogCtx, AesKeyLength);
					keyGenerator.Init(new KeyGenerationParameters(SecureRandom, AesKeyLength));
				}
				else
				{
					keyGenerator.Init(new KeyGenerationParameters(SecureRandom, 256));
				}*/
				keyGenerator.Init(new KeyGenerationParameters(SecureRandom, 256));

			}
			catch (System.Exception e) 
			{

				cipher = null;
				log.LogError("{} MessageCrypto initialization Failed {}", LogCtx, e.Message);

			}

			// Generate data key to encrypt messages
			dataKey = keyGenerator.GenerateKey();

			iv = new sbyte[IvLen];
		}

		
		private AsymmetricKeyParameter LoadPublicKey(sbyte[] KeyBytes)
		{

			var KeyReader = new StringReader(StringHelper.NewString(KeyBytes));
			try
			{
				var PemReader = new PemReader(KeyReader);
				var PemObj = (AsymmetricCipherKeyPair)PemReader.ReadObject();
				if (PemObj != null)
				{
					return PemObj.Public;
				}
				else
					throw new ArgumentException("Unknown key type");
			}
			catch (IOException E)
			{
				throw new System.Exception(E.Message, E);
			}
		}
		
		private AsymmetricKeyParameter LoadPrivateKey(sbyte[] KeyBytes)
		{

			var KeyReader = new StringReader(StringHelper.NewString(KeyBytes));
			try
			{
				var PemReader = new PemReader(KeyReader);
				var PemObj = (AsymmetricCipherKeyPair)PemReader.ReadObject();
				if(PemObj != null)
				{
					return PemObj.Private;
				}
				else
					throw new ArgumentException("Unknown key type");
			}
			catch (IOException E)
			{
				throw new System.Exception(E.Message, E);
			}
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
		public virtual void AddPublicKeyCipher(ISet<string> KeyNames, CryptoKeyReader KeyReader)
		{
			lock (this)
			{
        
				// Generate data key
				dataKey = keyGenerator.GenerateKey();
        
				foreach (string Key in KeyNames)
				{
					AddPublicKeyCipher(Key, KeyReader);
				}
			}
		}

		private void AddPublicKeyCipher(string KeyName, CryptoKeyReader KeyReader)
		{

			if (string.ReferenceEquals(KeyName, null) || KeyReader == null)
			{
				throw new PulsarClientException.CryptoException("Keyname or KeyReader is null");
			}

			// Read the public key and its info using callback
			EncryptionKeyInfo KeyInfo = KeyReader.GetPublicKey(KeyName, null);

			AsymmetricKeyParameter PubKey;

			try
			{
				PubKey = LoadPublicKey(KeyInfo.Key);
			}
			catch (System.Exception E)
			{
				string Msg = logCtx + "Failed to load public key " + KeyName + ". " + E.Message;
				log.LogError(Msg);
				throw new PulsarClientException.CryptoException(Msg);
			}
			
			sbyte[] EncryptedKey;

			try
			{
				// Encrypt data key using public key
				
				//cipher.i.init(Cipher.ENCRYPT_MODE, PubKey);
				EncryptedKey = (sbyte[])(object)cipher.DoFinal(dataKey, dataKey.Length);

			}
			catch (System.Exception E)
			{
				log.LogError("{} Failed to encrypt data key {}. {}", logCtx, KeyName, E.Message);
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
			encryptedDataKeyMap.Remove(KeyName, out var d);
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
		public virtual IByteBuffer Encrypt(ISet<string> EncKeys, CryptoKeyReader KeyReader, MessageMetadata.Builder MsgMetadata, IByteBuffer Payload)
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
							KeyInfo.Metadata.ToList().ForEach(x =>
							{
								KvList.Add(KeyValue.NewBuilder().SetKey(x.Key).SetValue(x.Value).Build());
							});
							MsgMetadata.AddEncryptionKeys(EncryptionKeys.NewBuilder().SetKey(KeyName).SetValue(ByteString.CopyFrom((byte[])(object)KeyInfo.Key)).AddAllMetadata(KvList).Build());
						}
						else
						{
							MsgMetadata.AddEncryptionKeys(EncryptionKeys.NewBuilder().SetKey(KeyName).SetValue(ByteString.CopyFrom((byte[])(object)KeyInfo.Key)).Build());
						}
					}
					else
					{
						// We should never reach here.
						log.LogError("{} Failed to find encrypted Data key for key {}.", logCtx, KeyName);
					}
        
				}

				// Create gcm param
				// TODO: Replace random with counter and periodic refreshing based on timer/counter value
				var iV = (byte[])(object)iv;
				SecureRandom.NextBytes(iV, 0, iV.Length);
				var gcmParam = new AeadParameters(new KeyParameter(dataKey), TagLen, iV);
        
				// Update message metadata with encryption param
				MsgMetadata.SetEncryptionParam(ByteString.CopyFrom(iV));
        
				IByteBuffer TargetBuf = null;
				try
				{
					// Encrypt the data
					cipher.Init(true, gcmParam);
        
					var SourceNioBuf = Payload.GetIoBuffer(Payload.ReaderIndex, Payload.ReadableBytes);
        
					int MaxLength = cipher.GetOutputSize(Payload.ReadableBytes);
					TargetBuf = PulsarByteBufAllocator.DEFAULT.Buffer(MaxLength, MaxLength);
					var TargetNioBuf = TargetBuf.GetIoBuffer(0, MaxLength);
        
					int BytesStored = cipher.DoFinal(SourceNioBuf.ToArray(), TargetNioBuf.ToArray().Length);
					TargetBuf.SetWriterIndex(BytesStored);
        
				}
				catch (System.Exception e) 
				{
        
					TargetBuf.Release();
					log.LogError("{} Failed to encrypt message. {}", logCtx, e);
					throw new PulsarClientException.CryptoException(e.Message);
        
				}
        
				Payload.Release();
				return TargetBuf;
			}
		}

		private bool DecryptDataKey(string KeyName, sbyte[] EncryptedDataKey, IList<KeyValue> EncKeyMeta, CryptoKeyReader KeyReader)
		{

			IDictionary<string, string> KeyMeta = new Dictionary<string, string>();
			EncKeyMeta.ToList().ForEach(kv =>
			{
				KeyMeta[kv.Key] = kv.Value;
			});

			// Read the private key info using callback
			EncryptionKeyInfo KeyInfo = KeyReader.GetPrivateKey(KeyName, KeyMeta);

			// Convert key from byte to PivateKey
			AsymmetricKeyParameter PrivateKey;
			try
			{
				PrivateKey = LoadPrivateKey(KeyInfo.Key);
				if (PrivateKey == null)
				{
					log.LogError("{} Failed to load private key {}.", logCtx, KeyName);
					return false;
				}
			}
			catch (System.Exception E)
			{
				log.LogErro("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, KeyName, E.Message);
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
			catch (Exception E) 
			{
				log.error("{} Failed to decrypt data key {} to decrypt messages {}", logCtx, KeyName, E.Message);
				return false;
			}
			dataKey = new SecretKeySpec(DataKeyValue, "AES");
			dataKeyCache.put(ByteBuffer.Wrap(KeyDigest), dataKey);
			return true;
		}

		private IByteBuffer DecryptData(SecretKey DataKeySecret, MessageMetadata MsgMetadata, IByteBuffer Payload)
		{

			// unpack iv and encrypted data
			ByteString IvString = MsgMetadata.EncryptionParam;
			IvString.copyTo(iv, 0);

			GCMParameterSpec GcmParams = new GCMParameterSpec(TagLen, iv);
			IByteBuffer TargetBuf = null;
			try
			{
				cipher.init(Cipher.DECRYPT_MODE, DataKeySecret, GcmParams);

				var SourceNioBuf = Payload.GetIoBuffer(Payload.ReaderIndex, Payload.ReadableBytes);

				int MaxLength = cipher.getOutputSize(Payload.ReadableBytes);
				TargetBuf = PulsarByteBufAllocator.DEFAULT.Buffer(MaxLength, MaxLength);
				var TargetNioBuf = TargetBuf.GetIoBuffer(0, MaxLength);

				int DecryptedSize = cipher.doFinal(SourceNioBuf, TargetNioBuf);
				TargetBuf.SetWriterIndex(DecryptedSize);

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

		private IByteBuffer GetKeyAndDecryptData(MessageMetadata MsgMetadata, IByteBuffer Payload)
		{

			IByteBuffer DecryptedData = null;

			IList<EncryptionKeys> EncKeys = MsgMetadata.EncryptionKeys;

			// Go through all keys to retrieve data key from cache
			for (int I = 0; I < EncKeys.Count; I++)
			{

				sbyte[] MsgDataKey = (sbyte[])(object)EncKeys[I].Value.ToByteArray();
				sbyte[] KeyDigest = Digest.Update(MsgDataKey);
				SecretKey StoredSecretKey = dataKeyCache.getIfPresent(ByteBuffer.Wrap(KeyDigest));
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
					log.LogDebug("{} Failed to decrypt data or data key is not in cache. Will attempt to refresh", logCtx);
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
		public virtual IByteBuffer Decrypt(MessageMetadata MsgMetadata, IByteBuffer Payload, CryptoKeyReader KeyReader)
		{

			// If dataKey is present, attempt to decrypt using the existing key
			if (dataKey != null)
			{
				IByteBuffer DecryptedData = GetKeyAndDecryptData(MsgMetadata, Payload);
				// If decryption succeeded, data is non null
				if (DecryptedData != null)
				{
					return DecryptedData;
				}
			}

			// dataKey is null or decryption failed. Attempt to regenerate data key
			IList<EncryptionKeys> EncKeys = MsgMetadata.EncryptionKeys;
			EncryptionKeys EncKeyInfo = EncKeys.Where(kbv =>
			{
				sbyte[] EncDataKey = (sbyte[])(object)kbv.Value.ToByteArray();
				IList<KeyValue> EncKeyMeta = kbv.Metadata;
				return DecryptDataKey(kbv.Key, EncDataKey, EncKeyMeta, KeyReader);
			}).FirstOrDefault();

			if (EncKeyInfo == null || dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return GetKeyAndDecryptData(MsgMetadata, Payload);

		}

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(MessageCrypto));

	}

}