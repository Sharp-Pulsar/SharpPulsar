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
namespace SharpPulsar.Admin.Interfaces
{
    public interface IPulsarAdmin : System.IDisposable
	{

		/// <summary>
		/// Get a new builder instance that can used to configure and build a <seealso cref="IPulsarAdmin"/> instance.
		/// </summary>
		/// <returns> the <seealso cref="IPulsarAdminBuilder"/>
		///  </returns>
		static IPulsarAdminBuilder Builder()
		{
			return DefaultImplementation.newAdminClientBuilder();
		}

		/// <returns> the clusters management object </returns>
		Clusters Clusters();

		/// <returns> the brokers management object </returns>
		Brokers Brokers();

		/// <returns> the tenants management object </returns>
		ITenants Tenants();

		/// <returns> the resourcegroups managements object </returns>
		IResourceGroups Resourcegroups();

		/// <returns> the namespaces management object </returns>
		Namespaces Namespaces();

		/// <returns> the topics management object </returns>
		ITopics Topics();

		/// <summary>
		/// Get the topic policies management object. </summary>
		/// <returns> the topic policies management object </returns>
		ITopicPolicies TopicPolicies();

		/// <summary>
		/// Get the local/global topic policies management object. </summary>
		/// <returns> the topic policies management object </returns>
		ITopicPolicies TopicPolicies(bool isGlobal);

		/// <returns> the bookies management object </returns>
		Bookies Bookies();

		
		/// <returns> the resource quota management object </returns>
		IResourceQuotas ResourceQuotas();

		/// <returns> does a looks up for the broker serving the topic </returns>
		Lookup Lookups();

		/// 
		/// <returns> the functions management object </returns>
		Functions Functions();

		
		/// <returns> the sources management object </returns>
		ISources Sources();

		/// <returns> the sinks management object </returns>
		ISinks Sinks();

		/// <returns> the Worker stats </returns>
		IWorker Worker();

		/// <returns> the broker statics </returns>
		BrokerStats BrokerStats();

		/// <returns> the proxy statics </returns>
		IProxyStats ProxyStats();

		/// <returns> the service HTTP URL that is being used </returns>
		string ServiceUrl {get;}

		/// <returns> the schemas </returns>
		ISchemas Schemas();

		
		/// 
		/// <returns> the transactions management object </returns>
		ITransactions Transactions();

		/// <summary>
		/// Close the PulsarAdminClient and release all the resources.
		/// 
		/// </summary>
		void Close();
	}

}