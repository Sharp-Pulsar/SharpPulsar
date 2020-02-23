using SharpPulsar.Api;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using SharpPulsar.Utility;

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
namespace SharpPulsar.Impl.Auth
{


    public class AuthenticationDataTls : IAuthenticationDataProvider
	{
		private X509Certificate2[] _tlsCertificates;
		private AsymmetricAlgorithm _tlsPrivateKey;
		protected internal FileModifiedTimeUpdater CertFile, KeyFile;
		public AuthenticationDataTls(string certFilePath, string keyFilePath)
		{
			if (ReferenceEquals(certFilePath, null))
			{
				throw new System.ArgumentException("certFilePath must not be null");
			}
			if (ReferenceEquals(keyFilePath, null))
			{
				throw new System.ArgumentException("keyFilePath must not be null");
			}
			CertFile = new FileModifiedTimeUpdater(certFilePath);
			KeyFile = new FileModifiedTimeUpdater(keyFilePath);
			_tlsCertificates = SecurityUtility.LoadCertificatesFromPemFile(certFilePath);
			_tlsPrivateKey = SecurityUtility.LoadPrivateKeyFromPemFile(keyFilePath);
		}

		/*
		 * TLS
		 */

		public bool HasDataForTls()
		{
			return true;
		}

		public X509Certificate2[] TlsCertificates
		{
			get
			{
				if (CertFile.CheckAndRefresh())
				{
					try
					{
						_tlsCertificates = SecurityUtility.LoadCertificatesFromPemFile(CertFile.FileName);
					}
					catch (System.Exception e)
					{
						Log.LogError("Unable to refresh authData for cert {}: ", CertFile.FileName);
					}
				}
				return _tlsCertificates;
			}
		}

		public AsymmetricAlgorithm TlsPrivateKey
		{
			get
			{
				if (KeyFile.CheckAndRefresh())
				{
					try
					{
						_tlsPrivateKey = SecurityUtility.LoadPrivateKeyFromPemFile(KeyFile.FileName);
					}
					catch (System.Exception e)
					{
						Log.LogError("Unable to refresh authData for cert {}: ", KeyFile.FileName);
					}
				}
				return _tlsPrivateKey;
			}
		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger((typeof(AuthenticationDataTls)));
	}

}