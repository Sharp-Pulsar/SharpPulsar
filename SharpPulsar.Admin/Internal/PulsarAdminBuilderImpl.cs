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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using AuthenticationFactory = Org.Apache.Pulsar.Client.Api.AuthenticationFactory;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using UnsupportedAuthenticationException = Org.Apache.Pulsar.Client.Api.PulsarClientException.UnsupportedAuthenticationException;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;


	public class PulsarAdminBuilderImpl : PulsarAdminBuilder
	{

		protected internal readonly ClientConfigurationData Conf;
		private int connectTimeout = PulsarAdmin.DEFAULT_CONNECT_TIMEOUT_SECONDS;
		private int readTimeout = PulsarAdmin.DEFAULT_READ_TIMEOUT_SECONDS;
		private int requestTimeout = PulsarAdmin.DEFAULT_REQUEST_TIMEOUT_SECONDS;
		private TimeUnit connectTimeoutUnit = TimeUnit.SECONDS;
		private TimeUnit readTimeoutUnit = TimeUnit.SECONDS;
		private TimeUnit requestTimeoutUnit = TimeUnit.SECONDS;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdmin build() throws org.apache.pulsar.client.api.PulsarClientException
		public override PulsarAdmin Build()
		{
			return new PulsarAdmin(Conf.ServiceUrl, Conf, connectTimeout, connectTimeoutUnit, readTimeout, readTimeoutUnit, requestTimeout, requestTimeoutUnit);
		}

		public PulsarAdminBuilderImpl()
		{
			this.Conf = new ClientConfigurationData();
		}

		private PulsarAdminBuilderImpl(ClientConfigurationData Conf)
		{
			this.Conf = Conf;
		}

		public override PulsarAdminBuilder Clone()
		{
			return new PulsarAdminBuilderImpl(Conf.clone());
		}

		public override PulsarAdminBuilder ServiceHttpUrl(string ServiceHttpUrl)
		{
			Conf.ServiceUrl = ServiceHttpUrl;
			return this;
		}

		public override PulsarAdminBuilder Authentication(Authentication Authentication)
		{
			Conf.Authentication = Authentication;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdminBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public override PulsarAdminBuilder Authentication(string AuthPluginClassName, IDictionary<string, string> AuthParams)
		{
			Conf.Authentication = AuthenticationFactory.create(AuthPluginClassName, AuthParams);
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.admin.PulsarAdminBuilder authentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public override PulsarAdminBuilder Authentication(string AuthPluginClassName, string AuthParamsString)
		{
			Conf.Authentication = AuthenticationFactory.create(AuthPluginClassName, AuthParamsString);
			return this;
		}

		public override PulsarAdminBuilder TlsTrustCertsFilePath(string TlsTrustCertsFilePath)
		{
			Conf.TlsTrustCertsFilePath = TlsTrustCertsFilePath;
			return this;
		}

		public override PulsarAdminBuilder AllowTlsInsecureConnection(bool AllowTlsInsecureConnection)
		{
			Conf.TlsAllowInsecureConnection = AllowTlsInsecureConnection;
			return this;
		}

		public override PulsarAdminBuilder EnableTlsHostnameVerification(bool EnableTlsHostnameVerification)
		{
			Conf.TlsHostnameVerificationEnable = EnableTlsHostnameVerification;
			return this;
		}

		public override PulsarAdminBuilder ConnectionTimeout(int ConnectionTimeout, TimeUnit ConnectionTimeoutUnit)
		{
			this.connectTimeout = ConnectionTimeout;
			this.connectTimeoutUnit = ConnectionTimeoutUnit;
			return this;
		}

		public override PulsarAdminBuilder ReadTimeout(int ReadTimeout, TimeUnit ReadTimeoutUnit)
		{
			this.readTimeout = ReadTimeout;
			this.readTimeoutUnit = ReadTimeoutUnit;
			return this;
		}

		public override PulsarAdminBuilder RequestTimeout(int RequestTimeout, TimeUnit RequestTimeoutUnit)
		{
			this.requestTimeout = RequestTimeout;
			this.requestTimeoutUnit = RequestTimeoutUnit;
			return this;
		}
	}

}