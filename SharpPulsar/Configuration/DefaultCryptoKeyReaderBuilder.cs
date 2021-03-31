using Akka.Util.Internal;
using SharpPulsar.Crypto;
using System;
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
namespace SharpPulsar.Configuration
{
	public class DefaultCryptoKeyReaderBuilder
	{

		private DefaultCryptoKeyReaderConfigurationData _conf;

		internal DefaultCryptoKeyReaderBuilder() : this(new DefaultCryptoKeyReaderConfigurationData())
		{
		}

		internal DefaultCryptoKeyReaderBuilder(DefaultCryptoKeyReaderConfigurationData conf)
		{
			this._conf = conf;
		}

		public virtual DefaultCryptoKeyReaderBuilder DefaultPublicKey(string defaultPublicKey)
		{
			_conf.DefaultPublicKey = defaultPublicKey;
			return this;
		}

		public virtual DefaultCryptoKeyReaderBuilder DefaultPrivateKey(string defaultPrivateKey)
		{
			_conf.DefaultPrivateKey = defaultPrivateKey;
			return this;
		}

		public virtual DefaultCryptoKeyReaderBuilder PublicKey(string keyName, string publicKey)
		{
			_conf.SetPublicKey(keyName, publicKey);
			return this;
		}

		public virtual DefaultCryptoKeyReaderBuilder PrivateKey(string keyName, string privateKey)
		{
			_conf.SetPrivateKey(keyName, privateKey);
			return this;
		}

		public virtual DefaultCryptoKeyReaderBuilder PublicKeys(IDictionary<string, string> publicKeys)
		{
			publicKeys.ForEach(kv => _conf.PublicKeys.Add(kv));
			return this;
		}

		public virtual DefaultCryptoKeyReaderBuilder PrivateKeys(IDictionary<string, string> privateKeys)
		{
			privateKeys.ForEach(kv => _conf.PrivateKeys.Add(kv));
			return this;
		}

		public virtual DefaultCryptoKeyReader Build()
		{
			return new DefaultCryptoKeyReader(_conf);
		}

		public DefaultCryptoKeyReaderBuilder Clone()
		{
			return new DefaultCryptoKeyReaderBuilder(_conf.Clone());
		}

	}
}
