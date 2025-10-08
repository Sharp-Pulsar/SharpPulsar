using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using SharpPulsar.API;
using SharpPulsar.Common.Protocol.Proto;
using SharpPulsar.Extension;
using SharpPulsar.Internal.Producer;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Shared;
using SharpPulsar.Shared.Buf;
using SharpPulsar.Shared.Exceptions;
using ZstdNet;
using static SharpPulsar.Protocol.Schema.Commands;

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
namespace SharpPulsar.Batch
{
    /// <summary>
	/// Default batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into single batch message:
	/// [(k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)]
	/// </summary>
	internal class BatchMessageContainer<T> : AbstractBatchMessageContainer<T>
	{

		private MessageMetadata _messageMetadata = new MessageMetadata();
		// sequence id for this batch which will be persisted as a single entry by broker
		private long _lowestSequenceId = -1L;
		private long _highestSequenceId = -1L;
		private ByteBuf _batchedMessageMetadataAndPayload;
		private IList<Message<T>> _messages = new List<Message<T>>();
        private SendCallback<T> _previousCallback = null;
        // keep track of callbacks for individual messages being published in a batch
        private SendCallback<T> _firstCallback;
        private readonly ILoggingAdapter _log; 
        private const int SHRINK_COOLING_OFF_PERIOD = 10;
        private int consecutiveShrinkTime = 0;


        public BatchMessageContainer(ILoggingAdapter log)
        {
            _log = log;
        }
        public BatchMessageContainer(IActor producer) : this()
        {
            SetProducer(producer);
        }
        public BatchMessageContainer()
        {
        }
        public override bool Add(Message<T> msg, SendCallback<T> callback)
		{

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{TopicName}] [{ProducerName}] add message to batch, num messages in batch so far {NumMessagesInBatch}");
			}

			if (++NumMessagesInBatch == 1)
			{
                try
                {
                    // some properties are common amongst the different messages in the batch, hence we just pick it up from
                    // the first message
                    _messageMetadata.SequenceId = (ulong)msg.SequenceId;
                    _lowestSequenceId = Commands.InitBatchMessageMetadata(_messageMetadata);

                    _firstCallback = callback;
                    _batchedMessageMetadataAndPayload = new ByteBuf(Math.Min(MaxBatchSize, Container.MaxMessageSize));
                    if (msg.Metadata.OriginalMetadata.ShouldSerializeTxnidMostBits() && CurrentTxnidMostBits == -1)
                    {
                        CurrentTxnidMostBits = (long)msg.Metadata.OriginalMetadata.TxnidMostBits;
                    }
                    if (msg.Metadata.OriginalMetadata.ShouldSerializeTxnidLeastBits() && CurrentTxnidLeastBits == -1)
                    {
                        CurrentTxnidLeastBits = (long)msg.Metadata.OriginalMetadata.TxnidLeastBits;
                    }
                }
                catch(Exception ex)
                {
                    _log.Error($"construct first message failed, exception is {ex}");
                    Discard(new PulsarClientException(ex));
                    return false;
                }
			}
            if (_previousCallback != null)
            {
                _previousCallback.AddCallback(msg, callback);
            }
            _previousCallback = callback;
			CurrentBatchSize += msg.Data.Length;
			_messages.Add(msg);
            TryUpdateTimestamp();

            if (_lowestSequenceId == -1L)
			{
				_lowestSequenceId = msg.SequenceId;
				_messageMetadata.SequenceId = (ulong)_lowestSequenceId;
			}
			_highestSequenceId = msg.SequenceId;
            //callback(msg.SequenceId, null);

			return BatchFull;
		}

        protected internal virtual ByteBuf CompressedBatchMetadataAndPayload()
        {
            return CompressedBatchMetadataAndPayload(true);
        }

        protected internal virtual ByteBuf CompressedBatchMetadataAndPayload(bool clientOperation)
        {
            int batchWriteIndex = _batchedMessageMetadataAndPayload.WriterIndex;
            int batchReadIndex = _batchedMessageMetadataAndPayload.ReaderIndex;

            for (int i = 0, n = _messages.Count; i < n; i++)
            {
                var msg = _messages[i];
                msg.DataBuffer().MarkReaderIndex();
                try
                {
                    if (n == 1)
                    {
                        _batchedMessageMetadataAndPayload.WriteBytes(msg.DataBuffer());
                    }
                    else
                    {
                        _batchedMessageMetadataAndPayload = Commands.SerializeSingleMessageInBatchWithPayload(msg.MessageBuilder(), msg.DataBuffer(), _batchedMessageMetadataAndPayload);
                    }
                }
                catch (Exception th)
                {
                    // serializing batch message can corrupt the index of message and batch-message. Reset the index so,
                    // next iteration doesn't send corrupt message to broker.
                    _batchedMessageMetadataAndPayload.WriterIndex = batchWriteIndex;
                    _batchedMessageMetadataAndPayload.ReaderIndex = batchReadIndex;
                    throw;
                }
                finally
                {
                    msg.DataBuffer().Reset();
                }
            }

            int uncompressedSize = _batchedMessageMetadataAndPayload.ReadableBytes;
            ByteBuf compressedPayload;
            if (clientOperation && producer != null)
            {
                if (CompressionType != CompressionType.NONE && uncompressedSize > producer.conf.getCompressMinMsgBodySize())
                {
                    compressedPayload = producer.applyCompression(_batchedMessageMetadataAndPayload);
                    _messageMetadata.Compression = CompressionType;
                    _messageMetadata.UncompressedSize = uncompressedSize;
                }
                else
                {
                    compressedPayload = _batchedMessageMetadataAndPayload;
                }
            }
            else
            {
                compressedPayload = Compressor.Encode(_batchedMessageMetadataAndPayload);
                _batchedMessageMetadataAndPayload.Release();
                if (CompressionType != CompressionType.None)
                {
                    _messageMetadata.Compression = (CompressionType);
                    _messageMetadata.UncompressedSize = (UncompressedSize);
                }
            }

            // Update the current max batch size using the uncompressed size, which is what we need in any case to
            // accumulate the batch content
            UpdateMaxBatchSize(uncompressedSize);
            maxMessagesNum = Math.Max(maxMessagesNum, NumMessagesInBatch);
            return compressedPayload;
        }

        internal virtual void UpdateMaxBatchSize(int uncompressedSize)
        {
            if (uncompressedSize > MaxBatchSize)
            {
                MaxBatchSize = uncompressedSize;
                consecutiveShrinkTime = 0;
            }
            else
            {
                int shrank = MaxBatchSize - (MaxBatchSize >> 2);
                if (uncompressedSize <= shrank)
                {
                    if (consecutiveShrinkTime <= SHRINK_COOLING_OFF_PERIOD)
                    {
                        consecutiveShrinkTime++;
                    }
                    else
                    {
                        MaxBatchSize = shrank;
                        consecutiveShrinkTime = 0;
                    }
                }
                else
                {
                    consecutiveShrinkTime = 0;
                }
            }
        }


        public override void Clear()
		{
			_messages = new List<Message<T>>();
			_firstCallback = null;
			_previousCallback = null;
			_messageMetadata = new MessageMetadata();
			NumMessagesInBatch = 0;
			CurrentBatchSize = 0;
			_lowestSequenceId = -1L;
			_highestSequenceId = -1L;
			_batchedMessageMetadataAndPayload = null;
			CurrentTxnidMostBits = -1L;
			CurrentTxnidLeastBits = -1L;
		}
        public virtual void ResetPayloadAfterFailedPublishing()
        {
            if (_batchedMessageMetadataAndPayload != null)
            {
                _batchedMessageMetadataAndPayload.ReaderIndex = 0;
                _batchedMessageMetadataAndPayload.WriterIndex = 0;
            }
        }

        protected internal virtual void UpdateAndReserveBatchAllocatedSize(int updatedSizeBytes)
        {
            int delta = updatedSizeBytes - _batchAllocatedSizeBytes;
            batchAllocatedSizeBytes = updatedSizeBytes;
            if (producer != null)
            {
                if (delta > 0)
                {
                    producer.client.getMemoryLimitController().forceReserveMemory(delta);
                }
                else if (delta < 0)
                {
                    producer.client.getMemoryLimitController().releaseMemory(-delta);
                }
            }
        }

        public override bool Empty => _messages.Count == 0;

        public override void Discard(Exception ex)
		{
			try
            {
                // Need to protect ourselves from any exception being thrown in the future handler from the application
                _firstCallback.SendComplete(ex);
            }
			catch (Exception t)
			{
				_log.Warning($"[{TopicName}] [{ProducerName}] Got exception while completing the callback for msg {_lowestSequenceId}: {t}");
			}
			Clear();
		}

		public override bool MultiBatches => false;

		public override ProducerActor<T>.OpSendMsg<T> CreateOpSendMsg()
		{
			var encryptedPayload = CompressedBatchMetadataAndPayload();
            if (Container.Configuration.EncryptionEnabled && Container.Crypto != null)
            {
                try
                {
                    encryptedPayload = Container.Crypto.Encrypt(Container.Configuration.EncryptionKeys, Container.Configuration.CryptoKeyReader, _messageMetadata, encryptedPayload);
                }
                catch (PulsarClientException e)
                {
                    // Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
                    if (Container.Configuration.CryptoFailureAction != ProducerCryptoFailureAction.Send) 
                        throw;
                    _log.Warning($"[{TopicName}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
                    encryptedPayload = CompressedBatchMetadataAndPayload();
                }
            }
			if (encryptedPayload.Capacity > Container.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message Size is bigger than " + Container.MaxMessageSize + " bytes"));
				return null;
			}
			_messageMetadata.NumMessagesInBatch = NumMessagesInBatch;
			_messageMetadata.SequenceId = (ulong)_messages[0].SequenceId;
			_messageMetadata.HighestSequenceId = (ulong)_highestSequenceId;
			_messageMetadata.PartitionKey = _messages[0].Key;
			_messageMetadata.OrderingKey = _messages[0].OrderingKey;
			_messageMetadata.ProducerName = Container.ProducerName;
			_messageMetadata.PublishTime = (ulong)DateTimeHelper.CurrentUnixTimeMillis();
			if (CurrentTxnidMostBits != -1)
			{
				_messageMetadata.TxnidMostBits = (ulong)CurrentTxnidMostBits;
			}
			if (CurrentTxnidLeastBits != -1)
			{
				_messageMetadata.TxnidLeastBits = (ulong)CurrentTxnidLeastBits;
            }
            var cmd = SendMessage(Container.ProducerId, _messages[0].SequenceId, NumMessagesInBatch, _messages[0].MessageId, _messageMetadata, encryptedPayload);

			var op = ProducerActor<T>.OpSendMsg<T>.Create(_messages, cmd, _messages[0].SequenceId, _highestSequenceId, _firstCallback);

			op.NumMessagesInBatch = NumMessagesInBatch;
			op.BatchSizeByte = CurrentBatchSize;
			_lowestSequenceId = -1L;
			return op;
		}
        private ReadOnlySequence<byte> SendMessage(long producerId, long sequenceId, int numMessages, IMessageId messageId, MessageMetadata msgMetadata, byte[] compressedPayload)
        {
            if (messageId is MessageIdAdv)
            {
                return Commands.NewSend(producerId, sequenceId, numMessages, ChecksumType.Crc32C, ((MessageIdAdv)messageId).LedgerId, ((MessageIdAdv)messageId).EntryId, msgMetadata, compressedPayload);
            }
            else
            {
                return Commands.NewSend(producerId, sequenceId, numMessages, ChecksumType.Crc32C, -1, -1, msgMetadata, compressedPayload);
            }
        }
        public override bool HasSameSchema(Message<T> msg)
		{
			if (NumMessagesInBatch == 0)
			{
				return true;
			}
			if (!_messageMetadata.ShouldSerializeSchemaVersion())
			{
				return msg.SchemaVersion == null;
			}
			return Equals(msg.SchemaVersion, _messageMetadata.SchemaVersion);
		}

	}

}