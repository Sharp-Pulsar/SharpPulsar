using SharpPulsar.Api;
using SharpPulsar.Impl.Auth;
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
namespace SharpPulsar.Impl.Conf
{


	/// <summary>
	/// This is a simple holder of the client configuration values.
	/// </summary>
	[Serializable]
	public class ClientConfigurationData : ICloneable
	{
		public const long SerialVersionUID = 1L;
		[NonSerialized]
		public string ServiceUrl;
		[NonSerialized]
		public ServiceUrlProvider ServiceUrlProvider;
		[NonSerialized]
		private IAuthentication _authentication = new AuthenticationDisabled();
		[NonSerialized]
		public string AuthPluginClassName;
		[NonSerialized]
		public string AuthParams;

		public long OperationTimeoutMs = 30000;
		public long StatsIntervalSeconds = 60;

		public int NumIoThreads = 1;
		public int NumListenerThreads = 1;
		public int ConnectionsPerBroker = 1;

		public bool UseTcpNoDelay = true;

		private bool _useTls = false;
		[NonSerialized]
		public string TlsTrustCertsFilePath;
		public bool TlsAllowInsecureConnection = false;
		public bool TlsHostnameVerificationEnable = false;
		public int ConcurrentLookupRequest = 5000;
		public int MaxLookupRequest = 50000;
		public int MaxNumberOfRejectedRequestPerConnection = 50;
		public int KeepAliveIntervalSeconds = 30;
		public int ConnectionTimeoutMs = 10000;
		public int RequestTimeoutMs = 60000;
		public long InitialBackoffIntervalNanos = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToNanos(100);
		public long MaxBackoffIntervalNanos = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToNanos(60);
		[NonSerialized]
		public DateTime clock = DateTime.Now;

		public virtual IAuthentication Authentication
		{
			get
			{
				if (_authentication == null)
				{
					_authentication = new AuthenticationDisabled();
				}
				return _authentication;
			}
			set
			{
				_authentication = value;
			}
		}

		public virtual bool UseTls
		{
			get
			{
				if (_useTls)
				{
					return true;
				}
				if (ServiceUrl != null && (this.ServiceUrl.StartsWith("pulsar+ssl") || this.ServiceUrl.StartsWith("https")))
				{
					_useTls = true;
					return true;
				}
				return false;
			}
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