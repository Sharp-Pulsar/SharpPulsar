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
namespace SharpPulsar.Crypto
{

	using IOUtils = org.apache.commons.io.IOUtils;
	using CryptoKeyReader = org.apache.pulsar.client.api.CryptoKeyReader;
	using EncryptionKeyInfo = org.apache.pulsar.client.api.EncryptionKeyInfo;
	using URL = org.apache.pulsar.client.api.url.URL;
	using DefaultCryptoKeyReaderConfigurationData = Org.Apache.Pulsar.Client.Impl.conf.DefaultCryptoKeyReaderConfigurationData;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class DefaultCryptoKeyReader : CryptoKeyReader
	{

		private static readonly Logger _lOG = LoggerFactory.getLogger(typeof(DefaultCryptoKeyReader));

		private const string ApplicationXPemFile = "application/x-pem-file";

		private string _defaultPublicKey;
		private string _defaultPrivateKey;

		private IDictionary<string, string> _publicKeys;
		private IDictionary<string, string> _privateKeys;

		public static DefaultCryptoKeyReaderBuilder Builder()
		{
			return new DefaultCryptoKeyReaderBuilder();
		}

		internal DefaultCryptoKeyReader(DefaultCryptoKeyReaderConfigurationData conf)
		{
			this._defaultPublicKey = conf.DefaultPublicKey;
			this._defaultPrivateKey = conf.DefaultPrivateKey;
			this._publicKeys = conf.PublicKeys;
			this._privateKeys = conf.PrivateKeys;
		}

		public override EncryptionKeyInfo GetPublicKey(string keyName, IDictionary<string, string> metadata)
		{
			EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
			string publicKey = _publicKeys.getOrDefault(keyName, _defaultPublicKey);

			if (string.ReferenceEquals(publicKey, null))
			{
				_lOG.warn("Public key named {} is not set", keyName);
			}
			else
			{
				try
				{
					keyInfo.Key = LoadKey(publicKey);
				}
				catch (Exception e)
				{
					_lOG.error("Failed to load public key named {}", keyName, e);
				}
			}

			return keyInfo;
		}

		public override EncryptionKeyInfo GetPrivateKey(string keyName, IDictionary<string, string> metadata)
		{
			EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
			string privateKey = _privateKeys.getOrDefault(keyName, _defaultPrivateKey);

			if (string.ReferenceEquals(privateKey, null))
			{
				_lOG.warn("Private key named {} is not set", keyName);
			}
			else
			{
				try
				{
					keyInfo.Key = LoadKey(privateKey);
				}
				catch (Exception e)
				{
					_lOG.error("Failed to load private key named {}", keyName, e);
				}
			}

			return keyInfo;
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private byte[] loadKey(String keyUrl) throws java.io.IOException, IllegalAccessException, InstantiationException
		private sbyte[] LoadKey(string keyUrl)
		{
			try
			{
				URLConnection urlConnection = (new URL(keyUrl)).openConnection();
				string protocol = urlConnection.URL.Protocol;
				if ("data".Equals(protocol) && !ApplicationXPemFile.Equals(urlConnection.ContentType))
				{
					throw new System.ArgumentException("Unsupported media type or encoding format: " + urlConnection.ContentType);
				}
				return IOUtils.toByteArray(urlConnection);
			}
			catch (URISyntaxException)
			{
				throw new System.ArgumentException("Invalid key format");
			}
		}

	}
}
