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

	public class UnAckedTopicMessageTracker : UnAckedMessageTracker
	{

// JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
// ORIGINAL LINE: public UnAckedTopicMessageTracker(PulsarClientImpl client, ConsumerBase<?> consumerBase, org.apache.pulsar.client.impl.conf.ConsumerConfigurationData<?> conf)
		public UnAckedTopicMessageTracker(PulsarClientImpl Client, ConsumerBase<T1> ConsumerBase, ConsumerConfigurationData<T2> Conf) : base(Client, ConsumerBase, Conf)
		{
		}

		public virtual int RemoveTopicMessages(string TopicName)
		{
			WriteLock.@lock();
			try
			{
				int Removed = 0;
				IEnumerator<KeyValuePair<MessageId, HashSet<MessageId>>> Iterator = MessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
				while (Iterator.MoveNext())
				{
					KeyValuePair<MessageId, HashSet<MessageId>> Entry = Iterator.Current;
					MessageId MessageId = Entry.Key;
					if (MessageId is TopicMessageIdImpl && ((TopicMessageIdImpl) MessageId).TopicPartitionName.Contains(TopicName))
					{
						Entry.Value.remove(MessageId);
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