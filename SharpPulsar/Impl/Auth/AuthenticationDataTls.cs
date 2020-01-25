using System;

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

	using IAuthenticationDataProvider = SharpPulsar.Api.IAuthenticationDataProvider;
	using FileModifiedTimeUpdater = Org.Apache.Pulsar.Common.Util.FileModifiedTimeUpdater;
	using SecurityUtility = Org.Apache.Pulsar.Common.Util.SecurityUtility;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	[Serializable]
	public class AuthenticationDataTls : IAuthenticationDataProvider
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal X509Certificate[] TlsCertificatesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal PrivateKey TlsPrivateKeyConflict;
		protected internal FileModifiedTimeUpdater CertFile, KeyFile;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public AuthenticationDataTls(String certFilePath, String keyFilePath) throws java.security.KeyManagementException
		public AuthenticationDataTls(string CertFilePath, string KeyFilePath)
		{
			if (string.ReferenceEquals(CertFilePath, null))
			{
				throw new System.ArgumentException("certFilePath must not be null");
			}
			if (string.ReferenceEquals(KeyFilePath, null))
			{
				throw new System.ArgumentException("keyFilePath must not be null");
			}
			this.CertFile = new FileModifiedTimeUpdater(CertFilePath);
			this.KeyFile = new FileModifiedTimeUpdater(KeyFilePath);
			this.TlsCertificatesConflict = SecurityUtility.loadCertificatesFromPemFile(CertFilePath);
			this.TlsPrivateKeyConflict = SecurityUtility.loadPrivateKeyFromPemFile(KeyFilePath);
		}

		/*
		 * TLS
		 */

		public override bool HasDataForTls()
		{
			return true;
		}

		public virtual Certificate[] TlsCertificates
		{
			get
			{
				if (this.CertFile.checkAndRefresh())
				{
					try
					{
						this.TlsCertificatesConflict = SecurityUtility.loadCertificatesFromPemFile(CertFile.FileName);
					}
					catch (KeyManagementException E)
					{
						LOG.error("Unable to refresh authData for cert {}: ", CertFile.FileName, E);
					}
				}
				return this.TlsCertificatesConflict;
			}
		}

		public virtual PrivateKey TlsPrivateKey
		{
			get
			{
				if (this.KeyFile.checkAndRefresh())
				{
					try
					{
						this.TlsPrivateKeyConflict = SecurityUtility.loadPrivateKeyFromPemFile(KeyFile.FileName);
					}
					catch (KeyManagementException E)
					{
						LOG.error("Unable to refresh authData for cert {}: ", KeyFile.FileName, E);
					}
				}
				return this.TlsPrivateKeyConflict;
			}
		}

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(AuthenticationDataTls));
	}

}