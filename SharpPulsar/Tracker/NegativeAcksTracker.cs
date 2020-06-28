
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
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Tracker
{

	public class NegativeAcksTracker
	{

		private Dictionary<MessageId, long> _nackedMessages;
        private ActorSystem _system;

		private readonly IActorRef _consumer;
		private readonly long _nackDelayNanos;
		private readonly long _timerIntervalNanos;

		private ICancelable _timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly long MinNackDelayNanos = 100;

		public NegativeAcksTracker(ActorSystem system, IActorRef consumer, ConsumerConfigurationData conf)
        {
            _system = system;
			_consumer = consumer;
			_nackDelayNanos = Math.Max(conf.NegativeAckRedeliveryDelayMicros, MinNackDelayNanos);
			_timerIntervalNanos = _nackDelayNanos / 3;
		}

		private void TriggerRedelivery()
        {
            if (_nackedMessages.Count == 0)
            {
                _timeout = null;
                return;
            }

            // Group all the nacked messages into one single re-delivery request
            ISet<MessageId> messagesToRedeliver = new HashSet<MessageId>();
            var now = DateTimeHelper.CurrentUnixTimeMillis();
            _nackedMessages.ForEach(a =>
            {
                if (a.Value < now)
                {
                    UnAckedMessageTracker.AddChunkedMessageIdsAndRemoveFromSequnceMap(a.Key, messagesToRedeliver, _consumer);
                    messagesToRedeliver.Add(a.Key);
                }
            });

            messagesToRedeliver.ForEach(a =>_nackedMessages.Remove(a));
            _consumer.Tell(new OnNegativeAcksSend(messagesToRedeliver));
            _consumer.Tell(new RedeliverUnacknowledgedMessages(messagesToRedeliver));

            _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(_timerIntervalNanos), TriggerRedelivery);
        }

        public virtual void Add(MessageId messageId)
        {
            lock (this)
            {
                if (messageId is BatchMessageId batchMessageId)
                {
                    messageId = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex, null);
                }

                if (_nackedMessages == null)
                {
                    _nackedMessages = new Dictionary<MessageId, long>();
                }
                _nackedMessages[messageId] = DateTimeHelper.CurrentUnixTimeMillis() + _nackDelayNanos;

                if (_timeout == null)
                {
                    // Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
                    // nack immediately following the current one will be batched into the same redeliver request.
                    _timeout = _timeout = _system.Scheduler.Advanced.ScheduleOnceCancelable(TimeSpan.FromSeconds(_timerIntervalNanos), TriggerRedelivery);
                }
            }
        }
	}
    public sealed class OnNegativeAcksSend
    {
        public OnNegativeAcksSend(ISet<MessageId> messageIds)
        {
            MessageIds = messageIds;
        }

        public ISet<MessageId> MessageIds { get; }
    }
}
