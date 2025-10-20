using System;
using System.IO;
using DotNetty.Buffers;
using Google.Protobuf;
using Pulsar.Proto;
using SharpPulsar.API;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Shared.Buf;

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
namespace SharpPulsar
{
    public class MessageId : IMessageIdAdv
	{
		private  readonly long _ledgerId;
		private readonly long _entryId;
		private readonly int _partitionIndex;

		// Private constructor used only for json deserialization
		private MessageId() : this(-1, -1, -1)
		{
		}

		public MessageId(long ledgerId, long entryId, int partitionIndex)
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
            return MessageIdAdvUtils.HashCode(this);
        }

		public override bool Equals(object obj)
		{
            return MessageIdAdvUtils.Equals(this, obj);
        }

		public override string ToString()
		{
			return $"{_ledgerId:D}:{_entryId:D}:{_partitionIndex:D}";
		}

		// / Serialization

		public static IMessageId FromByteArray(byte[] data)
		{
			if(data == null)
				throw new ArgumentException();
			var inputStream = new CodedInputStream(data);
			var builder = new MessageIdData();

            MessageIdData idData;
            try
            {
                idData = MessageIdData.Parser.ParseFrom(data, 0, data.Length);
            }
            catch (Exception e)
            {
                throw new IOException(e.Source);
            }

            MessageId messageId;
            if (idData.HasBatchIndex)
            {
                if (idData.HasBatchSize)
                {
                    messageId = new BatchMessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition, idData.BatchIndex, idData.BatchSize, BatchMessageId.NewAckSet(idData.BatchSize));
                }
                else
                {
                    messageId = new BatchMessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition, idData.BatchIndex);
                }
            }
            else if (idData.FirstChunkMessageId != null)
            {
                var firstChunkIdData = idData.FirstChunkMessageId;
                messageId = new ChunkMessageId(
                        new MessageId((long)firstChunkIdData.LedgerId, (long)firstChunkIdData.EntryId,
                                firstChunkIdData.Partition),
                        new MessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition));
            }
            else
            {
                messageId = new MessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition);
            }
            

			return messageId;
		}

		public static IMessageId FromByteArrayWithTopic(byte[] data, string topicName)
		{
			return FromByteArrayWithTopic(data, TopicName.Get(topicName));
		}

		public static IMessageId FromByteArrayWithTopic(byte[] data, TopicName topicName)
		{
            if (data == null)
                throw new ArgumentException();

            MessageIdData idData;
            try
            {
                idData = MessageIdData.Parser.ParseFrom(data, 0, data.Length);
            }
            catch (Exception e)
            {
                throw new IOException(e.Source);
            }

            IMessageIdAdv messageId;
            if (idData.HasBatchIndex)
            {
                if (idData.HasBatchSize)
                {
                    messageId = new BatchMessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition, idData.BatchIndex, idData.BatchSize, BatchMessageId.NewAckSet(idData.BatchSize));
                }
                else
                {
                    messageId = new BatchMessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition,
                        idData.BatchIndex, 0, null);
                }
            }
            else
            {
                messageId = new MessageId((long)idData.LedgerId, (long)idData.EntryId, idData.Partition);
            }
			if (idData.Partition > -1 && topicName != null)
			{
				var t = new TopicName();
				messageId = new TopicMessageId(t.GetPartition(idData.Partition).ToString(), topicName.ToString(), messageId);
			}

			return messageId;
		}
        public MessageIdData WriteMessageIdData(MessageIdData msgId, int batchIndex, int batchSize)
        {
            if (msgId == null)
            {
                //msgId = LOCAL_MESSAGE_ID.get()
                        //.clear();
            }

            msgId.LedgerId = (ulong)LedgerId;
            msgId.EntryId = (ulong)EntryId;

            if (PartitionIndex >= 0)
            {
                msgId.Partition = PartitionIndex;
            }

            if (batchIndex != -1)
            {
                msgId.BatchIndex = batchIndex;
            }

            if (batchSize > -1)
            {
                msgId.BatchSize = batchSize;
            }

            return msgId;
        }

		// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
		public virtual byte[] ToByteArray(int batchIndex, int batchSize)
		{
			MessageIdData msgId = WriteMessageIdData(null, batchIndex, batchSize);

            int size = msgId.CalculateSize();
            var serialized = Unpooled.Buffer(size, size);
            msgId.WriteTo(new CodedOutputStream(serialized.Array));

            return serialized.Array;
        }
		public virtual byte[] ToByteArray()
		{
			// there is no message batch so we pass -1
			return ToByteArray(-1, 0);
		}

        public int CompareTo(IMessageId o)
        {

            if (o is BatchMessageId bm)
            {
                var ord = 0;
                var ledgercompare = _ledgerId.CompareTo(bm.LedgerId);

                if (ledgercompare != 0)
                    ord = ledgercompare;

                var entryCompare = EntryId.CompareTo(bm.EntryId);
                if (entryCompare != 0 && ord == 0)
                    ord = entryCompare;

                var partitionCompare = PartitionIndex.CompareTo(bm.PartitionIndex);
                if (partitionCompare != 0 && ord == 0)
                    ord = partitionCompare;

                var result = ledgercompare == 0 && entryCompare == 0 && partitionCompare == 0;
                if (result && bm.BatchIndex > -1)
                    return -1;

                return ord;
            }
            if (o is MessageId other)
            {
                var ledgerCompare = _ledgerId.CompareTo(other.LedgerId);
                if (ledgerCompare != 0)
                    return ledgerCompare;

                var entryCompare = _entryId.CompareTo(other.EntryId);
                if (entryCompare != 0)
                    return entryCompare;

                var partitionedCompare = _partitionIndex.CompareTo(other.PartitionIndex);
                if (partitionedCompare != 0)
                    return partitionedCompare;

                return 0;
            }
            if (o is TopicMessageId impl)
            {
                return CompareTo(impl.MessageId);
            }
            throw new ArgumentException("expected MessageId object. Got instance of " + o.GetType().FullName);
        }
    }

}