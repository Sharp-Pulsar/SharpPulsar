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
namespace org.apache.pulsar.client.impl.auth
{

	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using FileModifiedTimeUpdater = org.apache.pulsar.common.util.FileModifiedTimeUpdater;
	using SecurityUtility = org.apache.pulsar.common.util.SecurityUtility;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class AuthenticationDataTls : AuthenticationDataProvider
	{
		protected internal X509Certificate[] tlsCertificates;
		protected internal PrivateKey tlsPrivateKey;
		protected internal FileModifiedTimeUpdater certFile, keyFile;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public AuthenticationDataTls(String certFilePath, String keyFilePath) throws java.security.KeyManagementException
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

		public override bool hasDataForTls()
		{
			return true;
		}

		public override Certificate[] TlsCertificates
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

		public override PrivateKey TlsPrivateKey
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

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(AuthenticationDataTls));
	}

}