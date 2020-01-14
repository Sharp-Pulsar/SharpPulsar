using System;
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
namespace org.apache.pulsar.client.impl
{
	using Lists = com.google.common.collect.Lists;

	using ByteBuf = io.netty.buffer.ByteBuf;


	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using OpSendMsg = org.apache.pulsar.client.impl.ProducerImpl.OpSendMsg;
	using PulsarByteBufAllocator = org.apache.pulsar.common.allocator.PulsarByteBufAllocator;

	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using ByteBufPair = org.apache.pulsar.common.protocol.ByteBufPair;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	/// <summary>
	/// Default batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into single batch message:
	/// [(k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)]
	/// </summary>
	internal class BatchMessageContainerImpl : AbstractBatchMessageContainer
	{

		private PulsarApi.MessageMetadata.Builder messageMetadata = PulsarApi.MessageMetadata.newBuilder();
		// sequence id for this batch which will be persisted as a single entry by broker
		private long lowestSequenceId = -1L;
		private long highestSequenceId = -1L;
		private ByteBuf batchedMessageMetadataAndPayload;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<MessageImpl<?>> messages = com.google.common.collect.Lists.newArrayList();
		private IList<MessageImpl<object>> messages = Lists.newArrayList();
		protected internal SendCallback previousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		protected internal SendCallback firstCallback;

		public override bool add<T1>(MessageImpl<T1> msg, SendCallback callback)
		{

			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] add message to batch, num messages in batch so far {}", topicName, producerName, numMessagesInBatch);
			}

			if (++numMessagesInBatch == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				lowestSequenceId = Commands.initBatchMessageMetadata(messageMetadata, msg.MessageBuilder);
				this.firstCallback = callback;
				batchedMessageMetadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(Math.Min(maxBatchSize, ClientCnx.MaxMessageSize));
			}

			if (previousCallback != null)
			{
				previousCallback.addCallback(msg, callback);
			}
			previousCallback = callback;
			currentBatchSizeBytes += msg.DataBuffer.readableBytes();
			messages.Add(msg);

			if (lowestSequenceId == -1L)
			{
				lowestSequenceId = msg.SequenceId;
				messageMetadata.SequenceId = lowestSequenceId;
			}
			highestSequenceId = msg.SequenceId;
			producer.lastSequenceIdPushed = msg.SequenceId;

			return BatchFull;
		}

		private ByteBuf CompressedBatchMetadataAndPayload
		{
			get
			{
				int batchWriteIndex = batchedMessageMetadataAndPayload.writerIndex();
				int batchReadIndex = batchedMessageMetadataAndPayload.readerIndex();
    
				for (int i = 0, n = messages.Count; i < n; i++)
				{
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: MessageImpl<?> msg = messages.get(i);
					MessageImpl<object> msg = messages[i];
					PulsarApi.MessageMetadata.Builder msgBuilder = msg.MessageBuilder;
					msg.DataBuffer.markReaderIndex();
					try
					{
						batchedMessageMetadataAndPayload = Commands.serializeSingleMessageInBatchWithPayload(msgBuilder, msg.DataBuffer, batchedMessageMetadataAndPayload);
					}
					catch (Exception th)
					{
						// serializing batch message can corrupt the index of message and batch-message. Reset the index so,
						// next iteration doesn't send corrupt message to broker.
						for (int j = 0; j <= i; j++)
						{
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: MessageImpl<?> previousMsg = messages.get(j);
							MessageImpl<object> previousMsg = messages[j];
							previousMsg.DataBuffer.resetReaderIndex();
						}
						batchedMessageMetadataAndPayload.writerIndex(batchWriteIndex);
						batchedMessageMetadataAndPayload.readerIndex(batchReadIndex);
						throw new Exception(th);
					}
				}
				// Recycle messages only once they serialized successfully in batch
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (MessageImpl<?> msg : messages)
				foreach (MessageImpl<object> msg in messages)
				{
					msg.MessageBuilder.recycle();
				}
				int uncompressedSize = batchedMessageMetadataAndPayload.readableBytes();
				ByteBuf compressedPayload = compressor.encode(batchedMessageMetadataAndPayload);
				batchedMessageMetadataAndPayload.release();
				if (compressionType != PulsarApi.CompressionType.NONE)
				{
					messageMetadata.Compression = compressionType;
					messageMetadata.UncompressedSize = uncompressedSize;
				}
    
				// Update the current max batch size using the uncompressed size, which is what we need in any case to
				// accumulate the batch content
				maxBatchSize = Math.Max(maxBatchSize, uncompressedSize);
				return compressedPayload;
			}
		}

		public override void clear()
		{
			messages = Lists.newArrayList();
			firstCallback = null;
			previousCallback = null;
			messageMetadata.clear();
			numMessagesInBatch = 0;
			currentBatchSizeBytes = 0;
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

		public override void discard(Exception ex)
		{
			try
			{
				// Need to protect ourselves from any exception being thrown in the future handler from the application
				if (firstCallback != null)
				{
					firstCallback.sendComplete(ex);
				}
			}
			catch (Exception t)
			{
				log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", topicName, producerName, lowestSequenceId, t);
			}
			clear();
		}

		public override bool MultiBatches
		{
			get
			{
				return false;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.impl.ProducerImpl.OpSendMsg createOpSendMsg() throws java.io.IOException
		public override OpSendMsg createOpSendMsg()
		{
			ByteBuf encryptedPayload = producer.encryptMessage(messageMetadata, CompressedBatchMetadataAndPayload);
			if (encryptedPayload.readableBytes() > ClientCnx.MaxMessageSize)
			{
				discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}
			messageMetadata.NumMessagesInBatch = numMessagesInBatch;
			messageMetadata.HighestSequenceId = highestSequenceId;
			ByteBufPair cmd = producer.sendMessage(producer.producerId, messageMetadata.SequenceId, messageMetadata.HighestSequenceId, numMessagesInBatch, messageMetadata.build(), encryptedPayload);

			OpSendMsg op = OpSendMsg.create(messages, cmd, messageMetadata.SequenceId, messageMetadata.HighestSequenceId, firstCallback);

			op.NumMessagesInBatch = numMessagesInBatch;
			op.BatchSizeByte = currentBatchSizeBytes;
			lowestSequenceId = -1L;
			return op;
		}

		public override bool hasSameSchema<T1>(MessageImpl<T1> msg)
		{
			if (numMessagesInBatch == 0)
			{
				return true;
			}
			if (!messageMetadata.hasSchemaVersion())
			{
				return msg.SchemaVersion == null;
			}
			return Arrays.equals(msg.SchemaVersion, messageMetadata.SchemaVersion.toByteArray());
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BatchMessageContainerImpl));
	}

}