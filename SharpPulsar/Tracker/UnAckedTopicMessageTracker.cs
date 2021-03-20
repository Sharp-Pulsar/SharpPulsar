using Akka.Actor;
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
    public class UnAckedTopicMessageTracker : UnAckedMessageTracker
	{
		public UnAckedTopicMessageTracker(IActorRef consumerBase, long ackTimeoutMillis) : base(ackTimeoutMillis, 0, consumerBase)
		{
		}

		public UnAckedTopicMessageTracker(IActorRef consumerBase, long ackTimeoutMillis, long tickDurationMillis) : base(ackTimeoutMillis, tickDurationMillis, consumerBase)
		{
			Receive<RemoveTopicMessages>(m => {
				RemoveTopicMessages(m.Topic);
			});
		}
		public static Props Prop(IActorRef consumerBase, long ackTimeoutMillis)
        {
			return Props.Create(() => new UnAckedTopicMessageTracker(consumerBase, ackTimeoutMillis));
        }
		public static Props Prop(IActorRef consumerBase, long ackTimeoutMillis, long tickDurationMillis)
        {
			return Props.Create(() => new UnAckedTopicMessageTracker(consumerBase, ackTimeoutMillis, tickDurationMillis));
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