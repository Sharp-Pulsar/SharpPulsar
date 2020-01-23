using DotNetty.Buffers;
using Google.ProtocolBuffers;
using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Impl.Batch;
using SharpPulsar.Interface.Message;
using SharpPulsar.Util.Protobuf;
using System;
using System.IO;

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
namespace SharpPulsar.Impl.Message
{

	public class MessageIdImpl : IMessageId
	{
		protected internal readonly long ledgerId;
		protected internal readonly long entryId;
		protected internal readonly int partitionIndex;

		// Private constructor used only for json deserialization
		private MessageIdImpl() : this(-1, -1, -1)
		{
		}

		public MessageIdImpl(long ledgerId, long entryId, int partitionIndex)
		{
			this.ledgerId = ledgerId;
			this.entryId = entryId;
			this.partitionIndex = partitionIndex;
		}

		public long LedgerId
		{
			get
			{
				return ledgerId;
			}
		}

		public long EntryId
		{
			get
			{
				return entryId;
			}
		}

		public int PartitionIndex
		{
			get
			{
				return partitionIndex;
			}
		}

		public override int GetHashCode()
		{
			return (int)(31 * (ledgerId + 31 * entryId) + partitionIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageIdImpl)
			{
				BatchMessageIdImpl other = (BatchMessageIdImpl) obj;
				return other.Equals(this);
			}
			else if (obj is MessageIdImpl)
			{
				MessageIdImpl other = (MessageIdImpl) obj;
				return ledgerId == other.ledgerId && entryId == other.entryId && partitionIndex == other.partitionIndex;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0:D}:{1:D}:{2:D}", ledgerId, entryId, partitionIndex);
		}

		public static IMessageId FromByteArray(sbyte[] data)
		{
			if (data != null)
				throw new NullReferenceException("Data is null");
			ByteBufCodedInputStream inputStream = ByteBufCodedInputStream.Get(Unpooled.WrappedBuffer((byte[])(Array)data, 0, data.Length));
			MessageIdData builder = new MessageIdData();

			MessageIdData idData;
			try
			{
				idData = builder.MergeFrom(inputStream, null).build();
			}
			catch (UninitializedMessageException e)
			{
				throw new IOException(e.Message);
			}

			MessageIdImpl messageId;
			//if (idData.hasBatchIndex())
			if (idData.BatchIndex > 0)
				{
				messageId = new BatchMessageIdImpl((long)idData.ledgerId, (long)idData.entryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageIdImpl((long)idData.ledgerId, (long)idData.entryId, idData.Partition);
			}

			inputStream.Recycle();
			builder.Recycle();
			idData.recycle();
			return messageId;
		}


		public static IMessageId FromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return FromByteArrayWithTopic(data, TopicName.get(topicName));
		}

		public static IMessageId FomByteArrayWithTopic(sbyte[] data, TopicName topicName)
		{
			if (data != null)
				throw new NullReferenceException("Data is null");
			ByteBufCodedInputStream inputStream = ByteBufCodedInputStream.get(Unpooled.wrappedBuffer(data, 0, data.Length));
			PulsarApi.MessageIdData.Builder builder = PulsarApi.MessageIdData.newBuilder();

			PulsarApi.MessageIdData idData;
			try
			{
				idData = builder.mergeFrom(inputStream, null).build();
			}
			catch (UninitializedMessageException e)
			{
				throw new IOException(e);
			}

			MessageId messageId;
			if (idData.hasBatchIndex())
			{
				messageId = new BatchMessageIdImpl(idData.LedgerId, idData.EntryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageIdImpl(idData.LedgerId, idData.EntryId, idData.Partition);
			}
			if (idData.Partition > -1 && topicName != null)
			{
				messageId = new TopicMessageIdImpl(topicName.getPartition(idData.Partition).ToString(), topicName.ToString(), messageId);
			}

			inputStream.recycle();
			builder.recycle();
			idData.recycle();
			return messageId;
		}

		// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
		protected internal sbyte[] ToByteArray(int batchIndex)
		{
			PulsarApi.MessageIdData.Builder builder = PulsarApi.MessageIdData.newBuilder();
			builder.LedgerId = ledgerId;
			builder.EntryId = entryId;
			if (partitionIndex >= 0)
			{
				builder.Partition = partitionIndex;
			}

			if (batchIndex != -1)
			{
				builder.BatchIndex = batchIndex;
			}

			PulsarApi.MessageIdData msgId = builder.build();
			int size = msgId.SerializedSize;
			ByteBuf serialized = Unpooled.buffer(size, size);
			ByteBufCodedOutputStream stream = ByteBufCodedOutputStream.get(serialized);
			try
			{
				msgId.writeTo(stream);
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(e);
			}

			msgId.recycle();
			builder.recycle();
			stream.recycle();
			return serialized.array();
		}

		public sbyte[] ToByteArray()
		{
			// there is no message batch so we pass -1
			return ToByteArray(-1);
		}

		public int CompareTo(IMessageId o)
		{
			if (o is MessageIdImpl)
			{
				MessageIdImpl other = (MessageIdImpl) o;
				return ComparisonChain.start().compare(this.ledgerId, other.ledgerId).compare(this.entryId, other.entryId).compare(this.PartitionIndex, other.PartitionIndex).result();
			}
			else if (o is TopicMessageIdImpl)
			{
				return CompareTo(((TopicMessageIdImpl) o).InnerMessageId);
			}
			else
			{

				throw new ArgumentException("expected MessageIdImpl object. Got instance of " + o.GetType().FullName);
			}
		}


	}

}