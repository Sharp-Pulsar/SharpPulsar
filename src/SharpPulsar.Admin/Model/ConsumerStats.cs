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
	/// Consumer statistics.
	/// </summary>
	public interface ConsumerStats
	{
		/// <summary>
		/// Total rate of messages delivered to the consumer (msg/s). </summary>
		double MsgRateOut {get;}

		/// <summary>
		/// Total throughput delivered to the consumer (bytes/s). </summary>
		double MsgThroughputOut {get;}

		/// <summary>
		/// Total bytes delivered to consumer (bytes). </summary>
		long BytesOutCounter {get;}

		/// <summary>
		/// Total messages delivered to consumer (msg). </summary>
		long MsgOutCounter {get;}

		/// <summary>
		/// Total rate of messages redelivered by this consumer (msg/s). </summary>
		double MsgRateRedeliver {get;}

		/// <summary>
		/// Total rate of message ack(msg/s).
		/// </summary>
		double MessageAckRate {get;}

		/// <summary>
		/// The total rate of chunked messages delivered to this consumer. </summary>
		double ChunkedMessageRate {get;}

		/// <summary>
		/// Name of the consumer. </summary>
		string ConsumerName {get;}

		/// <summary>
		/// Number of available message permits for the consumer. </summary>
		int AvailablePermits {get;}

		/// <summary>
		/// Number of unacknowledged messages for the consumer, where an unacknowledged message is one that has been
		/// sent to the consumer but not yet acknowledged. This field is only meaningful when using a
		/// <seealso cref="org.apache.pulsar.client.api.SubscriptionType"/> that tracks individual message acknowledgement, like
		/// <seealso cref="org.apache.pulsar.client.api.SubscriptionType.Shared"/> or
		/// <seealso cref="org.apache.pulsar.client.api.SubscriptionType.Key_Shared"/>.
		/// </summary>
		int UnackedMessages {get;}

		/// <summary>
		/// Number of average messages per entry for the consumer consumed. </summary>
		int AvgMessagesPerEntry {get;}

		/// <summary>
		/// Flag to verify if consumer is blocked due to reaching threshold of unacked messages. </summary>
		bool BlockedConsumerOnUnackedMsgs {get;}

		/// <summary>
		/// The read position of the cursor when the consumer joining. </summary>
		string ReadPositionWhenJoining {get;}

		/// <summary>
		/// Address of this consumer. </summary>
		string Address {get;}

		/// <summary>
		/// Timestamp of connection. </summary>
		string ConnectedSince {get;}

		/// <summary>
		/// Client library version. </summary>
		string ClientVersion {get;}

		long LastAckedTimestamp {get;}
		long LastConsumedTimestamp {get;}
		long LastConsumedFlowTimestamp {get;}

		/// <summary>
		/// Hash ranges assigned to this consumer if is Key_Shared sub mode. * </summary>
		IList<string> KeyHashRanges {get;}

		/// <summary>
		/// Metadata (key/value strings) associated with this consumer. </summary>
		IDictionary<string, string> Metadata {get;}
	}

}