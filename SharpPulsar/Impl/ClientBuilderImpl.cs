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
namespace SharpPulsar.Impl
{

	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Authentication = SharpPulsar.Api.Authentication;
	using AuthenticationFactory = SharpPulsar.Api.AuthenticationFactory;
	using ClientBuilder = SharpPulsar.Api.ClientBuilder;
	using PulsarClient = SharpPulsar.Api.PulsarClient;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using UnsupportedAuthenticationException = SharpPulsar.Api.PulsarClientException.UnsupportedAuthenticationException;
	using ServiceUrlProvider = SharpPulsar.Api.ServiceUrlProvider;
	using ClientConfigurationData = SharpPulsar.Impl.Conf.ClientConfigurationData;
	using ConfigurationDataUtils = SharpPulsar.Impl.Conf.ConfigurationDataUtils;

	public class ClientBuilderImpl : ClientBuilder
	{
		internal ClientConfigurationData Conf;

		public ClientBuilderImpl() : this(new ClientConfigurationData())
		{
		}

		public ClientBuilderImpl(ClientConfigurationData Conf)
		{
			this.Conf = Conf;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.PulsarClient build() throws SharpPulsar.api.PulsarClientException
		public override PulsarClient Build()
		{
			if (StringUtils.isBlank(Conf.ServiceUrl) && Conf.ServiceUrlProvider == null)
			{
				throw new System.ArgumentException("service URL or service URL provider needs to be specified on the ClientBuilder object.");
			}
			if (StringUtils.isNotBlank(Conf.ServiceUrl) && Conf.ServiceUrlProvider != null)
			{
				throw new System.ArgumentException("Can only chose one way service URL or service URL provider.");
			}
			if (Conf.ServiceUrlProvider != null)
			{
				if (StringUtils.isBlank(Conf.ServiceUrlProvider.ServiceUrl))
				{
					throw new System.ArgumentException("Cannot get service url from service url provider.");
				}
				else
				{
					Conf.ServiceUrl = Conf.ServiceUrlProvider.ServiceUrl;
				}
			}
			PulsarClient Client = new PulsarClientImpl(Conf);
			if (Conf.ServiceUrlProvider != null)
			{
				Conf.ServiceUrlProvider.initialize(Client);
			}
			return Client;
		}

		public override ClientBuilder Clone()
		{
			return new ClientBuilderImpl(Conf.clone());
		}

		public override ClientBuilder LoadConf(IDictionary<string, object> Config)
		{
			Conf = ConfigurationDataUtils.loadData(Config, Conf, typeof(ClientConfigurationData));
			return this;
		}

		public override ClientBuilder ServiceUrl(string ServiceUrl)
		{
			if (StringUtils.isBlank(ServiceUrl))
			{
				throw new System.ArgumentException("Param serviceUrl must not be blank.");
			}
			Conf.ServiceUrl = ServiceUrl;
			if (!Conf.UseTls)
			{
				EnableTls(ServiceUrl.StartsWith("pulsar+ssl", StringComparison.Ordinal) || ServiceUrl.StartsWith("https", StringComparison.Ordinal));
			}
			return this;
		}

		public override ClientBuilder ServiceUrlProvider(ServiceUrlProvider ServiceUrlProvider)
		{
			if (ServiceUrlProvider == null)
			{
				throw new System.ArgumentException("Param serviceUrlProvider must not be null.");
			}
			Conf.ServiceUrlProvider = ServiceUrlProvider;
			return this;
		}

		public override ClientBuilder Authentication(Authentication Authentication)
		{
			Conf.Authentication = Authentication;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.ClientBuilder authentication(String authPluginClassName, String authParamsString) throws SharpPulsar.api.PulsarClientException.UnsupportedAuthenticationException
		public override ClientBuilder Authentication(string AuthPluginClassName, string AuthParamsString)
		{
			Conf.Authentication = AuthenticationFactory.create(AuthPluginClassName, AuthParamsString);
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.ClientBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws SharpPulsar.api.PulsarClientException.UnsupportedAuthenticationException
		public override ClientBuilder Authentication(string AuthPluginClassName, IDictionary<string, string> AuthParams)
		{
			Conf.Authentication = AuthenticationFactory.create(AuthPluginClassName, AuthParams);
			return this;
		}

		public override ClientBuilder OperationTimeout(int OperationTimeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.OperationTimeoutMs = Unit.toMillis(OperationTimeout);
			return this;
		}

		public override ClientBuilder IoThreads(int NumIoThreads)
		{
			Conf.NumIoThreads = NumIoThreads;
			return this;
		}

		public override ClientBuilder ListenerThreads(int NumListenerThreads)
		{
			Conf.NumListenerThreads = NumListenerThreads;
			return this;
		}

		public override ClientBuilder ConnectionsPerBroker(int ConnectionsPerBroker)
		{
			Conf.ConnectionsPerBroker = ConnectionsPerBroker;
			return this;
		}

		public override ClientBuilder EnableTcpNoDelay(bool UseTcpNoDelay)
		{
			Conf.UseTcpNoDelay = UseTcpNoDelay;
			return this;
		}

		public override ClientBuilder EnableTls(bool UseTls)
		{
			Conf.UseTls = UseTls;
			return this;
		}

		public override ClientBuilder EnableTlsHostnameVerification(bool EnableTlsHostnameVerification)
		{
			Conf.TlsHostnameVerificationEnable = EnableTlsHostnameVerification;
			return this;
		}

		public override ClientBuilder TlsTrustCertsFilePath(string TlsTrustCertsFilePath)
		{
			Conf.TlsTrustCertsFilePath = TlsTrustCertsFilePath;
			return this;
		}

		public override ClientBuilder AllowTlsInsecureConnection(bool TlsAllowInsecureConnection)
		{
			Conf.TlsAllowInsecureConnection = TlsAllowInsecureConnection;
			return this;
		}

		public override ClientBuilder StatsInterval(long StatsInterval, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.StatsIntervalSeconds = Unit.toSeconds(StatsInterval);
			return this;
		}

		public override ClientBuilder MaxConcurrentLookupRequests(int ConcurrentLookupRequests)
		{
			Conf.ConcurrentLookupRequest = ConcurrentLookupRequests;
			return this;
		}

		public override ClientBuilder MaxLookupRequests(int MaxLookupRequests)
		{
			Conf.MaxLookupRequest = MaxLookupRequests;
			return this;
		}

		public override ClientBuilder MaxNumberOfRejectedRequestPerConnection(int MaxNumberOfRejectedRequestPerConnection)
		{
			Conf.MaxNumberOfRejectedRequestPerConnection = MaxNumberOfRejectedRequestPerConnection;
			return this;
		}

		public override ClientBuilder KeepAliveInterval(int KeepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.KeepAliveIntervalSeconds = (int)Unit.toSeconds(KeepAliveInterval);
			return this;
		}

		public override ClientBuilder ConnectionTimeout(int Duration, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.ConnectionTimeoutMs = (int)Unit.toMillis(Duration);
			return this;
		}

		public override ClientBuilder StartingBackoffInterval(long Duration, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.InitialBackoffIntervalNanos = Unit.toNanos(Duration);
			return this;
		}

		public override ClientBuilder MaxBackoffInterval(long Duration, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			Conf.MaxBackoffIntervalNanos = Unit.toNanos(Duration);
			return this;
		}

		public virtual ClientConfigurationData ClientConfigurationData
		{
			get
			{
				return Conf;
			}
		}

		public override ClientBuilder Clock(Clock Clock)
		{
			Conf.Clock = Clock;
			return this;
		}
	}

}