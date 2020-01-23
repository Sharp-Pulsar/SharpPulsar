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

	using HashingScheme = SharpPulsar.Api.HashingScheme;
	using SharpPulsar.Api;
	using TopicMetadata = SharpPulsar.Api.TopicMetadata;

	[Serializable]
	public class SinglePartitionMessageRouterImpl : MessageRouterBase
	{

		private const long SerialVersionUID = 1L;

		private readonly int partitionIndex;

		public SinglePartitionMessageRouterImpl(int PartitionIndex, HashingScheme HashingScheme) : base(HashingScheme)
		{
			this.partitionIndex = PartitionIndex;
		}

		public override int ChoosePartition<T1>(Message<T1> Msg, TopicMetadata Metadata)
		{
			// If the message has a key, it supersedes the single partition routing policy
			if (Msg.hasKey())
			{
				return signSafeMod(Hash.makeHash(Msg.Key), Metadata.numPartitions());
			}

			return partitionIndex;
		}

	}

}