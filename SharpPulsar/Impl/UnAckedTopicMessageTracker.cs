using SharpPulsar.Api;
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
namespace SharpPulsar.Impl
{

	public class UnAckedTopicMessageTracker<T> : UnAckedMessageTracker<T>
	{

		public UnAckedTopicMessageTracker(PulsarClientImpl Client, ConsumerBase<T> consumerBase, long AckTimeoutMillis) : base(Client, consumerBase, AckTimeoutMillis)
		{
		}

		public UnAckedTopicMessageTracker(PulsarClientImpl Client, ConsumerBase<T> consumerBase, long AckTimeoutMillis, long TickDurationMillis) : base(Client, consumerBase, AckTimeoutMillis, TickDurationMillis)
		{
		}

		public virtual int RemoveTopicMessages(string TopicName)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<IMessageId> Iterator = MessageIdPartitionMap.Keys.GetEnumerator();
				while (Iterator.MoveNext())
				{
					IMessageId MessageId = Iterator.Current;
					if (MessageId is TopicMessageIdImpl && ((TopicMessageIdImpl)MessageId).TopicPartitionName.Contains(TopicName))
					{
						var exist = MessageIdPartitionMap[MessageId];
						if (exist != null)
						{
							exist.remove(MessageId);
						}
						//Iterator.Remove();
						Removed++;
					}
				}
				return Removed;
			}
			finally
			{
				WriteLock.unlock();
			}
		}

	}

}