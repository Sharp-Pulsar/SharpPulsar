using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Tracker.Messages;

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
    internal class UnAckedTopicMessageRedeliveryTracker<T> : UnAckedMessageRedeliveryTracker<T>
    {

        public UnAckedTopicMessageRedeliveryTracker(IActorRef consumer, IActorRef unack, ConsumerConfigurationData<T> conf) : base(consumer, unack, conf)
        {

            Receive<RemoveTopicMessages>(m => {
                RemoveTopicMessages(m.TopicName);
            });
        }
        public new static Props Prop(IActorRef consumer, IActorRef unack, ConsumerConfigurationData<T> conf)
        {
            return Props.Create(() => new UnAckedTopicMessageRedeliveryTracker<T>(consumer, unack, conf));
        }

        internal virtual int RemoveTopicMessages(string topicName)
        {
            try
            {
                var removed = 0;
                var iterator = RedeliveryMessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
                while (iterator.MoveNext())
                {
                    var entry = iterator.Current;
                    var messageIdWrapper = entry.Key;
                    var messageId = messageIdWrapper.MessageId;
                    if (messageId is TopicMessageId && ((TopicMessageId)messageId).TopicPartitionName.Contains(topicName))
                    {
                        var exist = RedeliveryMessageIdPartitionMap[messageIdWrapper];
                        entry.Value.Remove(messageIdWrapper);
                        //iterator.Remove();
                        messageIdWrapper.Recycle();
                        removed++;
                    }
                }

                var iteratorAckTimeOut = AckTimeoutMessages.Keys.GetEnumerator();
                while (iterator.MoveNext())
                {
                    var messageId = iteratorAckTimeOut.Current;
                    if (messageId is TopicMessageId && ((TopicMessageId)messageId).TopicPartitionName.Contains(topicName))
                    {
                        //Iterator.remove();
                        removed++;
                    }
                }
                return removed;
            }
            finally
            {
               
            }
        }

    }

}