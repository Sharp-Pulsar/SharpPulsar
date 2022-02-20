using SharpPulsar.Auth;
using SharpPulsar.Common;
using SharpPulsar.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
namespace SharpPulsar.Configuration
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
            _conf.ClientCertificates = certs;
            return this;
        }

        public PulsarClientConfigBuilder AllowTlsInsecureConnection(bool allowTlsInsecureConnection)
        {
            _conf.TlsAllowInsecureConnection = allowTlsInsecureConnection;
            return this;
        }

        public PulsarClientConfigBuilder AddTrustedAuthCert(X509Certificate2 cert)
        {
            _conf.TrustedCertificateAuthority = cert;
            return this;
        }
        public PulsarClientConfigBuilder WebUrl(string webUrl)
        {
            _conf.WebUrl = webUrl;
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

        public PulsarClientConfigBuilder ServiceUrlProvider(IServiceUrlProvider serviceUrlProvider)
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

        public PulsarClientConfigBuilder ProxyServiceUrl(string proxyAddress, ProxyProtocol protocol)
        {
            _conf.ProxyServiceUrl = proxyAddress;
            _conf.ProxyProtocol = protocol;
            return this;
        }

        public PulsarClientConfigBuilder ListenerName(string listenerName)
        {
            _conf.ListenerName = listenerName;
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

        public PulsarClientConfigBuilder OperationTimeout(TimeSpan operationTimeout)
        {
            _conf.OperationTimeout = operationTimeout;
            return this;
        }
        public PulsarClientConfigBuilder LookupTimeout(TimeSpan lookupTimeout)
        {
            _conf.LookupTimeout = lookupTimeout;
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

        public PulsarClientConfigBuilder StatsInterval(TimeSpan statsIntervalSeconds)
        {
            _conf.StatsIntervalSeconds = statsIntervalSeconds;
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


        public PulsarClientConfigBuilder MaxLookupRedirects(int maxLookupRedirects)
        {
            _conf.MaxLookupRedirects = maxLookupRedirects;
            return this;
        }

        public PulsarClientConfigBuilder MaxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
        {
            _conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
            return this;
        }
        public PulsarClientConfigBuilder MemoryLimitBytes(long memoryLimitBytes)
        {
            if(memoryLimitBytes < 1)
                throw new ArgumentNullException("MemoryLimitBytes must be greater than 0");

            _conf.MemoryLimitBytes = memoryLimitBytes;
            return this;
        }
        public PulsarClientConfigBuilder ConnectionTimeout(TimeSpan duration)
        {
            _conf.ConnectionTimeoutMs = (int)duration.TotalMilliseconds;
            return this;
        }

        public PulsarClientConfigBuilder StartingBackoffInterval(TimeSpan duration)
        {
            _conf.InitialBackoffIntervalMs = (long)duration.TotalMilliseconds;
            return this;
        }

        public PulsarClientConfigBuilder MaxBackoffInterval(TimeSpan duration)
        {
            _conf.MaxBackoffIntervalMs = (long)duration.TotalMilliseconds;
            return this;
        }
        public ClientConfigurationData ClientConfigurationData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_conf.ServiceUrl) && _conf.ServiceUrlProvider == null)
                {
                    throw new ArgumentException("service URL or service URL provider needs to be specified on the ClientBuilder object.");
                }
                if (!string.IsNullOrWhiteSpace(_conf.ServiceUrl) && _conf.ServiceUrlProvider != null)
                {
                    throw new ArgumentException("Can only chose one way service URL or service URL provider.");
                }
                if (_conf.ServiceUrlProvider != null)
                {
                    if (string.IsNullOrWhiteSpace(_conf.ServiceUrlProvider.ServiceUrl))
                    {
                        throw new ArgumentException("Cannot get service url from service url provider.");
                    }
                    else
                    {
                        _conf.ServiceUrl = _conf.ServiceUrlProvider.ServiceUrl;
                    }
                }
                return _conf;
            }
        }

        public PulsarClientConfigBuilder Clock(DateTime clock)
        {
            _conf.Clock = clock;
            return this;
        }

        public PulsarClientConfigBuilder EnableTransaction(bool enableTransaction)
        {
            _conf.EnableTransaction = enableTransaction;
            return this;
        }
    }

}