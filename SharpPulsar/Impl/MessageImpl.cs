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
    using SharpPulsar.Protocol;
    using SharpPulsar.Shared;
    using SharpPulsar.Common.Enum;

    public class MessageImpl<T> : Message<T>
	{

		protected internal IMessageId MessageId;
		public MessageMetadata.Builder MessageBuilder;
		public ClientCnx Cnx;
		public IByteBuffer DataBuffer;
		private ISchema<T> schema;
		private SchemaState schemaState = SchemaState.None;
		private EncryptionContext encryptionCtx;

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

		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, Option<EncryptionContext> encryptionCtx, ClientCnx cnx, ISchema<T> schema, int redeliveryCount)
		{
			MessageBuilder = MessageMetadata.NewBuilder(msgMetadata);
			MessageId = messageId;
			TopicName = topic;
			Cnx = cnx;
			RedeliveryCount = redeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			DataBuffer = Unpooled.CopiedBuffer(payload);
			encryptionCtx = EncryptionCtx;

			if (msgMetadata.Properties.Count > 0)
			{
				msgMetadata.Properties.ToList().ForEach(x => Properties.Add(x.Key, x.Value));
			}
			else
			{
				Properties.Clear();
			}
			schema = Schema;
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, MessageMetadata MsgMetadata, SingleMessageMetadata SingleMessageMetadata, IByteBuffer Payload, EncryptionContext EncryptionCtx, ClientCnx Cnx, ISchema<T> Schema) : this(Topic, BatchMessageIdImpl, MsgMetadata, SingleMessageMetadata, Payload, EncryptionCtx, Cnx, Schema, 0)
		{
		}

		public MessageImpl(string Topic, BatchMessageIdImpl BatchMessageIdImpl, MessageMetadata MsgMetadata, SingleMessageMetadata SingleMessageMetadata, IByteBuffer Payload, EncryptionContext EncryptionCtx, ClientCnx cnx, ISchema<T> Schema, int redeliveryCount)
		{
			MessageBuilder = MessageMetadata.NewBuilder(MsgMetadata);
			MessageId = BatchMessageIdImpl;
			TopicName = Topic;
			Cnx = cnx;
			RedeliveryCount = redeliveryCount;

			DataBuffer = Unpooled.CopiedBuffer(Payload);
			encryptionCtx = EncryptionCtx;

			if (SingleMessageMetadata.Properties.Count > 0)
			{
				foreach (KeyValue Entry in SingleMessageMetadata.Properties)
				{
					Properties[Entry.Key] = Entry.Value;
				}
			}
			else
			{
				Properties.Clear();
			}

			if (SingleMessageMetadata.HasPartitionKey)
			{
				MessageBuilder.SetPartitionKeyB64Encoded(SingleMessageMetadata.PartitionKeyB64Encoded);
				MessageBuilder.SetPartitionKey(SingleMessageMetadata.PartitionKey);
			}

			if (SingleMessageMetadata.HasEventTime)
			{
				MessageBuilder.SetEventTime((long)SingleMessageMetadata.EventTime);
			}

			if (SingleMessageMetadata.HasSequenceId)
			{
				MessageBuilder.SetSequenceId((long)SingleMessageMetadata.SequenceId);
			}

			schema = Schema;
		}

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> Properties, sbyte[] Payload, ISchema<T> Schema) : this(Topic, MsgId, Properties, Unpooled.wrappedBuffer(Payload), Schema)
		{
		}

		public MessageImpl(string Topic, string MsgId, IDictionary<string, string> properties, IByteBuffer Payload, ISchema<T> Schema)
		{
			string[] Data = MsgId.Split(":", true);
			long LedgerId = long.Parse(Data[0]);
			long EntryId = long.Parse(Data[1]);
			if (Data.Length == 3)
			{
				MessageId = new BatchMessageIdImpl(LedgerId, EntryId, -1, int.Parse(Data[2]));
			}
			else
			{
				MessageId = new MessageIdImpl(LedgerId, EntryId, -1);
			}
			TopicName = Topic;
			Cnx = null;
			DataBuffer = Payload;
			properties.ToList().ForEach(x => Properties.Add(x.Key, x.Value));
			schema = Schema;
			RedeliveryCount = 0;
		}

		public static MessageImpl<sbyte[]> Deserialize(IByteBuffer HeadersAndPayload)
		{
			var Msg = _pool.Take();
			MessageMetadata MsgMetadata = Commands.ParseMessageMetadata(HeadersAndPayload);

			Msg.MessageBuilder = MessageMetadata.NewBuilder(MsgMetadata);
			MsgMetadata.Recycle();
			Msg.DataBuffer = HeadersAndPayload;
			Msg.MessageId = null;
			Msg.Cnx = null;
			Msg.Properties.Clear();
			return Msg;
		}

		public virtual string ReplicatedFrom
		{
			set
			{
				if(MessageBuilder != null)
					MessageBuilder.SetReplicatedFrom(value);
			}
			get
			{
				if (MessageBuilder != null)
					return MessageBuilder.getReplicatedFrom();
				else
					return string.Empty;
			}
		}

		public virtual bool Replicated
		{
			get
			{
				if(MessageBuilder != null)
					return MessageBuilder.HasReplicatedFrom();
				return false;
			}
		}


		public virtual long PublishTime
		{
			get
			{
				if(MessageBuilder != null)
					return MessageBuilder.GetPublishTime();
				return 0L;
			}
		}

		public virtual long EventTime
		{
			get
			{
				if(MessageBuilder != null)
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
				if (DataBuffer.ArrayOffset == 0 && DataBuffer.Capacity == DataBuffer.Array.Length)
				{
					return (sbyte[])(object) DataBuffer.Array;
				}
				else
				{
					// Need to copy into a smaller byte array
					sbyte[] Data = new sbyte[DataBuffer.ReadableBytes];
					DataBuffer.ReadBytes(Data, Data.Length);
					return Data;
				}
			}
		}

		public virtual ISchema<T> Schema
		{
			get
			{
				return schema;
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


		public virtual IMessageId GetMessageId()
		{
			checkNotNull(MessageIdConflict, "Cannot get the message id of a message that was not received");
			return MessageId;
		}

		public virtual IDictionary<string, string> Properties
		{
			get
			{
				lock (this)
				{
					if (Properties == null)
					{
						if (MessageBuilder.PropertiesCount > 0)
						{
							Properties = Collections.unmodifiableMap(MessageBuilder.PropertiesList.ToDictionary(KeyValue::getKey, KeyValue::getValue));
						}
						else
						{
							properties = Collections.emptyMap();
						}
					}
					return properties;
				}
			}
		}

		public bool HasProperty(string Name)
		{
			return Properties.ContainsKey(Name);
		}

		public string GetProperty(string Name)
		{
			return Properties[Name];
		}


		public bool HasKey()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.HasPartitionKey();
		}


		public virtual string Key
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.GetPartitionKey();
			}
		}

		public bool HasBase64EncodedKey()
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

		public bool HasOrderingKey()
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
			MessageId = null;
			TopicName = null;
			DataBuffer = null;
			Properties = null;
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
			RedeliveryCount = 0;
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
			MessageId = MessageId;
		}

		public virtual EncryptionContext EncryptionCtx
		{
			get
			{//check not null 
				return encryptionCtx;
			}
		}


		public virtual SchemaState? GetSchemaState()
		{
			return schemaState;
		}

		public virtual void SetSchemaState(SchemaState SchemaState)
		{
			schemaState = SchemaState;
		}

		public enum SchemaState
		{
			None,
			Ready,
			Broken
		}
		public static implicit operator MessageImpl<T>(MessageImpl<object> v)
		{
			return (MessageImpl<T>)Convert.ChangeType(v, typeof(MessageImpl<T>));
		}
		public static implicit operator MessageImpl<object>(MessageImpl<T> v)
		{
			return (MessageImpl<object>)Convert.ChangeType(v, typeof(MessageImpl<object>));
		}
	}

}