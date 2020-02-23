using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Api;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
	/// Default batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into single batch message:
	/// [(k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)]
	/// </summary>
	public class BatchMessageContainerImpl<T> : AbstractBatchMessageContainer<T>
	{
		// sequence id for this batch which will be persisted as a single entry by broker
        private readonly MessageMetadata.Builder _messageMetadata = MessageMetadata.NewBuilder();
		private long _lowestSequenceId = -1L;
		private long _highestSequenceId = -1L;
		private IByteBuffer _batchedMessageMetadataAndPayload;
		private IList<MessageImpl<T>> _messages = new List<MessageImpl<T>>();
		protected internal SendCallback PreviousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		protected internal SendCallback FirstCallback;

        public override bool HasSameSchema(MessageImpl<T> msg)
        {
			if (NumMessagesInBatch == 0)
            {
                return true;
            }
            if (!_messageMetadata.HasSchemaVersion())
            {
                return msg.SchemaVersion == null;
            }
            return Equals(msg.SchemaVersion, _messageMetadata.GetSchemaVersion().ToByteArray());
		}

        public override bool Add(MessageImpl<T> msg, SendCallback callback)
		{

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] [{}] add message to batch, num messages in batch so far {}", TopicName, ProducerName, NumMessagesInBatch);
			}

			if (++NumMessagesInBatch == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				_lowestSequenceId = Commands.InitBatchMessageMetadata(MessageMetadata.NewBuilder());
				FirstCallback = callback;
				_batchedMessageMetadataAndPayload = PooledByteBufferAllocator.Default.Buffer(Math.Min(MaxBatchSize, ClientCnx.MaxMessageSize));
			}

            PreviousCallback?.AddCallback(msg, callback);
            PreviousCallback = callback;
			CurrentBatchSizeBytes += msg.DataBuffer.ReadableBytes;
			_messages.Add(msg);

			if (_lowestSequenceId == -1L)
			{
				_lowestSequenceId = msg.SequenceId;
				_messageMetadata.SetSequenceId(_lowestSequenceId);
			}
			_highestSequenceId = msg.SequenceId;
			Producer.LastSequenceIdPushed = msg.SequenceId;

			return BatchFull;
		}

		private IByteBuffer CompressedBatchMetadataAndPayload
		{
			get
			{
				var batchWriteIndex = _batchedMessageMetadataAndPayload.WriterIndex;
				var batchReadIndex = _batchedMessageMetadataAndPayload.ReaderIndex;
    
				for (int i = 0, n = _messages.Count; i < n; i++)
				{
					var msg = _messages[i];
					var msgBuilder = msg.MessageBuilder;
					msg.DataBuffer.MarkReaderIndex();
					try
					{
						_batchedMessageMetadataAndPayload = Commands.SerializeSingleMessageInBatchWithPayload(msgBuilder, msg.DataBuffer, _batchedMessageMetadataAndPayload);
					}
					catch (System.Exception th)
					{
						// serializing batch message can corrupt the index of message and batch-message. Reset the index so,
						// next iteration doesn't send corrupt message to broker.
						for (var j = 0; j <= i; j++)
						{
							var previousMsg = _messages[j];
							previousMsg.DataBuffer.ResetReaderIndex();
						}
						_batchedMessageMetadataAndPayload.SetWriterIndex(batchWriteIndex);
						_batchedMessageMetadataAndPayload.SetReaderIndex(batchReadIndex);
						throw new System.Exception(th.Message);
					}
				}
				// Recycle messages only once they serialized successfully in batch
				foreach (var msg in _messages)
				{
					msg.MessageBuilder.Recycle();
				}
				var uncompressedSize = _batchedMessageMetadataAndPayload.ReadableBytes;
				var compressedPayload = Compressor.Encode(_batchedMessageMetadataAndPayload);
				_batchedMessageMetadataAndPayload.Release();
				if (CompressionType != ICompressionType.None)
                {
                    var compression = Enum.GetValues(typeof(Common.Enum.CompressionType)).Cast<Common.Enum.CompressionType>()
                        .ToList()[(int)CompressionType];
					_messageMetadata.SetCompression(compression);
					_messageMetadata.SetUncompressedSize(uncompressedSize);
				}
    
				// Update the current max batch size using the uncompressed size, which is what we need in any case to
				// accumulate the batch content
				MaxBatchSize = Math.Max(MaxBatchSize, uncompressedSize);
				return compressedPayload;
			}
		}

		public override void Clear()
		{
			_messages = new List<MessageImpl<T>>();
			FirstCallback = null;
			PreviousCallback = null;
			_messageMetadata.Clear();
			NumMessagesInBatch = 0;
			CurrentBatchSizeBytes = 0;
			_lowestSequenceId = -1L;
			_highestSequenceId = -1L;
			_batchedMessageMetadataAndPayload = null;
		}

		public override bool Empty => _messages.Count == 0;

        public override void Discard(System.Exception ex)
		{
			try
            {
                // Need to protect ourselves from any exception being thrown in the future handler from the application
                FirstCallback?.SendComplete(ex);
            }
			catch (System.Exception T)
			{
				Log.LogWarning("[{}] [{}] Got exception while completing the callback for msg {}:", TopicName, ProducerName, _lowestSequenceId, T);
			}
			Clear();
		}

		public override bool MultiBatches => false;

        public new OpSendMsg<T> CreateOpSendMsg()
		{
			var encryptedPayload = Producer.EncryptMessage(_messageMetadata, CompressedBatchMetadataAndPayload);
			if (encryptedPayload.ReadableBytes > ClientCnx.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}
			_messageMetadata.SetNumMessagesInBatch(NumMessagesInBatch);
			_messageMetadata.SetHighestSequenceId(_highestSequenceId);
            var cmd = Producer.SendMessage(Producer.ProducerId, _messageMetadata.SequenceId(), _messageMetadata.HighestSequenceId, NumMessagesInBatch, _messageMetadata.Build(), encryptedPayload);

			var op = OpSendMsg<T>.Create(_messages, cmd, _messageMetadata.SequenceId(), _messageMetadata.HighestSequenceId, FirstCallback);

			op.NumMessagesInBatch = NumMessagesInBatch;
			op.BatchSizeByte = CurrentBatchSizeBytes;
			_lowestSequenceId = -1L;
			return op;
		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<BatchMessageContainerImpl<T>>();
	}

}