using SharpPulsar.Interface;
using SharpPulsar.Interface.Auth;
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
	[Serializable]
	public class ClientConfigurationData : ICloneable
	{
		public const long SerialVersionUID = 1L;

		public string ServiceUrl { get; set; }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private transient org.apache.pulsar.client.api.ServiceUrlProvider serviceUrlProvider;
		[NonSerialized]
		private IServiceUrlProvider serviceUrlProvider;

/
		[NonSerialized]
		private Authentication authentication = new AuthenticationDisabled();
		private string authPluginClassName;
		private string authParams;

		private long operationTimeoutMs = 30000;
		private long statsIntervalSeconds = 60;

		private int numIoThreads = 1;
		private int numListenerThreads = 1;
		private int connectionsPerBroker = 1;

		private bool useTcpNoDelay = true;

		private bool useTls = false;
		private string tlsTrustCertsFilePath = "";
		private bool tlsAllowInsecureConnection = false;
		private bool tlsHostnameVerificationEnable = false;
		private int concurrentLookupRequest = 5000;
		private int maxLookupRequest = 50000;
		private int maxNumberOfRejectedRequestPerConnection = 50;
		private int keepAliveIntervalSeconds = 30;
		private int connectionTimeoutMs = 10000;
		private int requestTimeoutMs = 60000;
		private long initialBackoffIntervalNanos = TimeUnit.MILLISECONDS.toNanos(100);
		private long maxBackoffIntervalNanos = TimeUnit.SECONDS.toNanos(60);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnore private java.time.Clock clock = java.time.Clock.systemDefaultZone();
		private Clock clock = Clock.systemDefaultZone();

		public virtual IAuthentication Authentication
		{
			get
			{
				if (authentication == null)
				{
					this.authentication = new AuthenticationDisabled();
				}
				return authentication;
			}
		}

		public virtual bool UseTls
		{
			get
			{
				if (useTls)
				{
					return true;
				}
				if (ServiceUrl != null && (this.ServiceUrl.StartsWith("pulsar+ssl") || this.ServiceUrl.StartsWith("https")))
				{
					this.useTls = true;
					return true;
				}
				return false;
			}
		}

		public virtual ClientConfigurationData clone()
		{
			try
			{
				return (ClientConfigurationData) base.clone();
			}
			catch (CloneNotSupportedException)
			{
				throw new Exception("Failed to clone ClientConfigurationData");
			}
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}
	}

}