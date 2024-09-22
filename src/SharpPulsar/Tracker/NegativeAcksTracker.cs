
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
using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Configuration;
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages.Consumer;
using System.Threading.Tasks;

namespace SharpPulsar.Tracker
{

    public class NegativeAcksTracker<T>:ReceiveActor, IWithUnboundedStash, IWithTimers
	{

		private Dictionary<IMessageId, long> _nackedMessages;
		private readonly TimeSpan _nackDelayMs;
		private readonly TimeSpan _timerIntervalMs;
        private readonly IActorRef _consumer;
        private readonly IActorRef _unack;
        private readonly IActorRef _self;
        private readonly ILoggingAdapter _log; 
        private readonly IRedeliveryBackoff _negativeAckRedeliveryBackoff;

        //private ICancelable _timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly TimeSpan MinNackDelayMs = TimeSpan.FromMilliseconds(100);

        public IStash Stash { get; set; }
        public ITimerScheduler Timers { get; set; }

        public NegativeAcksTracker(ConsumerConfigurationData<T> conf, IActorRef consumer, IActorRef unack)
        {
            _nackedMessages = new Dictionary<IMessageId, long>();
            _log = Context.GetLogger();
            _self = Self;
            _consumer = consumer;
			_nackDelayMs = conf.NegativeAckRedeliveryDelay > MinNackDelayMs? conf.NegativeAckRedeliveryDelay: MinNackDelayMs;
            _negativeAckRedeliveryBackoff = conf.NegativeAckRedeliveryBackoff;
            if (_negativeAckRedeliveryBackoff != null)
            {
                _timerIntervalMs = TimeSpan.FromMilliseconds(Math.Max(TimeSpan.FromMilliseconds(_negativeAckRedeliveryBackoff.Next(0)).TotalMilliseconds, MinNackDelayMs.TotalMilliseconds) / 3);
            }
            else
            {
                _timerIntervalMs = _nackDelayMs.Divide(3);
            }            
            _unack = unack;
            Ready();

            //_timeout = Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(_timerIntervalMs), TimeSpan.FromMilliseconds(_timerIntervalMs), _self, Trigger.Instance, ActorRefs.NoSender);
            //_timeout = Context.System.Scheduler.ScheduleTellOnceCancelable(_timerIntervalMs, _self, Trigger.Instance, ActorRefs.NoSender);
            Timers.StartSingleTimer(Trigger.Instance, Trigger.Instance, _timerIntervalMs);
        }
        private void Ready()
        {
            Receive<Add>(a =>
            {
                if (a.MessageId != null && a.RedeliveryCount > 0)
                    Add(a.MessageId);
                else
                    Add(a.MessageId);
            });
            Receive<Add<T>>(a =>
            {
                if (a.Message != null)
                    Add(a.Message);
            });
            ReceiveAsync<Trigger>(async t => 
            {
               await TriggerRedelivery();
            });
        }
       
        protected override void PostStop()
        {
            if (Timers != null)
            {
                Timers!.Cancel(Trigger.Instance);
                //Timers = null;  
               // _timeout.Cancel();
                //_timeout = null;
            }

            if (_nackedMessages != null)
            {
                _nackedMessages.Clear();
                _nackedMessages = null;
            }
            base.PostStop();
        }
        public static Props Prop(ConsumerConfigurationData<T> conf, IActorRef consumer, IActorRef unack)
        {
            return Props.Create(()=> new NegativeAcksTracker<T>(conf, consumer, unack));
        }
		private async ValueTask TriggerRedelivery()
        {
            if(_nackedMessages.Count == 0)
            {
                Timers!.Cancel(Trigger.Instance);
                //Timers = null;
                //_timeout?.Cancel();
                //_timeout = null;
                return;
            }

            var messagesToRedeliver = new HashSet<IMessageId>();
            var now = DateTimeHelper.CurrentUnixTimeMillis();
            foreach (var unack in _nackedMessages)
            {
                if (unack.Value < now)
                {
                    var ids = await _unack.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { unack.Key}));
                    foreach (var i in ids.MessageIds)
                        messagesToRedeliver.Add(i);

                    messagesToRedeliver.Add(unack.Key);
                    _unack.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { unack.Key }));
                }
            }

            _log.Info($"Number of Negatively Accked Messages to be Redelivered: {messagesToRedeliver.Count}");
            messagesToRedeliver.ForEach(i => _nackedMessages.Remove(i));
            _consumer.Tell(new OnNegativeAcksSend(messagesToRedeliver));
            _consumer.Tell(new RedeliverUnacknowledgedMessageIds(messagesToRedeliver));

            Timers.StartSingleTimer(Trigger.Instance, Trigger.Instance, _timerIntervalMs);
            //_timeout = Context.System.Scheduler.ScheduleTellOnceCancelable(_timerIntervalMs, _self, Trigger.Instance, ActorRefs.NoSender);
        }
        private void Add(IMessageId messageId)
        {
            Add(messageId, 0);
        }

        private void Add(IMessage<T> message)
        {
            Add(message.MessageId, message.RedeliveryCount);
        }
        private void Add(IMessageId messageId, int redeliveryCount)
        {
            if (_nackedMessages == null)
            {
                _nackedMessages = new Dictionary<IMessageId, long>();
            }

            long backoffNs;
            if (_negativeAckRedeliveryBackoff != null)
            {
                backoffNs = (long)TimeSpan.FromMilliseconds(_negativeAckRedeliveryBackoff.Next(redeliveryCount)).TotalMilliseconds;
            }
            else
            {
                backoffNs = (long)_nackDelayMs.TotalMilliseconds;
            }

            _nackedMessages[MessageIdAdvUtils.DiscardBatch(messageId)] = DateTimeHelper.CurrentUnixTimeMillis() + backoffNs;

            //if (Timers == null)
            //{
                // Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
                // nack immediately following the current one will be batched into the same redeliver request.
            Timers.StartSingleTimer(Trigger.Instance, Trigger.Instance, _timerIntervalMs);
                //_timeout = Context.System.Scheduler.ScheduleTellOnceCancelable(_timerIntervalMs, _self, Trigger.Instance, ActorRefs.NoSender);
            //}
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
