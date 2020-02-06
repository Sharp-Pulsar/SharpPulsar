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
namespace org.apache.pulsar.client.admin.@internal
{
	using Authentication = client.api.Authentication;
	using AuthenticationFactory = client.api.AuthenticationFactory;
	using PulsarClientException = client.api.PulsarClientException;
	using UnsupportedAuthenticationException = client.api.PulsarClientException.UnsupportedAuthenticationException;
	using ClientConfigurationData = client.impl.conf.ClientConfigurationData;


	public class PulsarAdminBuilderImpl : PulsarAdminBuilder
	{

		protected internal readonly ClientConfigurationData conf;
		private int connectTimeout = PulsarAdmin.DEFAULT_CONNECT_TIMEOUT_SECONDS;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int readTimeout_Conflict = PulsarAdmin.DEFAULT_READ_TIMEOUT_SECONDS;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int requestTimeout_Conflict = PulsarAdmin.DEFAULT_REQUEST_TIMEOUT_SECONDS;
		private TimeUnit connectTimeoutUnit = TimeUnit.SECONDS;
		private TimeUnit readTimeoutUnit = TimeUnit.SECONDS;
		private TimeUnit requestTimeoutUnit = TimeUnit.SECONDS;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdmin build() throws org.apache.pulsar.client.api.PulsarClientException
		public virtual PulsarAdmin build()
		{
			return new PulsarAdmin(conf.ServiceUrl, conf, connectTimeout, connectTimeoutUnit, readTimeout_Conflict, readTimeoutUnit, requestTimeout_Conflict, requestTimeoutUnit);
		}

		public PulsarAdminBuilderImpl()
		{
			this.conf = new ClientConfigurationData();
		}

		private PulsarAdminBuilderImpl(ClientConfigurationData conf)
		{
			this.conf = conf;
		}

		public virtual PulsarAdminBuilder clone()
		{
			return new PulsarAdminBuilderImpl(conf.clone());
		}

		public virtual PulsarAdminBuilder serviceHttpUrl(string serviceHttpUrl)
		{
			conf.ServiceUrl = serviceHttpUrl;
			return this;
		}

		public virtual PulsarAdminBuilder authentication(Authentication authentication)
		{
			conf.Authentication = authentication;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdminBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public virtual PulsarAdminBuilder authentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			conf.Authentication = AuthenticationFactory.create(authPluginClassName, authParams);
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdminBuilder authentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public virtual PulsarAdminBuilder authentication(string authPluginClassName, string authParamsString)
		{
			conf.Authentication = AuthenticationFactory.create(authPluginClassName, authParamsString);
			return this;
		}

		public virtual PulsarAdminBuilder tlsTrustCertsFilePath(string tlsTrustCertsFilePath)
		{
			conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
			return this;
		}

		public virtual PulsarAdminBuilder allowTlsInsecureConnection(bool allowTlsInsecureConnection)
		{
			conf.TlsAllowInsecureConnection = allowTlsInsecureConnection;
			return this;
		}

		public virtual PulsarAdminBuilder enableTlsHostnameVerification(bool enableTlsHostnameVerification)
		{
			conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
			return this;
		}

		public virtual PulsarAdminBuilder connectionTimeout(int connectionTimeout, TimeUnit connectionTimeoutUnit)
		{
			this.connectTimeout = connectionTimeout;
			this.connectTimeoutUnit = connectionTimeoutUnit;
			return this;
		}

		public virtual PulsarAdminBuilder readTimeout(int readTimeout, TimeUnit readTimeoutUnit)
		{
			this.readTimeout_Conflict = readTimeout;
			this.readTimeoutUnit = readTimeoutUnit;
			return this;
		}

		public virtual PulsarAdminBuilder requestTimeout(int requestTimeout, TimeUnit requestTimeoutUnit)
		{
			this.requestTimeout_Conflict = requestTimeout;
			this.requestTimeoutUnit = requestTimeoutUnit;
			return this;
		}
	}

}