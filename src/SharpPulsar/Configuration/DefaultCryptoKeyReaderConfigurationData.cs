using Akka.Util.Internal;
using System;
using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Configuration
{
	[Serializable]
	public class DefaultCryptoKeyReaderConfigurationData
	{
        private const string ToStringFormat = "%s(defaultPublicKey=%s, defaultPrivateKey=%s, publicKeys=%s, privateKeys=%s)";
		
		public string DefaultPublicKey { get; set; }
		public string DefaultPrivateKey { get; set; }

		public IDictionary<string, string> PublicKeys = new Dictionary<string, string>();

		public IDictionary<string, string> PrivateKeys = new Dictionary<string, string>();

		public virtual void SetPublicKey(string keyName, string publicKey)
		{
			PublicKeys[keyName] = publicKey;
		}

		public virtual void SetPrivateKey(string keyName, string privateKey)
		{
			PrivateKeys[keyName] = privateKey;
		}

		public DefaultCryptoKeyReaderConfigurationData Clone()
		{
			DefaultCryptoKeyReaderConfigurationData clone = new DefaultCryptoKeyReaderConfigurationData();

			if (!string.ReferenceEquals(DefaultPublicKey, null))
			{
				clone.DefaultPublicKey = DefaultPublicKey;
			}

			if (!string.ReferenceEquals(DefaultPrivateKey, null))
			{
				clone.DefaultPrivateKey = DefaultPrivateKey;
			}

			if (PublicKeys != null)
			{
				clone.PublicKeys = new Dictionary<string, string>(PublicKeys);
			}

			if (PrivateKeys != null)
			{
				clone.PrivateKeys = new Dictionary<string, string>(PrivateKeys);
			}

			return clone;
		}

		public override string ToString()
		{
			return string.Format(ToStringFormat, this.GetType().Name, MaskKeyData(DefaultPublicKey), MaskKeyData(DefaultPrivateKey), MaskKeyData(PublicKeys), MaskKeyData(PrivateKeys));
		}

		private static string MaskKeyData(IDictionary<string, string> keys)
		{
			if (keys == null)
			{
				return "null";
			}
			else
			{
				StringBuilder keysStr = new StringBuilder();
				keysStr.Append("{");

				IList<string> kvList = new List<string>();
				keys.ForEach(kv => kvList.Add(kv.Key + "=" + MaskKeyData(kv.Value)));
				keysStr.Append(string.Join(", ", kvList));

				keysStr.Append("}");
				return keysStr.ToString();
			}
		}

		private static string MaskKeyData(string key)
		{
			if (string.ReferenceEquals(key, null))
			{
				return "null";
			}
			else if (key.StartsWith("data:", StringComparison.Ordinal))
			{
				return "data:*****";
			}
			else
			{
				return key;
			}
		}

    }
}
