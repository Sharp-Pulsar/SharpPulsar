using System;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using SharpPulsar.Api;
using SharpPulsar.Impl;
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
		public void LoadConf(IDictionary<string, object> config)
		{
			_conf = ConfigurationDataUtils.LoadData(config, _conf);
			
		}

		public void ServiceUrl(string serviceUrl)
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
			
		}

		public void ServiceUrlProvider(ServiceUrlProvider serviceUrlProvider)
		{
			if (serviceUrlProvider == null)
			{
				throw new ArgumentException("Param serviceUrlProvider must not be null.");
			}
			_conf.ServiceUrlProvider = serviceUrlProvider;
			
		}

		public void Authentication(IAuthentication authentication)
		{
			_conf.Authentication = authentication;
			
		}

		public void Authentication(string authPluginClassName, string authParamsString)
		{
			_conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParamsString);
			
		}

		public void Authentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			_conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParams);
			
		}

		public void OperationTimeout(int operationTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.OperationTimeoutMs = unit.ToMillis(operationTimeout);
			
		}

		public void IoThreads(int numIoThreads)
		{
			_conf.NumIoThreads = numIoThreads;
			
		}

		public void ListenerThreads(int numListenerThreads)
		{
			_conf.NumListenerThreads = numListenerThreads;
			
		}

		public void ConnectionsPerBroker(int connectionsPerBroker)
		{
			_conf.ConnectionsPerBroker = connectionsPerBroker;
			
		}

		public void EnableTcpNoDelay(bool useTcpNoDelay)
		{
			_conf.UseTcpNoDelay = useTcpNoDelay;
			
		}

		public void EnableTls(bool useTls)
		{
			_conf.UseTls = useTls;
			
		}

		public void EnableTlsHostnameVerification(bool enableTlsHostnameVerification)
		{
			_conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
			
		}

		public void TlsTrustCertsFilePath(string tlsTrustCertsFilePath)
		{
			_conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
			
		}

		public void AllowTlsInsecureConnection(bool tlsAllowInsecureConnection)
		{
			_conf.TlsAllowInsecureConnection = tlsAllowInsecureConnection;
			
		}

		public void StatsInterval(long statsInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.StatsIntervalSeconds = unit.ToSeconds(statsInterval);
			
		}

		public void MaxConcurrentLookupRequests(int concurrentLookupRequests)
		{
			_conf.ConcurrentLookupRequest = concurrentLookupRequests;
			
		}

		public void MaxLookupRequests(int maxLookupRequests)
		{
			_conf.MaxLookupRequest = maxLookupRequests;
			
		}

		public void MaxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
		{
			_conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
			
		}

		public void KeepAliveInterval(int keepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.KeepAliveIntervalSeconds = (int)unit.ToSeconds(keepAliveInterval);
			
		}

		public void ConnectionTimeout(int duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.ConnectionTimeoutMs = (int)unit.ToMillis(duration);
			
		}

		public void StartingBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.InitialBackoffIntervalNanos = unit.ToNanos(duration);
			
		}

		public void MaxBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			_conf.MaxBackoffIntervalNanos = unit.ToNanos(duration);
			
		}

		public ClientConfigurationData ClientConfigurationData => _conf;

        public void Clock(DateTime clock)
		{
			_conf.Clock = clock;
			
		}

    }

}