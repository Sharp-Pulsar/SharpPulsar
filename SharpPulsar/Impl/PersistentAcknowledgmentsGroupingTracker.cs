using SharpPulsar.Api;

using System.Collections.Generic;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
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
	public class PersistentAcknowledgmentsGroupingTracker<T>: AcknowledgmentsGroupingTracker
	{

		/// <summary>
		/// When reaching the max group size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;
		private readonly ConsumerImpl<T> _consumer;

		private readonly long _acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private volatile MessageIdImpl _lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
		private volatile bool _cumulativeAckFlushRequired = false;

		private static readonly AtomicReferenceFieldUpdater<PersistentAcknowledgmentsGroupingTracker, MessageIdImpl> LastCumulativeAckUpdater = AtomicReferenceFieldUpdater.newUpdater(typeof(PersistentAcknowledgmentsGroupingTracker), typeof(MessageIdImpl), "lastCumulativeAck");

		/// <summary>
		/// This is a set of all the individual acks that the application has issued and that were not already sent to
		/// broker.
		/// </summary>
		private readonly ConcurrentSkipListSet<MessageIdImpl> _pendingIndividualAcks;
		private readonly ScheduledFuture<object> _scheduledTask;

		public PersistentAcknowledgmentsGroupingTracker(ConsumerImpl<T> consumer, ConsumerConfigurationData<T> conf, IEventLoopGroup eventLoopGroup)
		{
			this._consumer = consumer;
			this._pendingIndividualAcks = new ConcurrentSkipListSet<MessageIdImpl>();
			this._acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;

			if (_acknowledgementGroupTimeMicros > 0)
			{
				_scheduledTask = eventLoopGroup.next().scheduleWithFixedDelay(this.flush, _acknowledgementGroupTimeMicros, _acknowledgementGroupTimeMicros, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS);
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
			if (messageId.CompareTo(_lastCumulativeAck) <= 0)
			{
				// Already included in a cumulative ack
				return true;
			}
			else
			{
				return _pendingIndividualAcks.contains(messageId);
			}
		}

		public virtual void AddAcknowledgment(MessageIdImpl msgId, CommandAck.Types.AckType ackType, IDictionary<string, long> properties)
		{
			if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
			{
				// We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
				// uncommon condition since it's only used for the compaction subscription.
				DoImmediateAck(msgId, ackType, properties);
			}
			else if (ackType == ackType.Cumulative)
			{
				DoCumulativeAck(msgId);
			}
			else
			{
				// Individual ack
				_pendingIndividualAcks.add(msgId);
				if (_pendingIndividualAcks.size() >= MaxAckGroupSize)
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
				MessageIdImpl lastCumlativeAck = this._lastCumulativeAck;
				if (msgId.CompareTo(lastCumlativeAck) > 0)
				{
					if (LastCumulativeAckUpdater.compareAndSet(this, lastCumlativeAck, msgId))
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
			ClientCnx cnx = _consumer.ClientCnx;

			if (cnx == null)
			{
				return false;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf cmd = org.apache.pulsar.common.protocol.Commands.newAck(consumer.consumerId, msgId.getLedgerId(), msgId.getEntryId(), ackType, null, properties);
			ByteBuf cmd = Commands.newAck(_consumer.ConsumerId, msgId.LedgerId, msgId.EntryId, ackType, null, properties);

			cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
			return true;
		}

		/// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		public virtual void Flush()
		{
			ClientCnx cnx = _consumer.ClientCnx;

			if (cnx == null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Cannot flush pending acks since we're not connected to broker", _consumer);
				}
				return;
			}

			bool shouldFlush = false;
			if (_cumulativeAckFlushRequired)
			{
				ByteBuf cmd = Commands.newAck(_consumer.ConsumerId, _lastCumulativeAck.LedgerIdConflict, _lastCumulativeAck.EntryIdConflict, CommandAck.Types.AckType.Cumulative, null, Collections.emptyMap());
				cnx.ctx().write(cmd, cnx.ctx().voidPromise());
				shouldFlush = true;
				_cumulativeAckFlushRequired = false;
			}

			// Flush all individual acks
			if (!_pendingIndividualAcks.Empty)
			{
				if (Commands.peerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
				{
					// We can send 1 single protobuf command with all individual acks
					IList<Pair<long, long>> entriesToAck = new List<Pair<long, long>>(_pendingIndividualAcks.size());
					while (true)
					{
						MessageIdImpl msgId = _pendingIndividualAcks.pollFirst();
						if (msgId == null)
						{
							break;
						}

						entriesToAck.Add(Pair.of(msgId.LedgerId, msgId.EntryId));
					}

					cnx.ctx().write(Commands.newMultiMessageAck(_consumer.ConsumerId, entriesToAck), cnx.ctx().voidPromise());
					shouldFlush = true;
				}
				else
				{
					// When talking to older brokers, send the acknowledgements individually
					while (true)
					{
						MessageIdImpl msgId = _pendingIndividualAcks.pollFirst();
						if (msgId == null)
						{
							break;
						}

						cnx.ctx().write(Commands.newAck(_consumer.ConsumerId, msgId.LedgerId, msgId.EntryId, CommandAck.Types.AckType.Individual, null, Collections.emptyMap()), cnx.ctx().voidPromise());
						shouldFlush = true;
					}
				}
			}

			if (shouldFlush)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Flushing pending acks to broker: last-cumulative-ack: {} -- individual-acks: {}", _consumer, _lastCumulativeAck, _pendingIndividualAcks);
				}
				cnx.ctx().flush();
			}
		}

		public override void FlushAndClean()
		{
			Flush();
			_lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
			_pendingIndividualAcks.clear();
		}

		public override void Close()
		{
			Flush();
			if (_scheduledTask != null && !_scheduledTask.Cancelled)
			{
				_scheduledTask.cancel(true);
			}
		}
	}

}