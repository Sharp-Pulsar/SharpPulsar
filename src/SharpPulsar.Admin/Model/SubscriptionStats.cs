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
namespace Org.Apache.Pulsar.Common.Policies.Data
{

	/// <summary>
	/// Statistics about subscription.
	/// </summary>
	public interface SubscriptionStats
	{
		/// <summary>
		/// Total rate of messages delivered on this subscription (msg/s). </summary>
		double MsgRateOut {get;}

		/// <summary>
		/// Total throughput delivered on this subscription (bytes/s). </summary>
		double MsgThroughputOut {get;}

		/// <summary>
		/// Total bytes delivered to consumer (bytes). </summary>
		long BytesOutCounter {get;}

		/// <summary>
		/// Total messages delivered to consumer (msg). </summary>
		long MsgOutCounter {get;}

		/// <summary>
		/// Total rate of messages redelivered on this subscription (msg/s). </summary>
		double MsgRateRedeliver {get;}

		/// <summary>
		/// Total rate of message ack(msg/s).
		/// </summary>
		double MessageAckRate {get;}

		/// <summary>
		/// Chunked message dispatch rate. </summary>
		int ChunkedMessageRate {get;}

		/// <summary>
		/// Number of entries in the subscription backlog. </summary>
		long MsgBacklog {get;}

		/// <summary>
		/// Size of backlog in byte. * </summary>
		long BacklogSize {get;}

		/// <summary>
		/// Get the publish time of the earliest message in the backlog. </summary>
		long EarliestMsgPublishTimeInBacklog {get;}

		/// <summary>
		/// Number of entries in the subscription backlog that do not contain the delay messages. </summary>
		long MsgBacklogNoDelayed {get;}

		/// <summary>
		/// Flag to verify if subscription is blocked due to reaching threshold of unacked messages. </summary>
		bool BlockedSubscriptionOnUnackedMsgs {get;}

		/// <summary>
		/// Number of delayed messages currently being tracked. </summary>
		long MsgDelayed {get;}

		/// <summary>
		/// Number of unacknowledged messages for the subscription, where an unacknowledged message is one that has been
		/// sent to a consumer but not yet acknowledged. Calculated by summing all <seealso cref="ConsumerStats.getUnackedMessages()"/>
		/// for this subscription. See <seealso cref="ConsumerStats.getUnackedMessages()"/> for additional details.
		/// </summary>
		long UnackedMessages {get;}

		/// <summary>
		/// The subscription type as defined by <seealso cref="org.apache.pulsar.client.api.SubscriptionType"/>. </summary>
		string Type {get;}

		/// <summary>
		/// The name of the consumer that is active for single active consumer subscriptions i.e. failover or exclusive. </summary>
		string ActiveConsumerName {get;}

		/// <summary>
		/// Total rate of messages expired on this subscription (msg/s). </summary>
		double MsgRateExpired {get;}

		/// <summary>
		/// Total messages expired on this subscription. </summary>
		long TotalMsgExpired {get;}

		/// <summary>
		/// Last message expire execution timestamp. </summary>
		long LastExpireTimestamp {get;}

		/// <summary>
		/// Last received consume flow command timestamp. </summary>
		long LastConsumedFlowTimestamp {get;}

		/// <summary>
		/// Last consume message timestamp. </summary>
		long LastConsumedTimestamp {get;}

		/// <summary>
		/// Last acked message timestamp. </summary>
		long LastAckedTimestamp {get;}

		/// <summary>
		/// Last MarkDelete position advanced timesetamp. </summary>
		long LastMarkDeleteAdvancedTimestamp {get;}

		/// <summary>
		/// List of connected consumers on this subscription w/ their stats. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.List<? extends ConsumerStats> getConsumers();
		IList<ConsumerStats> Consumers {get;}

		/// <summary>
		/// Tells whether this subscription is durable or ephemeral (eg.: from a reader). </summary>
		bool Durable {get;}

		/// <summary>
		/// Mark that the subscription state is kept in sync across different regions. </summary>
		bool Replicated {get;}

		/// <summary>
		/// Whether out of order delivery is allowed on the Key_Shared subscription. </summary>
		bool AllowOutOfOrderDelivery {get;}

		/// <summary>
		/// Whether the Key_Shared subscription mode is AUTO_SPLIT or STICKY. </summary>
		string KeySharedMode {get;}

		/// <summary>
		/// This is for Key_Shared subscription to get the recentJoinedConsumers in the Key_Shared subscription. </summary>
		IDictionary<string, string> ConsumersAfterMarkDeletePosition {get;}

		/// <summary>
		/// SubscriptionProperties (key/value strings) associated with this subscribe. </summary>
		IDictionary<string, string> SubscriptionProperties {get;}

		/// <summary>
		/// The number of non-contiguous deleted messages ranges. </summary>
		int NonContiguousDeletedMessagesRanges {get;}

		/// <summary>
		/// The serialized size of non-contiguous deleted messages ranges. </summary>
		int NonContiguousDeletedMessagesRangesSerializedSize {get;}

		long FilterProcessedMsgCount {get;}

		long FilterAcceptedMsgCount {get;}

		long FilterRejectedMsgCount {get;}

		long FilterRescheduledMsgCount {get;}
	}

}