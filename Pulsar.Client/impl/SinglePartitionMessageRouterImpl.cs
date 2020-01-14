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

	public class SinglePartitionMessageRouterImpl : MessageRouterBase
	{

		private const long serialVersionUID = 1L;

		private readonly int partitionIndex;

		public SinglePartitionMessageRouterImpl(int partitionIndex, HashingScheme hashingScheme) : base(hashingScheme)
		{
			this.partitionIndex = partitionIndex;
		}

		public override int choosePartition<T1>(Message<T1> msg, TopicMetadata metadata)
		{
			// If the message has a key, it supersedes the single partition routing policy
			if (msg.hasKey())
			{
				return signSafeMod(hash.makeHash(msg.Key), metadata.numPartitions());
			}

			return partitionIndex;
		}

	}

}