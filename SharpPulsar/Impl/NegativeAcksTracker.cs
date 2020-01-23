using System;
using System.Collections.Generic;

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
namespace SharpPulsar.Impl
{
	using Timeout = io.netty.util.Timeout;
	using Timer = io.netty.util.Timer;


	using MessageId = SharpPulsar.Api.MessageId;
	using SharpPulsar.Impl.Conf;

	public class NegativeAcksTracker
	{

		private Dictionary<MessageId, long> nackedMessages = null;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ConsumerBase<?> consumer;
		private readonly ConsumerBase<object> consumer;
		private readonly Timer timer;
		private readonly long nackDelayNanos;
		private readonly long timerIntervalNanos;

		private Timeout timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly long MIN_NACK_DELAY_NANOS = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.toNanos(100);

		public NegativeAcksTracker<T1, T2>(ConsumerBase<T1> Consumer, ConsumerConfigurationData<T2> Conf)
		{
			this.consumer = Consumer;
			this.timer = ((PulsarClientImpl) Consumer.Client).timer();
			this.nackDelayNanos = Math.Max(BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.toNanos(Conf.NegativeAckRedeliveryDelayMicros), MIN_NACK_DELAY_NANOS);
			this.timerIntervalNanos = nackDelayNanos / 3;
		}

		private void TriggerRedelivery(Timeout T)
		{
			lock (this)
			{
				if (nackedMessages.Count == 0)
				{
					this.timeout = null;
					return;
				}
        
				// Group all the nacked messages into one single re-delivery request
				ISet<MessageId> MessagesToRedeliver = new HashSet<MessageId>();
				long Now = System.nanoTime();
				nackedMessages.forEach((msgId, timestamp) =>
				{
				if (timestamp < Now)
				{
					MessagesToRedeliver.Add(msgId);
				}
				});
        
				MessagesToRedeliver.forEach(nackedMessages.remove);
				consumer.OnNegativeAcksSend(MessagesToRedeliver);
				consumer.RedeliverUnacknowledgedMessages(MessagesToRedeliver);
        
				this.timeout = timer.newTimeout(this.triggerRedelivery, timerIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS);
			}
		}

		public virtual void Add(MessageId MessageId)
		{
			lock (this)
			{
				if (MessageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl BatchMessageId = (BatchMessageIdImpl) MessageId;
					MessageId = new MessageIdImpl(BatchMessageId.LedgerId, BatchMessageId.EntryId, BatchMessageId.PartitionIndex);
				}
        
				if (nackedMessages == null)
				{
					nackedMessages = new Dictionary<MessageId, long>();
				}
				nackedMessages[MessageId] = System.nanoTime() + nackDelayNanos;
        
				if (this.timeout == null)
				{
					// Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
					// nack immediately following the current one will be batched into the same redeliver request.
					this.timeout = timer.newTimeout(this.triggerRedelivery, timerIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS);
				}
			}
		}
	}

}