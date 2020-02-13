using System;
using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Admin
{


	using StringUtils = org.apache.commons.lang3.StringUtils;
	using BookiesImpl = Org.Apache.Pulsar.Client.Admin.@internal.BookiesImpl;
	using BrokerStatsImpl = Org.Apache.Pulsar.Client.Admin.@internal.BrokerStatsImpl;
	using BrokersImpl = Org.Apache.Pulsar.Client.Admin.@internal.BrokersImpl;
	using ClustersImpl = Org.Apache.Pulsar.Client.Admin.@internal.ClustersImpl;
	using FunctionsImpl = Org.Apache.Pulsar.Client.Admin.@internal.FunctionsImpl;
	using JacksonConfigurator = Org.Apache.Pulsar.Client.Admin.@internal.JacksonConfigurator;
	using LookupImpl = Org.Apache.Pulsar.Client.Admin.@internal.LookupImpl;
	using NamespacesImpl = Org.Apache.Pulsar.Client.Admin.@internal.NamespacesImpl;
	using NonPersistentTopicsImpl = Org.Apache.Pulsar.Client.Admin.@internal.NonPersistentTopicsImpl;
	using PulsarAdminBuilderImpl = Org.Apache.Pulsar.Client.Admin.@internal.PulsarAdminBuilderImpl;
	using ResourceQuotasImpl = Org.Apache.Pulsar.Client.Admin.@internal.ResourceQuotasImpl;
	using SchemasImpl = Org.Apache.Pulsar.Client.Admin.@internal.SchemasImpl;
	using SinksImpl = Org.Apache.Pulsar.Client.Admin.@internal.SinksImpl;
	using SourcesImpl = Org.Apache.Pulsar.Client.Admin.@internal.SourcesImpl;
	using TenantsImpl = Org.Apache.Pulsar.Client.Admin.@internal.TenantsImpl;
	using TopicsImpl = Org.Apache.Pulsar.Client.Admin.@internal.TopicsImpl;
	using WorkerImpl = Org.Apache.Pulsar.Client.Admin.@internal.WorkerImpl;
	using AsyncHttpConnectorProvider = Org.Apache.Pulsar.Client.Admin.@internal.Http.AsyncHttpConnectorProvider;
	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using AuthenticationFactory = Org.Apache.Pulsar.Client.Api.AuthenticationFactory;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using AuthenticationDisabled = Org.Apache.Pulsar.Client.Impl.Auth.AuthenticationDisabled;
	using ClientConfigurationData = Org.Apache.Pulsar.Client.Impl.Conf.ClientConfigurationData;
	using AsyncHttpClient = org.asynchttpclient.AsyncHttpClient;
	using ClientConfig = org.glassfish.jersey.client.ClientConfig;
	using ClientProperties = org.glassfish.jersey.client.ClientProperties;
	using JacksonFeature = org.glassfish.jersey.jackson.JacksonFeature;
	using MultiPartFeature = org.glassfish.jersey.media.multipart.MultiPartFeature;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
	using SLF4JBridgeHandler = org.slf4j.bridge.SLF4JBridgeHandler;

	/// <summary>
	/// Pulsar client admin API client.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public class PulsarAdmin implements java.io.Closeable
	public class PulsarAdmin : System.IDisposable
	{
		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(PulsarAdmin));

		public const int DefaultConnectTimeoutSeconds = 60;
		public const int DefaultReadTimeoutSeconds = 60;
		public const int DefaultRequestTimeoutSeconds = 300;

		private readonly Clusters clusters;
		private readonly Brokers brokers;
		private readonly BrokerStats brokerStats;
		private readonly Tenants tenants;
		private readonly Properties properties;
		private readonly Namespaces namespaces;
		private readonly Bookies bookies;
		private readonly TopicsImpl topics;
		private readonly NonPersistentTopics nonPersistentTopics;
		private readonly ResourceQuotas resourceQuotas;
		public virtual ClientConfigData {get;}
		private readonly Client client;
		private readonly AsyncHttpClient httpAsyncClient;
		public virtual ServiceUrl {get;}
		private readonly Lookup lookups;
		private readonly Functions functions;
		private readonly Sources sources;
		private readonly Sinks sinks;
		private readonly Worker worker;
		private readonly Schemas schemas;
		protected internal readonly WebTarget Root;
		protected internal readonly Authentication Auth;
		private readonly int connectTimeout;
		private readonly TimeUnit connectTimeoutUnit;
		private readonly int readTimeout;
		private readonly TimeUnit readTimeoutUnit;
		private readonly int requestTimeout;
		private readonly TimeUnit requestTimeoutUnit;

		static PulsarAdmin()
		{
			/// <summary>
			/// The presence of slf4j-jdk14.jar, that is the jul binding for SLF4J, will force SLF4J calls to be delegated to
			/// jul. On the other hand, the presence of jul-to-slf4j.jar, plus the installation of SLF4JBridgeHandler, by
			/// invoking "SLF4JBridgeHandler.install()" will route jul records to SLF4J. Thus, if both jar are present
			/// simultaneously (and SLF4JBridgeHandler is installed), slf4j calls will be delegated to jul and jul records
			/// will be routed to SLF4J, resulting in an endless loop. We avoid this loop by detecting if slf4j-jdk14 is used
			/// in the client class path. If slf4j-jdk14 is found, we don't use the slf4j bridge.
			/// </summary>
			try
			{
				Type.GetType("org.slf4j.impl.JDK14LoggerFactory");
			}
			catch (Exception)
			{
				// Setup the bridge for java.util.logging to SLF4J
				SLF4JBridgeHandler.removeHandlersForRootLogger();
				SLF4JBridgeHandler.install();
			}
		}

		/// <summary>
		/// Creates a builder to construct an instance of <seealso cref="PulsarAdmin"/>.
		/// </summary>
		public static PulsarAdminBuilder Builder()
		{
			return new PulsarAdminBuilderImpl();
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarAdmin(String serviceUrl, org.apache.pulsar.client.impl.conf.ClientConfigurationData clientConfigData) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarAdmin(string ServiceUrl, ClientConfigurationData ClientConfigData) : this(ServiceUrl, ClientConfigData, DefaultConnectTimeoutSeconds, TimeUnit.SECONDS, DefaultReadTimeoutSeconds, TimeUnit.SECONDS, DefaultRequestTimeoutSeconds, TimeUnit.SECONDS)
		{

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarAdmin(String serviceUrl, org.apache.pulsar.client.impl.conf.ClientConfigurationData clientConfigData, int connectTimeout, java.util.concurrent.TimeUnit connectTimeoutUnit, int readTimeout, java.util.concurrent.TimeUnit readTimeoutUnit, int requestTimeout, java.util.concurrent.TimeUnit requestTimeoutUnit) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarAdmin(string ServiceUrl, ClientConfigurationData ClientConfigData, int ConnectTimeout, TimeUnit ConnectTimeoutUnit, int ReadTimeout, TimeUnit ReadTimeoutUnit, int RequestTimeout, TimeUnit RequestTimeoutUnit)
		{
			this.connectTimeout = ConnectTimeout;
			this.connectTimeoutUnit = ConnectTimeoutUnit;
			this.readTimeout = ReadTimeout;
			this.readTimeoutUnit = ReadTimeoutUnit;
			this.requestTimeout = RequestTimeout;
			this.requestTimeoutUnit = RequestTimeoutUnit;
			this.ClientConfigData = ClientConfigData;
			this.Auth = ClientConfigData != null ? ClientConfigData.Authentication : new AuthenticationDisabled();
			LOG.debug("created: serviceUrl={}, authMethodName={}", ServiceUrl, Auth != null ? Auth.AuthMethodName : null);

			if (Auth != null)
			{
				Auth.start();
			}

			if (StringUtils.isBlank(ClientConfigData.ServiceUrl))
			{
				ClientConfigData.ServiceUrl = ServiceUrl;
			}

			AsyncHttpConnectorProvider AsyncConnectorProvider = new AsyncHttpConnectorProvider(ClientConfigData);

			ClientConfig HttpConfig = new ClientConfig();
			HttpConfig.property(ClientProperties.FOLLOW_REDIRECTS, true);
			HttpConfig.property(ClientProperties.ASYNC_THREADPOOL_SIZE, 8);
			HttpConfig.register(typeof(MultiPartFeature));
			HttpConfig.connectorProvider(AsyncConnectorProvider);

			ClientBuilder ClientBuilder = ClientBuilder.newBuilder().withConfig(HttpConfig).connectTimeout(this.connectTimeout, this.connectTimeoutUnit).readTimeout(this.readTimeout, this.readTimeoutUnit).register(typeof(JacksonConfigurator)).register(typeof(JacksonFeature));

			bool UseTls = ClientConfigData.ServiceUrl.StartsWith("https://");

			this.client = ClientBuilder.build();

			this.ServiceUrl = ServiceUrl;
			Root = client.target(ServiceUrl);

			this.httpAsyncClient = AsyncConnectorProvider.getConnector(Math.toIntExact(ConnectTimeoutUnit.toMillis(this.connectTimeout)), Math.toIntExact(ReadTimeoutUnit.toMillis(this.readTimeout)), Math.toIntExact(RequestTimeoutUnit.toMillis(this.requestTimeout))).HttpClient;

			long ReadTimeoutMs = ReadTimeoutUnit.toMillis(this.readTimeout);
			this.clusters = new ClustersImpl(Root, Auth, ReadTimeoutMs);
			this.brokers = new BrokersImpl(Root, Auth, ReadTimeoutMs);
			this.brokerStats = new BrokerStatsImpl(Root, Auth, ReadTimeoutMs);
			this.tenants = new TenantsImpl(Root, Auth, ReadTimeoutMs);
			this.properties = new TenantsImpl(Root, Auth, ReadTimeoutMs);
			this.namespaces = new NamespacesImpl(Root, Auth, ReadTimeoutMs);
			this.topics = new TopicsImpl(Root, Auth, ReadTimeoutMs);
			this.nonPersistentTopics = new NonPersistentTopicsImpl(Root, Auth, ReadTimeoutMs);
			this.resourceQuotas = new ResourceQuotasImpl(Root, Auth, ReadTimeoutMs);
			this.lookups = new LookupImpl(Root, Auth, UseTls, ReadTimeoutMs);
			this.functions = new FunctionsImpl(Root, Auth, httpAsyncClient, ReadTimeoutMs);
			this.sources = new SourcesImpl(Root, Auth, httpAsyncClient, ReadTimeoutMs);
			this.sinks = new SinksImpl(Root, Auth, httpAsyncClient, ReadTimeoutMs);
			this.worker = new WorkerImpl(Root, Auth, ReadTimeoutMs);
			this.schemas = new SchemasImpl(Root, Auth, ReadTimeoutMs);
			this.bookies = new BookiesImpl(Root, Auth, ReadTimeoutMs);
		}

		/// <summary>
		/// Construct a new Pulsar Admin client object.
		/// <para>
		/// This client object can be used to perform many subsquent API calls
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrl">
		///            the Pulsar service URL (eg. "http://my-broker.example.com:8080") </param>
		/// <param name="auth">
		///            the Authentication object to be used to talk with Pulsar </param>
		/// @deprecated Since 2.0. Use <seealso cref="builder()"/> to construct a new <seealso cref="PulsarAdmin"/> instance. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.") public PulsarAdmin(java.net.URL serviceUrl, org.apache.pulsar.client.api.Authentication auth) throws org.apache.pulsar.client.api.PulsarClientException
		[Obsolete(@"Since 2.0. Use <seealso cref=""builder()""/> to construct a new <seealso cref=""PulsarAdmin""/> instance.")]
		public PulsarAdmin(URL ServiceUrl, Authentication Auth) : this(ServiceUrl.ToString(), GetConfigData(Auth))
		{
		}

		private static ClientConfigurationData GetConfigData(Authentication Auth)
		{
			ClientConfigurationData Conf = new ClientConfigurationData();
			Conf.Authentication = Auth;
			return Conf;
		}

		/// <summary>
		/// Construct a new Pulsar Admin client object.
		/// <para>
		/// This client object can be used to perform many subsquent API calls
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrl">
		///            the Pulsar URL (eg. "http://my-broker.example.com:8080") </param>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParamsString">
		///            string which represents parameters for the Authentication-Plugin, e.g., "key1:val1,key2:val2" </param>
		/// @deprecated Since 2.0. Use <seealso cref="builder()"/> to construct a new <seealso cref="PulsarAdmin"/> instance. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.") public PulsarAdmin(java.net.URL serviceUrl, String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException
		[Obsolete(@"Since 2.0. Use <seealso cref=""builder()""/> to construct a new <seealso cref=""PulsarAdmin""/> instance.")]
		public PulsarAdmin(URL ServiceUrl, string AuthPluginClassName, string AuthParamsString) : this(ServiceUrl, AuthenticationFactory.create(AuthPluginClassName, AuthParamsString))
		{
		}

		/// <summary>
		/// Construct a new Pulsar Admin client object.
		/// <para>
		/// This client object can be used to perform many subsquent API calls
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrl">
		///            the Pulsar URL (eg. "http://my-broker.example.com:8080") </param>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParams">
		///            map which represents parameters for the Authentication-Plugin </param>
		/// @deprecated Since 2.0. Use <seealso cref="builder()"/> to construct a new <seealso cref="PulsarAdmin"/> instance. 
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.") public PulsarAdmin(java.net.URL serviceUrl, String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException
		[Obsolete(@"Since 2.0. Use <seealso cref=""builder()""/> to construct a new <seealso cref=""PulsarAdmin""/> instance.")]
		public PulsarAdmin(URL ServiceUrl, string AuthPluginClassName, IDictionary<string, string> AuthParams) : this(ServiceUrl, AuthenticationFactory.create(AuthPluginClassName, AuthParams))
		{
		}

		/// <returns> the clusters management object </returns>
		public virtual Clusters Clusters()
		{
			return clusters;
		}

		/// <returns> the brokers management object </returns>
		public virtual Brokers Brokers()
		{
			return brokers;
		}

		/// <returns> the tenants management object </returns>
		public virtual Tenants Tenants()
		{
			return tenants;
		}

		/// 
		/// @deprecated since 2.0. See <seealso cref="tenants()"/> 
		[Obsolete(@"since 2.0. See <seealso cref=""tenants()""/>")]
		public virtual Properties Properties()
		{
			return properties;
		}

		/// <returns> the namespaces management object </returns>
		public virtual Namespaces Namespaces()
		{
			return namespaces;
		}

		public virtual Topics Topics()
		{
			return topics;
		}

		/// <returns> the bookies management object </returns>
		public virtual Bookies Bookies()
		{
			return bookies;
		}

		/// <returns> the persistentTopics management object </returns>
		/// @deprecated Since 2.0. See <seealso cref="topics()"/> 
		[Obsolete(@"Since 2.0. See <seealso cref=""topics()""/>")]
		public virtual NonPersistentTopics NonPersistentTopics()
		{
			return nonPersistentTopics;
		}

		/// <returns> the resource quota management object </returns>
		public virtual ResourceQuotas ResourceQuotas()
		{
			return resourceQuotas;
		}

		/// <returns> does a looks up for the broker serving the topic </returns>
		public virtual Lookup Lookups()
		{
			return lookups;
		}

		/// 
		/// <returns> the functions management object </returns>
		public virtual Functions Functions()
		{
			return functions;
		}

		/// <returns> the sources management object </returns>
		/// @deprecated in favor of <seealso cref="sources()"/> 
		[Obsolete(@"in favor of <seealso cref=""sources()""/>")]
		public virtual Source Source()
		{
			return (Source) sources;
		}

		public virtual Sources Sources()
		{
			return sources;
		}

		/// <returns> the sinks management object </returns>
		/// @deprecated in favor of <seealso cref="sinks"/> 
		[Obsolete(@"in favor of <seealso cref=""sinks""/>")]
		public virtual Sink Sink()
		{
			return (Sink) sinks;
		}

		/// <returns> the sinks management object </returns>
		public virtual Sinks Sinks()
		{
			return sinks;
		}

		/// <returns> the Worker stats </returns>
	   public virtual Worker Worker()
	   {
		   return worker;
	   }

		/// <returns> the broker statics </returns>
		public virtual BrokerStats BrokerStats()
		{
			return brokerStats;
		}

		/// <returns> the service HTTP URL that is being used </returns>

		/// <returns> the client Configuration Data that is being used </returns>

		/// <returns> the schemas </returns>
		public virtual Schemas Schemas()
		{
			return schemas;
		}

		/// <summary>
		/// Close the Pulsar admin client to release all the resources
		/// </summary>
		public override void Close()
		{
			try
			{
				if (Auth != null)
				{
					Auth.Dispose();
				}
			}
			catch (IOException E)
			{
				LOG.error("Failed to close the authentication service", E);
			}
			client.close();

			try
			{
				httpAsyncClient.close();
			}
			catch (IOException E)
			{
			   LOG.error("Failed to close http async client", E);
			}
		}
	}

}