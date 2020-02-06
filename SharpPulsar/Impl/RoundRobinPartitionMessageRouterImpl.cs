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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.util.MathUtils.signSafeMod;


	using HashingScheme = Api.HashingScheme;
	using SharpPulsar.Api;
	using TopicMetadata = Api.TopicMetadata;

	/// <summary>
	/// The routing strategy here:
	/// <ul>
	/// <li>If a key is present, choose a partition based on a hash of the key.
	/// <li>If no key is present, choose a partition in a "round-robin" fashion. Batching-Awareness is built-in to improve
	/// batching locality.
	/// </ul>
	/// </summary>
	[Serializable]
	public class RoundRobinPartitionMessageRouterImpl : MessageRouterBase
	{

		private const long SerialVersionUID = 1L;

		private static readonly AtomicIntegerFieldUpdater<RoundRobinPartitionMessageRouterImpl> PARTITION_INDEX_UPDATER = AtomicIntegerFieldUpdater.newUpdater(typeof(RoundRobinPartitionMessageRouterImpl), "partitionIndex");
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile int partitionIndex = 0;
		private volatile int partitionIndex = 0;

		private readonly int startPtnIdx;
		private readonly bool isBatchingEnabled;
		private readonly long partitionSwitchMs;

		private readonly Clock clock;

		private static readonly Clock SYSTEM_CLOCK = Clock.systemUTC();

		public RoundRobinPartitionMessageRouterImpl(HashingScheme HashingScheme, int StartPtnIdx, bool IsBatchingEnabled, long PartitionSwitchMs) : this(HashingScheme, StartPtnIdx, IsBatchingEnabled, PartitionSwitchMs, SYSTEM_CLOCK)
		{
		}

		public RoundRobinPartitionMessageRouterImpl(HashingScheme HashingScheme, int StartPtnIdx, bool IsBatchingEnabled, long PartitionSwitchMs, Clock Clock) : base(HashingScheme)
		{
			PARTITION_INDEX_UPDATER.set(this, StartPtnIdx);
			this.startPtnIdx = StartPtnIdx;
			this.isBatchingEnabled = IsBatchingEnabled;
			this.partitionSwitchMs = Math.Max(1, PartitionSwitchMs);
			this.clock = Clock;
		}

		public override int ChoosePartition<T1>(Message<T1> Msg, TopicMetadata TopicMetadata)
		{
			// If the message has a key, it supersedes the round robin routing policy
			if (Msg.hasKey())
			{
				return signSafeMod(Hash.makeHash(Msg.Key), TopicMetadata.numPartitions());
			}

			if (isBatchingEnabled)
			{ // if batching is enabled, choose partition on `partitionSwitchMs` boundary.
				long CurrentMs = clock.millis();
				return signSafeMod(CurrentMs / partitionSwitchMs + startPtnIdx, TopicMetadata.numPartitions());
			}
			else
			{
				return signSafeMod(PARTITION_INDEX_UPDATER.getAndIncrement(this), TopicMetadata.numPartitions());
			}
		}

	}

}