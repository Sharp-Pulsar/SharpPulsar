using System;
using System.Collections.Generic;
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
using SharpPulsar.Messages.Requests;
using System.Collections;
using System.Threading.Tasks;
using SharpPulsar.Messages.Consumer;
using System.Linq;
using System.Buffers;
using SharpPulsar.Messages.Client;

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
        private const int MaxAckGroupSize = 50;//1000;
        private readonly long _consumerId;
        private readonly IActorRef _consumer;
        private readonly IActorRef _generator;
        private IActorRef _conx;

		private readonly TimeSpan _acknowledgementGroupTime;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private IMessageId _lastCumulativeAck = IMessageId.Earliest;
        private bool _cumulativeAckFlushRequired;


        /// <summary>
        /// This is a set of all the individual acks that the application has issued and that were not already sent to
        /// broker.
        /// </summary>
        private readonly SortedSet<MessageId> _pendingIndividualAcks;
        private readonly IActorRef _handler;
        private readonly SortedSet<MessageId> _pendingIndividualBatchIndexAcks;

        private readonly ICancelable _scheduledTask;

        private readonly bool _batchIndexAckEnabled;
        private readonly bool _ackReceiptEnabled;
        private readonly IActorRef _unAckedChunckedMessageIdSequenceMap;

        public PersistentAcknowledgmentsGroupingTracker(IActorRef sequenceMap, IActorRef consumer, IActorRef generator, long consumerid, IActorRef handler, ConsumerConfigurationData<T> conf)
        {
            _handler = handler;
            _consumer = consumer;
            _generator = generator;
            _consumerId = consumerid;
            _pendingIndividualAcks = new SortedSet<MessageId>();
            _acknowledgementGroupTime = conf.AcknowledgementsGroupTime;
            _pendingIndividualBatchIndexAcks = new SortedSet<MessageId>();
            _ackReceiptEnabled = conf.AckReceiptEnabled;
            _batchIndexAckEnabled = conf.BatchIndexAckEnabled;
            _unAckedChunckedMessageIdSequenceMap = sequenceMap;
            BecomeActive();
			_scheduledTask = _acknowledgementGroupTime.TotalMilliseconds > 0 ? Context.System.Scheduler.ScheduleTellRepeatedlyCancelable(_acknowledgementGroupTime, _acknowledgementGroupTime, Self, FlushPending.Instance, ActorRefs.NoSender) : null;
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
                await AddAcknowledgment(d.MessageId, d.AckType, d.Properties); 
                Sender.Tell("done");
            });
            ReceiveAsync<FlushAndClean>( async _ => await FlushAndClean());
            ReceiveAsync<FlushPending>(async _ => await Flush());
            ReceiveAsync<AddListAcknowledgment>(async a => 
            {
                await AddListAcknowledgment(a.MessageIds, a.AckType, a.Properties);
            });
        }
        
        public static Props Prop(IActorRef sequenceMap, IActorRef consumer, IActorRef generator, long consumerid, IActorRef handler, ConsumerConfigurationData<T> conf)
        {
			return Props.Create(()=> new PersistentAcknowledgmentsGroupingTracker<T>(sequenceMap, consumer, generator, consumerid, handler, conf));
        }
		/// <summary>
		/// Since the ack are delayed, we need to do some best-effort duplicate check to discard messages that are being
		/// resent after a disconnection and for which the user has already sent an acknowledgement.
		/// </summary>
		private bool IsDuplicate(IMessageId messageId)
        {
            if (_lastCumulativeAck == null)
                return false;

            if (messageId.CompareTo(_lastCumulativeAck) <= 0)
	        {
		        // Already included in a cumulative ack
		        return true;
	        }

            return _pendingIndividualAcks.Contains(messageId);
        }
        private async ValueTask AddListAcknowledgment(IList<IMessageId> messageIds, AckType ackType, IDictionary<string, long> properties)
        {
            if (ackType == AckType.Cumulative)
            {
                var cnx = await Cnx();
                if (await IsAckReceiptEnabled(cnx))
                {
                    ISet<Task> taskSet = new HashSet<Task>();
                    messageIds.ForEach(messageId => taskSet.Add(AddAcknowledgment(messageId, ackType, properties).AsTask()));
                    Task.WaitAll(taskSet.ToArray());
                }
                else
                {
                    messageIds.ForEach(async messageId => await AddAcknowledgment(messageId, ackType, properties));
                   
                }
            }
            else
            {
                var cnx = await Cnx();
                if (await IsAckReceiptEnabled(cnx))
                {
                    // when flush the ack, we should bind the this ack in the currentFuture, during this time we can't
                    // change currentFuture. but we can lock by the read lock, because the currentFuture is not change
                    // any ack operation is allowed.
                    
                    try
                    {
                        if (messageIds.Count != 0)
                        {
                            AddListAcknowledgment(messageIds);
                        }
                    }
                    finally
                    {
                        if (_acknowledgementGroupTime.TotalMilliseconds == 0 || _pendingIndividualAcks.Count >= MaxAckGroupSize)
                        {
                            await Flush();
                        }
                    }
                }
                else
                {
                    AddListAcknowledgment(messageIds);
                    if (_acknowledgementGroupTime.TotalMilliseconds == 0 || _pendingIndividualAcks.Count >= MaxAckGroupSize)
                    {
                       await Flush();
                    }
                }
            }
        }
        private void AddListAcknowledgment(IList<IMessageId> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                _consumer.Tell(new OnAcknowledge(messageId, null));
                if (messageId is BatchMessageId batchMessageId)
                {
                    if (!batchMessageId.AckIndividual())
                    {
                        DoIndividualBatchAckAsync(batchMessageId);
                    }
                    else
                    {
                        var msgId = ModifyBatchMessageIdAndStatesInConsumer(batchMessageId);
                        DoIndividualAckAsync(msgId);
                    }
                }
                else
                {
                    ModifyMessageIdStatesInConsumer((MessageId)messageId);
                    DoIndividualAckAsync((MessageId)messageId);
                }
            }
        }
        private async ValueTask AddAcknowledgment(IMessageId msgId, AckType ackType, IDictionary<string, long> properties)
        {
            if (msgId is BatchMessageId batchMessageId)
            {
                if (ackType == AckType.Individual)
                {
                    _consumer.Tell(new OnAcknowledge(batchMessageId, null));
                    // ack this ack carry bitSet index and judge bit set are all ack
                    if (batchMessageId.AckIndividual())
                    {
                        var messageId = ModifyBatchMessageIdAndStatesInConsumer(batchMessageId);
                        await DoIndividualAck(messageId, properties);
                    }
                    else if (_batchIndexAckEnabled)
                    {
                        await DoIndividualBatchAck(batchMessageId, properties);
                    }
                    else
                    {
                        // if we prevent batchIndexAck, we can't send the ack command to broker when the batch message are
                        // all ack complete
                        return;// CompletableFuture.completedFuture(null);
                    }
                }
                else
                {
                    _consumer.Tell(new OnAcknowledgeCumulative(batchMessageId, null));
                    if (batchMessageId.AckCumulative())
                    {
                        await DoCumulativeAck(batchMessageId, properties, null);
                    }
                    else
                    {
                        if (_batchIndexAckEnabled)
                        {
                            await DoCumulativeBatchIndexAck(batchMessageId, properties);
                        }
                        else
                        {
                            // ack the pre messageId, because we prevent the batchIndexAck, we can ensure pre messageId can
                            // ack
                            if (AckType.Cumulative == ackType && !batchMessageId.Acker.PrevBatchCumulativelyAcked)
                            {
                                await DoCumulativeAck(batchMessageId.PrevBatchMessageId(), properties, null);
                                batchMessageId.Acker.PrevBatchCumulativelyAcked = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (ackType == AckType.Individual)
                {
                    _consumer.Tell(new OnAcknowledge(msgId, null));
                    ModifyMessageIdStatesInConsumer((MessageId)msgId);
                    await DoIndividualAck((MessageId)msgId, properties);
                }
                else
                {
                    _consumer.Tell(new OnAcknowledgeCumulative(msgId, null));
                    await DoCumulativeAck((MessageId)msgId, properties, null);
                }
            }
        }
        private async ValueTask DoCumulativeBatchIndexAck(BatchMessageId batchMessageId, IDictionary<string, long> properties)
        {
            if (_acknowledgementGroupTime.TotalMilliseconds == 0 || (properties != null && properties.Count > 0))
            {
                await DoImmediateBatchIndexAck(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, AckType.Cumulative, properties);
            }
            else
            {
                var bitSet = new BitArray(batchMessageId.BatchSize, true);
                bitSet[batchMessageId.BatchSize] = false;
                await DoCumulativeAck(batchMessageId, null, bitSet.ToLongArray());
            }
        }
        private async ValueTask DoIndividualBatchAck(BatchMessageId batchMessageId, IDictionary<string, long> properties)
        {
            if (_acknowledgementGroupTime.TotalMilliseconds == 0 || (properties != null && properties.Count > 0))
            {
                await DoImmediateBatchIndexAck(batchMessageId, batchMessageId.BatchIndex, batchMessageId.BatchSize, AckType.Individual, properties);
            }
            else
            {
                await DoIndividualBatchAck(batchMessageId);
            }
        }

        private async ValueTask DoIndividualBatchAck(BatchMessageId batchMessageId)
        {
            var cnx = await Cnx();
            if (await IsAckReceiptEnabled(cnx))
            {
                try
                {
                    DoIndividualBatchAckAsync(batchMessageId);
                }
                finally
                {
                    
                }
            }
            else
            {
                DoIndividualBatchAckAsync(batchMessageId);
            }
        }

        private async ValueTask DoCumulativeAck(MessageId messageId, IDictionary<string, long> properties, long[] bitset)
        {
            var count = await _consumer.Ask<int>(new RemoveMessagesTill(messageId)).ConfigureAwait(false);
            _consumer.Tell(new IncrementNumAcksSent(count));
            if (_acknowledgementGroupTime.TotalMilliseconds == 0 || (properties != null && properties.Count > 0))
            {
                // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
                // uncommon condition since it's only used for the compaction subscription.
                await DoImmediateAck(messageId, AckType.Cumulative, properties, bitset);
            }
            else
            {
                var cnx = await Cnx();
                if (await IsAckReceiptEnabled(cnx))
                {
                    try
                    {
                        DoCumulativeAckAsync(messageId);
                    }
                    finally
                    {
                        if (_pendingIndividualBatchIndexAcks.Count >= MaxAckGroupSize)
                        {
                            await Flush();
                        }
                    }
                }
                else
                {
                    DoCumulativeAckAsync(messageId);
                    if (_pendingIndividualBatchIndexAcks.Count >= MaxAckGroupSize)
                    {
                        await Flush();
                    }
                }
            }
        }
        private void DoCumulativeAckAsync(MessageId msgId)
        {
            if(msgId.CompareTo(_lastCumulativeAck) > 0)
            {
                _lastCumulativeAck = msgId;
                _cumulativeAckFlushRequired = true;
            }   
        }

        private void DoIndividualBatchAckAsync(BatchMessageId batchMessageId)
        {
            var msgId = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
            if (!_pendingIndividualBatchIndexAcks.TryGetValue(msgId, out _))
            {
                _pendingIndividualBatchIndexAcks.Add(msgId);
            }
        }
        private async ValueTask<bool> IsAckReceiptEnabled(IActorRef cnx)
        {
            var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
            var protocolVersion = version.Version;
            return _ackReceiptEnabled && cnx != null && Commands.PeerSupportsAckReceipt(protocolVersion);
        }
        private MessageId ModifyBatchMessageIdAndStatesInConsumer(BatchMessageId batchMessageId)
        {
            var messageId = new MessageId(batchMessageId.LedgerId, batchMessageId.EntryId, batchMessageId.PartitionIndex);
            _consumer.Tell(new IncrementNumAcksSent(batchMessageId.BatchSize));
            ClearMessageIdFromUnAckTrackerAndDeadLetter(messageId);
            return messageId;
        }

        private void ModifyMessageIdStatesInConsumer(MessageId messageId)
        {
            _consumer.Tell(new IncrementNumAcksSent(1));
            ClearMessageIdFromUnAckTrackerAndDeadLetter(messageId);
        }

        private void ClearMessageIdFromUnAckTrackerAndDeadLetter(MessageId messageId)
        {
            _consumer.Tell(new UnAckedMessageTrackerRemove(messageId));
            _consumer.Tell(new PossibleSendToDeadLetterTopicMessagesRemove(messageId));
        }
        private async ValueTask DoImmediateAck(MessageId msgId, AckType ackType, IDictionary<string, long> properties, long[] bitSet)
        {
            var cnx = await Cnx();

            if (cnx == null)
            {
                Context.System.Log.Error("Consumer connect fail!");
            }
            else
               await NewImmediateAckAndFlush(_consumerId, msgId, bitSet, ackType, properties, cnx);
        }
        private async ValueTask DoImmediateBatchIndexAck(BatchMessageId msgId, int batchIndex, int batchSize, AckType ackType, IDictionary<string, long> properties)
        {
            var cnx = await Cnx();

            if (cnx == null)
            {
                Context.System.Log.Error("Consumer connect fail!");
                return ;
            }
            BitArray bitSet;
            if (msgId.Acker != null && !(msgId.Acker is BatchMessageAckerDisabled))
            {
                bitSet = new BitArray(msgId.Acker.BatchSize, true);
            }
            else
            {
                bitSet = new BitArray(batchSize, true);
            }
            if (ackType == AckType.Cumulative)
            {
                for(var j = 0; j <= batchIndex; j++)
                    bitSet[j] = false;
            }
            else
            {
                bitSet[batchIndex] = false;
            }

            await NewMessageAckCommandAndWrite(cnx, _consumerId, msgId.LedgerId, msgId.EntryId, bitSet.ToLongArray(), ackType, null, properties, true, null);
            
        }
        private async ValueTask DoIndividualAck(MessageId messageId, IDictionary<string, long> properties)
        {
            if (_acknowledgementGroupTime.TotalMilliseconds == 0 || (properties != null && properties.Count > 0))
            {
                // We cannot group acks if the delay is 0 or when there are properties attached to it. Fortunately that's an
                // uncommon condition since it's only used for the compaction subscription.
                await DoImmediateAck(messageId, AckType.Individual, properties, null);
            }
            else
            {
                var cnx = await Cnx();
                if (await IsAckReceiptEnabled(cnx))
                {
                    try
                    {
                        DoIndividualAckAsync(messageId);
                    }
                    finally
                    {
                        if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
                        {
                            await Flush();
                        }
                    }
                }
                else
                {
                    DoIndividualAckAsync(messageId);
                    if (_pendingIndividualAcks.Count >= MaxAckGroupSize)
                    {
                        await Flush();
                    }
                }
            }
        }


        private void DoIndividualAckAsync(MessageId messageId)
        {
            _pendingIndividualAcks.Add(messageId);
            _pendingIndividualBatchIndexAcks.Remove(messageId);
        }
        /// <summary>
		/// Flush all the pending acks and send them to the broker
		/// </summary>
		private async ValueTask Flush()
        {
            var cnx = await Cnx();
            try
            {
                //bool shouldFlush = false;
                if (_cumulativeAckFlushRequired)
                {
                    var lastAck = (MessageId)_lastCumulativeAck;
                    await NewMessageAckCommandAndWrite(cnx, _consumerId, lastAck.LedgerId, lastAck.EntryId, null, AckType.Cumulative, null, new Dictionary<string, long>(), false,  null);
                    _unAckedChunckedMessageIdSequenceMap.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> {_lastCumulativeAck}));
                    //shouldFlush = true;
                    _cumulativeAckFlushRequired = false;
                }

                // Flush all individual acks
                var entriesToAck = new List<(long ledger, long entry, long[] bitSet)>(_pendingIndividualAcks.Count + _pendingIndividualBatchIndexAcks.Count);
                //Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>> transactionEntriesToAck = new Dictionary<IActorRef, IList<(long ledger, long entry, BitSet bitSet)>>();
                if (_pendingIndividualAcks.Count > 0)
                {
                    var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
                    var protocolVersion = version.Version;
                    if (Commands.PeerSupportsMultiMessageAcknowledgment(protocolVersion))
                    {
                        // We can send 1 single protobuf command with all individual acks
                        while (_pendingIndividualAcks.Count > 0)
                        {
                            var msgId = _pendingIndividualAcks.Min;
                            _pendingIndividualAcks.Remove(msgId);

                            // if messageId is checked then all the chunked related to that msg also processed so, ack all of
                            // them
                            var result = await _unAckedChunckedMessageIdSequenceMap.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, new List<IMessageId> { msgId }));
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
                                _unAckedChunckedMessageIdSequenceMap.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, new List<IMessageId> { msgId }));
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
                        while (_pendingIndividualAcks.Count > 0)
                        {
                            var msgId = _pendingIndividualAcks.Min;
                            _pendingIndividualAcks.Remove(msgId);
                            await NewMessageAckCommandAndWrite(cnx, _consumerId, msgId.LedgerId, msgId.EntryId, null, AckType.Individual, null, new Dictionary<string, long>(), false, null);
                            //shouldFlush = true;
                        }
                    }
                }

                if (_pendingIndividualBatchIndexAcks.Count > 0)
                {
                    var acks = _pendingIndividualBatchIndexAcks;

                    foreach(var ack in acks)
                    {
                        entriesToAck.Add((ack.LedgerId, ack.EntryId, null));
                        _pendingIndividualBatchIndexAcks.Remove(ack);
                    }
                }
                if (entriesToAck.Count > 0)
                {
                    await NewMessageAckCommandAndWrite(cnx, _consumerId, 0L, 0L, null, AckType.Individual, null, null, true, entriesToAck);
                    //shouldFlush = true;
                }

            }
            catch (Exception ex)
            {
                Context.System.Log.Error(ex.ToString());
            }
        }
        private async ValueTask<IActorRef> Cnx()
        {
            if (_conx == null)
            {
                var response = await _handler.Ask<AskResponse>(GetCnx.Instance);
                if(response.Data != null)
                    _conx = response.ConvertTo<IActorRef>();
            }

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
        
        private async ValueTask NewImmediateAckAndFlush(long consumerId, MessageId msgId, long[] bitSet, AckType ackType, IDictionary<string, long> map, IActorRef cnx)
        {
            var response = await _unAckedChunckedMessageIdSequenceMap.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.GetRemoved, new List<IMessageId> { msgId})).ConfigureAwait(false);
            var chunkMsgIds = response.MessageIds;
            // cumulative ack chunk by the last messageId
            if (chunkMsgIds != null && ackType != AckType.Cumulative)
            {
                var version = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance);
                var protocolVersion = version.Version;
                if (Commands.PeerSupportsMultiMessageAcknowledgment(protocolVersion))
                {
                    var entriesToAck = new List<(long LedgerId, long EntryId, long[] Sets)>(chunkMsgIds.Length);
                    foreach (MessageId cMsgId in chunkMsgIds)
                    {
                        if (cMsgId != null && chunkMsgIds.Length > 1)
                        {
                            entriesToAck.Add((cMsgId.LedgerId, cMsgId.EntryId, null));
                        }
                    }
                    await NewMessageAckCommandAndWrite(cnx, _consumerId, 0L, 0L, null, ackType, null, null, true, entriesToAck);
                }
                else
                {
                    // if don't support multi message ack, it also support ack receipt, so we should not think about the
                    // ack receipt in this logic
                    foreach (MessageId cMsgId in chunkMsgIds)
                    {
                        await NewMessageAckCommandAndWrite(cnx, consumerId, cMsgId.LedgerId, cMsgId.EntryId, bitSet, ackType, null, map, true, null);
                    }
                }
            }
            else
            {
                await NewMessageAckCommandAndWrite(cnx, consumerId, msgId.LedgerId, msgId.EntryId, bitSet, ackType, null, map, true, null);
            }
        }
        private async ValueTask NewMessageAckCommandAndWrite(IActorRef cnx, long consumerId, long ledgerId, long entryId, long[] ackSet, AckType ackType, ValidationError? validationError, IDictionary<string, long> properties, bool flush, IList<(long LedgerId, long EntryId, long[] Sets)> entriesToAck)
        {
            if (await IsAckReceiptEnabled(cnx))
            {               
                var response = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                long requestId = response.Id;

                ReadOnlySequence<byte> cmd;
                if (entriesToAck == null)
                {
                    cmd = Commands.NewAck(consumerId, ledgerId, entryId, ackSet, ackType, null, properties, requestId);
                }
                else
                {
                    cmd = Commands.NewMultiMessageAck(consumerId, entriesToAck, requestId);
                }
                cnx.Tell(new Payload(cmd, requestId, "NewAckForReceipt"));
            }
            else
            {                
                ReadOnlySequence<byte> cmd;
                if (entriesToAck == null)
                {
                    cmd = Commands.NewAck(consumerId, ledgerId, entryId, ackSet, ackType, null, properties, -1);
                }
                else
                {
                    cmd = Commands.NewMultiMessageAck(consumerId, entriesToAck, -1);
                }
                cnx.Tell(new Payload(cmd, -1, "NewMessageAckCommandAndWrite"));
            }
        }
    }
    
    public sealed class FlushPending
    {
		public static FlushPending Instance = new FlushPending();
    }
    public sealed class AddListAcknowledgment
    {
        public IList<IMessageId> MessageIds { get; }
        public AckType AckType { get; }
        public IDictionary<string, long> Properties { get; }
        public AddListAcknowledgment(IList<IMessageId> messageIds, AckType ackType, IDictionary<string, long> properties)
        {
            MessageIds = messageIds;
            AckType = ackType;
            Properties = properties;
        }
    }
}