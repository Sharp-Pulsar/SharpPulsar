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
namespace org.apache.pulsar.client.impl
{

	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Authentication = org.apache.pulsar.client.api.Authentication;
	using AuthenticationFactory = org.apache.pulsar.client.api.AuthenticationFactory;
	using ClientBuilder = org.apache.pulsar.client.api.ClientBuilder;
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using UnsupportedAuthenticationException = org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
	using ServiceUrlProvider = org.apache.pulsar.client.api.ServiceUrlProvider;
	using ClientConfigurationData = org.apache.pulsar.client.impl.conf.ClientConfigurationData;
	using ConfigurationDataUtils = org.apache.pulsar.client.impl.conf.ConfigurationDataUtils;

	public class ClientBuilderImpl : ClientBuilder
	{
		internal ClientConfigurationData conf;

		public ClientBuilderImpl() : this(new ClientConfigurationData())
		{
		}

		public ClientBuilderImpl(ClientConfigurationData conf)
		{
			this.conf = conf;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.PulsarClient build() throws org.apache.pulsar.client.api.PulsarClientException
		public override PulsarClient build()
		{
			if (StringUtils.isBlank(conf.ServiceUrl) && conf.ServiceUrlProvider == null)
			{
				throw new System.ArgumentException("service URL or service URL provider needs to be specified on the ClientBuilder object.");
			}
			if (StringUtils.isNotBlank(conf.ServiceUrl) && conf.ServiceUrlProvider != null)
			{
				throw new System.ArgumentException("Can only chose one way service URL or service URL provider.");
			}
			if (conf.ServiceUrlProvider != null)
			{
				if (StringUtils.isBlank(conf.ServiceUrlProvider.ServiceUrl))
				{
					throw new System.ArgumentException("Cannot get service url from service url provider.");
				}
				else
				{
					conf.ServiceUrl = conf.ServiceUrlProvider.ServiceUrl;
				}
			}
			PulsarClient client = new PulsarClientImpl(conf);
			if (conf.ServiceUrlProvider != null)
			{
				conf.ServiceUrlProvider.initialize(client);
			}
			return client;
		}

		public override ClientBuilder clone()
		{
			return new ClientBuilderImpl(conf.clone());
		}

		public override ClientBuilder loadConf(IDictionary<string, object> config)
		{
			conf = ConfigurationDataUtils.loadData(config, conf, typeof(ClientConfigurationData));
			return this;
		}

		public override ClientBuilder serviceUrl(string serviceUrl)
		{
			if (StringUtils.isBlank(serviceUrl))
			{
				throw new System.ArgumentException("Param serviceUrl must not be blank.");
			}
			conf.ServiceUrl = serviceUrl;
			if (!conf.UseTls)
			{
				enableTls(serviceUrl.StartsWith("pulsar+ssl", StringComparison.Ordinal) || serviceUrl.StartsWith("https", StringComparison.Ordinal));
			}
			return this;
		}

		public override ClientBuilder serviceUrlProvider(ServiceUrlProvider serviceUrlProvider)
		{
			if (serviceUrlProvider == null)
			{
				throw new System.ArgumentException("Param serviceUrlProvider must not be null.");
			}
			conf.ServiceUrlProvider = serviceUrlProvider;
			return this;
		}

		public override ClientBuilder authentication(Authentication authentication)
		{
			conf.Authentication = authentication;
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ClientBuilder authentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public override ClientBuilder authentication(string authPluginClassName, string authParamsString)
		{
			conf.Authentication = AuthenticationFactory.create(authPluginClassName, authParamsString);
			return this;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.ClientBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException
		public override ClientBuilder authentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			conf.Authentication = AuthenticationFactory.create(authPluginClassName, authParams);
			return this;
		}

		public override ClientBuilder operationTimeout(int operationTimeout, TimeUnit unit)
		{
			conf.OperationTimeoutMs = unit.toMillis(operationTimeout);
			return this;
		}

		public override ClientBuilder ioThreads(int numIoThreads)
		{
			conf.NumIoThreads = numIoThreads;
			return this;
		}

		public override ClientBuilder listenerThreads(int numListenerThreads)
		{
			conf.NumListenerThreads = numListenerThreads;
			return this;
		}

		public override ClientBuilder connectionsPerBroker(int connectionsPerBroker)
		{
			conf.ConnectionsPerBroker = connectionsPerBroker;
			return this;
		}

		public override ClientBuilder enableTcpNoDelay(bool useTcpNoDelay)
		{
			conf.UseTcpNoDelay = useTcpNoDelay;
			return this;
		}

		public override ClientBuilder enableTls(bool useTls)
		{
			conf.UseTls = useTls;
			return this;
		}

		public override ClientBuilder enableTlsHostnameVerification(bool enableTlsHostnameVerification)
		{
			conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
			return this;
		}

		public override ClientBuilder tlsTrustCertsFilePath(string tlsTrustCertsFilePath)
		{
			conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
			return this;
		}

		public override ClientBuilder allowTlsInsecureConnection(bool tlsAllowInsecureConnection)
		{
			conf.TlsAllowInsecureConnection = tlsAllowInsecureConnection;
			return this;
		}

		public override ClientBuilder statsInterval(long statsInterval, TimeUnit unit)
		{
			conf.StatsIntervalSeconds = unit.toSeconds(statsInterval);
			return this;
		}

		public override ClientBuilder maxConcurrentLookupRequests(int concurrentLookupRequests)
		{
			conf.ConcurrentLookupRequest = concurrentLookupRequests;
			return this;
		}

		public override ClientBuilder maxLookupRequests(int maxLookupRequests)
		{
			conf.MaxLookupRequest = maxLookupRequests;
			return this;
		}

		public override ClientBuilder maxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
		{
			conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
			return this;
		}

		public override ClientBuilder keepAliveInterval(int keepAliveInterval, TimeUnit unit)
		{
			conf.KeepAliveIntervalSeconds = (int)unit.toSeconds(keepAliveInterval);
			return this;
		}

		public override ClientBuilder connectionTimeout(int duration, TimeUnit unit)
		{
			conf.ConnectionTimeoutMs = (int)unit.toMillis(duration);
			return this;
		}

		public override ClientBuilder startingBackoffInterval(long duration, TimeUnit unit)
		{
			conf.InitialBackoffIntervalNanos = unit.toNanos(duration);
			return this;
		}

		public override ClientBuilder maxBackoffInterval(long duration, TimeUnit unit)
		{
			conf.MaxBackoffIntervalNanos = unit.toNanos(duration);
			return this;
		}

		public virtual ClientConfigurationData ClientConfigurationData
		{
			get
			{
				return conf;
			}
		}

		public override ClientBuilder clock(Clock clock)
		{
			conf.Clock = clock;
			return this;
		}
	}

}