using System;
using System.Collections.Generic;
using SharpPulsar.Util;

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

    public sealed class MessageImpl<T> : Message<T>
	{
        internal IMessageId MessageId;
		public MessageMetadata.Builder MessageBuilder;
		public ClientCnx Cnx;
		public IByteBuffer DataBuffer;
		private ISchema<T> _schema;
		private SchemaState _schemaState = SchemaState.None;
		private EncryptionContext _encryptionCtx;

		public string TopicName {get;} // only set for incoming messages
		public long RedeliveryCount;

		// Constructor for out-going message
		internal static MessageImpl<T> Create(MessageMetadata.Builder msgMetadataBuilder, IByteBuffer payload, ISchema<T> schema)
		{
			var msg = _pool.Take();
			msg.MessageBuilder = msgMetadataBuilder;
			msg.DataBuffer = Unpooled.WrappedBuffer(payload);
			msg._schema = schema;
			return msg;
		}

		// Constructor for incoming message
		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, ClientCnx cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, null, cnx, schema)
		{
		}

		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, EncryptionContext encryptionCtx, ClientCnx cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, 0)
		{
		}

		public MessageImpl(string topic, MessageIdImpl messageId, MessageMetadata msgMetadata, IByteBuffer payload, EncryptionContext encryptionCtx, ClientCnx cnx, ISchema<T> schema, int redeliveryCount)
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

		public MessageImpl(string topic, BatchMessageIdImpl batchMessageIdImpl, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, IByteBuffer payload, EncryptionContext encryptionCtx, ClientCnx cnx, ISchema<T> schema) : this(topic, batchMessageIdImpl, msgMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, 0)
		{
		}

		public MessageImpl(string topic, BatchMessageIdImpl batchMessageIdImpl, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, IByteBuffer payload, EncryptionContext encryptionCtx, ClientCnx cnx, ISchema<T> schema, int redeliveryCount)
		{
			MessageBuilder = MessageMetadata.NewBuilder(msgMetadata);
			MessageId = batchMessageIdImpl;
			TopicName = topic;
			Cnx = cnx;
			RedeliveryCount = redeliveryCount;

			DataBuffer = Unpooled.CopiedBuffer(payload);
			_encryptionCtx = encryptionCtx;

			if (singleMessageMetadata.Properties.Count > 0)
			{
				foreach (var entry in singleMessageMetadata.Properties)
				{
					Properties[entry.Key] = entry.Value;
				}
			}
			else
			{
				Properties.Clear();
			}

			if (singleMessageMetadata.HasPartitionKey)
			{
				MessageBuilder.SetPartitionKeyB64Encoded(singleMessageMetadata.PartitionKeyB64Encoded);
				MessageBuilder.SetPartitionKey(singleMessageMetadata.PartitionKey);
			}

			if (singleMessageMetadata.HasEventTime)
			{
				MessageBuilder.SetEventTime((long)singleMessageMetadata.EventTime);
			}

			if (singleMessageMetadata.HasSequenceId)
			{
				MessageBuilder.SetSequenceId((long)singleMessageMetadata.SequenceId);
			}

			_schema = schema;
		}

		public MessageImpl(string topic, string msgId, IDictionary<string, string> properties, sbyte[] payload, ISchema<T> schema) : this(topic, msgId, properties, Unpooled.WrappedBuffer((byte[])(object)payload), schema)
		{
		}

		public MessageImpl(string topic, string msgId, IDictionary<string, string> properties, IByteBuffer payload, ISchema<T> schema)
		{
			var data = msgId.Split(":", true);
			var ledgerId = long.Parse(data[0]);
			var entryId = long.Parse(data[1]);
			if (data.Length == 3)
			{
				MessageId = new BatchMessageIdImpl(ledgerId, entryId, -1, int.Parse(data[2]));
			}
			else
			{
				MessageId = new MessageIdImpl(ledgerId, entryId, -1);
			}
			TopicName = topic;
			Cnx = null;
			DataBuffer = payload;
			properties.ToList().ForEach(x => Properties.Add(x.Key, x.Value));
			_schema = schema;
			RedeliveryCount = 0;
		}

		public static MessageImpl<T> Deserialize(IByteBuffer headersAndPayload)
		{
			var msg = _pool.Take();
			var msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);

			msg.MessageBuilder = MessageMetadata.NewBuilder(msgMetadata);
			msgMetadata.Recycle();
			msg.DataBuffer = headersAndPayload;
			msg.MessageId = null;
			msg.Cnx = null;
			msg.Properties.Clear();
			return msg;
		}

		public string ReplicatedFrom
		{
			set => MessageBuilder?.SetReplicatedFrom(value);
            get => MessageBuilder != null ? MessageBuilder.GetReplicatedFrom() : string.Empty;
        }

		public bool Replicated => MessageBuilder != null && MessageBuilder.HasReplicatedFrom();


        public long PublishTime => MessageBuilder?.GetPublishTime() ?? 0L;

        public long EventTime
		{
			get
			{
                if (MessageBuilder == null) return 0;
                return MessageBuilder.HasEventTime() ? MessageBuilder.EventTime : 0;
            }
		}

		public bool IsExpired(int messageTtlInSeconds)
		{
			return messageTtlInSeconds != 0 && DateTimeHelper.CurrentUnixTimeMillis() > (PublishTime + BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToMillis(messageTtlInSeconds));
		}

		public sbyte[] Data
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
					var data = new byte[DataBuffer.ReadableBytes];
					DataBuffer.ReadBytes(data);
					return (sbyte[])(object)data;
				}
			}
		}

		public ISchema<T> Schema => _schema;

        public sbyte[] SchemaVersion
		{
			get
			{
				if (MessageBuilder != null && MessageBuilder.HasSchemaVersion())
				{
					return (sbyte[])(object)MessageBuilder.GetSchemaVersion().ToByteArray();
				}
				else
				{
					return null;
				}
			}
		}

		public T Value
		{
			get
			{
				if (_schema.SchemaInfo != null && SchemaType.KEY_VALUE == _schema.SchemaInfo.Type)
				{
					if (_schema.SupportSchemaVersioning())
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
					if (_schema.SupportSchemaVersioning())
					{
						var schemaversion = SchemaVersion;
						if (null == schemaversion)
						{
							return _schema.Decode(Data);
						}
						else
						{
							return _schema.Decode(Data, schemaversion);
						}
					}
					else
					{
						return _schema.Decode(Data);
					}
				}
			}
		}

		private T KeyValueBySchemaVersion
		{
			get
			{
				KeyValueSchema<T> kvSchema = (KeyValueSchema<T>) _schema;
				sbyte[] schemaVersion = SchemaVersion;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T)kvSchema.Decode(KeyBytes, Data, schemaVersion);
				}
				else
				{
					return _schema.Decode(Data, schemaVersion);
				}
			}
		}

		private T KeyValue
		{
			get
			{
				KeyValueSchema<T> kvSchema = (KeyValueSchema<T>) _schema;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T) kvSchema.Decode(KeyBytes, Data, null);
				}
				else
				{
					return _schema.Decode(Data);
				}
			}
		}

		public long SequenceId
		{
			get
            {
                if (MessageBuilder is null)
                    throw new NullReferenceException();
				if (MessageBuilder.HasSequenceId())
				{
					return MessageBuilder.GetSequenceId();
				}
				return -1;
			}
		}

		public string ProducerName
		{
			get
			{
                if (MessageBuilder is null)
                    throw new NullReferenceException();
				if (MessageBuilder.HasProducerName())
				{
					return MessageBuilder.GetProducerName();
				}
				return null;
			}
		}


		public IMessageId GetMessageId()
		{
            if (MessageId is null)
                throw new NullReferenceException("Cannot get the message id of a message that was not received");
			
            return MessageId;
		}

		public IDictionary<string, string> Properties
		{
			get
			{
				lock (this)
				{
					if (Properties == null)
					{
						if (MessageBuilder.PropertiesCount > 0)
						{
							Properties = new Dictionary<string,string>(MessageBuilder.PropertiesList.ToDictionary(x => x.Key, x => x.Value));
						}
						else
						{
							properties = Collections.emptyMap();
						}
					}
					return properties;
				}
			}
            set { properties = value; }
        }

		public bool HasProperty(string name)
		{
			return Properties.ContainsKey(name);
		}

		public string GetProperty(string name)
		{
			return Properties[name];
		}


		public bool HasKey()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.HasPartitionKey();
		}


		public string Key
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

		public sbyte[] KeyBytes
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

		public sbyte[] OrderingKey
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.OrderingKey.toByteArray();
			}
		}


		public void Recycle()
		{
			MessageBuilder = null;
			MessageId = null;
			TopicName = null;
			DataBuffer = null;
			Properties = null;
			_schema = null;
			_schemaState = SchemaState.None;

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
		
		public bool HasReplicateTo()
		{
			checkNotNull(MessageBuilder);
			return MessageBuilder.ReplicateToCount > 0;
		}

		public IList<string> ReplicateTo
		{
			get
			{
				checkNotNull(MessageBuilder);
				return MessageBuilder.ReplicateToList;
			}
		}

		public void SetMessageId(MessageIdImpl messageId)
		{
			messageId = messageId;
		}

        //check not null 
        public EncryptionContext EncryptionCtx => _encryptionCtx;


        public SchemaState? GetSchemaState()
		{
			return _schemaState;
		}

		public void SetSchemaState(SchemaState schemaState)
		{
			_schemaState = schemaState;
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