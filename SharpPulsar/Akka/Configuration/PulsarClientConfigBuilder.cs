using System;
using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
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
namespace SharpPulsar.Akka.Configuration
{

	public sealed class PulsarClientConfigBuilder
	{
		private ClientConfigurationData _conf = new ClientConfigurationData();

        public ClientConfigurationData Build()
        {
            return _conf;
        }
		public PulsarClientConfigBuilder LoadConf(IDictionary<string, object> config)
		{
			_conf = (ClientConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;

        }

		public PulsarClientConfigBuilder ServiceUrl(string serviceUrl)
		{
			if (string.IsNullOrWhiteSpace(serviceUrl))
			{
				throw new ArgumentException("Param serviceUrl must not be blank.");
			}
			_conf.ServiceUrl = serviceUrl;
			if (!_conf.UseTls)
			{
				EnableTls(serviceUrl.StartsWith("pulsar+ssl", StringComparison.Ordinal) || serviceUrl.StartsWith("https", StringComparison.Ordinal));
			}
            return this;
		}

		public PulsarClientConfigBuilder ServiceUrlProvider(ServiceUrlProvider serviceUrlProvider)
		{
			if (serviceUrlProvider == null)
			{
				throw new ArgumentException("Param serviceUrlProvider must not be null.");
			}
			_conf.ServiceUrlProvider = serviceUrlProvider;
            return this;
		}

		public PulsarClientConfigBuilder Authentication(IAuthentication authentication)
		{
			_conf.Authentication = authentication;
            return this;
		}

		public PulsarClientConfigBuilder Authentication(string authPluginClassName, string authParamsString)
		{
			_conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParamsString);
            return this;
		}

        public PulsarClientConfigBuilder ProtocolVersion(int version)
        {
            _conf.ProtocolVersion = version;
            return this;
        }
		public PulsarClientConfigBuilder Authentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			_conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParams);
            return this;
		}

		public PulsarClientConfigBuilder OperationTimeout(int operationTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.OperationTimeoutMs = unit.ToMillis(operationTimeout);
            return this;
		}

		public PulsarClientConfigBuilder IoThreads(int numIoThreads)
		{
			_conf.NumIoThreads = numIoThreads;
            return this;
		}

		public PulsarClientConfigBuilder ListenerThreads(int numListenerThreads)
		{
			_conf.NumListenerThreads = numListenerThreads;
            return this;
		}

		public PulsarClientConfigBuilder ConnectionsPerBroker(int connectionsPerBroker)
		{
			_conf.ConnectionsPerBroker = connectionsPerBroker;
            return this;
		}

		public PulsarClientConfigBuilder EnableTcpNoDelay(bool useTcpNoDelay)
		{
			_conf.UseTcpNoDelay = useTcpNoDelay;
            return this;
		}

		public PulsarClientConfigBuilder EnableTls(bool useTls)
		{
			_conf.UseTls = useTls;
            return this;
		}

		public PulsarClientConfigBuilder EnableTlsHostnameVerification(bool enableTlsHostnameVerification)
		{
			_conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
            return this;
		}

		public PulsarClientConfigBuilder TlsTrustCertsFilePath(string tlsTrustCertsFilePath)
		{
			_conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
            return this;
		}

		public PulsarClientConfigBuilder AllowTlsInsecureConnection(bool tlsAllowInsecureConnection)
		{
			_conf.TlsAllowInsecureConnection = tlsAllowInsecureConnection;
            return this;
		}

		public PulsarClientConfigBuilder StatsInterval(long statsInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.StatsIntervalSeconds = unit.ToSeconds(statsInterval);
            return this;
		}

		public PulsarClientConfigBuilder MaxConcurrentLookupRequests(int concurrentLookupRequests)
		{
			_conf.ConcurrentLookupRequest = concurrentLookupRequests;
            return this;
		}

		public PulsarClientConfigBuilder MaxLookupRequests(int maxLookupRequests)
		{
			_conf.MaxLookupRequest = maxLookupRequests;
            return this;
		}

		public PulsarClientConfigBuilder MaxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
		{
			_conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
            return this;
		}

		public PulsarClientConfigBuilder KeepAliveInterval(int keepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.KeepAliveIntervalSeconds = (int)unit.ToSeconds(keepAliveInterval);
            return this;
		}

		public PulsarClientConfigBuilder ConnectionTimeout(int duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.ConnectionTimeoutMs = (int)unit.ToMillis(duration);
            return this;
		}

		public PulsarClientConfigBuilder StartingBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.InitialBackoffIntervalNanos = unit.ToNanos(duration);
            return this;
		}

		public PulsarClientConfigBuilder MaxBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.MaxBackoffIntervalNanos = unit.ToNanos(duration);
            return this;
		}

		public ClientConfigurationData ClientConfigurationData => _conf;

        public PulsarClientConfigBuilder Clock(DateTime clock)
		{
			_conf.Clock = clock;
            return this;
		}

    }

}