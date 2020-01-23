using TimeUnit = BAMCIS.Util.Concurrent.TimeUnit;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
using SharpPulsar.Util;
using System;

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
	public class ClientConfigurationData 
	{
		public const long SerialVersionUID = 1L;

		public string ServiceUrl { get; set; }
		public IServiceUrlProvider ServiceUrlProvider { get; set; }

		private IAuthentication _authentication = new AuthenticationDisabled();
		public string AuthPluginClassName { get; set; }
		public string AuthParams { get; set; }

		public long OperationTimeoutMs = 30000;
		public long StatsIntervalSeconds = 60;

		public int NumIoThreads = 1;
		public int NumListenerThreads = 1;
		public int ConnectionsPerBroker = 1;

		public bool UseTcpNoDelay = true;

		private bool _useTls = false;
		public string TlsTrustCertsFilePath = "";
		public bool TlsAllowInsecureConnection = false;
		public bool TlsHostnameVerificationEnable = false;
		public int ConcurrentLookupRequest = 5000;
		public int MaxLookupRequest = 50000;
		public int MaxNumberOfRejectedRequestPerConnection = 50;
		public int KeepAliveIntervalSeconds = 30;
		public int ConnectionTimeoutMs = 10000;
		public int RequestTimeoutMs = 60000;
		public long InitialBackoffIntervalNanos = TimeUnit.MILLISECONDS.ToNanos(100);
		public long MaxBackoffIntervalNanos = TimeUnit.SECONDS.ToNanos(60);
		public DateTime Clock = DateTime.Now;

		public IAuthentication Authentication
		{
			get
			{
				if (_authentication == null)
				{
					this._authentication = new AuthenticationDisabled();
				}
				return _authentication;
			}
			set
			{
				_authentication = value;
			}
		}

		public bool UseTls
		{
			get
			{
				if (_useTls)
				{
					return true;
				}
				if (ServiceUrl != null && (this.ServiceUrl.StartsWith("pulsar+ssl") || this.ServiceUrl.StartsWith("https")))
				{
					this._useTls = true;
					return true;
				}
				return false;
			}
			set
			{
				_useTls = value;
			}
		}

	}

}