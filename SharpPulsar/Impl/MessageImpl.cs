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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Maps = com.google.common.collect.Maps;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;


	using SharpPulsar.Api;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using SharpPulsar.Api;
	using SharpPulsar.Impl.Schema;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using EncryptionContext = Org.Apache.Pulsar.Common.Api.EncryptionContext;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using KeyValue = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
    using DotNetty.Buffers;

    public class MessageImpl<T> : Message<T>
	{

		protected internal IMessageId MessageId;
		public MessageBuilder messageBuilder;
		public ClientCnx Cnx;
		public IByteBuffer DataBuffer;
		private Schema<T> schema;
		private SchemaState schemaState = SchemaState.None;
		private Optional<EncryptionContext> encryptionCtx = null;

		public string TopicName {get;} // only set for incoming messages
		[NonSerialized]
		public IDictionary<string, string> Properties { get; set; }
		public virtual RedeliveryCount {get;}

		// Constructor for out-going message
		internal static MessageImpl<T> Create<T>(PulsarApi.MessageMetadata.Builder MsgMetadataBuilder, ByteBuffer Payload, Schema<T> Schema)
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") MessageImpl<T> msg = (MessageImpl<T>) RECYCLER.get();
			MessageImpl<T> Msg = (MessageImpl<T>) RECYCLER.get();
			Msg.MessageBuilder = MsgMetadataBuilder;
			Msg.MessageIdConflict = null;
			Msg.TopicName = null;
			Msg.Cnx = null;
			Msg.DataBuffer = Unpooled.wrappedBuffer(Payload);
			Msg.properties = null;
			Msg.schema = Schema;
			return Msg;
		}

		// Constructor for incoming message
		public MessageImpl(string Topic, MessageIdImpl MessageId, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload, ClientCnx Cnx, Schema<T> Schema) : this(Topic, MessageId, MsgMetadata, Payload, null, Cnx, Schema)
		{
		}

		public MessageImpl(string Topic, MessageIdImpl MessageId, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, Schema<T> Schema) : this(Topic, MessageId, MsgMetadata, Payload, EncryptionCtx, Cnx, Schema, 0)
		{
		}

		public MessageImpl(string Topic, MessageIdImpl MessageId, PulsarApi.MessageMetadata MsgMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, Schema<T> Schema, int RedeliveryCount)
		{
			this.MessageBuilder = PulsarApi.MessageMetadata.newBuilder(MsgMetadata);
			this.MessageIdConflict = MessageId;
			this.TopicName = Topic;
			this.Cnx = Cnx;
			this.RedeliveryCount = RedeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			this.DataBuffer = Unpooled.copiedBuffer(Payload);
			this.encryptionCtx = EncryptionCtx;

			if (MsgMetadata.PropertiesCount > 0)
			{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				this.properties = Collections.unmodifiableMap(MessageBuilder.PropertiesList.ToDictionary(PulsarApi.KeyValue::getKey, PulsarApi.KeyValue::getValue));
			}
			else
			{
				properties = Collections.emptyMap();
			}
			this.schema = Schema;
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, PulsarApi.MessageMetadata MsgMetadata, PulsarApi.SingleMessageMetadata SingleMessageMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, Schema<T> Schema) : this(Topic, BatchMessageIdImpl, MsgMetadata, SingleMessageMetadata, Payload, EncryptionCtx, Cnx, Schema, 0)
		{
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, PulsarApi.MessageMetadata MsgMetadata, PulsarApi.SingleMessageMetadata SingleMessageMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, Schema<T> Schema, int RedeliveryCount)
		{
			this.MessageBuilder = PulsarApi.MessageMetadata.newBuilder(MsgMetadata);
			this.MessageIdConflict = BatchMessageIdImpl;
			this.TopicName = Topic;
			this.Cnx = Cnx;
			this.RedeliveryCount = RedeliveryCount;

			this.DataBuffer = Unpooled.copiedBuffer(Payload);
			this.encryptionCtx = EncryptionCtx;

			if (SingleMessageMetadata.PropertiesCount > 0)
			{
				IDictionary<string, string> Properties = Maps.newTreeMap();
				foreach (PulsarApi.KeyValue Entry in SingleMessageMetadata.PropertiesList)
				{
					Properties[Entry.Key] = Entry.Value;
				}
				this.properties = Collections.unmodifiableMap(Properties);
			}
			else
			{
				properties = Collections.emptyMap();
			}

			if (SingleMessageMetadata.hasPartitionKey())
			{
				MessageBuilder.PartitionKeyB64Encoded = SingleMessageMetadata.PartitionKeyB64Encoded;
				MessageBuilder.setPartitionKey(SingleMessageMetadata.PartitionKey);
			}

			if (SingleMessageMetadata.hasEventTime())
			{
				MessageBuilder.EventTime = SingleMessageMetadata.EventTime;
			}

			if (SingleMessageMetadata.hasSequenceId())
			{
				MessageBuilder.SequenceId = SingleMessageMetadata.SequenceId;
			}

			this.schema = Schema;
		}

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> Properties, sbyte[] Payload, Schema<T> Schema) : this(Topic, MsgId, Properties, Unpooled.wrappedBuffer(Payload), Schema)
		{
		}

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> Properties, ByteBuf Payload, Schema<T> Schema)
		{
			string[] Data = MsgId.Split(":", true);
			long LedgerId = long.Parse(Data[0]);
			long EntryId = long.Parse(Data[1]);
			if (Data.Length == 3)
			{
				this.MessageIdConflict = new BatchMessageIdImpl(LedgerId, EntryId, -1, int.Parse(Data[2]));
			}
			else
			{
				this.MessageIdConflict = new MessageIdImpl(LedgerId, EntryId, -1);
			}
			this.TopicName = Topic;
			this.Cnx = null;
			this.DataBuffer = Payload;
			this.properties = Collections.unmodifiableMap(Properties);
			this.schema = Schema;
			this.RedeliveryCount = 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static MessageImpl<byte[]> deserialize(io.netty.buffer.ByteBuf headersAndPayload) throws java.io.IOException
		public static MessageImpl<sbyte[]> Deserialize(ByteBuf HeadersAndPayload)
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") MessageImpl<byte[]> msg = (MessageImpl<byte[]>) RECYCLER.get();
			MessageImpl<sbyte[]> Msg = (MessageImpl<sbyte[]>) RECYCLER.get();
			PulsarApi.MessageMetadata MsgMetadata = Commands.parseMessageMetadata(HeadersAndPayload);

			Msg.MessageBuilder = PulsarApi.MessageMetadata.newBuilder(MsgMetadata);
			MsgMetadata.recycle();
			Msg.DataBuffer = HeadersAndPayload;
			Msg.MessageIdConflict = null;
			Msg.TopicName = null;
			Msg.Cnx = null;
			Msg.properties = Collections.emptyMap();
			return Msg;
		}

		public virtual string ReplicatedFrom
		{
			set
			{
				checkNotNull(MessageBuilder);
				MessageBuilder.setReplicatedFrom(value);
			}
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.getReplicatedFrom();
			}
		}

		public virtual bool Replicated
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.HasReplicatedFrom();
			}
		}


		public virtual long PublishTime
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.PublishTime;
			}
		}

		public virtual long EventTime
		{
			get
			{
				checkNotNull(MessageBuilder);
				if (MessageBuilder.HasEventTime())
				{
					return MessageBuilder.EventTime;
				}
				return 0;
			}
		}

		public virtual bool IsExpired(int MessageTTLInSeconds)
		{
			return MessageTTLInSeconds != 0 && DateTimeHelper.CurrentUnixTimeMillis() > (PublishTime + BAMCIS.Util.Concurrent.TimeUnit.SECONDS.toMillis(MessageTTLInSeconds));
		}

		public virtual sbyte[] Data
		{
			get
			{
				if (DataBuffer.arrayOffset() == 0 && DataBuffer.capacity() == DataBuffer.array().length)
				{
					return DataBuffer.array();
				}
				else
				{
					// Need to copy into a smaller byte array
					sbyte[] Data = new sbyte[DataBuffer.readableBytes()];
					DataBuffer.readBytes(Data);
					return Data;
				}
			}
		}

		public virtual Schema Schema
		{
			get
			{
				return this.schema;
			}
		}

		public virtual sbyte[] SchemaVersion
		{
			get
			{
				if (MessageBuilder != null && MessageBuilder.HasSchemaVersion())
				{
					return MessageBuilder.SchemaVersion.toByteArray();
				}
				else
				{
					return null;
				}
			}
		}

		public virtual T Value
		{
			get
			{
				if (schema.SchemaInfo != null && SchemaType.KEY_VALUE == schema.SchemaInfo.Type)
				{
					if (schema.SupportSchemaVersioning())
					{
						return KeyValueBySchemaVersion;
					}
					else
					{
						return KeyValue;
					}
				}
				else
				{
					// check if the schema passed in from client supports schema versioning or not
					// this is an optimization to only get schema version when necessary
					if (schema.SupportSchemaVersioning())
					{
						sbyte[] SchemaVersion = SchemaVersion;
						if (null == SchemaVersion)
						{
							return schema.Decode(Data);
						}
						else
						{
							return schema.Decode(Data, SchemaVersion);
						}
					}
					else
					{
						return schema.Decode(Data);
					}
				}
			}
		}

		private T KeyValueBySchemaVersion
		{
			get
			{
				KeyValueSchema KvSchema = (KeyValueSchema) schema;
				sbyte[] SchemaVersion = SchemaVersion;
				if (KvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T) KvSchema.decode(KeyBytes, Data, SchemaVersion);
				}
				else
				{
					return schema.Decode(Data, SchemaVersion);
				}
			}
		}

		private T KeyValue
		{
			get
			{
				KeyValueSchema KvSchema = (KeyValueSchema) schema;
				if (KvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T) KvSchema.decode(KeyBytes, Data, null);
				}
				else
				{
					return schema.Decode(Data);
				}
			}
		}

		public virtual long SequenceId
		{
			get
			{
				checkNotNull(MessageBuilder);
				if (MessageBuilder.HasSequenceId())
				{
					return MessageBuilder.SequenceId;
				}
				return -1;
			}
		}

		public virtual string ProducerName
		{
			get
			{
				checkNotNull(MessageBuilder);
				if (MessageBuilder.HasProducerName())
				{
					return MessageBuilder.getProducerName();
				}
				return null;
			}
		}


		public virtual MessageId getMessageId()
		{
			checkNotNull(MessageIdConflict, "Cannot get the message id of a message that was not received");
			return MessageIdConflict;
		}

		public virtual IDictionary<string, string> Properties
		{
			get
			{
				lock (this)
				{
					if (this.properties == null)
					{
						if (MessageBuilder.PropertiesCount > 0)
						{
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
							this.properties = Collections.unmodifiableMap(MessageBuilder.PropertiesList.ToDictionary(PulsarApi.KeyValue::getKey, PulsarApi.KeyValue::getValue));
						}
						else
						{
							this.properties = Collections.emptyMap();
						}
					}
					return this.properties;
				}
			}
		}

		public override bool HasProperty(string Name)
		{
			return Properties.ContainsKey(Name);
		}

		public override string GetProperty(string Name)
		{
			return this.Properties[Name];
		}


		public override bool HasKey()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.HasPartitionKey();
		}


		public virtual string Key
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.getPartitionKey();
			}
		}

		public override bool HasBase64EncodedKey()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.PartitionKeyB64Encoded;
		}

		public virtual sbyte[] KeyBytes
		{
			get
			{
				checkNotNull(MessageBuilder);
				if (HasBase64EncodedKey())
				{
					return Base64.Decoder.decode(Key);
				}
				else
				{
					return Key.GetBytes(UTF_8);
				}
			}
		}

		public override bool HasOrderingKey()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.HasOrderingKey();
		}

		public virtual sbyte[] OrderingKey
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.OrderingKey.toByteArray();
			}
		}


		public virtual void Recycle()
		{
			MessageBuilder = null;
			MessageIdConflict = null;
			TopicName = null;
			DataBuffer = null;
			properties = null;
			schema = null;
			schemaState = SchemaState.None;

			if (recyclerHandle != null)
			{
				recyclerHandle.recycle(this);
			}
		}

		private MessageImpl<T1>(Recycler.Handle<T1> RecyclerHandle)
		{
			this.recyclerHandle = RecyclerHandle;
			this.RedeliveryCount = 0;
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private io.netty.util.Recycler.Handle<MessageImpl<?>> recyclerHandle;
		private Recycler.Handle<MessageImpl<object>> recyclerHandle;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final static io.netty.util.Recycler<MessageImpl<?>> RECYCLER = new io.netty.util.Recycler<MessageImpl<?>>()
		private static readonly Recycler<MessageImpl<object>> RECYCLER = new RecyclerAnonymousInnerClass();

		public class RecyclerAnonymousInnerClass : Recycler<MessageImpl<JavaToDotNetGenericWildcard>>
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: @Override protected MessageImpl<?> newObject(io.netty.util.Recycler.Handle<MessageImpl<?>> handle)
			public override MessageImpl<object> newObject<T1>(Recycler.Handle<T1> Handle)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new MessageImpl<>(handle);
				return new MessageImpl<object>(Handle);
			}
		}

		public virtual bool HasReplicateTo()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.ReplicateToCount > 0;
		}

		public virtual IList<string> ReplicateTo
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.ReplicateToList;
			}
		}

		public virtual void SetMessageId(MessageIdImpl MessageId)
		{
			this.MessageIdConflict = MessageId;
		}

		public virtual Optional<EncryptionContext> EncryptionCtx
		{
			get
			{
				return encryptionCtx;
			}
		}


		public virtual SchemaState? GetSchemaState()
		{
			return schemaState;
		}

		public virtual void SetSchemaState(SchemaState SchemaState)
		{
			this.schemaState = SchemaState;
		}

		public enum SchemaState
		{
			None,
			Ready,
			Broken
		}
	}

}