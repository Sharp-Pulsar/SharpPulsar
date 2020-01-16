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

	using NotAuthorizedException = org.apache.pulsar.client.admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = org.apache.pulsar.client.admin.PulsarAdminException.NotFoundException;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using NonPersistentTopicStats = org.apache.pulsar.common.policies.data.NonPersistentTopicStats;
	using PersistentTopicInternalStats = org.apache.pulsar.common.policies.data.PersistentTopicInternalStats;

	/// @deprecated since 2.0. See <seealso cref="Topics"/> 
	[Obsolete("since 2.0. See <seealso cref=\"Topics\"/>")]
	public interface NonPersistentTopics
	{



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
		///       "connected" : true,              // Whether the replication-subscriber is currently connected locally
		///     },
		///     "cluster_2" : {
		///       "msgRateIn" : 100.0,
		///       "msgThroughputIn" : 10240.0,
		///       "msgRateOut" : 100.0,
		///       "msghroughputOut" : 10240.0,
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
		///            Topic name </param>
		/// <returns> the topic statistics
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.NonPersistentTopicStats getStats(String topic) throws PulsarAdminException;
		NonPersistentTopicStats getStats(string topic);

		/// <summary>
		/// Get the stats for the topic asynchronously. All the rates are computed over a 1 minute window and are relative
		/// the last completed 1 minute period.
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic statistics are returned
		///  </returns>
		CompletableFuture<NonPersistentTopicStats> getStatsAsync(string topic);

		/// <summary>
		/// Get the internal stats for the topic.
		/// <para>
		/// Access the internal state of the topic
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
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
		///            Topic Name
		/// </param>
		/// <returns> a future that can be used to track when the internal topic statistics are returned </returns>
		CompletableFuture<PersistentTopicInternalStats> getInternalStatsAsync(string topic);

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
		/// Unload a topic.
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
		///            Topic name
		/// </param>
		/// <returns> a future that can be used to track when the topic is unloaded </returns>
		CompletableFuture<Void> unloadAsync(string topic);

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
		/// Get list of topics exist into given namespace
		/// </summary>
		/// <param name="namespace">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getList(String namespace) throws PulsarAdminException;
		IList<string> getList(string @namespace);

		/// <summary>
		/// Get list of topics exist into given namespace asynchronously.
		/// </summary>
		/// <param name="namespace"> </param>
		/// <param name="bundleRange">
		/// @return </param>
		CompletableFuture<IList<string>> getListAsync(string @namespace);

	}

}