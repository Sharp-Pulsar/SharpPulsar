
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
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;

using SharpPulsar.Cache;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Utility;

namespace SharpPulsar.Crypto
{
    //https://pulsar.apache.org/docs/en/security-encryption/
    //https://github.com/eaba/Bouncy-Castle-AES-GCM-Encryption/blob/master/EncryptionService.cs
    public class MessageCrypto:IMessageCrypto
	{

		private readonly Aes _keyGenerator;
		private const int TagLen = 32 * 8;
		public const int IvLen = 256;
		private byte[] _iv;
		private readonly SHA256 _hash;
		private readonly string _logCtx;
		// Data key which is used to encrypt message
		private byte[] _dataKey;
		private readonly Cache<string, byte[]> _dataKeyCache;

		// Map of key name and encrypted gcm key, metadata pair which is sent with encrypted message
		private readonly ConcurrentDictionary<string, EncryptionKeyInfo> _encryptedDataKeyMap;

		private readonly SecureRandom _secureRandom;
        private readonly ILoggingAdapter _log;

		public MessageCrypto(string logCtx, bool keyGenNeeded, ILoggingAdapter log)
		{

			_iv = new byte[16];
			_hash = SHA256.Create();
			_keyGenerator = Aes.Create();
			_secureRandom = new SecureRandom();
			_logCtx = logCtx;
            _log = log;
            _encryptedDataKeyMap = new ConcurrentDictionary<string, EncryptionKeyInfo>();
			_dataKeyCache =  new Cache<string, byte[]>(TimeSpan.FromHours(4)); //four hours

			try
			{
				// If keygen is not needed(e.g: consumer), data key will be decrypted from the message
				if (!keyGenNeeded)
				{

					_hash = SHA256.Create();//new MD5CryptoServiceProvider();

					_dataKey = null;
					return;
				}
				_secureRandom.NextBytes(_iv);
				_keyGenerator.IV = _iv;

			}
			catch (Exception e)
			{
				_log.Error($"{_logCtx} MessageCrypto initialization Failed {e.Message}");

			}

			// Generate data key to encrypt messages
			_keyGenerator.GenerateKey();
			_dataKey = _keyGenerator.Key;
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
			_keyGenerator.GenerateKey();
			_dataKey = _keyGenerator.Key;

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
			var encryptedKey = CryptoHelper.Encrypt(_dataKey, keyInfo.Key);
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
				if (!_encryptedDataKeyMap.TryGetValue(keyName, out var e))
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
						var encKey = new EncryptionKeys { Key = keyName, Value = keyInfo.Key};
						encKey.Metadatas.AddRange(kvList);
						msgMetadata.EncryptionKeys.Add(encKey);
					}
					else
					{
						msgMetadata.EncryptionKeys.Add(new EncryptionKeys { Key = keyName, Value = keyInfo.Key });
					}
				}
				else
				{
					// We should never reach here.
					_log.Error($"{_logCtx} Failed to find encrypted Data key for key {keyName}.");
				}

			}

			// Create gcm param
			// TODO: Replace random with counter and periodic refreshing based on timer/counter value
			_secureRandom.NextBytes(_iv);
			// Update message metadata with encryption param
			msgMetadata.EncryptionParam = _iv;
			try
			{
				// Encrypt the data

				var encData = CryptoHelper.Encrypt(_dataKey, payload, _iv, TagLen);

				return encData;
			}
			catch (Exception e)
			{

				_log.Error($"{_logCtx} Failed to encrypt message. {e}");
				throw new PulsarClientException.CryptoException(e.Message);

			}

		}

		private bool DecryptDataKey(string keyName, byte[] encryptedDataKey, IList<KeyValue> encKeyMeta, ICryptoKeyReader keyReader)
		{

			// Read the private key info using callback
			EncryptionKeyInfo keyInfo = keyReader.GetPrivateKey(keyName, encKeyMeta.ToDictionary(x => x.Key, x => x.Value));

			// Convert key from byte to PivateKey

			// Decrypt data key to decrypt messages
			byte[] dataKeyValue;
			string keyDigest;
			try
			{

				// Decrypt data key using private key
				dataKeyValue = CryptoHelper.Decrypt(encryptedDataKey, keyInfo.Key);
				keyDigest = Convert.ToBase64String(_hash.ComputeHash(encryptedDataKey));

			}
			catch (Exception e)
			{
				_log.Error($"{_logCtx} Failed to decrypt data key {keyName} to decrypt messages {e.Message}");
				return false;
			}
			_dataKey = dataKeyValue;
			_dataKeyCache.Put(keyDigest, _dataKey);
			return true;
		}

		private byte[] DecryptData(byte[] dataKeySecret, MessageMetadata msgMetadata, byte[] payload)
		{

			// unpack iv and encrypted data
			var iv = msgMetadata.EncryptionParam;
			_iv = iv;
			try
			{
				var encData = CryptoHelper.Decrypt(dataKeySecret, payload, _iv, TagLen);

				return encData;
			}
			catch (Exception e)
			{
				_log.Error($"{_logCtx} Failed to decrypt message {e.Message}");
			}

			return null;
		}

		private byte[] GetKeyAndDecryptData(MessageMetadata msgMetadata, byte[] payload)
		{

			byte[] decryptedData = null;

			IList<EncryptionKeys> encKeys = msgMetadata.EncryptionKeys;

			// Go through all keys to retrieve data key from cache
			foreach (var t in encKeys)
			{
				var msgDataKey = t.Value;
				var keyDigest = Convert.ToBase64String(_hash.ComputeHash(msgDataKey));
                var storedSecretKey = _dataKeyCache.Get(keyDigest);
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
					_log.Warning($"{_logCtx} Failed to decrypt data or data key is not in cache. Will attempt to refresh");
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

    }


}