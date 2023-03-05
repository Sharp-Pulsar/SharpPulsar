using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Tracker.Messages;
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
namespace SharpPulsar.Tracker
{
    internal class UnAckedTopicMessageTracker<T> : UnAckedMessageTracker<T>
	{

		public UnAckedTopicMessageTracker(IActorRef unack, IActorRef consumerBase, ConsumerConfigurationData<T> conf) : base(consumerBase, unack, conf)
		{
			Receive<RemoveTopicMessages>(m => {
				RemoveTopicMessages(m.TopicName);
			});
		}
		public new static Props Prop(IActorRef unack, IActorRef consumerBase, ConsumerConfigurationData<T> conf)
        {
			return Props.Create(() => new UnAckedTopicMessageTracker<T>(unack, consumerBase, conf));
        }
		
		public virtual int RemoveTopicMessages(string topicName)
		{
			int removed = 0;
			var iterator = MessageIdPartitionMap.Keys.GetEnumerator();
			while (iterator.MoveNext())
			{
				var messageId = iterator.Current;
				if (messageId is TopicMessageId && ((TopicMessageId)messageId).TopicPartitionName.Contains(topicName))
				{					
					if (MessageIdPartitionMap.TryGetValue(messageId, out var exist))
					{
						exist.Remove(messageId);
					}
					MessageIdPartitionMap.Remove(messageId, out var remove);
					removed++;
				}
			}
			return removed;
		}

	}

}