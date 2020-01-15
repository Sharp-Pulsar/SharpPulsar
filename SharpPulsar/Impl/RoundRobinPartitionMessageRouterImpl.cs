using System;

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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.util.MathUtils.signSafeMod;


	using HashingScheme = org.apache.pulsar.client.api.HashingScheme;
	using Message = org.apache.pulsar.client.api.Message;
	using TopicMetadata = org.apache.pulsar.client.api.TopicMetadata;

	/// <summary>
	/// The routing strategy here:
	/// <ul>
	/// <li>If a key is present, choose a partition based on a hash of the key.
	/// <li>If no key is present, choose a partition in a "round-robin" fashion. Batching-Awareness is built-in to improve
	/// batching locality.
	/// </ul>
	/// </summary>
	public class RoundRobinPartitionMessageRouterImpl : MessageRouterBase
	{

		private const long serialVersionUID = 1L;

		private static readonly AtomicIntegerFieldUpdater<RoundRobinPartitionMessageRouterImpl> PARTITION_INDEX_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(RoundRobinPartitionMessageRouterImpl), "partitionIndex");
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int partitionIndex = 0;
		private volatile int partitionIndex = 0;

		private readonly int startPtnIdx;
		private readonly bool isBatchingEnabled;
		private readonly long partitionSwitchMs;

		private readonly Clock clock;

		private static readonly Clock SYSTEM_CLOCK = Clock.systemUTC();

		public RoundRobinPartitionMessageRouterImpl(HashingScheme hashingScheme, int startPtnIdx, bool isBatchingEnabled, long partitionSwitchMs) : this(hashingScheme, startPtnIdx, isBatchingEnabled, partitionSwitchMs, SYSTEM_CLOCK)
		{
		}

		public RoundRobinPartitionMessageRouterImpl(HashingScheme hashingScheme, int startPtnIdx, bool isBatchingEnabled, long partitionSwitchMs, Clock clock) : base(hashingScheme)
		{
			PARTITION_INDEX_UPDATER.set(this, startPtnIdx);
			this.startPtnIdx = startPtnIdx;
			this.isBatchingEnabled = isBatchingEnabled;
			this.partitionSwitchMs = Math.Max(1, partitionSwitchMs);
			this.clock = clock;
		}

		public override int choosePartition<T1>(Message<T1> msg, TopicMetadata topicMetadata)
		{
			// If the message has a key, it supersedes the round robin routing policy
			if (msg.hasKey())
			{
				return signSafeMod(hash.makeHash(msg.Key), topicMetadata.numPartitions());
			}

			if (isBatchingEnabled)
			{ // if batching is enabled, choose partition on `partitionSwitchMs` boundary.
				long currentMs = clock.millis();
				return signSafeMod(currentMs / partitionSwitchMs + startPtnIdx, topicMetadata.numPartitions());
			}
			else
			{
				return signSafeMod(PARTITION_INDEX_UPDATER.getAndIncrement(this), topicMetadata.numPartitions());
			}
		}

	}

}