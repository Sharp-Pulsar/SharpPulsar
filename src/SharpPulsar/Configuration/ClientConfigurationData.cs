using SharpPulsar.Common;
using SharpPulsar.Auth;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using SharpPulsar.API;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;

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


    /// <summary>
    /// This is a simple holder of the client configuration values.
    /// </summary>
    public sealed class ClientConfigurationData
    {
        /// <summary>
        /// TLS KeyStore type configuration: JKS, PKCS12
        /// </summary>
        public long InitialBackoffIntervalMs = 100;

        /// <summary>
        /// Number of IO threads.
        /// </summary>
        public int NumIoThreads = Environment.ProcessorCount;

        /// <summary>
        /// Number of consumer listener threads
        /// </summary>
        public int NumListenerThreads = Environment.ProcessorCount;

        /// <summary>
        /// Duration of waiting for a connection to a broker to be established.
        /// If the duration passes without a response from a broker, the connection attempt is dropped.
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Maximum duration for completing a request.
        /// </summary>
        public int RequestTimeoutMs { get; set; } = 60000;

        /// <summary>
        /// Seconds of auto refreshing certificate
        /// </summary>
        public int AutoCertRefreshSeconds { get; set; } = 300;

        /// <summary>
        /// Maximum read time of a request.
        /// </summary>
        public int ReadTimeoutMs { get; set; } = 60000;
        public string WebUrl { get; set; }

        /// <summary>
        /// Maximum times of redirected lookup requests.
        /// </summary>
		public int MaxLookupRedirects { get; set; } = 20;

        public long MaxBackoffIntervalMs = 60;

        /// <summary>
        /// Client lookup timeout (in milliseconds).
        /// </summary>
        public long LookupTimeoutMs { get; set; } = -1;

        /// <summary>
        /// TLS TrustStore type configuration. You need to set this configuration when client authentication
        /// is required.
        /// </summary>
        public string TlsTrustStoreType { get; set; } = "PKCS12";

        /// <summary>
        /// Whether to enable transaction.
        /// </summary>
        public bool EnableTransaction { get; set; } = false;

        /// <summary>
        /// Path of TLS TrustStore.
        /// </summary>
        public string TlsTrustStorePath { get; set; } = null;

        /// <summary>
        /// Path to the trusted TLS certificate file.
        /// </summary>
		public string TlsTrustCertsFilePath { get; set; } = null;

        /// <summary>
        /// Password of TLS TrustStore.
        /// </summary>
		public string TlsTrustStorePassword { get; set; } = null;

        /// <summary>
        /// Initial backoff interval (in nanosecond).
        /// </summary>
        public long InitialBackoffIntervalNanos { get; set; } = TimeUnit.TimeUnit.MILLISECONDS.ToNanoseconds(100);

        /// <summary>
        /// Max backoff interval (in nanosecond).
        /// </summary>
        public long MaxBackoffIntervalNanos = TimeUnit.TimeUnit.SECONDS.ToNanoseconds(60);


        /// <summary>
        /// Whether to enable BusyWait for EpollEventLoopGroup."
        /// </summary>
        public bool EnableBusyWait { get; set; } = false;

        /// <summary>
        /// The TLS provider used by an internal client to authenticate with other Pulsar brokers.
        /// </summary>
        public string SslProvider { get; set; } = null;

        /// <summary>
        /// Set TLS using KeyStore way.
        /// </summary>
        public bool UseKeyStoreTls { get; set; } = false;

        /// <summary>
        /// Whether the client accepts untrusted TLS certificates from the broker.
        /// </summary>
		public bool TlsAllowInsecureConnection { get; set; } = false;

        /// <summary>
        /// URL of proxy service. proxyServiceUrl and proxyProtocol must be mutually inclusive
        /// </summary>
        public string ProxyServiceUrl { get; set; }

        /// <summary>
        /// Protocol of proxy service. proxyServiceUrl and proxyProtocol must be mutually inclusive
        /// </summary>
        public ProxyProtocol? ProxyProtocol { get; set; }
        private long _memoryLimitBytes = 64 * 1024 * 1024;
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan LookupTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ClientCnx { get; set; } = TimeSpan.FromSeconds(10);
        public int WebServicePort { get; set; } = 8080;

        public string WebServiceScheme { get; set; } = "http";

        /// <summary>
        /// Interval to print client stats (in seconds)
        /// </summary>
		public TimeSpan StatsIntervalSeconds { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// "Number of connections established between the client and each Broker. A value of 0 means to disable connection pooling.
        /// </summary>
		public int ConnectionsPerBroker { get; set; } = 1;
        public X509Certificate2 TrustedCertificateAuthority { get; set; }

        /// <summary>
        /// Whether the hostname is validated when the client creates a TLS connection with brokers
        /// </summary>
        public bool TlsHostnameVerificationEnable { get; set; } = false;

        /// <summary>
        /// The number of concurrent lookup requests that can be sent on each broker connection. Setting a maximum prevents overloading a broker.
        /// </summary>
		public int ConcurrentLookupRequest { get; set; } = 5000;

        /// <summary>
        /// Maximum number of lookup requests allowed on each broker connection to prevent overloading a broker.
        /// </summary>
		public int MaxLookupRequest { get; set; } = 50000;

        /// <summary>
        /// Maximum number of rejected requests of a broker in a certain time frame (60 seconds)
        /// after the current connection is closed and the client
        /// creating a new connection to connect to a different broker.
        /// </summary>
		public int MaxNumberOfRejectedRequestPerConnection { get; set; } = 50;

        /// <summary>
        /// Seconds of keeping alive interval for each client broker connection.
        /// </summary>
        public int keepAliveIntervalSeconds { get; set; } = 30;

        private IAuthentication _authentication;

        /// <summary>
        /// Authentication settings of the client.
        /// </summary>
		public IAuthentication Authentication
		{
			get { return _authentication ??= new AuthenticationDisabled(); }
			set => _authentication = value;
        }

        /// <summary>
        /// The implementation class of ServiceUrlProvider used to generate ServiceUrl.
        /// </summary>
        public IServiceUrlProvider ServiceUrlProvider { get; set; }

        /// <summary>
        /// The max duration (in milliseconds) to quarantine endpoints that fail to connect A value of 0 means don't quarantine any endpoints even if they fail.
        /// </summary>
        public long ServiceUrlQuarantineInitDurationMs = 60000;

        /// <summary>
        /// The max duration (in milliseconds) to quarantine endpoints that fail to connect.A value of 0 means don't quarantine any endpoints even if they fail.
        /// </summary>
        public long ServiceUrlQuarantineMaxDurationMs = TimeUnit.TimeUnit.DAYS.ToMilliseconds(1);

        /// <summary>
        /// Class name of authentication plugin of the client.
        /// </summary>
        public string AuthPluginClassName { get; set; }

        /// <summary>
        /// Original principal for proxy authentication scenarios.
        /// </summary>
        public string OriginalPrincipal {  get; set; }

        /// <summary>
        /// Client operation timeout (in milliseconds).
        /// </summary>
        public long OperationTimeoutMs = 30000;

        /// <summary>
        /// Listener name for lookup. Clients can use listenerName to choose one of the listeners
        /// as the service URL to create a connection to the broker as long as the network is accessible.
        /// \"advertisedListeners\" must enabled in broker side.
        /// </summary>
        public string ListenerName { get; set; }

        /// <summary>
        /// Authentication map of the client.
        /// </summary>
		public IDictionary<string, string> AuthParamMap { get; set; }

        /// <summary>
        /// Authentication parameter of the client.
        /// </summary>
		public string AuthParams { get; set; }

        /// <summary>
        /// "Release the connection if it is not used for more than [connectionMaxIdleSeconds] seconds. If  [connectionMaxIdleSeconds] < 0, disabled the feature that auto release the idle connections
        /// </summary>
        public int ConnectionMaxIdleSeconds { get; set; } = 60;

        /// <summary>
        /// Whether to use TCP NoDelay option.
        /// </summary>
        public bool UseTcpNoDelay = true;
        private bool _useTls;
        private string _serviceUrl;

        /// <summary>
        /// Whether to use TLS
        /// </summary>
		public bool UseTls
        {
            get
			{
				if (_useTls)
				{
					return true;
				}
				if (_serviceUrl != null && (_serviceUrl.StartsWith("pulsar+ssl") || _serviceUrl.StartsWith("https")))
				{
					_useTls = true;
					return true;
				}
				return false;
			}
            set => _useTls = value;
        }

        /// <summary>
        /// Path to the TLS key file.
        /// </summary>
        public string TlsKeyFilePath = null;

        /// <summary>
        /// Path to the TLS certificate file.
        /// </summary>
        public string TlsCertificateFilePath = null;

        /// <summary>
        /// Limit of client memory usage (in byte). The 64M default can guarantee a high producer throughput.
        /// </summary>
        public long MemoryLimitBytes 
        {
            get { return _memoryLimitBytes; }
            set 
            {
                _memoryLimitBytes = value;
            }
        }
        /// <summary>
        /// Pulsar cluster HTTP URL to connect to a broker.
        /// </summary>
        public string ServiceUrl
        {
            get => _serviceUrl;
            set => _serviceUrl = value;
        }

        public int ProtocolVersion { get; set; } = 19;
		public X509Certificate2Collection ClientCertificates { get; set; }

		public DateTime Clock { get; set; } = DateTime.Now;

        /// <summary>
        /// Set of TLS Ciphers.
        /// </summary>
        public ISet<string> TlsCiphers = new SortedSet<string>();

        /// <summary>
        /// Protocols of TLS.
        /// </summary>
        public ISet<string> TlsProtocols = new SortedSet<string>();

        /// <summary>
        /// The Pulsar client dns lookup bind address, default behavior is bind on 0.0.0.0
        /// </summary>
        public string DnsLookupBindAddress { get; set; } = null;

       /// <summary>
       /// The Pulsar client dns lookup bind port, takes effect when dnsLookupBindAddress is configured,
       /// default value is 0.
       /// </summary>
       public int DnsLookupBindPort { get; set; } = 0;

        /// <summary>
        /// The Pulsar client dns lookup server address
        /// </summary>
        public IList<DnsEndPoint> DnsServerAddresses = new List<DnsEndPoint>();

    }

}