﻿using System;
using System.Collections.Generic;
using SharpPulsar.Protocol.Proto;

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
namespace SharpPulsar.Interfaces
{

    /// <summary>
	/// Interface that abstracts the method to encrypt/decrypt message for End to End Encryption.
	/// </summary>
	public interface IMessageCrypto
	{
		public static int IV_LEN = 12;

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
		void AddPublicKeyCipher(ISet<string> keyNames, ICryptoKeyReader keyReader);


		/*
		 * Remove a key <p> Remove the key identified by the keyName from the list of keys.<p>
		 *
		 * @param keyName Unique name to identify the key
		 *
		 * @return true if succeeded, false otherwise
		 */
		bool RemoveKeyCipher(string keyName);

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
        byte[] Encrypt(ISet<string> encKeys, ICryptoKeyReader keyReader, MessageMetadata messageMetadata, byte[] payload);

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
		byte[] Decrypt(MessageMetadata messageMetadata, byte[] payload, ICryptoKeyReader keyReader);
	}
}