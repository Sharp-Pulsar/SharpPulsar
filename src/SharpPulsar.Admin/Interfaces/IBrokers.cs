using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;

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

    /// <summary>
    /// Admin interface for brokers management.
    /// </summary>
    public interface IBrokers
	{
		/// <summary>
		/// Get the list of active brokers in the local cluster.
		/// <p/>
		/// Get the list of active brokers (web service addresses) in the local cluster.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["prod1-broker1.messaging.use.example.com:8080", "prod1-broker2.messaging.use.example.com:8080"
		/// * * "prod1-broker3.messaging.use.example.com:8080"]</code>
		/// </pre>
		/// </summary>
		/// <returns> a list of (host:port) </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the list of active brokers in the cluster </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> ActiveBrokers {get;}

		/// <summary>
		/// Get the list of active brokers in the local cluster asynchronously.
		/// <p/>
		/// Get the list of active brokers (web service addresses) in the local cluster.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["prod1-broker1.messaging.use.example.com:8080", "prod1-broker2.messaging.use.example.com:8080",
		/// "prod1-broker3.messaging.use.example.com:8080"]</code>
		/// </pre>
		/// </summary>
		/// <returns> a list of (host:port) </returns>
		ValueTask<IList<string>> ActiveBrokersAsync {get;}
		/// <summary>
		/// Get the list of active brokers in the cluster.
		/// <p/>
		/// Get the list of active brokers (web service addresses) in the cluster.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["prod1-broker1.messaging.use.example.com:8080", "prod1-broker2.messaging.use.example.com:8080"
		/// * * "prod1-broker3.messaging.use.example.com:8080"]</code>
		/// </pre>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <returns> a list of (host:port) </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the list of active brokers in the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> GetActiveBrokers(string cluster);

		/// <summary>
		/// Get the list of active brokers in the cluster asynchronously.
		/// <p/>
		/// Get the list of active brokers (web service addresses) in the cluster.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["prod1-broker1.messaging.use.example.com:8080", "prod1-broker2.messaging.use.example.com:8080",
		/// "prod1-broker3.messaging.use.example.com:8080"]</code>
		/// </pre>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <returns> a list of (host:port) </returns>
		ValueTask<IList<string>> GetActiveBrokersAsync(string cluster);

		/// <summary>
		/// Get the information of the leader broker.
		/// <p/>
		/// Get the information of the leader broker.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{serviceUrl:"prod1-broker1.messaging.use.example.com:8080"}</code>
		/// </pre>
		/// </summary>
		/// <returns> the information of the leader broker. </returns>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		BrokerInfo LeaderBroker {get;}

		/// <summary>
		/// Get the service url of the leader broker asynchronously.
		/// <p/>
		/// Get the service url of the leader broker.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{serviceUrl:"prod1-broker1.messaging.use.example.com:8080"}</code>
		/// </pre>
		/// </summary>
		/// <returns> the service url of the leader broker </returns>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ValueTask<BrokerInfo> LeaderBrokerAsync {get;}

		/// <summary>
		/// Get the map of owned namespaces and their status from a single broker in the cluster.
		/// <p/>
		/// The map is returned in a JSON object format below
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{"ns-1":{"broker_assignment":"shared","is_active":"true","is_controlled":"false"},
		/// "ns-2":{"broker_assignment":"primary","is_active":"true","is_controlled":"true"}}</code>
		/// </pre>
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="brokerUrl">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IDictionary<string, NamespaceOwnershipStatus> GetOwnedNamespaces(string cluster, string brokerUrl);

		/// <summary>
		/// Get the map of owned namespaces and their status from a single broker in the cluster asynchronously.
		/// <p/>
		/// The map is returned in a JSON object format below
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{"ns-1":{"broker_assignment":"shared","is_active":"true","is_controlled":"false"},
		/// "ns-2":{"broker_assignment":"primary","is_active":"true","is_controlled":"true"}}</code>
		/// </pre>
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="brokerUrl">
		/// @return </param>
		ValueTask<IDictionary<string, NamespaceOwnershipStatus>> GetOwnedNamespacesAsync(string cluster, string brokerUrl);

		/// <summary>
		/// Update a dynamic configuration value into ZooKeeper.
		/// <p/>
		/// It updates dynamic configuration value in to Zk that triggers watch on
		/// brokers and all brokers can update <seealso cref="ServiceConfiguration"/> value
		/// locally
		/// </summary>
		/// <param name="configName"> </param>
		/// <param name="configValue"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void UpdateDynamicConfiguration(string configName, string configValue);

		/// <summary>
		/// Update a dynamic configuration value into ZooKeeper asynchronously.
		/// <p/>
		/// It updates dynamic configuration value in to Zk that triggers watch on
		/// brokers and all brokers can update <seealso cref="ServiceConfiguration"/> value
		/// locally
		/// </summary>
		/// <param name="configName"> </param>
		/// <param name="configValue"> </param>
		ValueTask UpdateDynamicConfigurationAsync(string configName, string configValue);

		/// <summary>
		/// It deletes dynamic configuration value into ZooKeeper.
		/// <p/>
		/// It will not impact current value in broker but next time when
		/// broker restarts, it applies value from configuration file only.
		/// </summary>
		/// <param name="configName"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void DeleteDynamicConfiguration(string configName);

		/// <summary>
		/// It deletes dynamic configuration value into ZooKeeper asynchronously.
		/// <p/>
		/// It will not impact current value in broker but next time when
		/// broker restarts, it applies value from configuration file only.
		/// </summary>
		/// <param name="configName"> </param>
		ValueTask DeleteDynamicConfigurationAsync(string configName);

		/// <summary>
		/// Get list of updatable configuration name.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		IList<string> DynamicConfigurationNames {get;}

		/// <summary>
		/// Get list of updatable configuration name asynchronously.
		/// 
		/// @return
		/// </summary>
		ValueTask<IList<string>> DynamicConfigurationNamesAsync {get;}

		/// <summary>
		/// Get values of runtime configuration.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		IDictionary<string, string> RuntimeConfigurations {get;}

		/// <summary>
		/// Get values of runtime configuration asynchronously.
		/// 
		/// @return
		/// </summary>
		ValueTask<IDictionary<string, string>> RuntimeConfigurationsAsync {get;}

		/// <summary>
		/// Get values of all overridden dynamic-configs.
		/// 
		/// @return </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		IDictionary<string, string> AllDynamicConfigurations {get;}

		/// <summary>
		/// Get values of all overridden dynamic-configs asynchronously.
		/// 
		/// @return
		/// </summary>
		ValueTask<IDictionary<string, string>> AllDynamicConfigurationsAsync {get;}

		/// <summary>
		/// Get the internal configuration data.
		/// </summary>
		/// <returns> internal configuration data. </returns>
		InternalConfigurationData InternalConfigurationData {get;}

		/// <summary>
		/// Get the internal configuration data asynchronously.
		/// </summary>
		/// <returns> internal configuration data. </returns>
		ValueTask<InternalConfigurationData> InternalConfigurationDataAsync {get;}

		/// <summary>
		/// Manually trigger backlogQuotaCheck.
		/// </summary>
		/// <exception cref="PulsarAdminException"> </exception>
		void BacklogQuotaCheck();

		/// <summary>
		/// Manually trigger backlogQuotaCheck asynchronously.
		/// @return
		/// </summary>
		ValueTask BacklogQuotaCheckAsync();


		/// <summary>
		/// Run a healthcheck on the broker.
		/// </summary>
		/// <exception cref="PulsarAdminException"> if the healthcheck fails. </exception>
		void Healthcheck(TopicVersion topicVersion);

		/// <summary>
		/// Run a healthcheck on the broker asynchronously.
		/// </summary>
		ValueTask HealthcheckAsync(TopicVersion topicVersion);

		/// <summary>
		/// Shutdown current broker gracefully. </summary>
		/// <param name="maxConcurrentUnloadPerSec"> </param>
		/// <param name="forcedTerminateTopic">
		/// @return </param>
		ValueTask ShutDownBrokerGracefully(int maxConcurrentUnloadPerSec, bool forcedTerminateTopic);

		/// <summary>
		/// Get version of broker. </summary>
		/// <returns> version of broker. </returns>
		string Version {get;}
	}

}