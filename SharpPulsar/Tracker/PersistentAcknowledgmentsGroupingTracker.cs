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
using SharpPulsar.Tracker.Messages;
using SharpPulsar.Interfaces;
using static SharpPulsar.Protocol.Proto.CommandAck;
using Akka.Util.Internal;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Transaction;
using System.Collections;
using System.Threading.Tasks;

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
        private readonly long _consumerId;
        private readonly IActorRef _consumer;
        private IActorRef _conx;
        private object[] _invokeAgrs;
        private Action _nextBecome;

		private readonly long _acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private IMessageId _lastCumulativeAck = IMessageId.Earliest;
        private BitSet _lastCumulativeAckSet;
        private bool _cumulativeAckFlushRequired;


        /// <summary>
        /// This is a set of all the individual acks that the application has issued and that were not already sent to
        /// broker.
        /// </summary>
        private readonly Queue<IMessageId> _pendingIndividualAcks;
        private readonly IActorRef _handler;
        private readonly ConcurrentDictionary<IMessageId, BitSet> _pendingIndividualBatchIndexAcks;
        private readonly Queue<(long MostSigBits, long LeastSigBits, MessageId MessageId)> _pendingIndividualTransactionAcks;

        private readonly ConcurrentDictionary<IActorRef, Dictionary<MessageId, BitSet>> _pendingIndividualTransactionBatchIndexAcks;

        private  ICancelable _scheduledTask;

        public PersistentAcknowledgmentsGroupingTracker(IActorRef consumer, long consumerid, IActorRef handler, ConsumerConfigurationData<T> conf)
        {
            _handler = handler;
            _consumer = consumer;
            _consumerId = consumerid;
			_pendingIndividualAcks = new Queue<IMessageId>();
            _acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;
            _pendingIndividualBatchIndexAcks = new ConcurrentDictionary<IMessageId, BitSet>();
            _pendingIndividualTransactionBatchIndexAcks = new ConcurrentDictionary<IActorRef, Dictionary<MessageId, BitSet>>();
            _pendingIndividualTransactionAcks = new Queue<(long MostSigBits, long LeastSigBits, MessageId MessageId)>();
            BecomeActive();
			_scheduledTask = _acknowledgementGroupTimeMicros > 0 ? Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(TimeSpan.FromMilliseconds(_acknowledgementGroupTimeMicros), TimeSpan.FromMilliseconds(_acknowledgementGroupTimeMicros), Self, FlushPending.Instance, ActorRefs.NoSender) : null;
		}

        private void BecomeActive()
        {
            Receive<IsDuplicate>(d =>
            {
                var isd = IsDuplicate(d.MessageId);
                Sender.Tell(isd);
            });
            ReceiveAsync<AddAcknowledgment>(async d => 
            {
                _invokeAgrs = new object[] { d.MessageId, d.AckType, d.Properties, d.Txn };
                await AddAcknowledgment(d.MessageId, d.AckType, d.Properties, d.Txn); 
            });
            ReceiveAsync<AddBatchIndexAcknowledgment>(async d => 
            {
                _invokeAgrs = new object[] { d.MessageId, d.BatchIndex, d.BatchSize, d.AckType, d.Properties, d.Txn };
                await AddBatchIndexAcknowledgment(d.MessageId, d.BatchIndex, d.BatchSize, d.AckType, d.Properties, d.Txn); 
            
            });
            ReceiveAsync<FlushAndClean>( async _ => await FlushAndClean());
            ReceiveAsync<FlushPending>(async _ => await Flush());
            ReceiveAsync<AddListAcknowledgment>(async a => 
            {
                _invokeAgrs = new object[] { a.MessageIds, a.AckType, a.Properties };
                await AddListAcknowledgment(a.MessageIds, a.AckType, a.Properties);
            });
        }
        public static Props Prop(IActorRef consumer, long consumerid, IActorRef handler, ConsumerConfigurationData<T> conf)
        {
			return Props.Create(()=> new PersistentAcknowledgmentsGroupingTracker<T>(consumer, consumerid, handler, conf));
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
        private async ValueTask AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            if ((_acknowledgementGroupTimeMicros == 0 || properties.Count > 0) && txn != null)
            {
                var bits = await txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                await DoImmediateBatchIndexAck(msgId, batchIndex, batchSize, ackType, properties, txn == null ? -1 : bits.MostBits, txn == null ? -1 : bits.LeastBits);
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
                    await Flush();
                }
            }
        }
        private async ValueTask AddListAcknowledgment(IList<MessageId> messageIds, AckType ackType, IDictionary<string, long> properties)
        {
            if (ackType == AckType.Cumulative)
            {
                messageIds.ForEach(messageId => DoCumulativeAck(messageId, null));
                return;
            }
            messageIds.ForEach(async messageId =>
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
                    await Flush();
                }
            });
            if (_acknowledgementGroupTimeMicros == 0)
            {
                await Flush();
            }
        }
        private async ValueTask AddAcknowledgment(IMessageId msgId, AckType ackType, IDictionary<string, long> properties, IActorRef txn)
        {
            if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0 || (txn != null && ackType == AckType.Cumulative))
            {
                if (msgId is BatchMessageId && txn != null)
                {
                    var bits = await txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                    var batchMessageId = (BatchMessageId)msgId;
                    await DoImmediateBatchIndexAck(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchIndex, ackType, properties, bits.MostBits, bits.LeastBits);
                    return;
                }
                // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
                // uncommon condition since it's only used for the compaction subscription.
                await DoImmediateAck(msgId, ackType, properties, txn);
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
                        var bits = await txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);

                        _pendingIndividualTransactionAcks.Enqueue((bits.MostBits, bits.LeastBits, (MessageId)msgId));
                    }
                    else
                    {
                        _pendingIndividualAcks.Enqueue(msgId);
                    }
                }
                _pendingIndividualBatchIndexAcks.Remove(msgId, out _);
                if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
                {
                    await Flush();
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

        private async ValueTask<bool> DoImmediateAck(IMessageId msgId, AckType ackType, IDictionary<string, long> properties, IActorRef transaction)
        {
            if (transaction != null)
            {
                var bits = await transaction.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                await NewAckCommand(_consumerId, msgId, null, ackType, null, properties, true, bits.MostBits, bits.LeastBits);
            }
            else
            {
                await NewAckCommand(_consumerId, msgId, null, ackType, null, properties, true, -1, -1);
            }
            return true;
        }
        private async ValueTask<bool> DoImmediateBatchIndexAck(BatchMessageId msgId, int batchIndex, int batchSize, AckType ackType, IDictionary<string, long> properties, long txnidMostBits, long txnidLeastBits)
        {
            var cnx = await Cnx();
            BitArray bitSet = new BitArray(msgId.Acker.BatchSize, true);
            if (ackType == AckType.Cumulative)
            {
                for (var i = 0; i <= batchSize; i++)
                    bitSet[i] = false;
            }
            else
            {
                bitSet[batchIndex] = false;
            }
            var cmd = new Commands().NewAck(_consumerId, msgId.LedgerId, msgId.EntryId, bitSet.ToLongArray(), ackType, null, properties, txnidLeastBits, txnidMostBits, -1);
            var payload = new Payload(cmd, -1, "NewAck");
            cnx.Tell(payload);
            return true;
        }
        /// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		private async ValueTask Flush()
        {
            var cnx = await Cnx();
            try
            {
                if (_cumulativeAckFlushRequired)
                {
                    await NewAckCommand(_consumerId, _lastCumulativeAck, _lastCumulativeAckSet, AckType.Cumulative, null, new Dictionary<string, long>(), false, -1, -1);
                    _cumulativeAckFlushRequired = false;
                }

                // Flush all individual acks
                IList<(long ledger, long entry, BitSet bitSet)> entriesToAck = new List<(long ledger, long entry, BitSet bitSet)>(_pendingIndividualAcks.Count + _pendingIndividualBatchIndexAcks.Count);
                Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>> transactionEntriesToAck = new Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>>();
                if (_pendingIndividualAcks.Count > 0)
                {
                    var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
                    var protocolVersion = version.Version;
                    if (new Commands().PeerSupportsMultiMessageAcknowledgment(protocolVersion))
                    {
                        // We can send 1 single protobuf command with all individual acks
                        while (true)
                        {
                            if (!_pendingIndividualAcks.TryDequeue(out var msgId))
                            {
                                break;
                            }

                            // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                            // them
                            var result = await _consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { msgId }));
                            var chunkMsgIds = result.MessageIds;
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
                                _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { msgId }));
                            }
                            else
                            {
                                var msgid = (MessageId)msgId;
                                entriesToAck.Add((msgid.LedgerId, msgid.EntryId, null));
                            }
                        }
                    }
                    else
                    {
                        // When talking to older brokers, send the acknowledgements individually
                        while (true)
                        {
                            if (!_pendingIndividualAcks.TryDequeue(out var messageId))
                            {
                                break;
                            }
                            MessageId msgId = (MessageId)messageId;
                            await NewAckCommand(_consumerId, msgId, null, AckType.Individual, null, new Dictionary<string, long>(), false, -1, -1);

                        }
                    }
                }

                if (_pendingIndividualBatchIndexAcks.Count > 0)
                {
                    var acks = _pendingIndividualBatchIndexAcks.SetOfKeyValuePairs();

                    foreach(var ack in acks)
                    {
                        var key = (MessageId)ack.Key;
                        entriesToAck.Add((key.LedgerId, key.EntryId, ack.Value));
                        _pendingIndividualBatchIndexAcks.Remove(key, out var u);
                    }
                }

                if (_pendingIndividualTransactionAcks.Count > 0)
                {
                    var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
                    var protocolVersion = version.Version;
                    if (new Commands().PeerSupportsMultiMessageAcknowledgment(protocolVersion))
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
                            var result = await _consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId>{entry.MessageId}));
                            var chunkMsgIds = result.MessageIds;
                            long mostSigBits = entry.MostSigBits;
                            long leastSigBits = entry.LeastSigBits;
                            var messageId = entry.MessageId;
                            if (chunkMsgIds != null && chunkMsgIds.Length > 1)
                            {
                                foreach (var cMsgId in chunkMsgIds)
                                {
                                    if (cMsgId != null)
                                    {
                                        await NewAckCommand(_consumerId, cMsgId, null, AckType.Individual, null, new Dictionary<string, long>(), false, mostSigBits, leastSigBits);
                                    }
                                }
                                // messages will be acked so, remove checked message sequence

                                _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { messageId }));
                            }
                            else
                            {
                                await NewAckCommand(_consumerId, messageId, null, AckType.Individual, null, new Dictionary<string, long>(), false, mostSigBits, leastSigBits);
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

                            await NewAckCommand(_consumerId, entry.MessageId, null, AckType.Individual, null, new Dictionary<string, long>(), false, entry.MostSigBits, entry.LeastSigBits);

                        }
                    }
                }

                if (!_pendingIndividualTransactionBatchIndexAcks.IsEmpty)
                {
                    var acks = _pendingIndividualTransactionBatchIndexAcks.SetOfKeyValuePairs();
                    foreach(var ack in acks)
                    {
                        var txn = ack.Key;
                        if (_pendingIndividualTransactionBatchIndexAcks.ContainsKey(txn))
                        {
                            var messageIdBitSetList = new List<(long ledger, long entry, BitSet bitSet)>();
                            transactionEntriesToAck[txn] = messageIdBitSetList;
                            var messageIds = ack.Value;
                            foreach (var id in messageIds)
                            {
                                var bitSet = id.Value;
                                var messageId = id.Key;
                                messageIdBitSetList.Add((messageId.LedgerId, messageId.EntryId, bitSet));
                                id.Value.Set(0, id.Value.Size());

                                _pendingIndividualTransactionBatchIndexAcks[txn].Remove(messageId);

                                _pendingIndividualTransactionBatchIndexAcks.Remove(txn, out var m);
                            }
                        }
                    }
                    if (transactionEntriesToAck.Count > 0)
                    {
                        var toAcks = transactionEntriesToAck.SetOfKeyValuePairs();
                        foreach(var ack in toAcks)
                        {
                            var bits = await ack.Key.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance);
                            var cmd = new Commands().NewMultiTransactionMessageAck(_consumerId, new TxnID(bits.MostBits, bits.LeastBits), ack.Value);
                            var payload = new Payload(cmd, -1, "NewMultiTransactionMessageAck");
                            cnx.Tell(payload);
                        }
                    }

                    if (entriesToAck.Count > 0)
                    {
                        var cmd = new Commands().NewMultiMessageAck(_consumerId, entriesToAck);
                        var payload = new Payload(cmd, -1, "NewMultiMessageAck");
                        cnx.Tell(payload);
                    }
                }
            }
            catch (Exception ex)
            {
                Context.System.Log.Error(ex.ToString());
            }
        }
        private async ValueTask<IActorRef> Cnx()
        {
            if(_conx == null)
                _conx = await _handler.Ask<IActorRef>(GetCnx.Instance);

            return _conx;
        }
        private async ValueTask FlushAndClean()
        {
	        await Flush();
	        _lastCumulativeAck = (MessageId)IMessageId.Earliest;
	        _pendingIndividualAcks.Clear();
        }

        protected override void PostStop()
        {
			Flush().ConfigureAwait(false);
            if (_scheduledTask != null && !_scheduledTask.IsCancellationRequested)
            {
                _scheduledTask.Cancel(true);
            }
		}
        private async ValueTask NewAckCommand(long consumerId, IMessageId msgId, BitSet lastCumulativeAckSet, AckType ackType, ValidationError? validationError, IDictionary<string, long> map, bool flush, long txnidMostBits, long txnidLeastBits)
        {
            var cnx = await Cnx();
            var result = await _consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { msgId }));
            var chunkMsgIds = result.MessageIds;
            if (chunkMsgIds?.Length > 0 && txnidLeastBits < 0 && txnidMostBits < 0)
            {

                var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
                var protocolVersion = version.Version;
                if (new Commands().PeerSupportsMultiMessageAcknowledgment(protocolVersion) && ackType != AckType.Cumulative)
                {
                    IList<(long ledger, long entry, BitSet Bits)> entriesToAck = new List<(long ledger, long entry, BitSet Bits)>(chunkMsgIds.Length);
                    foreach (var cMsgId in chunkMsgIds)
                    {
                        if (cMsgId != null && chunkMsgIds.Length > 1)
                        {
                            entriesToAck.Add((cMsgId.LedgerId, cMsgId.EntryId, null));
                        }
                    }
                    var cmd = new Commands().NewMultiMessageAck(_consumerId, entriesToAck);
                    cnx.Tell(new Payload(cmd, -1, "NewMultiMessageAck"));
                }
                else
                {
                    foreach (var cMsgId in chunkMsgIds)
                    {
                        var cmd = new Commands().NewAck(consumerId, cMsgId.LedgerId, cMsgId.EntryId, lastCumulativeAckSet.ToLongArray(), ackType, validationError, map);
                        cnx.Tell(new Payload(cmd, -1, "NewAck"));
                    }
                }
                _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { msgId }));
            }
            else
            {
                var sets = new long[] { };

                if (lastCumulativeAckSet != null)
                    sets = lastCumulativeAckSet.ToLongArray();

                var mid = (MessageId)msgId;

                var cmd = new Commands().NewAck(consumerId, mid.LedgerId, mid.EntryId, sets, ackType, validationError, map, txnidLeastBits, txnidMostBits, -1);
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