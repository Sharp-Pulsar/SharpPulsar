
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
using System.Threading;
using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using SharpPulsar.Api;
using SharpPulsar.Impl;
namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker
    {
        internal readonly ConcurrentDictionary<IMessageId, ConcurrentSet<IMessageId>> MessageIdPartitionMap;
        private readonly ILoggingAdapter _log;
        private readonly ActorSystem _system;
        private ICancelable _timeout;
        private static IActorRef _consumer;
        
        private ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();
        public static readonly UnAckedMessageTrackerDisabled UnackedMessageTrackerDisabled = new UnAckedMessageTrackerDisabled();

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
            TimePartitions = null;
            MessageIdPartitionMap = null;
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
            _tickDurationInMs = tickDurationInMs;
            _system = system;
            _log = system.Log;
            MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, ConcurrentSet<IMessageId>>();
            TimePartitions = new Queue<ConcurrentSet<IMessageId>>();

            var blankPartitions = (int) Math.Ceiling((double) ackTimeoutMillis / _tickDurationInMs);
            for (var i = 0; i < blankPartitions + 1; i++)
            {
                TimePartitions.Enqueue(new ConcurrentSet<IMessageId>(1, 16));
            }

            _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(tickDurationInMs), Job);

        }

        private void Job()
        {
            var messageIds = new SortedSet<IMessageId>();
            _rwl.EnterWriteLock();
            try
            {
                TimePartitions.TryDequeue(out var headPartition);
                if (headPartition?.Count > 0)
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
                TimePartitions.Enqueue(headPartition);
            }
            finally
            {
                if (messageIds.Count > 0)
                {
                    _consumer.Tell(new AckTimeoutSend(messageIds));
                    _consumer.Tell(new RedeliverUnacknowledgedMessages(messageIds));
                }

                _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(_tickDurationInMs), Job);
                _rwl.ExitWriteLock();
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
            _rwl.EnterWriteLock();
            try
            {
                MessageIdPartitionMap.Clear();
                foreach (var t in TimePartitions)
                {
                    t.Clear();
                }
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public virtual  bool Add(IMessageId messageId)
        {
            _rwl.EnterWriteLock();
            try
            {
                var partition = TimePartitions.Last();
                MessageIdPartitionMap.TryGetValue(messageId, out var previousPartition);
                if (previousPartition == null)
                {
                    var added = partition.TryAdd(messageId);
                    MessageIdPartitionMap[messageId] = partition;
                    return added;
                }

                return false;
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public virtual bool Empty()
        {
            _rwl.EnterReadLock();
            try
            {
                return MessageIdPartitionMap.IsEmpty;
            }
            finally
            {
                _rwl.ExitReadLock();
            }
        }

        public virtual bool Remove(IMessageId messageId)
        {
            _rwl.EnterWriteLock();
            try
            {
                var removed = false;
                MessageIdPartitionMap.Remove(messageId, out var exist);
                if (exist != null)
                {
                    removed = exist.TryRemove(messageId);
                }

                return removed;
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public virtual long Size()
        { 
            _rwl.EnterReadLock();
            try
            {
                return MessageIdPartitionMap.Count;
            }
            finally
            {
                _rwl.ExitReadLock();
            }
        }

        public virtual int RemoveMessagesTill(IMessageId msgId)
        { 
            _rwl.EnterWriteLock();
            try
            {
                var removed = 0;
                var iterator = MessageIdPartitionMap.Keys;
                foreach (var i in iterator)
                {
                    var messageId = i;
                    if (messageId.CompareTo(msgId) <= 0)
                    {
                        var exist = MessageIdPartitionMap[messageId];
                        exist?.TryRemove(messageId);
                        MessageIdPartitionMap.Remove(i, out var remove);
                        removed++;
                    }
                }

                return removed;
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
        }

        public Queue<ConcurrentSet<IMessageId>> TimePartitions { get; }
        public void Stop()
        {
            _rwl.EnterWriteLock();
            try
            {
                if (_timeout != null && !_timeout.IsCancellationRequested)
                {
                    _timeout.Cancel();
                }

                Clear();
            }
            finally
            {
                _rwl.ExitWriteLock();
            }
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

    public sealed class AckTimeoutSend
    {
        public AckTimeoutSend(ISet<IMessageId> messageIds)
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
