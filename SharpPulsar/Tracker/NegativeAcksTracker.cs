
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
using BAMCIS.Util.Concurrent;
using PulsarAdmin.Models;
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using Timer = System.Timers.Timer;

namespace SharpPulsar.Tracker
{

	public class NegativeAcksTracker
	{

		private Dictionary<MessageId, long> nackedMessages = null;
        private ActorSystem _system;

		private readonly IActorRef consumer;
		private readonly Timer timer;
		private readonly long nackDelayNanos;
		private readonly long timerIntervalNanos;

		private Timer timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly long MinNackDelayNanos = 100;

		public NegativeAcksTracker(ActorSystem system, IActorRef consumer, ConsumerConfigurationData conf)
        {
            _system = system;
			this.consumer = consumer;
			this.timer = consumer.Client.timer();
			this.nackDelayNanos = Math.Max(TimeUnit.MICROSECONDS.toNanos(conf.NegativeAckRedeliveryDelayMicros), MinNackDelayNanos);
			this.timerIntervalNanos = nackDelayNanos / 3;
		}

		private void TriggerRedelivery(Timer t)
        {
            lock (this)
            {
                if (nackedMessages.Count == 0)
                {
                    this.timeout = null;
                    return;
                }

                // Group all the nacked messages into one single re-delivery request
                ISet<MessageId> messagesToRedeliver = new HashSet<MessageId>();
                long now = System.nanoTime();
                nackedMessages.forEach((msgId, timestamp) =>
                {
                    if (timestamp < now)
                    {
                        AddChunkedMessageIdsAndRemoveFromSequnceMap(msgId, messagesToRedeliver, this.consumer);
                        messagesToRedeliver.Add(msgId);
                    }
                });

                messagesToRedeliver.forEach(nackedMessages.remove);
                consumer.onNegativeAcksSend(messagesToRedeliver);
                consumer.redeliverUnacknowledgedMessages(messagesToRedeliver);

                this.timeout = timer.newTimeout(this.triggerRedelivery, timerIntervalNanos, TimeUnit.NANOSECONDS);
            }
        }

        public virtual void Add(MessageId messageId)
        {
            lock (this)
            {
                if (messageId is BatchMessageId)
                {
                    BatchMessageId batchMessageId = (BatchMessageId)messageId;
                    messageId = new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
                }

                if (nackedMessages == null)
                {
                    nackedMessages = new Dictionary<MessageId, long>();
                }
                nackedMessages[messageId] = System.nanoTime() + nackDelayNanos;

                if (this.timeout == null)
                {
                    // Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
                    // nack immediately following the current one will be batched into the same redeliver request.
                    this.timeout = timer.newTimeout(this.triggerRedelivery, timerIntervalNanos, TimeUnit.NANOSECONDS);
                }
            }
        }
	}

}
