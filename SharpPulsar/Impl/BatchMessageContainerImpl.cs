﻿using System;
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
namespace SharpPulsar.Impl
{
	using Lists = com.google.common.collect.Lists;

	using ByteBuf = io.netty.buffer.ByteBuf;


	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using OpSendMsg = SharpPulsar.Impl.ProducerImpl.OpSendMsg;
	using PulsarByteBufAllocator = Org.Apache.Pulsar.Common.Allocator.PulsarByteBufAllocator;

	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using ByteBufPair = Org.Apache.Pulsar.Common.Protocol.ByteBufPair;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
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
	public class BatchMessageContainerImpl : AbstractBatchMessageContainer
	{

		private PulsarApi.MessageMetadata.Builder messageMetadata = PulsarApi.MessageMetadata.newBuilder();
		// sequence id for this batch which will be persisted as a single entry by broker
		private long lowestSequenceId = -1L;
		private long highestSequenceId = -1L;
		private ByteBuf batchedMessageMetadataAndPayload;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<MessageImpl<?>> messages = com.google.common.collect.Lists.newArrayList();
		private IList<MessageImpl<object>> messages = Lists.newArrayList();
		protected internal SendCallback PreviousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		protected internal SendCallback FirstCallback;

		public override bool Add<T1>(MessageImpl<T1> Msg, SendCallback Callback)
		{

			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] add message to batch, num messages in batch so far {}", TopicName, ProducerName, NumMessagesInBatchConflict);
			}

			if (++NumMessagesInBatchConflict == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				lowestSequenceId = Commands.initBatchMessageMetadata(messageMetadata, Msg.MessageBuilder);
				this.FirstCallback = Callback;
				batchedMessageMetadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(Math.Min(MaxBatchSize, ClientCnx.MaxMessageSize));
			}

			if (PreviousCallback != null)
			{
				PreviousCallback.addCallback(Msg, Callback);
			}
			PreviousCallback = Callback;
			CurrentBatchSizeBytes += Msg.DataBuffer.readableBytes();
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

		private ByteBuf CompressedBatchMetadataAndPayload
		{
			get
			{
				int BatchWriteIndex = batchedMessageMetadataAndPayload.writerIndex();
				int BatchReadIndex = batchedMessageMetadataAndPayload.readerIndex();
    
				for (int I = 0, n = messages.Count; I < n; I++)
				{
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: MessageImpl<?> msg = messages.get(i);
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
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: MessageImpl<?> previousMsg = messages.get(j);
							MessageImpl<object> PreviousMsg = messages[J];
							PreviousMsg.DataBuffer.resetReaderIndex();
						}
						batchedMessageMetadataAndPayload.writerIndex(BatchWriteIndex);
						batchedMessageMetadataAndPayload.readerIndex(BatchReadIndex);
						throw new Exception(Th);
					}
				}
				// Recycle messages only once they serialized successfully in batch
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (MessageImpl<?> msg : messages)
				foreach (MessageImpl<object> Msg in messages)
				{
					Msg.MessageBuilder.recycle();
				}
				int UncompressedSize = batchedMessageMetadataAndPayload.readableBytes();
				ByteBuf CompressedPayload = Compressor.encode(batchedMessageMetadataAndPayload);
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

		public override void Discard(Exception Ex)
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.impl.ProducerImpl.OpSendMsg createOpSendMsg() throws java.io.IOException
		public override OpSendMsg CreateOpSendMsg()
		{
			ByteBuf EncryptedPayload = ProducerConflict.encryptMessage(messageMetadata, CompressedBatchMetadataAndPayload);
			if (EncryptedPayload.readableBytes() > ClientCnx.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}
			messageMetadata.NumMessagesInBatch = NumMessagesInBatchConflict;
			messageMetadata.HighestSequenceId = highestSequenceId;
			ByteBufPair Cmd = ProducerConflict.sendMessage(ProducerConflict.producerId, messageMetadata.SequenceId, messageMetadata.HighestSequenceId, NumMessagesInBatchConflict, messageMetadata.Build(), EncryptedPayload);

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

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BatchMessageContainerImpl));
	}

}