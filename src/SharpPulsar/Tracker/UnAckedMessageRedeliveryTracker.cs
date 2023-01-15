using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;

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

    internal class UnAckedMessageRedeliveryTracker<T> : UnAckedMessageTracker<T>
    {
        private readonly ILoggingAdapter _log;
        private ICancelable _timeout;
        protected internal readonly Dictionary<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>> RedeliveryMessageIdPartitionMap;
        protected internal readonly ArrayDeque<HashSet<UnackMessageIdWrapper>> RedeliveryTimePartitions;
        private readonly IScheduler _scheduler;
        protected internal readonly Dictionary<IMessageId, long> AckTimeoutMessages;
        private readonly IRedeliveryBackoff _ackTimeoutRedeliveryBackoff;
        private UnackMessageIdWrapper _unackMessageIdWrapper;
        public UnAckedMessageRedeliveryTracker(IActorRef consumer, IActorRef unack, ConsumerConfigurationData<T> conf) : base(consumer, unack, conf)
        {
            _unackMessageIdWrapper = new UnackMessageIdWrapper();
            _ackTimeoutRedeliveryBackoff = conf.AckTimeoutRedeliveryBackoff;
            AckTimeoutMessages = new Dictionary<IMessageId, long>();
            RedeliveryMessageIdPartitionMap = new Dictionary<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>>();
            RedeliveryTimePartitions = new ArrayDeque<HashSet<UnackMessageIdWrapper>>();

            var BlankPartitions = (int)Math.Ceiling((double)AckTimeout / TickDuration);
            for (var I = 0; I < BlankPartitions + 1; I++)
            {
                RedeliveryTimePartitions.AddLast(new HashSet<UnackMessageIdWrapper>());
            }
            _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TickDuration), Self, RunJob.Instance, ActorRefs.NoSender);
           // Timeout = Client.Timer().newTimeout(new TimerTaskAnonymousInnerClass(this, Client, ConsumerBase), this.TickDurationInMs, TimeUnit.MILLISECONDS);

        }
        internal override void RedeliverMessages()
        {
            try
            {
                var headPartition = RedeliveryTimePartitions.RemoveFirst();
                if (headPartition.Count > 0)
                {
                    headPartition.ForEach(unackMessageIdWrapper =>
                    {
                        AddAckTimeoutMessages(unackMessageIdWrapper);
                        RedeliveryMessageIdPartitionMap.Remove(unackMessageIdWrapper);
                        unackMessageIdWrapper.Recycle();
                    });
                }
                headPartition.Clear();
                RedeliveryTimePartitions.AddLast(headPartition);
                TriggerRedelivery(Unack);
            }
            finally
            {
                _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(TickDuration), Self, RunJob.Instance, ActorRefs.NoSender);
            }
            MessageIds.Clear();
            var messagesToRedeliver = new HashSet<IMessageId>();
            try
            {
                var headPartition = TimePartitions.RemoveFirst();
                if (headPartition.Count > 0)
                {
                    _log.Warning($"[{Consumer.Path.Name}] {headPartition.Count} messages will be re-delivered");
                    headPartition.ForEach(async messageId =>
                    {
                        await AddChunkedMessageIdsAndRemoveFromSequenceMap(messageId, MessageIds, Unack);
                        MessageIds.Add(messageId);
                        _ = MessageIdPartitionMap.TryRemove(messageId, out var _);
                    });
                    headPartition.Clear();
                    TimePartitions.AddLast(headPartition);
                }
            }
            finally
            {
                try
                {
                    _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(AckTimeout), Self, RunJob.Instance, ActorRefs.NoSender);
                }
                catch
                {
                    if (messagesToRedeliver.Count > 0)
                    {
                        Consumer.Tell(new AckTimeoutSend(messagesToRedeliver));
                        Consumer.Tell(new RedeliverUnacknowledgedMessageIds(messagesToRedeliver));
                    }
                }

            }

        }
        

        private void AddAckTimeoutMessages(UnackMessageIdWrapper messageIdWrapper)
        {
            try
            {
                var messageId = messageIdWrapper.MessageId;
                var redeliveryCount = messageIdWrapper.RedeliveryCount;
                var backoffNs = _ackTimeoutRedeliveryBackoff.Next(redeliveryCount);
                AckTimeoutMessages[messageId] = DateTimeHelper.CurrentUnixTimeMillis() + backoffNs;
            }
            finally
            {
                
            }
        }

        private void TriggerRedelivery(IActorRef consumerBase)
        {
            if (AckTimeoutMessages.Count == 0)
            {
                return;
            }
            
            MessageIds.Clear();

            try
            {
                var now = DateTimeHelper.CurrentUnixTimeMillis();
                AckTimeoutMessages.ForEach(async d =>
                {
                    if (d.Value <= now)
                    {
                        await AddChunkedMessageIdsAndRemoveFromSequenceMap(d.Key, MessageIds, consumerBase);
                        MessageIds.Add(d.Key);
                    }
                });
                if (MessageIds.Count > 0)
                {
                    _log.Info($"[{Unack}] {MessageIds.Count} messages will be re-delivered");
                    var iterator = MessageIds.GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        var messageId = iterator.Current;
                        AckTimeoutMessages.Remove(messageId);
                    }
                }
            }
            finally
            {
                if (MessageIds.Count > 0)
                {
                     Consumer.Tell(new AckTimeoutSend(MessageIds));
                     Consumer.Tell(new RedeliverUnacknowledgedMessageIds(MessageIds));
                }
            }
        }

        internal override bool Empty
        {
            get
            {
                
                try
                {
                    return RedeliveryMessageIdPartitionMap.Count == 0 && AckTimeoutMessages.Count == 0;
                }
                finally
                {
                    
                }
            }
        }

        internal override void Clear()
        {
            try
            {
                RedeliveryMessageIdPartitionMap.Clear();
                RedeliveryTimePartitions.ForEach(tp =>
                {
                    tp.ForEach(unackMessageIdWrapper => unackMessageIdWrapper.Recycle());
                    tp.Clear();
                });
                AckTimeoutMessages.Clear();
            }
            finally
            {
                
            }
        }

        internal override bool Add(IMessageId messageId)
        {
            return Add(messageId, 0);
        }

        internal override bool Add(IMessageId messageId, int redeliveryCount)
        {
            try
            {
                var messageIdWrapper = _unackMessageIdWrapper.ValueOf(messageId, redeliveryCount);
                var partition = RedeliveryTimePartitions.Last;
                var previousPartition = RedeliveryMessageIdPartitionMap.PutIfAbsent(messageIdWrapper, partition);
                if (previousPartition == null)
                {
                    return partition.Add(messageIdWrapper);
                }
                else
                {
                    messageIdWrapper.Recycle();
                    return false;
                }
            }
            finally
            {
                
            }
        }

        internal override bool Remove(IMessageId messageId)
        {
            var messageIdWrapper = _unackMessageIdWrapper.ValueOf(messageId);
            try
            {
                var removed = false;
                var exist = RedeliveryMessageIdPartitionMap.Remove(messageIdWrapper);
                if (exist != null)
                {
                    removed = exist;
                }
                return removed || AckTimeoutMessages.Remove(messageId) != null;
            }
            finally
            {
                messageIdWrapper.Recycle();
            }
        }

        internal override long Size()
        {
            try
            {
                return RedeliveryMessageIdPartitionMap.Count + AckTimeoutMessages.Count;
            }
            finally
            {
                
            }
        }

        internal override int RemoveMessagesTill(IMessageId msgId)
        {
            try
            {
                var removed = 0;
                var iterator = RedeliveryMessageIdPartitionMap.SetOfKeyValuePairs().GetEnumerator();
                while (iterator.MoveNext())
                {
                    var entry = iterator.Current;
                    var messageIdWrapper = entry.Key;
                    if (messageIdWrapper.MessageId.CompareTo(msgId) <= 0)
                    {
                        entry.Value.Remove(messageIdWrapper);
                        //iterator.Remove();
                        messageIdWrapper.Recycle();
                        removed++;
                    }
                }

                var iteratorAckTimeOut = AckTimeoutMessages.Keys.GetEnumerator();
                while (iteratorAckTimeOut.MoveNext())
                {
                    var messageId = iteratorAckTimeOut.Current;
                    if (messageId.CompareTo(msgId) <= 0)
                    {
                      
                        //iteratorAckTimeOut.remove();
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