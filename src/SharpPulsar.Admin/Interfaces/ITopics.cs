using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces;

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
    /// Admin interface for Topics management.
    /// </summary>
    public interface ITopics
	{
		/// <summary>
		/// Get the both persistent and non-persistent topics under a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> a list of topics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getList(String namespace) throws PulsarAdminException;
		IList<string> GetList(string @namespace);

		/// <summary>
		/// Get the list of topics under a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <param name="topicDomain">
		///            use <seealso cref="TopicDomain.persistent"/> to get persistent topics
		///            use <seealso cref="TopicDomain.non_persistent"/> to get non-persistent topics
		///            Use null to get both persistent and non-persistent topics
		/// </param>
		/// <returns> a list of topics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getList(String namespace, org.apache.pulsar.common.naming.TopicDomain topicDomain) throws PulsarAdminException;
		IList<string> GetList(string @namespace, TopicDomain topicDomain);

		
		/// <summary>
		/// Get the list of topics under a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <param name="topicDomain">
		///            use <seealso cref="TopicDomain.persistent"/> to get persistent topics
		///            use <seealso cref="TopicDomain.non_persistent"/> to get non-persistent topics
		///            Use null to get both persistent and non-persistent topics
		/// </param>
		/// <param name="options">
		///            params to query the topics
		/// </param>
		/// <returns> a list of topics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getList(String namespace, org.apache.pulsar.common.naming.TopicDomain topicDomain, ListTopicsOptions options) throws PulsarAdminException;
		IList<string> GetList(string @namespace, TopicDomain topicDomain, ListTopicsOptions options);

		/// <summary>
		/// Get both persistent and non-persistent topics under a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> a list of topics </returns>
		ValueTask<IList<string>> GetListAsync(string @namespace);

		/// <summary>
		/// Get the list of topics under a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <param name="topicDomain">
		///            use <seealso cref="TopicDomain.persistent"/> to get persistent topics
		///            use <seealso cref="TopicDomain.non_persistent"/> to get non-persistent topics
		///            Use null to get both persistent and non-persistent topics
		/// </param>
		/// <returns> a list of topics </returns>
		ValueTask<IList<string>> GetListAsync(string @namespace, TopicDomain topicDomain);

		/// <summary>
		/// Get the list of topics under a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name
		/// </param>
		/// <param name="topicDomain">
		///            use <seealso cref="TopicDomain.persistent"/> to get persistent topics
		///            use <seealso cref="TopicDomain.non_persistent"/> to get non-persistent topics
		///            Use null to get both persistent and non-persistent topics
		/// </param>
		/// <param name="options">
		///            params to get the topics
		/// </param>
		/// <returns> a list of topics </returns>
		ValueTask<IList<string>> GetListAsync(string @namespace, TopicDomain topicDomain, ListTopicsOptions options);

		/// <summary>
		/// Get the list of partitioned topics under a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/my-namespace/topic-1",
		///  "persistent://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> a list of partitioned topics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getPartitionedTopicList(String namespace) throws PulsarAdminException;
		IList<string> GetPartitionedTopicList(string @namespace);

		/// <summary>
		/// Get the list of partitioned topics under a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/my-namespace/topic-1",
		///  "persistent://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <returns> a list of partitioned topics </returns>
		ValueTask<IList<string>> GetPartitionedTopicListAsync(string @namespace);

		/// <summary>
		/// Get the list of partitioned topics under a namespace.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/my-namespace/topic-1",
		///  "persistent://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="options">
		///            params to get the topics </param>
		/// <returns> a list of partitioned topics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getPartitionedTopicList(String namespace, ListTopicsOptions options) throws PulsarAdminException;
		IList<string> GetPartitionedTopicList(string @namespace, ListTopicsOptions options);

		/// <summary>
		/// Get the list of partitioned topics under a namespace asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/my-namespace/topic-1",
		///  "persistent://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// </summary>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="options">
		///           params to filter the results </param>
		/// <returns> a list of partitioned topics </returns>
		ValueTask<IList<string>> GetPartitionedTopicListAsync(string @namespace, ListTopicsOptions options);

		/// <summary>
		/// Get list of topics exist into given bundle.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundleRange">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.List<String> getListInBundle(String namespace, String bundleRange) throws PulsarAdminException;
		IList<string> GetListInBundle(string @namespace, string bundleRange);

		/// <summary>
		/// Get list of topics exist into given bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundleRange">
		/// @return </param>
		ValueTask<IList<string>> GetListInBundleAsync(string @namespace, string bundleRange);

		/// <summary>
		/// Get permissions on a topic.
		/// <p/>
		/// Retrieve the effective permissions for a topic. These permissions are defined by the permissions set at the
		/// namespace level combined (union) with any eventual specific permission set on the topic.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{
		///   "role-1" : [ "produce" ],
		///   "role-2" : [ "consume" ]
		/// }</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
		/// <returns> a map of topics an their permissions set
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String topic) throws PulsarAdminException;
		IDictionary<string, ISet<AuthAction>> GetPermissions(string topic);

		/// <summary>
		/// Get permissions on a topic asynchronously.
		/// <p/>
		/// Retrieve the effective permissions for a topic. These permissions are defined by the permissions set at the
		/// namespace level combined (union) with any eventual specific permission set on the topic.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{
		///   "role-1" : [ "produce" ],
		///   "role-2" : [ "consume" ]
		/// }</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
		/// <returns> a map of topics an their permissions set </returns>
		ValueTask<IDictionary<string, ISet<AuthAction>>> GetPermissionsAsync(string topic);

		/// <summary>
		/// Grant permission on a topic.
		/// <p/>
		/// Grant a new permission to a client role on a single topic.
		/// <p/>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>["produce", "consume"]</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
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

// ORIGINAL LINE: void grantPermission(String topic, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws PulsarAdminException;
		void GrantPermission(string topic, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Grant permission on a topic asynchronously.
		/// <p/>
		/// Grant a new permission to a client role on a single topic.
		/// <p/>
		/// Request parameter example:
		/// 
		/// <pre>
		/// <code>["produce", "consume"]</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
		/// <param name="role">
		///            Client role to which grant permission </param>
		/// <param name="actions">
		///            Auth actions (produce and consume) </param>
		ValueTask GrantPermissionAsync(string topic, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Revoke permissions on a topic.
		/// <p/>
		/// Revoke permissions to a client role on a single topic. If the permission was not set at the topic level, but
		/// rather at the namespace level, this operation will return an error (HTTP status code 412).
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
		/// <param name="role">
		///            Client role to which remove permission </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Permissions are not set at the topic level </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void revokePermissions(String topic, String role) throws PulsarAdminException;
		void RevokePermissions(string topic, string role);

		/// <summary>
		/// Revoke permissions on a topic asynchronously.
		/// <p/>
		/// Revoke permissions to a client role on a single topic. If the permission was not set at the topic level, but
		/// rather at the namespace level, this operation will return an error (HTTP status code 412).
		/// </summary>
		/// <param name="topic">
		///            Topic url </param>
		/// <param name="role">
		///            Client role to which remove permission </param>
		ValueTask RevokePermissionsAsync(string topic, string role);

		/// <summary>
		/// Create a partitioned topic.
		/// <p/>
		/// Create a partitioned topic. It needs to be called before creating a producer for a partitioned topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: default void createPartitionedTopic(String topic, int numPartitions) throws PulsarAdminException
		void CreatePartitionedTopic(string topic, int numPartitions)
		{
			CreatePartitionedTopic(topic, numPartitions, null);
		}

		/// <summary>
		/// Create a partitioned topic.
		/// <p/>
		/// Create a partitioned topic. It needs to be called before creating a producer for a partitioned topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <param name="properties">
		///            topic properties </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void createPartitionedTopic(String topic, int numPartitions, java.util.Map<String, String> properties) throws PulsarAdminException;
		void CreatePartitionedTopic(string topic, int numPartitions, IDictionary<string, string> properties);

		/// <summary>
		/// Create a partitioned topic asynchronously.
		/// <p/>
		/// Create a partitioned topic asynchronously. It needs to be called before creating a producer for a partitioned
		/// topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <returns> a future that can be used to track when the partitioned topic is created </returns>
		ValueTask CreatePartitionedTopicAsync(string topic, int numPartitions)
		{
			return CreatePartitionedTopicAsync(topic, numPartitions, null);
		}

		/// <summary>
		/// Create a partitioned topic asynchronously.
		/// <p/>
		/// Create a partitioned topic asynchronously. It needs to be called before creating a producer for a partitioned
		/// topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <param name="properties">
		///            Topic properties </param>
		/// <returns> a future that can be used to track when the partitioned topic is created </returns>
		ValueTask CreatePartitionedTopicAsync(string topic, int numPartitions, IDictionary<string, string> properties);

		/// <summary>
		/// Create a non-partitioned topic.
		/// <p/>
		/// Create a non-partitioned topic.
		/// <p/>
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: default void createNonPartitionedTopic(String topic) throws PulsarAdminException
		void CreateNonPartitionedTopic(string topic)
		{
			CreateNonPartitionedTopic(topic, null);
		}

		/// <summary>
		/// Create a non-partitioned topic.
		/// <p/>
		/// Create a non-partitioned topic.
		/// <p/>
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="properties"> Topic properties </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void createNonPartitionedTopic(String topic, java.util.Map<String, String> properties) throws PulsarAdminException;
		void CreateNonPartitionedTopic(string topic, IDictionary<string, string> properties);

		/// <summary>
		/// Create a non-partitioned topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask CreateNonPartitionedTopicAsync(string topic)
		{
			return CreateNonPartitionedTopicAsync(topic, null);
		}

		/// <summary>
		/// Create a non-partitioned topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="properties"> Topic properties </param>
		ValueTask CreateNonPartitionedTopicAsync(string topic, IDictionary<string, string> properties);

		/// <summary>
		/// Create missed partitions for partitioned topic.
		/// <p/>
		/// When disable topic auto creation, use this method to try create missed partitions while
		/// partitions create failed or users already have partitioned topic without partitions.
		/// </summary>
		/// <param name="topic"> partitioned topic name </param>

// ORIGINAL LINE: void createMissedPartitions(String topic) throws PulsarAdminException;
		void CreateMissedPartitions(string topic);

		/// <summary>
		/// Create missed partitions for partitioned topic asynchronously.
		/// <p/>
		/// When disable topic auto creation, use this method to try create missed partitions while
		/// partitions create failed or users already have partitioned topic without partitions.
		/// </summary>
		/// <param name="topic"> partitioned topic name </param>
		ValueTask CreateMissedPartitionsAsync(string topic);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic
		/// 
		/// @returns a future that can be used to track when the partitioned topic is updated. </param>

// ORIGINAL LINE: void updatePartitionedTopic(String topic, int numPartitions) throws PulsarAdminException;
		void UpdatePartitionedTopic(string topic, int numPartitions);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic asynchronously.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic
		/// </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
		ValueTask UpdatePartitionedTopicAsync(string topic, int numPartitions);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic </param>
		/// <param name="updateLocalTopicOnly">
		///            Used by broker for global topic with multiple replicated clusters </param>
		/// <param name="force">
		///            Update forcefully without validating existing partitioned topic
		/// @returns a future that can be used to track when the partitioned topic is updated </param>

// ORIGINAL LINE: void updatePartitionedTopic(String topic, int numPartitions, boolean updateLocalTopicOnly, boolean force) throws PulsarAdminException;
		void UpdatePartitionedTopic(string topic, int numPartitions, bool updateLocalTopicOnly, bool force);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic asynchronously.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic </param>
		/// <param name="updateLocalTopicOnly">
		///            Used by broker for global topic with multiple replicated clusters </param>
		/// <param name="force">
		///            Update forcefully without validating existing partitioned topic </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
		ValueTask UpdatePartitionedTopicAsync(string topic, int numPartitions, bool updateLocalTopicOnly, bool force);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic </param>
		/// <param name="updateLocalTopicOnly">
		///            Used by broker for global topic with multiple replicated clusters
		/// @returns a future that can be used to track when the partitioned topic is updated </param>

// ORIGINAL LINE: void updatePartitionedTopic(String topic, int numPartitions, boolean updateLocalTopicOnly) throws PulsarAdminException;
		void UpdatePartitionedTopic(string topic, int numPartitions, bool updateLocalTopicOnly);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic asynchronously.
		/// <p/>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic </param>
		/// <param name="updateLocalTopicOnly">
		///            Used by broker for global topic with multiple replicated clusters </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
		ValueTask UpdatePartitionedTopicAsync(string topic, int numPartitions, bool updateLocalTopicOnly);

		/// <summary>
		/// Get metadata of a partitioned topic.
		/// <p/>
		/// Get metadata of a partitioned topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> Partitioned topic metadata </returns>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.partition.PartitionedTopicMetadata getPartitionedTopicMetadata(String topic) throws PulsarAdminException;
		PartitionedTopicMetadata GetPartitionedTopicMetadata(string topic);

		/// <summary>
		/// Get metadata of a partitioned topic asynchronously.
		/// <p/>
		/// Get metadata of a partitioned topic asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> a future that can be used to track when the partitioned topic metadata is returned </returns>
		ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadataAsync(string topic);

		/// <summary>
		/// Get properties of a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> Topic properties </returns>

// ORIGINAL LINE: java.util.Map<String, String> getProperties(String topic) throws PulsarAdminException;
		IDictionary<string, string> GetProperties(string topic);

		/// <summary>
		/// Get properties of a topic asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> a future that can be used to track when the topic properties is returned </returns>
		ValueTask<IDictionary<string, string>> GetPropertiesAsync(string topic);

		/// <summary>
		/// Update Topic Properties on a topic.
		/// The new properties will override the existing values, old properties in the topic will be keep if not override. </summary>
		/// <param name="topic"> </param>
		/// <param name="properties"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void updateProperties(String topic, java.util.Map<String, String> properties) throws PulsarAdminException;
		void UpdateProperties(string topic, IDictionary<string, string> properties);

		/// <summary>
		/// Update Topic Properties on a topic.
		/// The new properties will override the existing values, old properties in the topic will be keep if not override. </summary>
		/// <param name="topic"> </param>
		/// <param name="properties">
		/// @return </param>
		ValueTask UpdatePropertiesAsync(string topic, IDictionary<string, string> properties);

		/// <summary>
		/// Remove the key in properties on a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="key"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeProperties(String topic, String key) throws PulsarAdminException;
		void RemoveProperties(string topic, string key);

		/// <summary>
		/// Remove the key in properties on a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="key">
		/// @return </param>
		ValueTask RemovePropertiesAsync(string topic, string key);

        /// <summary>
        /// Delete a partitioned topic and its schemas.
        /// <p/>
        /// It will also delete all the partitions of the topic if it exists.
        /// <p/>
        /// </summary>
        /// <param name="topic">
        ///            Topic name </param>
        /// <param name="force">
        ///            Delete topic forcefully </param>
        /// <exception cref="PulsarAdminException"> </exception>
        void DeletePartitionedTopic(string topic, bool force);

        /// <summary>
        /// Delete a partitioned topic and its schemas asynchronously.
        /// <p/>
        /// It will also delete all the partitions of the topic if it exists.
        /// <p/>
        /// </summary>
        /// <param name="topic">
        ///            Topic name </param>
        /// <param name="force">
        ///            Delete topic forcefully </param>
        /// <returns> a future that can be used to track when the partitioned topic is deleted </returns>
        ValueTask DeletePartitionedTopicAsync(string topic, bool force);

		/// <summary>
		/// Delete a partitioned topic.
		/// <p/>
		/// It will also delete all the partitions of the topic if it exists.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void deletePartitionedTopic(String topic) throws PulsarAdminException;
		void DeletePartitionedTopic(string topic);

		/// <summary>
		/// Delete a partitioned topic asynchronously.
		/// <p/>
		/// It will also delete all the partitions of the topic if it exists.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask DeletePartitionedTopicAsync(string topic);

        /// <summary>
        /// Delete a topic and its schemas.
        /// <p/>
        /// Delete a topic. The topic cannot be deleted if force flag is disable and there's any active
        /// subscription or producer connected to the it. Force flag deletes topic forcefully by closing
        /// all active producers and consumers.
        /// <p/>
        /// </summary>
        /// <param name="topic">
        ///            Topic name </param>
        /// <param name="force">
        ///            Delete topic forcefully </param>
        /// <exception cref="NotAuthorizedException">
        ///             Don't have admin permission </exception>
        /// <exception cref="NotFoundException">
        ///             Topic does not exist </exception>
        /// <exception cref="PreconditionFailedException">
        ///             Topic has active subscriptions or producers </exception>
        /// <exception cref="PulsarAdminException">
        ///             Unexpected error </exception>
        void Delete(string topic, bool force);

        /// <summary>
        /// Delete a topic and its schemas asynchronously.
        /// <p/>
        /// Delete a topic asynchronously. The topic cannot be deleted if force flag is disable and there's any active
        /// subscription or producer connected to the it. Force flag deletes topic forcefully by closing all active producers
        /// and consumers.
        /// <p/>
        /// </summary>
        /// <param name="topic">
        ///            topic name </param>
        /// <param name="force">
        ///            Delete topic forcefully </param>
        /// <param name="deleteSchema">
        ///            Delete topic's schema storage and it is always true even if it is specified as false
        /// </param>
        /// <returns> a future that can be used to track when the topic is deleted </returns>
        ValueTask DeleteAsync(string topic, bool force);

		/// <summary>
		/// Delete a topic.
		/// <p/>
		/// Delete a topic. The topic cannot be deleted if there's any active subscription or producer connected to the it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Topic has active subscriptions or producers </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void delete(String topic) throws PulsarAdminException;
		void Delete(string topic);

		/// <summary>
		/// Delete a topic asynchronously.
		/// <p/>
		/// Delete a topic. The topic cannot be deleted if there's any active subscription or producer connected to the it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask DeleteAsync(string topic);

		/// <summary>
		/// Unload a topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void unload(String topic) throws PulsarAdminException;
		void Unload(string topic);

		/// <summary>
		/// Unload a topic asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic is unloaded </returns>
		ValueTask UnloadAsync(string topic);

		/// <summary>
		/// Terminate the topic and prevent any more messages being published on it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the message id of the last message that was published in the topic
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.client.api.MessageId terminateTopic(String topic) throws PulsarAdminException;
		MessageId TerminateTopic(string topic);

		/// <summary>
		/// Terminate the topic and prevent any more messages being published on it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the message id of the last message that was published in the topic </returns>
		ValueTask<MessageId> TerminateTopicAsync(string topic);

		/// <summary>
		/// Terminate the partitioned topic and prevent any more messages being published on it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the message id of the last message that was published in the each partition of topic </returns>

// ORIGINAL LINE: java.util.Map<int, org.apache.pulsar.client.api.MessageId> terminatePartitionedTopic(String topic) throws PulsarAdminException;
		IDictionary<int, MessageId> TerminatePartitionedTopic(string topic);

		/// <summary>
		/// Terminate the partitioned topic and prevent any more messages being published on it.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the message id of the last message that was published in the each partition of topic </returns>
		ValueTask<IDictionary<int, MessageId>> TerminatePartitionedTopicAsync(string topic);

		/// <summary>
		/// Get the list of subscriptions.
		/// <p/>
		/// Get the list of persistent subscriptions for a given topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the list of subscriptions
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<String> getSubscriptions(String topic) throws PulsarAdminException;
		IList<string> GetSubscriptions(string topic);

		/// <summary>
		/// Get the list of subscriptions asynchronously.
		/// <p/>
		/// Get the list of persistent subscriptions for a given topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> a future that can be used to track when the list of subscriptions is returned </returns>
		ValueTask<IList<string>> GetSubscriptionsAsync(string topic);

		/// <summary>
		/// Get the stats for the topic.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///   "msgRateIn" : 100.0,                    // Total rate of messages published on the topic. msg/s
		///   "msgThroughputIn" : 10240.0,            // Total throughput of messages published on the topic. byte/s
		///   "msgRateOut" : 100.0,                   // Total rate of messages delivered on the topic. msg/s
		///   "msgThroughputOut" : 10240.0,           // Total throughput of messages delivered on the topic. byte/s
		///   "averageMsgSize" : 1024.0,              // Average size of published messages. bytes
		///   "publishers" : [                        // List of publishes on this topic with their stats
		///      {
		///          "producerId" : 10                // producer id
		///          "address"   : 10.4.1.23:3425     // IP and port for this producer
		///          "connectedSince" : 2014-11-21 23:54:46 // Timestamp of this published connection
		///          "msgRateIn" : 100.0,             // Total rate of messages published by this producer. msg/s
		///          "msgThroughputIn" : 10240.0,     // Total throughput of messages published by this producer. byte/s
		///          "averageMsgSize" : 1024.0,       // Average size of published messages by this producer. bytes
		///      },
		///   ],
		///   "subscriptions" : {                     // Map of subscriptions on this topic
		///     "sub1" : {
		///       "msgRateOut" : 100.0,               // Total rate of messages delivered on this subscription. msg/s
		///       "msgThroughputOut" : 10240.0,       // Total throughput delivered on this subscription. bytes/s
		///       "msgBacklog" : 0,                   // Number of messages in the subscriotion backlog
		///       "type" : Exclusive                  // Whether the subscription is exclusive or shared
		///       "consumers" [                       // List of consumers on this subscription
		///          {
		///              "id" : 5                            // Consumer id
		///              "address" : 10.4.1.23:3425          // IP and port for this consumer
		///              "connectedSince" : 2014-11-21 23:54:46 // Timestamp of this consumer connection
		///              "msgRateOut" : 100.0,               // Total rate of messages delivered to this consumer. msg/s
		///              "msgThroughputOut" : 10240.0,       // Total throughput delivered to this consumer. bytes/s
		///          }
		///       ],
		///   },
		///   "replication" : {                    // Replication statistics
		///     "cluster_1" : {                    // Cluster name in the context of from-cluster or to-cluster
		///       "msgRateIn" : 100.0,             // Total rate of messages received from this remote cluster. msg/s
		///       "msgThroughputIn" : 10240.0,     // Total throughput received from this remote cluster. bytes/s
		///       "msgRateOut" : 100.0,            // Total rate of messages delivered to the replication-subscriber. msg/s
		///       "msgThroughputOut" : 10240.0,    // Total throughput delivered to the replication-subscriber. bytes/s
		///       "replicationBacklog" : 0,        // Number of messages pending to be replicated to this remote cluster
		///       "connected" : true,              // Whether the replication-subscriber is currently connected locally
		///     },
		///     "cluster_2" : {
		///       "msgRateIn" : 100.0,
		///       "msgThroughputIn" : 10240.0,
		///       "msgRateOut" : 100.0,
		///       "msgThroughputOut" : 10240.0,
		///       "replicationBacklog" : 0,
		///       "connected" : true,
		///     }
		///   },
		/// }
		/// </code>
		/// </pre>
		/// 
		/// <para>All the rates are computed over a 1 minute window and are relative the last completed 1 minute period.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="getStatsOptions">
		///            get stats options </param>
		/// <returns> the topic statistics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TopicStats getStats(String topic, GetStatsOptions getStatsOptions) throws PulsarAdminException;
		TopicStats GetStats(string topic, GetStatsOptions getStatsOptions);



// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TopicStats getStats(String topic, boolean getPreciseBacklog, boolean subscriptionBacklogSize, boolean getEarliestTimeInBacklog) throws PulsarAdminException
		TopicStats GetStats(string topic, bool getPreciseBacklog, bool subscriptionBacklogSize, bool getEarliestTimeInBacklog)
		{
			GetStatsOptions getStatsOptions = new GetStatsOptions { GetEarliestTimeInBacklog = getEarliestTimeInBacklog,SubscriptionBacklogSize = subscriptionBacklogSize, GetPreciseBacklog = getPreciseBacklog };
			return GetStats(topic, getStatsOptions);
		}


// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TopicStats getStats(String topic, boolean getPreciseBacklog, boolean subscriptionBacklogSize) throws PulsarAdminException
		TopicStats GetStats(string topic, bool getPreciseBacklog, bool subscriptionBacklogSize)
		{
            GetStatsOptions getStatsOptions = new GetStatsOptions { GetEarliestTimeInBacklog = false, SubscriptionBacklogSize = subscriptionBacklogSize, GetPreciseBacklog = getPreciseBacklog };
            return GetStats(topic, getStatsOptions);
		}


// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TopicStats getStats(String topic, boolean getPreciseBacklog) throws PulsarAdminException
		TopicStats GetStats(string topic, bool getPreciseBacklog)
		{
            GetStatsOptions getStatsOptions = new GetStatsOptions { GetEarliestTimeInBacklog = false, SubscriptionBacklogSize = false, GetPreciseBacklog = getPreciseBacklog };

            return GetStats(topic, getStatsOptions);
		}


// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TopicStats getStats(String topic) throws PulsarAdminException
		TopicStats GetStats(string topic)
		{
			return GetStats(topic, new GetStatsOptions { GetEarliestTimeInBacklog = false, SubscriptionBacklogSize = false, GetPreciseBacklog = false});
		}

		/// <summary>
		/// Get the stats for the topic asynchronously. All the rates are computed over a 1 minute window and are relative
		/// the last completed 1 minute period.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="getPreciseBacklog">
		///            Set to true to get precise backlog, Otherwise get imprecise backlog. </param>
		/// <param name="subscriptionBacklogSize">
		///            Whether to get backlog size for each subscription. </param>
		/// <param name="getEarliestTimeInBacklog">
		///            Whether to get the earliest time in backlog. </param>
		/// <returns> a future that can be used to track when the topic statistics are returned
		///  </returns>
		ValueTask<TopicStats> GetStatsAsync(string topic, bool getPreciseBacklog, bool subscriptionBacklogSize, bool getEarliestTimeInBacklog);

		ValueTask<TopicStats> GetStatsAsync(string topic)
		{
			return GetStatsAsync(topic, false, false, false);
		}

		/// <summary>
		/// Get the internal stats for the topic.
		/// <p/>
		/// Access the internal state of the topic
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="metadata">
		///            flag to include ledger metadata </param>
		/// <returns> the topic statistics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistentTopicInternalStats getInternalStats(String topic, boolean metadata) throws PulsarAdminException;
		PersistentTopicInternalStats GetInternalStats(string topic, bool metadata);

		/// <summary>
		/// Get the internal stats for the topic.
		/// <p/>
		/// Access the internal state of the topic
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the topic statistics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistentTopicInternalStats getInternalStats(String topic) throws PulsarAdminException;
		PersistentTopicInternalStats GetInternalStats(string topic);

		/// <summary>
		/// Get the internal stats for the topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <param name="metadata">
		///            flag to include ledger metadata </param>
		/// <returns> a future that can be used to track when the internal topic statistics are returned </returns>
		ValueTask<PersistentTopicInternalStats> GetInternalStatsAsync(string topic, bool metadata);

		/// <summary>
		/// Get the internal stats for the topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <returns> a future that can be used to track when the internal topic statistics are returned </returns>
		ValueTask<PersistentTopicInternalStats> GetInternalStatsAsync(string topic);

		/// <summary>
		/// Get a JSON representation of the topic metadata stored in ZooKeeper.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the topic internal metadata </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: String getInternalInfo(String topic) throws PulsarAdminException;
		string GetInternalInfo(string topic);

		/// <summary>
		/// Get a JSON representation of the topic metadata stored in ZooKeeper.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> a future to receive the topic internal metadata </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		ValueTask<string> GetInternalInfoAsync(string topic);

		/// <summary>
		/// Get the stats for the partitioned topic
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///   "msgRateIn" : 100.0,                 // Total rate of messages published on the partitioned topic. msg/s
		///   "msgThroughputIn" : 10240.0,         // Total throughput of messages published on the partitioned topic. byte/s
		///   "msgRateOut" : 100.0,                // Total rate of messages delivered on the partitioned topic. msg/s
		///   "msgThroughputOut" : 10240.0,        // Total throughput of messages delivered on the partitioned topic. byte/s
		///   "averageMsgSize" : 1024.0,           // Average size of published messages. bytes
		///   "publishers" : [                     // List of publishes on this partitioned topic with their stats
		///      {
		///          "msgRateIn" : 100.0,          // Total rate of messages published by this producer. msg/s
		///          "msgThroughputIn" : 10240.0,  // Total throughput of messages published by this producer. byte/s
		///          "averageMsgSize" : 1024.0,    // Average size of published messages by this producer. bytes
		///      },
		///   ],
		///   "subscriptions" : {                  // Map of subscriptions on this topic
		///     "sub1" : {
		///       "msgRateOut" : 100.0,            // Total rate of messages delivered on this subscription. msg/s
		///       "msgThroughputOut" : 10240.0,    // Total throughput delivered on this subscription. bytes/s
		///       "msgBacklog" : 0,                // Number of messages in the subscriotion backlog
		///       "type" : Exclusive               // Whether the subscription is exclusive or shared
		///       "consumers" [                    // List of consumers on this subscription
		///          {
		///              "msgRateOut" : 100.0,               // Total rate of messages delivered to this consumer. msg/s
		///              "msgThroughputOut" : 10240.0,       // Total throughput delivered to this consumer. bytes/s
		///          }
		///       ],
		///   },
		///   "replication" : {                    // Replication statistics
		///     "cluster_1" : {                    // Cluster name in the context of from-cluster or to-cluster
		///       "msgRateIn" : 100.0,             // Total rate of messages received from this remote cluster. msg/s
		///       "msgThroughputIn" : 10240.0,     // Total throughput received from this remote cluster. bytes/s
		///       "msgRateOut" : 100.0,            // Total rate of messages delivered to the replication-subscriber. msg/s
		///       "msgThroughputOut" : 10240.0,    // Total throughput delivered to the replication-subscriber. bytes/s
		///       "replicationBacklog" : 0,        // Number of messages pending to be replicated to this remote cluster
		///       "connected" : true,              // Whether the replication-subscriber is currently connected locally
		///     },
		///     "cluster_2" : {
		///       "msgRateIn" : 100.0,
		///       "msgThroughputIn" : 10240.0,
		///       "msgRateOut" : 100.0,
		///       "msghroughputOut" : 10240.0,
		///       "replicationBacklog" : 0,
		///       "connected" : true,
		///     }
		///   },
		/// }
		/// </code>
		/// </pre>
		/// 
		/// <para>All the rates are computed over a 1 minute window and are relative the last completed 1 minute period.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="perPartition">
		///            flag to get stats per partition </param>
		/// <param name="getPreciseBacklog">
		///            Set to true to get precise backlog, Otherwise get imprecise backlog. </param>
		/// <param name="subscriptionBacklogSize">
		///            Whether to get backlog size for each subscription. </param>
		/// <returns> the partitioned topic statistics </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PartitionedTopicStats getPartitionedStats(String topic, boolean perPartition, boolean getPreciseBacklog, boolean subscriptionBacklogSize, boolean getEarliestTimeInBacklog) throws PulsarAdminException;
		PartitionedTopicStats GetPartitionedStats(string topic, bool perPartition, bool getPreciseBacklog, bool subscriptionBacklogSize, bool getEarliestTimeInBacklog);


// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.PartitionedTopicStats getPartitionedStats(String topic, boolean perPartition) throws PulsarAdminException
		PartitionedTopicStats GetPartitionedStats(string topic, bool perPartition)
		{
			return GetPartitionedStats(topic, perPartition, false, false, false);
		}

		/// <summary>
		/// Get the stats for the partitioned topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <param name="perPartition">
		///            flag to get stats per partition </param>
		/// <param name="getPreciseBacklog">
		///            Set to true to get precise backlog, Otherwise get imprecise backlog. </param>
		/// <param name="subscriptionBacklogSize">
		///            Whether to get backlog size for each subscription. </param>
		/// <returns> a future that can be used to track when the partitioned topic statistics are returned </returns>
		ValueTask<PartitionedTopicStats> GetPartitionedStatsAsync(string topic, bool perPartition, bool getPreciseBacklog, bool subscriptionBacklogSize, bool getEarliestTimeInBacklog);

		ValueTask<PartitionedTopicStats> GetPartitionedStatsAsync(string topic, bool perPartition)
		{
			return GetPartitionedStatsAsync(topic, perPartition, false, false, false);
		}

		/// <summary>
		/// Get the stats for the partitioned topic.
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats getPartitionedInternalStats(String topic) throws PulsarAdminException;
		PartitionedTopicInternalStats GetPartitionedInternalStats(string topic);

		/// <summary>
		/// Get the stats-internal for the partitioned topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <returns> a future that can be used to track when the partitioned topic statistics are returned </returns>
		ValueTask<PartitionedTopicInternalStats> GetPartitionedInternalStatsAsync(string topic);

		/// <summary>
		/// Delete a subscription.
		/// <p/>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Subscription has active consumers </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void deleteSubscription(String topic, String subName) throws PulsarAdminException;
		void DeleteSubscription(string topic, string subName);

		/// <summary>
		/// Delete a subscription.
		/// <p/>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// Force flag deletes subscription forcefully by closing all active consumers.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Subscription has active consumers </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void deleteSubscription(String topic, String subName, boolean force) throws PulsarAdminException;
		void DeleteSubscription(string topic, string subName, bool force);

		/// <summary>
		/// Delete a subscription asynchronously.
		/// <p/>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <returns> a future that can be used to track when the subscription is deleted </returns>
		ValueTask DeleteSubscriptionAsync(string topic, string subName);

		/// <summary>
		/// Delete a subscription asynchronously.
		/// <p/>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// Force flag deletes subscription forcefully by closing all active consumers.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <returns> a future that can be used to track when the subscription is deleted </returns>
		ValueTask DeleteSubscriptionAsync(string topic, string subName, bool force);

		/// <summary>
		/// Skip all messages on a topic subscription.
		/// <p/>
		/// Completely clears the backlog on the subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void skipAllMessages(String topic, String subName) throws PulsarAdminException;
		void SkipAllMessages(string topic, string subName);

		/// <summary>
		/// Skip all messages on a topic subscription asynchronously.
		/// <p/>
		/// Completely clears the backlog on the subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <returns> a future that can be used to track when all the messages are skipped </returns>
		ValueTask SkipAllMessagesAsync(string topic, string subName);

		/// <summary>
		/// Skip messages on a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="numMessages">
		///            Number of messages
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void skipMessages(String topic, String subName, long numMessages) throws PulsarAdminException;
		void SkipMessages(string topic, string subName, long numMessages);

		/// <summary>
		/// Skip messages on a topic subscription asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="numMessages">
		///            Number of messages
		/// </param>
		/// <returns> a future that can be used to track when the number of messages are skipped </returns>
		ValueTask SkipMessagesAsync(string topic, string subName, long numMessages);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void expireMessages(String topic, String subscriptionName, long expireTimeInSeconds) throws PulsarAdminException;
		void ExpireMessages(string topic, string subscriptionName, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds
		/// @return </param>
		ValueTask ExpireMessagesAsync(string topic, string subscriptionName, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            Position before which all messages will be expired. </param>
		/// <param name="isExcluded">
		///            Will message at passed in position also be expired. </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void expireMessages(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId, boolean isExcluded) throws PulsarAdminException;
		void ExpireMessages(string topic, string subscriptionName, MessageId messageId, bool isExcluded);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            Position before which all messages will be expired. </param>
		/// <param name="isExcluded">
		///            Will message at passed in position also be expired.
		/// @return
		///            A <seealso cref="ValueTask"/> that'll be completed when expire message is done. </param>
		ValueTask ExpireMessagesAsync(string topic, string subscriptionName, MessageId messageId, bool isExcluded);

		/// <summary>
		/// Expire all messages older than given N seconds for all subscriptions of the persistent-topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void expireMessagesForAllSubscriptions(String topic, long expireTimeInSeconds) throws PulsarAdminException;
		void ExpireMessagesForAllSubscriptions(string topic, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N seconds for all subscriptions of the persistent-topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		ValueTask ExpireMessagesForAllSubscriptionsAsync(string topic, long expireTimeInSeconds);

		/// <summary>
		/// Peek messages from a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="numMessages">
		///            Number of messages
		/// @return </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.List<org.apache.pulsar.client.api.Message<byte[]>> peekMessages(String topic, String subName, int numMessages) throws PulsarAdminException;
		IList<IMessage<byte[]>> PeekMessages(string topic, string subName, int numMessages);

		/// <summary>
		/// Peek messages from a topic subscription asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="numMessages">
		///            Number of messages </param>
		/// <returns> a future that can be used to track when the messages are returned </returns>
		ValueTask<IList<IMessage<byte[]>>> PeekMessagesAsync(string topic, string subName, int numMessages);

		/// <summary>
		/// Get a message by its messageId via a topic subscription. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="ledgerId">
		///            Ledger id </param>
		/// <param name="entryId">
		///            Entry id </param>
		/// <returns> the message indexed by the messageId </returns>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.client.api.Message<byte[]> getMessageById(String topic, long ledgerId, long entryId) throws PulsarAdminException;
		IMessage<byte[]> GetMessageById(string topic, long ledgerId, long entryId);

		/// <summary>
		/// Get a message by its messageId via a topic subscription asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="ledgerId">
		///            Ledger id </param>
		/// <param name="entryId">
		///            Entry id </param>
		/// <returns> a future that can be used to track when the message is returned </returns>
		ValueTask<IMessage<byte[]>> GetMessageByIdAsync(string topic, long ledgerId, long entryId);

		/// <summary>
		/// Get message ID published at or just after this absolute timestamp (in ms). </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="timestamp">
		///            Timestamp </param>
		/// <returns> MessageId </returns>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.client.api.MessageId getMessageIdByTimestamp(String topic, long timestamp) throws PulsarAdminException;
		IMessageId GetMessageIdByTimestamp(string topic, long timestamp);

		/// <summary>
		/// Get message ID published at or just after this absolute timestamp (in ms) asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="timestamp">
		///            Timestamp </param>
		/// <returns> a future that can be used to track when the message ID is returned. </returns>
		ValueTask<IMessageId> GetMessageIdByTimestampAsync(string topic, long timestamp);

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id.
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="ConflictException">
		///             Subscription already exists </exception>
		/// <exception cref="NotAllowedException">
		///             Command disallowed for requested resource </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: default void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException
		void CreateSubscription(string topic, string subscriptionName, IMessageId messageId)
		{
			CreateSubscription(topic, subscriptionName, messageId, false);
		}

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id. </param>
		ValueTask CreateSubscriptionAsync(string topic, string subscriptionName, IMessageId messageId)
		{
			return CreateSubscriptionAsync(topic, subscriptionName, messageId, false);
		}

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id. </param>
		/// <param name="replicated">
		///            replicated subscriptions. </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="ConflictException">
		///             Subscription already exists </exception>
		/// <exception cref="NotAllowedException">
		///             Command disallowed for requested resource </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: default void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId, boolean replicated) throws PulsarAdminException
		void CreateSubscription(string topic, string subscriptionName, IMessageId messageId, bool replicated)
		{
			CreateSubscription(topic, subscriptionName, messageId, replicated, null);
		}

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id. </param>
		/// <param name="replicated">
		///            replicated subscriptions. </param>
		/// <param name="properties">
		///            subscription properties. </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="ConflictException">
		///             Subscription already exists </exception>
		/// <exception cref="NotAllowedException">
		///             Command disallowed for requested resource </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId, boolean replicated, java.util.Map<String, String> properties) throws PulsarAdminException;
		void CreateSubscription(string topic, string subscriptionName, IMessageId messageId, bool replicated, IDictionary<string, string> properties);

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id.
		/// </param>
		/// <param name="replicated">
		///           replicated subscriptions. </param>
		ValueTask CreateSubscriptionAsync(string topic, string subscriptionName, IMessageId messageId, bool replicated)
		{
			return CreateSubscriptionAsync(topic, subscriptionName, messageId, replicated, null);
		}

		/// <summary>
		/// Create a new subscription on a topic.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id.
		/// </param>
		/// <param name="replicated">
		///           replicated subscriptions. </param>
		/// <param name="properties">
		///            subscription properties. </param>
		ValueTask CreateSubscriptionAsync(string topic, string subscriptionName, IMessageId messageId, bool replicated, IDictionary<string, string> properties);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="timestamp">
		///            reset subscription to position closest to time in ms since epoch
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="NotAllowedException">
		///             Command disallowed for requested resource </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void resetCursor(String topic, String subName, long timestamp) throws PulsarAdminException;
		void ResetCursor(string topic, string subName, long timestamp);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// <p/>
		/// and start consume messages from the next position of the reset position. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="messageId"> </param>
		/// <param name="isExcluded"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void resetCursor(String topic, String subName, org.apache.pulsar.client.api.MessageId messageId, boolean isExcluded) throws PulsarAdminException;
		void ResetCursor(string topic, string subName, MessageId messageId, bool isExcluded);

		/// <summary>
		/// Update Subscription Properties on a topic subscription.
		/// The new properties will override the existing values, properties that are not passed will be removed. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="subscriptionProperties"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void updateSubscriptionProperties(String topic, String subName, java.util.Map<String, String> subscriptionProperties) throws PulsarAdminException;
		void UpdateSubscriptionProperties(string topic, string subName, IDictionary<string, string> subscriptionProperties);

		/// <summary>
		/// Get Subscription Properties on a topic subscription. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.Map<String, String> getSubscriptionProperties(String topic, String subName) throws PulsarAdminException;
		IDictionary<string, string> GetSubscriptionProperties(string topic, string subName);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="timestamp">
		///            reset subscription to position closest to time in ms since epoch </param>
		ValueTask ResetCursorAsync(string topic, string subName, long timestamp);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// <p/>
		/// and start consume messages from the next position of the reset position. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="messageId"> </param>
		/// <param name="isExcluded">
		/// @return </param>
		ValueTask ResetCursorAsync(string topic, string subName, MessageId messageId, bool isExcluded);

		/// <summary>
		/// Update Subscription Properties on a topic subscription.
		/// The new properties will override the existing values, properties that are not passed will be removed. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="subscriptionProperties"> </param>
		ValueTask UpdateSubscriptionPropertiesAsync(string topic, string subName, IDictionary<string, string> subscriptionProperties);

		/// <summary>
		/// Get Subscription Properties on a topic subscription. </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		ValueTask<IDictionary<string, string>> GetSubscriptionPropertiesAsync(string topic, string subName);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            reset subscription to messageId (or previous nearest messageId if given messageId is not valid)
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic or subscription does not exist </exception>
		/// <exception cref="NotAllowedException">
		///             Command disallowed for requested resource </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void resetCursor(String topic, String subName, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		void ResetCursor(string topic, string subName, MessageId messageId);

		/// <summary>
		/// Reset cursor position on a topic subscription.
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            reset subscription to messageId (or previous nearest messageId if given messageId is not valid) </param>
		ValueTask ResetCursorAsync(string topic, string subName, MessageId messageId);

		/// <summary>
		/// Trigger compaction to run for a topic. A single topic can only have one instance of compaction
		/// running at any time. Any attempt to trigger another will be met with a ConflictException.
		/// </summary>
		/// <param name="topic">
		///            The topic on which to trigger compaction </param>

// ORIGINAL LINE: void triggerCompaction(String topic) throws PulsarAdminException;
		void TriggerCompaction(string topic);

		/// <summary>
		/// Trigger compaction to run for a topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            The topic on which to trigger compaction </param>
		ValueTask TriggerCompactionAsync(string topic);

		/// <summary>
		/// Check the status of an ongoing compaction for a topic.
		/// </summary>
		/// <param name="topic"> The topic whose compaction status we wish to check </param>

// ORIGINAL LINE: LongRunningProcessStatus compactionStatus(String topic) throws PulsarAdminException;
		LongRunningProcessStatus CompactionStatus(string topic);

		/// <summary>
		/// Check the status of an ongoing compaction for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> The topic whose compaction status we wish to check </param>
		ValueTask<LongRunningProcessStatus> CompactionStatusAsync(string topic);

		/// <summary>
		/// Trigger offloading messages in topic to longterm storage.
		/// </summary>
		/// <param name="topic"> the topic to offload </param>
		/// <param name="messageId"> ID of maximum message which should be offloaded </param>

// ORIGINAL LINE: void triggerOffload(String topic, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		void TriggerOffload(string topic, MessageId messageId);

		/// <summary>
		/// Trigger offloading messages in topic to longterm storage asynchronously.
		/// </summary>
		/// <param name="topic"> the topic to offload </param>
		/// <param name="messageId"> ID of maximum message which should be offloaded </param>
		ValueTask TriggerOffloadAsync(string topic, MessageId messageId);

		/// <summary>
		/// Check the status of an ongoing offloading operation for a topic.
		/// </summary>
		/// <param name="topic"> the topic being offloaded </param>
		/// <returns> the status of the offload operation </returns>

// ORIGINAL LINE: OffloadProcessStatus offloadStatus(String topic) throws PulsarAdminException;
		IOffloadProcessStatus OffloadStatus(string topic);

		/// <summary>
		/// Check the status of an ongoing offloading operation for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> the topic being offloaded </param>
		/// <returns> the status of the offload operation </returns>
		ValueTask<IOffloadProcessStatus> OffloadStatusAsync(string topic);

		/// <summary>
		/// Get the last commit message Id of a topic.
		/// </summary>
		/// <param name="topic"> the topic name
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.client.api.MessageId getLastMessageId(String topic) throws PulsarAdminException;
		MessageId GetLastMessageId(string topic);

		/// <summary>
		/// Get the last commit message Id of a topic asynchronously.
		/// </summary>
		/// <param name="topic"> the topic name
		/// @return </param>
		ValueTask<MessageId> GetLastMessageIdAsync(string topic);


		/// <summary>
		/// Analyze subscription backlog.
		/// This is a potentially expensive operation, as it requires
		/// to read the messages from storage.
		/// This function takes into consideration batch messages
		/// and also Subscription filters. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="subscriptionName">
		///            the subscription </param>
		/// <param name="startPosition">
		///           the position to start the scan from (empty means the last processed message) </param>
		/// <returns> an accurate analysis of the backlog </returns>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.stats.AnalyzeSubscriptionBacklogResult analyzeSubscriptionBacklog(String topic, String subscriptionName, java.util.Optional<org.apache.pulsar.client.api.MessageId> startPosition) throws PulsarAdminException;
		AnalyzeSubscriptionBacklogResult AnalyzeSubscriptionBacklog(string topic, string subscriptionName, Optional<MessageId> startPosition);

		/// <summary>
		/// Analyze subscription backlog.
		/// This is a potentially expensive operation, as it requires
		/// to read the messages from storage.
		/// This function takes into consideration batch messages
		/// and also Subscription filters. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="subscriptionName">
		///            the subscription </param>
		/// <param name="startPosition">
		///           the position to start the scan from (empty means the last processed message) </param>
		/// <returns> an accurate analysis of the backlog </returns>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>
		ValueTask<AnalyzeSubscriptionBacklogResult> AnalyzeSubscriptionBacklogAsync(string topic, string subscriptionName, Optional<MessageId> startPosition);

		/// <summary>
		/// Get backlog size by a message ID. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="messageId">
		///            message ID </param>
		/// <returns> the backlog size from </returns>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<long> getBacklogSizeByMessageId(String topic, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		long? GetBacklogSizeByMessageId(string topic, IMessageId messageId);

		/// <summary>
		/// Get backlog size by a message ID asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="messageId">
		///            message ID </param>
		/// <returns> the backlog size from </returns>
		ValueTask<long> GetBacklogSizeByMessageIdAsync(string topic, IMessageId messageId);

		/// <summary>
		/// Examine a specific message on a topic by position relative to the earliest or the latest message.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="initialPosition"> Relative start position to examine message. It can be 'latest' or 'earliest' </param>
		/// <param name="messagePosition"> The position of messages (default 1) </param>

// ORIGINAL LINE: org.apache.pulsar.client.api.Message<byte[]> examineMessage(String topic, String initialPosition, long messagePosition) throws PulsarAdminException;
		IMessage<byte[]> ExamineMessage(string topic, string initialPosition, long messagePosition);

		/// <summary>
		/// Examine a specific message on a topic by position relative to the earliest or the latest message.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="initialPosition"> Relative start position to examine message. It can be 'latest' or 'earliest' </param>
		/// <param name="messagePosition"> The position of messages (default 1) </param>

// ORIGINAL LINE: java.util.concurrent.ValueTask<org.apache.pulsar.client.api.Message<byte[]>> examineMessageAsync(String topic, String initialPosition, long messagePosition) throws PulsarAdminException;
		ValueTask<IMessage<byte[]>> ExamineMessageAsync(string topic, string initialPosition, long messagePosition);

		/// <summary>
		/// Truncate a topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void truncate(String topic) throws PulsarAdminException;
		void Truncate(string topic);

		/// <summary>
		/// Truncate a topic asynchronously.
		/// <p/>
		/// The latest ledger cannot be deleted.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic is truncated </returns>
		ValueTask TruncateAsync(string topic);

		/// <summary>
		/// Enable or disable a replicated subscription on a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="enabled"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setReplicatedSubscriptionStatus(String topic, String subName, boolean enabled) throws PulsarAdminException;
		void SetReplicatedSubscriptionStatus(string topic, string subName, bool enabled);

		/// <summary>
		/// Enable or disable a replicated subscription on a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subName"> </param>
		/// <param name="enabled">
		/// @return </param>
		ValueTask SetReplicatedSubscriptionStatusAsync(string topic, string subName, bool enabled);

		/// <summary>
		/// Get the replication clusters for a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: java.util.Set<String> getReplicationClusters(String topic, boolean applied) throws PulsarAdminException;
		ISet<string> GetReplicationClusters(string topic, bool applied);

		/// <summary>
		/// Get the replication clusters for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		ValueTask<ISet<string>> GetReplicationClustersAsync(string topic, bool applied);

		/// <summary>
		/// Set the replication clusters for the topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="clusterIds">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setReplicationClusters(String topic, java.util.List<String> clusterIds) throws PulsarAdminException;
		void SetReplicationClusters(string topic, IList<string> clusterIds);

		/// <summary>
		/// Set the replication clusters for the topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="clusterIds">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		ValueTask SetReplicationClustersAsync(string topic, IList<string> clusterIds);

		/// <summary>
		/// Remove the replication clusters for the topic.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeReplicationClusters(String topic) throws PulsarAdminException;
		void RemoveReplicationClusters(string topic);

		/// <summary>
		/// Remove the replication clusters for the topic asynchronously.
		/// </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		ValueTask RemoveReplicationClustersAsync(string topic);

		/// <summary>
		/// Get replicated subscription status on a topic.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="subName"> subscription name </param>
		/// <returns> a map of replicated subscription status on a topic
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.Map<String, bool> getReplicatedSubscriptionStatus(String topic, String subName) throws PulsarAdminException;
		IDictionary<string, bool> GetReplicatedSubscriptionStatus(string topic, string subName);

		/// <summary>
		/// Get replicated subscription status on a topic asynchronously.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <returns> a map of replicated subscription status on a topic </returns>
		ValueTask<IDictionary<string, bool>> GetReplicatedSubscriptionStatusAsync(string topic, string subName);

		/// <summary>
		/// Get schema validation enforced for a topic.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <returns> whether the schema validation enforced is set or not </returns>

// ORIGINAL LINE: boolean getSchemaValidationEnforced(String topic, boolean applied) throws PulsarAdminException;
		bool GetSchemaValidationEnforced(string topic, bool applied);

		/// <summary>
		/// Get schema validation enforced for a topic.
		/// </summary>
		/// <param name="topic"> topic name </param>

// ORIGINAL LINE: void setSchemaValidationEnforced(String topic, boolean enable) throws PulsarAdminException;
		void SetSchemaValidationEnforced(string topic, bool enable);

		/// <summary>
		/// Get schema validation enforced for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <returns> whether the schema validation enforced is set or not </returns>
		ValueTask<bool> GetSchemaValidationEnforcedAsync(string topic, bool applied);

		/// <summary>
		/// Get schema validation enforced for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> topic name </param>
		ValueTask SetSchemaValidationEnforcedAsync(string topic, bool enable);

		/// <summary>
		/// Set shadow topic list for a source topic.
		/// </summary>
		/// <param name="sourceTopic">  source topic name </param>
		/// <param name="shadowTopics"> list of shadow topic name </param>

// ORIGINAL LINE: void setShadowTopics(String sourceTopic, java.util.List<String> shadowTopics) throws PulsarAdminException;
		void SetShadowTopics(string sourceTopic, IList<string> shadowTopics);

		/// <summary>
		/// Remove all shadow topics for a source topic.
		/// </summary>
		/// <param name="sourceTopic"> source topic name </param>

// ORIGINAL LINE: void removeShadowTopics(String sourceTopic) throws PulsarAdminException;
		void RemoveShadowTopics(string sourceTopic);

		/// <summary>
		/// Get shadow topic list of the source topic.
		/// </summary>
		/// <param name="sourceTopic"> source topic name </param>
		/// <returns> shadow topic list </returns>

// ORIGINAL LINE: java.util.List<String> getShadowTopics(String sourceTopic) throws PulsarAdminException;
		IList<string> GetShadowTopics(string sourceTopic);

		/// <summary>
		/// Set shadow topic list for a source topic asynchronously.
		/// </summary>
		/// <param name="sourceTopic"> source topic name </param>
		ValueTask SetShadowTopicsAsync(string sourceTopic, IList<string> shadowTopics);

		/// <summary>
		/// Remove all shadow topics for a source topic asynchronously.
		/// </summary>
		/// <param name="sourceTopic"> source topic name </param>
		ValueTask RemoveShadowTopicsAsync(string sourceTopic);

		/// <summary>
		/// Get shadow topic list of the source topic asynchronously.
		/// </summary>
		/// <param name="sourceTopic"> source topic name </param>
		ValueTask<IList<string>> GetShadowTopicsAsync(string sourceTopic);

		/// <summary>
		/// Get the shadow source topic name of the given shadow topic. </summary>
		/// <param name="shadowTopic"> shadow topic name. </param>
		/// <returns> The topic name of the source of the shadow topic. </returns>

// ORIGINAL LINE: String getShadowSource(String shadowTopic) throws PulsarAdminException;
		string GetShadowSource(string shadowTopic);

		/// <summary>
		/// Get the shadow source topic name of the given shadow topic asynchronously. </summary>
		/// <param name="shadowTopic"> shadow topic name. </param>
		/// <returns> The topic name of the source of the shadow topic. </returns>
		ValueTask<string> GetShadowSourceAsync(string shadowTopic);

		/// <summary>
		/// Create a new shadow topic as the shadow of the source topic.
		/// The source topic must exist before call this method.
		/// <para>
		///     For partitioned source topic, the partition number of shadow topic follows the source topic at creation. If
		///     the partition number of the source topic changes, the shadow topic needs to update its partition number
		///     manually.
		///     For non-partitioned source topic, the shadow topic will be created as non-partitioned topic.
		/// </para>
		/// 
		/// NOTE: This is still WIP until <a href="https://github.com/apache/pulsar/issues/16153">PIP-180</a> is finished.
		/// </summary>
		/// <param name="shadowTopic"> shadow topic name, and it must be a persistent topic name. </param>
		/// <param name="sourceTopic"> source topic name, and it must be a persistent topic name. </param>
		/// <param name="properties"> properties to be created with in the shadow topic. </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void createShadowTopic(String shadowTopic, String sourceTopic, java.util.Map<String, String> properties) throws PulsarAdminException;
		void CreateShadowTopic(string shadowTopic, string sourceTopic, IDictionary<string, string> properties);

		/// <summary>
		/// Create a new shadow topic, see #<seealso cref="createShadowTopic(String, String, Map)"/> for details.
		/// </summary>
		ValueTask CreateShadowTopicAsync(string shadowTopic, string sourceTopic, IDictionary<string, string> properties);

		/// <summary>
		/// Create a new shadow topic, see #<seealso cref="createShadowTopic(String, String, Map)"/> for details.
		/// </summary>

// ORIGINAL LINE: default void createShadowTopic(String shadowTopic, String sourceTopic) throws PulsarAdminException
		void CreateShadowTopic(string shadowTopic, string sourceTopic)
		{
			CreateShadowTopic(shadowTopic, sourceTopic, null);
		}

		/// <summary>
		/// Create a new shadow topic, see #<seealso cref="createShadowTopic(String, String, Map)"/> for details.
		/// </summary>
		ValueTask CreateShadowTopicAsync(string shadowTopic, string sourceTopic)
		{
			return CreateShadowTopicAsync(shadowTopic, sourceTopic, null);
		}
	}

	public sealed class TopicsQueryParam
	{
		public static readonly TopicsQueryParam Bundle = new TopicsQueryParam("Bundle", InnerEnum.Bundle, "bundle");

		private static readonly List<TopicsQueryParam> valueList = new List<TopicsQueryParam>();

		static TopicsQueryParam()
		{
			valueList.Add(Bundle);
		}

		public enum InnerEnum
		{
			Bundle
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		public readonly string Value;

		internal TopicsQueryParam(string name, InnerEnum innerEnum, string value)
		{
			this.Value = value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static TopicsQueryParam[] Values()
		{
			return valueList.ToArray();
		}

		public int Ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static TopicsQueryParam ValueOf(string name)
		{
			foreach (TopicsQueryParam enumInstance in TopicsQueryParam.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}