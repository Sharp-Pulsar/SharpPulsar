using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using SharpPulsar.Common.Enum;

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
    /// Admin interface for namespaces management.
    /// </summary>
    public interface INamespaces
	{
		/// <summary>
		/// Get the list of namespaces.
		/// <p/>
		/// Get the list of all the namespaces for a certain tenant.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant/c1/namespace1",
		///  "my-tenant/global/namespace2",
		///  "my-tenant/c2/namespace3"]</code>
		/// </pre>
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
		IList<string> GetNamespaces(string tenant);

		/// <summary>
		/// Get the list of namespaces asynchronously.
		/// <p/>
		/// Get the list of all the namespaces for a certain tenant.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["my-tenant/c1/namespace1",
		///  "my-tenant/global/namespace2",
		///  "my-tenant/c2/namespace3"]</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		ValueTask<IList<string>> GetNamespacesAsync(string tenant);


		/// <summary>
		/// Get the list of topics.
		/// <p/>
		/// Get the list of all the topics under a certain namespace.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/use/namespace1/my-topic-1",
		///  "persistent://my-tenant/use/namespace1/my-topic-2"]</code>
		/// </pre>
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
		IList<string> GetTopics(string @namespace);

		/// <summary>
		/// Get the list of topics asynchronously.
		/// <p/>
		/// Get the list of all the topics under a certain namespace.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/use/namespace1/my-topic-1",
		///  "persistent://my-tenant/use/namespace1/my-topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<IList<string>> GetTopicsAsync(string @namespace);

		/// <summary>
		/// Get the list of topics.
		/// <p/>
		/// Get the list of all the topics under a certain namespace.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/use/namespace1/my-topic-1",
		///  "persistent://my-tenant/use/namespace1/my-topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="options">
		///            List namespace topics options
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> GetTopics(string @namespace, ListNamespaceTopicsOptions options);

		/// <summary>
		/// Get the list of topics asynchronously.
		/// <p/>
		/// Get the list of all the topics under a certain namespace.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/use/namespace1/my-topic-1",
		///  "persistent://my-tenant/use/namespace1/my-topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="options">
		///            List namespace topics options </param>
		ValueTask<IList<string>> GetTopicsAsync(string @namespace, ListNamespaceTopicsOptions options);

		/// <summary>
		/// Get the list of bundles.
		/// <p/>
		/// Get the list of all the bundles under a certain namespace.
		/// <p/>
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
		BundlesData GetBundles(string @namespace);

		/// <summary>
		/// Get the list of bundles asynchronously.
		/// <p/>
		/// Get the list of all the bundles under a certain namespace.
		/// <p/>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<BundlesData> GetBundlesAsync(string @namespace);

		/// <summary>
		/// Get policies for a namespace.
		/// <p/>
		/// Get the dump all the policies specified for a namespace.
		/// <p/>
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
		/// </summary>
		/// <seealso cref="Policies"
		/// />
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		Policies GetPolicies(string @namespace);

		/// <summary>
		/// Get policies for a namespace asynchronously.
		/// <p/>
		/// Get the dump all the policies specified for a namespace.
		/// <p/>
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
		/// </summary>
		/// <seealso cref="Policies"
		/// />
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<Policies> GetPoliciesAsync(string @namespace);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
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
		void CreateNamespace(string @namespace, int numBundles);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="numBundles">
		///            Number of bundles </param>
		ValueTask CreateNamespaceAsync(string @namespace, int numBundles);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
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
		void CreateNamespace(string @namespace, BundlesData bundlesData);

		/// <summary>
		/// Create a new namespace asynchronously.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundlesData">
		///            Bundles Data </param>
		ValueTask CreateNamespaceAsync(string @namespace, BundlesData bundlesData);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
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
		void CreateNamespace(string @namespace);

		/// <summary>
		/// Create a new namespace asynchronously.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask CreateNamespaceAsync(string @namespace);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
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
		void CreateNamespace(string @namespace, ISet<string> clusters);

		/// <summary>
		/// Create a new namespace asynchronously.
		/// <p/>
		/// Creates a new empty namespace with no policies attached.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="clusters">
		///            Clusters in which the namespace will be present. If more than one cluster is present, replication
		///            across clusters will be enabled. </param>
		ValueTask CreateNamespaceAsync(string @namespace, ISet<string> clusters);

		/// <summary>
		/// Create a new namespace.
		/// <p/>
		/// Creates a new namespace with the specified policies.
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
		void CreateNamespace(string @namespace, Policies policies);

		/// <summary>
		/// Create a new namespace asynchronously.
		/// <p/>
		/// Creates a new namespace with the specified policies.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="policies">
		///            Policies for the namespace </param>
		ValueTask CreateNamespaceAsync(string @namespace, Policies policies);

		/// <summary>
		/// Delete an existing namespace.
		/// <p/>
		/// The namespace needs to be empty.
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
		void DeleteNamespace(string @namespace);

		/// <summary>
		/// Delete an existing namespace.
		/// <p/>
		/// Force flag deletes namespace forcefully by force deleting all topics under it.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="force">
		///            Delete namespace forcefully
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Namespace is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteNamespace(string @namespace, bool force);

		/// <summary>
		/// Delete an existing namespace asynchronously.
		/// <p/>
		/// The namespace needs to be empty.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask DeleteNamespaceAsync(string @namespace);

		/// <summary>
		/// Delete an existing namespace asynchronously.
		/// <p/>
		/// Force flag deletes namespace forcefully by force deleting all topics under it.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="force">
		///            Delete namespace forcefully </param>
		ValueTask DeleteNamespaceAsync(string @namespace, bool force);

		/// <summary>
		/// Delete an existing bundle in a namespace.
		/// <p/>
		/// The bundle needs to be empty.
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
		void DeleteNamespaceBundle(string @namespace, string bundleRange);

		/// <summary>
		/// Delete an existing bundle in a namespace.
		/// <p/>
		/// Force flag deletes bundle forcefully.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundleRange">
		///            range of the bundle </param>
		/// <param name="force">
		///            Delete bundle forcefully
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace/bundle does not exist </exception>
		/// <exception cref="ConflictException">
		///             Bundle is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteNamespaceBundle(string @namespace, string bundleRange, bool force);

		/// <summary>
		/// Delete an existing bundle in a namespace asynchronously.
		/// <p/>
		/// The bundle needs to be empty.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundleRange">
		///            range of the bundle
		/// </param>
		/// <returns> a future that can be used to track when the bundle is deleted </returns>
		ValueTask DeleteNamespaceBundleAsync(string @namespace, string bundleRange);

		/// <summary>
		/// Delete an existing bundle in a namespace asynchronously.
		/// <p/>
		/// Force flag deletes bundle forcefully.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="bundleRange">
		///            range of the bundle </param>
		/// <param name="force">
		///            Delete bundle forcefully
		/// </param>
		/// <returns> a future that can be used to track when the bundle is deleted </returns>
		ValueTask DeleteNamespaceBundleAsync(string @namespace, string bundleRange, bool force);

		/// <summary>
		/// Get permissions on a namespace.
		/// <p/>
		/// Retrieve the permissions for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>{
		///   "my-role" : [ "produce" ]
		///   "other-role" : [ "consume" ]
		/// }</code>
		/// </pre>
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
		IDictionary<string, ISet<AuthAction>> GetPermissions(string @namespace);

		/// <summary>
		/// Get permissions on a namespace asynchronously.
		/// <p/>
		/// Retrieve the permissions for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>{
		///   "my-role" : [ "produce" ]
		///   "other-role" : [ "consume" ]
		/// }</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<IDictionary<string, ISet<AuthAction>>> GetPermissionsAsync(string @namespace);

		/// <summary>
		/// Grant permission on a namespace.
		/// <p/>
		/// Grant a new permission to a client role on a namespace.
		/// <p/>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>["produce", "consume"]</code>
		/// </pre>
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
		void GrantPermissionOnNamespace(string @namespace, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Grant permission on a namespace asynchronously.
		/// <p/>
		/// Grant a new permission to a client role on a namespace.
		/// <p/>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>["produce", "consume"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="role">
		///            Client role to which grant permission </param>
		/// <param name="actions">
		///            Auth actions (produce and consume) </param>
		ValueTask GrantPermissionOnNamespaceAsync(string @namespace, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Revoke permissions on a namespace.
		/// <p/>
		/// Revoke all permissions to a client role on a namespace.
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
		void RevokePermissionsOnNamespace(string @namespace, string role);

		/// <summary>
		/// Revoke permissions on a namespace asynchronously.
		/// <p/>
		/// Revoke all permissions to a client role on a namespace.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="role">
		///            Client role to which remove permissions </param>
		ValueTask RevokePermissionsOnNamespaceAsync(string @namespace, string role);

		/// <summary>
		/// Get permission to role to access subscription's admin-api. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IDictionary<string, ISet<string>> GetPermissionOnSubscription(string @namespace);

		/// <summary>
		/// Get permission to role to access subscription's admin-api asynchronously. </summary>
		/// <param name="namespace"> </param>
		ValueTask<IDictionary<string, ISet<string>>> GetPermissionOnSubscriptionAsync(string @namespace);

		/// <summary>
		/// Grant permission to role to access subscription's admin-api. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="roles"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void GrantPermissionOnSubscription(string @namespace, string subscription, ISet<string> roles);

		/// <summary>
		/// Grant permission to role to access subscription's admin-api asynchronously. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="roles"> </param>
		ValueTask GrantPermissionOnSubscriptionAsync(string @namespace, string subscription, ISet<string> roles);

		/// <summary>
		/// Revoke permissions on a subscription's admin-api access. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="role"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void RevokePermissionOnSubscription(string @namespace, string subscription, string role);

		/// <summary>
		/// Revoke permissions on a subscription's admin-api access asynchronously. </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <param name="role"> </param>
		ValueTask RevokePermissionOnSubscriptionAsync(string @namespace, string subscription, string role);

		/// <summary>
		/// Get the replication clusters for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["use", "usw", "usc"]</code>
		/// </pre>
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
		IList<string> GetNamespaceReplicationClusters(string @namespace);

		/// <summary>
		/// Get the replication clusters for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["use", "usw", "usc"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<IList<string>> GetNamespaceReplicationClustersAsync(string @namespace);

		/// <summary>
		/// Set the replication clusters for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>["us-west", "us-east", "us-cent"]</code>
		/// </pre>
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
		void SetNamespaceReplicationClusters(string @namespace, ISet<string> clusterIds);

		/// <summary>
		/// Set the replication clusters for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>["us-west", "us-east", "us-cent"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="clusterIds">
		///            Pulsar Cluster Ids </param>
		ValueTask SetNamespaceReplicationClustersAsync(string @namespace, ISet<string> clusterIds);

		/// <summary>
		/// Get the message TTL for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
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

		int? GetNamespaceMessageTTL(string @namespace);

		/// <summary>
		/// Get the message TTL for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetNamespaceMessageTTLAsync(string @namespace);

		/// <summary>
		/// Set the messages Time to Live for all the topics within a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
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


		void SetNamespaceMessageTTL(string @namespace, int ttlInSeconds);

		/// <summary>
		/// Set the messages Time to Live for all the topics within a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="ttlInSeconds">
		///            TTL values for all messages for all topics in this namespace </param>
		ValueTask SetNamespaceMessageTTLAsync(string @namespace, int ttlInSeconds);

		/// <summary>
		/// Remove the messages Time to Live for all the topics within a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

		void RemoveNamespaceMessageTTL(string @namespace);

		/// <summary>
		/// Remove the messages Time to Live for all the topics within a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveNamespaceMessageTTLAsync(string @namespace);

		/// <summary>
		/// Get the subscription expiration time for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>1440</code>
		/// </pre>
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
        
        int? GetSubscriptionExpirationTime(string @namespace);

		/// <summary>
		/// Get the subscription expiration time for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>1440</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetSubscriptionExpirationTimeAsync(string @namespace);

		/// <summary>
		/// Set the subscription expiration time in minutes for all the topics within a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>1440</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="expirationTime">
		///            Expiration time values for all subscriptions for all topics in this namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void SetSubscriptionExpirationTime(string @namespace, int expirationTime);

		/// <summary>
		/// Set the subscription expiration time in minutes for all the topics within a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>1440</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="expirationTime">
		///            Expiration time values for all subscriptions for all topics in this namespace </param>
		ValueTask SetSubscriptionExpirationTimeAsync(string @namespace, int expirationTime);

		/// <summary>
		/// Remove the subscription expiration time for a namespace.
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
		void RemoveSubscriptionExpirationTime(string @namespace);

		/// <summary>
		/// Remove the subscription expiration time for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask RemoveSubscriptionExpirationTimeAsync(string @namespace);

		/// <summary>
		/// Set anti-affinity group name for a namespace.
		/// <p/>
		/// Request example:
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
		///             Unexpected error </exception>n;
		void SetNamespaceAntiAffinityGroup(string @namespace, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Set anti-affinity group name for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="namespaceAntiAffinityGroup">
		///            anti-affinity group name for a namespace </param>
		ValueTask SetNamespaceAntiAffinityGroupAsync(string @namespace, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Get all namespaces that grouped with given anti-affinity group.
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


		IList<string> GetAntiAffinityNamespaces(string tenant, string cluster, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Get all namespaces that grouped with given anti-affinity group asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            tenant is only used for authorization. Client has to be admin of any of the tenant to access this
		///            api api. </param>
		/// <param name="cluster">
		///            cluster name </param>
		/// <param name="namespaceAntiAffinityGroup">
		///            Anti-affinity group name </param>
		/// <returns> list of namespace grouped under a given anti-affinity group </returns>
		ValueTask<IList<string>> GetAntiAffinityNamespacesAsync(string tenant, string cluster, string namespaceAntiAffinityGroup);

		/// <summary>
		/// Get anti-affinity group name for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
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

		string GetNamespaceAntiAffinityGroup(string @namespace);

		/// <summary>
		/// Get anti-affinity group name for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<string> GetNamespaceAntiAffinityGroupAsync(string @namespace);

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
		void DeleteNamespaceAntiAffinityGroup(string @namespace);

		/// <summary>
		/// Delete anti-affinity group name for a namespace.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask DeleteNamespaceAntiAffinityGroupAsync(string @namespace);

		/// <summary>
		/// Remove the deduplication status for all topics within a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void RemoveDeduplicationStatus(string @namespace);

		/// <summary>
		/// Get the deduplication status for all topics within a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveDeduplicationStatusAsync(string @namespace);
		/// <summary>
		/// Get the deduplication status for all topics within a namespace . </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		bool? GetDeduplicationStatus(string @namespace);

		/// <summary>
		/// Get the deduplication status for all topics within a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<bool> GetDeduplicationStatusAsync(string @namespace);
		/// <summary>
		/// Set the deduplication status for all topics within a namespace.
		/// <p/>
		/// When deduplication is enabled, the broker will prevent to store the same message multiple times.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
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
		void SetDeduplicationStatus(string @namespace, bool enableDeduplication);

		/// <summary>
		/// Set the deduplication status for all topics within a namespace asynchronously.
		/// <p/>
		/// When deduplication is enabled, the broker will prevent to store the same message multiple times.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="enableDeduplication">
		///            wether to enable or disable deduplication feature </param>
		ValueTask SetDeduplicationStatusAsync(string @namespace, bool enableDeduplication);

		/// <summary>
		/// Sets the autoTopicCreation policy for a given namespace, overriding broker settings.
		/// <p/>
		/// When autoTopicCreationOverride is enabled, new topics will be created upon connection,
		/// regardless of the broker level configuration.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "allowAutoTopicCreation" : true,
		///      "topicType" : "partitioned",
		///      "defaultNumPartitions": 2
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="autoTopicCreationOverride">
		///            Override policies for auto topic creation
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void SetAutoTopicCreation(string @namespace, AutoTopicCreationOverride autoTopicCreationOverride);

		/// <summary>
		/// Sets the autoTopicCreation policy for a given namespace, overriding broker settings asynchronously.
		/// <p/>
		/// When autoTopicCreationOverride is enabled, new topics will be created upon connection,
		/// regardless of the broker level configuration.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "allowAutoTopicCreation" : true,
		///      "topicType" : "partitioned",
		///      "defaultNumPartitions": 2
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="autoTopicCreationOverride">
		///            Override policies for auto topic creation </param>
		ValueTask SetAutoTopicCreationAsync(string @namespace, AutoTopicCreationOverride autoTopicCreationOverride);

		/// <summary>
		/// Get the autoTopicCreation info within a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		AutoTopicCreationOverride GetAutoTopicCreation(string @namespace);

		/// <summary>
		/// Get the autoTopicCreation info within a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<AutoTopicCreationOverride> GetAutoTopicCreationAsync(string @namespace);

		/// <summary>
		/// Removes the autoTopicCreation policy for a given namespace.
		/// <p/>
		/// Allowing the broker to dictate the auto-creation policy.
		/// <p/>
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
		void RemoveAutoTopicCreation(string @namespace);

		/// <summary>
		/// Removes the autoTopicCreation policy for a given namespace asynchronously.
		/// <p/>
		/// Allowing the broker to dictate the auto-creation policy.
		/// <p/>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask RemoveAutoTopicCreationAsync(string @namespace);

		/// <summary>
		/// Sets the autoSubscriptionCreation policy for a given namespace, overriding broker settings.
		/// <p/>
		/// When autoSubscriptionCreationOverride is enabled, new subscriptions will be created upon connection,
		/// regardless of the broker level configuration.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "allowAutoSubscriptionCreation" : true
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="autoSubscriptionCreationOverride">
		///            Override policies for auto subscription creation
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void SetAutoSubscriptionCreation(string @namespace, AutoSubscriptionCreationOverride autoSubscriptionCreationOverride);

		/// <summary>
		/// Sets the autoSubscriptionCreation policy for a given namespace, overriding broker settings asynchronously.
		/// <p/>
		/// When autoSubscriptionCreationOverride is enabled, new subscriptions will be created upon connection,
		/// regardless of the broker level configuration.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "allowAutoSubscriptionCreation" : true
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="autoSubscriptionCreationOverride">
		///            Override policies for auto subscription creation </param>
		ValueTask SetAutoSubscriptionCreationAsync(string @namespace, AutoSubscriptionCreationOverride autoSubscriptionCreationOverride);

		/// <summary>
		/// Get the autoSubscriptionCreation info within a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		AutoSubscriptionCreationOverride GetAutoSubscriptionCreation(string @namespace);

		/// <summary>
		/// Get the autoSubscriptionCreation info within a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<AutoSubscriptionCreationOverride> GetAutoSubscriptionCreationAsync(string @namespace);

		/// <summary>
		/// Sets the subscriptionTypesEnabled policy for a given namespace, overriding broker settings.
		/// 
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "subscriptionTypesEnabled" : {"Shared", "Failover"}
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="subscriptionTypesEnabled">
		///            is enable subscription types
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void SetSubscriptionTypesEnabled(string @namespace, ISet<SubscriptionType> subscriptionTypesEnabled);

		/// <summary>
		/// Sets the subscriptionTypesEnabled policy for a given namespace, overriding broker settings.
		/// 
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		///  {
		///      "subscriptionTypesEnabled" : {"Shared", "Failover"}
		///  }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="subscriptionTypesEnabled">
		///            is enable subscription types </param>
		ValueTask SetSubscriptionTypesEnabledAsync(string @namespace, ISet<SubscriptionType> subscriptionTypesEnabled);

		/// <summary>
		/// Get the subscriptionTypesEnabled policy for a given namespace, overriding broker settings.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> subscription types <seealso cref="System.Collections.Generic.ISet<SubscriptionType>"/> the subscription types
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ISet<SubscriptionType> GetSubscriptionTypesEnabled(string @namespace);

		/// <summary>
		/// Get the subscriptionTypesEnabled policy for a given namespace, overriding broker settings.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> the future of subscription types <seealso cref="System.Collections.Generic.ISet<SubscriptionType>"/> the subscription types </returns>
		ValueTask<ISet<SubscriptionType>> GetSubscriptionTypesEnabledAsync(string @namespace);

		/// <summary>
		/// Removes the subscriptionTypesEnabled policy for a given namespace.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		/// @return </exception>
		void RemoveSubscriptionTypesEnabled(string @namespace);

		/// <summary>
		/// Removes the subscriptionTypesEnabled policy for a given namespace.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// @return </param>
		ValueTask RemoveSubscriptionTypesEnabledAsync(string @namespace);

		/// <summary>
		/// Removes the autoSubscriptionCreation policy for a given namespace.
		/// <p/>
		/// Allowing the broker to dictate the subscription auto-creation policy.
		/// <p/>
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
		void RemoveAutoSubscriptionCreation(string @namespace);

		/// <summary>
		/// Removes the autoSubscriptionCreation policy for a given namespace asynchronously.
		/// <p/>
		/// Allowing the broker to dictate the subscription auto-creation policy.
		/// <p/>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask RemoveAutoSubscriptionCreationAsync(string @namespace);

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
		//
        BundlesData getBundlesData(string @namespace);

		/// <summary>
		/// Get backlog quota map on a namespace.
		/// <p/>
		/// Get backlog quota map on a namespace.
		/// <p/>
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

		IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> GetBacklogQuotaMap(string @namespace);

		/// <summary>
		/// Get backlog quota map on a namespace asynchronously.
		/// <p/>
		/// Get backlog quota map on a namespace.
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota>> GetBacklogQuotaMapAsync(string @namespace);

		/// <summary>
		/// Set a backlog quota for all the topics on a namespace.
		/// <p/>
		/// Set a backlog quota on a namespace.
		/// <p/>
		/// The backlog quota can be set on this resource:
		/// <p/>
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
		void SetBacklogQuota(string @namespace, BacklogQuota backlogQuota, BacklogQuota.BacklogQuotaType backlogQuotaType);

		void SetBacklogQuota(string @namespace, BacklogQuota backlogQuota)
		{
			SetBacklogQuota(@namespace, backlogQuota, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}

		/// <summary>
		/// Set a backlog quota for all the topics on a namespace asynchronously.
		/// <p/>
		/// Set a backlog quota on a namespace.
		/// <p/>
		/// The backlog quota can be set on this resource:
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="backlogQuota">
		///            the new BacklogQuota </param>
		ValueTask SetBacklogQuotaAsync(string @namespace, BacklogQuota backlogQuota, BacklogQuota.BacklogQuotaType backlogQuotaType);

		ValueTask SetBacklogQuotaAsync(string @namespace, BacklogQuota backlogQuota)
		{
			return SetBacklogQuotaAsync(@namespace, backlogQuota, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}

		/// <summary>
		/// Remove a backlog quota policy from a namespace.
		/// <p/>
		/// Remove a backlog quota policy from a namespace.
		/// <p/>
		/// The backlog retention policy will fall back to the default.
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
		void RemoveBacklogQuota(string @namespace, BacklogQuota.BacklogQuotaType backlogQuotaType);

		void RemoveBacklogQuota(string @namespace)
		{
			RemoveBacklogQuota(@namespace, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}

		/// <summary>
		/// Remove a backlog quota policy from a namespace asynchronously.
		/// <p/>
		/// Remove a backlog quota policy from a namespace.
		/// <p/>
		/// The backlog retention policy will fall back to the default.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask RemoveBacklogQuotaAsync(string @namespace, BacklogQuota.BacklogQuotaType backlogQuotaType);

		ValueTask RemoveBacklogQuotaAsync(string @namespace)
		{
			return RemoveBacklogQuotaAsync(@namespace, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}


		/// <summary>
		/// Remove the persistence configuration on a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void RemovePersistence(string @namespace);

		/// <summary>
		/// Remove the persistence configuration on a namespace asynchronously. </summary>
		/// <param name="namespace"> </param>
		ValueTask RemovePersistenceAsync(string @namespace);

		/// <summary>
		/// Set the persistence configuration for all the topics on a namespace.
		/// <p/>
		/// Set the persistence configuration on a namespace.
		/// <p/>
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
		/// </summary>
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
		void SetPersistence(string @namespace, PersistencePolicies persistence);

		/// <summary>
		/// Set the persistence configuration for all the topics on a namespace asynchronously.
		/// <p/>
		/// Set the persistence configuration on a namespace.
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="persistence">
		///            Persistence policies object </param>
		ValueTask SetPersistenceAsync(string @namespace, PersistencePolicies persistence);

		/// <summary>
		/// Get the persistence configuration for a namespace.
		/// <p/>
		/// Get the persistence configuration for a namespace.
		/// <p/>
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
		PersistencePolicies GetPersistence(string @namespace);

		/// <summary>
		/// Get the persistence configuration for a namespace asynchronously.
		/// <p/>
		/// Get the persistence configuration for a namespace.
		/// <p/>
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
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<PersistencePolicies> GetPersistenceAsync(string @namespace);

		/// <summary>
		/// Set bookie affinity group for a namespace to isolate namespace write to bookies that are part of given affinity
		/// group.
		/// </summary>
		/// <param name="namespace">
		///            namespace name </param>
		/// <param name="bookieAffinityGroup">
		///            bookie affinity group </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void SetBookieAffinityGroup(string @namespace, BookieAffinityGroupData bookieAffinityGroup);

		/// <summary>
		/// Set bookie affinity group for a namespace to isolate namespace write to bookies that are part of given affinity
		/// group asynchronously.
		/// </summary>
		/// <param name="namespace">
		///            namespace name </param>
		/// <param name="bookieAffinityGroup">
		///            bookie affinity group </param>
		ValueTask SetBookieAffinityGroupAsync(string @namespace, BookieAffinityGroupData bookieAffinityGroup);

		/// <summary>
		/// Delete bookie affinity group configured for a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void DeleteBookieAffinityGroup(string @namespace);

		/// <summary>
		/// Delete bookie affinity group configured for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		ValueTask DeleteBookieAffinityGroupAsync(string @namespace);

		/// <summary>
		/// Get bookie affinity group configured for a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		BookieAffinityGroupData GetBookieAffinityGroup(string @namespace);

		/// <summary>
		/// Get bookie affinity group configured for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<BookieAffinityGroupData> GetBookieAffinityGroupAsync(string @namespace);

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


        void SetRetention(string @namespace, RetentionPolicies retention);

		/// <summary>
		/// Set the retention configuration for all the topics on a namespace asynchronously.
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
		///            Namespace name </param>
		ValueTask SetRetentionAsync(string @namespace, RetentionPolicies retention);

		/// <summary>
		/// Remove the retention configuration for all the topics on a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveRetention(string @namespace);

		/// <summary>
		/// Remove the retention configuration for all the topics on a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveRetentionAsync(string @namespace);

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

        RetentionPolicies GetRetention(string @namespace);

		/// <summary>
		/// Get the retention configuration for a namespace asynchronously.
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
		ValueTask<RetentionPolicies> GetRetentionAsync(string @namespace);

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

        void Unload(string @namespace);

		/// <summary>
		/// Unload a namespace from the current serving broker asynchronously.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask UnloadAsync(string @namespace);

		/// <summary>
		/// Get the replication configuration version for a given namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> Replication configuration version </returns>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        string GetReplicationConfigVersion(string @namespace);

		/// <summary>
		/// Get the replication configuration version for a given namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> Replication configuration version </returns>
		ValueTask<string> GetReplicationConfigVersionAsync(string @namespace);

		/// <summary>
		/// Unload namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		///           range of bundle to unload </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        void UnloadNamespaceBundle(string @namespace, string bundle);

		/// <summary>
		/// Unload namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		///           range of bundle to unload
		/// </param>
		/// <returns> a future that can be used to track when the bundle is unloaded </returns>
		ValueTask UnloadNamespaceBundleAsync(string @namespace, string bundle);

		/// <summary>
		/// Split namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle to split </param>
		/// <param name="unloadSplitBundles"> </param>
		/// <param name="splitAlgorithmName"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

        void SplitNamespaceBundle(string @namespace, string bundle, bool unloadSplitBundles, string splitAlgorithmName);

		/// <summary>
		/// Split namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle to split </param>
		/// <param name="unloadSplitBundles"> </param>
		/// <param name="splitAlgorithmName"> </param>
		ValueTask SplitNamespaceBundleAsync(string @namespace, string bundle, bool unloadSplitBundles, string splitAlgorithmName);

		/// <summary>
		/// Split namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle to split </param>
		/// <param name="unloadSplitBundles"> </param>
		/// <param name="splitAlgorithmName"> </param>
		/// <param name="splitBoundaries"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

        void SplitNamespaceBundle(string @namespace, string bundle, bool unloadSplitBundles, string splitAlgorithmName, IList<long> splitBoundaries);

		/// <summary>
		/// Split namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle to split </param>
		/// <param name="unloadSplitBundles"> </param>
		/// <param name="splitAlgorithmName"> </param>
		/// <param name="splitBoundaries"> </param>
		ValueTask SplitNamespaceBundleAsync(string @namespace, string bundle, bool unloadSplitBundles, string splitAlgorithmName, IList<long> splitBoundaries);

		/// <summary>
		/// Get positions for topic list in a bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle </param>
		/// <param name="topics"> </param>
		/// <returns> hash positions for all topics in topicList </returns>
		/// <exception cref="PulsarAdminException"> </exception>
    	TopicHashPositions GetTopicHashPositions(string @namespace, string bundle, IList<string> topics);

		/// <summary>
		/// Get positions for topic list in a bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> range of bundle </param>
		/// <param name="topics"> </param>
		/// <returns> hash positions for all topics in topicList </returns>
		/// <exception cref="PulsarAdminException"> </exception>
		ValueTask<TopicHashPositions> GetTopicHashPositionsAsync(string @namespace, string bundle, IList<string> topics);

		/// <summary>
		/// Set message-publish-rate (topics under this namespace can publish this many messages per second).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="publishMsgRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
        void SetPublishRate(string @namespace, PublishRate publishMsgRate);

		/// <summary>
		/// Remove message-publish-rate (topics under this namespace can publish this many messages per second).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
        void RemovePublishRate(string @namespace);

		/// <summary>
		/// Set message-publish-rate (topics under this namespace can publish this many messages per second) asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="publishMsgRate">
		///            number of messages per second </param>
		ValueTask SetPublishRateAsync(string @namespace, PublishRate publishMsgRate);

		/// <summary>
		/// Remove message-publish-rate asynchronously.
		/// <p/>
		/// topics under this namespace can publish this many messages per second </summary>
		/// <param name="namespace"> </param>
		ValueTask RemovePublishRateAsync(string @namespace);

		/// <summary>
		/// Get message-publish-rate (topics under this namespace can publish this many messages per second).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> number of messages per second </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
        /// 
        PublishRate GetPublishRate(string @namespace);

		/// <summary>
		/// Get message-publish-rate (topics under this namespace can publish this many messages per second) asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> number of messages per second </returns>
		ValueTask<PublishRate> GetPublishRateAsync(string @namespace);

		/// <summary>
		/// Remove message-dispatch-rate. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>



		void RemoveDispatchRate(string @namespace);

		/// <summary>
		/// Remove message-dispatch-rate asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveDispatchRateAsync(string @namespace);
		/// <summary>
		/// Set message-dispatch-rate (topics under this namespace can dispatch this many messages per second).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


		void SetDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Set message-dispatch-rate asynchronously.
		/// <p/>
		/// topics under this namespace can dispatch this many messages per second
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetDispatchRateAsync(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Get message-dispatch-rate (topics under this namespace can dispatch this many messages per second).
		/// </summary>
		/// <param name="namespace">
		/// @returns messageRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        DispatchRate GetDispatchRate(string @namespace);

		/// <summary>
		/// Get message-dispatch-rate asynchronously.
		/// <p/>
		/// Topics under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace">
		/// @returns messageRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetDispatchRateAsync(string @namespace);

		/// <summary>
		/// Set namespace-subscribe-rate (topics under this namespace will limit by subscribeRate).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscribeRate">
		///            consumer subscribe limit by this subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetSubscribeRate(string @namespace, SubscribeRate subscribeRate);

		/// <summary>
		/// Set namespace-subscribe-rate (topics under this namespace will limit by subscribeRate) asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscribeRate">
		///            consumer subscribe limit by this subscribeRate </param>
		ValueTask SetSubscribeRateAsync(string @namespace, SubscribeRate subscribeRate);

		/// <summary>
		/// Remove namespace-subscribe-rate (topics under this namespace will limit by subscribeRate).
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveSubscribeRate(string @namespace);

		/// <summary>
		/// Remove namespace-subscribe-rate (topics under this namespace will limit by subscribeRate) asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		ValueTask RemoveSubscribeRateAsync(string @namespace);

		/// <summary>
		/// Get namespace-subscribe-rate (topics under this namespace allow subscribe times per consumer in a period).
		/// </summary>
		/// <param name="namespace">
		/// @returns subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        SubscribeRate GetSubscribeRate(string @namespace);

		/// <summary>
		/// Get namespace-subscribe-rate asynchronously.
		/// <p/>
		/// Topics under this namespace allow subscribe times per consumer in a period.
		/// </summary>
		/// <param name="namespace">
		/// @returns subscribeRate </param>
		ValueTask<SubscribeRate> GetSubscribeRateAsync(string @namespace);

		/// <summary>
		/// Remove subscription-message-dispatch-rate. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveSubscriptionDispatchRate(string @namespace);

		/// <summary>
		/// Remove subscription-message-dispatch-rate asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveSubscriptionDispatchRateAsync(string @namespace);

		/// <summary>
		/// Set subscription-message-dispatch-rate.
		/// <p/>
		/// Subscriptions under this namespace can dispatch this many messages per second
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetSubscriptionDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Set subscription-message-dispatch-rate asynchronously.
		/// <p/>
		/// Subscriptions under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetSubscriptionDispatchRateAsync(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Get subscription-message-dispatch-rate.
		/// <p/>
		/// Subscriptions under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        DispatchRate GetSubscriptionDispatchRate(string @namespace);

		/// <summary>
		/// Get subscription-message-dispatch-rate asynchronously.
		/// <p/>
		/// Subscriptions under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string @namespace);

		/// <summary>
		/// Set replicator-message-dispatch-rate.
		/// <p/>
		/// Replicators under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        
        void SetReplicatorDispatchRate(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Set replicator-message-dispatch-rate asynchronously.
		/// <p/>
		/// Replicators under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetReplicatorDispatchRateAsync(string @namespace, DispatchRate dispatchRate);

		/// <summary>
		/// Remove replicator-message-dispatch-rate.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void RemoveReplicatorDispatchRate(string @namespace);

		/// <summary>
		/// Set replicator-message-dispatch-rate asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		ValueTask RemoveReplicatorDispatchRateAsync(string @namespace);

		/// <summary>
		/// Get replicator-message-dispatch-rate.
		/// <p/>
		/// Replicators under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        DispatchRate GetReplicatorDispatchRate(string @namespace);

		/// <summary>
		/// Get replicator-message-dispatch-rate asynchronously.
		/// <p/>
		/// Replicators under this namespace can dispatch this many messages per second.
		/// </summary>
		/// <param name="namespace">
		/// @returns DispatchRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetReplicatorDispatchRateAsync(string @namespace);

		/// <summary>
		/// Clear backlog for all topics on a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void ClearNamespaceBacklog(string @namespace);

		/// <summary>
		/// Clear backlog for all topics on a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		ValueTask ClearNamespaceBacklogAsync(string @namespace);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void ClearNamespaceBacklogForSubscription(string @namespace, string subscription);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		ValueTask ClearNamespaceBacklogForSubscriptionAsync(string @namespace, string subscription);

		/// <summary>
		/// Clear backlog for all topics on a namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        void ClearNamespaceBundleBacklog(string @namespace, string bundle);

		/// <summary>
		/// Clear backlog for all topics on a namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle">
		/// </param>
		/// <returns> a future that can be used to track when the bundle is cleared </returns>
		ValueTask ClearNamespaceBundleBacklogAsync(string @namespace, string bundle);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void ClearNamespaceBundleBacklogForSubscription(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Clear backlog for a given subscription on all topics on a namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription">
		/// </param>
		/// <returns> a future that can be used to track when the bundle is cleared </returns>
		ValueTask ClearNamespaceBundleBacklogForSubscriptionAsync(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Unsubscribe the given subscription on all topics on a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void UnsubscribeNamespace(string @namespace, string subscription);

		/// <summary>
		/// Unsubscribe the given subscription on all topics on a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscription"> </param>
		ValueTask UnsubscribeNamespaceAsync(string @namespace, string subscription);

		/// <summary>
		/// Unsubscribe the given subscription on all topics on a namespace bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void UnsubscribeNamespaceBundle(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Unsubscribe the given subscription on all topics on a namespace bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundle"> </param>
		/// <param name="subscription">
		/// </param>
		/// <returns> a future that can be used to track when the subscription is unsubscribed </returns>
		ValueTask UnsubscribeNamespaceBundleAsync(string @namespace, string bundle, string subscription);

		/// <summary>
		/// Set the encryption required status for all topics within a namespace.
		/// <p/>
		/// When encryption required is true, the broker will prevent to store unencrypted messages.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
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


        void SetEncryptionRequiredStatus(string @namespace, bool encryptionRequired);

		/// <summary>
		/// Get the encryption required status within a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        bool? GetEncryptionRequiredStatus(string @namespace);

		/// <summary>
		/// Get the encryption required status within a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<bool> GetEncryptionRequiredStatusAsync(string @namespace);

		/// <summary>
		/// Set the encryption required status for all topics within a namespace asynchronously.
		/// <p/>
		/// When encryption required is true, the broker will prevent to store unencrypted messages.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>true</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="encryptionRequired">
		///            whether message encryption is required or not </param>
		ValueTask SetEncryptionRequiredStatusAsync(string @namespace, bool encryptionRequired);

		/// <summary>
		/// Get the delayed delivery messages for all topics within a namespace.
		/// <p/>
		/// If disabled, messages will be immediately delivered and there will
		/// be no tracking overhead.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "active" : true,   // Enable or disable delayed delivery for messages on a namespace
		///     "tickTime" : 1000, // The tick time for when retrying on delayed delivery messages
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> delayedDeliveryPolicies
		///            Whether to enable the delayed delivery for messages.
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        DelayedDeliveryPolicies GetDelayedDelivery(string @namespace);

		/// <summary>
		/// Get the delayed delivery messages for all topics within a namespace asynchronously.
		/// <p/>
		/// If disabled, messages will be immediately delivered and there will
		/// be no tracking overhead.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "active" : true,   // Enable or disable delayed delivery for messages on a namespace
		///     "tickTime" : 1000, // The tick time for when retrying on delayed delivery messages
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> delayedDeliveryPolicies
		///            Whether to enable the delayed delivery for messages. </returns>
		ValueTask<DelayedDeliveryPolicies> GetDelayedDeliveryAsync(string @namespace);

		/// <summary>
		/// Set the delayed delivery messages for all topics within a namespace.
		/// <p/>
		/// If disabled, messages will be immediately delivered and there will
		/// be no tracking overhead.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "tickTime" : 1000, // Enable or disable delayed delivery for messages on a namespace
		///     "active" : true,   // The tick time for when retrying on delayed delivery messages
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="delayedDeliveryPolicies">
		///            Whether to enable the delayed delivery for messages.
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetDelayedDeliveryMessages(string @namespace, DelayedDeliveryPolicies delayedDeliveryPolicies);

		/// <summary>
		/// Set the delayed delivery messages for all topics within a namespace asynchronously.
		/// <p/>
		/// If disabled, messages will be immediately delivered and there will
		/// be no tracking overhead.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "tickTime" : 1000, // Enable or disable delayed delivery for messages on a namespace
		///     "active" : true,   // The tick time for when retrying on delayed delivery messages
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="delayedDeliveryPolicies">
		///            Whether to enable the delayed delivery for messages. </param>
		ValueTask SetDelayedDeliveryMessagesAsync(string @namespace, DelayedDeliveryPolicies delayedDeliveryPolicies);

		/// <summary>
		/// Remove the delayed delivery messages for all topics within a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveDelayedDeliveryMessages(string @namespace);
		/// <summary>
		/// Remove the delayed delivery messages for all topics within a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveDelayedDeliveryMessagesAsync(string @namespace);

		/// <summary>
		/// Get the inactive deletion strategy for all topics within a namespace synchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        InactiveTopicPolicies GetInactiveTopicPolicies(string @namespace);

		/// <summary>
		/// remove InactiveTopicPolicies from a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveInactiveTopicPoliciesAsync(string @namespace);

		/// <summary>
		/// Remove inactive topic policies from a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveInactiveTopicPolicies(string @namespace);

		/// <summary>
		/// Get the inactive deletion strategy for all topics within a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<InactiveTopicPolicies> GetInactiveTopicPoliciesAsync(string @namespace);

		/// <summary>
		/// As same as setInactiveTopicPoliciesAsync, but it is synchronous. </summary>
		/// <param name="namespace"> </param>
		/// <param name="inactiveTopicPolicies"> </param>


        void SetInactiveTopicPolicies(string @namespace, InactiveTopicPolicies inactiveTopicPolicies);

		/// <summary>
		/// You can set the inactive deletion strategy at the namespace level.
		/// Its priority is higher than the inactive deletion strategy at the broker level.
		/// All topics under this namespace will follow this strategy. </summary>
		/// <param name="namespace"> </param>
		/// <param name="inactiveTopicPolicies">
		/// @return </param>
		ValueTask SetInactiveTopicPoliciesAsync(string @namespace, InactiveTopicPolicies inactiveTopicPolicies);
		/// <summary>
		/// Set the given subscription auth mode on all topics on a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscriptionAuthMode"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void SetSubscriptionAuthMode(string @namespace, SubscriptionAuthMode subscriptionAuthMode);

		/// <summary>
		/// Set the given subscription auth mode on all topics on a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="subscriptionAuthMode"> </param>
		ValueTask SetSubscriptionAuthModeAsync(string @namespace, SubscriptionAuthMode subscriptionAuthMode);

		/// <summary>
		/// Get the subscriptionAuthMode within a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        SubscriptionAuthMode GetSubscriptionAuthMode(string @namespace);

		/// <summary>
		/// Get the subscriptionAuthMode within a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<SubscriptionAuthMode> GetSubscriptionAuthModeAsync(string @namespace);

		/// <summary>
		/// Get the deduplicationSnapshotInterval for a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        int? GetDeduplicationSnapshotInterval(string @namespace);

		/// <summary>
		/// Get the deduplicationSnapshotInterval for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<int> GetDeduplicationSnapshotIntervalAsync(string @namespace);

		/// <summary>
		/// Set the deduplicationSnapshotInterval for a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="interval"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void SetDeduplicationSnapshotInterval(string @namespace, int? interval);

		/// <summary>
		/// Set the deduplicationSnapshotInterval for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="interval">
		/// @return </param>
		ValueTask SetDeduplicationSnapshotIntervalAsync(string @namespace, int? interval);

		/// <summary>
		/// Remove the deduplicationSnapshotInterval for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveDeduplicationSnapshotInterval(string @namespace);

		/// <summary>
		/// Remove the deduplicationSnapshotInterval for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveDeduplicationSnapshotIntervalAsync(string @namespace);

		/// <summary>
		/// Get the maxSubscriptionsPerTopic for a namespace.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        int? GetMaxSubscriptionsPerTopic(string @namespace);

		/// <summary>
		/// Get the maxSubscriptionsPerTopic for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask<int> GetMaxSubscriptionsPerTopicAsync(string @namespace);

		/// <summary>
		/// Set the maxSubscriptionsPerTopic for a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="maxSubscriptionsPerTopic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void SetMaxSubscriptionsPerTopic(string @namespace, int maxSubscriptionsPerTopic);

		/// <summary>
		/// Set the maxSubscriptionsPerTopic for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="maxSubscriptionsPerTopic">
		/// @return </param>
		ValueTask SetMaxSubscriptionsPerTopicAsync(string @namespace, int maxSubscriptionsPerTopic);

		/// <summary>
		/// Remove the maxSubscriptionsPerTopic for a namespace.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveMaxSubscriptionsPerTopic(string @namespace);

		/// <summary>
		/// Remove the maxSubscriptionsPerTopic for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveMaxSubscriptionsPerTopicAsync(string @namespace);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
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


        int? GetMaxProducersPerTopic(string @namespace);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetMaxProducersPerTopicAsync(string @namespace);

		/// <summary>
		/// Set maxProducersPerTopic for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
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


        void SetMaxProducersPerTopic(string @namespace, int maxProducersPerTopic);

		/// <summary>
		/// Set maxProducersPerTopic for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxProducersPerTopic">
		///            maxProducersPerTopic value for a namespace </param>
		ValueTask SetMaxProducersPerTopicAsync(string @namespace, int maxProducersPerTopic);

		/// <summary>
		/// Remove maxProducersPerTopic for a namespace. </summary>
		/// <param name="namespace"> Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void RemoveMaxProducersPerTopic(string @namespace);

		/// <summary>
		/// Set maxProducersPerTopic for a namespace asynchronously. </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask RemoveMaxProducersPerTopicAsync(string @namespace);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
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


        int? GetMaxConsumersPerTopic(string @namespace);

		/// <summary>
		/// Get the maxProducersPerTopic for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetMaxConsumersPerTopicAsync(string @namespace);

		/// <summary>
		/// Set maxConsumersPerTopic for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
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


        void SetMaxConsumersPerTopic(string @namespace, int maxConsumersPerTopic);

		/// <summary>
		/// Set maxConsumersPerTopic for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxConsumersPerTopic">
		///            maxConsumersPerTopic value for a namespace </param>
		ValueTask SetMaxConsumersPerTopicAsync(string @namespace, int maxConsumersPerTopic);

		/// <summary>
		/// Remove maxConsumersPerTopic for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveMaxConsumersPerTopic(string @namespace);

		/// <summary>
		/// Remove maxConsumersPerTopic for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveMaxConsumersPerTopicAsync(string @namespace);

		/// <summary>
		/// Get the maxConsumersPerSubscription for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
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


        int? GetMaxConsumersPerSubscription(string @namespace);

		/// <summary>
		/// Get the maxConsumersPerSubscription for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetMaxConsumersPerSubscriptionAsync(string @namespace);

		/// <summary>
		/// Set maxConsumersPerSubscription for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
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


        void SetMaxConsumersPerSubscription(string @namespace, int maxConsumersPerSubscription);

		/// <summary>
		/// Set maxConsumersPerSubscription for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxConsumersPerSubscription">
		///            maxConsumersPerSubscription value for a namespace </param>
		ValueTask SetMaxConsumersPerSubscriptionAsync(string @namespace, int maxConsumersPerSubscription);

		/// <summary>
		/// Remove maxConsumersPerSubscription for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveMaxConsumersPerSubscription(string @namespace);

		/// <summary>
		/// Remove maxConsumersPerSubscription for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveMaxConsumersPerSubscriptionAsync(string @namespace);

		/// <summary>
		/// Get the maxUnackedMessagesPerConsumer for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
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


        int? GetMaxUnackedMessagesPerConsumer(string @namespace);

		/// <summary>
		/// Get the maxUnackedMessagesPerConsumer for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetMaxUnackedMessagesPerConsumerAsync(string @namespace);

		/// <summary>
		/// Set maxUnackedMessagesPerConsumer for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxUnackedMessagesPerConsumer">
		///            maxUnackedMessagesPerConsumer value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetMaxUnackedMessagesPerConsumer(string @namespace, int maxUnackedMessagesPerConsumer);

		/// <summary>
		/// Set maxUnackedMessagesPerConsumer for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxUnackedMessagesPerConsumer">
		///            maxUnackedMessagesPerConsumer value for a namespace </param>
		ValueTask SetMaxUnackedMessagesPerConsumerAsync(string @namespace, int maxUnackedMessagesPerConsumer);

		/// <summary>
		/// Remove maxUnackedMessagesPerConsumer for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveMaxUnackedMessagesPerConsumer(string @namespace);

		/// <summary>
		/// Remove maxUnackedMessagesPerConsumer for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveMaxUnackedMessagesPerConsumerAsync(string @namespace);
		/// <summary>
		/// Get the maxUnackedMessagesPerSubscription for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
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

        
        int? GetMaxUnackedMessagesPerSubscription(string @namespace);

		/// <summary>
		/// Get the maxUnackedMessagesPerSubscription for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<int> GetMaxUnackedMessagesPerSubscriptionAsync(string @namespace);

		/// <summary>
		/// Set maxUnackedMessagesPerSubscription for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxUnackedMessagesPerSubscription">
		///            Max number of unacknowledged messages allowed per shared subscription.
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        
        void SetMaxUnackedMessagesPerSubscription(string @namespace, int maxUnackedMessagesPerSubscription);

		/// <summary>
		/// Set maxUnackedMessagesPerSubscription for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="maxUnackedMessagesPerSubscription">
		///            Max number of unacknowledged messages allowed per shared subscription. </param>
		ValueTask SetMaxUnackedMessagesPerSubscriptionAsync(string @namespace, int maxUnackedMessagesPerSubscription);

		/// <summary>
		/// Remove maxUnackedMessagesPerSubscription for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveMaxUnackedMessagesPerSubscription(string @namespace);

		/// <summary>
		/// Remove maxUnackedMessagesPerSubscription for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveMaxUnackedMessagesPerSubscriptionAsync(string @namespace);

		/// <summary>
		/// Get the compactionThreshold for a namespace. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
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


        long? GetCompactionThreshold(string @namespace);

		/// <summary>
		/// Get the compactionThreshold for a namespace asynchronously. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<long> GetCompactionThresholdAsync(string @namespace);

		/// <summary>
		/// Set the compactionThreshold for a namespace. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
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


        void SetCompactionThreshold(string @namespace, long compactionThreshold);

		/// <summary>
		/// Set the compactionThreshold for a namespace asynchronously. The maximum number of bytes topics in the namespace
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="compactionThreshold">
		///            maximum number of backlog bytes before compaction is triggered </param>
		ValueTask SetCompactionThresholdAsync(string @namespace, long compactionThreshold);

		/// <summary>
		/// Delete the compactionThreshold for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>


        void RemoveCompactionThreshold(string @namespace);

		/// <summary>
		/// Delete the compactionThreshold for a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveCompactionThresholdAsync(string @namespace);

		/// <summary>
		/// Get the offloadThreshold for a namespace. The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// 
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
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


        long GetOffloadThreshold(string @namespace);

		/// <summary>
		/// Get the offloadThreshold for a namespace asynchronously.
		/// <p/>
		/// The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// 
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<long> GetOffloadThresholdAsync(string @namespace);

		/// <summary>
		/// Set the offloadThreshold for a namespace.
		/// <p/>
		/// The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// <p/>
		/// Negative values disabled automatic offloading. Setting a threshold of 0 will offload data as soon as possible.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
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


        void SetOffloadThreshold(string @namespace, long offloadThreshold);

		/// <summary>
		/// Set the offloadThreshold for a namespace asynchronously.
		/// <p/>
		/// The maximum number of bytes stored on the pulsar cluster for topics
		/// in the namespace before data starts being offloaded to longterm storage.
		/// <p/>
		/// Negative values disabled automatic offloading. Setting a threshold of 0 will offload data as soon as possible.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="offloadThreshold">
		///            maximum number of bytes stored before offloading is triggered </param>
		ValueTask SetOffloadThresholdAsync(string @namespace, long offloadThreshold);

		/// <summary>
		/// Get the offload deletion lag for a namespace, in milliseconds.
		/// The number of milliseconds to wait before deleting a ledger segment which has been offloaded from
		/// the Pulsar cluster's local storage (i.e. BookKeeper).
		/// <p/>
		/// If the offload deletion lag has not been set for the namespace, the method returns 'null'
		/// and the namespace will use the configured default of the pulsar broker.
		/// <p/>
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// 
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>3600000</code>
		/// </pre>
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


        long? GetOffloadDeleteLagMs(string @namespace);

		/// <summary>
		/// Get the offload deletion lag asynchronously for a namespace, in milliseconds.
		/// <p/>
		/// The number of milliseconds to wait before deleting a ledger segment which has been offloaded from
		/// the Pulsar cluster's local storage (i.e. BookKeeper).
		/// <p/>
		/// If the offload deletion lag has not been set for the namespace, the method returns 'null'
		/// and the namespace will use the configured default of the pulsar broker.
		/// <p/>
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// 
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>3600000</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> the offload deletion lag for the namespace in milliseconds, or null if not set </returns>
		ValueTask<long> GetOffloadDeleteLagMsAsync(string @namespace);

		/// <summary>
		/// Set the offload deletion lag for a namespace.
		/// <p/>
		/// The offload deletion lag is the amount of time to wait after offloading a ledger segment to long term storage,
		/// before deleting its copy stored on the Pulsar cluster's local storage (i.e. BookKeeper).
		/// <p/>
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="lag"> the duration to wait before deleting the local copy </param>
		/// <param name="unit"> the TimeSpan of the duration
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetOffloadDeleteLag(string @namespace, long lag, TimeSpan unit);

		/// <summary>
		/// Set the offload deletion lag for a namespace asynchronously.
		/// <p/>
		/// The offload deletion lag is the amount of time to wait after offloading a ledger segment to long term storage,
		/// before deleting its copy stored on the Pulsar cluster's local storage (i.e. BookKeeper).
		/// <p/>
		/// A negative value disables deletion of the local ledger completely, though it will still be deleted
		/// if it exceeds the topics retention policy, along with the offloaded copy.
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="lag"> the duration to wait before deleting the local copy </param>
		/// <param name="unit"> the TimeSpan of the duration </param>
		ValueTask SetOffloadDeleteLagAsync(string @namespace, long lag, TimeSpan unit);

		/// <summary>
		/// Clear the offload deletion lag for a namespace.
		/// <p/>
		/// The namespace will fall back to using the configured default of the pulsar broker.
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void ClearOffloadDeleteLag(string @namespace);

		/// <summary>
		/// Clear the offload deletion lag for a namespace asynchronously.
		/// <p/>
		/// The namespace will fall back to using the configured default of the pulsar broker.
		/// </summary>
		ValueTask ClearOffloadDeleteLagAsync(string @namespace);

	

		/// <summary>
		/// Get schema validation enforced for namespace. </summary>
		/// <param name="namespace"> namespace for this command. </param>
		/// <returns> the schema validation enforced flag </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        
        bool GetSchemaValidationEnforced(string @namespace);

		/// <summary>
		/// Get schema validation enforced for namespace asynchronously. </summary>
		/// <param name="namespace"> namespace for this command.
		/// </param>
		/// <returns> the schema validation enforced flag </returns>
		ValueTask<bool> GetSchemaValidationEnforcedAsync(string @namespace);

		/// <summary>
		/// Get schema validation enforced for namespace. </summary>
		/// <param name="namespace"> namespace for this command. </param>
		/// <param name="applied"> applied for this command. </param>
		/// <returns> the schema validation enforced flag </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

        
        bool GetSchemaValidationEnforced(string @namespace, bool applied);

		/// <summary>
		/// Get schema validation enforced for namespace asynchronously. </summary>
		/// <param name="namespace"> namespace for this command. </param>
		/// <param name="applied"> applied for this command.
		/// </param>
		/// <returns> the schema validation enforced flag </returns>
		ValueTask<bool> GetSchemaValidationEnforcedAsync(string @namespace, bool applied);

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

        
        void SetSchemaValidationEnforced(string @namespace, bool schemaValidationEnforced);

		/// <summary>
		/// Set schema validation enforced for namespace asynchronously.
		/// if a producer without a schema attempts to produce to a topic with schema in this the namespace, the
		/// producer will be failed to connect. PLEASE be carefully on using this, since non-java clients don't
		/// support schema. if you enable this setting, it will cause non-java clients failed to produce.
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <param name="schemaValidationEnforced"> flag to enable or disable schema validation for the given namespace </param>
		ValueTask SetSchemaValidationEnforcedAsync(string @namespace, bool schemaValidationEnforced);

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

        
        SchemaCompatibilityStrategy GetSchemaCompatibilityStrategy(string @namespace);

		/// <summary>
		/// Get the strategy used to check the a new schema provided by a producer is compatible with the current schema
		/// before it is installed asynchronously.
		/// </summary>
		/// <param name="namespace"> The namespace in whose policy we are interested </param>
		/// <returns> the strategy used to check compatibility </returns>
		ValueTask<SchemaCompatibilityStrategy> GetSchemaCompatibilityStrategyAsync(string @namespace);

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


        void SetSchemaCompatibilityStrategy(string @namespace, SchemaCompatibilityStrategy strategy);

		/// <summary>
		/// Set the strategy used to check the a new schema provided by a producer is compatible with the current schema
		/// before it is installed asynchronously.
		/// </summary>
		/// <param name="namespace"> The namespace in whose policy should be set </param>
		/// <param name="strategy"> The schema compatibility strategy </param>
		ValueTask SetSchemaCompatibilityStrategyAsync(string @namespace, SchemaCompatibilityStrategy strategy);

		/// <summary>
		/// Get whether allow auto update schema.
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <returns> the schema validation enforced flag </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Tenant or Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        bool GetIsAllowAutoUpdateSchema(string @namespace);

		/// <summary>
		/// Get whether allow auto update schema asynchronously.
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <returns> the schema validation enforced flag </returns>
		ValueTask<bool> GetIsAllowAutoUpdateSchemaAsync(string @namespace);

		/// <summary>
		/// Set whether to allow automatic schema updates.
		/// <p/>
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


        void SetIsAllowAutoUpdateSchema(string @namespace, bool isAllowAutoUpdateSchema);

		/// <summary>
		/// Set whether to allow automatic schema updates asynchronously.
		/// <p/>
		/// The flag is when producer bring a new schema and the schema pass compatibility check
		/// whether allow schema auto registered
		/// </summary>
		/// <param name="namespace"> pulsar namespace name </param>
		/// <param name="isAllowAutoUpdateSchema"> flag to enable or disable auto update schema </param>
		ValueTask SetIsAllowAutoUpdateSchemaAsync(string @namespace, bool isAllowAutoUpdateSchema);

		/// <summary>
		/// Set the offload configuration for all the topics in a namespace.
		/// <p/>
		/// Set the offload configuration in a namespace. This operation requires pulsar tenant access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "region" : "us-east-2",                   // The long term storage region
		///     "bucket" : "bucket",                      // Bucket to place offloaded ledger into
		///     "endpoint" : "endpoint",                  // Alternative endpoint to connect to
		///     "maxBlockSize" : 1024,                    // Max Block Size, default 64MB
		///     "readBufferSize" : 1024,                  // Read Buffer Size, default 1MB
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="offloadPolicies">
		///            Offload configuration
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
	
        void SetOffloadPolicies(string @namespace, OffloadPolicies offloadPolicies);

		/// <summary>
		/// Remove the offload configuration for a namespace.
		/// <p/>
		/// Remove the offload configuration in a namespace. This operation requires pulsar tenant access.
		/// <p/>
		/// </summary>
		/// <param name="namespace"> Namespace name </param>
		/// <exception cref="NotAuthorizedException"> Don't have admin permission </exception>
		/// <exception cref="NotFoundException">      Namespace does not exist </exception>
		/// <exception cref="ConflictException">      Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">   Unexpected error </exception>

        
        void RemoveOffloadPolicies(string @namespace);

		/// <summary>
		/// Set the offload configuration for all the topics in a namespace asynchronously.
		/// <p/>
		/// Set the offload configuration in a namespace. This operation requires pulsar tenant access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "region" : "us-east-2",                   // The long term storage region
		///     "bucket" : "bucket",                      // Bucket to place offloaded ledger into
		///     "endpoint" : "endpoint",                  // Alternative endpoint to connect to
		///     "maxBlockSize" : 1024,                    // Max Block Size, default 64MB
		///     "readBufferSize" : 1024,                  // Read Buffer Size, default 1MB
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="offloadPolicies">
		///            Offload configuration </param>
		ValueTask SetOffloadPoliciesAsync(string @namespace, OffloadPolicies offloadPolicies);

		/// <summary>
		/// Remove the offload configuration for a namespace asynchronously.
		/// <p/>
		/// Remove the offload configuration in a namespace. This operation requires pulsar tenant access.
		/// <p/>
		/// </summary>
		/// <param name="namespace"> Namespace name </param>
		/// <exception cref="NotAuthorizedException"> Don't have admin permission </exception>
		/// <exception cref="NotFoundException">      Namespace does not exist </exception>
		/// <exception cref="ConflictException">      Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">   Unexpected error </exception>
		ValueTask RemoveOffloadPoliciesAsync(string @namespace);

		/// <summary>
		/// Get the offload configuration for a namespace.
		/// <p/>
		/// Get the offload configuration for a namespace.
		/// <p/>
		/// Response example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "region" : "us-east-2",                   // The long term storage region
		///     "bucket" : "bucket",                      // Bucket to place offloaded ledger into
		///     "endpoint" : "endpoint",                  // Alternative endpoint to connect to
		///     "maxBlockSize" : 1024,                    // Max Block Size, default 64MB
		///     "readBufferSize" : 1024,                  // Read Buffer Size, default 1MB
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


        OffloadPolicies GetOffloadPolicies(string @namespace);

		/// <summary>
		/// Get the offload configuration for a namespace asynchronously.
		/// <p/>
		/// Get the offload configuration for a namespace.
		/// <p/>
		/// Response example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "region" : "us-east-2",                   // The long term storage region
		///     "bucket" : "bucket",                      // Bucket to place offloaded ledger into
		///     "endpoint" : "endpoint",                  // Alternative endpoint to connect to
		///     "maxBlockSize" : 1024,                    // Max Block Size, default 64MB
		///     "readBufferSize" : 1024,                  // Read Buffer Size, default 1MB
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<OffloadPolicies> GetOffloadPoliciesAsync(string @namespace);

		/// <summary>
		/// Get maxTopicsPerNamespace for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		///     <code>0</code>
		/// </pre> </summary>
		/// <param name="namespace">
		///              Namespace name
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///              Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///              Namespace dost not exist </exception>
		/// <exception cref="PulsarAdminException">
		///              Unexpected error </exception>


        int GetMaxTopicsPerNamespace(string @namespace);

		/// <summary>
		/// Get maxTopicsPerNamespace for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		///     <code>0</code>
		/// </pre> </summary>
		/// <param name="namespace">
		///          Namespace name
		/// @return </param>
		ValueTask<int> GetMaxTopicsPerNamespaceAsync(string @namespace);

		/// <summary>
		/// Set maxTopicsPerNamespace for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		///     <code>100</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="maxTopicsPerNamespace">
		///              maxTopicsPerNamespace value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///              Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///              Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///              Unexpected error </exception>


        void SetMaxTopicsPerNamespace(string @namespace, int maxTopicsPerNamespace);

		/// <summary>
		/// Set maxTopicsPerNamespace for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		///     <code>100</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="maxTopicsPerNamespace">
		///              maxTopicsPerNamespace value for a namespace
		/// @return </param>
		ValueTask SetMaxTopicsPerNamespaceAsync(string @namespace, int maxTopicsPerNamespace);

		/// <summary>
		/// remove maxTopicsPerNamespace for a namespace.
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <exception cref="NotAuthorizedException">
		///              Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///              Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///              Unexpected error </exception>


        void RemoveMaxTopicsPerNamespace(string @namespace);

		/// <summary>
		/// remove maxTopicsPerNamespace for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// @<exception cref="NotAuthorizedException">
		///              Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///              Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///              Unexpected error </exception>
		ValueTask RemoveMaxTopicsPerNamespaceAsync(string @namespace);

		/// <summary>
		/// Set key value pair property for a namespace.
		/// If the property absents, a new property will added. Otherwise, the new value will overwrite.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().setProperty("a", "a");
		///     admin.namespaces().setProperty("b", "b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property </param>
		/// <param name="value">
		///              value of the property </param>
		ValueTask SetPropertyAsync(string @namespace, string key, string value);

		/// <summary>
		/// Set key value pair property for a namespace.
		/// If the property absents, a new property will added. Otherwise, the new value will overwrite.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().setProperty("a", "a");
		///     admin.namespaces().setProperty("b", "b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property </param>
		/// <param name="value">
		///              value of the property </param>


        void SetProperty(string @namespace, string key, string value);

		/// <summary>
		/// Set key value pair properties for a namespace asynchronously.
		/// If the property absents, a new property will added. Otherwise, the new value will overwrite.
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="properties">
		///              key value pair properties </param>
		ValueTask SetPropertiesAsync(string @namespace, IDictionary<string, string> properties);

		/// <summary>
		/// Set key value pair properties for a namespace.
		/// If the property absents, a new property will added. Otherwise, the new value will overwrite.
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="properties">
		///              key value pair properties </param>


        void SetProperties(string @namespace, IDictionary<string, string> properties);

		/// <summary>
		/// Get property value for a given key.
		/// If the property absents, will return null.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().getProperty("a");
		///     admin.namespaces().getProperty("b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property
		/// </param>
		/// <returns> value of the property. </returns>
		ValueTask<string> GetPropertyAsync(string @namespace, string key);

		/// <summary>
		/// Get property value for a given key.
		/// If the property absents, will return null.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().getProperty("a");
		///     admin.namespaces().getProperty("b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property
		/// </param>
		/// <returns> value of the property. </returns>


        string GetProperty(string @namespace, string key);

		/// <summary>
		/// Get all properties of a namespace asynchronously.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().getPropertiesAsync();
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name
		/// </param>
		/// <returns> key value pair properties. </returns>
		ValueTask<IDictionary<string, string>> GetPropertiesAsync(string @namespace);

		/// <summary>
		/// Get all properties of a namespace.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().getProperties();
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name
		/// </param>
		/// <returns> key value pair properties. </returns>


        IDictionary<string, string> GetProperties(string @namespace);

		/// <summary>
		/// Remove a property for a given key.
		/// Return value of the property if the property exists, otherwise return null.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().removeProperty("a");
		///     admin.namespaces().removeProperty("b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property
		/// </param>
		/// <returns> value of the property. </returns>
		ValueTask<string> RemovePropertyAsync(string @namespace, string key);

		/// <summary>
		/// Remove a property for a given key.
		/// Return value of the property if the property exists, otherwise return null.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().removeProperty("a");
		///     admin.namespaces().removeProperty("b");
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		/// <param name="key">
		///              key of the property
		/// </param>
		/// <returns> value of the property. </returns>


        string RemoveProperty(string @namespace, string key);

		/// <summary>
		/// Clear all properties of a namespace asynchronously.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().clearPropertiesAsync();
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>
		ValueTask ClearPropertiesAsync(string @namespace);

		/// <summary>
		/// Clear all properties of a namespace.
		/// 
		/// <p/>
		/// Example:
		/// 
		/// <pre>
		///     admin.namespaces().clearProperties();
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///              Namespace name </param>


        void ClearProperties(string @namespace);

		/// <summary>
		/// Get the ResourceGroup for a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		/// @return </exception>


        string GetNamespaceResourceGroup(string @namespace);

		/// <summary>
		/// Get the ResourceGroup for a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		ValueTask<string> GetNamespaceResourceGroupAsync(string @namespace);

		/// <summary>
		/// Set the ResourceGroup for a namespace.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="resourcegroupname">
		///            ResourceGroup name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>


        void SetNamespaceResourceGroup(string @namespace, string resourcegroupname);

		/// <summary>
		/// Set the ResourceGroup for a namespace asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>60</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="resourcegroupname">
		///            TTL values for all messages for all topics in this namespace </param>
		ValueTask SetNamespaceResourceGroupAsync(string @namespace, string resourcegroupname);

		/// <summary>
		/// Remove the ResourceGroup on  a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

        
        void RemoveNamespaceResourceGroup(string @namespace);

		/// <summary>
		/// Remove the ResourceGroup on a namespace asynchronously. </summary>
		/// <param name="namespace">
		/// @return </param>
		ValueTask RemoveNamespaceResourceGroupAsync(string @namespace);

		/// <summary>
		/// Get entry filters for a namespace. </summary>
		/// <param name="namespace"> </param>
		/// <returns> entry filters classes info. </returns>
		/// <exception cref="PulsarAdminException"> </exception>


        EntryFilters GetNamespaceEntryFilters(string @namespace);

		/// <summary>
		/// Get entry filters for a namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <returns> entry filters classes info. </returns>
		ValueTask<EntryFilters> GetNamespaceEntryFiltersAsync(string @namespace);

		/// <summary>
		/// Set entry filters on a namespace.
		/// </summary>
		/// <param name="namespace">    Namespace name </param>
		/// <param name="entryFilters"> The entry filters </param>

        
        void SetNamespaceEntryFilters(string @namespace, EntryFilters entryFilters);

		/// <summary>
		/// Set entry filters on a namespace asynchronously.
		/// </summary>
		/// <param name="namespace">    Namespace name </param>
		/// <param name="entryFilters"> The entry filters </param>
		ValueTask SetNamespaceEntryFiltersAsync(string @namespace, EntryFilters entryFilters);

		/// <summary>
		/// remove entry filters of a namespace. </summary>
		/// <param name="namespace">    Namespace name </param>
		/// <exception cref="PulsarAdminException"> </exception>

        
        void RemoveNamespaceEntryFilters(string @namespace);

		/// <summary>
		/// remove entry filters of a namespace asynchronously. </summary>
		/// <param name="namespace">     Namespace name
		/// @return </param>
		ValueTask RemoveNamespaceEntryFiltersAsync(string @namespace);
	}

}