using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
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
using SharpPulsar.Transaction;

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
        private readonly Queue<(long MostSigBits, long LeastSigBits, MessageId MessageId)> _pendingIndividualTransactionAcks;

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
            _pendingIndividualTransactionAcks = new Queue<(long MostSigBits, long LeastSigBits, MessageId MessageId)>();
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
            Receive<AddAcknowledgment>(d => 
            { 
                AddAcknowledgment(d.MessageId, d.AckType, d.Properties, d.Txn); 
            });
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
        public virtual void AddAcknowledgment(IMessageId msgId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0 || (txn != null && ackType == AckType.Cumulative))
            {
                if (msgId is BatchMessageId && txn != null)
                {
                    var bits = txn.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                    var batchMessageId = (BatchMessageId)msgId;
                    DoImmediateBatchIndexAck(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchIndex, ackType, properties, bits.MostBits, bits.LeastBits);
                    return;
                }
                // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
                // uncommon condition since it's only used for the compaction subscription.
                DoImmediateAck(msgId, ackType, properties, txn);
            }
            else if (ackType == AckType.Cumulative)
            {
                DoCumulativeAck(msgId, null);
            }
            else
            {
                // Individual ack
                if (msgId is BatchMessageId m)
                {
                    _pendingIndividualAcks.Enqueue(new MessageId(m.LedgerId, m.EntryId, m.PartitionIndex));
                }
                else
                {
                    if (txn != null)
                    {
                        var bits = txn.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);

                        _pendingIndividualTransactionAcks.Enqueue((bits.MostBits, bits.LeastBits, (MessageId)msgId));
                    }
                    else
                    {
                        _pendingIndividualAcks.Enqueue(msgId);
                    }
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

        private bool DoImmediateAck(IMessageId msgId, AckType ackType, IDictionary<string, long> properties, IActorRef transaction)
        {
            if (transaction != null)
            {
                var bits = transaction.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                NewAckCommand(_consumerId, msgId, null, ackType, null, properties, _clientCnx, true, bits.MostBits, bits.LeastBits);
            }
            else
            {
                NewAckCommand(_consumerId, msgId, null, ackType, null, properties, _clientCnx, true, -1, -1);
            }
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
            IList<(long ledger, long entry, BitSet bitSet)> entriesToAck = new List<(long ledger, long entry, BitSet bitSet)>(_pendingIndividualAcks.Count + _pendingIndividualBatchIndexAcks.Count);
            Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>> transactionEntriesToAck = new Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>>();
            if (_pendingIndividualAcks.Count > 0)
            {
                var protocolVersion = _clientCnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
                if (Commands.PeerSupportsMultiMessageAcknowledgment(protocolVersion))
                {
                    // We can send 1 single protobuf command with all individual acks
                    while (true)
                    {
                        MessageId msgId = (MessageId)_pendingIndividualAcks.Dequeue();
                        if (msgId == null)
                        {
                            break;
                        }

                        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                        // them
                        var chunkMsgIds = Context.Parent.AskFor<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, msgId)).MessageIds;
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
                else
                {
                    // When talking to older brokers, send the acknowledgements individually
                    while (true)
                    {
                        MessageId msgId = (MessageId)_pendingIndividualAcks.Dequeue();
                        if (msgId == null)
                        {
                            break;
                        }

                        NewAckCommand(_consumerId, msgId, null, AckType.Individual, null, new Dictionary<string, long>(), _clientCnx, false, -1, -1);
                        
                    }
                }
            }

            if (!_pendingIndividualBatchIndexAcks.IsEmpty)
            {
                var iterator = _pendingIndividualBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();

                while (iterator.MoveNext())
                {
                    var entry = iterator.Current;
                    var key = (MessageId)entry.Key;
                    entriesToAck.Add((key.LedgerId, key.EntryId, entry.Value));
                    _pendingIndividualBatchIndexAcks.Remove(key, out var u);
                }
            }

            if (_pendingIndividualTransactionAcks.Count > 0)
            {
                var protocolVersion = _clientCnx.AskFor<int>(RemoteEndpointProtocolVersion.Instance);
                if (Commands.PeerSupportsMultiMessageAcknowledgment(protocolVersion))
                {
                    // We can send 1 single protobuf command with all individual acks
                    while (true)
                    {
                        if (!_pendingIndividualTransactionAcks.TryDequeue(out var entry))
                        {
                            break;
                        }

                        // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                        // them
                        var chunkMsgIds = Context.Parent.AskFor<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, entry.MessageId)).MessageIds;
                        long mostSigBits = entry.MostSigBits;
                        long leastSigBits = entry.LeastSigBits;
                        var messageId = entry.MessageId;
                        if (chunkMsgIds != null && chunkMsgIds.Length > 1)
                        {
                            foreach (var cMsgId in chunkMsgIds)
                            {
                                if (cMsgId != null)
                                {
                                    NewAckCommand(_consumerId, cMsgId, null, AckType.Individual, null, new Dictionary<string, long>(), _clientCnx, false, mostSigBits, leastSigBits);
                                }
                            }
                            // messages will be acked so, remove checked message sequence

                            Context.Parent.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, messageId));
                        }
                        else
                        {
                            NewAckCommand(_consumerId, messageId, null, AckType.Individual, null, new Dictionary<string, long>(), _clientCnx, false, mostSigBits, leastSigBits);
                        }
                    }
                }
                else
                {
                    // When talking to older brokers, send the acknowledgements individually
                    while (true)
                    {
                        if (!_pendingIndividualTransactionAcks.TryDequeue(out var entry))
                        {
                            break;
                        }

                        NewAckCommand(_consumerId, entry.MessageId, null, AckType.Individual, null, new Dictionary<string, long>(), _clientCnx, false, entry.MostSigBits, entry.LeastSigBits);
                        
                    }
                }
            }

            if (!_pendingIndividualTransactionBatchIndexAcks.IsEmpty)
            {
                var transactionIterator = _pendingIndividualTransactionBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();
                while (transactionIterator.MoveNext())
                {
                    var transactionEntry = transactionIterator.Current;
                    var txn = transactionEntry.Key;
                    if (_pendingIndividualTransactionBatchIndexAcks.ContainsKey(txn))
                    {
                        var messageIdBitSetList = new List<(long ledger, long entry, BitSet bitSet)>();
                        transactionEntriesToAck[txn] = messageIdBitSetList;
                        var messageIdIterator = transactionEntry.Value.GetEnumerator();
                        while (messageIdIterator.MoveNext())
                        {
                            var messageIdEntry = messageIdIterator.Current;
                            var bitSet = messageIdEntry.Value;
                            var messageId = messageIdEntry.Key;
                            messageIdBitSetList.Add((messageId.LedgerId, messageId.EntryId, bitSet));
                            messageIdEntry.Value.Set(0, messageIdEntry.Value.Size());

                            _pendingIndividualTransactionBatchIndexAcks[txn].Remove(messageId);
                        }
                        //JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
                        _pendingIndividualTransactionBatchIndexAcks.Remove(txn, out var m);
                    }
                }
            }

            if (transactionEntriesToAck.Count > 0)
            {
                var iterator = transactionEntriesToAck.SetOfKeyValuePairs().GetEnumerator();
                while (iterator.MoveNext())
                {
                    var entry = iterator.Current;
                    var bits = entry.Key.AskFor<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                    var cmd = Commands.NewMultiTransactionMessageAck(_consumerId, new TxnID(bits.MostBits, bits.LeastBits), entry.Value);
                    var payload = new Payload(cmd, -1, "NewMultiTransactionMessageAck");
                    _clientCnx.Tell(payload);
                }
            }

            if (entriesToAck.Count > 0)
            {
                var cmd = Commands.NewMultiMessageAck(_consumerId, entriesToAck);
                var payload = new Payload(cmd, -1, "NewMultiMessageAck");
                _clientCnx.Tell(payload);
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