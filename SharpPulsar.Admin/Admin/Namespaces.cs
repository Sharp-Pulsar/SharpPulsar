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

	using ConflictException = PulsarAdminException.ConflictException;
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using AuthAction = pulsar.common.policies.data.AuthAction;
	using BacklogQuota = pulsar.common.policies.data.BacklogQuota;
	using BookieAffinityGroupData = pulsar.common.policies.data.BookieAffinityGroupData;
	using BundlesData = pulsar.common.policies.data.BundlesData;
	using DispatchRate = pulsar.common.policies.data.DispatchRate;
	using PersistencePolicies = pulsar.common.policies.data.PersistencePolicies;
	using Policies = pulsar.common.policies.data.Policies;
	using PublishRate = pulsar.common.policies.data.PublishRate;
	using RetentionPolicies = pulsar.common.policies.data.RetentionPolicies;
	using SchemaAutoUpdateCompatibilityStrategy = pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy;
	using SchemaCompatibilityStrategy = pulsar.common.policies.data.SchemaCompatibilityStrategy;
	using SubscribeRate = pulsar.common.policies.data.SubscribeRate;
	using SubscriptionAuthMode = pulsar.common.policies.data.SubscriptionAuthMode;

	/// <summary>
	/// Admin interface for namespaces management
	/// </summary>
	public interface Namespaces
	{
		/// <summary>
		/// Get the list of namespaces.
		/// <para>
		/// Get the list of all the namespaces for a certain tenant.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant/c1/namespace1",
		///  "my-tenant/global/namespace2",
		///  "my-tenant/c2/namespace3"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getNamespaces(String tenant) throws PulsarAdminException;
		IList<string> getNamespaces(string tenant);

		/// <summary>
		/// Get the list of namespaces.
		/// <para>
		/// Get the list of all the namespaces for a certain tenant on single cluster.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant/use/namespace1", "my-tenant/use/namespace2"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="cluster">
		///            Cluster name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated List<String> getNamespaces(String tenant, String cluster) throws PulsarAdminException;
		[Obsolete]
		IList<string> getNamespaces(string tenant, string cluster);

		/// <summary>
		/// Get the list of topics.
		/// <para>
		/// Get the list of all the topics under a certain namespace.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/use/namespace1/my-topic-1",
		///  "persistent://my-tenant/use/namespace1/my-topic-2"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getTopics(String namespace) throws PulsarAdminException;
		IList<string> getTopics(string @namespace);

		/// <summary>
		/// Get policies for a namespace.
		/// <para>
		/// Get the dump all the policies specified for a namespace.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>{
		///   "auth_policies" : {
		///     "namespace_auth" : {
		///       "my-role" : [ "produce" ]
		///     },
		///     "destination_auth" : {
		///       "persistent://prop/local/ns1/my-topic" : {
		///         "role-1" : [ "produce" ],
		///         "role-2" : [ "consume" ]
		///       }
		///     }
		///   },
		///   "replication_clusters" : ["use", "usw"],
		///   "message_ttl_in_seconds" : 300
		/// }</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref= Policies
		/// </seealso>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.Policies getPolicies(String namespace) throws PulsarAdminException;
		Policies getPolicies(string @namespace);

		/// <summary>
		/// Create a new namespace.
		/// <para>
		/// Creates a new empty namespace with no policies attached.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="numBundles">
		///            Number of bundles
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespace(String namespace, int numBundles) throws PulsarAdminException;
		void createNamespace(string @namespace, int numBundles);

		/// <summary>
		/// Create a new namespace.
		/// <para>
		/// Creates a new empty namespace with no policies attached.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundlesData">
		///            Bundles Data
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespace(String namespace, org.apache.pulsar.common.policies.data.BundlesData bundlesData) throws PulsarAdminException;
		void createNamespace(string @namespace, BundlesData bundlesData);

		/// <summary>
		/// Create a new namespace.
		/// <para>
		/// Creates a new empty namespace with no policies attached.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespace(String namespace) throws PulsarAdminException;
		void createNamespace(string @namespace);

		/// <summary>
		/// Create a new namespace.
		/// <para>
		/// Creates a new empty namespace with no policies attached.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="clusters">
		///            Clusters in which the namespace will be present. If more than one cluster is present, replication
		///            across clusters will be enabled.
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespace(String namespace, java.util.Set<String> clusters) throws PulsarAdminException;
		void createNamespace(string @namespace, ISet<string> clusters);

		/// <summary>
		/// Create a new namespace.
		/// <para>
		/// Creates a new namespace with the specified policies.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="policies">
		///            Policies for the namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or cluster does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace already exists </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		/// 
		/// @since 2.0 </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNamespace(String namespace, org.apache.pulsar.common.policies.data.Policies policies) throws PulsarAdminException;
		void createNamespace(string @namespace, Policies policies);

		/// <summary>
		/// Delete an existing namespace.
		/// <para>
		/// The namespace needs to be empty.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteNamespace(String namespace) throws PulsarAdminException;
		void deleteNamespace(string @namespace);

		/// <summary>
		/// Delete an existing bundle in a namespace.
		/// <para>
		/// The bundle needs to be empty.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundleRange">
		///            range of the bundle
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace/bundle does not exist </exception>
		/// <exception cref="ConflictException">
		///             Bundle is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteNamespaceBundle(String namespace, String bundleRange) throws PulsarAdminException;
		void deleteNamespaceBundle(string @namespace, string bundleRange);

		/// <summary>
		/// Delete an existing bundle in a namespace asynchronously.
		/// <para>
		/// The bundle needs to be empty.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundleRange">
		///            range of the bundle
		/// </param>
		/// <returns> a future that can be used to track when the bundle is deleted </returns>
		CompletableFuture<Void> deleteNamespaceBundleAsync(string @namespace, string bundleRange);

		/// <summary>
		/// Get permissions on a namespace.
		/// <para>
		/// Retrieve the permissions for a namespace.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>{
		///   "my-role" : [ "produce" ]
		///   "other-role" : [ "consume" ]
		/// }</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String namespace) throws PulsarAdminException;
		IDictionary<string, ISet<AuthAction>> getPermissions(string @namespace);

		/// <summary>
		/// Grant permission on a namespace.
		/// <para>
		/// Grant a new permission to a client role on a namespace.
		/// </para>
		/// <para>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>["produce", "consume"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="role">
		///            Client role to which grant permission </param>
		/// <param name="actions">
		///            Auth actions (produce and consume)
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void grantPermissionOnNamespace(String namespace, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws PulsarAdminException;
		void grantPermissionOnNamespace(string @namespace, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Revoke permissions on a namespace.
		/// <para>
		/// Revoke all permissions to a client role on a namespace.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="role">
		///            Client role to which remove permissions
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void revokePermissionsOnNamespace(String namespace, String role) throws PulsarAdminException;
		void revokePermissionsOnNamespace(string @namespace, string role);

		/// <summary>
		/// Grant permission to role to access subscription's admin-api. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="roles"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void grantPermissionOnSubscription(String namespace, String subscription, java.util.Set<String> roles) throws PulsarAdminException;
		void grantPermissionOnSubscription(string @namespace, string subscription, ISet<string> roles);

		/// <summary>
		/// Revoke permissions on a subscription's admin-api access. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="role"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void revokePermissionOnSubscription(String namespace, String subscription, String role) throws PulsarAdminException;
		void revokePermissionOnSubscription(string @namespace, string subscription, string role);

		/// <summary>
		/// Get the replication clusters for a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["use", "usw", "usc"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Namespace is not global </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getNamespaceReplicationClusters(String namespace) throws PulsarAdminException;
		IList<string> getNamespaceReplicationClusters(string @namespace);

		/// <summary>
		/// Set the replication clusters for a namespace.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>["us-west", "us-east", "us-cent"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="clusterIds">
		///            Pulsar Cluster Ids
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Namespace is not global </exception>
		/// <exception cref="PreconditionFailedException">
		///             Invalid cluster ids </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setNamespaceReplicationClusters(String namespace, java.util.Set<String> clusterIds) throws PulsarAdminException;
		void setNamespaceReplicationClusters(string @namespace, ISet<string> clusterIds);

		/// <summary>
		/// Get the message TTL for a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int getNamespaceMessageTTL(String namespace) throws PulsarAdminException;
		int getNamespaceMessageTTL(string @namespace);

		/// <summary>
		/// Set the messages Time to Live for all the topics within a namespace.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="ttlInSeconds">
		///            TTL values for all messages for all topics in this namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setNamespaceMessageTTL(String namespace, int ttlInSeconds) throws PulsarAdminException;
		void setNamespaceMessageTTL(string @namespace, int ttlInSeconds);


		/// <summary>
		/// Set anti-affinity group name for a namespace
		/// <para>
		/// Request example:
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="namespaceAntiAffinityGroup">
		///            anti-affinity group name for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setNamespaceAntiAffinityGroup(String namespace, String namespaceAntiAffinityGroup) throws PulsarAdminException;
		void setNamespaceAntiAffinityGroup(string @namespace, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Get all namespaces that grouped with given anti-affinity group
		/// </summary>
		/// <param name="tenant">
		///            tenant is only used for authorization. Client has to be admin of any of the tenant to access this
		///            api api. </param>
		/// <param name="cluster">
		///            cluster name </param>
		/// <param name="namespaceAntiAffinityGroup">
		///            Anti-affinity group name </param>
		/// <returns> list of namespace grouped under a given anti-affinity group </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getAntiAffinityNamespaces(String tenant, String cluster, String namespaceAntiAffinityGroup) throws PulsarAdminException;
		IList<string> getAntiAffinityNamespaces(string tenant, string cluster, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Get anti-affinity group name for a namespace
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String getNamespaceAntiAffinityGroup(String namespace) throws PulsarAdminException;
		string getNamespaceAntiAffinityGroup(string @namespace);

		/// <summary>
		/// Delete anti-affinity group name for a namespace.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteNamespaceAntiAffinityGroup(String namespace) throws PulsarAdminException;
		void deleteNamespaceAntiAffinityGroup(string @namespace);

		/// <summary>
		/// Set the deduplication status for all topics within a namespace.
		/// <para>
		/// When deduplication is enabled, the broker will prevent to store the same message multiple times.
		/// </para>
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="enableDeduplication">
		///            wether to enable or disable deduplication feature
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setDeduplicationStatus(String namespace, boolean enableDeduplication) throws PulsarAdminException;
		void setDeduplicationStatus(string @namespace, bool enableDeduplication);

		/// <summary>
		/// Get the bundles split data.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// 
		/// @HttpCode 200 Successfully retrieved
		/// @HttpCode 403 Don't have admin permission
		/// @HttpCode 404 Namespace does not exist
		/// @HttpCode 412 Namespace is not setup to split in bundles </param>
		// BundlesData getBundlesData(String namespace);

		/// <summary>
		/// Get backlog quota map on a namespace.
		/// <para>
		/// Get backlog quota map on a namespace.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "namespace_memory" : {
		///          "limit" : "134217728",
		///          "policy" : "consumer_backlog_eviction"
		///      },
		///      "destination_storage" : {
		///          "limit" : "-1",
		///          "policy" : "producer_exception"
		///      }
		///  }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Permission denied </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType, org.apache.pulsar.common.policies.data.BacklogQuota> getBacklogQuotaMap(String namespace) throws PulsarAdminException;
		IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> getBacklogQuotaMap(string @namespace);

		/// <summary>
		/// Set a backlog quota for all the topics on a namespace.
		/// <para>
		/// Set a backlog quota on a namespace.
		/// </para>
		/// <para>
		/// The backlog quota can be set on this resource:
		/// </para>
		/// <para>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "limit" : "134217728",
		///     "policy" : "consumer_backlog_eviction"
		/// }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="backlogQuota">
		///            the new BacklogQuota
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setBacklogQuota(String namespace, org.apache.pulsar.common.policies.data.BacklogQuota backlogQuota) throws PulsarAdminException;
		void setBacklogQuota(string @namespace, BacklogQuota backlogQuota);

		/// <summary>
		/// Remove a backlog quota policy from a namespace.
		/// <para>
		/// Remove a backlog quota policy from a namespace.
		/// </para>
		/// <para>
		/// The backlog retention policy will fall back to the default.
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void removeBacklogQuota(String namespace) throws PulsarAdminException;
		void removeBacklogQuota(string @namespace);

		/// <summary>
		/// Set the persistence configuration for all the topics on a namespace.
		/// <para>
		/// Set the persistence configuration on a namespace.
		/// </para>
		/// <para>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "bookkeeperEnsemble" : 3,                 // Number of bookies to use for a topic
		///     "bookkeeperWriteQuorum" : 2,              // How many writes to make of each entry
		///     "bookkeeperAckQuorum" : 2,                // Number of acks (guaranteed copies) to wait for each entry
		///     "managedLedgerMaxMarkDeleteRate" : 10.0,  // Throttling rate of mark-delete operation
		///                                               // to avoid high number of updates for each
		///                                               // consumer
		/// }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="persistence">
		///            Persistence policies object
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setPersistence(String namespace, org.apache.pulsar.common.policies.data.PersistencePolicies persistence) throws PulsarAdminException;
		void setPersistence(string @namespace, PersistencePolicies persistence);

		/// <summary>
		/// Get the persistence configuration for a namespace.
		/// <para>
		/// Get the persistence configuration for a namespace.
		/// </para>
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "bookkeeperEnsemble" : 3,                 // Number of bookies to use for a topic
		///     "bookkeeperWriteQuorum" : 2,              // How many writes to make of each entry
		///     "bookkeeperAckQuorum" : 2,                // Number of acks (guaranteed copies) to wait for each entry
		///     "managedLedgerMaxMarkDeleteRate" : 10.0,  // Throttling rate of mark-delete operation
		///                                               // to avoid high number of updates for each
		///                                               // consumer
		/// }
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="cluster">
		///            Cluster name </param>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistencePolicies getPersistence(String namespace) throws PulsarAdminException;
		PersistencePolicies getPersistence(string @namespace);

		/// <summary>
		/// Set bookie affinity group for a namespace to isolate namespace write to bookies that are part of given affinity
		/// group.
		/// </summary>
		/// <param name="namespace">
		///            namespace name </param>
		/// <param name="bookieAffinityGroup">
		///            bookie affinity group </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setBookieAffinityGroup(String namespace, org.apache.pulsar.common.policies.data.BookieAffinityGroupData bookieAffinityGroup) throws PulsarAdminException;
		void setBookieAffinityGroup(string @namespace, BookieAffinityGroupData bookieAffinityGroup);

		/// <summary>
		/// Delete bookie affinity group configured for a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteBookieAffinityGroup(String namespace) throws PulsarAdminException;
		void deleteBookieAffinityGroup(string @namespace);

		/// <summary>
		/// Get bookie affinity group configured for a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.BookieAffinityGroupData getBookieAffinityGroup(String namespace) throws PulsarAdminException;
		BookieAffinityGroupData getBookieAffinityGroup(string @namespace);

		/// <summary>
		/// Set the retention configuration for all the topics on a namespace.
		/// <p/>
		/// Set the retention configuration on a namespace. This operation requires Pulsar super-user access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setRetention(String namespace, org.apache.pulsar.common.policies.data.RetentionPolicies retention) throws PulsarAdminException;
		void setRetention(string @namespace, RetentionPolicies retention);

		/// <summary>
		/// Get the retention configuration for a namespace.
		/// <p/>
		/// Get the retention configuration for a namespace.
		/// <p/>
		/// Response example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.RetentionPolicies getRetention(String namespace) throws PulsarAdminException;
		RetentionPolicies getRetention(string @namespace);

		/// <summary>
		/// Unload a namespace from the current serving broker.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Namespace is already unloaded </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unload(String namespace) throws PulsarAdminException;
		void unload(string @namespace);

		/// <summary>
		/// Get the replication configuration version for a given namespace
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> Replication configuration version </returns>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String getReplicationConfigVersion(String namespace) throws PulsarAdminException;
		string getReplicationConfigVersion(string @namespace);

		/// <summary>
		/// Unload namespace bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		///           range of bundle to unload </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unloadNamespaceBundle(String namespace, String bundle) throws PulsarAdminException;
		void unloadNamespaceBundle(string @namespace, string bundle);

		/// <summary>
		/// Unload namespace bundle asynchronously
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		///           range of bundle to unload
		/// </param>
		/// <returns> a future that can be used to track when the bundle is unloaded </returns>
		CompletableFuture<Void> unloadNamespaceBundleAsync(string @namespace, string bundle);

		/// <summary>
		/// Split namespace bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="range"> of bundle to split </param>
		/// <param name="unload"> newly split bundles from the broker </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void splitNamespaceBundle(String namespace, String bundle, boolean unloadSplitBundles) throws PulsarAdminException;
		void splitNamespaceBundle(string @namespace, string bundle, bool unloadSplitBundles);

		/// <summary>
		/// Set message-publish-rate (topics under this namespace can publish this many messages per second)
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="publishMsgRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setPublishRate(String namespace, org.apache.pulsar.common.policies.data.PublishRate publishMsgRate) throws PulsarAdminException;
		void setPublishRate(string @namespace, PublishRate publishMsgRate);

		/// <summary>
		/// Get message-publish-rate (topics under this namespace can publish this many messages per second)
		/// </summary>
		/// <param name="namespace">
		/// @returns messageRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.PublishRate getPublishRate(String namespace) throws PulsarAdminException;
		PublishRate getPublishRate(string @namespace);

		/// <summary>
		/// Set message-dispatch-rate (topics under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void setDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Get message-dispatch-rate (topics under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace">
		/// @returns messageRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getDispatchRate(String namespace) throws PulsarAdminException;
		DispatchRate getDispatchRate(string @namespace);

		/// <summary>
		/// Set namespace-subscribe-rate (topics under this namespace will limit by subscribeRate)
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscribeRate">
		///            consumer subscribe limit by this subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setSubscribeRate(String namespace, org.apache.pulsar.common.policies.data.SubscribeRate subscribeRate) throws PulsarAdminException;
		void setSubscribeRate(string @namespace, SubscribeRate subscribeRate);

		/// <summary>
		/// Get namespace-subscribe-rate (topics under this namespace allow subscribe times per consumer in a period)
		/// </summary>
		/// <param name="namespace">
		/// @returns subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SubscribeRate getSubscribeRate(String namespace) throws PulsarAdminException;
		SubscribeRate getSubscribeRate(string @namespace);

		/// <summary>
		/// Set subscription-message-dispatch-rate (subscriptions under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setSubscriptionDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void setSubscriptionDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Get subscription-message-dispatch-rate (subscriptions under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String namespace) throws PulsarAdminException;
		DispatchRate getSubscriptionDispatchRate(string @namespace);

		/// <summary>
		/// Set replicator-message-dispatch-rate (Replicators under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setReplicatorDispatchRate(String namespace, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void setReplicatorDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Get replicator-message-dispatch-rate (Replicators under this namespace can dispatch this many messages per second)
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getReplicatorDispatchRate(String namespace) throws PulsarAdminException;
		DispatchRate getReplicatorDispatchRate(string @namespace);

		/// <summary>
		/// Clear backlog for all topics on a namespace
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearNamespaceBacklog(String namespace) throws PulsarAdminException;
		void clearNamespaceBacklog(string @namespace);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearNamespaceBacklogForSubscription(String namespace, String subscription) throws PulsarAdminException;
		void clearNamespaceBacklogForSubscription(string @namespace, string subscription);

		/// <summary>
		/// Clear backlog for all topics on a namespace bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearNamespaceBundleBacklog(String namespace, String bundle) throws PulsarAdminException;
		void clearNamespaceBundleBacklog(string @namespace, string bundle);

		/// <summary>
		/// Clear backlog for all topics on a namespace bundle asynchronously
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		/// </param>
		/// <returns> a future that can be used to track when the bundle is cleared </returns>
		CompletableFuture<Void> clearNamespaceBundleBacklogAsync(string @namespace, string bundle);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearNamespaceBundleBacklogForSubscription(String namespace, String bundle, String subscription) throws PulsarAdminException;
		void clearNamespaceBundleBacklogForSubscription(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace bundle asynchronously
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription">
		/// </param>
		/// <returns> a future that can be used to track when the bundle is cleared </returns>
		CompletableFuture<Void> clearNamespaceBundleBacklogForSubscriptionAsync(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Unsubscribes the given subscription on all topics on a namespace
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unsubscribeNamespace(String namespace, String subscription) throws PulsarAdminException;
		void unsubscribeNamespace(string @namespace, string subscription);

		/// <summary>
		/// Unsubscribes the given subscription on all topics on a namespace bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unsubscribeNamespaceBundle(String namespace, String bundle, String subscription) throws PulsarAdminException;
		void unsubscribeNamespaceBundle(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Unsubscribes the given subscription on all topics on a namespace bundle asynchronously
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription">
		/// </param>
		/// <returns> a future that can be used to track when the subscription is unsubscribed </returns>
		CompletableFuture<Void> unsubscribeNamespaceBundleAsync(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Set the encryption required status for all topics within a namespace.
		/// <para>
		/// When encryption required is true, the broker will prevent to store unencrypted messages.
		/// </para>
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="encryptionRequired">
		///            whether message encryption is required or not
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setEncryptionRequiredStatus(String namespace, boolean encryptionRequired) throws PulsarAdminException;
		void setEncryptionRequiredStatus(string @namespace, bool encryptionRequired);

		 /// <summary>
		 /// Set the given subscription auth mode on all topics on a namespace
		 /// </summary>
		 /// <param name="namespace"> </param>
		 /// <param name="subscriptionAuthMode"> </param>
		 /// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setSubscriptionAuthMode(String namespace, org.apache.pulsar.common.policies.data.SubscriptionAuthMode subscriptionAuthMode) throws PulsarAdminException;
		void setSubscriptionAuthMode(string @namespace, SubscriptionAuthMode subscriptionAuthMode);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int getMaxProducersPerTopic(String namespace) throws PulsarAdminException;
		int getMaxProducersPerTopic(string @namespace);

		/// <summary>
		/// Set maxProducersPerTopic for a namespace.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxProducersPerTopic">
		///            maxProducersPerTopic value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setMaxProducersPerTopic(String namespace, int maxProducersPerTopic) throws PulsarAdminException;
		void setMaxProducersPerTopic(string @namespace, int maxProducersPerTopic);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int getMaxConsumersPerTopic(String namespace) throws PulsarAdminException;
		int getMaxConsumersPerTopic(string @namespace);

		/// <summary>
		/// Set maxConsumersPerTopic for a namespace.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxConsumersPerTopic">
		///            maxConsumersPerTopic value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setMaxConsumersPerTopic(String namespace, int maxConsumersPerTopic) throws PulsarAdminException;
		void setMaxConsumersPerTopic(string @namespace, int maxConsumersPerTopic);

		/// <summary>
		/// Get the maxConsumersPerSubscription for a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int getMaxConsumersPerSubscription(String namespace) throws PulsarAdminException;
		int getMaxConsumersPerSubscription(string @namespace);

		/// <summary>
		/// Set maxConsumersPerSubscription for a namespace.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxConsumersPerSubscription">
		///            maxConsumersPerSubscription value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setMaxConsumersPerSubscription(String namespace, int maxConsumersPerSubscription) throws PulsarAdminException;
		void setMaxConsumersPerSubscription(string @namespace, int maxConsumersPerSubscription);

		/// <summary>
		/// Get the compactionThreshold for a namespace. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getCompactionThreshold(String namespace) throws PulsarAdminException;
		long getCompactionThreshold(string @namespace);

		/// <summary>
		/// Set the compactionThreshold for a namespace. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="compactionThreshold">
		///            maximum number of backlog bytes before compaction is triggered
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setCompactionThreshold(String namespace, long compactionThreshold) throws PulsarAdminException;
		void setCompactionThreshold(string @namespace, long compactionThreshold);

		/// <summary>
		/// Get the offloadThreshold for a namespace. The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// 
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long getOffloadThreshold(String namespace) throws PulsarAdminException;
		long getOffloadThreshold(string @namespace);

		/// <summary>
		/// Set the offloadThreshold for a namespace. The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// 
		/// Negative values disabled automatic offloading. Setting a threshold of 0 will offload data as soon as possible.
		/// <para>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="offloadThreshold">
		///            maximum number of bytes stored before offloading is triggered
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setOffloadThreshold(String namespace, long compactionThreshold) throws PulsarAdminException;
		void setOffloadThreshold(string @namespace, long compactionThreshold);

		/// <summary>
		/// Get the offload deletion lag for a namespace, in milliseconds.
		/// The number of milliseconds to wait before deleting a ledger segment which has been offloaded from
		/// the Pulsar cluster's local storage (i.e. BookKeeper).
		/// 
		/// If the offload deletion lag has not been set for the namespace, the method returns 'null'
		/// and the namespace will use the configured default of the pulsar broker.
		/// 
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// 
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>3600000</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> the offload deletion lag for the namespace in milliseconds, or null if not set
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: System.Nullable<long> getOffloadDeleteLagMs(String namespace) throws PulsarAdminException;
		long? getOffloadDeleteLagMs(string @namespace);

		/// <summary>
		/// Set the offload deletion lag for a namespace.
		/// 
		/// The offload deletion lag is the amount of time to wait after offloading a ledger segment to long term storage,
		/// before deleting its copy stored on the Pulsar cluster's local storage (i.e. BookKeeper).
		/// 
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="lag"> the duration to wait before deleting the local copy </param>
		/// <param name="unit"> the timeunit of the duration
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setOffloadDeleteLag(String namespace, long lag, java.util.concurrent.TimeUnit unit) throws PulsarAdminException;
		void setOffloadDeleteLag(string @namespace, long lag, TimeUnit unit);

		/// <summary>
		/// Clear the offload deletion lag for a namespace.
		/// 
		/// The namespace will fall back to using the configured default of the pulsar broker.
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void clearOffloadDeleteLag(String namespace) throws PulsarAdminException;
		void clearOffloadDeleteLag(string @namespace);

        /// <summary>
        /// Get the strategy used to check the a new schema provided by a producer is compatible with the current schema
        /// before it is installed.
        /// 
        /// <para>If this is
        /// <seealso cref="pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy.AutoUpdateDisabled"/>,
        /// then all new schemas provided via the producer are rejected, and schemas must be updated through the REST api.
        /// 
        /// </para>
        /// </summary>
        /// <param name="namespace"> The namespace in whose policy we are interested </param>
        /// <returns> the strategy used to check compatibility </returns>
        /// <exception cref="NotAuthorizedException">
        ///             Don't have admin permission </exception>
        /// <exception cref="NotFoundException">
        ///             Namespace does not exist </exception>
        /// <exception cref="PulsarAdminException">
        ///             Unexpected error </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: @Deprecated SchemaAutoUpdateCompatibilityStrategy getSchemaAutoUpdateCompatibilityStrategy(String namespace) throws PulsarAdminException;
        [Obsolete]
		SchemaAutoUpdateCompatibilityStrategy getSchemaAutoUpdateCompatibilityStrategy(string @namespace);

        /// <summary>
        /// Set the strategy used to check the a new schema provided by a producer is compatible with the current schema
        /// before it is installed.
        /// 
        /// <para>To disable all new schema updates through the producer, set this to
        /// <seealso cref="pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy.AutoUpdateDisabled"/>.
        /// 
        /// </para>
        /// </summary>
        /// <param name="namespace"> The namespace in whose policy should be set </param>
        /// <param name="autoUpdate"> true if connecting producers can automatically update the schema, false otherwise </param>
        /// <exception cref="NotAuthorizedException">
        ///             Don't have admin permission </exception>
        /// <exception cref="NotFoundException">
        ///             Namespace does not exist </exception>
        /// <exception cref="PulsarAdminException">
        ///             Unexpected error </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: @Deprecated void setSchemaAutoUpdateCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaAutoUpdateCompatibilityStrategy strategy) throws PulsarAdminException;
        [Obsolete]
		void setSchemaAutoUpdateCompatibilityStrategy(string @namespace, SchemaAutoUpdateCompatibilityStrategy strategy);

		/// <summary>
		/// Get schema validation enforced for namespace. </summary>
		/// <returns> the schema validation enforced flag </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean getSchemaValidationEnforced(String namespace) throws PulsarAdminException;
		bool getSchemaValidationEnforced(string @namespace);
		/// <summary>
		/// Set schema validation enforced for namespace.
		/// if a producer without a schema attempts to produce to a topic with schema in this the namespace, the
		/// producer will be failed to connect. PLEASE be carefully on using this, since non-java clients don't
		/// support schema. if you enable this setting, it will cause non-java clients failed to produce.
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <param name="schemaValidationEnforced"> flag to enable or disable schema validation for the given namespace </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setSchemaValidationEnforced(String namespace, boolean schemaValidationEnforced) throws PulsarAdminException;
		void setSchemaValidationEnforced(string @namespace, bool schemaValidationEnforced);

		/// <summary>
		/// Get the strategy used to check the a new schema provided by a producer is compatible with the current schema
		/// before it is installed.
		/// </summary>
		/// <param name="namespace"> The namespace in whose policy we are interested </param>
		/// <returns> the strategy used to check compatibility </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(String namespace) throws PulsarAdminException;
		SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(string @namespace);

		/// <summary>
		/// Set the strategy used to check the a new schema provided by a producer is compatible with the current schema
		/// before it is installed.
		/// </summary>
		/// <param name="namespace"> The namespace in whose policy should be set </param>
		/// <param name="strategy"> The schema compatibility strategy </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setSchemaCompatibilityStrategy(String namespace, org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy strategy) throws PulsarAdminException;
		void setSchemaCompatibilityStrategy(string @namespace, SchemaCompatibilityStrategy strategy);

		/// <summary>
		/// Get whether allow auto update schema
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <returns> the schema validation enforced flag </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean getIsAllowAutoUpdateSchema(String namespace) throws PulsarAdminException;
		bool getIsAllowAutoUpdateSchema(string @namespace);

		/// <summary>
		/// The flag is when producer bring a new schema and the schema pass compatibility check
		/// whether allow schema auto registered
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <param name="isAllowAutoUpdateSchema"> flag to enable or disable auto update schema </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setIsAllowAutoUpdateSchema(String namespace, boolean isAllowAutoUpdateSchema) throws PulsarAdminException;
		void setIsAllowAutoUpdateSchema(string @namespace, bool isAllowAutoUpdateSchema);
	}

}