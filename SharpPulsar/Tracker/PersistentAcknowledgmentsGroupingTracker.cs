using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Util;
using PulsarAdmin.Models;
using SharpPulsar.Akka;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Extension;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utils;
using IAcknowledgmentsGroupingTracker = SharpPulsar.Tracker.Api.IAcknowledgmentsGroupingTracker;
using MessageId = SharpPulsar.Impl.MessageId;

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
	public class PersistentAcknowledgmentsGroupingTracker : IAcknowledgmentsGroupingTracker
	{

		/// <summary>
		/// When reaching the max group Size, an ack command is sent out immediately
		/// </summary>
		private const int MaxAckGroupSize = 1000;
		private readonly IActorRef _consumer;
		private readonly IActorRef _broker;
        private readonly long _consumerId;

		private readonly long _acknowledgementGroupTimeMicros;

		/// <summary>
		/// Latest cumulative ack sent to broker
		/// </summary>
		private IMessageId _lastCumulativeAck = MessageIdFields.Earliest;
		private  BitSet _lastCumulativeAckSet;
		private bool _cumulativeAckFlushRequired;


		/// <summary>
		/// This is a set of all the individual acks that the application has issued and that were not already sent to
		/// broker.
		/// </summary>
		private readonly Queue<IMessageId> _pendingIndividualAcks;
		private readonly ConcurrentDictionary<IMessageId, BitSet> _pendingIndividualBatchIndexAcks;

		private readonly ICancelable _scheduledTask;

		public PersistentAcknowledgmentsGroupingTracker(ActorSystem system, IActorRef broker, IActorRef consumer, long consumerid, ConsumerConfigurationData conf)
        {
            _broker = broker;
			_consumer = consumer;
            _consumerId = consumerid;
			_pendingIndividualAcks = new Queue<IMessageId>();
			_pendingIndividualBatchIndexAcks = new ConcurrentDictionary<IMessageId, BitSet>();
			_acknowledgementGroupTimeMicros = conf.AcknowledgementsGroupTimeMicros;
			if (_acknowledgementGroupTimeMicros > 0)
            {
                var interval = ConvertTimeUnits.ConvertMicrosecondsToMilliseconds(_acknowledgementGroupTimeMicros);
				_scheduledTask = system.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(interval), TimeSpan.FromMilliseconds(interval), Flush);
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

            return _pendingIndividualAcks.Contains(messageId);
        }

        public void AddAcknowledgment(IMessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
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

        public virtual void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        if (_acknowledgementGroupTimeMicros == 0 || properties.Count > 0)
	        {
		        DoImmediateBatchIndexAck(msgId, batchIndex, batchSize, ackType, properties);
	        }
	        else if (ackType == CommandAck.AckType.Cumulative)
	        {
		        var bitSet = BitSet.Create();
		        bitSet.Set(0, batchSize);
		        bitSet.Clear(0, batchIndex + 1);
		        DoCumulativeAck(msgId, bitSet);
	        }
	        else if (ackType == CommandAck.AckType.Individual)
            {
                var msgid = new MessageId(msgId.LedgerId, msgId.EntryId, msgId.PartitionIndex);
                var value = BitSet.Create();
                value.Set(0, batchSize + 1);
                value.Clear(batchIndex);
				var bitSet = _pendingIndividualBatchIndexAcks.AddOrUpdate(msgid, value, (s,v) => value);
		        bitSet.Set(batchIndex, false);
		        if (_pendingIndividualBatchIndexAcks.Count >= MaxAckGroupSize)
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
                    if (lastCumlativeAck.CompareTo(msgId) != 0)
                    {
						_lastCumulativeAck = msgId;
                        if (lastBitSet == null || !lastBitSet.Equals(bitSet))
                        {
                            _lastCumulativeAckSet = bitSet;
                            // Successfully updated the last cumulative ack. Next flush iteration will send this to broker.
                            _cumulativeAckFlushRequired = true;
                            return;
						}
                    }
		        }
		        else
		        {
			        // message id acknowledging an before the current last cumulative ack
			        return;
		        }
	        }
        }

        private bool DoImmediateAck(IMessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        NewAckCommand(_consumerId, msgId, null, ackType, null, properties, true);
	        return true;
        }

        private bool DoImmediateBatchIndexAck(BatchMessageId msgId, int batchIndex, int batchSize, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
	        var bitSet = BitSet.Create();
	        bitSet.Set(0, batchSize);
	        if (ackType == CommandAck.AckType.Cumulative)
	        {
		        bitSet.Clear(0, batchIndex + 1);
	        }
	        else
	        {
		        bitSet.Clear(batchIndex);
	        }

            var cmd = Commands.NewAck(_consumerId, msgId.LedgerId, msgId.EntryId, bitSet.ToLongArray(), ackType, null, properties);
			var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
            var payload = new Payload(cmd, requestid, "NewAck");
            _broker.Tell(payload, _consumer);
			return true;
        }

        /// <summary>
        /// Flush all the pending acks and send them to the broker
        /// </summary>
        public virtual void Flush()
        {
	        var shouldFlush = false;
	        if (_cumulativeAckFlushRequired)
	        {
		        NewAckCommand(_consumerId, _lastCumulativeAck, _lastCumulativeAckSet, CommandAck.AckType.Cumulative, null, new Dictionary<string, long>(), false);
		        shouldFlush = true;
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
					var chunkMsgIdsResponse = _consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, msgId)).GetAwaiter().GetResult();
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
                        _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, msgId));
                    }
                    else
                    {
                        entriesToAck.Add((msgId.LedgerId, msgId.EntryId, null));
                    }
                }
			}

	        if (!_pendingIndividualBatchIndexAcks.IsEmpty)
	        {
                using var iterator = _pendingIndividualBatchIndexAcks.SetOfKeyValuePairs().GetEnumerator();

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
                _broker.Tell(payload, _consumer);
		        shouldFlush = true;
	        }

        }

        public virtual void FlushAndClean()
        {
	        Flush();
	        _lastCumulativeAck = (MessageId)MessageIdFields.Earliest;
	        _pendingIndividualAcks.Clear();
        }

        public virtual void Close()
        {
	        Flush();
	        if (_scheduledTask != null && !_scheduledTask.IsCancellationRequested)
	        {
		        _scheduledTask.Cancel(true);
	        }
        }

        private void NewAckCommand(long consumerId, IMessageId msgid, BitSet lastCumulativeAckSet, CommandAck.AckType ackType, CommandAck.ValidationError? validationError, IDictionary<string, long> map, bool flush)
        {
            var chunkMsgIdsResponse = _consumer.Ask<UnAckedChunckedMessageIdSequenceMapCmdResponse>(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Get, msgid)).GetAwaiter().GetResult();
            var chunkMsgIds = chunkMsgIdsResponse.MessageIds;
            MessageId msgId;
            if (msgid is BatchMessageId id)
            {
                msgId = new MessageId(id.LedgerId, id.EntryId, id.PartitionIndex);
            }
            else msgId = (MessageId) msgid;

			if (chunkMsgIds != null)
	        {
				//SharpPulsar PeerSupportsMultiMessageAcknowledgment
				if (ackType != CommandAck.AckType.Cumulative)
		        {
			        IList<(long ledgerId, long entryId, BitSet sets)> entriesToAck = new List<(long ledgerId, long entryId, BitSet sets)>(chunkMsgIds.Length);
			        foreach (var cMsgId in chunkMsgIds)
			        {
				        if (cMsgId != null && chunkMsgIds.Length > 1)
				        {
					        entriesToAck.Add((cMsgId.LedgerId, cMsgId.EntryId, BitSet.Create()));
				        }
			        }
			        var cmd = Commands.NewMultiMessageAck(_consumerId, entriesToAck);
                    var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                    var payload = new Payload(cmd, requestid, "NewMultiMessageAck");
                    _broker.Tell(payload, _consumer);
				}
		        else
		        {
			        foreach (var cMsgId in chunkMsgIds)
			        {
				        var cmd = Commands.NewAck(consumerId, cMsgId.LedgerId, cMsgId.EntryId, lastCumulativeAckSet.ToLongArray(), ackType, validationError, map);
						var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
                        var payload = new Payload(cmd, requestid, "NewAck");
                        _broker.Tell(payload, _consumer);
					}
		        }
		        _consumer.Tell(new UnAckedChunckedMessageIdSequenceMapCmd(UnAckedCommand.Remove, msgId));
	        }
	        else
	        {
                var requestid = Interlocked.Increment(ref IdGenerators.RequestId);
				var cmd = Commands.NewAck(consumerId, msgId.LedgerId, msgId.EntryId, lastCumulativeAckSet.ToLongArray(), ackType, validationError, map);
				var payload = new Payload(cmd, requestid, "NewAck");
                _broker.Tell(payload, _consumer);
			}
        }
	}
}