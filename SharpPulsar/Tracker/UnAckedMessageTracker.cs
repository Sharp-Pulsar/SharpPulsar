
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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using Akka.Util;
using Akka.Util.Internal;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker:ReceiveActor
    {
        internal readonly ConcurrentDictionary<IMessageId, ConcurrentSet<IMessageId>> MessageIdPartitionMap;
        private readonly ILoggingAdapter _log;
        private ICancelable _timeout;
        private static IActorRef _parent;
        private readonly long _tickDurationInMs;
        private readonly long _ackTimeoutMillis;
        private readonly IScheduler _scheduler;
        
        public UnAckedMessageTracker(long ackTimeoutMillis, long tickDurationInMs)
        {
            _scheduler = Context.System.Scheduler;
            _parent = Context.Parent;
            _log = Context.System.Log;
            Precondition.Condition.CheckArgument(tickDurationInMs > 0 && ackTimeoutMillis >= tickDurationInMs);
            _tickDurationInMs = tickDurationInMs;
            _ackTimeoutMillis = ackTimeoutMillis;
            MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, ConcurrentSet<IMessageId>>();
            TimePartitions = new Queue<ConcurrentSet<IMessageId>>();

            var blankPartitions = (int) Math.Ceiling((double) ackTimeoutMillis / _tickDurationInMs);
            for (var i = 0; i < blankPartitions + 1; i++)
            {
                TimePartitions.Enqueue(new ConcurrentSet<IMessageId>(1, 16));
            }
            BecomeReady();
        }

        private void BecomeReady()
        {
            Receive<Empty>(c =>
            {
                var emptied = Empty();
                //Sender.Tell(emptied);
            });
            Receive<Clear>(c => Clear());
            Receive<Remove>(c =>
            {
                var removed = Remove(c.MessageId);
               // Sender.Tell(removed);
            });
            Receive<RemoveMessagesTill>(c =>
            {
                var removed = RemoveMessagesTill(c.MessageId);
                Sender.Tell(removed);
            });
            Receive<Add>(c =>
            {
                var added = Add(c.MessageId);
                //Sender.Tell(added);
            });
            Receive<Size>(c =>
            {
                var size = Size();
                Sender.Tell(size);
            });
            Receive<RunJob>(r=> Job());
            Receive<AddChunkedMessageIdsAndRemoveFromSequnceMap>(c =>
            {
                var ids = new HashSet<IMessageId>(c.MessageIds);
                AddChunkedMessageIdsAndRemoveFromSequnceMap(c.MessageId, ids);
                Sender.Tell(ids.ToImmutableHashSet());
            });
            _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_ackTimeoutMillis), Self, RunJob.Instance, ActorRefs.NoSender);

        }
        private void Job()
        {
            var messageIds = new SortedSet<IMessageId>();
            try
            {
                TimePartitions.TryDequeue(out var headPartition);
                if (headPartition?.Count > 0)
                {
                    _log.Warning($"[{_parent.Path.Name}] {headPartition.Count} messages have timed-out");
                    headPartition.ForEach(messageId =>
                    {
                        AddChunkedMessageIdsAndRemoveFromSequnceMap(messageId, messageIds);
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
                    _parent.Tell(new AckTimeoutSend(messageIds));
                    _parent.Tell(new RedeliverUnacknowledgedMessages(messageIds));
                }
                _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_ackTimeoutMillis), Self, RunJob.Instance, ActorRefs.NoSender);
            }
        }

        private void AddChunkedMessageIdsAndRemoveFromSequnceMap(IMessageId messageId, ISet<IMessageId> messageIds)
        {
            if (messageId is MessageId id)
            {
                //use ask here
                var chunkedMsgIds = _parent.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, id)).GetAwaiter().GetResult();
                if (chunkedMsgIds != null && chunkedMsgIds.MessageIds.Length> 0)
                {
                    foreach (var msgId in chunkedMsgIds.MessageIds)
                    {
                        messageIds.Add(msgId);
                    }
                }

                _parent.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, id));
            }
        }

        private void Clear()
        {
            try
            {
                MessageIdPartitionMap.Clear();
                foreach (var t in TimePartitions)
                {
                    t.Clear();
                }
            }

            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
        }

        private  bool Add(IMessageId messageId)
        {
            try
            {
                var partition = TimePartitions.First();
                MessageIdPartitionMap.TryGetValue(messageId, out var previousPartition);
                if (previousPartition == null)
                {
                    var added = partition.TryAdd(messageId);
                    MessageIdPartitionMap[messageId] = partition;
                    return added;
                }

                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }

        private bool Empty()
        {
            try
            {
                return MessageIdPartitionMap.IsEmpty;
            }

            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }

        private bool Remove(IMessageId messageId)
        {
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
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }
        }

        private long Size()
        {
            return MessageIdPartitionMap.Count;
        }

        private int RemoveMessagesTill(IMessageId msgId)
        { 
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
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return -1;
            }
        }

        public Queue<ConcurrentSet<IMessageId>> TimePartitions { get; }
        protected override void PostStop()
        {
            _timeout?.Cancel();
            Clear();
        }

        public static Props Prop(long ackTimeoutMillis, long tickDurationInMs)
        {
            return Props.Create(()=> new UnAckedMessageTracker(ackTimeoutMillis, tickDurationInMs));
        }
    }

    public sealed class RunJob
    {
        public static RunJob Instance = new RunJob();
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
