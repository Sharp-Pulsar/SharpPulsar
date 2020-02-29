using System;
using System.Collections.Concurrent;
using SharpPulsar.Api;

using System.Collections.Generic;
using System.Linq;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

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
	/// <summary>
	/// Group the acknowledgements for a certain time and then sends them out in a single protobuf command.
	/// </summary>
	public class PersistentAcknowledgmentsGroupingTracker: IAcknowledgmentsGroupingTracker
	{

		/// <summary>
		/// When reaching the max group size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;
		private readonly ConsumerImpl _consumer;

		private readonly long _acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private volatile MessageIdImpl _lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
		private volatile bool _cumulativeAckFlushRequired = false;

		private static readonly ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, MessageIdImpl> LastCumulativeAckUpdater = new ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, MessageIdImpl>();

		/// <summary>
		/// This is a set of all the individual acks that the application has issued and that were not already sent to
		/// broker.
		/// </summary>
		private readonly ConcurrentBag<MessageIdImpl> _pendingIndividualAcks;

		private readonly IScheduledTask _scheduledTask;

		public PersistentAcknowledgmentsGroupingTracker(ConsumerImpl consumer, ConsumerConfigurationData conf, IEventLoopGroup eventLoopGroup)
		{
			this._consumer = consumer;
			this._pendingIndividualAcks = new ConcurrentBag<MessageIdImpl>();
			this._acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;

			if (_acknowledgementGroupTimeMicros > 0)
			{
				_scheduledTask = ((MultithreadEventLoopGroup)eventLoopGroup).GetNext().Schedule(Flush, TimeSpan.FromMilliseconds(_acknowledgementGroupTimeMicros));
			}
			else
			{
				_scheduledTask = null;
			}
		}

		/// <summary>
		/// Since the ack are delayed, we need to do some best-effort duplicate check to discard messages that are being
		/// resent after a disconnection and for which the user has already sent an acknowledgement.
		/// </summary>
		public virtual bool IsDuplicate(IMessageId messageId)
        {
            return messageId.CompareTo(_lastCumulativeAck) <= 0 || _pendingIndividualAcks.Contains(messageId);
        }

		public virtual void AddAcknowledgment(MessageIdImpl msgId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties)
		{
			if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
			{
				// We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
				// uncommon condition since it's only used for the compaction subscription.
				DoImmediateAck(msgId, ackType, properties);
			}
			else if (ackType == CommandAck.Types.AckType.Cumulative)
			{
				DoCumulativeAck(msgId);
			}
			else
			{
				// Individual ack
				_pendingIndividualAcks.Add(msgId);
				if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
				{
					Flush();
				}
			}
		}

		private void DoCumulativeAck(MessageIdImpl msgId)
		{
			// Handle concurrent updates from different threads
			while (true)
			{
				var lastCumlativeAck = this._lastCumulativeAck;
				if (msgId.CompareTo(lastCumlativeAck) > 0)
				{
					if (LastCumulativeAckUpdater.TryUpdate(this, msgId,lastCumlativeAck))
					{
						// Successfully updated the last cumulative ack. Next flush iteration will send this to broker.
						_cumulativeAckFlushRequired = true;
						return;
					}
				}
				else
				{
					// message id acknowledging an before the current last cumulative ack
					return;
				}
			}
		}

		private bool DoImmediateAck(MessageIdImpl msgId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties)
		{
			var cnx = _consumer.ClientCnx;

			if (cnx == null)
			{
				return false;
			}
            var cmd = Commands.NewAck(_consumer.ConsumerId, msgId.LedgerId, msgId.EntryId, ackType, null, properties);

			cnx.Ctx().WriteAndFlushAsync(cmd);
			return true;
		}

		/// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		public virtual void Flush()
		{
			var cnx = _consumer.ClientCnx;

			if (cnx == null)
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Cannot flush pending acks since we're not connected to broker", _consumer);
				}
				return;
			}

			var shouldFlush = false;
			if (_cumulativeAckFlushRequired)
			{
				var cmd = Commands.NewAck(_consumer.ConsumerId, _lastCumulativeAck.LedgerId, _lastCumulativeAck.EntryId, CommandAck.Types.AckType.Cumulative, null, new Dictionary<string, long>());
				cnx.Ctx().WriteAsync(cmd);
				shouldFlush = true;
				_cumulativeAckFlushRequired = false;
			}

			// Flush all individual acks
			if (!_pendingIndividualAcks.IsEmpty)
			{
				if (Commands.PeerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
				{
					// We can send 1 single protobuf command with all individual acks
					IList<KeyValuePair<long, long>> entriesToAck = new List<KeyValuePair<long, long>>(_pendingIndividualAcks.Count);
					while (true)
					{
						if (!_pendingIndividualAcks.TryTake(out var msgId))
						{
							break;
						}

						entriesToAck.Add(new KeyValuePair<long, long>(msgId.LedgerId, msgId.EntryId));
					}

					cnx.Ctx().WriteAsync(Commands.NewMultiMessageAck(_consumer.ConsumerId, entriesToAck));
					shouldFlush = true;
				}
				else
				{
					// When talking to older brokers, send the acknowledgements individually
					while (true)
					{
						if (!_pendingIndividualAcks.TryTake(out var msgId))
						{
							break;
						}

						cnx.Ctx().WriteAsync(Commands.NewAck(_consumer.ConsumerId, msgId.LedgerId, msgId.EntryId, CommandAck.Types.AckType.Individual, null, new Dictionary<string, long>()));
						shouldFlush = true;
					}
				}
			}

			if (shouldFlush)
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("[{}] Flushing pending acks to broker: last-cumulative-ack: {} -- individual-acks: {}", _consumer, _lastCumulativeAck, _pendingIndividualAcks);
				}
				cnx.Ctx().Flush();
			}
		}

		public void FlushAndClean()
		{
			Flush();
			_lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
			_pendingIndividualAcks.Clear();
		}

		public void Close()
		{
			Flush();
			if (_scheduledTask != null && !_scheduledTask.Completion.IsCanceled)
            {
                _scheduledTask.Cancel();
            }
		}
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<PersistentAcknowledgmentsGroupingTracker>();
        public void Dispose()
        {
           Close();
        }
    }

}