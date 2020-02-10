using System;
using System.Collections.Generic;
using DotNetty.Common.Utilities;
using SharpPulsar.Util;

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

    using IMessageId = Api.IMessageId;
	using SharpPulsar.Impl.Conf;

	public class NegativeAcksTracker<T>
	{

		private Dictionary<IMessageId, long> _nackedMessages = null;
		private readonly ConsumerBase<object> _consumer;
		private readonly HashedWheelTimer _timer;
		private readonly long _nackDelayNanos;
		private readonly long _timerIntervalNanos;

		private ITimeout _timeout;

		// Set a min delay to allow for grouping nacks within a single batch
		private static readonly long MinNackDelayNanos = BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS.ToNanos(100);

		public NegativeAcksTracker(ConsumerBase<T> consumer, ConsumerConfigurationData<T> conf)
		{
			this._consumer = consumer;
			this._timer = ((PulsarClientImpl) consumer.Client).Timer;
			this._nackDelayNanos = Math.Max(BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS.ToNanos(conf.NegativeAckRedeliveryDelayMicros), MinNackDelayNanos);
			this._timerIntervalNanos = _nackDelayNanos / 3;
		}

		private void TriggerRedelivery(ITimeout T)
		{
			lock (this)
			{
				if (_nackedMessages.Count == 0)
				{
					this._timeout = null;
					return;
				}
        
				// Group all the nacked messages into one single re-delivery request
				ISet<IMessageId> messagesToRedeliver = new HashSet<IMessageId>();
				long now = System.nanoTime();
				_nackedMessages.forEach((msgId, timestamp) =>
				{
				if (timestamp < now)
				{
					messagesToRedeliver.Add(msgId);
				}
				});
        
				messagesToRedeliver.forEach(_nackedMessages.remove);
				_consumer.OnNegativeAcksSend(messagesToRedeliver);
				_consumer.RedeliverUnacknowledgedMessages(messagesToRedeliver);
        
				this._timeout = _timer.NewTimeout(()=>TriggerRedelivery, TimeSpan.FromTicks(_timerIntervalNanos));
			}
		}

		public virtual void Add(IMessageId messageId)
		{
			lock (this)
			{
				if (messageId is BatchMessageIdImpl)
				{
					BatchMessageIdImpl batchMessageId = (BatchMessageIdImpl) messageId;
					messageId = new MessageIdImpl(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
				}
        
				if (_nackedMessages == null)
				{
					_nackedMessages = new Dictionary<IMessageId, long>();
				}
				_nackedMessages[messageId] = System.nanoTime() + _nackDelayNanos;
        
				if (this._timeout == null)
				{
					// Schedule a task and group all the redeliveries for same period. Leave a small buffer to allow for
					// nack immediately following the current one will be batched into the same redeliver request.
					this._timeout = _timer.newTimeout(this.triggerRedelivery, _timerIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS);
				}
			}
		}
	}

}