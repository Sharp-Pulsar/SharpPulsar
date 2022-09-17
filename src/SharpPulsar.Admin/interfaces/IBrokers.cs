using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin.interfaces
{
   

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

    /// <summary>
    /// Admin interface for brokers management.
    /// </summary>
    public interface IBrokers
    { 
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
        /// 
        IList<string> GetActiveBrokers(string cluster, Dictionary<string, List<string>> customHeaders = null);

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
        ValueTask<IList<string>> GetActiveBrokersAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string IsReady(Dictionary<string, List<string>> customHeaders = null);
        ValueTask<string> IsReadyAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
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
        BrokerInfo GetLeaderBroker(Dictionary<string, List<string>> customHeaders = null);

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
        ValueTask<BrokerInfo> GetLeaderBrokerAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

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
        IDictionary<string, NamespaceOwnershipStatus> GetOwnedNamespaces(string cluster, string brokerUrl, Dictionary<string, List<string>> customHeaders = null);

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
        ValueTask<IDictionary<string, NamespaceOwnershipStatus>> GetOwnedNamespacesAsync(string cluster, string brokerUrl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update a dynamic configuration value into ZooKeeper.
        /// <p/>
        /// It updates dynamic configuration value in to Zk that triggers watch on
        /// brokers and all brokers can update <seealso cref="ServiceConfiguration"/> value
        /// locally
        /// </summary>
        /// <param name="configName"> </param>
        /// <param name="configValue"> </param>
        string UpdateDynamicConfiguration(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Update a dynamic configuration value into ZooKeeper asynchronously.
        /// <p/>
        /// It updates dynamic configuration value in to Zk that triggers watch on
        /// brokers and all brokers can update <seealso cref="ServiceConfiguration"/> value
        /// locally
        /// </summary>
        /// <param name="configName"> </param>
        /// <param name="configValue"> </param>
        ValueTask<string> UpdateDynamicConfigurationAsync(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// It deletes dynamic configuration value into ZooKeeper.
        /// <p/>
        /// It will not impact current value in broker but next time when
        /// broker restarts, it applies value from configuration file only.
        /// </summary>
        /// <param name="configName"> </param>
        string DeleteDynamicConfiguration(string configName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// It deletes dynamic configuration value into ZooKeeper asynchronously.
        /// <p/>
        /// It will not impact current value in broker but next time when
        /// broker restarts, it applies value from configuration file only.
        /// </summary>
        /// <param name="configName"> </param>
        ValueTask<string> DeleteDynamicConfigurationAsync(string configName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get list of updatable configuration name.
        /// 
        /// @return </summary>
        IList<string> GetDynamicConfigurationNames(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get list of updatable configuration name asynchronously.
        /// 
        /// @return
        /// </summary>
        ValueTask<IList<string>> GetDynamicConfigurationNamesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get values of runtime configuration.
        /// 
        /// @return </summary>
        IDictionary<string, string> GetRuntimeConfigurations(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get values of runtime configuration asynchronously.
        /// 
        /// @return
        /// </summary>
        ValueTask<IDictionary<string, string>> GetRuntimeConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get values of all overridden dynamic-configs.
        /// 
        /// @return </summary>
        IDictionary<string, string> GetAllDynamicConfigurations(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get values of all overridden dynamic-configs asynchronously.
        /// 
        /// @return
        /// </summary>
        ValueTask<IDictionary<string, string>> GetAllDynamicConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the internal configuration data.
        /// </summary>
        /// <returns> internal configuration data. </returns>
        InternalConfigurationData GetInternalConfigurationData(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the internal configuration data asynchronously.
        /// </summary>
        /// <returns> internal configuration data. </returns>
        ValueTask<InternalConfigurationData> GetInternalConfigurationDataAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Manually trigger backlogQuotaCheck.
        /// </summary>
        string BacklogQuotaCheck(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Manually trigger backlogQuotaCheck asynchronously.
        /// @return
        /// </summary>
        ValueTask<string> BacklogQuotaCheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

       
        /// <summary>
        /// Run a healthcheck on the broker.
        /// </summary>
        string Healthcheck(TopicVersion? topicVersion, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Run a healthcheck on the broker asynchronously.
        /// </summary>
        ValueTask<string> HealthcheckAsync(TopicVersion? topicVersion, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Shutdown current broker gracefully. </summary>
        /// <param name="maxConcurrentUnloadPerSec"> </param>
        /// <param name="forcedTerminateTopic">
        /// @return </param>
        string ShutDownBrokerGracefully(int? maxConcurrentUnloadPerSec, bool? forcedTerminateTopic = true, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Shutdown current broker gracefully. </summary>
        /// <param name="maxConcurrentUnloadPerSec"> </param>
        /// <param name="forcedTerminateTopic">
        /// @return </param>
        ValueTask<string> ShutDownBrokerGracefullyAsync(int? maxConcurrentUnloadPerSec, bool? forcedTerminateTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get version of broker. </summary>
        /// <returns> version of broker. </returns>
        string Version(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get version of broker. </summary>
        /// <returns> version of broker. </returns>
        ValueTask<string> VersionAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
