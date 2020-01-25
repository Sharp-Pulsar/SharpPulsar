using DotNetty.Buffers;
using Microsoft.Extensions.Logging;
using SharpPulsar.Command.Builder;
using SharpPulsar.Common;
using SharpPulsar.Protocol;
using System;
using System.Collections.Generic;
using static SharpPulsar.Impl.ProducerImpl<object>;

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
	public class BatchMessageContainerImpl : AbstractBatchMessageContainer
	{
		// sequence id for this batch which will be persisted as a single entry by broker
		private long lowestSequenceId = -1L;
		private long highestSequenceId = -1L;
		private IByteBuffer batchedMessageMetadataAndPayload;
		private IList<MessageImpl<object>> messages = new List<MessageImpl<object>>();
		protected internal SendCallback PreviousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		protected internal SendCallback FirstCallback;

		public override bool Add(MessageImpl<object> msg, SendCallback callback)
		{

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("[{}] [{}] add message to batch, num messages in batch so far {}", TopicName, ProducerName, NumMessagesInBatchConflict);
			}

			if (++NumMessagesInBatchConflict == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				lowestSequenceId = Commands.InitBatchMessageMetadata(new MessageMetadataBuilder(), msg.MessageBuilder);
				this.FirstCallback = callback;
				batchedMessageMetadataAndPayload = PulsarByteBufAllocator.DEFAULT.Buffer(Math.Min(MaxBatchSize, ClientCnx.MaxMessageSize));
			}

			if (PreviousCallback != null)
			{
				PreviousCallback.AddCallback(Msg, callback);
			}
			PreviousCallback = callback;
			CurrentBatchSizeBytes += Msg.DataBuffer.ReadableBytes;
			messages.Add(Msg);

			if (lowestSequenceId == -1L)
			{
				lowestSequenceId = Msg.SequenceId;
				messageMetadata.SequenceId = lowestSequenceId;
			}
			highestSequenceId = Msg.SequenceId;
			ProducerConflict.lastSequenceIdPushed = Msg.SequenceId;

			return BatchFull;
		}

		private IByteBuffer CompressedBatchMetadataAndPayload
		{
			get
			{
				int BatchWriteIndex = batchedMessageMetadataAndPayload.writerIndex();
				int BatchReadIndex = batchedMessageMetadataAndPayload.readerIndex();
    
				for (int I = 0, n = messages.Count; I < n; I++)
				{
					MessageImpl<object> Msg = messages[I];
					PulsarApi.MessageMetadata.Builder MsgBuilder = Msg.MessageBuilder;
					Msg.DataBuffer.markReaderIndex();
					try
					{
						batchedMessageMetadataAndPayload = Commands.serializeSingleMessageInBatchWithPayload(MsgBuilder, Msg.DataBuffer, batchedMessageMetadataAndPayload);
					}
					catch (Exception Th)
					{
						// serializing batch message can corrupt the index of message and batch-message. Reset the index so,
						// next iteration doesn't send corrupt message to broker.
						for (int J = 0; J <= i; J++)
						{
							MessageImpl<object> PreviousMsg = messages[J];
							PreviousMsg.DataBuffer.resetReaderIndex();
						}
						batchedMessageMetadataAndPayload.writerIndex(BatchWriteIndex);
						batchedMessageMetadataAndPayload.readerIndex(BatchReadIndex);
						throw new Exception(Th);
					}
				}
				// Recycle messages only once they serialized successfully in batch
				foreach (MessageImpl<object> Msg in messages)
				{
					Msg.MessageBuilder.recycle();
				}
				int UncompressedSize = batchedMessageMetadataAndPayload.readableBytes();
				IByteBuffer CompressedPayload = Compressor.encode(batchedMessageMetadataAndPayload);
				batchedMessageMetadataAndPayload.release();
				if (CompressionType != PulsarApi.CompressionType.NONE)
				{
					messageMetadata.Compression = CompressionType;
					messageMetadata.UncompressedSize = UncompressedSize;
				}
    
				// Update the current max batch size using the uncompressed size, which is what we need in any case to
				// accumulate the batch content
				MaxBatchSize = Math.Max(MaxBatchSize, UncompressedSize);
				return CompressedPayload;
			}
		}

		public override void Clear()
		{
			messages = Lists.newArrayList();
			FirstCallback = null;
			PreviousCallback = null;
			messageMetadata.Clear();
			NumMessagesInBatchConflict = 0;
			CurrentBatchSizeBytes = 0;
			lowestSequenceId = -1L;
			highestSequenceId = -1L;
			batchedMessageMetadataAndPayload = null;
		}

		public override bool Empty
		{
			get
			{
				return messages.Count == 0;
			}
		}

		public override void Discard(System.Exception ex)
		{
			try
			{
				// Need to protect ourselves from any exception being thrown in the future handler from the application
				if (FirstCallback != null)
				{
					FirstCallback.sendComplete(Ex);
				}
			}
			catch (Exception T)
			{
				log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", TopicName, ProducerName, lowestSequenceId, T);
			}
			Clear();
		}

		public override bool MultiBatches
		{
			get
			{
				return false;
			}
		}
		public OpSendMsg CreateOpSendMsg()
		{
			IByteBuffer EncryptedPayload = ProducerConflict.encryptMessage(messageMetadata, CompressedBatchMetadataAndPayload);
			if (EncryptedPayload.readableBytes() > ClientCnx.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}
			messageMetadata.NumMessagesInBatch = NumMessagesInBatchConflict;
			messageMetadata.HighestSequenceId = highestSequenceId;
			IByteBufferPair Cmd = ProducerConflict.sendMessage(ProducerConflict.producerId, messageMetadata.SequenceId, messageMetadata.HighestSequenceId, NumMessagesInBatchConflict, messageMetadata.Build(), EncryptedPayload);

			OpSendMsg Op = OpSendMsg.create(messages, Cmd, messageMetadata.SequenceId, messageMetadata.HighestSequenceId, FirstCallback);

			Op.NumMessagesInBatch = NumMessagesInBatchConflict;
			Op.BatchSizeByte = CurrentBatchSizeBytes;
			lowestSequenceId = -1L;
			return Op;
		}

		public override bool HasSameSchema<T1>(MessageImpl<T1> Msg)
		{
			if (NumMessagesInBatchConflict == 0)
			{
				return true;
			}
			if (!messageMetadata.HasSchemaVersion())
			{
				return Msg.SchemaVersion == null;
			}
			return Arrays.equals(Msg.SchemaVersion, messageMetadata.SchemaVersion.toByteArray());
		}
		private static readonly ILogger log = new LoggerFactory().CreateLogger<BatchMessageContainerImpl>();
	}

}