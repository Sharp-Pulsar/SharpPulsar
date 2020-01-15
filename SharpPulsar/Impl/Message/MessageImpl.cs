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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkNotNull;

	using Maps = com.google.common.collect.Maps;

	using ByteBuf = io.netty.buffer.ByteBuf;
	using Unpooled = io.netty.buffer.Unpooled;
	using Recycler = io.netty.util.Recycler;
	using Handle = io.netty.util.Recycler.Handle;


	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Schema = org.apache.pulsar.client.api.Schema;
	using org.apache.pulsar.client.impl.schema;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using EncryptionContext = org.apache.pulsar.common.api.EncryptionContext;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	public class MessageImpl<T> : Message<T>
	{

		protected internal MessageId messageId;
		private PulsarApi.MessageMetadata.Builder msgMetadataBuilder;
		private ClientCnx cnx;
		private ByteBuf payload;
		private Schema<T> schema;
		private SchemaState schemaState = SchemaState.None;
		private Optional<EncryptionContext> encryptionCtx = null;

		private string topic; // only set for incoming messages
		[NonSerialized]
		private IDictionary<string, string> properties;
		private readonly int redeliveryCount;

		// Constructor for out-going message
		internal static MessageImpl<T> create<T>(PulsarApi.MessageMetadata.Builder msgMetadataBuilder, ByteBuffer payload, Schema<T> schema)
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") MessageImpl<T> msg = (MessageImpl<T>) RECYCLER.get();
			MessageImpl<T> msg = (MessageImpl<T>) RECYCLER.get();
			msg.msgMetadataBuilder = msgMetadataBuilder;
			msg.messageId = null;
			msg.topic = null;
			msg.cnx = null;
			msg.payload = Unpooled.wrappedBuffer(payload);
			msg.properties = null;
			msg.schema = schema;
			return msg;
		}

		// Constructor for incoming message
		internal MessageImpl(string topic, MessageIdImpl messageId, PulsarApi.MessageMetadata msgMetadata, ByteBuf payload, ClientCnx cnx, Schema<T> schema) : this(topic, messageId, msgMetadata, payload, null, cnx, schema)
		{
		}

		internal MessageImpl(string topic, MessageIdImpl messageId, PulsarApi.MessageMetadata msgMetadata, ByteBuf payload, Optional<EncryptionContext> encryptionCtx, ClientCnx cnx, Schema<T> schema) : this(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, 0)
		{
		}

		internal MessageImpl(string topic, MessageIdImpl messageId, PulsarApi.MessageMetadata msgMetadata, ByteBuf payload, Optional<EncryptionContext> encryptionCtx, ClientCnx cnx, Schema<T> schema, int redeliveryCount)
		{
			this.msgMetadataBuilder = PulsarApi.MessageMetadata.newBuilder(msgMetadata);
			this.messageId = messageId;
			this.topic = topic;
			this.cnx = cnx;
			this.redeliveryCount = redeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			this.payload = Unpooled.copiedBuffer(payload);
			this.encryptionCtx = encryptionCtx;

			if (msgMetadata.PropertiesCount > 0)
			{
				this.properties = Collections.unmodifiableMap(msgMetadataBuilder.PropertiesList.ToDictionary(PulsarApi.KeyValue.getKey, PulsarApi.KeyValue.getValue));
			}
			else
			{
				properties = Collections.emptyMap();
			}
			this.schema = schema;
		}

		internal MessageImpl(string topic, BatchMessageIdImpl batchMessageIdImpl, PulsarApi.MessageMetadata msgMetadata, PulsarApi.SingleMessageMetadata singleMessageMetadata, ByteBuf payload, Optional<EncryptionContext> encryptionCtx, ClientCnx cnx, Schema<T> schema) : this(topic, batchMessageIdImpl, msgMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, 0)
		{
		}

		internal MessageImpl(string topic, BatchMessageIdImpl batchMessageIdImpl, PulsarApi.MessageMetadata msgMetadata, PulsarApi.SingleMessageMetadata singleMessageMetadata, ByteBuf payload, Optional<EncryptionContext> encryptionCtx, ClientCnx cnx, Schema<T> schema, int redeliveryCount)
		{
			this.msgMetadataBuilder = PulsarApi.MessageMetadata.newBuilder(msgMetadata);
			this.messageId = batchMessageIdImpl;
			this.topic = topic;
			this.cnx = cnx;
			this.redeliveryCount = redeliveryCount;

			this.payload = Unpooled.copiedBuffer(payload);
			this.encryptionCtx = encryptionCtx;

			if (singleMessageMetadata.PropertiesCount > 0)
			{
				IDictionary<string, string> properties = Maps.newTreeMap();
				foreach (PulsarApi.KeyValue entry in singleMessageMetadata.PropertiesList)
				{
					properties[entry.Key] = entry.Value;
				}
				this.properties = Collections.unmodifiableMap(properties);
			}
			else
			{
				properties = Collections.emptyMap();
			}

			if (singleMessageMetadata.hasPartitionKey())
			{
				msgMetadataBuilder.PartitionKeyB64Encoded = singleMessageMetadata.PartitionKeyB64Encoded;
				msgMetadataBuilder.PartitionKey = singleMessageMetadata.PartitionKey;
			}

			if (singleMessageMetadata.hasEventTime())
			{
				msgMetadataBuilder.EventTime = singleMessageMetadata.EventTime;
			}

			if (singleMessageMetadata.hasSequenceId())
			{
				msgMetadataBuilder.SequenceId = singleMessageMetadata.SequenceId;
			}

			this.schema = schema;
		}

		public MessageImpl(string topic, string msgId, IDictionary<string, string> properties, sbyte[] payload, Schema<T> schema) : this(topic, msgId, properties, Unpooled.wrappedBuffer(payload), schema)
		{
		}

		public MessageImpl(string topic, string msgId, IDictionary<string, string> properties, ByteBuf payload, Schema<T> schema)
		{
			string[] data = msgId.Split(":", true);
			long ledgerId = long.Parse(data[0]);
			long entryId = long.Parse(data[1]);
			if (data.Length == 3)
			{
				this.messageId = new BatchMessageIdImpl(ledgerId, entryId, -1, int.Parse(data[2]));
			}
			else
			{
				this.messageId = new MessageIdImpl(ledgerId, entryId, -1);
			}
			this.topic = topic;
			this.cnx = null;
			this.payload = payload;
			this.properties = Collections.unmodifiableMap(properties);
			this.schema = schema;
			this.redeliveryCount = 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static MessageImpl<byte[]> deserialize(io.netty.buffer.ByteBuf headersAndPayload) throws java.io.IOException
		public static MessageImpl<sbyte[]> deserialize(ByteBuf headersAndPayload)
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") MessageImpl<byte[]> msg = (MessageImpl<byte[]>) RECYCLER.get();
			MessageImpl<sbyte[]> msg = (MessageImpl<sbyte[]>) RECYCLER.get();
			PulsarApi.MessageMetadata msgMetadata = Commands.parseMessageMetadata(headersAndPayload);

			msg.msgMetadataBuilder = PulsarApi.MessageMetadata.newBuilder(msgMetadata);
			msgMetadata.recycle();
			msg.payload = headersAndPayload;
			msg.messageId = null;
			msg.topic = null;
			msg.cnx = null;
			msg.properties = Collections.emptyMap();
			return msg;
		}

		public virtual string ReplicatedFrom
		{
			set
			{
				checkNotNull(msgMetadataBuilder);
				msgMetadataBuilder.ReplicatedFrom = value;
			}
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.ReplicatedFrom;
			}
		}

		public override bool Replicated
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.hasReplicatedFrom();
			}
		}


		public override long PublishTime
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.PublishTime;
			}
		}

		public override long EventTime
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				if (msgMetadataBuilder.hasEventTime())
				{
					return msgMetadataBuilder.EventTime;
				}
				return 0;
			}
		}

		public virtual bool isExpired(int messageTTLInSeconds)
		{
			return messageTTLInSeconds != 0 && DateTimeHelper.CurrentUnixTimeMillis() > (PublishTime + TimeUnit.SECONDS.toMillis(messageTTLInSeconds));
		}

		public override sbyte[] Data
		{
			get
			{
				if (payload.arrayOffset() == 0 && payload.capacity() == payload.array().length)
				{
					return payload.array();
				}
				else
				{
					// Need to copy into a smaller byte array
					sbyte[] data = new sbyte[payload.readableBytes()];
					payload.readBytes(data);
					return data;
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

		public override sbyte[] SchemaVersion
		{
			get
			{
				if (msgMetadataBuilder != null && msgMetadataBuilder.hasSchemaVersion())
				{
					return msgMetadataBuilder.SchemaVersion.toByteArray();
				}
				else
				{
					return null;
				}
			}
		}

		public override T Value
		{
			get
			{
				if (schema.SchemaInfo != null && SchemaType.KEY_VALUE == schema.SchemaInfo.Type)
				{
					if (schema.supportSchemaVersioning())
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
					if (schema.supportSchemaVersioning())
					{
						sbyte[] schemaVersion = SchemaVersion;
						if (null == schemaVersion)
						{
							return schema.decode(Data);
						}
						else
						{
							return schema.decode(Data, schemaVersion);
						}
					}
					else
					{
						return schema.decode(Data);
					}
				}
			}
		}

		private T KeyValueBySchemaVersion
		{
			get
			{
				KeyValueSchema kvSchema = (KeyValueSchema) schema;
				sbyte[] schemaVersion = SchemaVersion;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T) kvSchema.decode(KeyBytes, Data, schemaVersion);
				}
				else
				{
					return schema.decode(Data, schemaVersion);
				}
			}
		}

		private T KeyValue
		{
			get
			{
				KeyValueSchema kvSchema = (KeyValueSchema) schema;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T) kvSchema.decode(KeyBytes, Data, null);
				}
				else
				{
					return schema.decode(Data);
				}
			}
		}

		public virtual long SequenceId
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				if (msgMetadataBuilder.hasSequenceId())
				{
					return msgMetadataBuilder.SequenceId;
				}
				return -1;
			}
		}

		public override string ProducerName
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				if (msgMetadataBuilder.hasProducerName())
				{
					return msgMetadataBuilder.ProducerName;
				}
				return null;
			}
		}

		public virtual ByteBuf DataBuffer
		{
			get
			{
				return payload;
			}
		}

		public override MessageId getMessageId()
		{
			checkNotNull(messageId, "Cannot get the message id of a message that was not received");
			return messageId;
		}

		public override IDictionary<string, string> Properties
		{
			get
			{
				lock (this)
				{
					if (this.properties == null)
					{
						if (msgMetadataBuilder.PropertiesCount > 0)
						{
							this.properties = Collections.unmodifiableMap(msgMetadataBuilder.PropertiesList.ToDictionary(PulsarApi.KeyValue.getKey, PulsarApi.KeyValue.getValue));
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

		public override bool hasProperty(string name)
		{
			return Properties.ContainsKey(name);
		}

		public override string getProperty(string name)
		{
			return this.Properties[name];
		}

		public virtual PulsarApi.MessageMetadata.Builder MessageBuilder
		{
			get
			{
				return msgMetadataBuilder;
			}
		}

		public override bool hasKey()
		{
			checkNotNull(msgMetadataBuilder);
			return msgMetadataBuilder.hasPartitionKey();
		}

		public override string TopicName
		{
			get
			{
				return topic;
			}
		}

		public override string Key
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.PartitionKey;
			}
		}

		public override bool hasBase64EncodedKey()
		{
			checkNotNull(msgMetadataBuilder);
			return msgMetadataBuilder.PartitionKeyB64Encoded;
		}

		public override sbyte[] KeyBytes
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				if (hasBase64EncodedKey())
				{
					return Base64.Decoder.decode(Key);
				}
				else
				{
					return Key.GetBytes(UTF_8);
				}
			}
		}

		public override bool hasOrderingKey()
		{
			checkNotNull(msgMetadataBuilder);
			return msgMetadataBuilder.hasOrderingKey();
		}

		public override sbyte[] OrderingKey
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.OrderingKey.toByteArray();
			}
		}

		public virtual ClientCnx Cnx
		{
			get
			{
				return cnx;
			}
		}

		public virtual void recycle()
		{
			msgMetadataBuilder = null;
			messageId = null;
			topic = null;
			payload = null;
			properties = null;
			schema = null;
			schemaState = SchemaState.None;

			if (recyclerHandle != null)
			{
				recyclerHandle.recycle(this);
			}
		}

		private MessageImpl<T1>(Recycler.Handle<T1> recyclerHandle)
		{
			this.recyclerHandle = recyclerHandle;
			this.redeliveryCount = 0;
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private io.netty.util.Recycler.Handle<MessageImpl<?>> recyclerHandle;
		private Recycler.Handle<MessageImpl<object>> recyclerHandle;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final static io.netty.util.Recycler<MessageImpl<?>> RECYCLER = new io.netty.util.Recycler<MessageImpl<?>>()
		private static readonly Recycler<MessageImpl<object>> RECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : Recycler<MessageImpl<JavaToDotNetGenericWildcard>>
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: @Override protected MessageImpl<?> newObject(io.netty.util.Recycler.Handle<MessageImpl<?>> handle)
			protected internal override MessageImpl<object> newObject<T1>(Recycler.Handle<T1> handle)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new MessageImpl<>(handle);
				return new MessageImpl<object>(handle);
			}
		}

		public virtual bool hasReplicateTo()
		{
			checkNotNull(msgMetadataBuilder);
			return msgMetadataBuilder.ReplicateToCount > 0;
		}

		public virtual IList<string> ReplicateTo
		{
			get
			{
				checkNotNull(msgMetadataBuilder);
				return msgMetadataBuilder.ReplicateToList;
			}
		}

		internal virtual void setMessageId(MessageIdImpl messageId)
		{
			this.messageId = messageId;
		}

		public override Optional<EncryptionContext> EncryptionCtx
		{
			get
			{
				return encryptionCtx;
			}
		}

		public override int RedeliveryCount
		{
			get
			{
				return redeliveryCount;
			}
		}

		internal virtual SchemaState getSchemaState()
		{
			return schemaState;
		}

		internal virtual void setSchemaState(SchemaState schemaState)
		{
			this.schemaState = schemaState;
		}

		internal enum SchemaState
		{
			None,
			Ready,
			Broken
		}
	}

}