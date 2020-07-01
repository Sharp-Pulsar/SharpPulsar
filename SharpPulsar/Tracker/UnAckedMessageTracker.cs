
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Api;
using SharpPulsar.Impl;
namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker
    {
        internal readonly ConcurrentDictionary<IMessageId, HashSet<IMessageId>> MessageIdPartitionMap;
        private readonly Queue<HashSet<IMessageId>> _timePartitions;
        private readonly ILoggingAdapter _log;
        private readonly ActorSystem _system;
        private ICancelable _timeout;
        private static IActorRef _consumer;

        public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled =
            new UnAckedMessageTrackerDisabled();

        private readonly long _ackTimeoutMillis;
        private readonly long _tickDurationInMs;

        public class UnAckedMessageTrackerDisabled : UnAckedMessageTracker
        {

            public override void Clear()
            {
            }

            public override long Size()
            {
                return 0;
            }

            public override bool Add(IMessageId m)
            {
                return true;
            }

            public override bool Remove(IMessageId m)
            {
                return true;
            }

            public override int RemoveMessagesTill(IMessageId msgId)
            {
                return 0;
            }

            public virtual void Dispose()
            {
            }
        }

        public UnAckedMessageTracker()
        {
            _system = null;
            _timePartitions = null;
            MessageIdPartitionMap = null;
            _ackTimeoutMillis = 0;
            _tickDurationInMs = 0;
        }

        public UnAckedMessageTracker(IActorRef consumer, long ackTimeoutMillis, ActorSystem system) : this(consumer,
            ackTimeoutMillis, ackTimeoutMillis, system)
        {
        }



        public UnAckedMessageTracker(IActorRef consumer, long ackTimeoutMillis, long tickDurationInMs, ActorSystem system)
        {
            _consumer = consumer;
            Precondition.Condition.CheckArgument(tickDurationInMs > 0 && ackTimeoutMillis >= tickDurationInMs);
            _ackTimeoutMillis = ackTimeoutMillis;
            _tickDurationInMs = tickDurationInMs;
            _system = system;
            MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, HashSet<IMessageId>>();
            _timePartitions = new Queue<HashSet<IMessageId>>();

            var blankPartitions = (int) Math.Ceiling((double) _ackTimeoutMillis / _tickDurationInMs);
            for (var i = 0; i < blankPartitions + 1; i++)
            {
                _timePartitions.Enqueue(new HashSet<IMessageId>(16));
            }

            _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(tickDurationInMs), Job);

        }

        private void Job()
        {
            var messageIds = new HashSet<IMessageId>();
            try
            {
                var headPartition = _timePartitions.FirstOrDefault();
                if (headPartition != null && headPartition.Count > 0)
                {
                    _log.Warning($"[{_consumer.Path.Name}] {headPartition.Count} messages have timed-out");
                    headPartition.ForEach(messageId =>
                    {
                        AddChunkedMessageIdsAndRemoveFromSequnceMap(messageId, messageIds, _consumer);
                        messageIds.Add(messageId);
                        MessageIdPartitionMap.Remove(messageId, out var m);
                    });
                }

                headPartition?.Clear();
                _timePartitions.Enqueue(headPartition);
            }
            finally
            {
                if (messageIds.Count > 0)
                {
                    _consumer.Tell(new OnAckTimeoutSend(messageIds));
                    _consumer.Tell(new RedeliverUnacknowledgedMessages(messageIds));
                }

                _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_tickDurationInMs), Job);
            }
        }

        public static void AddChunkedMessageIdsAndRemoveFromSequnceMap(IMessageId messageId, ISet<IMessageId> messageIds, IActorRef consumer)
        {
            if (messageId is MessageId id)
            {
                //use ask here
                var chunkedMsgIds = consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, id)).GetAwaiter().GetResult();
                if (chunkedMsgIds != null && chunkedMsgIds.MessageIds.Length> 0)
                {
                    foreach (var msgId in chunkedMsgIds.MessageIds)
                    {
                        messageIds.Add(msgId);
                    }
                }

                consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, id));
            }
        }

        public virtual void Clear()
        {
            MessageIdPartitionMap.Clear();
            foreach (var t in _timePartitions)
            {
                t.Clear();
            }
        }

        public virtual  bool Add(IMessageId messageId)
        {
            var partition = _timePartitions.LastOrDefault();
            var previousPartition = MessageIdPartitionMap.GetOrAdd(messageId, p => partition);
            if (previousPartition?.Count == 0)
            {
                return partition.Add(messageId);
            }

            return false;
        }

        public virtual bool Empty => MessageIdPartitionMap.IsEmpty;

        public virtual bool Remove(IMessageId messageId)
        {
            var removed = false;
            MessageIdPartitionMap.Remove(messageId, out var exist);
            if (exist != null)
            {
                removed = exist.Remove(messageId);
            }

            return removed;
        }

        public virtual long Size()
        {
            return MessageIdPartitionMap.Count;
        }

        public virtual int RemoveMessagesTill(IMessageId msgId)
        {
            var removed = 0;
            var iterator = MessageIdPartitionMap.Keys;
            foreach (var i in iterator)
            {
                var messageId = i;
                if (messageId.CompareTo(msgId) <= 0)
                {
                    var exist = MessageIdPartitionMap[messageId];
                    exist?.Remove(messageId);
                    MessageIdPartitionMap.Remove(i, out var remove);
                    removed++;
                }
            }

            return removed;
        }

        public Queue<HashSet<IMessageId>> TimePartitions => _timePartitions;
        public void Stop()
        {
            if (_timeout != null && !_timeout.IsCancellationRequested)
            {
                _timeout.Cancel();
            }

            Clear();
        }

    }

    public sealed class UnAckedChunckedMessageIdSequenceMapCmd
    {
        public UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand command, IMessageId messageId)
        {
            Command = command;
            MessageId = messageId;
        }

        public UnAckedCommand Command { get; }
        public IMessageId MessageId { get; }
    }

    public sealed class UnAckedChunckedMessageIdSequenceMapCmdResponse
    {
        public UnAckedChunckedMessageIdSequenceMapCmdResponse(MessageId[] messageIds)
        {
            MessageIds = messageIds;
        }

        public MessageId[] MessageIds { get; }
    }

    public sealed class OnAckTimeoutSend
    {
        public OnAckTimeoutSend(ISet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public ISet<IMessageId> MessageIds { get; }
    }
    public sealed class RedeliverUnacknowledgedMessages
    {
        public RedeliverUnacknowledgedMessages(ISet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public ISet<IMessageId> MessageIds { get; }
    }
    public enum UnAckedCommand
    {
        Get,
        Remove
    }
}
