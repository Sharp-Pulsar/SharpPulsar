using SharpPulsar.Api;

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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;


	using Slf4j = lombok.@extern.slf4j.Slf4j;

	using Pair = org.apache.commons.lang3.tuple.Pair;
	using MessageId = IMessageId;
	using SharpPulsar.Impl.Conf;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using AckType = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandAck.AckType;

	/// <summary>
	/// Group the acknowledgements for a certain time and then sends them out in a single protobuf command.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class PersistentAcknowledgmentsGroupingTracker implements AcknowledgmentsGroupingTracker
	public class PersistentAcknowledgmentsGroupingTracker : AcknowledgmentsGroupingTracker
	{

		/// <summary>
		/// When reaching the max group size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ConsumerImpl<?> consumer;
		private readonly ConsumerImpl<object> consumer;

		private readonly long acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private volatile MessageIdImpl lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
		private volatile bool cumulativeAckFlushRequired = false;

		private static readonly AtomicReferenceFieldUpdater<PersistentAcknowledgmentsGroupingTracker, MessageIdImpl> LAST_CUMULATIVE_ACK_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(PersistentAcknowledgmentsGroupingTracker), typeof(MessageIdImpl), "lastCumulativeAck");

		/// <summary>
		/// This is a set of all the individual acks that the application has issued and that were not already sent to
		/// broker.
		/// </summary>
		private readonly ConcurrentSkipListSet<MessageIdImpl> pendingIndividualAcks;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.concurrent.ScheduledFuture<?> scheduledTask;
		private readonly ScheduledFuture<object> scheduledTask;

		public PersistentAcknowledgmentsGroupingTracker<T1, T2>(ConsumerImpl<T1> Consumer, ConsumerConfigurationData<T2> Conf, EventLoopGroup EventLoopGroup)
		{
			this.consumer = IConsumer;
			this.pendingIndividualAcks = new ConcurrentSkipListSet<MessageIdImpl>();
			this.acknowledgementGroupTimeMicros = Conf.AcknowledgementsGroupTimeMicros;

			if (acknowledgementGroupTimeMicros > 0)
			{
				scheduledTask = EventLoopGroup.next().scheduleWithFixedDelay(this.flush, acknowledgementGroupTimeMicros, acknowledgementGroupTimeMicros, BAMCIS.Util.Concurrent.TimeUnit.MICROSECONDS);
			}
			else
			{
				scheduledTask = null;
			}
		}

		/// <summary>
		/// Since the ack are delayed, we need to do some best-effort duplicate check to discard messages that are being
		/// resent after a disconnection and for which the user has already sent an acknowledgement.
		/// </summary>
		public virtual bool IsDuplicate(IMessageId MessageId)
		{
			if (MessageId.CompareTo(lastCumulativeAck) <= 0)
			{
				// Already included in a cumulative ack
				return true;
			}
			else
			{
				return pendingIndividualAcks.contains(MessageId);
			}
		}

		public virtual void AddAcknowledgment(MessageIdImpl MsgId, AckType AckType, IDictionary<string, long> Properties)
		{
			if (acknowledgementGroupTimeMicros == 0 || Properties.Count > 0)
			{
				// We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
				// uncommon condition since it's only used for the compaction subscription.
				DoImmediateAck(MsgId, AckType, Properties);
			}
			else if (AckType == AckType.Cumulative)
			{
				DoCumulativeAck(MsgId);
			}
			else
			{
				// Individual ack
				pendingIndividualAcks.add(MsgId);
				if (pendingIndividualAcks.size() >= MaxAckGroupSize)
				{
					Flush();
				}
			}
		}

		private void DoCumulativeAck(MessageIdImpl MsgId)
		{
			// Handle concurrent updates from different threads
			while (true)
			{
				MessageIdImpl LastCumlativeAck = this.lastCumulativeAck;
				if (MsgId.CompareTo(LastCumlativeAck) > 0)
				{
					if (LAST_CUMULATIVE_ACK_UPDATER.compareAndSet(this, LastCumlativeAck, MsgId))
					{
						// Successfully updated the last cumulative ack. Next flush iteration will send this to broker.
						cumulativeAckFlushRequired = true;
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

		private bool DoImmediateAck(MessageIdImpl MsgId, AckType AckType, IDictionary<string, long> Properties)
		{
			ClientCnx Cnx = consumer.ClientCnx;

			if (Cnx == null)
			{
				return false;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf cmd = org.apache.pulsar.common.protocol.Commands.newAck(consumer.consumerId, msgId.getLedgerId(), msgId.getEntryId(), ackType, null, properties);
			ByteBuf Cmd = Commands.newAck(consumer.ConsumerId, MsgId.LedgerId, MsgId.EntryId, AckType, null, Properties);

			Cnx.ctx().writeAndFlush(Cmd, Cnx.ctx().voidPromise());
			return true;
		}

		/// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		public virtual void Flush()
		{
			ClientCnx Cnx = consumer.ClientCnx;

			if (Cnx == null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Cannot flush pending acks since we're not connected to broker", consumer);
				}
				return;
			}

			bool ShouldFlush = false;
			if (cumulativeAckFlushRequired)
			{
				ByteBuf Cmd = Commands.newAck(consumer.ConsumerId, lastCumulativeAck.LedgerIdConflict, lastCumulativeAck.EntryIdConflict, AckType.Cumulative, null, Collections.emptyMap());
				Cnx.ctx().write(Cmd, Cnx.ctx().voidPromise());
				ShouldFlush = true;
				cumulativeAckFlushRequired = false;
			}

			// Flush all individual acks
			if (!pendingIndividualAcks.Empty)
			{
				if (Commands.peerSupportsMultiMessageAcknowledgment(Cnx.RemoteEndpointProtocolVersion))
				{
					// We can send 1 single protobuf command with all individual acks
					IList<Pair<long, long>> EntriesToAck = new List<Pair<long, long>>(pendingIndividualAcks.size());
					while (true)
					{
						MessageIdImpl MsgId = pendingIndividualAcks.pollFirst();
						if (MsgId == null)
						{
							break;
						}

						EntriesToAck.Add(Pair.of(MsgId.LedgerId, MsgId.EntryId));
					}

					Cnx.ctx().write(Commands.newMultiMessageAck(consumer.ConsumerId, EntriesToAck), Cnx.ctx().voidPromise());
					ShouldFlush = true;
				}
				else
				{
					// When talking to older brokers, send the acknowledgements individually
					while (true)
					{
						MessageIdImpl MsgId = pendingIndividualAcks.pollFirst();
						if (MsgId == null)
						{
							break;
						}

						Cnx.ctx().write(Commands.newAck(consumer.ConsumerId, MsgId.LedgerId, MsgId.EntryId, AckType.Individual, null, Collections.emptyMap()), Cnx.ctx().voidPromise());
						ShouldFlush = true;
					}
				}
			}

			if (ShouldFlush)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Flushing pending acks to broker: last-cumulative-ack: {} -- individual-acks: {}", consumer, lastCumulativeAck, pendingIndividualAcks);
				}
				Cnx.ctx().flush();
			}
		}

		public override void FlushAndClean()
		{
			Flush();
			lastCumulativeAck = (MessageIdImpl) MessageIdFields.Earliest;
			pendingIndividualAcks.clear();
		}

		public override void Close()
		{
			Flush();
			if (scheduledTask != null && !scheduledTask.Cancelled)
			{
				scheduledTask.cancel(true);
			}
		}
	}

}