using SharpPulsar.Api;
using SharpPulsar.Impl.Auth;
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
namespace SharpPulsar.Impl.Conf
{


	/// <summary>
	/// This is a simple holder of the client configuration values.
	/// </summary>
	public sealed class ClientConfigurationData
    {
		/// <summary>
		/// TLS KeyStore type configuration: JKS, PKCS12
		/// </summary>
		public string TlsTrustStoreType { get; set; } = "PKCS12";
        public string TlsTrustStorePath { get; set; } = null;
        private string TlsTrustStorePassword { get; set; } = null;
        private ISet<string> TlsCiphers { get; set; } = new HashSet<string>();
        private ISet<string>TlsProtocols { get; set; } = new HashSet<string>();
		public string SslProvider { get; set; } = null;
		public bool UseKeyStoreTls { get; set; } = false;
		public bool TlsAllowInsecureConnection { get; set; } = false;
		public string ProxyServiceUrl { get; set; }
        public ProxyProtocol? ProxyProtocol { get; set; }
		public int OperationTimeoutMs { get; set; } = 30000;
        public int WebServicePort { get; set; } = 8080;
        public string WebServiceScheme { get; set; } = "http";
		public bool UseProxy { get; set; } = false;
		public long StatsIntervalSeconds { get; set; } = 60;
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

        public ServiceUrlProvider ServiceUrlProvider { get; set; }
		public string AuthPluginClassName { get; set; }
		public string ListenerName { get; set; }

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

        public string ServiceUrl
        {
            get => _serviceUrl;
            set => _serviceUrl = value;
        }

        public int ProtocolVersion { get; set; } = 15;
		public X509Certificate2Collection TlsTrustCerts { get; set; }

		public DateTime Clock { get; set; } = DateTime.Now;
        
	}

}