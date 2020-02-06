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

	using ConflictException = PulsarAdminException.ConflictException;
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using BrokerNamespaceIsolationData = pulsar.common.policies.data.BrokerNamespaceIsolationData;
	using ClusterData = pulsar.common.policies.data.ClusterData;
	using FailureDomain = pulsar.common.policies.data.FailureDomain;
	using NamespaceIsolationData = pulsar.common.policies.data.NamespaceIsolationData;

	/// <summary>
	/// Admin interface for clusters management.
	/// </summary>
	public interface Clusters
	{
		/// <summary>
		/// Get the list of clusters.
		/// <para>
		/// Get the list of all the Pulsar clusters.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["c1", "c2", "c3"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getClusters() throws PulsarAdminException;
		IList<string> getClusters();

		/// <summary>
		/// Get the configuration data for the specified cluster.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.ClusterData getCluster(String cluster) throws PulsarAdminException;
		ClusterData getCluster(string cluster);

		/// <summary>
		/// Create a new cluster.
		/// <para>
		/// Provisions a new cluster. This operation requires Pulsar super-user privileges.
		/// </para>
		/// <para>
		/// The name cannot contain '/' characters.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="clusterData">
		///            the cluster configuration object
		/// </param>
		/// <exception cref="NotAuthorized">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="ConflictException">
		///             Cluster already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws PulsarAdminException;
		void createCluster(string cluster, ClusterData clusterData);

		/// <summary>
		/// Update the configuration for a cluster.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateCluster(String cluster, org.apache.pulsar.common.policies.data.ClusterData clusterData) throws PulsarAdminException;
		void updateCluster(string cluster, ClusterData clusterData);

		/// <summary>
		/// Update peer cluster names.
		/// <para>
		/// This operation requires Pulsar super-user privileges.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updatePeerClusterNames(String cluster, java.util.LinkedHashSet<String> peerClusterNames) throws PulsarAdminException;
		void updatePeerClusterNames(string cluster, LinkedHashSet<string> peerClusterNames);

		/// <summary>
		/// Get peer-cluster names
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Set<String> getPeerClusterNames(String cluster) throws PulsarAdminException;
		ISet<string> getPeerClusterNames(string cluster);


		/// <summary>
		/// Delete an existing cluster
		/// <para>
		/// Delete a cluster
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteCluster(String cluster) throws PulsarAdminException;
		void deleteCluster(string cluster);

		/// <summary>
		/// Get the namespace isolation policies of a cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.NamespaceIsolationData> getNamespaceIsolationPolicies(String cluster) throws PulsarAdminException;
		IDictionary<string, NamespaceIsolationData> getNamespaceIsolationPolicies(string cluster);


		/// <summary>
		/// Create a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws PulsarAdminException;
		void createNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);


		/// <summary>
		/// Returns list of active brokers with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(String cluster) throws PulsarAdminException;
		IList<BrokerNamespaceIsolationData> getBrokersWithNamespaceIsolationPolicy(string cluster);

		/// <summary>
		/// Returns active broker with namespace-isolation policies attached to it.
		/// </summary>
		/// <param name="cluster"> </param>
		/// <param name="broker">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(String cluster, String broker) throws PulsarAdminException;
		BrokerNamespaceIsolationData getBrokerWithNamespaceIsolationPolicy(string cluster, string broker);


		/// <summary>
		/// Update a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateNamespaceIsolationPolicy(String cluster, String policyName, org.apache.pulsar.common.policies.data.NamespaceIsolationData namespaceIsolationData) throws PulsarAdminException;
		void updateNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData);


		/// <summary>
		/// Delete a namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteNamespaceIsolationPolicy(String cluster, String policyName) throws PulsarAdminException;
		void deleteNamespaceIsolationPolicy(string cluster, string policyName);

		/// <summary>
		/// Get a single namespace isolation policy for a cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.NamespaceIsolationData getNamespaceIsolationPolicy(String cluster, String policyName) throws PulsarAdminException;
		NamespaceIsolationData getNamespaceIsolationPolicy(string cluster, string policyName);

		/// <summary>
		/// Create a domain into cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="FailureDomain">
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws PulsarAdminException;
		void createFailureDomain(string cluster, string domainName, FailureDomain domain);


		/// <summary>
		/// Update a domain into cluster
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="cluster">
		///          Cluster name
		/// </param>
		/// <param name="domainName">
		///          domain name
		/// </param>
		/// <param name="FailureDomain">
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFailureDomain(String cluster, String domainName, org.apache.pulsar.common.policies.data.FailureDomain domain) throws PulsarAdminException;
		void updateFailureDomain(string cluster, string domainName, FailureDomain domain);


		/// <summary>
		/// Delete a domain in cluster
		/// <para>
		/// 
		/// </para>
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteFailureDomain(String cluster, String domainName) throws PulsarAdminException;
		void deleteFailureDomain(string cluster, string domainName);

		/// <summary>
		/// Get all registered domains in cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.FailureDomain> getFailureDomains(String cluster) throws PulsarAdminException;
		IDictionary<string, FailureDomain> getFailureDomains(string cluster);

		/// <summary>
		/// Get the domain registered into a cluster
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FailureDomain getFailureDomain(String cluster, String domainName) throws PulsarAdminException;
		FailureDomain getFailureDomain(string cluster, string domainName);

	}

}