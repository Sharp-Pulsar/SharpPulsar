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
namespace Org.Apache.Pulsar.Client.Impl
{
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using Org.Apache.Pulsar.Client.Impl.Conf;

	public class UnAckedTopicMessageRedeliveryTracker : UnAckedMessageRedeliveryTracker
	{

// JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
// ORIGINAL LINE: public UnAckedTopicMessageRedeliveryTracker(PulsarClientImpl client, ConsumerBase<?> consumerBase, org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf)
		public UnAckedTopicMessageRedeliveryTracker(PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, ConsumerConfigurationData<T2> Conf) : base(Client, ConsumerBase, Conf)
		{
		}

		public virtual int RemoveTopicMessages(string TopicName)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<KeyValuePair<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>>> Iterator = RedeliveryMessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
				while (Iterator.MoveNext())
				{
					KeyValuePair<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>> Entry = Iterator.Current;
					UnackMessageIdWrapper MessageIdWrapper = Entry.Key;
					MessageId MessageId = MessageIdWrapper.getMessageId();
					if (MessageId is TopicMessageIdImpl && ((TopicMessageIdImpl) MessageId).TopicPartitionName.Contains(TopicName))
					{
						HashSet<UnackMessageIdWrapper> Exist = RedeliveryMessageIdPartitionMap[MessageIdWrapper];
						Entry.Value.remove(MessageIdWrapper);
// JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						Iterator.remove();
						MessageIdWrapper.Recycle();
						Removed++;
					}
				}

				IEnumerator<MessageId> IteratorAckTimeOut = AckTimeoutMessages.Keys.GetEnumerator();
				while (Iterator.MoveNext())
				{
// JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					MessageId MessageId = IteratorAckTimeOut.next();
					if (MessageId is TopicMessageIdImpl && ((TopicMessageIdImpl) MessageId).TopicPartitionName.Contains(TopicName))
					{
// JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						Iterator.remove();
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