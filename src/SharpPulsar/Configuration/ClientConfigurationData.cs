using SharpPulsar.Common;
using SharpPulsar.Auth;
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


	/// <summary>
	/// This is a simple holder of the client configuration values.
	/// </summary>
	public sealed class ClientConfigurationData
    {
		/// <summary>
		/// TLS KeyStore type configuration: JKS, PKCS12
		/// </summary>
		public long InitialBackoffIntervalMs = 100;

		public int ConnectionTimeoutMs { get; set; }
		public string WebUrl { get; set; }
		public int MaxLookupRedirects { get; set; }

		public long MaxBackoffIntervalMs = 60;
        public long LookupTimeoutMs { get; set; } = -1; 

        public string TlsTrustStoreType { get; set; } = "PKCS12";
		public bool EnableTransaction { get; set; } = false;
		public string TlsTrustStorePath { get; set; } = null;
		public string TlsTrustCertsFilePath { get; set; } = null;
		public string TlsTrustStorePassword { get; set; } = null;
        public int InitialBackoffIntervalNanos { get; set; }    //.NET 7
        public string SslProvider { get; set; } = null;
		public bool UseKeyStoreTls { get; set; } = false;
		public bool TlsAllowInsecureConnection { get; set; } = false;
		public string ProxyServiceUrl { get; set; }
        public ProxyProtocol? ProxyProtocol { get; set; }
        private long _memoryLimitBytes = 0;
        public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(3);
		public TimeSpan LookupTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ClientCnx { get; set; } = TimeSpan.FromSeconds(10);
        public int WebServicePort { get; set; } = 8080;

        public string WebServiceScheme { get; set; } = "http";
		public TimeSpan StatsIntervalSeconds { get; set; } = TimeSpan.FromSeconds(60);
		public int ConnectionsPerBroker { get; set; } = 1;
		public X509Certificate2 TrustedCertificateAuthority { get; set; }
		
		public bool TlsHostnameVerificationEnable { get; set; } = false;
		public int ConcurrentLookupRequest { get; set; } = 5000;
		public int MaxLookupRequest { get; set; } = 50000;
		public int MaxNumberOfRejectedRequestPerConnection { get; set; } = 50;
		
        private IAuthentication _authentication;
		public IAuthentication Authentication
		{
			get { return _authentication ??= new AuthenticationDisabled(); }
			set => _authentication = value;
        }

        public IServiceUrlProvider ServiceUrlProvider { get; set; }
		public string AuthPluginClassName { get; set; }
		public string ListenerName { get; set; }

		public IDictionary<string, string> AuthParamMap { get; set; }

		public string AuthParams { get; set; }
        private bool _useTls;
        private string _serviceUrl;
		public  bool UseTls
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
        public long MemoryLimitBytes 
        {
            get { return _memoryLimitBytes; }
            set 
            {
                _memoryLimitBytes = value;
            }
        }
        public string ServiceUrl
        {
            get => _serviceUrl;
            set => _serviceUrl = value;
        }

        public int ProtocolVersion { get; set; } = 19;
		public X509Certificate2Collection ClientCertificates { get; set; }

		public DateTime Clock { get; set; } = DateTime.Now;
        
	}

}