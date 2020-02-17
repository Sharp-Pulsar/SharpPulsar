using DotNetty.Buffers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Api;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;
using SharpPulsar.Shared;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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

		private const string Ecdsa = "ECDSA";
		private const string Rsa = "RSA";
		private const string Ecies = "ECIES";

		// Ideally the transformation should also be part of the message property. This will prevent client
		// from assuming hardcoded value. However, it will increase the size of the message even further.
		private const string RsaTrans = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string Aesgcm = "AES/GCM/NoPadding";

		private static CipherKeyGenerator _keyGenerator;
		private const int TagLen = 16 * 8;
		public const int IvLen = 12;
		private sbyte[] _iv = new sbyte[IvLen];
		private GcmBlockCipher _cipher;
		internal MD5Digest Digest;
		private string _logCtx;

		// Data key which is used to encrypt message
		private byte[] _dataKey;
		//private LoadingCache<ByteBuffer, SecretKey> dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private ConcurrentDictionary<string, EncryptionKeyInfo> _encryptedDataKeyMap;

		internal static readonly SecureRandom SecureRandom;
		static MessageCrypto()
		{
			SecureRandom rand = null;
			try
			{
				rand = SecureRandom.GetInstance("NativePRNGNonBlocking");
			}
			catch (NoSuchAlgorithmException)
			{
				rand = new SecureRandom();
			}

			SecureRandom = rand;

			// Initial seed
			SecureRandom.NextBytes(new byte[IvLen]);
		}

		public MessageCrypto(string logCtx, bool keyGenNeeded)
		{

			_logCtx = logCtx;
			_encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			//dataKeyCache = CacheBuilder.newBuilder().expireAfterAccess(4, BAMCIS.Util.Concurrent.TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				_cipher = new GcmBlockCipher(new AesEngine());
				// If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					Digest = new MD5Digest();

					_dataKey = null;
					return;
				}
				_keyGenerator = new CipherKeyGenerator();//.GetInstance("AES");
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
				_keyGenerator.Init(new KeyGenerationParameters(SecureRandom, 256));

			}
			catch (System.Exception e) 
			{

				_cipher = null;
				Log.LogError("{} MessageCrypto initialization Failed {}", logCtx, e.Message);

			}

			// Generate data key to encrypt messages
			_dataKey = _keyGenerator.GenerateKey();

			_iv = new sbyte[IvLen];
		}

		
		private AsymmetricKeyParameter LoadPublicKey(sbyte[] keyBytes)
		{

			var keyReader = new StringReader(StringHelper.NewString(keyBytes));
			try
			{
				var pemReader = new PemReader(keyReader);
				var pemObj = (AsymmetricCipherKeyPair)pemReader.ReadObject();
				if (pemObj != null)
				{
					return pemObj.Public;
				}
				else
					throw new ArgumentException("Unknown key type");
			}
			catch (IOException e)
			{
				throw new System.Exception(e.Message, e);
			}
		}
		
		private AsymmetricKeyParameter LoadPrivateKey(sbyte[] keyBytes)
		{

			var keyReader = new StringReader(StringHelper.NewString(keyBytes));
			try
			{
				var pemReader = new PemReader(keyReader);
				var pemObj = (AsymmetricCipherKeyPair)pemReader.ReadObject();
				if(pemObj != null)
				{
					return pemObj.Private;
				}
				else
					throw new ArgumentException("Unknown key type");
			}
			catch (IOException e)
			{
				throw new System.Exception(e.Message, e);
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
		public virtual void AddPublicKeyCipher(ISet<string> keyNames, ICryptoKeyReader keyReader)
		{
			lock (this)
			{
        
				// Generate data key
				_dataKey = _keyGenerator.GenerateKey();
        
				foreach (var key in keyNames)
				{
					AddPublicKeyCipher(key, keyReader);
				}
			}
		}

		private void AddPublicKeyCipher(string keyName, ICryptoKeyReader keyReader)
		{

			if (string.ReferenceEquals(keyName, null) || keyReader == null)
			{
				throw new PulsarClientException.CryptoException("Keyname or KeyReader is null");
			}

			// Read the public key and its info using callback
			var keyInfo = keyReader.GetPublicKey(keyName, null);

			AsymmetricKeyParameter pubKey;

			try
			{
				pubKey = LoadPublicKey(keyInfo.Key);
			}
			catch (System.Exception e)
			{
				var msg = _logCtx + "Failed to load public key " + keyName + ". " + e.Message;
				Log.LogError(msg);
				throw new PulsarClientException.CryptoException(msg);
			}
			
			sbyte[] encryptedKey;

			try
			{
				// Encrypt data key using public key
				
				//cipher.i.init(Cipher.ENCRYPT_MODE, PubKey);
				encryptedKey = (sbyte[])(object)_cipher.DoFinal(_dataKey, _dataKey.Length);

			}
			catch (System.Exception e)
			{
				Log.LogError("{} Failed to encrypt data key {}. {}", _logCtx, keyName, e.Message);
				throw new PulsarClientException.CryptoException(e.Message);
			}
			var eki = new EncryptionKeyInfo(encryptedKey, keyInfo.Metadata);
			_encryptedDataKeyMap[keyName] = eki;
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
		public virtual bool RemoveKeyCipher(string keyName)
		{

			if (string.ReferenceEquals(keyName, null))
			{
				return false;
			}
			_encryptedDataKeyMap.Remove(keyName, out var d);
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
		public virtual IByteBuffer Encrypt(ISet<string> encKeys, ICryptoKeyReader keyReader, MessageMetadata.Builder msgMetadata, IByteBuffer payload)
		{
			lock (this)
			{
        
				if (encKeys.Count == 0)
				{
					return payload;
				}
        
				// Update message metadata with encrypted data key
				foreach (var keyName in encKeys)
				{
					if (_encryptedDataKeyMap[keyName] == null)
					{
						// Attempt to load the key. This will allow us to load keys as soon as
						// a new key is added to producer config
						AddPublicKeyCipher(keyName, keyReader);
					}
					var keyInfo = _encryptedDataKeyMap[keyName];
					if (keyInfo != null)
					{
						if (keyInfo.Metadata != null && keyInfo.Metadata.Count > 0)
						{
							IList<KeyValue> kvList = new List<KeyValue>();
							keyInfo.Metadata.ToList().ForEach(x =>
							{
								kvList.Add(KeyValue.NewBuilder().SetKey(x.Key).SetValue(x.Value).Build());
							});
							msgMetadata.AddEncryptionKeys(EncryptionKeys.NewBuilder().SetKey(keyName).SetValue(ByteString.CopyFrom((byte[])(object)keyInfo.Key)).AddAllMetadata(kvList).Build());
						}
						else
						{
							msgMetadata.AddEncryptionKeys(EncryptionKeys.NewBuilder().SetKey(keyName).SetValue(ByteString.CopyFrom((byte[])(object)keyInfo.Key)).Build());
						}
					}
					else
					{
						// We should never reach here.
						Log.LogError("{} Failed to find encrypted Data key for key {}.", _logCtx, keyName);
					}
        
				}

				// Create gcm param
				// TODO: Replace random with counter and periodic refreshing based on timer/counter value
				var iV = (byte[])(object)_iv;
				SecureRandom.NextBytes(iV, 0, iV.Length);
				var gcmParam = new AeadParameters(new KeyParameter(_dataKey), TagLen, iV);
        
				// Update message metadata with encryption param
				msgMetadata.SetEncryptionParam(ByteString.CopyFrom(iV));
        
				IByteBuffer targetBuf = null;
				try
				{
					// Encrypt the data
					_cipher.Init(true, gcmParam);
        
					var sourceNioBuf = payload.GetIoBuffer(payload.ReaderIndex, payload.ReadableBytes);
        
					var maxLength = _cipher.GetOutputSize(payload.ReadableBytes);
					targetBuf = PooledByteBufferAllocator.Default.Buffer(maxLength, maxLength);
					var targetNioBuf = targetBuf.GetIoBuffer(0, maxLength);
        
					var bytesStored = _cipher.DoFinal(sourceNioBuf.ToArray(), targetNioBuf.ToArray().Length);
					targetBuf.SetWriterIndex(bytesStored);
        
				}
				catch (System.Exception e) 
				{
                    targetBuf?.Release();
                    Log.LogError("{} Failed to encrypt message. {}", _logCtx, e);
					throw new PulsarClientException.CryptoException(e.Message);
        
				}
        
				payload.Release();
				return targetBuf;
			}
		}

		private bool DecryptDataKey(string keyName, sbyte[] encryptedDataKey, IList<KeyValue> encKeyMeta, ICryptoKeyReader keyReader)
		{

			IDictionary<string, string> keyMeta = new Dictionary<string, string>();
			encKeyMeta.ToList().ForEach(kv =>
			{
				keyMeta[kv.Key] = kv.Value;
			});

			// Read the private key info using callback
			var keyInfo = keyReader.GetPrivateKey(keyName, keyMeta);

			// Convert key from byte to PivateKey
			AsymmetricAlgorithm privateKey;
			try
			{
				privateKey = LoadPrivateKey(keyInfo.Key);
				if (privateKey == null)
				{
					Log.LogError("{} Failed to load private key {}.", _logCtx, keyName);
					return false;
				}
			}
			catch (System.Exception e)
			{
				Log.LogError("{} Failed to decrypt data key {} to decrypt messages {}", _logCtx, keyName, e.Message);
				return false;
			}

			// Decrypt data key to decrypt messages
            IAsymmetricBlockCipher dataKeyCipher = null;
			sbyte[] dataKeyValue = null;
			sbyte[] keyDigest = null;

			try
			{

				// Decrypt data key using private key
				if (Rsa.Equals(privateKey.KeyExchangeAlgorithm))
                {
					//dataKeyCipher = Cipher.getInstance(RsaTrans, BouncyCastleProvider.PROVIDER_NAME);
                    dataKeyCipher = new RsaEngine();
                }
				else if (Ecdsa.Equals(privateKey.KeyExchangeAlgorithm))
				{
					dataKeyCipher = ecdaengine();
				}
				else
				{
					Log.LogError("Unsupported key type {} for key {}.", privateKey.KeyExchangeAlgorithm, keyName);
					return false;
				}
				dataKeyCipher.Init(Cipher.DECRYPT_MODE, privateKey);
				dataKeyValue = dataKeyCipher.DoFinal(encryptedDataKey);

				keyDigest = Digest.Digest(encryptedDataKey);

			}
			catch (System.Exception e) 
			{
				Log.error("{} Failed to decrypt data key {} to decrypt messages {}", _logCtx, keyName, e.Message);
				return false;
			}
			_dataKey = new SecretKeySpec(dataKeyValue, "AES");
			dataKeyCache.put(ByteBuffer.Wrap(keyDigest), _dataKey);
			return true;
		}

		private IByteBuffer DecryptData(SecretKey dataKeySecret, MessageMetadata msgMetadata, IByteBuffer payload)
		{

			// unpack iv and encrypted data
			var ivString = msgMetadata.EncryptionParam;
			ivString.copyTo(_iv, 0);

			GCMParameterSpec gcmParams = new GCMParameterSpec(TagLen, _iv);
			IByteBuffer targetBuf = null;
			try
			{
				_cipher.init(Cipher.DECRYPT_MODE, dataKeySecret, gcmParams);

				var sourceNioBuf = payload.GetIoBuffer(payload.ReaderIndex, payload.ReadableBytes);

				int maxLength = _cipher.getOutputSize(payload.ReadableBytes);
				targetBuf = PooledByteBufferAllocator.Default.Buffer(maxLength, maxLength);
				var targetNioBuf = targetBuf.GetIoBuffer(0, maxLength);

				int decryptedSize = _cipher.doFinal(sourceNioBuf, targetNioBuf);
				targetBuf.SetWriterIndex(decryptedSize);

			}
			catch (Exception e) when (e is InvalidKeyException || e is InvalidAlgorithmParameterException || e is IllegalBlockSizeException || e is BadPaddingException || e is ShortBufferException)
			{
				Log.error("{} Failed to decrypt message {}", _logCtx, e.Message);
				if (targetBuf != null)
				{
					targetBuf.release();
					targetBuf = null;
				}
			}

			return targetBuf;
		}

		private IByteBuffer GetKeyAndDecryptData(MessageMetadata msgMetadata, IByteBuffer payload)
		{

			IByteBuffer decryptedData = null;

			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeys;

			// Go through all keys to retrieve data key from cache
			for (var i = 0; i < encKeys.Count; i++)
			{

				var msgDataKey = (sbyte[])(object)encKeys[i].Value.ToByteArray();
				sbyte[] keyDigest = Digest.Update(msgDataKey);
				SecretKey storedSecretKey = dataKeyCache.getIfPresent(ByteBuffer.Wrap(keyDigest));
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
					Log.LogDebug("{} Failed to decrypt data or data key is not in cache. Will attempt to refresh", _logCtx);
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
		public virtual IByteBuffer Decrypt(MessageMetadata msgMetadata, IByteBuffer payload, ICryptoKeyReader keyReader)
		{

			// If dataKey is present, attempt to decrypt using the existing key
			if (_dataKey != null)
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
				var encDataKey = (sbyte[])(object)kbv.Value.ToByteArray();
				IList<KeyValue> encKeyMeta = kbv.Metadata;
				return DecryptDataKey(kbv.Key, encDataKey, encKeyMeta, keyReader);
			}).FirstOrDefault();

			if (encKeyInfo == null || _dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return GetKeyAndDecryptData(msgMetadata, payload);

		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(MessageCrypto));

	}

}