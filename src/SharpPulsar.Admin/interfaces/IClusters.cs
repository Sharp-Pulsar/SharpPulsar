
using System.Collections.Generic;
using System.Threading;
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
namespace SharpPulsar.Admin.interfaces
{

    /// <summary>
    /// Admin interface for clusters management.
    /// </summary>
    public interface IClusters
    {
        /// <summary>
        /// Get the list of clusters.
        /// <p/>
        /// Get the list of all the Pulsar clusters.
        /// <p/>
        /// Response Example:
        /// 
        /// <pre>
        /// <code>["c1", "c2", "c3"]</code>
        /// </pre>
        /// </summary>
        IList<string> GetClusters(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the list of clusters asynchronously.
        /// <p/>
        /// Get the list of all the Pulsar clusters.
        /// <p/>
        /// Response Example:
        /// 
        /// <pre>
        /// <code>["c1", "c2", "c3"]</code>
        /// </pre>
        /// 
        /// </summary>
        ValueTask<IList<string>> GetClustersAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the configuration data for the specified cluster.
        /// <p/>
        /// Response Example:
        /// 
        /// <pre>
        /// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
        /// </pre>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// </param>
        /// <returns> the cluster configuration
        /// </returns>
        ClusterData GetCluster(string cluster, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the configuration data for the specified cluster asynchronously.
        /// <p/>
        /// Response Example:
        /// 
        /// <pre>
        /// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
        /// </pre>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// </param>
        /// <returns> the cluster configuration
        ///  </returns>
        ValueTask<ClusterData> GetClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Create a new cluster.
        /// <p/>
        /// Provisions a new cluster. This operation requires Pulsar super-user privileges.
        /// <p/>
        /// The name cannot contain '/' characters.
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name </param>
        /// <param name="clusterData">
        ///            the cluster configuration object
        /// </param>
        string CreateCluster(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Create a new cluster asynchronously.
        /// <p/>
        /// Provisions a new cluster. This operation requires Pulsar super-user privileges.
        /// <p/>
        /// The name cannot contain '/' characters.
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name </param>
        /// <param name="clusterData">
        ///            the cluster configuration object
        ///  </param>
        ValueTask<string> CreateClusterAsync(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update the configuration for a cluster.
        /// <p/>
        /// This operation requires Pulsar super-user privileges.
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name </param>
        /// <param name="clusterData">
        ///            the cluster configuration object
        /// </param>
        string UpdateCluster(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Update the configuration for a cluster asynchronously.
        /// <p/>
        /// This operation requires Pulsar super-user privileges.
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name </param>
        /// <param name="clusterData">
        ///            the cluster configuration object
        ///  </param>
        ValueTask<string> UpdateClusterAsync(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

       
        ValueTask<string> SetPeerClusterNamesAsync(string cluster, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        IList<string> GetPeerCluster(string cluster, Dictionary<string, List<string>> customHeaders = null);
        ValueTask<IList<string>> GetPeerClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Delete an existing cluster.
        /// <p/>
        /// Delete a cluster
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// </param>
        string DeleteCluster(string cluster, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Delete an existing cluster asynchronously.
        /// <p/>
        /// Delete a cluster
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        ///  </param>
        ValueTask<string> DeleteClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the namespace isolation policies of a cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return </param>
        IDictionary<string, NamespaceIsolationData> GetNamespaceIsolationPolicies(string cluster, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the namespace isolation policies of a cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return </param>
        ValueTask<IDictionary<string, NamespaceIsolationData>> GetNamespaceIsolationPoliciesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        string SetNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData body, Dictionary<string, List<string>> customHeaders = null);

        ValueTask<string> SetNamespaceIsolationPolicyAsync(string cluster, string policyName, NamespaceIsolationData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
        /// <summary>
        /// Returns list of active brokers with namespace-isolation policies attached to it.
        /// </summary>
        /// <param name="cluster">
        /// @return </param>
        IList<BrokerNamespaceIsolationData> GetBrokersWithNamespaceIsolationPolicy(string cluster, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Returns list of active brokers with namespace-isolation policies attached to it asynchronously.
        /// </summary>
        /// <param name="cluster">
        /// @return </param>
        ValueTask<IList<BrokerNamespaceIsolationData>> GetBrokersWithNamespaceIsolationPolicyAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns active broker with namespace-isolation policies attached to it.
        /// </summary>
        /// <param name="cluster"> </param>
        /// <param name="broker"> the broker name in the form host:port.
        /// @return </param>
        BrokerNamespaceIsolationData GetBrokerWithNamespaceIsolationPolicy(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Returns active broker with namespace-isolation policies attached to it asynchronously.
        /// </summary>
        /// <param name="cluster"> </param>
        /// <param name="broker">
        /// @return </param>
        ValueTask<BrokerNamespaceIsolationData> GetBrokerWithNamespaceIsolationPolicyAsync(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete a namespace isolation policy for a cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="policyName">
        ///          Policy name
        /// 
        /// @return </param>
        string DeleteNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Delete a namespace isolation policy for a cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="policyName">
        ///          Policy name
        /// 
        /// @return </param>

        ValueTask<string> DeleteNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get a single namespace isolation policy for a cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="policyName">
        ///          Policy name
        /// </param>
        NamespaceIsolationData GetNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get a single namespace isolation policy for a cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="policyName">
        ///          Policy name
        ///  </param>
        ValueTask<NamespaceIsolationData> GetNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
 
        string SetFailureDomain(string cluster, string domainName, FailureDomain body, Dictionary<string, List<string>> customHeaders = null);
        ValueTask<string> SetFailureDomainAsync(string cluster, string domainName, FailureDomain body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete a domain in cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="domainName">
        ///          Domain name
        /// 
        /// @return </param>
        string DeleteFailureDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Delete a domain in cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///          Cluster name
        /// </param>
        /// <param name="domainName">
        ///          Domain name
        /// 
        /// @return
        ///  </param>
        ValueTask<string> DeleteFailureDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all registered domains in cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return </param>
        IDictionary<string, FailureDomain> GetFailureDomains(string cluster, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get all registered domains in cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return
        ///  </param>
        ValueTask<IDictionary<string, FailureDomain>> GetFailureDomainsAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get the domain registered into a cluster.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return </param>
        FailureDomain GetDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Get the domain registered into a cluster asynchronously.
        /// <p/>
        /// </summary>
        /// <param name="cluster">
        ///            Cluster name
        /// @return
        ///  </param>
        ValueTask<FailureDomain> GetDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

    }
}
