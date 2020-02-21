using SharpPulsar.Api;
using SharpPulsar.Impl.Auth;
using System;
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
namespace SharpPulsar.Impl.Conf
{


	/// <summary>
	/// This is a simple holder of the client configuration values.
	/// </summary>
	[Serializable]
	public class ClientConfigurationData : ICloneable
	{
		public const long SerialVersionUid = 1L;
        [NonSerialized] 
        private string _serviceUrl;
		[NonSerialized]
		private ServiceUrlProvider _serviceUrlProvider;
		[NonSerialized]
		private IAuthentication _authentication = new AuthenticationDisabled();
		[NonSerialized]
		private string _authPluginClassName;
		[NonSerialized]
		private string _authParams;

		public long OperationTimeoutMs { get; set; } = 30000;
		public long StatsIntervalSeconds { get; set; } = 60;

		public int NumIoThreads { get; set; } = 1;
		public int NumListenerThreads { get; set; } = 1;
		public int ConnectionsPerBroker { get; set; } = 1;

		public bool UseTcpNoDelay { get; set; } = true;

		private bool _useTls = false;
        [NonSerialized] 
        private string _tlsTrustCertsFilePath;
		public bool TlsAllowInsecureConnection { get; set; } = false;
		public bool TlsHostnameVerificationEnable { get; set; } = false;
		public int ConcurrentLookupRequest { get; set; } = 5000;
		public int MaxLookupRequest { get; set; } = 50000;
		public int MaxNumberOfRejectedRequestPerConnection { get; set; } = 50;
		public int KeepAliveIntervalSeconds { get; set; } = 30;
		public int ConnectionTimeoutMs { get; set; } = 10000;
		public int RequestTimeoutMs { get; set; } = 60000;
		public long InitialBackoffIntervalNanos { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToNanos(100);
		public long MaxBackoffIntervalNanos { get; set; } = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToNanos(60);
		[NonSerialized]
		private DateTime _clock = DateTime.Now;

		public IAuthentication Authentication
		{
			get { return _authentication ??= new AuthenticationDisabled(); }
			set => _authentication = value;
        }

        public ServiceUrlProvider ServiceUrlProvider
        {
            get => _serviceUrlProvider;
            set => _serviceUrlProvider = value;
        }
		public string AuthPluginClassName
        {
            get => _authPluginClassName;
            set => _authPluginClassName = value;
        }

		public string AuthParams
        {
            get => _authParams;
            set => _authParams = value;
        }
		public virtual bool UseTls
        {
            get
			{
				if (_useTls)
				{
					return true;
				}
				if (_serviceUrl != null && (this._serviceUrl.StartsWith("pulsar+ssl") || this._serviceUrl.StartsWith("https")))
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

		public string TlsTrustCertsFilePath
        {
            get => _tlsTrustCertsFilePath;
            set => _tlsTrustCertsFilePath = value;
        }

		public DateTime Clock
        {
            get => _clock;
            set => _clock = value;
        }
        public virtual ClientConfigurationData Clone()
		{
			try
			{
				return (ClientConfigurationData) base.MemberwiseClone();
			}
			catch (System.Exception ex)
			{
				throw new System.Exception("Failed to clone ClientConfigurationData");
			}
		}

		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
	}

}