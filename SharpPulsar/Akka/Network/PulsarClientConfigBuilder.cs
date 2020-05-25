using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
namespace SharpPulsar.Akka.Network
{

	public sealed class PulsarClientConfigBuilder
	{
		private ClientConfigurationData _conf = new ClientConfigurationData();

		public PulsarClientConfigBuilder LoadConf(IDictionary<string, object> config)
		{
			_conf = (ClientConfigurationData)ConfigurationDataUtils.LoadData(config, _conf);
            return this;

        }

        public PulsarClientConfigBuilder AddTlsCerts(X509Certificate2Collection certs)
        {
            _conf.TlsTrustCerts = certs;
            return this;
        }
        public PulsarClientConfigBuilder AddTrustedAuthCert(X509Certificate2 cert)
        {
            _conf.TrustedCertificateAuthority = cert;
            return this;
        }
        public PulsarClientConfigBuilder VerifyCertAuth(bool verify)
        {
            _conf.VerifyCertificateAuthority = verify;
            return this;
        }
        public PulsarClientConfigBuilder VerifyCertName(bool verify)
        {
            _conf.VerifyCertificateName = verify;
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
        public PulsarClientConfigBuilder UseProxy(bool useProxy)
        {
            _conf.UseProxy = useProxy;
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

		public PulsarClientConfigBuilder OperationTimeout(int operationTimeout)
		{
			_conf.OperationTimeoutMs = operationTimeout;
            return this;
		}

		public PulsarClientConfigBuilder ConnectionsPerBroker(int connectionsPerBroker)
		{
			_conf.ConnectionsPerBroker = connectionsPerBroker;
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

		public ClientConfigurationData ClientConfigurationData => _conf;

        public PulsarClientConfigBuilder Clock(DateTime clock)
		{
			_conf.Clock = clock;
            return this;
		}

    }

}