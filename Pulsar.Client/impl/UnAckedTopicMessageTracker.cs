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
namespace org.apache.pulsar.client.impl
{
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using ConcurrentOpenHashSet = org.apache.pulsar.common.util.collections.ConcurrentOpenHashSet;

	public class UnAckedTopicMessageTracker : UnAckedMessageTracker
	{

		public UnAckedTopicMessageTracker<T1>(PulsarClientImpl client, ConsumerBase<T1> consumerBase, long ackTimeoutMillis) : base(client, consumerBase, ackTimeoutMillis)
		{
		}

		public UnAckedTopicMessageTracker<T1>(PulsarClientImpl client, ConsumerBase<T1> consumerBase, long ackTimeoutMillis, long tickDurationMillis) : base(client, consumerBase, ackTimeoutMillis, tickDurationMillis)
		{
		}

		public virtual int removeTopicMessages(string topicName)
		{
			writeLock.@lock();
			try
			{
				int removed = 0;
				IEnumerator<MessageId> iterator = messageIdPartitionMap.Keys.GetEnumerator();
				while (iterator.MoveNext())
				{
					MessageId messageId = iterator.Current;
					if (messageId is TopicMessageIdImpl && ((TopicMessageIdImpl)messageId).TopicPartitionName.Contains(topicName))
					{
						ConcurrentOpenHashSet<MessageId> exist = messageIdPartitionMap[messageId];
						if (exist != null)
						{
							exist.remove(messageId);
						}
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						iterator.remove();
						removed++;
					}
				}
				return removed;
			}
			finally
			{
				writeLock.unlock();
			}
		}

	}

}