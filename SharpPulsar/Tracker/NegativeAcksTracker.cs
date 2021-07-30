
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Batch;
using SharpPulsar.Configuration;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System.Threading.Tasks;
using Akka.Event;

namespace SharpPulsar.Tracker
{

    public class NegativeAcksTracker<T>:ReceiveActor, IWithUnboundedStash
	{

		private Dictionary<IMessageId, long> _nackedMessages;
		private readonly long _nackDelayMs;
		private readonly long _timerIntervalMs;
        private IActorRef _consumer;
        private IActorRef _self;
        private ILoggingAdapter _log;
        private HashSet<IMessageId> _unAckedChunckedMessageIdSequences;
        private bool _redeliveringMessages = false;


        private ICancelable _timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly long MinNackDelayMs = 100;

        public IStash Stash { get; set; }

        public NegativeAcksTracker(ConsumerConfigurationData<T> conf, IActorRef consumer)
        {
            _log = Context.GetLogger();
            _self = Self;
            _consumer = consumer;
			_nackDelayMs = Math.Max(conf.NegativeAckRedeliveryDelayMs, MinNackDelayMs);
			_timerIntervalMs = _nackDelayMs / 3;
            Ready();
            _timeout = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(_timerIntervalMs), TimeSpan.FromMilliseconds(_timerIntervalMs), _self, Trigger.Instance, ActorRefs.NoSender);

        }
        private void Ready()
        {
            Receive<Add>(a => Add(a.MessageId));
            Receive<Trigger>(t => 
            {
               TriggerRedelivery();
            });

            Receive<UnAckedChunckedMessageIdSequenceMapCmdResponse>(res =>
            {
                var messageIds = _unAckedChunckedMessageIdSequences.ToList();
                messageIds.AddRange(res.MessageIds);
                _log.Info($"Number of Negatively Accked Messages to be Redelivered: {messageIds.Count}");
                if (messageIds.Count > 0)
                {
                    messageIds.ForEach(a => _nackedMessages.Remove(a));
                    _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, messageIds));
                    _consumer.Tell(new OnNegativeAcksSend(messageIds.ToHashSet()));
                    _consumer.Tell(new RedeliverUnacknowledgedMessageIds(messageIds.ToHashSet()));
                }
                _redeliveringMessages = false;
            });
        }
        public static Props Prop(ConsumerConfigurationData<T> conf, IActorRef consumer)
        {
            return Props.Create(()=> new NegativeAcksTracker<T>(conf, consumer));
        }
		private void TriggerRedelivery()
        {
            if(!_redeliveringMessages)
            {
                // Group all the nacked messages into one single re-delivery request
                _unAckedChunckedMessageIdSequences = new HashSet<IMessageId>();
                if (_nackedMessages?.Count > 0)
                {
                    var now = DateTimeHelper.CurrentUnixTimeMillis();
                    foreach (var unack in _nackedMessages)
                    {
                        if (unack.Value < now)
                        {
                            _unAckedChunckedMessageIdSequences.Add(unack.Key);
                        }
                    }
                    _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, _unAckedChunckedMessageIdSequences.ToList()));
                    _redeliveringMessages = true;
                }
            }
        }

        private void Add(IMessageId messageId)
        {
            if (messageId is BatchMessageId batchMessageId)
            {
                messageId = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
            }

            if (_nackedMessages == null)
            {
                _nackedMessages = new Dictionary<IMessageId, long>();
            }
            _nackedMessages[messageId] = DateTimeHelper.CurrentUnixTimeMillis() + _nackDelayMs;

            if (_timeout == null)
            {
                // Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
                // nack immediately following the current one will be batched into the same redeliver request.
                _timeout = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_timerIntervalMs), Self, Trigger.Instance, ActorRefs.NoSender);
            }
        }
	}
    public sealed class OnNegativeAcksSend
    {
        public OnNegativeAcksSend(ISet<IMessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public ISet<IMessageId> MessageIds { get; }
    }

    public sealed class Trigger
    {
        public static Trigger Instance = new Trigger();
    }
}
