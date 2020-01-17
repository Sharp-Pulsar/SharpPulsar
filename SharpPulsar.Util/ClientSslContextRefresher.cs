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
namespace org.apache.pulsar.common.util
{
	using SslContext = io.netty.handler.ssl.SslContext;
	using AuthenticationDataProvider = org.apache.pulsar.client.api.AuthenticationDataProvider;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("checkstyle:JavadocType") public class ClientSslContextRefresher
	public class ClientSslContextRefresher
	{
		private volatile SslContext sslContext;
		private bool tlsAllowInsecureConnection;
		private string tlsTrustCertsFilePath;
		private AuthenticationDataProvider authData;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClientSslContextRefresher(boolean allowInsecure, String trustCertsFilePath, org.apache.pulsar.client.api.AuthenticationDataProvider authData) throws java.io.IOException, java.security.GeneralSecurityException
		public ClientSslContextRefresher(bool allowInsecure, string trustCertsFilePath, AuthenticationDataProvider authData)
		{
			this.tlsAllowInsecureConnection = allowInsecure;
			this.tlsTrustCertsFilePath = trustCertsFilePath;
			this.authData = authData;

			if (authData != null && authData.hasDataForTls())
			{
				this.sslContext = SecurityUtility.createNettySslContextForClient(this.tlsAllowInsecureConnection, this.tlsTrustCertsFilePath, (X509Certificate[]) authData.TlsCertificates, authData.TlsPrivateKey);
			}
			else
			{
				this.sslContext = SecurityUtility.createNettySslContextForClient(this.tlsAllowInsecureConnection, this.tlsTrustCertsFilePath);
			}
		}

		public virtual SslContext get()
		{
			if (authData != null && authData.hasDataForTls())
			{
				try
				{
					this.sslContext = SecurityUtility.createNettySslContextForClient(this.tlsAllowInsecureConnection, this.tlsTrustCertsFilePath, (X509Certificate[]) authData.TlsCertificates, authData.TlsPrivateKey);
				}
				catch (Exception e) when (e is GeneralSecurityException || e is IOException)
				{
					LOG.error("Exception occured while trying to refresh sslContext: ", e);
				}

			}
			return sslContext;
		}

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(ClientSslContextRefresher));
	}

}