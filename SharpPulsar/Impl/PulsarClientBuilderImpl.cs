using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using DotNetty.Transport.Channels;
using SharpPulsar.Protocol;
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
namespace SharpPulsar.Impl
{

	public class PulsarClientBuilderImpl : IPulsarClientBuilder
	{
		internal ClientConfigurationData Conf;

		public PulsarClientBuilderImpl() : this(new ClientConfigurationData())
		{
		}

		public PulsarClientBuilderImpl(ClientConfigurationData conf)
		{
			this.Conf = conf;
		}

		public IPulsarClient Build()
		{
			if (string.IsNullOrWhiteSpace(Conf.ServiceUrl) && Conf.ServiceUrlProvider == null)
			{
				throw new ArgumentException("service URL or service URL provider needs to be specified on the ClientBuilder object.");
			}
			if (string.IsNullOrWhiteSpace(Conf.ServiceUrl) && Conf.ServiceUrlProvider != null)
			{
				throw new ArgumentException("Can only chose one way service URL or service URL provider.");
			}
			if (Conf.ServiceUrlProvider != null)
            {
                if (string.IsNullOrWhiteSpace(Conf.ServiceUrlProvider.ServiceUrl))
				{
					throw new ArgumentException("Cannot get service url from service url provider.");
				}

                Conf.ServiceUrl = Conf.ServiceUrlProvider.ServiceUrl;
            }

            var serviceNameResolver = new PulsarServiceNameResolver();
			serviceNameResolver.UpdateServiceUrl(Conf.ServiceUrl);
			var thread = new MultithreadEventLoopGroup(Conf.NumIoThreads);
			var pool = new ConnectionPool(Conf, new MultithreadEventLoopGroup(Conf.NumIoThreads), serviceNameResolver);
            await pool.CreateConnections();
			IPulsarClient client = new PulsarClientImpl(Conf, thread, pool, serviceNameResolver);
            Conf.ServiceUrlProvider?.Initialize(client);
            return client;
		}

		public IPulsarClientBuilder Clone()
		{
			return new PulsarClientBuilderImpl(Conf.Clone());
		}

		public IPulsarClientBuilder LoadConf(IDictionary<string, object> config)
		{
			Conf = ConfigurationDataUtils.LoadData(config, Conf);
			return this;
		}

		public IPulsarClientBuilder ServiceUrl(string serviceUrl)
		{
			if (string.IsNullOrWhiteSpace(serviceUrl))
			{
				throw new ArgumentException("Param serviceUrl must not be blank.");
			}
			Conf.ServiceUrl = serviceUrl;
			if (!Conf.UseTls)
			{
				EnableTls(serviceUrl.StartsWith("pulsar+ssl", StringComparison.Ordinal) || serviceUrl.StartsWith("https", StringComparison.Ordinal));
			}
			return this;
		}

		public IPulsarClientBuilder ServiceUrlProvider(ServiceUrlProvider serviceUrlProvider)
		{
			if (serviceUrlProvider == null)
			{
				throw new ArgumentException("Param serviceUrlProvider must not be null.");
			}
			Conf.ServiceUrlProvider = serviceUrlProvider;
			return this;
		}

		public IPulsarClientBuilder Authentication(IAuthentication authentication)
		{
			Conf.Authentication = authentication;
			return this;
		}

		public IPulsarClientBuilder Authentication(string authPluginClassName, string authParamsString)
		{
			Conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParamsString);
			return this;
		}

		public IPulsarClientBuilder Authentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			Conf.Authentication = AuthenticationFactory.Create(authPluginClassName, authParams);
			return this;
		}

		public IPulsarClientBuilder OperationTimeout(int operationTimeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.OperationTimeoutMs = unit.ToMillis(operationTimeout);
			return this;
		}

		public IPulsarClientBuilder IoThreads(int numIoThreads)
		{
			Conf.NumIoThreads = numIoThreads;
			return this;
		}

		public IPulsarClientBuilder ListenerThreads(int numListenerThreads)
		{
			Conf.NumListenerThreads = numListenerThreads;
			return this;
		}

		public IPulsarClientBuilder ConnectionsPerBroker(int connectionsPerBroker)
		{
			Conf.ConnectionsPerBroker = connectionsPerBroker;
			return this;
		}

		public IPulsarClientBuilder EnableTcpNoDelay(bool useTcpNoDelay)
		{
			Conf.UseTcpNoDelay = useTcpNoDelay;
			return this;
		}

		public IPulsarClientBuilder EnableTls(bool useTls)
		{
			Conf.UseTls = useTls;
			return this;
		}

		public IPulsarClientBuilder EnableTlsHostnameVerification(bool enableTlsHostnameVerification)
		{
			Conf.TlsHostnameVerificationEnable = enableTlsHostnameVerification;
			return this;
		}

		public IPulsarClientBuilder TlsTrustCertsFilePath(string tlsTrustCertsFilePath)
		{
			Conf.TlsTrustCertsFilePath = tlsTrustCertsFilePath;
			return this;
		}

		public IPulsarClientBuilder AllowTlsInsecureConnection(bool tlsAllowInsecureConnection)
		{
			Conf.TlsAllowInsecureConnection = tlsAllowInsecureConnection;
			return this;
		}

		public IPulsarClientBuilder StatsInterval(long statsInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.StatsIntervalSeconds = unit.ToSeconds(statsInterval);
			return this;
		}

		public IPulsarClientBuilder MaxConcurrentLookupRequests(int concurrentLookupRequests)
		{
			Conf.ConcurrentLookupRequest = concurrentLookupRequests;
			return this;
		}

		public IPulsarClientBuilder MaxLookupRequests(int maxLookupRequests)
		{
			Conf.MaxLookupRequest = maxLookupRequests;
			return this;
		}

		public IPulsarClientBuilder MaxNumberOfRejectedRequestPerConnection(int maxNumberOfRejectedRequestPerConnection)
		{
			Conf.MaxNumberOfRejectedRequestPerConnection = maxNumberOfRejectedRequestPerConnection;
			return this;
		}

		public IPulsarClientBuilder KeepAliveInterval(int keepAliveInterval, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.KeepAliveIntervalSeconds = (int)unit.ToSeconds(keepAliveInterval);
			return this;
		}

		public IPulsarClientBuilder ConnectionTimeout(int duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.ConnectionTimeoutMs = (int)unit.ToMillis(duration);
			return this;
		}

		public IPulsarClientBuilder StartingBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.InitialBackoffIntervalNanos = unit.ToNanos(duration);
			return this;
		}

		public IPulsarClientBuilder MaxBackoffInterval(long duration, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			Conf.MaxBackoffIntervalNanos = unit.ToNanos(duration);
			return this;
		}

		public virtual ClientConfigurationData ClientConfigurationData => Conf;

        public IPulsarClientBuilder Clock(DateTime clock)
		{
			Conf.Clock = clock;
			return this;
		}

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

}