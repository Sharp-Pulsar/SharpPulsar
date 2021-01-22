using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Nito.AsyncEx;
using SharpPulsar.Akka;
using SharpPulsar.Messages;
using SharpPulsar.Batch;
using SharpPulsar.Extension;
using SharpPulsar.Configuration;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker.Messages;
using HashMapHelper = SharpPulsar.Presto.HashMapHelper;
using SharpPulsar.Interfaces;
using static SharpPulsar.Protocol.Proto.CommandAck;
using Akka.Util.Internal;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Messages.Requests;

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
	public class PersistentAcknowledgmentsGroupingTracker<T> : ReceiveActor
	{

		/// <summary>
		/// When reaching the max group Size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;
		private readonly IActorRef _clientCnx;
        private readonly long _consumerId;

		private readonly long _acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private IMessageId _lastCumulativeAck = IMessageId.Earliest;
		private  BitSet _lastCumulativeAckSet;
        private bool _cumulativeAckFlushRequired;


        /// <summary>
        /// This is a set of all the individual acks that the application has issued and that were not already sent to
        /// broker.
        /// </summary>
        private readonly Queue<IMessageId> _pendingIndividualAcks;
		private readonly ConcurrentDictionary<IMessageId, BitSet> _pendingIndividualBatchIndexAcks;
        private readonly ISet<(long LedgerId, long EntryId, MessageId MessageId)> _pendingIndividualTransactionAcks;

        private readonly ConcurrentDictionary<IActorRef, Dictionary<MessageId, BitSet>> _pendingIndividualTransactionBatchIndexAcks;

        private  ICancelable _scheduledTask;

        public PersistentAcknowledgmentsGroupingTracker(IActorRef clientCnx, long consumerid, ConsumerConfigurationData<T> conf)
        {
            _clientCnx = clientCnx;
            _consumerId = consumerid;
			_pendingIndividualAcks = new Queue<IMessageId>();
            _acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;
            _pendingIndividualBatchIndexAcks = new ConcurrentDictionary<IMessageId, BitSet>();
            _pendingIndividualTransactionBatchIndexAcks = new ConcurrentDictionary<IActorRef, Dictionary<MessageId, BitSet>>();
            _pendingIndividualTransactionAcks = new HashSet<(long LedgerId, long EntryId, MessageId MessageId)>();
            BecomeActive();
			_scheduledTask = _acknowledgementGroupTimeMicros > 0 ? Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_acknowledgementGroupTimeMicros), Self, FlushPending.Instance, ActorRefs.NoSender) : null;
		}

        private void BecomeActive()
        {
            Receive<IsDuplicate>(d =>
            {
                var isd = IsDuplicate(d.MessageId);
                Sender.Tell(isd);
            });
            Receive<AddAcknowledgment>(d => { AddAcknowledgment(d.MessageId, d.AckType, d.Properties); });
            Receive<AddBatchIndexAcknowledgment>(d => 
            { 
                AddBatchIndexAcknowledgment(d.MessageId, d.BatchIndex, d.BatchSize, d.AckType, d.Properties, d.Txn); 
            
            });
            Receive<FlushAndClean>(_ => FlushAndClean());
            Receive<FlushPending>(_ => Flush());
            Receive<AddListAcknowledgment>(a => {
                AddListAcknowledgment(a.MessageIds, a.AckType, a.Properties);
            });
        }
        public static Props Prop(IActorRef broker, long consumerid, ConsumerConfigurationData<T> conf)
        {
			return Props.Create(()=> new PersistentAcknowledgmentsGroupingTracker<T>(broker, consumerid, conf));
        }
		/// <summary>
		/// Since the ack are delayed, we need to do some best-effort duplicate check to discard messages that are being
		/// resent after a disconnection and for which the user has already sent an acknowledgement.
		/// </summary>
		private bool IsDuplicate(IMessageId messageId)
        {
            if (messageId.CompareTo(_lastCumulativeAck) <= 0)
	        {
		        // Already included in a cumulative ack
		        return true;
	        }

            return _pendingIndividualAcks.Contains(messageId);
        }
        private void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
            {
                var bits = txn.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                DoImmediateBatchIndexAck(msgId, batchIndex, batchSize, ackType, properties, txn == null ? -1 : bits.MostBits, txn == null ? -1 : bits.LeastBits);
            }
            else if (ackType == AckType.Cumulative)
            {
                BitSet bitSet = BitSet.Create();
                bitSet.Set(0, batchSize);
                bitSet.Clear(0, batchIndex + 1);
                DoCumulativeAck(msgId, bitSet);
            }
            else if (ackType == AckType.Individual)
            {
                BitSet bitSet;
                if (txn != null)
                {
                    Dictionary<MessageId, BitSet> transactionIndividualBatchIndexAcks;
                    if (_pendingIndividualTransactionBatchIndexAcks.ContainsKey(txn))
                        transactionIndividualBatchIndexAcks = _pendingIndividualTransactionBatchIndexAcks[txn];
                    else
                    {
                        _pendingIndividualTransactionBatchIndexAcks[txn] = new Dictionary<MessageId, BitSet>();
                        transactionIndividualBatchIndexAcks = _pendingIndividualTransactionBatchIndexAcks[txn];
                    }
                    if (transactionIndividualBatchIndexAcks.ContainsKey(msgId))
                        bitSet = transactionIndividualBatchIndexAcks[msgId];
                    else
                    {
                        BitSet value = BitSet.Create();
                        value.Set(0, msgId.Acker.BatchSize);
                        bitSet = value;
                        _pendingIndividualTransactionBatchIndexAcks[txn][msgId] = bitSet;
                    }
                    bitSet.Set(batchIndex, false);
                }
                else
                {
                    var msgid = new MessageId(msgId.LedgerId, msgId.EntryId, msgId.PartitionIndex);
                    BitSet value;
                    if (msgId.Acker != null && !(msgId.Acker is BatchMessageAckerDisabled))
                    {
                        value = BitSet.Create();
                        value.Set(0, msgId.Acker.BitSet.Size);
                    }
                    else
                    {
                        value = BitSet.Create();
                        value.Set(0, batchSize);
                    }
                    bitSet = _pendingIndividualBatchIndexAcks.AddOrUpdate(msgid, value, (s, v) => value);
                    
                    bitSet.Set(batchIndex, false);
                }
                if (_pendingIndividualBatchIndexAcks.Count >= MaxAckGroupSize)
                {
                    Flush();
                }
            }
        }
        private void AddListAcknowledgment(IList<MessageId> messageIds, AckType ackType, IDictionary<string, long> properties)
        {
            if (ackType == AckType.Cumulative)
            {
                messageIds.ForEach(messageId => DoCumulativeAck(messageId, null));
                return;
            }
            messageIds.ForEach(messageId =>
            {
                if (messageId is BatchMessageId batchMessageId)
                {
                    _pendingIndividualAcks.Enqueue(new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex));
                }
                else
                {
                    _pendingIndividualAcks.Enqueue(messageId);
                }
                _pendingIndividualBatchIndexAcks.TryRemove(messageId, out var bts);
                if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
                {
                    Flush();
                }
            });
            if (_acknowledgementGroupTimeMicros == 0)
            {
                Flush();
            }
        }
        private void AddAcknowledgment(IMessageId msgId, AckType ackType, IDictionary<string, long> properties)
        {
	        if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
	        {
		        // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
		        // uncommon condition since it's only used for the compaction subscription.
		        DoImmediateAck(msgId, ackType, properties);
	        }
	        else if (ackType == AckType.Cumulative)
	        {
		        DoCumulativeAck(msgId, null);
	        }
	        else
	        {
		        // Individual ack
		        if (msgId is BatchMessageId batch)
		        {
			        _pendingIndividualAcks.Enqueue(new MessageId(batch.LedgerId, batch.EntryId, batch.PartitionIndex));
		        }
		        else
		        {
			        _pendingIndividualAcks.Enqueue(msgId);
		        }
		        _pendingIndividualBatchIndexAcks.Remove(msgId, out var bitset);
		        if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
		        {
			        Flush();
		        }
	        }
        }

        private void DoCumulativeAck(IMessageId msgId, BitSet bitSet)
        {
	        // Handle concurrent updates from different threads
	        while (true)
	        {
		        var lastCumlativeAck = _lastCumulativeAck;
		        var lastBitSet = _lastCumulativeAckSet;
		        if (msgId.CompareTo(lastCumlativeAck) > 0)
                {
                    var updatedMsgId = Interlocked.CompareExchange(ref _lastCumulativeAck, msgId, lastCumlativeAck);
                    var updatedBitSet = Interlocked.CompareExchange(ref _lastCumulativeAckSet, bitSet, lastBitSet);

					if ((updatedMsgId == lastCumlativeAck) && (updatedBitSet == lastBitSet))
                    {
                        if (lastBitSet != null)
                        {
                            try
                            {
                                lastBitSet = null;
                            }
                            catch (Exception)
                            {
                                // no-op
                            }
                        }
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

        private bool DoImmediateAck(IMessageId msgId, AckType ackType, IDictionary<string, long> properties)
        {
	        NewAckCommand(_consumerId, msgId, null, ackType, null, properties, true);
	        return true;
        }
        private bool DoImmediateBatchIndexAck(BatchMessageId msgId, int batchIndex, int batchSize, AckType ackType, IDictionary<string, long> properties, long txnidMostBits, long txnidLeastBits)
        {
            BitSet bitSet;
            if (msgId.Acker != null && !(msgId.Acker is BatchMessageAckerDisabled))
            {
                bitSet = BitSet.ValueOf(msgId.Acker.BitSet.ToLongArray());
            }
            else
            {
                bitSet = BitSet.Create();
                bitSet.Set(0, batchSize);
            }
            if (ackType == AckType.Cumulative)
            {
                bitSet.Clear(0, batchIndex + 1);
            }
            else
            {
                bitSet.Clear(batchIndex);
            }
            var cmd = Commands.NewAck(_consumerId, msgId.LedgerId, msgId.EntryId, bitSet.ToLongArray(), ackType, null, properties, txnidLeastBits, txnidMostBits, -1);
            var payload = new Payload(cmd, -1, "NewAck");
            _clientCnx.Tell(payload);
            return true;
        }
        /// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		public virtual void Flush()
        {
            if (_cumulativeAckFlushRequired)
            {
                NewAckCommand(_consumerId, _lastCumulativeAck, _lastCumulativeAckSet, AckType.Cumulative, null, new Dictionary<string, long>(), _clientCnx, false, -1, -1);
                _cumulativeAckFlushRequired = false;
            }

            // Flush all individual acks
            IList<Triple<long, long, ConcurrentBitSetRecyclable>> entriesToAck = new List<Triple<long, long, ConcurrentBitSetRecyclable>>(_pendingIndividualAcks.size() + _pendingIndividualBatchIndexAcks.Count);
            Dictionary<TransactionImpl, IList<Triple<long, long, ConcurrentBitSetRecyclable>>> transactionEntriesToAck = new Dictionary<TransactionImpl, IList<Triple<long, long, ConcurrentBitSetRecyclable>>>();
            if (!_pendingIndividualAcks.Empty)
            {
                if (Commands.PeerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
                {
                    // We can send 1 single protobuf command with all individual acks
                    while (true)
                    {
                        MessageIdImpl msgId = _pendingIndividualAcks.pollFirst();
                        if (msgId == null)
                        {
                            break;
                        }

                        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                        // them
                        MessageIdImpl[] chunkMsgIds = this._consumer.UnAckedChunckedMessageIdSequenceMap.Get(msgId);
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
                            this._consumer.UnAckedChunckedMessageIdSequenceMap.Remove(msgId);
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
                        MessageIdImpl msgId = _pendingIndividualAcks.pollFirst();
                        if (msgId == null)
                        {
                            break;
                        }

                        NewAckCommand(_consumer.ConsumerId, msgId, null, AckType.Individual, null, Collections.emptyMap(), cnx, false, -1, -1);
                        shouldFlush = true;
                    }
                }
            }

            if (!_pendingIndividualBatchIndexAcks.IsEmpty)
            {
                IEnumerator<KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable>> iterator = _pendingIndividualBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();

                while (iterator.MoveNext())
                {
                    KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable> entry = iterator.Current;
                    entriesToAck.Add(Triple.of(entry.Key.ledgerId, entry.Key.entryId, entry.Value));
                    //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
                    iterator.remove();
                }
            }

            if (!_pendingIndividualTransactionAcks.Empty)
            {
                if (Commands.PeerSupportsMultiMessageAcknowledgment(cnx.RemoteEndpointProtocolVersion))
                {
                    // We can send 1 single protobuf command with all individual acks
                    while (true)
                    {
                        Triple<long, long, MessageIdImpl> entry = _pendingIndividualTransactionAcks.pollFirst();
                        if (entry == null)
                        {
                            break;
                        }

                        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                        // them
                        MessageIdImpl[] chunkMsgIds = this._consumer.UnAckedChunckedMessageIdSequenceMap.Get(entry.Right);
                        long mostSigBits = entry.Left;
                        long leastSigBits = entry.Middle;
                        MessageIdImpl messageId = entry.Right;
                        if (chunkMsgIds != null && chunkMsgIds.Length > 1)
                        {
                            foreach (MessageIdImpl cMsgId in chunkMsgIds)
                            {
                                if (cMsgId != null)
                                {
                                    NewAckCommand(_consumer.ConsumerId, cMsgId, null, AckType.Individual, null, Collections.emptyMap(), cnx, false, mostSigBits, leastSigBits);
                                }
                            }
                            // messages will be acked so, remove checked message sequence
                            this._consumer.UnAckedChunckedMessageIdSequenceMap.Remove(messageId);
                        }
                        else
                        {
                            NewAckCommand(_consumer.ConsumerId, messageId, null, AckType.Individual, null, Collections.emptyMap(), cnx, false, mostSigBits, leastSigBits);
                        }
                    }
                }
                else
                {
                    // When talking to older brokers, send the acknowledgements individually
                    while (true)
                    {
                        Triple<long, long, MessageIdImpl> entry = _pendingIndividualTransactionAcks.pollFirst();
                        if (entry == null)
                        {
                            break;
                        }

                        NewAckCommand(_consumer.ConsumerId, entry.Right, null, AckType.Individual, null, Collections.emptyMap(), cnx, false, entry.Left, entry.Middle);
                        shouldFlush = true;
                    }
                }
            }

            if (!_pendingIndividualTransactionBatchIndexAcks.IsEmpty)
            {
                IEnumerator<KeyValuePair<TransactionImpl, ConcurrentDictionary<MessageIdImpl, ConcurrentBitSetRecyclable>>> transactionIterator = _pendingIndividualTransactionBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();
                while (transactionIterator.MoveNext())
                {
                    KeyValuePair<TransactionImpl, ConcurrentDictionary<MessageIdImpl, ConcurrentBitSetRecyclable>> transactionEntry = transactionIterator.Current;
                    TransactionImpl txn = transactionEntry.Key;
                    lock (txn)
                    {
                        if (_pendingIndividualTransactionBatchIndexAcks.ContainsKey(txn))
                        {
                            IList<Triple<long, long, ConcurrentBitSetRecyclable>> messageIdBitSetList = new List<Triple<long, long, ConcurrentBitSetRecyclable>>();
                            transactionEntriesToAck[txn] = messageIdBitSetList;
                            IEnumerator<KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable>> messageIdIterator = transactionEntry.Value.entrySet().GetEnumerator();
                            while (messageIdIterator.MoveNext())
                            {
                                KeyValuePair<MessageIdImpl, ConcurrentBitSetRecyclable> messageIdEntry = messageIdIterator.Current;
                                ConcurrentBitSetRecyclable concurrentBitSetRecyclable = ConcurrentBitSetRecyclable.Create(messageIdEntry.Value);
                                MessageIdImpl messageId = messageIdEntry.Key;
                                messageIdBitSetList.Add(Triple.of(messageId.LedgerIdConflict, messageId.EntryIdConflict, concurrentBitSetRecyclable));
                                messageIdEntry.Value.set(0, messageIdEntry.Value.size());
                                //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
                                messageIdIterator.remove();
                            }
                            //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
                            transactionIterator.remove();
                        }
                    }
                }
            }

            if (transactionEntriesToAck.Count > 0)
            {
                IEnumerator<KeyValuePair<TransactionImpl, IList<Triple<long, long, ConcurrentBitSetRecyclable>>>> iterator = transactionEntriesToAck.SetOfKeyValuePairs().GetEnumerator();
                while (iterator.MoveNext())
                {
                    KeyValuePair<TransactionImpl, IList<Triple<long, long, ConcurrentBitSetRecyclable>>> entry = iterator.Current;
                    cnx.Ctx().write(Commands.NewMultiTransactionMessageAck(_consumer.ConsumerId, new TxnID(entry.Key.TxnIdMostBits, entry.Key.TxnIdLeastBits), entry.Value), cnx.Ctx().voidPromise());
                    shouldFlush = true;
                }
            }

            if (entriesToAck.Count > 0)
            {
                cnx.Ctx().write(Commands.NewMultiMessageAck(_consumer.ConsumerId, entriesToAck), cnx.Ctx().voidPromise());
                shouldFlush = true;
            }

        }

        /// <summary>
        /// Flush all the pending acks and send them to the broker
        /// </summary>
        private void Flush()
        {
            try
            {
                if (_cumulativeAckFlushRequired)
                {
                    NewAckCommand(_consumerId, _lastCumulativeAck, _lastCumulativeAckSet, AckType.Cumulative, null, new Dictionary<string, long>(), _clientCnx, false, -1, -1);
                   _cumulativeAckFlushRequired = false;
                }

                // Flush all individual acks
                var entriesToAck = new List<(long ledgerId, long entryId, BitSet sets)>(_pendingIndividualAcks.Count + _pendingIndividualBatchIndexAcks.Count);
                if (_pendingIndividualAcks.Count > 0)
                {
                    while (true)
                    {
                        if (!_pendingIndividualAcks.TryDequeue(out var msgid))
                            break;
                        MessageId msgId;
                        if (msgid is BatchMessageId id)
                        {
                            msgId = new MessageId(id.LedgerId, id.EntryId, id.PartitionIndex);
                        }
                        else msgId = (MessageId)msgid;
                        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                        // them
                        var ask = Context.Parent.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, msgId));
                        var chunkMsgIdsResponse = SynchronizationContextSwitcher.NoContext(async () => await ask).Result; 
                        
                        var chunkMsgIds = chunkMsgIdsResponse.MessageIds;

                        if (chunkMsgIds != null && chunkMsgIds.Length > 1)
                        {
                            foreach (var cMsgId in chunkMsgIds)
                            {
                                if (cMsgId != null)
                                {
                                    entriesToAck.Add((cMsgId.LedgerId, cMsgId.EntryId, null));
                                }
                            }
                            // messages will be acked so, remove checked message sequence
                            Context.Parent.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, msgId));
                        }
                        else
                        {
                            entriesToAck.Add((msgId.LedgerId, msgId.EntryId, null));
                        }
                    }
                }

                if (!_pendingIndividualBatchIndexAcks.IsEmpty)
                {
                    using var iterator = HashMapHelper.SetOfKeyValuePairs(_pendingIndividualBatchIndexAcks).GetEnumerator();

                    foreach (var kv in _pendingIndividualBatchIndexAcks)
                    {
                        var entry = kv;
                        MessageId msgId;
                        if (entry.Key is BatchMessageId id)
                        {
                            msgId = new MessageId(id.LedgerId, id.EntryId, id.PartitionIndex);
                        }
                        else msgId = (MessageId)entry.Key;
                        entriesToAck.Add((msgId.LedgerId, msgId.EntryId, entry.Value));
                        _pendingIndividualBatchIndexAcks.Remove(entry.Key, out var pendingAck);
                    }
                }

                if (entriesToAck.Count > 0)
                {
                    var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                    var cmd = Commands.NewMultiMessageAck(_consumerId, entriesToAck);
                    var payload = new Payload(cmd, requestid, "NewMultiMessageAck");
                    _clientCnx.Tell(payload);
                }
            }
            catch (Exception e)
            {
                Context.System.Log.Error(e.ToString());
            }
            finally
            {
                _scheduledTask = Context.System.Scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(_acknowledgementGroupTimeMicros), Self, FlushPending.Instance, ActorRefs.NoSender);
            }
        }

        private void FlushAndClean()
        {
	        Flush();
	        _lastCumulativeAck = (MessageId)IMessageId.Earliest;
	        _pendingIndividualAcks.Clear();
        }

        protected override void PostStop()
        {
			Flush();
            if (_scheduledTask != null && !_scheduledTask.IsCancellationRequested)
            {
                _scheduledTask.Cancel(true);
            }
		}
        private void NewAckCommand(long consumerId, IMessageId msgId, BitSet lastCumulativeAckSet, AckType ackType, ValidationError? validationError, IDictionary<string, long> map, IActorRef cnx, bool flush, long txnidMostBits, long txnidLeastBits)
        {

            var chunkMsgIds = Context.Parent.AskFor<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, msgId)).MessageIds;
            if (chunkMsgIds != null && txnidLeastBits < 0 && txnidMostBits < 0)
            {
                var protocolVersion = cnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
                if (Commands.PeerSupportsMultiMessageAcknowledgment(protocolVersion) && ackType != AckType.Cumulative)
                {
                    IList<(long ledger, long entry, BitSet Bits)> entriesToAck = new List<(long ledger, long entry, BitSet Bits)>(chunkMsgIds.Length);
                    foreach (var cMsgId in chunkMsgIds)
                    {
                        if (cMsgId != null && chunkMsgIds.Length > 1)
                        {
                            entriesToAck.Add((cMsgId.LedgerId, cMsgId.EntryId, null));
                        }
                    }
                    var cmd = Commands.NewMultiMessageAck(_consumerId, entriesToAck);
                    cnx.Tell(new Payload(cmd, -1, "NewMultiMessageAck"));
                }
                else
                {
                    foreach (var cMsgId in chunkMsgIds)
                    {
                        var cmd = Commands.NewAck(consumerId, cMsgId.LedgerId, cMsgId.EntryId, lastCumulativeAckSet.ToLongArray(), ackType, validationError, map);
                        cnx.Tell(new Payload(cmd, -1, "NewAck"));
                    }
                }
                Context.Parent.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, msgId));
            }
            else
            {
                var mid = (MessageId)msgId;
                var cmd = Commands.NewAck(consumerId, mid.LedgerId, mid.EntryId, lastCumulativeAckSet.ToLongArray(), ackType, validationError, map, txnidLeastBits, txnidMostBits, -1);
                cnx.Tell(new Payload(cmd, -1, "NewAck"));
            }
        }
    }
    
    public sealed class FlushPending
    {
		public static FlushPending Instance = new FlushPending();
    }
    public sealed class AddListAcknowledgment
    {
        public IList<MessageId> MessageIds { get; }
        public AckType AckType { get; }
        public IDictionary<string, long> Properties { get; }
        public AddListAcknowledgment(IList<MessageId> messageIds, AckType ackType, IDictionary<string, long> properties)
        {
            MessageIds = messageIds;
            AckType = ackType;
            Properties = properties;
        }
    }
}