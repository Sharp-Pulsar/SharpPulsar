﻿using System;
using System.IO;
using Google.Protobuf;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol.Extension;
using SharpPulsar.Protocol.Proto;

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
	public class MessageId : IMessageId
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
			return (int)(31 * (_ledgerId + 31 * _entryId) + _partitionIndex);
		}

		public override bool Equals(object obj)
		{
			if (obj is BatchMessageId other1)
			{
                return other1.Equals(this);
			}

            if (obj is MessageId other)
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
			var inputStream = new CodedInputStream((byte[])(object)data);
			var builder = new MessageIdData();

            MessageIdData idData = builder;
			try
			{
                //idData.MergeFrom(inputStream);
			}
			catch (System.Exception e)
			{
				throw e;
			}

			MessageId messageId;
			if (idData.BatchIndex >= 0)
			{
				messageId = new BatchMessageId((long)idData.ledgerId, (long)idData.entryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageId((long)idData.ledgerId, (long)idData.entryId, idData.Partition);
			}

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
            var builder = new MessageIdData();

            MessageIdData idData = builder;

			IMessageId messageId;
			if (idData.BatchIndex >= 0)
			{
				messageId = new BatchMessageId((long)idData.ledgerId, (long)idData.entryId, idData.Partition, idData.BatchIndex);
			}
			else
			{
				messageId = new MessageId((long)idData.ledgerId, (long)idData.entryId, idData.Partition);
			}
			if (idData.Partition > -1 && topicName != null)
			{
				var t = new TopicName();
				messageId = new TopicMessageId(t.GetPartition(idData.Partition).ToString(), topicName.ToString(), messageId);
			}

			return messageId;
		}

		// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
		public virtual sbyte[] ToByteArray(int batchIndex)
		{
            var builder = new MessageIdData {ledgerId = (ulong) (_ledgerId), entryId = (ulong) (_entryId)};
            if (_partitionIndex >= 0)
			{
				builder.Partition = (_partitionIndex);
			}

			if (batchIndex != -1)
			{
				builder.BatchIndex = (batchIndex);
			}
			return (sbyte[])(object)builder.ToByteArrays();
		}

		public sbyte[] ToByteArray()
		{
			// there is no message batch so we pass -1
			return ToByteArray(-1);
		}

		public int CompareTo(IMessageId o)
		{
			//Needs more 
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
                return CompareTo(impl.InnerMessageId);
            }
            throw new ArgumentException("expected MessageId object. Got instance of " + o.GetType().FullName);
        }
	}

}