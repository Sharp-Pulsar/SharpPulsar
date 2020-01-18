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


	using MessageId = org.apache.pulsar.client.api.MessageId;
	using SharpPulsar.Impl.conf;

	internal class NegativeAcksTracker
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
		private static readonly long MIN_NACK_DELAY_NANOS = TimeUnit.MILLISECONDS.toNanos(100);

		public NegativeAcksTracker<T1, T2>(ConsumerBase<T1> consumer, ConsumerConfigurationData<T2> conf)
		{
			this.consumer = consumer;
			this.timer = ((PulsarClientImpl) consumer.Client).timer();
			this.nackDelayNanos = Math.Max(TimeUnit.MICROSECONDS.toNanos(conf.NegativeAckRedeliveryDelayMicros), MIN_NACK_DELAY_NANOS);
			this.timerIntervalNanos = nackDelayNanos / 3;
		}

		private void triggerRedelivery(Timeout t)
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
					messagesToRedeliver.Add(msgId);
				}
				});
        
				messagesToRedeliver.forEach(nackedMessages.remove);
				consumer.onNegativeAcksSend(messagesToRedeliver);
				consumer.redeliverUnacknowledgedMessages(messagesToRedeliver);
        
				this.timeout = timer.newTimeout(this.triggerRedelivery, timerIntervalNanos, TimeUnit.NANOSECONDS);
			}
		}

		public virtual void add(MessageId messageId)
		{
			lock (this)
			{
				if (messageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;
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