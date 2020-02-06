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
namespace SharpPulsar.Impl
{
	using ComparisonChain = com.google.common.collect.ComparisonChain;
	using Lists = com.google.common.collect.Lists;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using PulsarClientException = Api.PulsarClientException;
	using PulsarByteBufAllocator = Org.Apache.Pulsar.Common.Allocator.PulsarByteBufAllocator;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using CompressionCodec = Org.Apache.Pulsar.Common.Compression.CompressionCodec;
	using ByteBufPair = Org.Apache.Pulsar.Common.Protocol.ByteBufPair;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using ByteString = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.ByteString;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


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

		public override bool Add<T1>(MessageImpl<T1> Msg, SendCallback Callback)
		{
			if (log.DebugEnabled)
			{
				log.debug("[{}] [{}] add message to batch, num messages in batch so far is {}", TopicName, ProducerName, NumMessagesInBatchConflict);
			}
			NumMessagesInBatchConflict++;
			CurrentBatchSizeBytes += Msg.DataBuffer.readableBytes();
			string Key = GetKey(Msg);
			KeyedBatch Part = batches[Key];
			if (Part == null)
			{
				Part = new KeyedBatch();
				Part.addMsg(Msg, Callback);
				Part.CompressionType = CompressionType;
				Part.Compressor = Compressor;
				Part.MaxBatchSize = MaxBatchSize;
				Part.TopicName = TopicName;
				Part.ProducerName = ProducerName;
				if (!batches.ContainsKey(Key)) batches.Add(Key, Part);
			}
			else
			{
				Part.addMsg(Msg, Callback);
			}
			return BatchFull;
		}

		public override void Clear()
		{
			NumMessagesInBatchConflict = 0;
			CurrentBatchSizeBytes = 0;
			batches = new Dictionary<string, KeyedBatch>();
		}

		public override bool Empty
		{
			get
			{
				return batches.Count == 0;
			}
		}

		public override void Discard(Exception Ex)
		{
			try
			{
				// Need to protect ourselves from any exception being thrown in the future handler from the application
				batches.forEach((k, v) => v.firstCallback.sendComplete(Ex));
			}
			catch (Exception T)
			{
				log.warn("[{}] [{}] Got exception while completing the callback", TopicName, ProducerName, T);
			}
			batches.forEach((k, v) => ReferenceCountUtil.safeRelease(v.batchedMessageMetadataAndPayload));
			Clear();
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
		private ProducerImpl.OpSendMsg CreateOpSendMsg(KeyedBatch KeyedBatch)
		{
			ByteBuf EncryptedPayload = ProducerConflict.encryptMessage(KeyedBatch.MessageMetadata, KeyedBatch.CompressedBatchMetadataAndPayload);
			if (EncryptedPayload.readableBytes() > ClientCnx.MaxMessageSize)
			{
				KeyedBatch.discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ClientCnx.MaxMessageSize + " bytes"));
				return null;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numMessagesInBatch = keyedBatch.messages.size();
			int NumMessagesInBatch = KeyedBatch.Messages.Count;
			long CurrentBatchSizeBytes = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (MessageImpl<?> message : keyedBatch.messages)
			foreach (MessageImpl<object> Message in KeyedBatch.Messages)
			{
				CurrentBatchSizeBytes += Message.DataBuffer.readableBytes();
			}
			KeyedBatch.MessageMetadata.NumMessagesInBatch = NumMessagesInBatch;
			ByteBufPair Cmd = ProducerConflict.sendMessage(ProducerConflict.producerId, KeyedBatch.SequenceId, NumMessagesInBatch, KeyedBatch.MessageMetadata.build(), EncryptedPayload);

			ProducerImpl.OpSendMsg Op = ProducerImpl.OpSendMsg.Create(KeyedBatch.Messages, Cmd, KeyedBatch.SequenceId, KeyedBatch.FirstCallback);

			Op.NumMessagesInBatch = NumMessagesInBatch;
			Op.BatchSizeByte = CurrentBatchSizeBytes;
			return Op;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<ProducerImpl.OpSendMsg> createOpSendMsgs() throws java.io.IOException
		public override IList<ProducerImpl.OpSendMsg> CreateOpSendMsgs()
		{
			IList<ProducerImpl.OpSendMsg> Result = new List<ProducerImpl.OpSendMsg>();
			IList<KeyedBatch> List = new List<KeyedBatch>(batches.Values);
			List.sort(((o1, o2) => ComparisonChain.start().compare(o1.sequenceId, o2.sequenceId).result()));
			foreach (KeyedBatch KeyedBatch in List)
			{
				ProducerImpl.OpSendMsg Op = CreateOpSendMsg(KeyedBatch);
				if (Op != null)
				{
					Result.Add(Op);
				}
			}
			return Result;
		}

		public override bool HasSameSchema<T1>(MessageImpl<T1> Msg)
		{
			string Key = GetKey(Msg);
			KeyedBatch Part = batches[Key];
			if (Part == null || Part.Messages.Count == 0)
			{
				return true;
			}
			if (!Part.MessageMetadata.hasSchemaVersion())
			{
				return Msg.SchemaVersion == null;
			}
			return Arrays.equals(Msg.SchemaVersion, Part.MessageMetadata.SchemaVersion.toByteArray());
		}

		private string GetKey<T1>(MessageImpl<T1> Msg)
		{
			if (Msg.hasOrderingKey())
			{
				return Base64.Encoder.encodeToString(Msg.OrderingKey);
			}
			return Msg.Key;
		}

		public class KeyedBatch
		{
			internal PulsarApi.MessageMetadata.Builder MessageMetadata = PulsarApi.MessageMetadata.newBuilder();
			// sequence id for this batch which will be persisted as a single entry by broker
			internal long SequenceId = -1;
			internal ByteBuf BatchedMessageMetadataAndPayload;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<MessageImpl<?>> messages = com.google.common.collect.Lists.newArrayList();
			internal IList<MessageImpl<object>> Messages = Lists.newArrayList();
			internal SendCallback PreviousCallback = null;
			internal PulsarApi.CompressionType CompressionType;
			internal CompressionCodec Compressor;
			internal int MaxBatchSize;
			internal string TopicName;
			internal string ProducerName;

			// keep track of callbacks for individual messages being published in a batch
			internal SendCallback FirstCallback;

			public virtual ByteBuf CompressedBatchMetadataAndPayload
			{
				get
				{
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (MessageImpl<?> msg : messages)
					foreach (MessageImpl<object> Msg in Messages)
					{
						PulsarApi.MessageMetadata.Builder MsgBuilder = Msg.MessageBuilder;
						BatchedMessageMetadataAndPayload = Commands.serializeSingleMessageInBatchWithPayload(MsgBuilder, Msg.DataBuffer, BatchedMessageMetadataAndPayload);
						MsgBuilder.recycle();
					}
					int UncompressedSize = BatchedMessageMetadataAndPayload.readableBytes();
					ByteBuf CompressedPayload = Compressor.encode(BatchedMessageMetadataAndPayload);
					BatchedMessageMetadataAndPayload.release();
					if (CompressionType != PulsarApi.CompressionType.NONE)
					{
						MessageMetadata.Compression = CompressionType;
						MessageMetadata.UncompressedSize = UncompressedSize;
					}
    
					// Update the current max batch size using the uncompressed size, which is what we need in any case to
					// accumulate the batch content
					MaxBatchSize = Math.Max(MaxBatchSize, UncompressedSize);
					return CompressedPayload;
				}
			}

			public virtual void AddMsg<T1>(MessageImpl<T1> Msg, SendCallback Callback)
			{
				if (Messages.Count == 0)
				{
					SequenceId = Commands.initBatchMessageMetadata(MessageMetadata, Msg.MessageBuilder);
					if (Msg.hasKey())
					{
						MessageMetadata.setPartitionKey(Msg.Key);
						if (Msg.hasBase64EncodedKey())
						{
							MessageMetadata.PartitionKeyB64Encoded = true;
						}
					}
					if (Msg.hasOrderingKey())
					{
						MessageMetadata.OrderingKey = ByteString.copyFrom(Msg.OrderingKey);
					}
					BatchedMessageMetadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(Math.Min(MaxBatchSize, ClientCnx.MaxMessageSize));
					FirstCallback = Callback;
				}
				if (PreviousCallback != null)
				{
					PreviousCallback.addCallback(Msg, Callback);
				}
				PreviousCallback = Callback;
				Messages.Add(Msg);
			}

			public virtual void Discard(Exception Ex)
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
					log.warn("[{}] [{}] Got exception while completing the callback for msg {}:", TopicName, ProducerName, SequenceId, T);
				}
				Clear();
			}

			public virtual void Clear()
			{
				Messages = Lists.newArrayList();
				FirstCallback = null;
				PreviousCallback = null;
				MessageMetadata.clear();
				SequenceId = -1;
				BatchedMessageMetadataAndPayload = null;
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BatchMessageKeyBasedContainer));

	}

}