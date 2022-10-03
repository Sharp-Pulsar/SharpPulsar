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
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> Clusters {get;}

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
		ValueTask<IList<string>> ClustersAsync {get;}

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ClusterData GetCluster(string cluster);

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
		ValueTask<ClusterData> GetClusterAsync(string cluster);

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="ConflictException">
		///             Cluster already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
    	void CreateCluster(string cluster, ClusterData clusterData);

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
		ValueTask CreateClusterAsync(string cluster, ClusterData clusterData);

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateCluster(string cluster, ClusterData clusterData);

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
		ValueTask UpdateClusterAsync(string cluster, ClusterData clusterData);

		/// <summary>
		/// Update peer cluster names.
		/// <p/>
		/// This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="peerClusterNames">
		///            list of peer cluster names
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
		void UpdatePeerClusterNames(string cluster, HashSet<string> peerClusterNames);

		/// <summary>
		/// Update peer cluster names asynchronously.
		/// <p/>
		/// This operation requires Pulsar super-user privileges.
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="peerClusterNames">
		///            list of peer cluster names
		///  </param>
		ValueTask UpdatePeerClusterNamesAsync(string cluster, HashSet<string> peerClusterNames);

		/// <summary>
		/// Get peer-cluster names.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Domain doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///   
		ISet<string> GetPeerClusterNames(string cluster);

		/// <summary>
		/// Get peer-cluster names asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return
		///  </param>
		ValueTask<ISet<string>> GetPeerClusterNamesAsync(string cluster);

		/// <summary>
		/// Delete an existing cluster.
		/// <p/>
		/// Delete a cluster
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Cluster does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteCluster(string cluster);

		/// <summary>
		/// Delete an existing cluster asynchronously.
		/// <p/>
		/// Delete a cluster
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		///  </param>
		ValueTask DeleteClusterAsync(string cluster);

		/// <summary>
		/// Get the namespace isolation policies of a cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Policies don't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IDictionary<string, NamespaceIsolationData> GetNamespaceIsolationPolicies(string cluster);

		/// <summary>
		/// Get the namespace isolation policies of a cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Policies don't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ValueTask<IDictionary<string, NamespaceIsolationData>> GetNamespaceIsolationPoliciesAsync(string cluster);

		/// <summary>
		/// Create a namespace isolation policy for a cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void CreateNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);

		/// <summary>
		/// Create a namespace isolation policy for a cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return </param>
		ValueTask CreateNamespaceIsolationPolicyAsync(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);

		/// <summary>
		/// Returns list of active brokers with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IList<BrokerNamespaceIsolationData> GetBrokersWithNamespaceIsolationPolicy(string cluster);

		/// <summary>
		/// Returns list of active brokers with namespace-isolation policies attached to it asynchronously.
		/// </summary>
		/// <param name="cluster">
		/// @return </param>
		ValueTask<IList<BrokerNamespaceIsolationData>> GetBrokersWithNamespaceIsolationPolicyAsync(string cluster);

		/// <summary>
		/// Returns active broker with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="broker"> the broker name in the form host:port.
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		BrokerNamespaceIsolationData GetBrokerWithNamespaceIsolationPolicy(string cluster, string broker);

		/// <summary>
		/// Returns active broker with namespace-isolation policies attached to it asynchronously.
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="broker">
		/// @return </param>
		ValueTask<BrokerNamespaceIsolationData> GetBrokerWithNamespaceIsolationPolicyAsync(string cluster, string broker);

		/// <summary>
		/// Update a namespace isolation policy for a cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);

		/// <summary>
		/// Update a namespace isolation policy for a cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="policyName">
		///          Policy name
		/// </param>
		/// <param name="namespaceIsolationData">
		///          Namespace isolation policy configuration
		/// 
		/// @return
		///  </param>
		ValueTask UpdateNamespaceIsolationPolicyAsync(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

		void DeleteNamespaceIsolationPolicy(string cluster, string policyName);

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

		ValueTask DeleteNamespaceIsolationPolicyAsync(string cluster, string policyName);

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Policy doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		NamespaceIsolationData GetNamespaceIsolationPolicy(string cluster, string policyName);

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
		ValueTask<NamespaceIsolationData> GetNamespaceIsolationPolicyAsync(string cluster, string policyName);

		/// <summary>
		/// Create a domain into cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="domain">
		///          Domain configurations
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="ConflictException">
		///             Broker already exist into other domain
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void CreateFailureDomain(string cluster, string domainName, FailureDomain domain);

		/// <summary>
		/// Create a domain into cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="domain">
		///          Domain configurations
		/// 
		/// @return
		///  </param>
		ValueTask CreateFailureDomainAsync(string cluster, string domainName, FailureDomain domain);

		/// <summary>
		/// Update a domain into cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="domain">
		///          Domain configurations
		/// 
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="ConflictException">
		///             Broker already exist into other domain
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
		void UpdateFailureDomain(string cluster, string domainName, FailureDomain domain);

		/// <summary>
		/// Update a domain into cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="domain">
		///          Domain configurations
		/// 
		/// @return
		///  </param>
		ValueTask UpdateFailureDomainAsync(string cluster, string domainName, FailureDomain domain);

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
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteFailureDomain(string cluster, string domainName);

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
		ValueTask DeleteFailureDomainAsync(string cluster, string domainName);

		/// <summary>
		/// Get all registered domains in cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Cluster don't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IDictionary<string, FailureDomain> GetFailureDomains(string cluster);

		/// <summary>
		/// Get all registered domains in cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return
		///  </param>
		ValueTask<IDictionary<string, FailureDomain>> GetFailureDomainsAsync(string cluster);

		/// <summary>
		/// Get the domain registered into a cluster.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster
		/// </exception>
		/// <exception cref="NotFoundException">
		///             Domain doesn't exist
		/// </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster doesn't exist
		/// </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
		FailureDomain GetFailureDomain(string cluster, string domainName);

		/// <summary>
		/// Get the domain registered into a cluster asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name
		/// @return
		///  </param>
		ValueTask<FailureDomain> GetFailureDomainAsync(string cluster, string domainName);

	}

}