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
	using NotAllowedException = PulsarAdminException.NotAllowedException;
	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using Message = client.api.Message;
	using MessageId = client.api.MessageId;
	using PartitionedTopicMetadata = pulsar.common.partition.PartitionedTopicMetadata;
	using AuthAction = pulsar.common.policies.data.AuthAction;
	using PartitionedTopicInternalStats = pulsar.common.policies.data.PartitionedTopicInternalStats;
	using PartitionedTopicStats = pulsar.common.policies.data.PartitionedTopicStats;
	using PersistentTopicInternalStats = pulsar.common.policies.data.PersistentTopicInternalStats;
	using TopicStats = pulsar.common.policies.data.TopicStats;

	using JsonObject = com.google.gson.JsonObject;

	public interface Topics
	{

		/// <summary>
		/// Get the list of topics under a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["topic://my-tenant/my-namespace/topic-1",
		///  "topic://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getList(String namespace) throws PulsarAdminException;
		IList<string> getList(string @namespace);

		/// <summary>
		/// Get the list of partitioned topics under a namespace.
		/// <para>
		/// Response example:
		/// 
		/// <pre>
		/// <code>["persistent://my-tenant/my-namespace/topic-1",
		///  "persistent://my-tenant/my-namespace/topic-2"]</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getPartitionedTopicList(String namespace) throws PulsarAdminException;
		IList<string> getPartitionedTopicList(string @namespace);

		/// <summary>
		/// Get list of topics exist into given bundle
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundleRange">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getListInBundle(String namespace, String bundleRange) throws PulsarAdminException;
		IList<string> getListInBundle(string @namespace, string bundleRange);

		/// <summary>
		/// Get list of topics exist into given bundle asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundleRange">
		/// @return </param>
		CompletableFuture<IList<string>> getListInBundleAsync(string @namespace, string bundleRange);

		/// <summary>
		/// Get permissions on a topic.
		/// <para>
		/// Retrieve the effective permissions for a topic. These permissions are defined by the permissions set at the
		/// namespace level combined (union) with any eventual specific permission set on the topic.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{
		///   "role-1" : [ "produce" ],
		///   "role-2" : [ "consume" ]
		/// }</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Map<String, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction>> getPermissions(String topic) throws PulsarAdminException;
		IDictionary<string, ISet<AuthAction>> getPermissions(string topic);

		/// <summary>
		/// Grant permission on a topic.
		/// <para>
		/// Grant a new permission to a client role on a single topic.
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void grantPermission(String topic, String role, java.util.Set<org.apache.pulsar.common.policies.data.AuthAction> actions) throws PulsarAdminException;
		void grantPermission(string topic, string role, ISet<AuthAction> actions);

		/// <summary>
		/// Revoke permissions on a topic.
		/// <para>
		/// Revoke permissions to a client role on a single topic. If the permission was not set at the topic level, but
		/// rather at the namespace level, this operation will return an error (HTTP status code 412).
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void revokePermissions(String topic, String role) throws PulsarAdminException;
		void revokePermissions(string topic, string role);

		/// <summary>
		/// Create a partitioned topic.
		/// <para>
		/// Create a partitioned topic. It needs to be called before creating a producer for a partitioned topic.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createPartitionedTopic(String topic, int numPartitions) throws PulsarAdminException;
		void createPartitionedTopic(string topic, int numPartitions);

		/// <summary>
		/// Create a non-partitioned topic.
		/// 
		/// <para>
		/// Create a non-partitioned topic. 
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createNonPartitionedTopic(String topic) throws PulsarAdminException;
		void createNonPartitionedTopic(string topic);

		/// <summary>
		/// Create missed partitions for partitioned topic.
		/// <para>
		/// When disable topic auto creation, use this method to try create missed partitions while
		/// partitions create failed or users already have partitioned topic without partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic"> partitioned topic name </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createMissedPartitions(String topic) throws PulsarAdminException;
		void createMissedPartitions(string topic);

		/// <summary>
		/// Create a partitioned topic asynchronously.
		/// <para>
		/// Create a partitioned topic asynchronously. It needs to be called before creating a producer for a partitioned
		/// topic.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of partitions to create of the topic </param>
		/// <returns> a future that can be used to track when the partitioned topic is created </returns>
		CompletableFuture<Void> createPartitionedTopicAsync(string topic, int numPartitions);

		/// <summary>
		/// Create a non-partitioned topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		CompletableFuture<Void> createNonPartitionedTopicAsync(string topic);

		/// <summary>
		/// Create missed partitions for partitioned topic asynchronously.
		/// <para>
		/// When disable topic auto creation, use this method to try create missed partitions while
		/// partitions create failed or users already have partitioned topic without partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic"> partitioned topic name </param>
		CompletableFuture<Void> createMissedPartitionsAsync(string topic);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic.
		/// <para>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic
		/// </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updatePartitionedTopic(String topic, int numPartitions) throws PulsarAdminException;
		void updatePartitionedTopic(string topic, int numPartitions);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic asynchronously.
		/// <para>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic
		/// </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
		CompletableFuture<Void> updatePartitionedTopicAsync(string topic, int numPartitions);

		/// <summary>
		/// Update number of partitions of a non-global partitioned topic asynchronously.
		/// <para>
		/// It requires partitioned-topic to be already exist and number of new partitions must be greater than existing
		/// number of partitions. Decrementing number of partitions requires deletion of topic which is not supported.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="numPartitions">
		///            Number of new partitions of already exist partitioned-topic </param>
		/// <param name="updateLocalTopicOnly">
		///            Used by broker for global topic with multiple replicated clusters
		/// </param>
		/// <returns> a future that can be used to track when the partitioned topic is updated </returns>
		CompletableFuture<Void> updatePartitionedTopicAsync(string topic, int numPartitions, bool updateLocalTopicOnly);

		/// <summary>
		/// Get metadata of a partitioned topic.
		/// <para>
		/// Get metadata of a partitioned topic.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> Partitioned topic metadata </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.partition.PartitionedTopicMetadata getPartitionedTopicMetadata(String topic) throws PulsarAdminException;
		PartitionedTopicMetadata getPartitionedTopicMetadata(string topic);

		/// <summary>
		/// Get metadata of a partitioned topic asynchronously.
		/// <para>
		/// Get metadata of a partitioned topic asynchronously.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <returns> a future that can be used to track when the partitioned topic metadata is returned </returns>
		CompletableFuture<PartitionedTopicMetadata> getPartitionedTopicMetadataAsync(string topic);

		/// <summary>
		/// Delete a partitioned topic.
		/// <para>
		/// It will also delete all the partitions of the topic if it exists.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deletePartitionedTopic(String topic, boolean force) throws PulsarAdminException;
		void deletePartitionedTopic(string topic, bool force);

		/// <summary>
		/// Delete a partitioned topic.
		/// <para>
		/// It will also delete all the partitions of the topic if it exists.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deletePartitionedTopic(String topic) throws PulsarAdminException;
		void deletePartitionedTopic(string topic);

		/// <summary>
		/// Delete a partitioned topic asynchronously.
		/// <para>
		/// It will also delete all the partitions of the topic if it exists.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <returns> a future that can be used to track when the partitioned topic is deleted </returns>
		CompletableFuture<Void> deletePartitionedTopicAsync(string topic, bool force);

		/// <summary>
		/// Delete a topic.
		/// <para>
		/// Delete a topic. The topic cannot be deleted if force flag is disable and there's any active subscription or producer connected to the it. Force flag deletes topic forcefully by closing all active producers and consumers.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Topic has active subscriptions or producers </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void delete(String topic, boolean force) throws PulsarAdminException;
		void delete(string topic, bool force);


		/// <summary>
		/// Delete a topic.
		/// <para>
		/// Delete a topic. The topic cannot be deleted if there's any active subscription or producer connected to the it.
		/// </para>
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void delete(String topic) throws PulsarAdminException;
		void delete(string topic);

		/// <summary>
		/// Delete a topic asynchronously.
		/// <para>
		/// Delete a topic asynchronously. The topic cannot be deleted if force flag is disable and there's any active
		/// subscription or producer connected to the it. Force flag deletes topic forcefully by closing all active producers
		/// and consumers.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="force">
		///            Delete topic forcefully
		/// </param>
		/// <returns> a future that can be used to track when the topic is deleted </returns>
		CompletableFuture<Void> deleteAsync(string topic, bool force);

		/// <summary>
		/// Unload a topic.
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unload(String topic) throws PulsarAdminException;
		void unload(string topic);

		/// <summary>
		/// Unload a topic asynchronously.
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic is unloaded </returns>
		CompletableFuture<Void> unloadAsync(string topic);

		/// <summary>
		/// Terminate the topic and prevent any more messages being published on it.
		/// <para>
		/// This
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> the message id of the last message that was published in the topic </returns>
		CompletableFuture<MessageId> terminateTopicAsync(string topic);

		/// <summary>
		/// Get the list of subscriptions.
		/// <para>
		/// Get the list of persistent subscriptions for a given topic.
		/// </para>
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getSubscriptions(String topic) throws PulsarAdminException;
		IList<string> getSubscriptions(string topic);

		/// <summary>
		/// Get the list of subscriptions asynchronously.
		/// <para>
		/// Get the list of persistent subscriptions for a given topic.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <returns> a future that can be used to track when the list of subscriptions is returned </returns>
		CompletableFuture<IList<string>> getSubscriptionsAsync(string topic);

		/// <summary>
		/// Get the stats for the topic.
		/// <para>
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
		/// All the rates are computed over a 1 minute window and are relative the last completed 1 minute period.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.TopicStats getStats(String topic) throws PulsarAdminException;
		TopicStats getStats(string topic);

		/// <summary>
		/// Get the stats for the topic asynchronously. All the rates are computed over a 1 minute window and are relative
		/// the last completed 1 minute period.
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic statistics are returned
		///  </returns>
		CompletableFuture<TopicStats> getStatsAsync(string topic);

		/// <summary>
		/// Get the internal stats for the topic.
		/// <para>
		/// Access the internal state of the topic
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistentTopicInternalStats getInternalStats(String topic) throws PulsarAdminException;
		PersistentTopicInternalStats getInternalStats(string topic);

		/// <summary>
		/// Get the internal stats for the topic asynchronously.
		/// </summary>
		/// <param name="topic">
		///            topic Name
		/// </param>
		/// <returns> a future that can be used to track when the internal topic statistics are returned </returns>
		CompletableFuture<PersistentTopicInternalStats> getInternalStatsAsync(string topic);

		/// <summary>
		/// Get a JSON representation of the topic metadata stored in ZooKeeper
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: com.google.gson.JsonObject getInternalInfo(String topic) throws PulsarAdminException;
		JsonObject getInternalInfo(string topic);

		/// <summary>
		/// Get a JSON representation of the topic metadata stored in ZooKeeper
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
		CompletableFuture<JsonObject> getInternalInfoAsync(string topic);

		/// <summary>
		/// Get the stats for the partitioned topic
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>
		/// {
		///   "msgRateIn" : 100.0,                    // Total rate of messages published on the partitioned topic. msg/s
		///   "msgThroughputIn" : 10240.0,            // Total throughput of messages published on the partitioned topic. byte/s
		///   "msgRateOut" : 100.0,                   // Total rate of messages delivered on the partitioned topic. msg/s
		///   "msgThroughputOut" : 10240.0,           // Total throughput of messages delivered on the partitioned topic. byte/s
		///   "averageMsgSize" : 1024.0,              // Average size of published messages. bytes
		///   "publishers" : [                        // List of publishes on this partitioned topic with their stats
		///      {
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
		/// All the rates are computed over a 1 minute window and are relative the last completed 1 minute period.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="perPartition">
		/// </param>
		/// <returns> the partitioned topic statistics </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.PartitionedTopicStats getPartitionedStats(String topic, boolean perPartition) throws PulsarAdminException;
		PartitionedTopicStats getPartitionedStats(string topic, bool perPartition);

		/// <summary>
		/// Get the stats for the partitioned topic asynchronously
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <param name="perPartition">
		///            flag to get stats per partition </param>
		/// <returns> a future that can be used to track when the partitioned topic statistics are returned </returns>
		CompletableFuture<PartitionedTopicStats> getPartitionedStatsAsync(string topic, bool perPartition);

		/// <summary>
		/// Get the stats for the partitioned topic
		/// </summary>
		/// <param name="topic">
		///            topic name
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.PartitionedTopicInternalStats getPartitionedInternalStats(String topic) throws PulsarAdminException;
		PartitionedTopicInternalStats getPartitionedInternalStats(string topic);

		/// <summary>
		/// Get the stats-internal for the partitioned topic asynchronously
		/// </summary>
		/// <param name="topic">
		///            topic Name </param>
		/// <returns> a future that can be used to track when the partitioned topic statistics are returned </returns>
		CompletableFuture<PartitionedTopicInternalStats> getPartitionedInternalStatsAsync(string topic);

		/// <summary>
		/// Delete a subscription.
		/// <para>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// </para>
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteSubscription(String topic, String subName) throws PulsarAdminException;
		void deleteSubscription(string topic, string subName);

		/// <summary>
		/// Delete a subscription asynchronously.
		/// <para>
		/// Delete a persistent subscription from a topic. There should not be any active consumers on the subscription.
		/// </para>
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <returns> a future that can be used to track when the subscription is deleted </returns>
		CompletableFuture<Void> deleteSubscriptionAsync(string topic, string subName);

		/// <summary>
		/// Skip all messages on a topic subscription.
		/// <para>
		/// Completely clears the backlog on the subscription.
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void skipAllMessages(String topic, String subName) throws PulsarAdminException;
		void skipAllMessages(string topic, string subName);

		/// <summary>
		/// Skip all messages on a topic subscription asynchronously.
		/// <para>
		/// Completely clears the backlog on the subscription.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name
		/// </param>
		/// <returns> a future that can be used to track when all the messages are skipped </returns>
		CompletableFuture<Void> skipAllMessagesAsync(string topic, string subName);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void skipMessages(String topic, String subName, long numMessages) throws PulsarAdminException;
		void skipMessages(string topic, string subName, long numMessages);

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
		CompletableFuture<Void> skipMessagesAsync(string topic, string subName, long numMessages);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void expireMessages(String topic, String subscriptionName, long expireTimeInSeconds) throws PulsarAdminException;
		void expireMessages(string topic, string subscriptionName, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for a given subscription asynchronously
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds
		/// @return </param>
		CompletableFuture<Void> expireMessagesAsync(string topic, string subscriptionName, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for all subscriptions of the
		/// persistent-topic
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void expireMessagesForAllSubscriptions(String topic, long expireTimeInSeconds) throws PulsarAdminException;
		void expireMessagesForAllSubscriptions(string topic, long expireTimeInSeconds);

		/// <summary>
		/// Expire all messages older than given N (expireTimeInSeconds) seconds for all subscriptions of the
		/// persistent-topic asynchronously
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="expireTimeInSeconds">
		///            Expire messages older than time in seconds </param>
		CompletableFuture<Void> expireMessagesForAllSubscriptionsAsync(string topic, long expireTimeInSeconds);

		/// <summary>
		/// Peek messages from a topic subscription
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.client.api.Message<byte[]>> peekMessages(String topic, String subName, int numMessages) throws PulsarAdminException;
		IList<Message<sbyte[]>> peekMessages(string topic, string subName, int numMessages);

		/// <summary>
		/// Peek messages from a topic subscription asynchronously
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="numMessages">
		///            Number of messages </param>
		/// <returns> a future that can be used to track when the messages are returned </returns>
		CompletableFuture<IList<Message<sbyte[]>>> peekMessagesAsync(string topic, string subName, int numMessages);

		/// <summary>
		/// Create a new subscription on a topic
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSubscription(String topic, String subscriptionName, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		void createSubscription(string topic, string subscriptionName, MessageId messageId);

		/// <summary>
		/// Create a new subscription on a topic
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subscriptionName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> on where to initialize the subscription. It could be <seealso cref="MessageId.latest"/>,
		///            <seealso cref="MessageId.earliest"/> or a specific message id. </param>
		CompletableFuture<Void> createSubscriptionAsync(string topic, string subscriptionName, MessageId messageId);

		/// <summary>
		/// Reset cursor position on a topic subscription
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void resetCursor(String topic, String subName, long timestamp) throws PulsarAdminException;
		void resetCursor(string topic, string subName, long timestamp);

		/// <summary>
		/// Reset cursor position on a topic subscription
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="timestamp">
		///            reset subscription to position closest to time in ms since epoch </param>
		CompletableFuture<Void> resetCursorAsync(string topic, string subName, long timestamp);

		/// <summary>
		/// Reset cursor position on a topic subscription
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void resetCursor(String topic, String subName, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		void resetCursor(string topic, string subName, MessageId messageId);

		/// <summary>
		/// Reset cursor position on a topic subscription
		/// </summary>
		/// <param name="topic">
		///            topic name </param>
		/// <param name="subName">
		///            Subscription name </param>
		/// <param name="messageId">
		///            reset subscription to messageId (or previous nearest messageId if given messageId is not valid) </param>
		CompletableFuture<Void> resetCursorAsync(string topic, string subName, MessageId messageId);

		/// <summary>
		/// Trigger compaction to run for a topic. A single topic can only have one instance of compaction
		/// running at any time. Any attempt to trigger another will be met with a ConflictException.
		/// </summary>
		/// <param name="topic">
		///            The topic on which to trigger compaction </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void triggerCompaction(String topic) throws PulsarAdminException;
		void triggerCompaction(string topic);

		/// <summary>
		/// Check the status of an ongoing compaction for a topic.
		/// </summary>
		/// <param name="topic"> The topic whose compaction status we wish to check </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: LongRunningProcessStatus compactionStatus(String topic) throws PulsarAdminException;
		LongRunningProcessStatus compactionStatus(string topic);

		/// <summary>
		/// Trigger offloading messages in topic to longterm storage.
		/// </summary>
		/// <param name="topic"> the topic to offload </param>
		/// <param name="messageId"> ID of maximum message which should be offloaded </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void triggerOffload(String topic, org.apache.pulsar.client.api.MessageId messageId) throws PulsarAdminException;
		void triggerOffload(string topic, MessageId messageId);

		/// <summary>
		/// Check the status of an ongoing offloading operation for a topic.
		/// </summary>
		/// <param name="topic"> the topic being offloaded </param>
		/// <returns> the status of the offload operation </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: OffloadProcessStatus offloadStatus(String topic) throws PulsarAdminException;
		OffloadProcessStatus offloadStatus(string topic);

		/// <summary>
		/// Get the last commit message Id of a topic
		/// </summary>
		/// <param name="topic"> the topic name
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.client.api.MessageId getLastMessageId(String topic) throws PulsarAdminException;
		MessageId getLastMessageId(string topic);
	}

}