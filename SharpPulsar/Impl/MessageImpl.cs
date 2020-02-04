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
    using DotNetty.Buffers;
    using Optional;
    using DotNetty.Common;
    using SharpPulsar.Api;
    using Pulsar.Common.Auth;
    using SharpPulsar.Protocol.Proto;
    using System.Linq;
    using SharpPulsar.Impl.Schema;

    public class MessageImpl<T> : Message<T>
	{

		protected internal IMessageId MessageId;
		public ITypedMessageBuilder<T> MessageBuilder;
		public ClientCnx Cnx;
		public IByteBuffer DataBuffer;
		private ISchema<T> schema;
		private SchemaState schemaState = SchemaState.None;
		private Option<EncryptionContext> encryptionCtx;

		public string TopicName {get;} // only set for incoming messages
		public long RedeliveryCount;

		// Constructor for out-going message
		internal static MessageImpl<T> Create(MessageMetadata.Builder msgMetadataBuilder, IByteBuffer payload, ISchema<T> schema)
		{
			MessageImpl<T> msg = _pool.Take();
			msg.MessageBuilder = msgMetadataBuilder;
			msg.DataBuffer = Unpooled.WrappedBuffer(payload);
			msg.schema = schema;
			return msg;
		}

		// Constructor for incoming message
		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, ClientCnx cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, null, cnx, schema)
		{
		}

		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, Option<EncryptionContext> encryptionCtx, ClientCnx cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, 0)
		{
		}

		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, Option<EncryptionContext> encryptionCtx, ClientCnx cnx, ISchema<T> schema, int rredeliveryCount)
		{
			this.MessageBuilder = MessageMetadata.NewBuilder(msgMetadata);
			this.MessageId = MessageId;
			this.TopicName = topic;
			this.Cnx = Cnx;
			this.RedeliveryCount = RedeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			this.DataBuffer = Unpooled.CopiedBuffer(payload);
			this.encryptionCtx = EncryptionCtx;

			if (msgMetadata.Properties.Count > 0)
			{
				Properties = msgMetadata.Properties.ToDictionary(x => x.Key, x=> x.Value);
			}
			else
			{
				properties = Collections.emptyMap();
			}
			this.schema = Schema;
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, MessageMetadata MsgMetadata, SingleMessageMetadata SingleMessageMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, ISchema<T> Schema) : this(Topic, BatchMessageIdImpl, MsgMetadata, SingleMessageMetadata, Payload, EncryptionCtx, Cnx, Schema, 0)
		{
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, MessageMetadata MsgMetadata, SingleMessageMetadata SingleMessageMetadata, ByteBuf Payload, Optional<EncryptionContext> EncryptionCtx, ClientCnx Cnx, ISchema<T> Schema, int RedeliveryCount)
		{
			this.MessageBuilder = MessageMetadata.NewBuilder(MsgMetadata);
			this.MessageIdConflict = BatchMessageIdImpl;
			this.TopicName = Topic;
			this.Cnx = Cnx;
			this.RedeliveryCount = RedeliveryCount;

			this.DataBuffer = Unpooled.CopiedBuffer(Payload);
			this.encryptionCtx = EncryptionCtx;

			if (SingleMessageMetadata.PropertiesCount > 0)
			{
				IDictionary<string, string> Properties = Maps.newTreeMap();
				foreach (KeyValue Entry in SingleMessageMetadata.PropertiesList)
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

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> Properties, sbyte[] Payload, ISchema<T> Schema) : this(Topic, MsgId, Properties, Unpooled.wrappedBuffer(Payload), Schema)
		{
		}

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> Properties, ByteBuf Payload, ISchema<T> Schema)
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

		public static MessageImpl<sbyte[]> Deserialize(ByteBuf HeadersAndPayload)
		{
			MessageImpl<sbyte[]> Msg = (MessageImpl<sbyte[]>) RECYCLER.get();
			MessageMetadata MsgMetadata = Commands.parseMessageMetadata(HeadersAndPayload);

			Msg.MessageBuilder = MessageMetadata.newBuilder(MsgMetadata);
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

		public virtual ISchema<T> Schema
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
							this.properties = Collections.unmodifiableMap(MessageBuilder.PropertiesList.ToDictionary(KeyValue::getKey, KeyValue::getValue));
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

			if (_handle != null)
			{
				_handle.Release(this);
			}
		}
		private static ThreadLocalPool<MessageImpl<T>> _pool = 	new ThreadLocalPool<MessageImpl<T>>(handle => new MessageImpl<T>(handle), 1, true);
		private ThreadLocalPool.Handle _handle;
		private MessageImpl(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
			this.RedeliveryCount = 0;
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

		public virtual Option<EncryptionContext> EncryptionCtx
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