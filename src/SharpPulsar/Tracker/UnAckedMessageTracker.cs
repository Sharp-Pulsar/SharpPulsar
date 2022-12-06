
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
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Tracker.Messages;

namespace SharpPulsar.Tracker
{
    public class UnAckedMessageTracker<T>:ReceiveActor, IWithUnboundedStash
    {
        internal readonly ConcurrentDictionary<IMessageId, HashSet<IMessageId>> MessageIdPartitionMap;
        public ArrayDeque<HashSet<IMessageId>> TimePartitions { get; }
        private readonly ILoggingAdapter _log;
        private ICancelable _timeout;
        internal readonly IActorRef Consumer;
        protected internal readonly long AckTimeout;
        protected internal readonly long TickDuration;
        private readonly IScheduler _scheduler;
        internal readonly IActorRef Unack;
        internal ISet<IMessageId> MessageIds;
        public UnAckedMessageTracker(IActorRef consumer, IActorRef unack, ConsumerConfigurationData<T> conf)
        {
            AckTimeout = (long)conf.AckTimeout.TotalMilliseconds;
            TickDuration = (long)Math.Min(conf.TickDuration.TotalMilliseconds, conf.AckTimeout.TotalMilliseconds);
            MessageIds = new HashSet<IMessageId>();
            Precondition.Condition.CheckArgument(conf.TickDuration > TimeSpan.Zero && conf.AckTimeout >= conf.TickDuration);
            _scheduler = Context.System.Scheduler;
            Consumer = consumer;
            Unack = unack;
            _log = Context.System.Log;
            if(conf.AckTimeoutRedeliveryBackoff == null)
            {
                MessageIdPartitionMap = new ConcurrentDictionary<IMessageId, HashSet<IMessageId>>();
                TimePartitions = new ArrayDeque<HashSet<IMessageId>>();
                var blankPartitions = (int)Math.Ceiling((double)AckTimeout / TickDuration);
                for (var i = 0; i < blankPartitions + 1; i++)
                {
                    TimePartitions.Add(new HashSet<IMessageId>());
                }

                _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(AckTimeout), Self, RunJob.Instance, ActorRefs.NoSender);
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
                var emptied = Empty;
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
            Receive<Add<T>>(c =>
            {
                var added = Add(c.Message.MessageId);
                Sender.Tell(added);
            });
            Receive<Add>(c =>
            {
                bool added;
                if (c.RedeliveryCount > 0)
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
        internal virtual void RedeliverMessages()
        {
            MessageIds.Clear();
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
            catch(Exception ex)
            {
                _log.Error(ex.ToString());
            }
            finally
            {
                try
                {
                    _timeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(AckTimeout), Self, RunJob.Instance, ActorRefs.NoSender);
                }
                catch
                {
                    if (MessageIds.Count > 0)
                    {
                        Consumer.Tell(new AckTimeoutSend(MessageIds));
                        Consumer.Tell(new RedeliverUnacknowledgedMessageIds(MessageIds));
                    }
                }
               
            }   

        }
        internal async ValueTask AddChunkedMessageIdsAndRemoveFromSequenceMap(IMessageId messageId, ISet<IMessageId> messageIds, IActorRef unack)
        {            
            if (messageId is MessageId)
            {
                var chunkedMsgIds = await Unack.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { messageId })); ;
                if (chunkedMsgIds != null && chunkedMsgIds.MessageIds.Length > 0)
                {
                    MessageIds = messageIds.Select(m => m).Concat(chunkedMsgIds.MessageIds.Select(c => c)).ToHashSet();
                }
                Unack.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { messageId }));
            }
        }
        internal virtual void Clear()
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

        internal virtual bool Add(IMessageId messageId)
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
        internal virtual bool Add(IMessageId messageId, int redeliveryCount)
        {
            return Add(messageId);
        }

        internal virtual bool Empty
        {
            get
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
            
        }

        internal virtual bool Remove(IMessageId messageId)
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

        internal virtual long Size()
        {
            return MessageIdPartitionMap.Count;
        }

        internal virtual int RemoveMessagesTill(IMessageId msgId)
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

        public static Props Prop(IActorRef consumer, IActorRef unack, ConsumerConfigurationData<T> conf)
        {
            return Props.Create(()=> new UnAckedMessageTracker<T>(consumer, unack, conf));
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
