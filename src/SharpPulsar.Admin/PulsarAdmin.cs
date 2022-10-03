using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;
using IdentityModel.Client;
using SharpPulsar.Admin.Interfaces;
using SharpPulsar.Auth;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces;
using SharpPulsar.Precondition;

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
namespace SharpPulsar.Admin
{
    // JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    // 	import static com.google.common.@base.Preconditions.checkArgument;
    using StringUtils = org.apache.commons.lang3.StringUtils;
    using Bookies = org.apache.pulsar.client.admin.Bookies;
    using BrokerStats = org.apache.pulsar.client.admin.BrokerStats;
    using Brokers = org.apache.pulsar.client.admin.Brokers;
    using Clusters = org.apache.pulsar.client.admin.Clusters;
    using Functions = org.apache.pulsar.client.admin.Functions;
    using Lookup = org.apache.pulsar.client.admin.Lookup;
    using Namespaces = org.apache.pulsar.client.admin.Namespaces;
    using Properties = org.apache.pulsar.client.admin.Properties;
    using ProxyStats = org.apache.pulsar.client.admin.ProxyStats;
    using PulsarAdmin = org.apache.pulsar.client.admin.PulsarAdmin;
    using ResourceGroups = org.apache.pulsar.client.admin.ResourceGroups;
    using ResourceQuotas = org.apache.pulsar.client.admin.ResourceQuotas;
    using Schemas = org.apache.pulsar.client.admin.Schemas;
    using Sink = org.apache.pulsar.client.admin.Sink;
    using Sinks = org.apache.pulsar.client.admin.Sinks;
    using Source = org.apache.pulsar.client.admin.Source;
    using Sources = org.apache.pulsar.client.admin.Sources;
    using Tenants = org.apache.pulsar.client.admin.Tenants;
    using TopicPolicies = org.apache.pulsar.client.admin.TopicPolicies;
    using Topics = org.apache.pulsar.client.admin.Topics;
    using Transactions = org.apache.pulsar.client.admin.Transactions;
    using Worker = org.apache.pulsar.client.admin.Worker;
    using AsyncHttpConnector = Org.Apache.Pulsar.Client.Admin.@internal.Http.AsyncHttpConnector;
    using AsyncHttpConnectorProvider = Org.Apache.Pulsar.Client.Admin.@internal.Http.AsyncHttpConnectorProvider;
    using Authentication = org.apache.pulsar.client.api.Authentication;
    using AuthenticationFactory = org.apache.pulsar.client.api.AuthenticationFactory;
    using ServiceURI = org.apache.pulsar.common.net.ServiceURI;
    using ClientConfig = org.glassfish.jersey.client.ClientConfig;
    using ClientProperties = org.glassfish.jersey.client.ClientProperties;
    using JacksonFeature = org.glassfish.jersey.jackson.JacksonFeature;
    using MultiPartFeature = org.glassfish.jersey.media.multipart.MultiPartFeature;
    using Logger = org.slf4j.Logger;
    using LoggerFactory = org.slf4j.LoggerFactory;

    /// <summary>
    /// Pulsar client admin API client.
    /// </summary>
    // JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    // ORIGINAL LINE: @SuppressWarnings("deprecation") public class PulsarAdmin implements org.apache.pulsar.client.admin.PulsarAdmin
    public class PulsarAdmin : IPulsarAdmin
    {
        private static readonly Logger lOG = LoggerFactory.getLogger(typeof(PulsarAdmin));

        public const int DefaultConnectTimeoutSeconds = 60;
        public const int DefaultReadTimeoutSeconds = 60;
        public const int DefaultRequestTimeoutSeconds = 300;
        public const int DefaultCertRefreshSeconds = 300;

        private readonly Clusters clusters;
        private readonly Brokers brokers;
        private readonly BrokerStats brokerStats;
        private readonly ProxyStats proxyStats;
        private readonly Tenants tenants;
        private readonly ResourceGroups resourcegroups;
        private readonly Properties properties;
        private readonly Namespaces namespaces;
        private readonly Bookies bookies;
        private readonly TopicsImpl topics;
        private readonly TopicPolicies localTopicPolicies;
        private readonly TopicPolicies globalTopicPolicies;
        private readonly ResourceQuotas resourceQuotas;
        private readonly ClientConfigurationData clientConfigData;
        private readonly Client client;
        private readonly AsyncHttpConnector asyncHttpConnector;
        private readonly string serviceUrl;
        private readonly Lookup lookups;
        private readonly Functions functions;
        private readonly Sources sources;
        private readonly Sinks sinks;
        private readonly Worker worker;
        private readonly Packages packages;
        private readonly Transactions transactions;
        protected internal readonly WebTarget Root;
        protected internal readonly IAuthentication Auth;
        private readonly int connectTimeout;
        private readonly TimeSpan connectTimeoutUnit;
        private readonly int readTimeout;
        private readonly TimeSpan readTimeoutUnit;
        private readonly int requestTimeout;
        private readonly TimeSpan requestTimeoutUnit;
        private HttpClient _httpClient;

        public PulsarAdmin(string serviceUrl, ClientConfigurationData clientConfigData, int connectTimeout, TimeSpan connectTimeoutUnit, int readTimeout, TimeSpan readTimeoutUnit, int requestTimeout, TimeSpan requestTimeoutUnit, int autoCertRefreshTime, TimeSpan autoCertRefreshTimeSpan)
        {
            Condition.CheckArgument(!string.IsNullOrWhiteSpace(serviceUrl), "Service URL needs to be specified");
            this.connectTimeout = connectTimeout;
            this.connectTimeoutUnit = connectTimeoutUnit;
            this.readTimeout = readTimeout;
            this.readTimeoutUnit = readTimeoutUnit;
            this.requestTimeout = requestTimeout;
            this.requestTimeoutUnit = requestTimeoutUnit;
            this.clientConfigData = clientConfigData;
            this.Auth = ClientConfigData != null ? clientConfigData.Authentication : new AuthenticationDisabled();
            lOG.debug("created: serviceUrl={}, authMethodName={}", ServiceUrl, Auth != null ? Auth.AuthMethodName : null);
            if (Auth != null)
            {
                Auth.Start();
            }

            if (clientConfigData != null && string.IsNullOrWhiteSpace(clientConfigData.ServiceUrl))
            {
                clientConfigData.ServiceUrl = serviceUrl;
            }
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true, 
                MaxConnectionsPerServer = 8
                
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "SharpPulsar-Admin");
            
            bool useTls = clientConfigData.ServiceUrl.StartsWith("https://");

            
            this.serviceUrl = serviceUrl;
            ServiceURI ServiceUri = ServiceURI.create(ServiceUrl);
            Root = client.target(ServiceUri.selectOne());

            this.asyncHttpConnector = AsyncConnectorProvider.GetConnector(Math.toIntExact(ConnectTimeoutUnit.toMillis(this.connectTimeout)), Math.toIntExact(ReadTimeoutUnit.toMillis(this.readTimeout)), Math.toIntExact(RequestTimeoutUnit.toMillis(this.requestTimeout)), (int)AutoCertRefreshTimeSpan.toSeconds(AutoCertRefreshTime));

            long ReadTimeoutMs = ReadTimeoutUnit.toMillis(this.readTimeout);
            this.clusters = new ClustersImpl(Root, Auth, ReadTimeoutMs);
            this.brokers = new BrokersImpl(Root, Auth, ReadTimeoutMs);
            this.brokerStats = new BrokerStatsImpl(Root, Auth, ReadTimeoutMs);
            this.proxyStats = new ProxyStatsImpl(Root, Auth, ReadTimeoutMs);
            this.tenants = new TenantsImpl(Root, Auth, ReadTimeoutMs);
            this.resourcegroups = new ResourceGroupsImpl(Root, Auth, ReadTimeoutMs);
            this.properties = new TenantsImpl(Root, Auth, ReadTimeoutMs);
            this.namespaces = new NamespacesImpl(Root, Auth, ReadTimeoutMs);
            this.topics = new TopicsImpl(Root, Auth, ReadTimeoutMs);
            this.localTopicPolicies = new TopicPoliciesImpl(Root, Auth, ReadTimeoutMs, false);
            this.globalTopicPolicies = new TopicPoliciesImpl(Root, Auth, ReadTimeoutMs, true);
            this.resourceQuotas = new ResourceQuotasImpl(Root, Auth, ReadTimeoutMs);
            this.lookups = new LookupImpl(Root, Auth, UseTls, ReadTimeoutMs, topics);
            this.functions = new FunctionsImpl(Root, Auth, asyncHttpConnector.getHttpClient(), ReadTimeoutMs);
            this.sources = new SourcesImpl(Root, Auth, asyncHttpConnector.getHttpClient(), ReadTimeoutMs);
            this.sinks = new SinksImpl(Root, Auth, asyncHttpConnector.getHttpClient(), ReadTimeoutMs);
            this.worker = new WorkerImpl(Root, Auth, ReadTimeoutMs);
            this.schemas = new SchemasImpl(Root, Auth, ReadTimeoutMs);
            this.bookies = new BookiesImpl(Root, Auth, ReadTimeoutMs);
            this.packages = new PackagesImpl(Root, Auth, asyncHttpConnector.getHttpClient(), ReadTimeoutMs);
            this.transactions = new TransactionsImpl(Root, Auth, ReadTimeoutMs);

        }


        private static ClientConfigurationData GetConfigData(Authentication auth)
        {
            ClientConfigurationData Conf = new ClientConfigurationData();
            Conf.setAuthentication(auth);
            return Conf;
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

        /// <returns> the resourcegroups management object </returns>
        public virtual ResourceGroups Resourcegroups()
        {
            return resourcegroups;
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

        public override TopicPolicies TopicPolicies()
        {
            return localTopicPolicies;
        }

        public override TopicPolicies TopicPolicies(bool IsGlobal)
        {
            return IsGlobal ? globalTopicPolicies : localTopicPolicies;
        }

        /// <returns> the bookies management object </returns>
        public virtual Bookies Bookies()
        {
            return bookies;
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


        public virtual Sources Sources()
        {
            return sources;
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

        /// <returns> the proxy statics </returns>
        public virtual ProxyStats ProxyStats()
        {
            return proxyStats;
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
        public virtual Schemas Schemas()
        {
            return schemas;
        }

       
        public override Transactions Transactions()
        {
            return transactions;
        }

        /// <summary>
        /// Close the Pulsar admin client to release all the resources.
        /// </summary>
        public override void close()
        {
            try
            {
                if (Auth != null)
                {
                    Auth.close();
                }
            }
            catch (IOException E)
            {
                lOG.error("Failed to close the authentication service", E);
            }
            client.close();

            asyncHttpConnector.close();
        }
    }

}