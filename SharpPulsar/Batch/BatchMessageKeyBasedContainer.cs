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
namespace SharpPulsar.Batch
{
    using ByteBuf = io.netty.buffer.ByteBuf;
    using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
    using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using CompressionCodec = org.apache.pulsar.common.compression.CompressionCodec;
	using ByteBufPair = org.apache.pulsar.common.protocol.ByteBufPair;
    using Logger = org.slf4j.Logger;


    /// <summary>
	/// Key based batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into multiple batch messages:
	/// [(k1, v1), (k1, v2), (k1, v3)], [(k2, v1), (k2, v2), (k2, v3)], [(k3, v1), (k3, v2), (k3, v3)]
	/// </summary>
	public class BatchMessageKeyBasedContainer : AbstractBatchMessageContainer
	{

		private IDictionary<string, KeyedBatch> batches = new Dictionary<string, KeyedBatch>();

		public override bool add<T1>(MessageImpl<T1> msg, SendCallback callback)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] add message to batch, num messages in batch so far is {}", topicName, producerName, numMessagesInBatch);
			}
			numMessagesInBatch++;
			currentBatchSizeBytes += msg.DataBuffer.readableBytes();
			string key = getKey(msg);
			KeyedBatch part = batches[key];
			if (part == null)
			{
				part = new KeyedBatch();
				part.addMsg(msg, callback);
				part.compressionType = compressionType;
				part.compressor = compressor;
				part.maxBatchSize = maxBatchSize;
				part.topicName = topicName;
				part.producerName = producerName;
				if (!batches.ContainsKey(key)) batches.Add(key, part);
			}
			else
			{
				part.addMsg(msg, callback);
			}
			return BatchFull;
		}

		public override void clear()
		{
			numMessagesInBatch = 0;
			currentBatchSizeBytes = 0;
			batches = new Dictionary<string, KeyedBatch>();
		}

		public override bool Empty
		{
			get
			{
				return batches.Count == 0;
			}
		}

		public override void discard(Exception ex)
		{
			try
			{
				// Need to protect ourselves from any exception being thrown in the future handler from the application
				batches.forEach((k, v) => v.firstCallback.sendComplete(ex));
			}
			catch (Exception t)
			{
				log.warn("[{}] [{}] Got exception while completing the callback", topicName, producerName, t);
			}
			batches.forEach((k, v) => ReferenceCountUtil.safeRelease(v.batchedMessageMetadataAndPayload));
			clear();
		}

		public override bool MultiBatches
		{
			get
			{
				return true;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ProducerImpl.OpSendMsg createOpSendMsg(KeyedBatch keyedBatch) throws java.io.IOException
		private ProducerImpl.OpSendMsg createOpSendMsg(KeyedBatch keyedBatch)
		{
			ByteBuf encryptedPayload = producer.encryptMessage(keyedBatch.messageMetadata, keyedBatch.CompressedBatchMetadataAndPayload);
			if (encryptedPayload.readableBytes() > ClientCnx.MaxMessageSize)
			{
				keyedBatch.discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessagesInBatch = keyedBatch.messages.size();
			int numMessagesInBatch = keyedBatch.messages.Count;
			long currentBatchSizeBytes = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (MessageImpl<?> message : keyedBatch.messages)
			foreach (MessageImpl<object> message in keyedBatch.messages)
			{
				currentBatchSizeBytes += message.DataBuffer.readableBytes();
			}
			keyedBatch.messageMetadata.NumMessagesInBatch = numMessagesInBatch;
			ByteBufPair cmd = producer.sendMessage(producer.producerId, keyedBatch.sequenceId, numMessagesInBatch, keyedBatch.messageMetadata.build(), encryptedPayload);

			ProducerImpl.OpSendMsg op = ProducerImpl.OpSendMsg.create(keyedBatch.messages, cmd, keyedBatch.sequenceId, keyedBatch.firstCallback);

			op.NumMessagesInBatch = numMessagesInBatch;
			op.BatchSizeByte = currentBatchSizeBytes;
			return op;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<ProducerImpl.OpSendMsg> createOpSendMsgs() throws java.io.IOException
		public override IList<ProducerImpl.OpSendMsg> createOpSendMsgs()
		{
			IList<ProducerImpl.OpSendMsg> result = new List<ProducerImpl.OpSendMsg>();
			IList<KeyedBatch> list = new List<KeyedBatch>(batches.Values);
			list.sort(((o1, o2) => ComparisonChain.start().compare(o1.sequenceId, o2.sequenceId).result()));
			foreach (KeyedBatch keyedBatch in list)
			{
				ProducerImpl.OpSendMsg op = createOpSendMsg(keyedBatch);
				if (op != null)
				{
					result.Add(op);
				}
			}
			return result;
		}

		public override bool hasSameSchema<T1>(MessageImpl<T1> msg)
		{
			string key = getKey(msg);
			KeyedBatch part = batches[key];
			if (part == null || part.messages.Count == 0)
			{
				return true;
			}
			if (!part.messageMetadata.hasSchemaVersion())
			{
				return msg.SchemaVersion == null;
			}
			return Arrays.equals(msg.SchemaVersion, part.messageMetadata.SchemaVersion.toByteArray());
		}

		private string getKey<T1>(MessageImpl<T1> msg)
		{
			if (msg.hasOrderingKey())
			{
				return Base64.Encoder.encodeToString(msg.OrderingKey);
			}
			return msg.Key;
		}

		public class KeyedBatch
		{
			internal PulsarApi.MessageMetadata.Builder messageMetadata = PulsarApi.MessageMetadata.newBuilder();
			// sequence id for this batch which will be persisted as a single entry by broker
			internal long sequenceId = -1;
			internal ByteBuf batchedMessageMetadataAndPayload;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<MessageImpl<?>> messages = com.google.common.collect.Lists.newArrayList();
			internal IList<MessageImpl<object>> messages = Lists.newArrayList();
			internal SendCallback previousCallback = null;
			internal PulsarApi.CompressionType compressionType;
			internal CompressionCodec compressor;
			internal int maxBatchSize;
			internal string topicName;
			internal string producerName;

			// keep track of callbacks for individual messages being published in a batch
			internal SendCallback firstCallback;

			public virtual ByteBuf CompressedBatchMetadataAndPayload
			{
				get
				{
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (MessageImpl<?> msg : messages)
					foreach (MessageImpl<object> msg in messages)
					{
						PulsarApi.MessageMetadata.Builder msgBuilder = msg.MessageBuilder;
						batchedMessageMetadataAndPayload = Commands.serializeSingleMessageInBatchWithPayload(msgBuilder, msg.DataBuffer, batchedMessageMetadataAndPayload);
						msgBuilder.recycle();
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

			public virtual void addMsg<T1>(MessageImpl<T1> msg, SendCallback callback)
			{
				if (messages.Count == 0)
				{
					sequenceId = Commands.initBatchMessageMetadata(messageMetadata, msg.MessageBuilder);
					if (msg.hasKey())
					{
						messageMetadata.setPartitionKey(msg.Key);
						if (msg.hasBase64EncodedKey())
						{
							messageMetadata.PartitionKeyB64Encoded = true;
						}
					}
					if (msg.hasOrderingKey())
					{
						messageMetadata.OrderingKey = ByteString.copyFrom(msg.OrderingKey);
					}
					batchedMessageMetadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(Math.Min(maxBatchSize, ClientCnx.MaxMessageSize));
					firstCallback = callback;
				}
				if (previousCallback != null)
				{
					previousCallback.addCallback(msg, callback);
				}
				previousCallback = callback;
				messages.Add(msg);
			}

			public virtual void discard(Exception ex)
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
					log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", topicName, producerName, sequenceId, t);
				}
				clear();
			}

			public virtual void clear()
			{
				messages = Lists.newArrayList();
				firstCallback = null;
				previousCallback = null;
				messageMetadata.clear();
				sequenceId = -1;
				batchedMessageMetadataAndPayload = null;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BatchMessageKeyBasedContainer));

	}

}