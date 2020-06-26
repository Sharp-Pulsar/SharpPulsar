using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Util;
using PulsarAdmin.Models;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker.Api;

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
namespace SharpPulsar.Tracker
{
    /// <summary>
	/// Group the acknowledgements for a certain time and then sends them out in a single protobuf command.
	/// </summary>
	public class PersistentAcknowledgmentsGroupingTracker : AcknowledgmentsGroupingTracker
	{

		/// <summary>
		/// When reaching the max group size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;
		private readonly IActorRef consumer;

		private readonly long acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private volatile MessageIdImpl lastCumulativeAck = (MessageIdImpl)MessageIdFields.Earliest;
		private volatile BitSetRecyclable lastCumulativeAckSet = null;
		private volatile bool cumulativeAckFlushRequired = false;

		private static readonly ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, MessageId> LastCumulativeAckUpdater = new ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, MessageId>();
		private static readonly ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, BitSetRecyclable> LastCumulativeAckSetUpdater = new ConcurrentDictionary<PersistentAcknowledgmentsGroupingTracker, BitSetRecyclable>();


		/// <summary>
		/// This is a set of all the individual acks that the application has issued and that were not already sent to
		/// broker.
		/// </summary>
		private readonly ConcurrentSet<MessageId> pendingIndividualAcks;
		private readonly ConcurrentDictionary<MessageIdImpl, ConcurrentBitSetRecyclable> pendingIndividualBatchIndexAcks;

		private readonly ScheduledFuture<object> scheduledTask;

		public PersistentAcknowledgmentsGroupingTracker(IActorRef consumer, ConsumerConfigurationData conf, EventLoopGroup eventLoopGroup)
		{
			this.consumer = consumer;
			this.pendingIndividualAcks = new ConcurrentSet<MessageId>();
			this.pendingIndividualBatchIndexAcks = new ConcurrentDictionary<MessageIdImpl, ConcurrentBitSetRecyclable>();
			this.acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;

			if (acknowledgementGroupTimeMicros > 0)
			{
				scheduledTask = eventLoopGroup.next().scheduleWithFixedDelay(Flush, acknowledgementGroupTimeMicros, acknowledgementGroupTimeMicros, TimeUnit.MICROSECONDS);
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
		public virtual bool IsDuplicate(MessageId messageId)
        {
	        if (messageId.CompareTo(lastCumulativeAck) <= 0)
	        {
		        // Already included in a cumulative ack
		        return true;
	        }
	        else
	        {
		        return pendingIndividualAcks.Contains(messageId);
	        }
        }

        public virtual void AddAcknowledgment(MessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        if (acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
	        {
		        // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
		        // uncommon condition since it's only used for the compaction subscription.
		        DoImmediateAck(msgId, ackType, properties);
	        }
	        else if (ackType == CommandAck.AckType.Cumulative)
	        {
		        DoCumulativeAck(msgId, null);
	        }
	        else
	        {
		        // Individual ack
		        if (msgId is BatchMessageId)
		        {
			        pendingIndividualAcks.TryAdd(new MessageId(msgId.LedgerId, msgId.EntryId, msgId.PartitionIndex, null));
		        }
		        else
		        {
			        pendingIndividualAcks.TryAdd(msgId);
		        }
		        pendingIndividualBatchIndexAcks.Remove(msgId);
		        if (pendingIndividualAcks.Count >= MaxAckGroupSize)
		        {
			        Flush();
		        }
	        }
        }

        public virtual void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        if (acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
	        {
		        doImmediateBatchIndexAck(msgId, batchIndex, batchSize, ackType, properties);
	        }
	        else if (ackType == CommandAck.AckType.Cumulative)
	        {
		        BitSetRecyclable bitSet = BitSetRecyclable.create();
		        bitSet.set(0, batchSize);
		        bitSet.clear(0, batchIndex + 1);
		        doCumulativeAck(msgId, bitSet);
	        }
	        else if (ackType == CommandAck.AckType.Individual)
	        {
		        ConcurrentBitSetRecyclable bitSet = pendingIndividualBatchIndexAcks.computeIfAbsent(new MessageIdImpl(msgId.LedgerId, msgId.EntryId, msgId.PartitionIndex), (v) =>
		        {
			        ConcurrentBitSetRecyclable value = ConcurrentBitSetRecyclable.create();
			        value.set(0, batchSize + 1);
			        value.clear(batchIndex);
			        return value;
		        });
		        bitSet.Set(batchIndex, false);
		        if (pendingIndividualBatchIndexAcks.Count >= MaxAckGroupSize)
		        {
			        Flush();
		        }
	        }
        }

        private void DoCumulativeAck(MessageId msgId, BitSetRecyclable bitSet)
        {
	        // Handle concurrent updates from different threads
	        while (true)
	        {
		        MessageIdImpl lastCumlativeAck = this.lastCumulativeAck;
		        BitSetRecyclable lastBitSet = this.lastCumulativeAckSet;
		        if (msgId.CompareTo(lastCumlativeAck) > 0)
		        {
			        if (LastCumulativeAckUpdater.compareAndSet(this, lastCumlativeAck, msgId) && LastCumulativeAckSetUpdater.compareAndSet(this, lastBitSet, bitSet))
			        {
				        if (lastBitSet != null)
				        {
					        try
					        {
						        lastBitSet.recycle();
					        }
					        catch (Exception)
					        {
						        // no-op
					        }
				        }
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

        private bool DoImmediateAck(MessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        ClientCnx cnx = consumer.ClientCnx;

	        if (cnx == null)
	        {
		        return false;
	        }

	        NewAckCommand(consumer.consumerId, msgId, null, ackType, null, properties, cnx, true);
	        return true;
        }

        private bool DoImmediateBatchIndexAck(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        ClientCnx cnx = consumer.ClientCnx;

	        if (cnx == null)
	        {
		        return false;
	        }
	        BitSetRecyclable bitSet = BitSetRecyclable.create();
	        bitSet.set(0, batchSize);
	        if (ackType == CommandAck.AckType.Cumulative)
	        {
		        bitSet.clear(0, batchIndex + 1);
	        }
	        else
	        {
		        bitSet.clear(batchIndex);
	        }

            var cmd = Commands.NewAck(consumer.consumerId, msgId.ledgerId, msgId.entryId, bitSet, ackType, null, properties);
	        cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
	        return true;
        }

        /// <summary>
        /// Flush all the pending acks and send them to the broker
        /// </summary>
        public virtual void Flush()
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
		        NewAckCommand(consumer.consumerId, lastCumulativeAck, lastCumulativeAckSet, CommandAck.AckType.Cumulative, null, Collections.emptyMap(), cnx, false);
		        shouldFlush = true;
		        cumulativeAckFlushRequired = false;
	        }

	        // Flush all individual acks
	        IList<Triple<long, long, ConcurrentBitSetRecyclable>> entriesToAck = new List<Triple<long, long, ConcurrentBitSetRecyclable>>(pendingIndividualAcks.size() + pendingIndividualBatchIndexAcks.Count);
	        if (!pendingIndividualAcks.Empty)
	        {
		        if (Commands.peerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
		        {
			        // We can send 1 single protobuf command with all individual acks
			        while (true)
			        {
				        MessageIdImpl msgId = pendingIndividualAcks.pollFirst();
				        if (msgId == null)
				        {
					        break;
				        }

				        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
				        // them
				        MessageIdImpl[] chunkMsgIds = this.consumer.unAckedChunckedMessageIdSequenceMap.get(msgId);
				        if (chunkMsgIds != null && chunkMsgIds.Length > 1)
				        {
					        foreach (MessageIdImpl cMsgId in chunkMsgIds)
					        {
						        if (cMsgId != null)
						        {
							        entriesToAck.Add(Triple.of(cMsgId.LedgerId, cMsgId.EntryId, null));
						        }
					        }
					        // messages will be acked so, remove checked message sequence
					        this.consumer.unAckedChunckedMessageIdSequenceMap.remove(msgId);
				        }
				        else
				        {
					        entriesToAck.Add(Triple.of(msgId.LedgerId, msgId.EntryId, null));
				        }
			        }
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

				        newAckCommand(consumer.consumerId, msgId, null, CommandAck.AckType.Individual, null, Collections.emptyMap(), cnx, false);
				        shouldFlush = true;
			        }
		        }
	        }

	        if (!pendingIndividualBatchIndexAcks.IsEmpty)
	        {
		        IEnumerator<KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable>> iterator = pendingIndividualBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();

		        while (iterator.MoveNext())
		        {
			        KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable> entry = iterator.Current;
			        entriesToAck.Add(Triple.of(entry.Key.ledgerId, entry.Key.entryId, entry.Value));
			        //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
			        iterator.remove();
		        }
	        }

	        if (entriesToAck.Count > 0)
	        {
		        cnx.ctx().write(Commands.newMultiMessageAck(consumer.consumerId, entriesToAck), cnx.ctx().voidPromise());
		        shouldFlush = true;
	        }

	        if (shouldFlush)
	        {
		        if (log.DebugEnabled)
		        {
			        log.debug("[{}] Flushing pending acks to broker: last-cumulative-ack: {} -- individual-acks: {} -- individual-batch-index-acks: {}", consumer, lastCumulativeAck, pendingIndividualAcks, pendingIndividualBatchIndexAcks);
		        }
		        cnx.ctx().flush();
	        }
        }

        public virtual void FlushAndClean()
        {
	        Flush();
	        lastCumulativeAck = (MessageId)MessageIdFields.Earliest;
	        pendingIndividualAcks.Clear();
        }

        public virtual void Close()
        {
	        Flush();
	        if (scheduledTask != null && !scheduledTask.Cancelled)
	        {
		        scheduledTask.cancel(true);
	        }
        }

        private void NewAckCommand(long consumerId, MessageIdImpl msgId, BitSetRecyclable lastCumulativeAckSet, CommandAck.AckType ackType, CommandAck.ValidationError validationError, IDictionary<string, long> map, ClientCnx cnx, bool flush)
        {

	        MessageIdImpl[] chunkMsgIds = this.consumer.unAckedChunckedMessageIdSequenceMap.get(msgId);
	        if (chunkMsgIds != null)
	        {
		        if (Commands.PeerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion) && ackType != CommandAck.AckType.Cumulative)
		        {
			        IList<Triple<long, long, ConcurrentBitSetRecyclable>> entriesToAck = new List<Triple<long, long, ConcurrentBitSetRecyclable>>(chunkMsgIds.Length);
			        foreach (MessageIdImpl cMsgId in chunkMsgIds)
			        {
				        if (cMsgId != null && chunkMsgIds.Length > 1)
				        {
					        entriesToAck.Add(Triple.of(cMsgId.LedgerId, cMsgId.EntryId, null));
				        }
			        }
			        var cmd = Commands.NewMultiMessageAck(consumer.consumerId, entriesToAck);
			        if (flush)
			        {
				        cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
			        }
			        else
			        {
				        cnx.ctx().write(cmd, cnx.ctx().voidPromise());
			        }
		        }
		        else
		        {
			        foreach (MessageIdImpl cMsgId in chunkMsgIds)
			        {
				        var cmd = Commands.NewAck(consumerId, cMsgId.LedgerId, cMsgId.EntryId, lastCumulativeAckSet, ackType, validationError, map);
				        if (flush)
				        {
					        cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
				        }
				        else
				        {
					        cnx.ctx().write(cmd, cnx.ctx().voidPromise());
				        }
			        }
		        }
		        this.consumer.unAckedChunckedMessageIdSequenceMap.remove(msgId);
	        }
	        else
	        {
		        var cmd = Commands.NewAck(consumerId, msgId.LedgerId, msgId.EntryId, lastCumulativeAckSet, ackType, validationError, map);
		        if (flush)
		        {
			        cnx.ctx().writeAndFlush(cmd, cnx.ctx().voidPromise());
		        }
		        else
		        {
			        cnx.ctx().write(cmd, cnx.ctx().voidPromise());
		        }
	        }
        }
	}
}