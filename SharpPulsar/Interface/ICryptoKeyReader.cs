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
namespace SharpPulsar.Interface
{

	/// <summary>
	/// Interface that abstracts the access to a key store.
	/// </summary>
	public interface ICryptoKeyReader
	{

		/// <summary>
		/// Return the encryption key corresponding to the key name in the argument.
		/// 
		/// <para>This method should be implemented to return the EncryptionKeyInfo. This method will be called at the time of
		/// producer creation as well as consumer receiving messages. Hence, application should not make any blocking calls
		/// within the implementation.
		/// 
		/// </para>
		/// </summary>
		/// <param name="keyName">
		///            Unique name to identify the key </param>
		/// <param name="metadata">
		///            Additional information needed to identify the key </param>
		/// <returns> EncryptionKeyInfo with details about the public key </returns>
		EncryptionKeyInfo GetPublicKey(string keyName, IDictionary<string, string> metadata);

		/// <param name="keyName">
		///            Unique name to identify the key </param>
		/// <param name="metadata">
		///            Additional information needed to identify the key </param>
		/// <returns> byte array of the private key value </returns>
		EncryptionKeyInfo GetPrivateKey(string keyName, IDictionary<string, string> metadata);

	}

}