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
namespace org.apache.pulsar.client.admin
{


	using StringUtils = apache.commons.lang3.StringUtils;
	using BookiesImpl = @internal.BookiesImpl;
	using BrokerStatsImpl = @internal.BrokerStatsImpl;
	using BrokersImpl = @internal.BrokersImpl;
	using ClustersImpl = @internal.ClustersImpl;
	using FunctionsImpl = @internal.FunctionsImpl;
	using JacksonConfigurator = @internal.JacksonConfigurator;
	using LookupImpl = @internal.LookupImpl;
	using NamespacesImpl = @internal.NamespacesImpl;
	using NonPersistentTopicsImpl = @internal.NonPersistentTopicsImpl;
	using PulsarAdminBuilderImpl = @internal.PulsarAdminBuilderImpl;
	using ResourceQuotasImpl = @internal.ResourceQuotasImpl;
	using SchemasImpl = @internal.SchemasImpl;
	using SinksImpl = @internal.SinksImpl;
	using SourcesImpl = @internal.SourcesImpl;
	using TenantsImpl = @internal.TenantsImpl;
	using TopicsImpl = @internal.TopicsImpl;
	using WorkerImpl = @internal.WorkerImpl;
	using AsyncHttpConnectorProvider = @internal.http.AsyncHttpConnectorProvider;
	using Authentication = client.api.Authentication;
	using AuthenticationFactory = client.api.AuthenticationFactory;
	using PulsarClientException = client.api.PulsarClientException;
	using AuthenticationDisabled = client.impl.auth.AuthenticationDisabled;
	using ClientConfigurationData = client.impl.conf.ClientConfigurationData;
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
	public class PulsarAdmin : IDisposable
    {
		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(PulsarAdmin));

		public const int DEFAULT_CONNECT_TIMEOUT_SECONDS = 60;
		public const int DEFAULT_READ_TIMEOUT_SECONDS = 60;
		public const int DEFAULT_REQUEST_TIMEOUT_SECONDS = 300;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Clusters clusters_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Brokers brokers_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly BrokerStats brokerStats_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Tenants tenants_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Properties properties_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Namespaces namespaces_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Bookies bookies_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly TopicsImpl topics_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly NonPersistentTopics nonPersistentTopics_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly ResourceQuotas resourceQuotas_Conflict;
		private readonly ClientConfigurationData clientConfigData;
		private readonly Client client;
		private readonly AsyncHttpClient httpAsyncClient;
		private readonly string serviceUrl;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Lookup lookups_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Functions functions_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Sources sources_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Sinks sinks_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Worker worker_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Schemas schemas_Conflict;
		protected internal readonly WebTarget root;
		protected internal readonly Authentication auth;
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
		public static PulsarAdminBuilder builder()
		{
			return new PulsarAdminBuilderImpl();
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarAdmin(String serviceUrl, org.apache.pulsar.client.impl.conf.ClientConfigurationData clientConfigData) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarAdmin(string serviceUrl, ClientConfigurationData clientConfigData) : this(serviceUrl, clientConfigData, DEFAULT_CONNECT_TIMEOUT_SECONDS, TimeUnit.SECONDS, DEFAULT_READ_TIMEOUT_SECONDS, TimeUnit.SECONDS, DEFAULT_REQUEST_TIMEOUT_SECONDS, TimeUnit.SECONDS)
		{

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PulsarAdmin(String serviceUrl, org.apache.pulsar.client.impl.conf.ClientConfigurationData clientConfigData, int connectTimeout, java.util.concurrent.TimeUnit connectTimeoutUnit, int readTimeout, java.util.concurrent.TimeUnit readTimeoutUnit, int requestTimeout, java.util.concurrent.TimeUnit requestTimeoutUnit) throws org.apache.pulsar.client.api.PulsarClientException
		public PulsarAdmin(string serviceUrl, ClientConfigurationData clientConfigData, int connectTimeout, TimeUnit connectTimeoutUnit, int readTimeout, TimeUnit readTimeoutUnit, int requestTimeout, TimeUnit requestTimeoutUnit)
		{
			this.connectTimeout = connectTimeout;
			this.connectTimeoutUnit = connectTimeoutUnit;
			this.readTimeout = readTimeout;
			this.readTimeoutUnit = readTimeoutUnit;
			this.requestTimeout = requestTimeout;
			this.requestTimeoutUnit = requestTimeoutUnit;
			this.clientConfigData = clientConfigData;
			this.auth = clientConfigData != null ? clientConfigData.Authentication : new AuthenticationDisabled();
			LOG.debug("created: serviceUrl={}, authMethodName={}", serviceUrl, auth != null ? auth.AuthMethodName : null);

			if (auth != null)
			{
				auth.start();
			}

			if (StringUtils.isBlank(clientConfigData.ServiceUrl))
			{
				clientConfigData.ServiceUrl = serviceUrl;
			}

			AsyncHttpConnectorProvider asyncConnectorProvider = new AsyncHttpConnectorProvider(clientConfigData);

			ClientConfig httpConfig = new ClientConfig();
			httpConfig.property(ClientProperties.FOLLOW_REDIRECTS, true);
			httpConfig.property(ClientProperties.ASYNC_THREADPOOL_SIZE, 8);
			httpConfig.register(typeof(MultiPartFeature));
			httpConfig.connectorProvider(asyncConnectorProvider);

			ClientBuilder clientBuilder = ClientBuilder.newBuilder().withConfig(httpConfig).connectTimeout(this.connectTimeout, this.connectTimeoutUnit).readTimeout(this.readTimeout, this.readTimeoutUnit).register(typeof(JacksonConfigurator)).register(typeof(JacksonFeature));

			bool useTls = clientConfigData.ServiceUrl.StartsWith("https://");

			this.client = clientBuilder.build();

			this.serviceUrl = serviceUrl;
			root = client.target(serviceUrl);

			this.httpAsyncClient = asyncConnectorProvider.getConnector(Math.toIntExact(connectTimeoutUnit.toMillis(this.connectTimeout)), Math.toIntExact(readTimeoutUnit.toMillis(this.readTimeout)), Math.toIntExact(requestTimeoutUnit.toMillis(this.requestTimeout))).HttpClient;

			long readTimeoutMs = readTimeoutUnit.toMillis(this.readTimeout);
			this.clusters_Conflict = new ClustersImpl(root, auth, readTimeoutMs);
			this.brokers_Conflict = new BrokersImpl(root, auth, readTimeoutMs);
			this.brokerStats_Conflict = new BrokerStatsImpl(root, auth, readTimeoutMs);
			this.tenants_Conflict = new TenantsImpl(root, auth, readTimeoutMs);
			this.properties_Conflict = new TenantsImpl(root, auth, readTimeoutMs);
			this.namespaces_Conflict = new NamespacesImpl(root, auth, readTimeoutMs);
			this.topics_Conflict = new TopicsImpl(root, auth, readTimeoutMs);
			this.nonPersistentTopics_Conflict = new NonPersistentTopicsImpl(root, auth, readTimeoutMs);
			this.resourceQuotas_Conflict = new ResourceQuotasImpl(root, auth, readTimeoutMs);
			this.lookups_Conflict = new LookupImpl(root, auth, useTls, readTimeoutMs);
			this.functions_Conflict = new FunctionsImpl(root, auth, httpAsyncClient, readTimeoutMs);
			this.sources_Conflict = new SourcesImpl(root, auth, httpAsyncClient, readTimeoutMs);
			this.sinks_Conflict = new SinksImpl(root, auth, httpAsyncClient, readTimeoutMs);
			this.worker_Conflict = new WorkerImpl(root, auth, readTimeoutMs);
			this.schemas_Conflict = new SchemasImpl(root, auth, readTimeoutMs);
			this.bookies_Conflict = new BookiesImpl(root, auth, readTimeoutMs);
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
		[Obsolete("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.")]
		public PulsarAdmin(URL serviceUrl, Authentication auth) : this(serviceUrl.ToString(), getConfigData(auth))
		{
		}

		private static ClientConfigurationData getConfigData(Authentication auth)
		{
			ClientConfigurationData conf = new ClientConfigurationData();
			conf.Authentication = auth;
			return conf;
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
		[Obsolete("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.")]
		public PulsarAdmin(URL serviceUrl, string authPluginClassName, string authParamsString) : this(serviceUrl, AuthenticationFactory.create(authPluginClassName, authParamsString))
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
		[Obsolete("Since 2.0. Use <seealso cref=\"builder()\"/> to construct a new <seealso cref=\"PulsarAdmin\"/> instance.")]
		public PulsarAdmin(URL serviceUrl, string authPluginClassName, IDictionary<string, string> authParams) : this(serviceUrl, AuthenticationFactory.create(authPluginClassName, authParams))
		{
		}

		/// <returns> the clusters management object </returns>
		public virtual Clusters clusters()
		{
			return clusters_Conflict;
		}

		/// <returns> the brokers management object </returns>
		public virtual Brokers brokers()
		{
			return brokers_Conflict;
		}

		/// <returns> the tenants management object </returns>
		public virtual Tenants tenants()
		{
			return tenants_Conflict;
		}

		/// 
		/// @deprecated since 2.0. See <seealso cref="tenants()"/> 
		[Obsolete("since 2.0. See <seealso cref=\"tenants()\"/>")]
		public virtual Properties properties()
		{
			return properties_Conflict;
		}

		/// <returns> the namespaces management object </returns>
		public virtual Namespaces namespaces()
		{
			return namespaces_Conflict;
		}

		public virtual Topics topics()
		{
			return topics_Conflict;
		}

		/// <returns> the bookies management object </returns>
		public virtual Bookies bookies()
		{
			return bookies_Conflict;
		}

		/// <returns> the persistentTopics management object </returns>
		/// @deprecated Since 2.0. See <seealso cref="topics()"/> 
		[Obsolete("Since 2.0. See <seealso cref=\"topics()\"/>")]
		public virtual NonPersistentTopics nonPersistentTopics()
		{
			return nonPersistentTopics_Conflict;
		}

		/// <returns> the resource quota management object </returns>
		public virtual ResourceQuotas resourceQuotas()
		{
			return resourceQuotas_Conflict;
		}

		/// <returns> does a looks up for the broker serving the topic </returns>
		public virtual Lookup lookups()
		{
			return lookups_Conflict;
		}

		/// 
		/// <returns> the functions management object </returns>
		public virtual Functions functions()
		{
			return functions_Conflict;
		}

		/// <returns> the sources management object </returns>
		/// @deprecated in favor of <seealso cref="sources()"/> 
		[Obsolete("in favor of <seealso cref=\"sources()\"/>")]
		public virtual Source source()
		{
			return (Source) sources_Conflict;
		}

		public virtual Sources sources()
		{
			return sources_Conflict;
		}

		/// <returns> the sinks management object </returns>
		/// @deprecated in favor of <seealso cref="sinks"/> 
		[Obsolete("in favor of <seealso cref=\"sinks\"/>")]
		public virtual Sink sink()
		{
			return (Sink) sinks_Conflict;
		}

		/// <returns> the sinks management object </returns>
		public virtual Sinks sinks()
		{
			return sinks_Conflict;
		}

		/// <returns> the Worker stats </returns>
	   public virtual Worker worker()
	   {
		   return worker_Conflict;
	   }

		/// <returns> the broker statics </returns>
		public virtual BrokerStats brokerStats()
		{
			return brokerStats_Conflict;
		}

		/// <returns> the service HTTP URL that is being used </returns>
		public virtual string ServiceUrl
		{
			get
			{
				return serviceUrl;
			}
		}

		/// <returns> the client Configuration Data that is being used </returns>
		public virtual ClientConfigurationData ClientConfigData
		{
			get
			{
				return clientConfigData;
			}
		}

		/// <returns> the schemas </returns>
		public virtual Schemas schemas()
		{
			return schemas_Conflict;
		}

		/// <summary>
		/// Close the Pulsar admin client to release all the resources
		/// </summary>
		public virtual void Dispose()
		{
			try
			{
				if (auth != null)
				{
					auth.close();
				}
			}
			catch (IOException e)
			{
				LOG.error("Failed to close the authentication service", e);
			}
			client.close();

			try
			{
				httpAsyncClient.close();
			}
			catch (IOException e)
			{
			   LOG.error("Failed to close http async client", e);
			}
		}
	}

}