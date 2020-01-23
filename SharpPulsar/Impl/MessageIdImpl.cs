using System;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using ComparisonChain = com.google.common.collect.ComparisonChain;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;

	using MessageId = SharpPulsar.Api.MessageId;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using MessageIdData = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageIdData;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using ByteBufCodedInputStream = Org.Apache.Pulsar.Common.Util.Protobuf.ByteBufCodedInputStream;
	using ByteBufCodedOutputStream = Org.Apache.Pulsar.Common.Util.Protobuf.ByteBufCodedOutputStream;
	using UninitializedMessageException = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.UninitializedMessageException;

	[Serializable]
	public class MessageIdImpl : MessageId
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly long LedgerIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly long EntryIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly int PartitionIndexConflict;

		// Private constructor used only for json deserialization
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private MessageIdImpl()
		private MessageIdImpl() : this(-1, -1, -1)
		{
		}

		public MessageIdImpl(long LedgerId, long EntryId, int PartitionIndex)
		{
			this.LedgerIdConflict = LedgerId;
			this.EntryIdConflict = EntryId;
			this.PartitionIndexConflict = PartitionIndex;
		}

		public virtual long LedgerId
		{
			get
			{
				return LedgerIdConflict;
			}
		}

		public virtual long EntryId
		{
			get
			{
				return EntryIdConflict;
			}
		}

		public virtual int PartitionIndex
		{
			get
			{
				return PartitionIndexConflict;
			}
		}

		public override int GetHashCode()
		{
			return (int)(31 * (LedgerIdConflict + 31 * EntryIdConflict) + PartitionIndexConflict);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is BatchMessageIdImpl)
			{
				BatchMessageIdImpl Other = (BatchMessageIdImpl) Obj;
				return Other.Equals(this);
			}
			else if (Obj is MessageIdImpl)
			{
				MessageIdImpl Other = (MessageIdImpl) Obj;
				return LedgerIdConflict == Other.LedgerIdConflict && EntryIdConflict == Other.EntryIdConflict && PartitionIndexConflict == Other.PartitionIndexConflict;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0:D}:{1:D}:{2:D}", LedgerIdConflict, EntryIdConflict, PartitionIndexConflict);
		}

		// / Serialization

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.MessageId fromByteArray(byte[] data) throws java.io.IOException
		public static MessageId FromByteArray(sbyte[] Data)
		{
			checkNotNull(Data);
			ByteBufCodedInputStream InputStream = ByteBufCodedInputStream.get(Unpooled.wrappedBuffer(Data, 0, Data.Length));
			PulsarApi.MessageIdData.Builder Builder = PulsarApi.MessageIdData.newBuilder();

			PulsarApi.MessageIdData IdData;
			try
			{
				IdData = Builder.mergeFrom(InputStream, null).build();
			}
			catch (UninitializedMessageException E)
			{
				throw new IOException(E);
			}

			MessageIdImpl MessageId;
			if (IdData.hasBatchIndex())
			{
				MessageId = new BatchMessageIdImpl(IdData.LedgerId, IdData.EntryId, IdData.Partition, IdData.BatchIndex);
			}
			else
			{
				MessageId = new MessageIdImpl(IdData.LedgerId, IdData.EntryId, IdData.Partition);
			}

			InputStream.recycle();
			Builder.recycle();
			IdData.recycle();
			return MessageId;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.MessageId fromByteArrayWithTopic(byte[] data, String topicName) throws java.io.IOException
		public static MessageId FromByteArrayWithTopic(sbyte[] Data, string TopicName)
		{
			return fromByteArrayWithTopic(Data, TopicName.get(TopicName));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static SharpPulsar.api.MessageId fromByteArrayWithTopic(byte[] data, org.apache.pulsar.common.naming.TopicName topicName) throws java.io.IOException
		public static MessageId FromByteArrayWithTopic(sbyte[] Data, TopicName TopicName)
		{
			checkNotNull(Data);
			ByteBufCodedInputStream InputStream = ByteBufCodedInputStream.get(Unpooled.wrappedBuffer(Data, 0, Data.Length));
			PulsarApi.MessageIdData.Builder Builder = PulsarApi.MessageIdData.newBuilder();

			PulsarApi.MessageIdData IdData;
			try
			{
				IdData = Builder.mergeFrom(InputStream, null).build();
			}
			catch (UninitializedMessageException E)
			{
				throw new IOException(E);
			}

			MessageId MessageId;
			if (IdData.hasBatchIndex())
			{
				MessageId = new BatchMessageIdImpl(IdData.LedgerId, IdData.EntryId, IdData.Partition, IdData.BatchIndex);
			}
			else
			{
				MessageId = new MessageIdImpl(IdData.LedgerId, IdData.EntryId, IdData.Partition);
			}
			if (IdData.Partition > -1 && TopicName != null)
			{
				MessageId = new TopicMessageIdImpl(TopicName.getPartition(IdData.Partition).ToString(), TopicName.ToString(), MessageId);
			}

			InputStream.recycle();
			Builder.recycle();
			IdData.recycle();
			return MessageId;
		}

		// batchIndex is -1 if message is non-batched message and has the batchIndex for a batch message
		public virtual sbyte[] ToByteArray(int BatchIndex)
		{
			PulsarApi.MessageIdData.Builder Builder = PulsarApi.MessageIdData.newBuilder();
			Builder.LedgerId = LedgerIdConflict;
			Builder.EntryId = EntryIdConflict;
			if (PartitionIndexConflict >= 0)
			{
				Builder.Partition = PartitionIndexConflict;
			}

			if (BatchIndex != -1)
			{
				Builder.BatchIndex = BatchIndex;
			}

			PulsarApi.MessageIdData MsgId = Builder.build();
			int Size = MsgId.SerializedSize;
			ByteBuf Serialized = Unpooled.buffer(Size, Size);
			ByteBufCodedOutputStream Stream = ByteBufCodedOutputStream.get(Serialized);
			try
			{
				MsgId.writeTo(Stream);
			}
			catch (IOException E)
			{
				// This is in-memory serialization, should not fail
				throw new Exception(E);
			}

			MsgId.recycle();
			Builder.recycle();
			Stream.recycle();
			return Serialized.array();
		}

		public override sbyte[] ToByteArray()
		{
			// there is no message batch so we pass -1
			return ToByteArray(-1);
		}

		public override int CompareTo(MessageId O)
		{
			if (O is MessageIdImpl)
			{
				MessageIdImpl Other = (MessageIdImpl) O;
				return ComparisonChain.start().compare(this.LedgerIdConflict, Other.LedgerIdConflict).compare(this.EntryIdConflict, Other.EntryIdConflict).compare(this.PartitionIndex, Other.PartitionIndex).result();
			}
			else if (O is TopicMessageIdImpl)
			{
				return CompareTo(((TopicMessageIdImpl) O).InnerMessageId);
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected MessageIdImpl object. Got instance of " + O.GetType().FullName);
			}
		}
	}

}