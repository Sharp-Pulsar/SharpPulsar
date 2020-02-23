
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
using DotNetty.Buffers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Utility;

namespace SharpPulsar.Impl
{
	//https://pulsar.apache.org/docs/en/security-encryption/
    //https://github.com/eaba/Bouncy-Castle-AES-GCM-Encryption/blob/master/EncryptionService.cs
	public class MessageCrypto
	{

		private const string Ecdsa = "ECDSA";
		private const string Rsa = "RSA";
		private const string Ecies = "ECIES";

		// Ideally the transformation should also be part of the message property. This will prevent client
		// from assuming hardcoded value. However, it will increase the size of the message even further.
		private const string RsaTrans = "RSA/NONE/OAEPWithSHA1AndMGF1Padding";
		private const string Aesgcm = "AES/GCM/NoPadding";

		private static SymmetricAlgorithm _keyGenerator;//https://www.c-sharpcorner.com/article/generating-symmetric-private-key-in-c-sharp-and-net/
		private const int TagLen = 16 * 8;
		public const int IvLen = 128;
		private byte[] _iv = new byte[IvLen];
		private GcmBlockCipher _cipher;
		internal MD5 Digest;
		private string _logCtx;

		// Data key which is used to encrypt message
		private AesManaged _dataKey;//https://stackoverflow.com/questions/39093489/c-sharp-equivalent-of-the-java-secretkeyspec-for-aes
		private ConcurrentDictionary<IByteBuffer, AesManaged> _dataKeyCache;

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
			catch (Exception)
			{
				rand = new SecureRandom();
			}

			SecureRandom = rand;

			// Initial seed
			SecureRandom.NextBytes(new byte[IvLen/8]);
		}

		public MessageCrypto(string logCtx, bool keyGenNeeded)
		{

			_logCtx = logCtx;
			_encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			_dataKeyCache = new ConcurrentDictionary<IByteBuffer, AesManaged>(); //CacheBuilder.newBuilder().expireAfterAccess(4, TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{

				_cipher = new GcmBlockCipher(new AesEngine());//https://stackoverflow.com/questions/34206699/bouncycastle-aes-gcm-nopadding-and-secretkeyspec-in-c-sharp
															 // If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					Digest = MD5.Create();//new MD5CryptoServiceProvider();

					_dataKey = null;
					return;
				}
				_keyGenerator = new AesCryptoServiceProvider();
				var aesKeyLength = _keyGenerator.KeySize;
				if (aesKeyLength <= 128)
				{
					Log.LogWarning("{} AES Cryptographic strength is limited to {} bits. Consider installing JCE Unlimited Strength Jurisdiction Policy Files.", logCtx, aesKeyLength);
					_keyGenerator.IV = SecureRandom.GetNextBytes(SecureRandom, aesKeyLength);
				}
				else
				{
					_keyGenerator.IV = SecureRandom.GetNextBytes(SecureRandom, 256);
				}

			}
			catch (Exception e) 
			{

				_cipher = null;
				Log.LogError("{} MessageCrypto initialization Failed {}", logCtx, e.Message);

			}

			// Generate data key to encrypt messages
			_keyGenerator.GenerateKey();
            _dataKey = new AesManaged {Key = _keyGenerator.Key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7};

            _iv = new byte[IvLen];
		}

        private RSACryptoServiceProvider LoadPublicKey(sbyte[] keyBytes)
		{
			var keyReader = new StringReader(StringHelper.NewString(keyBytes));
            var pr = new PemReader(keyReader);
			var publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            var csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
		}

		private RSACryptoServiceProvider LoadPrivateKey(sbyte[] keyBytes)
		{
            var keyReader = new StringReader(StringHelper.NewString(keyBytes));
            var pr = new PemReader(keyReader);
            var keyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

            var csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
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
                _keyGenerator.GenerateKey();
                _dataKey = new AesManaged { Key = _keyGenerator.Key, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

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

            RSACryptoServiceProvider pubKey;

			try
			{
				pubKey = LoadPublicKey(keyInfo.Key);
			}
			catch (Exception e)
			{
				var msg = _logCtx + "Failed to load public key " + keyName + ". " + e.Message;
				Log.LogError(msg);
				throw new PulsarClientException.CryptoException(msg);
			}

            byte[] encryptedKey;

			try
			{
                // Encrypt data key using public key
                GcmBlockCipher dataKeyCipher;
                if (pubKey.KeyExchangeAlgorithm != null && pubKey.KeyExchangeAlgorithm.StartsWith(Rsa, StringComparison.OrdinalIgnoreCase))
				{
					dataKeyCipher = new GcmBlockCipher(new AesEngine()); 
				}
                else
				{
					var msg = _logCtx + "Unsupported key type " + pubKey.KeyExchangeAlgorithm + " for key " + keyName;
					Log.LogError(msg);
					throw new PulsarClientException.CryptoException(msg);
				}
				SecureRandom.NextBytes(_iv);
				var key = CryptoHelper.ExportPublicKey(pubKey);
                var keyParameter = new AeadParameters(new KeyParameter(key), TagLen, _iv);
				//var keyParameter = ParameterUtilities.CreateKeyParameter("AES", key);
                //ICipherParameters cipherParameters = new ParametersWithIV(keyParameter, _iv);
				dataKeyCipher.Init(true, keyParameter);

                encryptedKey = new byte[dataKeyCipher.GetOutputSize(_dataKey.Key.Length)];
                var len = dataKeyCipher.ProcessBytes(_dataKey.Key, 0, _dataKey.Key.Length, encryptedKey, 0);

				dataKeyCipher.DoFinal(encryptedKey, len);

			}
			catch (Exception e) 
			{
				Log.LogError("{} Failed to encrypt data key {}. {}", _logCtx, keyName, e.Message);
				throw new PulsarClientException.CryptoException(e.Message);
			}
			var eki = new EncryptionKeyInfo((sbyte[])(object)encryptedKey, keyInfo.Metadata);
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
			lock (this)
            {
                _encryptedDataKeyMap.Remove(keyName, out var k);
            }
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
							keyInfo.Metadata.ToList().ForEach(m =>
							{
								kvList.Add(KeyValue.NewBuilder().SetKey(m.Key).SetValue(m.Value).Build());
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
				SecureRandom.NextBytes(_iv);
				var keyParameter = new AeadParameters(new KeyParameter(_dataKey.Key), TagLen, _iv);
				
				// Update message metadata with encryption param
				msgMetadata.SetEncryptionParam(ByteString.CopyFrom(_iv));

				IByteBuffer targetBuf = null;
				try
				{
					// Encrypt the data
					_cipher.Init(true, keyParameter);

					var sourceNioBuf = payload.GetIoBuffer(payload.ReaderIndex, payload.ReadableBytes);

					var maxLength = _cipher.GetOutputSize(payload.ReadableBytes);
					targetBuf = PooledByteBufferAllocator.Default.Buffer(maxLength, maxLength);
					var targetNioBuf = new byte[maxLength];

                    var len = _cipher.ProcessBytes(sourceNioBuf.ToArray(), 0, sourceNioBuf.ToArray().Length, targetNioBuf, 0);
					var bytesStored = _cipher.DoFinal(targetNioBuf, len);
                    
                    targetBuf.WriteBytes(targetNioBuf);
					targetBuf.SetWriterIndex(bytesStored);

				}
				catch (Exception e) 
				{

					targetBuf?.Release();
					Log.LogError("{} Failed to encrypt message. {}", _logCtx, e);
					throw new PulsarClientException.CryptoException(e.Message);

				}

				payload.Release();
				return targetBuf;
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
			var keyInfo = keyReader.GetPrivateKey(keyName, keyMeta);

			// Convert key from byte to PivateKey
            RSACryptoServiceProvider  privateKey;
			try
			{
				privateKey = LoadPrivateKey(keyInfo.Key);
				if (privateKey == null)
				{
					Log.LogError("{} Failed to load private key {}.", _logCtx, keyName);
					return false;
				}
			}
			catch (Exception e)
			{
                Log.LogError("{} Failed to decrypt data key {} to decrypt messages {}", _logCtx, keyName, e.Message);
				return false;
			}

			// Decrypt data key to decrypt messages
            byte[] dataKeyValue;
			byte[] keyDigest;
            using var cipherStream = new MemoryStream(encryptedDataKey);
            using var cipherReader = new BinaryReader(cipherStream);
            try
            {

                // Decrypt data key using private key
                GcmBlockCipher dataKeyCipher;
                if (privateKey.KeyExchangeAlgorithm != null && privateKey.KeyExchangeAlgorithm.StartsWith(Rsa, StringComparison.OrdinalIgnoreCase))
                {
                    dataKeyCipher = new GcmBlockCipher(new AesEngine());
                }
                else
                {
                    Log.LogError("Unsupported key type {} for key {}.", privateKey.KeyExchangeAlgorithm, keyName);
                    return false;
                }
                
                var key = CryptoHelper.ExportPrivateKey(privateKey);

                var keyParameter = new AeadParameters(new KeyParameter(key), TagLen, _iv);
                    
                    
                dataKeyCipher.Init(false, keyParameter);
                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedDataKey.Length - _iv.Length);
                dataKeyValue = new byte[dataKeyCipher.GetOutputSize(cipherText.Length)];

                var len = dataKeyCipher.ProcessBytes(cipherText, 0, cipherText.Length, dataKeyValue, 0);
                dataKeyCipher.DoFinal(dataKeyValue, len);

                keyDigest = Digest.ComputeHash(encryptedDataKey);

            }
            catch (Exception e)
            {
                Log.LogError("{} Failed to decrypt data key {} to decrypt messages {}", _logCtx, keyName, e.Message);
                return false;
            }
            _dataKey = new AesManaged { Key = dataKeyValue, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };// new (dataKeyValue, "AES");
            _dataKeyCache.TryAdd(Unpooled.WrappedBuffer(keyDigest), _dataKey);
            return true;
        }

		private IByteBuffer DecryptData(AesManaged dataKeySecret, MessageMetadata msgMetadata, IByteBuffer payload)
		{

			// unpack iv and encrypted data
			var ivString = msgMetadata.EncryptionParam;
			ivString.CopyTo(_iv, 0);
            var keyParameter = new AeadParameters(new KeyParameter(dataKeySecret.Key), TagLen, _iv);
			
			IByteBuffer targetBuf = null;
			try
			{
				_cipher.Init(false, keyParameter);

				var sourceNioBuf = payload.GetIoBuffer(payload.ReaderIndex, payload.ReadableBytes);

				int maxLength = _cipher.GetOutputSize(payload.ReadableBytes);
				targetBuf = PooledByteBufferAllocator.Default.Buffer(maxLength, maxLength);
				var targetNioBuf = new byte[maxLength];

                var len = _cipher.ProcessBytes(sourceNioBuf.ToArray(), 0, sourceNioBuf.ToArray().Length, targetNioBuf, 0);
                var decryptedSize = _cipher.DoFinal(targetNioBuf, len);
				//int decryptedSize = _cipher.doFinal(sourceNioBuf, targetNioBuf);
                targetBuf.WriteBytes(targetNioBuf);
                targetBuf.SetWriterIndex(decryptedSize);

			}
			catch (Exception e) 
			{
				Log.LogError("{} Failed to decrypt message {}", _logCtx, e.Message);
				if (targetBuf != null)
				{
					targetBuf.Release();
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

				var msgDataKey = encKeys[i].Value.ToByteArray();
				var keyDigest = Digest.ComputeHash(msgDataKey);
				if (_dataKeyCache.TryGetValue(Unpooled.WrappedBuffer(keyDigest), out var storedSecretKey))
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
					Log.LogWarning("{} Failed to decrypt data or data key is not in cache. Will attempt to refresh", _logCtx);
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
				var encDataKey = kbv.Value.ToByteArray();
				IList<KeyValue> encKeyMeta = kbv.Metadata;
				return DecryptDataKey(kbv.Key, encDataKey, encKeyMeta, keyReader);
			}).DefaultIfEmpty(null).First();

			if (encKeyInfo == null || _dataKey == null)
			{
				// Unable to decrypt data key
				return null;
			}

			return GetKeyAndDecryptData(msgMetadata, payload);

		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(MessageCrypto));

	}


}