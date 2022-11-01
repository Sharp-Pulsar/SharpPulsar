﻿
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
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker:ReceiveActor, IWithUnboundedStash
    {
        internal readonly ConcurrentDictionary<IMessageId, HashSet<IMessageId>> MessageIdPartitionMap;
        public ArrayDeque<HashSet<IMessageId>> TimePartitions { get; }
        private readonly ILoggingAdapter _log;
        private ICancelable _timeout;
        private readonly IActorRef _consumer;
        private readonly TimeSpan _tickDuration;
        private readonly TimeSpan _ackTimeout;
        private readonly IScheduler _scheduler;
        private readonly IActorRef _unack;

        public UnAckedMessageTracker(TimeSpan ackTimeoutMillis, TimeSpan tickDurationInMs, IActorRef consumer, IActorRef unack, IRedeliveryBackoff ackTimeoutRedeliveryBackoff = null)
        {
            Precondition.Condition.CheckArgument(tickDurationInMs > TimeSpan.Zero && ackTimeoutMillis >= tickDurationInMs);
            _scheduler = Context.System.Scheduler;
            _consumer = consumer;
            _log = Context.System.Log;
            _tickDuration = tickDurationInMs;
            _ackTimeout = ackTimeoutMillis;
            _unack = unack;
            if(ackTimeoutRedeliveryBackoff == null)
            {
                MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, HashSet<IMessageId>>();
                TimePartitions = new ArrayDeque<HashSet<IMessageId>>();
                var blankPartitions = (int)Math.Ceiling((double)ackTimeoutMillis.TotalMilliseconds / _tickDuration.TotalMilliseconds);
                for (var i = 0; i < blankPartitions + 1; i++)
                {
                    TimePartitions.Add(new HashSet<IMessageId>());
                }

                _timeout = _scheduler.ScheduleTellOnceCancelable(_ackTimeout, Self, RunJob.Instance, ActorRefs.NoSender);
            }
            else
            {
                MessageIdPartitionMap = null;
                TimePartitions = null;
            }
            BecomeReady();

        }
        private void BecomeReady()
        {
            Receive<Empty>(c =>
            {
                var emptied = Empty();
                Sender.Tell(emptied);
            });
            Receive<Clear>(c => Clear());
            Receive<Remove>(c =>
            {
                var removed = Remove(c.MessageId);
                Sender.Tell(removed);
            });
            Receive<RemoveMessagesTill>(c =>
            {
                var removed = RemoveMessagesTill(c.MessageId);
                Sender.Tell(removed);
            });
            Receive<Add>(c =>
            {
                bool added;
                if(c.RedeliveryCount > 0)
                    added = Add(c.MessageId, c.RedeliveryCount);
                else
                    added = Add(c.MessageId);

                Sender.Tell(added);
            });
            Receive<Size>(c =>
            {
                var size = Size();
                Sender.Tell(size);
            });
            Receive<RunJob>(_=> 
            {
                RedeliverMessages();
            });
        }
        private void RedeliverMessages()
        {
            var messagesToRedeliver = new HashSet<IMessageId>();
            try
            {
                var headPartition = TimePartitions.RemoveFirst();
                if (headPartition.Count > 0)
                {
                    _log.Warning($"[{_consumer.Path.Name}] {headPartition.Count} messages will be re-delivered");
                    headPartition.ForEach(async messageId =>
                    {
                        var ids = await _unack.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { messageId }));
                        foreach (var i in ids.MessageIds)
                            messagesToRedeliver.Add(i);

                        messagesToRedeliver.Add(messageId);
                        _unack.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { messageId }));
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
                    _timeout = _scheduler.ScheduleTellOnceCancelable(_ackTimeout, Self, RunJob.Instance, ActorRefs.NoSender);
                }
                catch
                {
                    if (messagesToRedeliver.Count > 0)
                    {
                        _consumer.Tell(new AckTimeoutSend(messagesToRedeliver));
                        _consumer.Tell(new RedeliverUnacknowledgedMessageIds(messagesToRedeliver));
                    }
                }
               
            }   

        }

        private void Clear()
        {
            try
            {
                MessageIdPartitionMap.Clear();
                TimePartitions.ForEach(tp => tp.Clear());
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

                var partition = TimePartitions.Last;
                if (!MessageIdPartitionMap.TryGetValue(messageId, out var _))
                {
                    MessageIdPartitionMap[messageId] = partition;
                    var added = partition.Add(messageId);
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
        private bool Add(IMessageId messageId, int redeliveryCount)
        {
            return Add(messageId);
        }

        private bool Empty()
        {
            try
            {
                return MessageIdPartitionMap.Count == 0;
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
                    removed = exist.Remove(messageId);
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
                        exist?.Remove(messageId);
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

        public IStash Stash { get; set; }

        protected override void PostStop()
        {
            _timeout?.Cancel();
            Clear();
        }

        public static Props Prop(TimeSpan ackTimeoutMillis, TimeSpan tickDurationInMs, IActorRef consumer, IActorRef unack, IRedeliveryBackoff ackTimeoutRedeliveryBackoff)
        {
            return Props.Create(()=> new UnAckedMessageTracker(ackTimeoutMillis, tickDurationInMs, consumer, unack, ackTimeoutRedeliveryBackoff));
        }
    }

    public sealed class RunJob
    {
        public static RunJob Instance = new RunJob();
    }
    public sealed class UnAckedChunckedMessageIdSequenceMapCmd
    {
        public UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand command, List<IMessageId> messageId)
        {
            Command = command;
            MessageId = messageId.ToImmutableList();
        }

        public UnAckedCommand Command { get; }
        public ImmutableList<IMessageId> MessageId { get; }
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
    public enum UnAckedCommand
    {
        Get,
        Remove,
        GetRemoved
    }
}
