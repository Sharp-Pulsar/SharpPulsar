using System;
using System.IO;
using DotNetty.Buffers;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Util.Protobuf;

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
	[Serializable]
	public class MessageIdImpl : IMessageId
	{
		private  readonly long _ledgerId;
		private readonly long _entryId;
		private readonly int _partitionIndex;

		// Private constructor used only for json deserialization
		private MessageIdImpl() : this(-1, -1, -1)
		{
		}

		public MessageIdImpl(long ledgerId, long entryId, int partitionIndex)
		{
			_ledgerId = ledgerId;
			_entryId = entryId;
			_partitionIndex = partitionIndex;
		}

		public virtual long LedgerId => _ledgerId;

        public virtual long EntryId => _entryId;

        public virtual int PartitionIndex => _partitionIndex;

        public override int GetHashCode()
		{
			return (int)(31 * (_ledgerId + 31 * _entryId) + _partitionIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageIdImpl other1)
			{
                return other1.Equals(this);
			}
			else if (obj is MessageIdImpl other)
			{
                return _ledgerId == other._ledgerId && _entryId == other._entryId && _partitionIndex == other._partitionIndex;
			}
			return false;
		}

		public override string ToString()
		{
			return $"{_ledgerId:D}:{_entryId:D}:{_partitionIndex:D}";
		}

		// / Serialization

		public static IMessageId FromByteArray(sbyte[] data)
		{
			if(data == null)
				throw new ArgumentException();
			var inputStream = ByteBufCodedInputStream.Get(Unpooled.WrappedBuffer((byte[])(object)data, 0, data.Length));
			var builder = MessageIdData.NewBuilder();

			MessageIdData idData;
			try
			{
				idData = ((MessageIdData.Builder)builder.MergeFrom(inputStream, null)).Build();
			}
			catch (System.Exception e)
			{
				throw e;
			}

			MessageIdImpl messageId;
			if (idData.HasBatchIndex)
			{
				messageId = new BatchMessageIdImpl((long)idData.LedgerId, (long)idData.EntryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageIdImpl((long)idData.LedgerId, (long)idData.EntryId, idData.Partition);
			}

			inputStream.Recycle();
			builder.Recycle();
			idData.Recycle();
			return messageId;
		}

		public static IMessageId FromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return FromByteArrayWithTopic(data, TopicName.Get(topicName));
		}

		public static IMessageId FromByteArrayWithTopic(sbyte[] data, TopicName topicName)
		{
            if (data == null)
                throw new ArgumentException();
			var inputStream = ByteBufCodedInputStream.Get(Unpooled.WrappedBuffer((byte[])(object)data, 0, data.Length));
			var builder = MessageIdData.NewBuilder();

			MessageIdData idData;
			try
			{
				idData = ((MessageIdData.Builder)builder.MergeFrom(inputStream, null)).Build();
			}
            catch (System.Exception e)
            {
                throw e;
            }

			IMessageId messageId;
			if (idData.HasBatchIndex)
			{
				messageId = new BatchMessageIdImpl((long)idData.LedgerId, (long)idData.EntryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageIdImpl((long)idData.LedgerId, (long)idData.EntryId, idData.Partition);
			}
			if (idData.Partition > -1 && topicName != null)
			{
				var t = new TopicName();
				messageId = new TopicMessageIdImpl(t.GetPartition(idData.Partition).ToString(), topicName.ToString(), messageId);
			}

			inputStream.Recycle();
			builder.Recycle();
			idData.Recycle();
			return messageId;
		}

		// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
		public virtual sbyte[] ToByteArray(int batchIndex)
		{
			var builder = MessageIdData.NewBuilder();
			builder.SetLedgerId(_ledgerId);
			builder.SetEntryId(_entryId);
			if (_partitionIndex >= 0)
			{
				builder.SetPartition(_partitionIndex);
			}

			if (batchIndex != -1)
			{
				builder.SetBatchIndex(batchIndex);
			}

			var msgId = builder.Build();
			var size = msgId.SerializedSize;
			var serialized = Unpooled.Buffer(size, size);
			var stream = ByteBufCodedOutputStream.Get(serialized);
			try
			{
				msgId.WriteTo(stream);
			}
			catch (IOException e)
			{
				// This is in-memory serialization, should not fail
				throw new System.Exception(e.Message);
			}

			msgId.Recycle();
			builder.Recycle();
			stream.Recycle();
			return (sbyte[])(object)serialized.Array;
		}

		public sbyte[] ToByteArray()
		{
			// there is no message batch so we pass -1
			return ToByteArray(-1);
		}

		public int CompareTo(IMessageId o)
		{
			//Needs more 
			if (o is MessageIdImpl other)
            {
                if ((_entryId > other.EntryId) && (_ledgerId > other.LedgerId) && (_partitionIndex > other.PartitionIndex))
                {
                    return -1;
                }
				else if ((_entryId < other.EntryId) && (_ledgerId < other.LedgerId) && (_partitionIndex < other.PartitionIndex))
                {
                    return 1;
                }

                return 0;
            }
			else if (o is TopicMessageIdImpl impl)
			{
				return CompareTo(impl.InnerMessageId);
			}
			else
			{
				throw new ArgumentException("expected MessageIdImpl object. Got instance of " + o.GetType().FullName);
			}
		}
	}

}