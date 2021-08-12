using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using SharpPulsar.Interfaces;
using SharpPulsar.Batch;
using SharpPulsar.Auth;
using SharpPulsar.Common.Naming;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Schema;

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
///

namespace SharpPulsar
{
    using Protocol.Proto;
    using System.Linq;
    using Akka.Actor;
    using Akka.Util;
    using Shared;
    using Schemas;
    using Extension;
    using System.Buffers;
    using Schema;

    public sealed class Message<T> : IMessage<T>
	{
		private  IMessageId _messageId;
		private  IActorRef _cnx;

		private  MessageMetadata _metadata;
        private  ReadOnlySequence<byte>? _payload;
		private  ISchema<T> _schema;
		private SchemaState _schemaState = SchemaState.None;
        private IDictionary<string, string> _properties;
		private int _redeliveryCount;
        private int _uncompressedSize;
        private bool _poolMessage;

        private string _topic;
        private BrokerEntryMetadata _brokerEntryMetadata;
        public MessageMetadata Metadata => _metadata;
        private Message(){}
		public Message(MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, ISchema<T> schema)
        {
            _metadata = null;
            _topic = null;
            _cnx = null;
            _properties = null;
			_metadata = msgMetadata;
			_payload = payload;
			_schema = schema;
            _uncompressedSize = (int)payload.Length;

        }
		// Constructor for out-going message
		public static Message<T> Create(MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, ISchema<T> schema)
		{
			return new Message<T>(msgMetadata, payload, schema);
		}

        // Constructor for incoming message
        internal Message(string topic, MessageId messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, IActorRef cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, null, cnx, schema)
        {
        }

        internal Message(string topic, MessageId messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema) : this(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, 0, false)
        {
        }
        internal Message(string topic, MessageId messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool pooledMessage)
        {
            _metadata = new MessageMetadata();
            Init(this, topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, pooledMessage);
        }

        public static Message<T> Create(string topic, MessageId messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool pooledMessage)
        {
            if (pooledMessage)
            {
                var msg = new Message<T>();
                Init(msg, topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, pooledMessage);
                return msg;
            }

            return new Message<T>(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, pooledMessage);
        }
        public static Message<T> Create(string topic, BatchMessageId batchMessageIdImpl, MessageMetadata batchMetadata, SingleMessageMetadata singleMessageMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool pooledMessage)
        {
            if (pooledMessage)
            {
                var msg = new Message<T>();
                Init(msg, topic, batchMessageIdImpl, batchMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, pooledMessage);
                return msg;
            }

            return new Message<T>(topic, batchMessageIdImpl, batchMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, pooledMessage);
        }
        internal Message(string topic, BatchMessageId batchMessageIdImpl, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema) : this(topic, batchMessageIdImpl, msgMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, 0, false)
        {
        }

        internal Message(string topic, BatchMessageId batchMessageIdImpl, MessageMetadata batchMetadata, SingleMessageMetadata singleMessageMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool keepMessageInDirectMemory)
        {
            _metadata = new MessageMetadata();
            Init(this, topic, batchMessageIdImpl, batchMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, redeliveryCount, keepMessageInDirectMemory);

        }
        
        internal static void Init(Message<T> msg, string topic, MessageId messageId, MessageMetadata msgMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool poolMessage)
        {
            Init(msg, topic, null, msgMetadata, null, payload, encryptionCtx, cnx, schema, redeliveryCount, poolMessage);
            msg._messageId = messageId;
        }
        private static void Init(Message<T> msg, string topic, BatchMessageId batchMessageIdImpl, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, ReadOnlySequence<byte> payload, Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount, bool poolMessage)
        {
            msg._metadata = null;
            msg._metadata = msgMetadata;
            msg._messageId = batchMessageIdImpl;
            msg._topic = topic;
            msg._cnx = cnx;
            msg._redeliveryCount = redeliveryCount;
            msg._encryptionCtx = encryptionCtx;
            msg._schema = schema;

            msg._poolMessage = poolMessage;
            // If it's not pool message then need to make a copy since the passed payload is 
            // using a ref-count buffer that we don't know when could release, since the 
            // Message is passed to the user. Also, the passed ByteBuf is coming from network 
            // and is backed by a direct buffer which we could not expose as a byte[]
            msg._payload = payload;//poolMessage ? payload.retain() : Unpooled.copiedBuffer(payload);

            if (singleMessageMetadata != null)
            {
                if (singleMessageMetadata.Properties.Count > 0)
                {
                    IDictionary<string, string> properties = new Dictionary<string, string>();
                    foreach (var entry in singleMessageMetadata.Properties)
                    {
                        properties[entry.Key] = entry.Value;
                    }
                    msg.Properties = properties.ToImmutableDictionary();
                }
                else
                {
                    msg.Properties = new Dictionary<string, string>();
                }
                if (singleMessageMetadata.ShouldSerializePartitionKey())
                {
                    msg._metadata.PartitionKeyB64Encoded = singleMessageMetadata.PartitionKeyB64Encoded;
                    msg._metadata.PartitionKey = singleMessageMetadata.PartitionKey;
                }
                else if (msg._metadata.ShouldSerializePartitionKey())
                {
                    msg._metadata.PartitionKey = string.Empty;
                    msg._metadata.PartitionKeyB64Encoded = false;
                }
                if (singleMessageMetadata.ShouldSerializeOrderingKey())
                {
                    msg._metadata.OrderingKey = singleMessageMetadata.OrderingKey;
                }
                else if (msgMetadata.ShouldSerializeOrderingKey())
                {
                    msg._metadata.OrderingKey = null;
                }

                if (singleMessageMetadata.ShouldSerializeEventTime())
                {
                    msg._metadata.EventTime = singleMessageMetadata.EventTime;
                }

                if (singleMessageMetadata.ShouldSerializeSequenceId())
                {
                    msg._metadata.SequenceId = singleMessageMetadata.SequenceId;
                }

                if (singleMessageMetadata.ShouldSerializeNullValue())
                {
                    msg._metadata.NullValue = singleMessageMetadata.NullValue;
                }

                if (singleMessageMetadata.ShouldSerializeNullPartitionKey())
                {
                    msg._metadata.NullPartitionKey = singleMessageMetadata.NullPartitionKey;
                }
            }
            else if (msgMetadata.Properties.Count > 0)
            {
                msg.Properties = msgMetadata.Properties.ToDictionary(x => x.Key, x=> x.Value).ToImmutableDictionary();
            }
            else
            {
                msg.Properties = new Dictionary<string, string>();
            }
        }
        public Message(string topic, string msgId, IDictionary<string, string> properties, byte[] payload, ISchema<T> schema, MessageMetadata msgMetadata) : this(topic, msgId, properties, new ReadOnlySequence<byte>(payload), schema, msgMetadata)
        {
        }
        public Message(string topic, string msgId, IDictionary<string, string> properties, ReadOnlySequence<byte> payload, ISchema<T> schema, MessageMetadata msgMetadata)
        {
            var data = msgId.Split(":");
            var ledgerId = long.Parse(data[0]);
            var entryId = long.Parse(data[1]);
            if (data.Length == 3)
            {
                _messageId = new BatchMessageId(ledgerId, entryId, -1, int.Parse(data[2]));
            }
            else
            {
                _messageId = new MessageId(ledgerId, entryId, -1);
            }
            _topic = topic;
            _cnx = null;
            _payload = payload;
            _properties = properties.ToImmutableDictionary();
            _schema = schema;
            _redeliveryCount = 0;
            _metadata = msgMetadata;
        }
        public static Message<byte[]> DeserializeBrokerEntryMetaDataFirst(ReadOnlySequence<byte> headersAndPayloadWithBrokerEntryMetadata)
        {
            var msg = new Message<byte[]>
            {
                _brokerEntryMetadata =
                    Commands.ParseBrokerEntryMetadataIfExist(headersAndPayloadWithBrokerEntryMetadata)
            };


            if (msg._brokerEntryMetadata != null)
            {
                msg._metadata = null;
                msg._payload = null;
                msg._messageId = null;
                msg._topic = null;
                msg._cnx = null;
                msg._properties = new Dictionary<string, string>();
                return msg;
            }

            Commands.ParseMessageMetadata(headersAndPayloadWithBrokerEntryMetadata/*, msg.Metadata*/);
            msg._payload = headersAndPayloadWithBrokerEntryMetadata;
            msg._messageId = null;
            msg._topic = null;
            msg._cnx = null;
            msg._properties = new Dictionary<string, string>();
            return msg;
        }
        public string ReplicatedFrom
		{
			set
			{
                _metadata.ReplicatedFrom = value;
			}
			get
			{
				if (Replicated)
                    return _metadata.ReplicatedFrom;

                return null;
			}
		}
		public bool Replicated
		{
			get
			{
                return _metadata.ShouldSerializeReplicatedFrom();
			}
		}

		public long PublishTime
		{
			get
			{
				return (long)_metadata.PublishTime;
			}
		}
		public long EventTime
		{
			get
			{
                if(_metadata.ShouldSerializeEventTime())
				    return (long)_metadata.EventTime;
                return 0;
            }
		}

        public ISchema<T> SchemaInternal()
        {
            return _schema;
        }
        public bool IsExpired(int messageTtlInSeconds)
        {
            return messageTtlInSeconds != 0 && (_brokerEntryMetadata == null || !_brokerEntryMetadata.ShouldSerializeBrokerTimestamp() ? (DateTimeHelper.CurrentUnixTimeMillis() > PublishTime + TimeSpan.FromSeconds(messageTtlInSeconds).TotalMilliseconds) : DateTimeHelper.CurrentUnixTimeMillis() > _brokerEntryMetadata.BrokerTimestamp + TimeSpan.FromSeconds(messageTtlInSeconds).TotalMilliseconds);
        }
        public bool PublishedEarlierThan(long timestamp)
        {
            return _brokerEntryMetadata == null || !_brokerEntryMetadata.ShouldSerializeBrokerTimestamp()? PublishTime < timestamp : (long)_brokerEntryMetadata.BrokerTimestamp < timestamp;
        }
        public ReadOnlySequence<byte> Data
		{
			get
            {
                if (_metadata.ShouldSerializeNullValue())
                {
					return ReadOnlySequence<byte>.Empty;
				}

                if (_payload != null) return _payload.Value;

                return ReadOnlySequence<byte>.Empty;
            }
		}
        public long Size()
        {
            if (_metadata.NullValue)
            {
                return 0;
            }
            return _payload.HasValue? _payload.Value.Length: 0;
        }
        public IMessageId MessageId { 
			get => _messageId; 
			set => _messageId = value; 
		}

        public ISchema<T> Schema => _schema;
		public byte[] SchemaVersion
		{
			get
            {
                if (_metadata.ShouldSerializeSchemaVersion())
				{
					return _metadata.SchemaVersion;
				}

                return null;
            }
		}

		public T Value
		{
			get
			{
				if (_schema.SchemaInfo != null && SchemaType.KeyValue == _schema.SchemaInfo.Type)
                {
                    if (_schema.SupportSchemaVersioning())
					{
						return KeyValueBySchemaVersion;
					}

                    return KeyValue;
                }

                if (_metadata.NullValue)
                {
                    return default(T);
                }
                // check if the schema passed in from client supports schema versioning or not
                // this is an optimization to only get schema version when necessary
                return Decode(_schema.SupportSchemaVersioning() ? SchemaVersion : null);
            }
		}
        private T Decode(byte[] schemaVersion)
        {
            //T value = _poolMessage ? schema.decode(payload.nioBuffer(), schemaVersion) : default(T);
            /*T value = _poolMessage ? schema.decode(payload.nioBuffer(), schemaVersion) : default(T);
            if (value != null)
            {
                return value;
            }*/
            if (schemaVersion == null)
            {
                return _schema.Decode(Data.ToArray());
            }

            return _schema.Decode(Data.ToArray(), schemaVersion);
        }
        private T KeyValueBySchemaVersion
		{
			get
			{
				var schemaType = _schema.GetType();
				var keyValueEncodingType = (KeyValueEncodingType)schemaType.GetProperty("KeyValueEncodingType")?.GetValue(_schema, null);
				
				var schemaVersion = SchemaVersion;
				if (keyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					var decode = schemaType
						.GetMethods()
						.Where(x=> x.Name == "Decode")
						.FirstOrDefault(x=> x.GetParameters().Length == 3);
					var k = _metadata.NullPartitionKey ? null : KeyBytes;
					var v = Data.ToArray();
					return (T)decode.Invoke(_schema, new object[] { k, v, schemaVersion });
				}
                return _schema.Decode(Data.ToArray(), schemaVersion);
            }
		}
		
		private T KeyValue
		{
			get
			{
				var schemaType = _schema.GetType();
				var keyValueEncodingType =
                    (KeyValueEncodingType) schemaType.GetProperty("KeyValueEncodingType")?.GetValue(_schema, null);

				var kvSchema = (KeyValueSchema<object, object>)_schema;
				if (keyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					var decode = schemaType
						.GetMethods()
						.Where(x => x.Name == "Decode")
						.FirstOrDefault(x => x.GetParameters().Length == 3);
					var k = _metadata.NullPartitionKey ? null : KeyBytes;
					var v = Data.ToArray();
					return (T)decode.Invoke(_schema, new object[] { k, v, null });
				}
				else
				{
					return _schema.Decode(Data.ToArray());
				}
			}
		}

		public long SequenceId
		{
			get
			{
				if(_metadata.SequenceId >= 0)
                {
                    return (long)_metadata.SequenceId;
                }
				return -1;
			}
		}

		public int RedeliveryCount { get => _redeliveryCount; }

		public string ProducerName
		{
			get
			{
                if (!string.IsNullOrWhiteSpace(_metadata.ProducerName))
                {
                    return _metadata.ProducerName;
                }
                return null;
            }
		}
        public BrokerEntryMetadata BrokerEntryMetadata
        {
            get
            {
                return _brokerEntryMetadata;
            }
            set
            {
                _brokerEntryMetadata = value;
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
                if (_properties?.Count > 0) return _properties;
                _properties = _metadata.Properties.Count > 0 ? _metadata.Properties.ToDictionary(x => x.Key, x => x.Value).ToImmutableDictionary() : ImmutableDictionary<string, string>.Empty;
                return _properties;
			}
            set => _properties = value;
        }

		public bool HasProperty(string name)
		{
			return Properties.ContainsKey(name);
		}

		public string GetProperty(string name)
		{
			return Properties.GetValueOrNull(name);
		}

        internal int UncompressedSize
        {
            get
            {
                return _uncompressedSize;
            }
        }
        public bool HasKey()
        {
            return _metadata.ShouldSerializeNullPartitionKey();
        }

		public string Key
		{
			get
			{
				return  _metadata.PartitionKey;
			}
		}

        public Option<ISchema<T>> ReaderSchema()
        {
            EnsureSchemaIsLoaded();
            if (_schema == null)
            {
                return Option<ISchema<T>>.None;
            }
            if (_schema is AutoConsumeSchema) 
            {
                var schemaVersion = SchemaVersion;
                //return new Option<ISchema<T>>(((AutoConsumeSchema)_schema).AtSchemaVersion(schemaVersion));
                return new Option<ISchema<T>>();
            } 
            else if (_schema is AbstractSchema<T> schema) 
            {
               var schemaVersion = SchemaVersion;
                return new Option<ISchema<T>>(schema.AtSchemaVersion(schemaVersion));
            } 
            else
            {
                return new Option<ISchema<T>>(_schema); 
            }
        }
        private void EnsureSchemaIsLoaded()
        {
            if (_schema is AutoConsumeSchema schema) 
            {
                schema.FetchSchemaIfNeeded();
            }
            else if (_schema is KeyValueSchema<object,object> kv)
            {
                //kv.FetchSchemaIfNeeded(_topic, BytesSchemaVersion.Of(SchemaVersion));
            }
        }
        public bool HasBase64EncodedKey()
		{
			return _metadata.ShouldSerializePartitionKey();
		}

		public byte[] KeyBytes
		{
			get
			{
				if (HasBase64EncodedKey())
				{
					return Convert.FromBase64String(Key);
				}

				return Encoding.UTF8.GetBytes(Key);
			}
		}

		public bool HasOrderingKey()
		{
			
			return OrderingKey?.Length > 0;
		}
		
		public byte[] OrderingKey
		{
			get
			{
                return _metadata.OrderingKey;
			}
		}

		
		public bool HasReplicateTo()
		{
			return ReplicateTo.Count > 0;
		}
		
		public IList<string> ReplicateTo
		{
			get
			{
				return _metadata.ReplicateToes;
			}
		}

		public void SetMessageId(MessageId messageId)
		{
			MessageId = messageId;
		}
		public IActorRef Cnx()
		{
			return _cnx;
		}
        public void Recycle()
        {
            _metadata = null;
            _brokerEntryMetadata = null;
            _cnx = null;
            _messageId = null;
            _topic = null;
            _payload = null;
            _encryptionCtx = null;
            _redeliveryCount = 0;
            _uncompressedSize = 0;
            _properties = null;
            _schema = null;
            _schemaState = SchemaState.None;
            _poolMessage = false;
        }
        //check not null 
        private Option<EncryptionContext> _encryptionCtx;
        public Option<EncryptionContext> EncryptionCtx => _encryptionCtx;
		public string Topic
		{
			get
			{
				return _topic;
			}
            set
            {
                _topic = value;
            }
		}

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
		
	}
}