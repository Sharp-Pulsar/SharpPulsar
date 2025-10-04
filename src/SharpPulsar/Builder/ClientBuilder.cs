using System;
using System.Collections.Generic;
using System.Net;
using SharpPulsar.API;
using SharpPulsar.Auth;
using SharpPulsar.Common;
using SharpPulsar.Common.Precondition;
using SharpPulsar.Configuration;
using SharpPulsar.Shared;
using SharpPulsar.Shared.Exceptions;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Builder
{

    public class ClientBuilder : IClientBuilder
    {
        internal ClientConfigurationData conf;

        public ClientBuilder() : this(new ClientConfigurationData())
        {
        }

        public ClientBuilder(ClientConfigurationData conf)
        {
            this.conf = conf;
        }

        public virtual PulsarClient Build()
        {
            Condition.CheckArgument(string.IsNullOrWhiteSpace(conf.ServiceUrl) || conf.ServiceUrlProvider != null, 
                "service URL or service URL provider needs to be specified on the ClientBuilder object.");
            Condition.CheckArgument(string.IsNullOrWhiteSpace(conf.ServiceUrl) || conf.ServiceUrlProvider == null, 
                "Can only chose one way service URL or service URL provider.");

            if (conf.ServiceUrlProvider != null)
            {
                Condition.CheckArgument(!string.IsNullOrWhiteSpace(conf.ServiceUrlProvider.ServiceUrl), "Cannot get service url from service url provider.");
                conf.ServiceUrl = conf.ServiceUrlProvider.ServiceUrl;
            }
            if (conf.Authentication == null || conf.Authentication == AuthenticationDisabled.INSTANCE)
            {
                AuthenticationFromPropsIfAvailable = conf;
            }
            return new PulsarClient(conf);
        }

        public virtual IClientBuilder Clone()
        {
            return new ClientBuilder(conf.clone());
        }

        public virtual IClientBuilder LoadConf(IDictionary<string, object> config)
        {
            conf = (ClientConfigurationData)ConfigurationDataUtils.LoadData(config, conf);
            AuthenticationFromPropsIfAvailable = conf;
            return this;
        }

        public virtual IClientBuilder ServiceUrl(string serviceUrl)
        {
            Condition.CheckArgument(!string.IsNullOrWhiteSpace(serviceUrl), "Param serviceUrl must not be blank.");
            conf.ServiceUrl = serviceUrl;
            if (!conf.UseTls)
            {
                EnableTls(serviceUrl.StartsWith("pulsar+ssl", StringComparison.Ordinal) || serviceUrl.StartsWith("https", StringComparison.Ordinal));
            }
            return this;
        }

        public virtual IClientBuilder ServiceUrlProvider(IServiceUrlProvider serviceUrlProvider)
        {
            Condition.CheckArgument(serviceUrlProvider != null, "Param serviceUrlProvider must not be null.");
            conf.ServiceUrlProvider = serviceUrlProvider;
            return this;
        }

        public virtual IClientBuilder ServiceUrlQuarantineInitDuration(long serviceUrlQuarantineInitDuration, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(serviceUrlQuarantineInitDuration >= 0, "serviceUrlQuarantineInitDuration needs to be >= 0");
            conf.ServiceUrlQuarantineInitDurationMs = unit.ToMilliseconds(serviceUrlQuarantineInitDuration));
            return this;
        }

        public virtual IClientBuilder ServiceUrlQuarantineMaxDuration(long serviceUrlQuarantineMaxDuration, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(serviceUrlQuarantineMaxDuration >= 0, "serviceUrlQuarantineMaxDuration needs to be >= 0");
            conf.ServiceUrlQuarantineMaxDurationMs = (unit.ToMilliseconds(serviceUrlQuarantineMaxDuration));
            return this;
        }

        public virtual IClientBuilder ListenerName(string listenerName)
        {
            Condition.CheckArgument(!string.IsNullOrWhiteSpace(listenerName), "Param listenerName must not be blank.");
            conf.ListenerName = listenerName.Trim();
            return this;
        }

        public virtual IClientBuilder ConnectionMaxIdleSeconds(int connectionMaxIdleSeconds)
        {
            Condition.CheckArgument(connectionMaxIdleSeconds < 0 || connectionMaxIdleSeconds >= ConnectionPool.IDLE_DETECTION_INTERVAL_SECONDS_MIN, "Connection idle detect interval seconds at least " + ConnectionPool.IDLE_DETECTION_INTERVAL_SECONDS_MIN + ".");
            conf.ConnectionMaxIdleSeconds = connectionMaxIdleSeconds;
            return this;
        }

        public virtual IClientBuilder Authentication(IAuthentication authentication)
        {
            conf.Authentication = authentication;
            return this;
        }

        public virtual IClientBuilder OpenTelemetry(OpenTelemetry openTelemetry)
        {
            conf.OpenTelemetry = (openTelemetry);
            return this;
        }
        public virtual IClientBuilder Authentication(string authPluginClassName, string authParamsString)
        {
            conf.AuthPluginClassName = authPluginClassName;
            conf.AuthParams = authParamsString;
            conf.AuthParamMap = null;
            conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParamsString);
            return this;
        }

        public virtual IClientBuilder Authentication(string authPluginClassName, IDictionary<string, string> authParams)
        {
            conf.AuthPluginClassName = authPluginClassName;
            conf.AuthParamMap = authParams;
            conf.AuthParams = null;
            conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParams);
            return this;
        }

        public virtual IClientBuilder OriginalPrincipal(string originalPrincipal)
        {
            conf.OriginalPrincipal = originalPrincipal;
            return this;
        }

        private ClientConfigurationData AuthenticationFromPropsIfAvailable
        {
            set
            {
                string authPluginClass = value.AuthPluginClassName;
                string authParams = value.AuthParams;
                IDictionary<string, string> authParamMap = value.AuthParamMap;
                if (string.IsNullOrWhiteSpace(authPluginClass) || (string.IsNullOrWhiteSpace(authParams) && authParamMap == null))
                {
                    return;
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(authParams))
                    {
                        Authentication(authPluginClass, authParams);
                    }
                    else if (authParamMap != null)
                    {
                        Authentication(authPluginClass, authParamMap);
                    }
                }
                catch (PulsarClientException.UnsupportedAuthenticationException ex)
                {
                    throw new Exception("Failed to create authentication: " + ex.Message, ex);
                }
            }
        }

        public virtual IClientBuilder OperationTimeout(int operationTimeout, TimeUnit.TimeUnit unit)
        {
            Condition.CheckArgument(operationTimeout >= 0, "operationTimeout needs to be >= 0");
            conf.OperationTimeoutMs = unit.ToMilliseconds(operationTimeout));
            return this;
        }

        public virtual IClientBuilder LookupTimeout(int lookupTimeout, TimeUnit.TimeUnit unit)
        {
            conf.LookupTimeoutMs = unit.ToMilliseconds(lookupTimeout));
            return this;
        }

        public virtual IClientBuilder IoThreads(int numIoThreads)
        {
            Condition.CheckArgument(numIoThreads > 0, "ioThreads needs to be > 0");
            conf.NumIoThreads = numIoThreads;
            return this;
        }

        public virtual IClientBuilder ListenerThreads(int numListenerThreads)
        {
            Condition.CheckArgument(numListenerThreads > 0, "listenerThreads needs to be > 0");
            conf.NumListenerThreads = numListenerThreads;
            return this;
        }

        public virtual IClientBuilder ConnectionsPerBroker(int connectionsPerBroker)
        {
            Condition.CheckArgument(connectionsPerBroker >= 0, "connectionsPerBroker needs to be >= 0");
            conf.ConnectionsPerBroker = connectionsPerBroker;
            return this;
        }

        public virtual IClientBuilder EnableTcpNoDelay(bool useTcpNoDelay)
        {
            conf.UseTcpNoDelay = useTcpNoDelay;
            return this;
        }

        public virtual IClientBuilder EnableTls(bool useTls)
        {
            conf.UseTls = useTls;
            return this;
        }

        public virtual IClientBuilder TlsKeyFilePath(string tlsKeyFilePath)
        {
            conf.TlsKeyFilePath = tlsKeyFilePath;
            return this;
        }

        public virtual IClientBuilder TlsCertificateFilePath(string tlsCertificateFilePath)
        {
            conf.TlsCertificateFilePath = tlsCertificateFilePath;
            return this;
        }

        public virtual IClientBuilder EnableTlsHostnameVerification(bool enableTlsHostnameVerification)
        {
            conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
            return this;
        }

        public virtual IClientBuilder TlsTrustCertsFilePath(string tlsTrustCertsFilePath)
        {
            conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
            return this;
        }

        public virtual IClientBuilder AllowTlsInsecureConnection(bool tlsAllowInsecureConnection)
        {
            conf.TlsAllowInsecureConnection = tlsAllowInsecureConnection;
            return this;
        }

        public virtual IClientBuilder UseKeyStoreTls(bool useKeyStoreTls)
        {
            conf.UseKeyStoreTls = useKeyStoreTls;
            return this;
        }

        public virtual IClientBuilder SslProvider(string sslProvider)
        {
            conf.SslProvider = sslProvider;
            return this;
        }

        public virtual IClientBuilder TlsKeyStoreType(string tlsKeyStoreType)
        {
            conf.TlsKeyStoreType = tlsKeyStoreType;
            return this;
        }

        public virtual IClientBuilder TlsKeyStorePath(string tlsTrustStorePath)
        {
            conf.TlsKeyStorePath = tlsTrustStorePath;
            return this;
        }

        public virtual IClientBuilder TlsKeyStorePassword(string tlsKeyStorePassword)
        {
            conf.TlsKeyStorePassword(tlsKeyStorePassword);
            return this;
        }

        public virtual IClientBuilder TlsTrustStoreType(string tlsTrustStoreType)
        {
            conf.TlsTrustStoreType = tlsTrustStoreType;
            return this;
        }

        public virtual IClientBuilder TlsTrustStorePath(string tlsTrustStorePath)
        {
            conf.TlsTrustStorePath = tlsTrustStorePath;
            return this;
        }

        public virtual IClientBuilder TlsTrustStorePassword(string tlsTrustStorePassword)
        {
            conf.TlsTrustStorePassword = tlsTrustStorePassword;
            return this;
        }

        public virtual IClientBuilder TlsCiphers(ISet<string> tlsCiphers)
        {
            conf.TlsCiphers = tlsCiphers;
            return this;
        }

        public virtual IClientBuilder TlsProtocols(ISet<string> tlsProtocols)
        {
            conf.TlsProtocols = tlsProtocols;
            return this;
        }

        public virtual IClientBuilder StatsInterval(long statsInterval, TimeUnit.TimeUnit unit)
        {
            conf.StatsIntervalSeconds = unit.ToMilliseconds(statsInterval);
            return this;
        }

        public virtual IClientBuilder MaxConcurrentLookupRequests(int concurrentLookupRequests)
        {
            conf.ConcurrentLookupRequest = concurrentLookupRequests;
            return this;
        }

        public virtual IClientBuilder MaxLookupRequests(int maxLookupRequests)
        {
            conf.MaxLookupRequest = maxLookupRequests;
            return this;
        }

        public virtual IClientBuilder MaxLookupRedirects(int maxLookupRedirects)
        {
            conf.MaxLookupRedirects = maxLookupRedirects;
            return this;
        }

        public virtual IClientBuilder MaxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
        {
            conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
            return this;
        }

        public virtual IClientBuilder KeepAliveInterval(int keepAliveInterval, TimeUnit.TimeUnit unit)
        {
            conf.KeepAliveIntervalSeconds = ((int)unit.ToSeconds(keepAliveInterval));
            return this;
        }

        public virtual IClientBuilder ConnectionTimeout(int duration, TimeUnit.TimeUnit unit)
        {
            conf.ConnectionTimeoutMs = ((int)unit.ToMilliseconds(duration));
            return this;
        }

        public virtual IClientBuilder StartingBackoffInterval(long duration, TimeUnit.TimeUnit unit)
        {
            conf.InitialBackoffIntervalNanos = unit.ToNanoseconds(duration);
            return this;
        }

        public virtual IClientBuilder MaxBackoffInterval(long duration, TimeUnit.TimeUnit unit)
        {
            conf.MaxBackoffIntervalNanos = unit.ToNanoseconds(duration));
            return this;
        }

        public virtual IClientBuilder EnableBusyWait(bool enableBusyWait)
        {
            conf.EnableBusyWait = enableBusyWait;
            return this;
        }

        public virtual ClientConfigurationData ClientConfigurationData
        {
            get
            {
                return conf;
            }
        }

        public virtual IClientBuilder MemoryLimit(long memoryLimit, SizeUnit unit)
        {
            conf.MemoryLimitBytes = unit.ToBytes(memoryLimit));
            return this;
        }

        public virtual IClientBuilder Clock(DateTime clock)
        {
            conf.Clock = clock;
            return this;
        }

        public virtual IClientBuilder ProxyServiceUrl(string proxyServiceUrl, ProxyProtocol proxyProtocol)
        {
            if (!string.IsNullOrWhiteSpace(proxyServiceUrl))
            {
               Condition.CheckArgument(proxyProtocol != null, "proxyProtocol must be present with proxyServiceUrl");
            }
            conf.ProxyServiceUrl = proxyServiceUrl;
            conf.ProxyProtocol = proxyProtocol;
            return this;
        }

        public virtual IClientBuilder EnableTransaction(bool enableTransaction)
        {
            conf.EnableTransaction = enableTransaction;
            return this;
        }

        public virtual IClientBuilder DnsLookupBind(string address, int port)
        {
            Condition.CheckArgument(port >= 0 && port <= 65535, "DnsLookBindPort need to be within the range of 0 and 65535");
            conf.DnsLookupBindAddress = address;
            conf.DnsLookupBindPort = port;
            return this;
        }

        public virtual IClientBuilder DnsServerAddresses(IList<DnsEndPoint> addresses)
        {
            foreach (DnsEndPoint address in addresses)
            {
                string ip = address.ToString();
                Condition.CheckArgument(InetAddressUtils.isIPv4Address(ip) || InetAddressUtils.isIPv6Address(ip), "DnsServerAddresses need to be valid IPv4 or IPv6 addresses");
            }
            conf.DnsServerAddresses = addresses;
            return this;
        }

        public virtual IClientBuilder Socks5ProxyAddress(DnsEndPoint socks5ProxyAddress)
        {
            //conf.Socks5ProxyAddress = (socks5ProxyAddress);
            return this;
        }

        public virtual IClientBuilder Socks5ProxyUsername(string socks5ProxyUsername)
        {
            //conf.Socks5ProxyUsername = (socks5ProxyUsername);
            return this;
        }

        public virtual IClientBuilder Socks5ProxyPassword(string socks5ProxyPassword)
        {
            //conf.Socks5ProxyPassword = (socks5ProxyPassword);
            return this;
        }

        public virtual IClientBuilder SslFactoryPlugin(string sslFactoryPlugin)
        {
            if (string.IsNullOrWhiteSpace(sslFactoryPlugin))
            {
                //conf.SslFactoryPlugin = (typeof(DefaultPulsarSslFactory).FullName);
            }
            else
            {
               // conf.SslFactoryPlugin(sslFactoryPlugin);
            }
            return this;
        }

        public virtual IClientBuilder SslFactoryPluginParams(string sslFactoryPluginParams)
        {
            //conf.SslFactoryPluginParams = (sslFactoryPluginParams);
            return this;
        }

        public virtual IClientBuilder AutoCertRefreshSeconds(int autoCertRefreshSeconds)
        {
            conf.AutoCertRefreshSeconds = autoCertRefreshSeconds;
            return this;
        }

        /// <summary>
        /// Set the description.
        /// 
        /// <para> By default, when the client connects to the broker, a version string like "Pulsar-Java-v<x.y.z>" will be
        /// carried and saved by the broker. The client version string could be queried from the topic stats.
        /// 
        /// </para>
        /// <para> This method provides a way to add more description to a specific PulsarClient instance. If it's configured,
        /// the description will be appended to the original client version string, with '-' as the separator.
        /// 
        /// </para>
        /// <para>For example, if the client version is 3.0.0, and the description is "forked", the final client version string
        /// will be "Pulsar-Java-v3.0.0-forked".
        /// 
        /// </para>
        /// </summary>
        /// <param name="description"> the description of the current PulsarClient instance </param>
        /// <exception cref="IllegalArgumentException"> if the length of description exceeds 64 </exception>
        public virtual IClientBuilder Description(string description)
        {
            if (!string.ReferenceEquals(description, null) && description.Length > 64)
            {
                throw new System.ArgumentException("description should be at most 64 characters");
            }
            conf.Description = (description);
            return this;
        }

        public virtual IClientBuilder LookupProperties(IDictionary<string, string> properties)
        {
            conf.LookupProperties = (properties);
            return this;
        }
    }
}
