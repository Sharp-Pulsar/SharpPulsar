using SharpPulsar.Builder;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Shared;
using System.Collections.Generic;
using System.IO;

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
namespace SharpPulsar.Crypto
{
    public class DefaultCryptoKeyReader : ICryptoKeyReader
	{
		private readonly string _defaultPublicKey;
		private readonly string _defaultPrivateKey;

		private readonly IDictionary<string, string> _publicKeys;
		private readonly IDictionary<string, string> _privateKeys;

		public static DefaultCryptoKeyReaderBuilder Builder()
		{
			return new DefaultCryptoKeyReaderBuilder();
		}

		internal DefaultCryptoKeyReader(DefaultCryptoKeyReaderConfigurationData conf)
		{
			_defaultPublicKey = conf.DefaultPublicKey;
			_defaultPrivateKey = conf.DefaultPrivateKey;
			_publicKeys = conf.PublicKeys;
			_privateKeys = conf.PrivateKeys;
		}

		public EncryptionKeyInfo GetPublicKey(string keyName, IDictionary<string, string> metadata)
		{
			EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
			string publicKey = _publicKeys.GetOrDefault(keyName, _defaultPublicKey);

			keyInfo.Key = LoadKey(publicKey);

			return keyInfo;
		}

		public EncryptionKeyInfo GetPrivateKey(string keyName, IDictionary<string, string> metadata)
		{
			EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
			string privateKey = _privateKeys.GetOrDefault(keyName, _defaultPrivateKey);

			keyInfo.Key = LoadKey(privateKey);

			return keyInfo;
		}
		private byte[] LoadKey(string keyFilePath)
		{
			return (byte[])(object)File.ReadAllBytes(Path.GetFullPath(keyFilePath));
		}

	}
}
