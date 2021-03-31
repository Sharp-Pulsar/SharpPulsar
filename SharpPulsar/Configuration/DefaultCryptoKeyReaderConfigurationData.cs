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
	public class DefaultCryptoKeyReaderConfigurationData : ICloneable
	{

		private const long SerialVersionUID = 1L;

		private const string ToStringFormat = "%s(defaultPublicKey=%s, defaultPrivateKey=%s, publicKeys=%s, privateKeys=%s)";

		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @NonNull private String defaultPublicKey;
		private string _defaultPublicKey;
		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @NonNull private String defaultPrivateKey;
		private string _defaultPrivateKey;

		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @NonNull private java.util.Map<String, String> publicKeys = new java.util.HashMap<>();
		private IDictionary<string, string> _publicKeys = new Dictionary<string, string>();
		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: @NonNull private java.util.Map<String, String> privateKeys = new java.util.HashMap<>();
		private IDictionary<string, string> _privateKeys = new Dictionary<string, string>();

		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: public void setPublicKey(@NonNull String keyName, @NonNull String publicKey)
		public virtual void SetPublicKey(string keyName, string publicKey)
		{
			_publicKeys[keyName] = publicKey;
		}

		//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		//ORIGINAL LINE: public void setPrivateKey(@NonNull String keyName, @NonNull String privateKey)
		public virtual void SetPrivateKey(string keyName, string privateKey)
		{
			_privateKeys[keyName] = privateKey;
		}

		public override DefaultCryptoKeyReaderConfigurationData Clone()
		{
			DefaultCryptoKeyReaderConfigurationData clone = new DefaultCryptoKeyReaderConfigurationData();

			if (!string.ReferenceEquals(_defaultPublicKey, null))
			{
				clone.DefaultPublicKey = _defaultPublicKey;
			}

			if (!string.ReferenceEquals(_defaultPrivateKey, null))
			{
				clone.DefaultPrivateKey = _defaultPrivateKey;
			}

			if (_publicKeys != null)
			{
				clone.PublicKeys = new Dictionary<string, string>(_publicKeys);
			}

			if (_privateKeys != null)
			{
				clone.PrivateKeys = new Dictionary<string, string>(_privateKeys);
			}

			return clone;
		}

		public override string ToString()
		{
			return string.format(ToStringFormat, this.GetType().Name, MaskKeyData(_defaultPublicKey), MaskKeyData(_defaultPrivateKey), MaskKeyData(_publicKeys), MaskKeyData(_privateKeys));
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
				keys.forEach((k, v) => kvList.Add(k + "=" + maskKeyData(v)));
				keysStr.Append(string.join(", ", kvList));

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
