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
namespace SharpPulsar.Admin.Model
{

	/// <summary>
	/// Statistics for a Pulsar topic.
	/// </summary>
	public interface TopicStats
	{
		/// <summary>
		/// Total rate of messages published on the topic (msg/s). </summary>
		double MsgRateIn {get;}

		/// <summary>
		/// Total throughput of messages published on the topic (byte/s). </summary>
		double MsgThroughputIn {get;}

		/// <summary>
		/// Total rate of messages dispatched for the topic (msg/s). </summary>
		double MsgRateOut {get;}

		/// <summary>
		/// Total throughput of messages dispatched for the topic (byte/s). </summary>
		double MsgThroughputOut {get;}

		/// <summary>
		/// Total bytes published to the topic (bytes). </summary>
		long BytesInCounter {get;}

		/// <summary>
		/// Total messages published to the topic (msg). </summary>
		long MsgInCounter {get;}

		/// <summary>
		/// Total bytes delivered to consumer (bytes). </summary>
		long BytesOutCounter {get;}

		/// <summary>
		/// Total messages delivered to consumer (msg). </summary>
		long MsgOutCounter {get;}

		/// <summary>
		/// Average size of published messages (bytes). </summary>
		double AverageMsgSize {get;}

		/// <summary>
		/// Topic has chunked message published on it. </summary>
		bool MsgChunkPublished {get;}

		/// <summary>
		/// Space used to store the messages for the topic (bytes). </summary>
		long StorageSize {get;}

		/// <summary>
		/// Get estimated total unconsumed or backlog size in bytes. </summary>
		long BacklogSize {get;}

		/// <summary>
		/// Get the publish time of the earliest message over all the backlogs. </summary>
		long EarliestMsgPublishTimeInBacklogs {get;}

		/// <summary>
		/// Space used to store the offloaded messages for the topic/. </summary>
		long OffloadedStorageSize {get;}

		/// <summary>
		/// List of connected publishers on this topic w/ their stats. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.List<? extends PublisherStats> getPublishers();
		IList<PublisherStats> Publishers {get;}

		int WaitingPublishers {get;}

		/// <summary>
		/// Map of subscriptions with their individual statistics. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.Map<String, ? extends SubscriptionStats> getSubscriptions();
		IDictionary<string, SubscriptionStats> Subscriptions {get;}

		/// <summary>
		/// Map of replication statistics by remote cluster context. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.Map<String, ? extends ReplicatorStats> getReplication();
		IDictionary<string, ReplicatorStats> Replication {get;}

		string DeduplicationStatus {get;}

		/// <summary>
		/// The topic epoch or empty if not set. </summary>
		long? TopicEpoch {get;}

		/// <summary>
		/// The number of non-contiguous deleted messages ranges. </summary>
		int NonContiguousDeletedMessagesRanges {get;}

		/// <summary>
		/// The serialized size of non-contiguous deleted messages ranges. </summary>
		int NonContiguousDeletedMessagesRangesSerializedSize {get;}

		/// <summary>
		/// The compaction stats. </summary>
		CompactionStats Compaction {get;}

		/// <summary>
		/// The broker that owns this topic. * </summary>
		string OwnerBroker {get;}
	}

}