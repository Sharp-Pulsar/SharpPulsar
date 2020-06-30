using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Impl;

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

		public UnAckedTopicMessageTracker(IActorRef consumer, long ackTimeoutMillis, ActorSystem system) : base(consumer, ackTimeoutMillis, system)
		{
		}

		public UnAckedTopicMessageTracker(IActorRef consumer, long ackTimeoutMillis, long tickDurationInMs, ActorSystem system) : base(consumer, ackTimeoutMillis, tickDurationInMs, system)
		{
		}

		public virtual int RemoveTopicMessages(string topicName)
		{
            var removed = 0;
            var keys = _messageIdPartitionMap.Keys;
            foreach (var key in keys)
            {
                if (key is TopicMessageId impl && impl.TopicPartitionName.Contains(topicName))
                {
                    var exist = _messageIdPartitionMap[impl];
                    exist?.Remove(impl);
                    keys.Remove(key);
                    removed++;
                }
            }
            return removed;
        }

	}

}