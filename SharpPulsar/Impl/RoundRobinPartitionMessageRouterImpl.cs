using System;
using System.Collections.Concurrent;
using SharpPulsar.Utility;

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
	using Api;
	using ITopicMetadata = Api.ITopicMetadata;

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

		private const long SerialVersionUid = 1L;

		private static readonly ConcurrentDictionary<RoundRobinPartitionMessageRouterImpl, int> PartitionIndexUpdater = new ConcurrentDictionary<RoundRobinPartitionMessageRouterImpl, int>();

		private readonly int _startPtnIdx;
		private readonly bool _isBatchingEnabled;
		private readonly long _partitionSwitchMs;

		private readonly DateTime _clock;

		private static readonly DateTime SystemClock = DateTime.UtcNow;

		public RoundRobinPartitionMessageRouterImpl(HashingScheme hashingScheme, int startPtnIdx, bool isBatchingEnabled, long partitionSwitchMs) : this(hashingScheme, startPtnIdx, isBatchingEnabled, partitionSwitchMs, SystemClock)
		{
		}

		public RoundRobinPartitionMessageRouterImpl(HashingScheme hashingScheme, int startPtnIdx, bool isBatchingEnabled, long partitionSwitchMs, DateTime clock) : base(hashingScheme)
		{
			PartitionIndexUpdater[this] = startPtnIdx;
			_startPtnIdx = startPtnIdx;
			_isBatchingEnabled = isBatchingEnabled;
			_partitionSwitchMs = Math.Max(1, partitionSwitchMs);
			_clock = clock;
		}

		public override int ChoosePartition<T1>(IMessage<T1> msg, ITopicMetadata topicMetadata)
		{
			// If the message has a key, it supersedes the round robin routing policy
			if (msg.HasKey())
			{
				return MathUtils.SignSafeMod(Hash.MakeHash(msg.Key), topicMetadata.NumPartitions());
			}

			if (_isBatchingEnabled)
			{ // if batching is enabled, choose partition on `partitionSwitchMs` boundary.
				long currentMs = _clock.Millisecond;
				return MathUtils.SignSafeMod(currentMs / _partitionSwitchMs + _startPtnIdx, topicMetadata.NumPartitions());
			}

            PartitionIndexUpdater[this] = PartitionIndexUpdater[this]++;
            return MathUtils.SignSafeMod(PartitionIndexUpdater[this], topicMetadata.NumPartitions());
        }

		public override int ChoosePartition<T1>(IMessage<T1> msg)
		{
			throw new NotImplementedException();
		}
	}

}