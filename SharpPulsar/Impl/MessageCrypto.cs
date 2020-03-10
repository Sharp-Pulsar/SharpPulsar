
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
using System.Security;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
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

		private static RsaKeyPairGenerator _keyGenerator;
		private const int TagLen = 16 * 8;
		public const int IvLen = 256;
		private byte[] _iv = new byte[IvLen];
		private SHA256Managed _hash;
		private string _logCtx;

        private long _lastKeyAccess;
		// Data key which is used to encrypt message
		private AesManaged _dataKey;
        private ConcurrentDictionary<byte[], AesManaged> _dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private readonly ConcurrentDictionary<string, EncryptionKeyInfo> _encryptedDataKeyMap;

		private readonly SecureRandom _secureRandom;
		
		public MessageCrypto(string logCtx, bool keyGenNeeded)
		{
			_dataKey = new AesManaged();
			_hash = new SHA256Managed();
			_keyGenerator = new RsaKeyPairGenerator();
            _keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
			_secureRandom = new SecureRandom();
			_logCtx = logCtx;
			_encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			_dataKeyCache = new ConcurrentDictionary<byte[], AesManaged>(); //CacheBuilder.newBuilder().expireAfterAccess(4, TimeUnit.HOURS).build(new CacheLoaderAnonymousInnerClass(this));

			try
			{
                // If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					_hash = new SHA256Managed();//new MD5CryptoServiceProvider();

					_dataKey = null;
					return;
				}
				
            }
			catch (Exception e) 
			{
				Log.LogError("{} MessageCrypto initialization Failed {}", logCtx, e.Message);

			}

			// Generate data key to encrypt messages
            if (_dataKey != null) _dataKey.Key = PublicDataKey();

            _iv = new byte[IvLen];
		}

        private RSACryptoServiceProvider LoadPublicKey(sbyte[] keyBytes)
		{
            return CryptoHelper.GetRsaProviderFromPem(StringHelper.NewString(keyBytes).Trim());
		}

		private RSACryptoServiceProvider LoadPrivateKey(sbyte[] keyBytes)
		{
            return CryptoHelper.GetRsaProviderFromPem(StringHelper.NewString(keyBytes).Trim());
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
            // Generate data key
            _dataKey.Key = PublicDataKey();

            foreach (var key in keyNames)
            {
                AddPublicKeyCipher(key, keyReader);
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
            var encryptedKey = CryptoHelper.Encrypt(_dataKey.Key, (byte[])(object)keyInfo.Key);
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
		
        public virtual byte[] Encrypt(ISet<string> encKeys, ICryptoKeyReader keyReader, MessageMetadata msgMetadata, byte[] payload)
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
							kvList.Add(new KeyValue
							{
								Key = m.Key,
								Value = m.Value
							});
						});
						msgMetadata.EncryptionKeys.Add(new EncryptionKeys{ Key = keyName, Value = (byte[])(object)keyInfo.Key, Metadatas = new List<KeyValue>(kvList) });
					}
					else
					{
						msgMetadata.EncryptionKeys.Add(new EncryptionKeys { Key = keyName, Value = (byte[])(object)keyInfo.Key });
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
			// Update message metadata with encryption param
			msgMetadata.EncryptionParam = _iv;
			try
			{
				// Encrypt the data
                var eData = CryptoHelper.Encrypt(payload, _dataKey);

				return eData;
            }
			catch (Exception e)
			{

				Log.LogError("{} Failed to encrypt message. {}", _logCtx, e);
				throw new PulsarClientException.CryptoException(e.Message);

			}

		}

		private bool DecryptDataKey(string keyName, byte[] encryptedDataKey, IList<KeyValue> encKeyMeta, ICryptoKeyReader keyReader)
		{

            // Read the private key info using callback
            EncryptionKeyInfo keyInfo = keyReader.GetPrivateKey(keyName, encKeyMeta.ToDictionary(x=>x.Key, x=> x.Value));

			// Convert key from byte to PivateKey
            RSACryptoServiceProvider privateKey;
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
			try
            {

				// Decrypt data key using private key
                dataKeyValue = CryptoHelper.Decrypt(encryptedDataKey, privateKey.ExportRSAPrivateKey());
                keyDigest = _hash.ComputeHash(encryptedDataKey);

            }
            catch (Exception e) 
            {
                Log.LogError("{} Failed to decrypt data key {} to decrypt messages {}", _logCtx, keyName, e.Message);
                return false;
            }
            _dataKey = new AsymmetricCipherKeyPair();
            _dataKey = dataKeyValue;
            _dataKeyCache.TryAdd(keyDigest, _dataKey);
            return true;
        }

		private byte[] DecryptData(byte[] dataKeySecret, MessageMetadata msgMetadata, byte[] payload)
		{

			// unpack iv and encrypted data
			var ivString = msgMetadata.EncryptionParam;
			try
            {
				var dData = CryptoHelper.Decrypt(dataKeySecret, payload);
                return dData;
            }
			catch (Exception e) 
			{
				Log.LogError("{} Failed to decrypt message {}", _logCtx, e.Message);
            }

            return null;
        }

        private byte[] PublicDataKey()
        {
            var keys = _keyGenerator.GenerateKeyPair();
            var pubF = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keys.Public);
            return pubF.GetEncoded();
		}
        private byte[] PrivateDataKey()
        {
            var keys = _keyGenerator.GenerateKeyPair();
            var privF = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keys.Private);
            return privF.GetEncoded();
        }
		private byte[] GetKeyAndDecryptData(MessageMetadata msgMetadata, byte[] payload)
		{

            byte[] decryptedData = null;

			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeys;

			// Go through all keys to retrieve data key from cache
			for (var i = 0; i < encKeys.Count; i++)
			{

				var msgDataKey = encKeys[i].Value;
				var keyDigest = Digest.ComputeHash(msgDataKey);
				if (_dataKeyCache.TryGetValue(keyDigest, out var storedSecretKey))
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
		public byte[] Decrypt(MessageMetadata msgMetadata, byte[] payload, ICryptoKeyReader keyReader)
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
				var encDataKey = kbv.Value;
				IList<KeyValue> encKeyMeta = kbv.Metadatas;
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