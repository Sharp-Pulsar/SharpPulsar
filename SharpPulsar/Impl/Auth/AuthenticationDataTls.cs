using SharpPulsar.Api;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using SharpPulsar.Util;

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
		protected internal X509Certificate2[] tlsCertificates;
		protected internal AsymmetricAlgorithm tlsPrivateKey;
		protected internal FileModifiedTimeUpdater certFile, keyFile;
		public AuthenticationDataTls(string certFilePath, string keyFilePath)
		{
			if (string.ReferenceEquals(certFilePath, null))
			{
				throw new System.ArgumentException("certFilePath must not be null");
			}
			if (string.ReferenceEquals(keyFilePath, null))
			{
				throw new System.ArgumentException("keyFilePath must not be null");
			}
			this.certFile = new FileModifiedTimeUpdater(certFilePath);
			this.keyFile = new FileModifiedTimeUpdater(keyFilePath);
			this.tlsCertificates = SecurityUtility.loadCertificatesFromPemFile(certFilePath);
			this.tlsPrivateKey = SecurityUtility.loadPrivateKeyFromPemFile(keyFilePath);
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
				if (this.certFile.checkAndRefresh())
				{
					try
					{
						this.tlsCertificates = SecurityUtility.loadCertificatesFromPemFile(certFile.FileName);
					}
					catch (KeyManagementException e)
					{
						LOG.error("Unable to refresh authData for cert {}: ", certFile.FileName, e);
					}
				}
				return this.tlsCertificates;
			}
		}

		public AsymmetricAlgorithm TlsPrivateKey
		{
			get
			{
				if (this.keyFile.checkAndRefresh())
				{
					try
					{
						this.tlsPrivateKey = SecurityUtility.loadPrivateKeyFromPemFile(keyFile.FileName);
					}
					catch (KeyManagementException e)
					{
						LOG.error("Unable to refresh authData for cert {}: ", keyFile.FileName, e);
					}
				}
				return this.tlsPrivateKey;
			}
		}

		private static readonly Logger<> LOG = LoggerFactory.getLogger(typeof(AuthenticationDataTls));
	}

}