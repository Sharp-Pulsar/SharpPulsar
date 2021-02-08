using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Interfaces;
using SharpPulsar.Batch;
using SharpPulsar.Auth;

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
    using Protocol.Proto;
    using System.Linq;
    using Protocol;
    using global::Akka.Actor;
    using global::Akka.Util;
    using SharpPulsar.Precondition;
    using BAMCIS.Util.Concurrent;
    using SharpPulsar.Shared;
    using SharpPulsar.Schemas;

    public class Message<T> : IMessage<T>
	{
		private IMessageId _messageId;
		private IActorRef _cnx;

		public MessageMetadata Metadata { get; set; }
		private byte[] _payload { get; set; }
		private ISchema<T> _schema;
		private SchemaState _schemaState = SchemaState.None;
        private IDictionary<string, string> _properties;
		private readonly int _redeliveryCount;

		private string _topic;

        public Message()
        {

        }
		// Constructor for out-going message
		public static Message<T> Create(MessageMetadata msgMetadata, byte[] payload, ISchema<T> schema)
		{
			var msg = new Message<T>
			{
				Metadata = msgMetadata,
				_properties = new Dictionary<string, string>(),
				_messageId = null,
				_topic = null,
				_payload = payload,
				_cnx = null,
				_schema = schema
			};
			return msg;
		}

		// Constructor for incoming message
		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata,
				byte[] payload, IActorRef cnx, ISchema<T> schema)
		{
			new Message<T>(topic, messageId, msgMetadata, payload, Option<EncryptionContext>.None, cnx, schema);
		}
		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata, byte[] payload,
				Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema)
		{
			new Message<T>(topic, messageId, msgMetadata, payload, encryptionCtx, cnx, schema, 0);
		}
		
		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata, byte[] payload,
				Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount)
		{
            _properties = new Dictionary<string, string>();
			Metadata = msgMetadata;
			_messageId = messageId;
			_topic = topic;
			_cnx = cnx;
			_redeliveryCount = redeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			_payload = payload;
            EncryptionCtx = encryptionCtx;

			if (msgMetadata.Properties.Count > 0)
			{
				msgMetadata.Properties.ToList().ForEach(x => Properties.Add(x.Key, x.Value));
			}
			else
			{
				Properties.Clear();
			}
			_schema = schema;
		}

        public static explicit operator Message<T>(Message<T> v)
        {
            throw new NotImplementedException();
        }

        Message(string topic, BatchMessageId batchMessageId, MessageMetadata msgMetadata,
				SingleMessageMetadata singleMessageMetadata, byte[] payload,
				Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema)
		{
			new Message<T>(topic, batchMessageId, msgMetadata, singleMessageMetadata, payload, encryptionCtx, cnx, schema, 0);
		}
		
		public Message(string topic, BatchMessageId batchMessageId, MessageMetadata msgMetadata,
				SingleMessageMetadata singleMessageMetadata, byte[] payload,
				Option<EncryptionContext> encryptionCtx, IActorRef cnx, ISchema<T> schema, int redeliveryCount)
        {
            Metadata = msgMetadata;
			_messageId = batchMessageId;
			_topic = topic;
			_cnx = cnx;
			_redeliveryCount = redeliveryCount;

			_payload = payload;
			EncryptionCtx = encryptionCtx;

			if (singleMessageMetadata.Properties.Count > 0)
			{
				var properties = new Dictionary<string, string>();
				foreach (var entry in singleMessageMetadata.Properties)
				{
					properties[entry.Key] = entry.Value;
				}
				Properties = properties;
			}
			else
			{
				Properties = new Dictionary<string, string>();
			}

			if (!string.IsNullOrWhiteSpace(singleMessageMetadata.PartitionKey))
			{
				Metadata.PartitionKeyB64Encoded = singleMessageMetadata.PartitionKeyB64Encoded;
				Metadata.PartitionKey = singleMessageMetadata.PartitionKey;
			}
			else if (!string.IsNullOrWhiteSpace(msgMetadata.PartitionKey))
			{
				Metadata.PartitionKey = string.Empty;
				Metadata.PartitionKeyB64Encoded = false;
			}
			if (singleMessageMetadata.OrderingKey.Length > 0)
			{
				Metadata.OrderingKey = singleMessageMetadata.OrderingKey;
			}
			else if (msgMetadata.OrderingKey.Length > 0)
			{
				Metadata.OrderingKey = new byte[0] {};
			}
			if (singleMessageMetadata.EventTime > 0)
			{
				Metadata.EventTime = singleMessageMetadata.EventTime;
			}

			if (singleMessageMetadata.SequenceId > 0)
			{
				Metadata.SequenceId = singleMessageMetadata.SequenceId;
			}

			if (singleMessageMetadata.NullValue)
			{
				Metadata.NullValue = singleMessageMetadata.NullValue;
			}

			if (singleMessageMetadata.NullPartitionKey)
			{
				Metadata.NullPartitionKey = singleMessageMetadata.NullPartitionKey;
			}

			_schema = schema;
		}
		public Message(string topic, string msgId, Dictionary<string, string> properties,
						   byte[] payload, ISchema<T> schema, MessageMetadata msgMetadata)
		{
			string[] data = msgId.Split(":");
			long ledgerId = long.Parse(data[0]);
			long entryId = long.Parse(data[1]);
			if (data.Length == 3)
			{
				_messageId = new BatchMessageId(ledgerId, entryId, -1, int.Parse(data[2]));
			}
			else
			{
				_messageId = new MessageId(ledgerId, entryId, -1);
			}
			_topic = topic;
			_payload = payload;
			_properties = properties;
			_schema = schema;
			_redeliveryCount = 0;
			Metadata = msgMetadata;
		}
		public static Message<byte[]> Deserialize(byte[] headersAndPayload) 
		{			
			var msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
            Message<byte[]> msg = new Message<byte[]>
            {
                Metadata = msgMetadata,
                _properties = new Dictionary<string, string>(),
                _messageId = null,
                _topic = string.Empty,
                _payload = headersAndPayload,
                _cnx = null
            };
            return msg;
		}
		public virtual string ReplicatedFrom
		{
			set
			{
				Condition.CheckNotNull(Metadata);
				Metadata.ReplicatedFrom = value;
			}
			get
			{
				Condition.CheckNotNull(Metadata);
				return Metadata.ReplicatedFrom;
			}
		}

		public virtual bool Replicated
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				return !string.IsNullOrWhiteSpace(Metadata.ReplicatedFrom);
			}
		}


		public virtual long PublishTime
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				return (long)Metadata.PublishTime;
			}
		}

		public virtual long EventTime
		{
			get
			{
				Condition.CheckNotNull(Metadata); ;
				if (Metadata.EventTime > 0)
				{
					return (long)Metadata.EventTime;
				}
				return 0;
			}
		}

		public virtual bool IsExpired(int messageTTLInSeconds)
		{
			return messageTTLInSeconds != 0 && DateTimeHelper.CurrentUnixTimeMillis() > (PublishTime + TimeUnit.SECONDS.ToMilliseconds(messageTTLInSeconds));
		}

		public virtual sbyte[] Data
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				if (Metadata.NullValue)
				{
					return null;
				}

				sbyte[] data = (sbyte[])(object)_payload;
				return data;
			}
		}
		
        public IMessageId MessageId { 
			get => _messageId; 
			set => _messageId = value; 
		}

        public ISchema<T> Schema => _schema;
		public virtual sbyte[] SchemaVersion
		{
			get
			{
				if (Metadata != null && Metadata.SchemaVersion.Length > 0)
				{
					return (sbyte[])(object)Metadata.SchemaVersion;
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
				Condition.CheckNotNull(Metadata);
				if (_schema.SchemaInfo != null && SchemaType.KeyValue == _schema.SchemaInfo.Type)
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
					if (Metadata.NullValue)
					{
						return default(T);
					}
					// check if the schema passed in from client supports schema versioning or not
					// this is an optimization to only get schema version when necessary
					if (_schema.SupportSchemaVersioning())
					{
						sbyte[] schemaVersion = SchemaVersion;
						if (null == schemaVersion)
						{
							return _schema.Decode(Data);
						}
						else
						{
							return _schema.Decode(Data, schemaVersion);
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
				//maybe it works
				var kvSchema = (KeyValueSchema<object,object>)_schema;
				sbyte[] schemaVersion = SchemaVersion;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T)(object)kvSchema.Decode(Metadata.NullPartitionKey ? null : KeyBytes, Metadata.NullValue ? null : Data, schemaVersion);
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
				//maybe it works
				var kvSchema = (KeyValueSchema<object, object>)_schema;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					return (T)(object)kvSchema.Decode(Metadata.NullPartitionKey ? null : KeyBytes, Metadata.NullValue ? null : Data, null);
				}
				else
				{
					return _schema.Decode(Data);
				}
			}
		}

		public virtual long SequenceId
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				if (Metadata.SequenceId >= 0)
				{
					return (long)Metadata.SequenceId;
				}
				return -1;
			}
		}

		public int RedeliveryCount { get => _redeliveryCount; }


		public string ProducerName
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				if (!string.IsNullOrWhiteSpace(Metadata.ProducerName))
				{
					return Metadata.ProducerName;
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
                if (_properties != null) return _properties;
                _properties = Metadata.Properties.Count > 0 ? Metadata.Properties.ToDictionary(x => x.Key, x => x.Value) : new Dictionary<string, string>();
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
			return Properties[name];
		}


		public bool HasKey()
		{
			Condition.CheckNotNull(Metadata);
			return !string.IsNullOrWhiteSpace(Metadata.PartitionKey);
		}


		public string Key
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				return Metadata.PartitionKey;
			}
		}

		public bool HasBase64EncodedKey()
		{
			Condition.CheckNotNull(Metadata);
			return Metadata.PartitionKeyB64Encoded;
		}

		public sbyte[] KeyBytes
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				if (HasBase64EncodedKey())
				{
					return (sbyte[])(object)Convert.FromBase64String(Key);
				}

                return (sbyte[])(object)Encoding.UTF8.GetBytes(Key);
            }
		}

		public bool HasOrderingKey()
		{
			Condition.CheckNotNull(Metadata);
			return Metadata.OrderingKey?.Length > 0;
		}

		public sbyte[] OrderingKey
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				return (sbyte[])(object)Metadata.OrderingKey;
			}
		}

		
		public bool HasReplicateTo()
		{
			Condition.CheckNotNull(Metadata);
			return Metadata.ReplicateToes.Count > 0;
		}

		public IList<string> ReplicateTo
		{
			get
			{
				Condition.CheckNotNull(Metadata);
				return Metadata.ReplicateToes;
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
		//check not null 
		public Option<EncryptionContext> EncryptionCtx { get; }
		public virtual string TopicName
		{
			get
			{
				return _topic;
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