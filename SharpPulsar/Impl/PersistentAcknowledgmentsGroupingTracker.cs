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
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using SharpPulsar.Impl.conf;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using AckType = org.apache.pulsar.common.api.proto.PulsarApi.CommandAck.AckType;

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
		private const int MAX_ACK_GROUP_SIZE = 1000;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ConsumerImpl<?> consumer;
		private readonly ConsumerImpl<object> consumer;

		private readonly long acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private volatile MessageIdImpl lastCumulativeAck = (MessageIdImpl) MessageId.earliest;
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

		public PersistentAcknowledgmentsGroupingTracker<T1, T2>(ConsumerImpl<T1> consumer, ConsumerConfigurationData<T2> conf, EventLoopGroup eventLoopGroup)
		{
			this.consumer = consumer;
			this.pendingIndividualAcks = new ConcurrentSkipListSet<MessageIdImpl>();
			this.acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;

			if (acknowledgementGroupTimeMicros > 0)
			{
				scheduledTask = eventLoopGroup.next().scheduleWithFixedDelay(this.flush, acknowledgementGroupTimeMicros, acknowledgementGroupTimeMicros, TimeUnit.MICROSECONDS);
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
		public virtual bool isDuplicate(MessageId messageId)
		{
			if (messageId.compareTo(lastCumulativeAck) <= 0)
			{
				// Already included in a cumulative ack
				return true;
			}
			else
			{
				return pendingIndividualAcks.contains(messageId);
			}
		}

		public virtual void addAcknowledgment(MessageIdImpl msgId, AckType ackType, IDictionary<string, long> properties)
		{
			if (acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
			{
				// We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
				// uncommon condition since it's only used for the compaction subscription.
				doImmediateAck(msgId, ackType, properties);
			}
			else if (ackType == AckType.Cumulative)
			{
				doCumulativeAck(msgId);
			}
			else
			{
				// Individual ack
				pendingIndividualAcks.add(msgId);
				if (pendingIndividualAcks.size() >= MAX_ACK_GROUP_SIZE)
				{
					flush();
				}
			}
		}

		private void doCumulativeAck(MessageIdImpl msgId)
		{
			// Handle concurrent updates from different threads
			while (true)
			{
				MessageIdImpl lastCumlativeAck = this.lastCumulativeAck;
				if (msgId.compareTo(lastCumlativeAck) > 0)
				{
					if (LAST_CUMULATIVE_ACK_UPDATER.compareAndSet(this, lastCumlativeAck, msgId))
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

		private bool doImmediateAck(MessageIdImpl msgId, AckType ackType, IDictionary<string, long> properties)
		{
			ClientCnx cnx = consumer.ClientCnx;

			if (cnx == null)
			{
				return false;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.buffer.ByteBuf cmd = org.apache.pulsar.common.protocol.Commands.newAck(consumer.consumerId, msgId.getLedgerId(), msgId.getEntryId(), ackType, null, properties);
			ByteBuf cmd = Commands.newAck(consumer.consumerId, msgId.LedgerId, msgId.EntryId, ackType, null, properties);

			cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
			return true;
		}

		/// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		public virtual void flush()
		{
			ClientCnx cnx = consumer.ClientCnx;

			if (cnx == null)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Cannot flush pending acks since we're not connected to broker", consumer);
				}
				return;
			}

			bool shouldFlush = false;
			if (cumulativeAckFlushRequired)
			{
				ByteBuf cmd = Commands.newAck(consumer.consumerId, lastCumulativeAck.ledgerId, lastCumulativeAck.entryId, AckType.Cumulative, null, Collections.emptyMap());
				cnx.ctx().write(cmd, cnx.ctx().voidPromise());
				shouldFlush = true;
				cumulativeAckFlushRequired = false;
			}

			// Flush all individual acks
			if (!pendingIndividualAcks.Empty)
			{
				if (Commands.peerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
				{
					// We can send 1 single protobuf command with all individual acks
					IList<Pair<long, long>> entriesToAck = new List<Pair<long, long>>(pendingIndividualAcks.size());
					while (true)
					{
						MessageIdImpl msgId = pendingIndividualAcks.pollFirst();
						if (msgId == null)
						{
							break;
						}

						entriesToAck.Add(Pair.of(msgId.LedgerId, msgId.EntryId));
					}

					cnx.ctx().write(Commands.newMultiMessageAck(consumer.consumerId, entriesToAck), cnx.ctx().voidPromise());
					shouldFlush = true;
				}
				else
				{
					// When talking to older brokers, send the acknowledgements individually
					while (true)
					{
						MessageIdImpl msgId = pendingIndividualAcks.pollFirst();
						if (msgId == null)
						{
							break;
						}

						cnx.ctx().write(Commands.newAck(consumer.consumerId, msgId.LedgerId, msgId.EntryId, AckType.Individual, null, Collections.emptyMap()), cnx.ctx().voidPromise());
						shouldFlush = true;
					}
				}
			}

			if (shouldFlush)
			{
				if (log.DebugEnabled)
				{
					log.debug("[{}] Flushing pending acks to broker: last-cumulative-ack: {} -- individual-acks: {}", consumer, lastCumulativeAck, pendingIndividualAcks);
				}
				cnx.ctx().flush();
			}
		}

		public virtual void flushAndClean()
		{
			flush();
			lastCumulativeAck = (MessageIdImpl) MessageId.earliest;
			pendingIndividualAcks.clear();
		}

		public virtual void close()
		{
			flush();
			if (scheduledTask != null && !scheduledTask.Cancelled)
			{
				scheduledTask.cancel(true);
			}
		}
	}

}