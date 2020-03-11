using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SharpPulsar.Akka.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Utility;

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
    using Pulsar.Common.Auth;
    using Protocol.Proto;
    using System.Linq;
    using Protocol;

    public class Message : IMessage
	{
		public MessageMetadata Metadata { get; }
		public byte[] Payload { get; set; }
		private ISchema _schema;
		private SchemaState _schemaState = SchemaState.None;
        private IDictionary<string, string> _properties;

		public string TopicName {get;} // only set for incoming messages

        public Message(byte[] data, MessageMetadata metadata, ISchema schema, string topic)
        {
            Payload = data;
            Metadata = metadata;
            _schema = schema;
            TopicName = topic;
        }
        public Message(byte[] data, MessageMetadata metadata)
        {
            Payload = data;
            Metadata = metadata;
        }
		// Constructor for out-going message
		public static Message Create(MessageMetadata msgMetadata, byte[] payload, ISchema schema, string topic)
		{
            var msg = new Message(payload, msgMetadata, schema, topic);
            return msg;
		}

		// Constructor for incoming message
		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata, byte[] payload, ISchema schema) : this(topic, messageId, msgMetadata, payload, null, schema)
		{
		}

		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata, byte[] payload, EncryptionContext encryptionCtx, ISchema schema) : this(topic, messageId, msgMetadata, payload, encryptionCtx, schema, 0)
		{
			_properties = new Dictionary<string, string>();
		}

		public Message(string topic, MessageId messageId, MessageMetadata msgMetadata, byte[] payload, EncryptionContext encryptionCtx, ISchema schema, int redeliveryCount)
		{
            _properties = new Dictionary<string, string>();
			Metadata = msgMetadata;
			MessageId = messageId;
			TopicName = topic;
			RedeliveryCount = redeliveryCount;

			// Need to make a copy since the passed payload is using a ref-count buffer that we don't know when could
			// release, since the Message is passed to the user. Also, the passed ByteBuf is coming from network and is
			// backed by a direct buffer which we could not expose as a byte[]
			Payload = payload;
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


		public Message(string topic, BatchMessageId batchMessageIdImpl, MessageMetadata msgMetadata, SingleMessageMetadata singleMessageMetadata, byte[] payload, EncryptionContext encryptionCtx, ISchema schema, int redeliveryCount)
		{
			Metadata = msgMetadata;
			MessageId = batchMessageIdImpl;
			TopicName = topic;
			RedeliveryCount = redeliveryCount;

			Payload = payload;
			EncryptionCtx = encryptionCtx;

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

			if (!string.IsNullOrWhiteSpace(singleMessageMetadata.PartitionKey))
			{
				Metadata.PartitionKeyB64Encoded = (singleMessageMetadata.PartitionKeyB64Encoded);
				Metadata.PartitionKey = (singleMessageMetadata.PartitionKey);
			}

			if (singleMessageMetadata.EventTime > 0)
			{
				Metadata.EventTime = (singleMessageMetadata.EventTime);
			}

			if (singleMessageMetadata.SequenceId > 0)
			{
				Metadata.SequenceId = (singleMessageMetadata.SequenceId);
			}

			_schema = schema;
		}

		public Message(string topic, string msgId, IDictionary<string, string> properties, byte[] payload, ISchema schema)
		{
			var data = msgId.Split(":", true);
			var ledgerId = long.Parse(data[0]);
			var entryId = long.Parse(data[1]);
			MessageId = data.Length == 3 ? new BatchMessageId(ledgerId, entryId, -1, int.Parse(data[2])) : new MessageId(ledgerId, entryId, -1);
			TopicName = topic;
			Payload = payload;
			properties.ToList().ForEach(x => Properties.Add(x.Key, x.Value));
			_schema = schema;
			RedeliveryCount = 0;
		}

		public static Message Deserialize(byte[] headersAndPayload)
		{
            var msgMetadata = Commands.ParseMessageMetadata(headersAndPayload);
            var msg = new Message(headersAndPayload, msgMetadata) {MessageId = null};
            msg.Properties.Clear();
			return msg;
		}

		public string ReplicatedFrom
		{
			set => Metadata.ReplicatedFrom = (value);
            get => Metadata != null ? Metadata.ReplicatedFrom : string.Empty;
        }

		public bool Replicated => Metadata != null && !string.IsNullOrWhiteSpace(Metadata.ReplicatedFrom);


        public IMessageId MessageId { get; set; }
        public long PublishTime => (long)Metadata.PublishTime;

        public long EventTime
		{
			get
			{
                if (Metadata == null) return 0;
                return (long)Metadata.EventTime;
            }
		}

		public bool IsExpired(int messageTtlInSeconds)
		{
			return messageTtlInSeconds != 0 && DateTimeHelper.CurrentUnixTimeMillis() > (PublishTime + BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToMillis(messageTtlInSeconds));
		}

        public MessageIdReceived ReceivedId
        {
            get
            {
				if(MessageId is BatchMessageId id)
				    return new MessageIdReceived(id.LedgerId, id.EntryId, id.BatchIndex, id.PartitionIndex);
                var m = (MessageId) MessageId;
                return new MessageIdReceived(m.LedgerId, m.EntryId, -1, m.PartitionIndex);
            }
        } 
		public sbyte[] Data => (sbyte[])(object)Payload;

        public ISchema Schema => _schema;

        public int RedeliveryCount { get; }

        public sbyte[] SchemaVersion
		{
			get
            {
                if (Metadata != null && Metadata.SchemaVersion?.Length > 0)
				{
					return (sbyte[])(object)Metadata.SchemaVersion;
				}

                return null;
            }
		}

        public T ToTypeOf<T>()
        {
            // check if the schema passed in from client supports schema versioning or not
            // this is an optimization to only get schema version when necessary
            if (_schema.SupportSchemaVersioning())
            {
                var schemaversion = SchemaVersion;
                if (schemaversion == null)
                {
                    return (T)_schema.Decode(Data, typeof(T));
                }
                return (T)_schema.Decode(Data, schemaversion, typeof(T));
            }

            return (T)_schema.Decode(Data, typeof(T));
		}

        public object Value => Data;

        public long SequenceId
		{
			get
            {
                if (Metadata is null)
                    throw new NullReferenceException();
                return (long)Metadata.SequenceId;
			}
		}

		public string ProducerName
		{
			get
			{
                if (Metadata is null)
                    throw new NullReferenceException();
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
			if(Metadata == null)
				throw  new NullReferenceException();
			return !string.IsNullOrWhiteSpace(Metadata.PartitionKey);
		}


		public string Key
		{
			get
			{
                if (Metadata == null)
                    throw new NullReferenceException();
				return Metadata.PartitionKey;
			}
		}

		public bool HasBase64EncodedKey()
		{
            if (Metadata == null)
                throw new NullReferenceException();
			return Metadata.PartitionKeyB64Encoded;
		}

		public sbyte[] KeyBytes
		{
			get
			{
				if (Metadata == null)
					throw new NullReferenceException();
				if (HasBase64EncodedKey())
				{
					return (sbyte[])(object)Convert.FromBase64String(Key);
				}

                return (sbyte[])(object)Encoding.UTF8.GetBytes(Key);
            }
		}

		public bool HasOrderingKey()
		{
            if (Metadata == null)
                throw new NullReferenceException();
			return Metadata.OrderingKey?.Length > 0;
		}

		public sbyte[] OrderingKey
		{
			get
			{
                if (Metadata == null)
                    throw new NullReferenceException();
				return (sbyte[])(object)Metadata.OrderingKey;
			}
		}

		
		public bool HasReplicateTo()
		{
            if (Metadata == null)
                throw new NullReferenceException();
			return Metadata.ReplicateToes.Count > 0;
		}

		public IList<string> ReplicateTo
		{
			get
			{
                if (Metadata == null)
                    throw new NullReferenceException();
				return Metadata.ReplicateToes;
			}
		}

		public void SetMessageId(MessageId messageId)
		{
			MessageId = messageId;
		}

        //check not null 
        public EncryptionContext EncryptionCtx { get; }


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