
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Extension;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;

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
    using global::Akka.Actor;
    using SharpPulsar.Configuration;
    using SharpPulsar.Exceptions;
    using SharpPulsar.Interfaces;
    using SharpPulsar.Messages.Producer;
    using SharpPulsar.Messages.Transaction;
    using SharpPulsar.Precondition;
    using SharpPulsar.Schemas;
    using System.Buffers;
    using System.Threading.Tasks;

    [Serializable]
	public class TypedMessageBuilder<T> : ITypedMessageBuilder<T>
	{
        [NonSerialized]
        private readonly IActorRef _producer;//topic
        [NonSerialized]
        private readonly MessageMetadata _metadata;
        [NonSerialized]
        private readonly ISchema<T> _schema;
        [NonSerialized]
        private ReadOnlySequence<byte> _content;
        [NonSerialized]
        private readonly User.Transaction _txn;
        [NonSerialized]
        private readonly ProducerConfigurationData _conf;

		public TypedMessageBuilder(IActorRef producer, ISchema<T> schema, ProducerConfigurationData conf) : this(producer, schema, null, conf)
		{
		}

		public TypedMessageBuilder(IActorRef producer, ISchema<T> schema, User.Transaction txn, ProducerConfigurationData conf)
		{
            _metadata = new MessageMetadata();
            _conf = conf;
			_producer = producer;
			_schema = schema;
			_content = ReadOnlySequence<byte>.Empty;
			_txn = txn;
		}

		private async Task<long> BeforeSend()
		{
			if (_txn == null)
			{
				return -1L;
			}
			
			var bits = await _txn.Txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance).ConfigureAwait(false);
			var sequence = await _txn.Txn.Ask<long>(NextSequenceId.Instance).ConfigureAwait(false);
			_metadata.TxnidLeastBits = (ulong)bits.LeastBits;
			_metadata.TxnidMostBits = (ulong)bits.MostBits;
			var sequenceId = sequence;
			_metadata.SequenceId = (ulong)sequenceId;
			return sequenceId;
		}
		public MessageId Send()
		{
			return SendAsync().GetAwaiter().GetResult();
		}
		public async ValueTask<MessageId> SendAsync()
		{
            try
            {
                var message = await Message().ConfigureAwait(false);
                object obj = null;
                if (_txn != null)
                {
                    obj = await _producer.Ask(new InternalSendWithTxn<T>(message, _txn.Txn), TimeSpan.FromMilliseconds(_conf.SendTimeoutMs)).ConfigureAwait(false);
                }
                else
                {
                    //obj = await _producer.Ask(new InternalSend<T>(message), TimeSpan.FromMilliseconds(_conf.SendTimeoutMs)).ConfigureAwait(false);
                    obj = await _producer.Ask(new InternalSend<T>(message)).ConfigureAwait(false);
                }
                switch(obj)
                {
                    case MessageId msgid:
                        return msgid;
                    case NoMessageIdYet _:
                        return null;
                    case PulsarClientException ex:
                        throw ex;
                    default:
                        throw new Exception(obj.ToString());
                }
            }
            catch
            {
                throw;
            }
		}
		public ITypedMessageBuilder<T> Key(string key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
                var schemaType = _schema.GetType();
                var keyValueEncodingType = (KeyValueEncodingType)schemaType.GetProperty("KeyValueEncodingType")?.GetValue(_schema, null);

                Condition.CheckArgument(!(keyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when the encoding type is not SEPARATED");
				if (string.IsNullOrWhiteSpace(key))
				{
					_metadata.NullPartitionKey = true;
					return this;
				}
			}
			_metadata.PartitionKey = key;
			_metadata.PartitionKeyB64Encoded = false;
			return this;
		}
		public ITypedMessageBuilder<T> KeyBytes(byte[] key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
                var schemaType = _schema.GetType();
                var keyValueEncodingType = (KeyValueEncodingType)schemaType.GetProperty("KeyValueEncodingType")?.GetValue(_schema, null);

                Condition.CheckArgument(!(keyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when the encoding type is not SEPARATED");
				if (key == null)
				{
					_metadata.NullPartitionKey = true;
					return this;
				}
			}
			_metadata.PartitionKey = Convert.ToBase64String(key);
			_metadata.PartitionKeyB64Encoded = true;
			return this;
		}
		public ITypedMessageBuilder<T> OrderingKey(byte[] orderingKey)
		{
			_metadata.OrderingKey = orderingKey;
			return this;
		}
        /// <summary>
        /// For KeyValueSchema, please make use of Value<TK, TV>(T value)
        /// to supply the key and value type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public ITypedMessageBuilder<T> Value(T value)
		{
			if (value == null)
			{
				_metadata.NullValue = true;
				return this;
			}
			if (_schema.SchemaInfo != null && _schema.SchemaInfo.Type == SchemaType.KeyValue)
            {
                throw new Exception("Get method only support non keyvalue schema");
            }
			_content = new ReadOnlySequence<byte>(_schema.Encode(value));
			return this;
		}

        public ITypedMessageBuilder<T> Value<TK, TV>(T value)
        {
            if (_schema.SchemaInfo != null && _schema.SchemaInfo.Type == SchemaType.KeyValue)
            {
                var schemaType = _schema.GetType();
                var keyValueEncodingType = (KeyValueEncodingType)schemaType.GetProperty("KeyValueEncodingType")?.GetValue(_schema, null);
                var keySchema = (ISchema<TK>)schemaType.GetProperty("KeySchema")?.GetValue(_schema, null);
                var valueSchema = (ISchema<TV>)schemaType.GetProperty("ValueSchema")?.GetValue(_schema, null);

                var kv = (KeyValue<TK, TV>)(object)value;
                if (keyValueEncodingType == KeyValueEncodingType.SEPARATED)
                {
                    // set key as the message key
                    if (kv.Key != null)
                    {
                        _metadata.PartitionKey = Convert.ToBase64String(keySchema.Encode(kv.Key));
                        _metadata.PartitionKeyB64Encoded = true;
                    }
                    else
                    {
                        _metadata.NullPartitionKey = true;
                    }

                    // set value as the payload
                    if (kv.Value != null)
                    {
                        _content = new ReadOnlySequence<byte>(valueSchema.Encode(kv.Value));
                    }
                    else
                    {
                        _metadata.NullValue = true;
                    }
                    return this;
                }
            }
            _content = new ReadOnlySequence<byte>(_schema.Encode(value));
            return this;
        }
        public ITypedMessageBuilder<T> Property(string name, string value)
		{
			Condition.CheckArgument(!string.IsNullOrWhiteSpace(name), "Need Non-Null name");
			Condition.CheckArgument(!string.IsNullOrWhiteSpace(value), "Need Non-Null value for name: " + name);
			_metadata.Properties.Add(new KeyValue { Key = name, Value = value });
			return this;
		}
		public ITypedMessageBuilder<T> Properties(IDictionary<string, string> properties)
		{
			foreach (var entry in properties.SetOfKeyValuePairs())
			{
				Condition.CheckArgument(entry.Key != null, "Need Non-Null key");
				Condition.CheckArgument(entry.Value != null, "Need Non-Null value for key: " + entry.Key);
				_metadata.Properties.Add(new KeyValue { Key = entry.Key, Value = entry.Value });
			}

			return this;
		}

		public ITypedMessageBuilder<T> EventTime(long timestamp)
		{
			Condition.CheckArgument(timestamp > 0, "Invalid timestamp : '%s'", timestamp);
			_metadata.EventTime = (ulong)timestamp;
			return this;
		}

		public ITypedMessageBuilder<T> SequenceId(long sequenceId)
		{
			Condition.CheckArgument(sequenceId >= 0);
			_metadata.SequenceId = (ulong)sequenceId;
			return this;
		}

		public ITypedMessageBuilder<T> ReplicationClusters(IList<string> clusters)
		{
			Condition.CheckNotNull(clusters);
			_metadata.ReplicateToes.Clear();
			_metadata.ReplicateToes.AddRange(clusters);
			return this;
		}
		public ITypedMessageBuilder<T> DisableReplication()
		{
			_metadata.ReplicateToes.Clear();
			_metadata.ReplicateToes.Add("__local__");
			return this;
		}
		public ITypedMessageBuilder<T> DeliverAfter(long delay)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + delay);
		}

		public ITypedMessageBuilder<T> DeliverAt(long timestamp)
		{
			_metadata.DeliverAtTime = timestamp;
			return this;
		}
		

		public ITypedMessageBuilder<T> LoadConf(IDictionary<string, object> config)
		{
			config.ToList().ForEach(d =>
			{
			if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_KEY, StringComparison.OrdinalIgnoreCase))
			{
				Key(d.Value.ToString());
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_PROPERTIES, StringComparison.OrdinalIgnoreCase))
			{
				Properties((IDictionary<string, string>)d.Value);
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_EVENT_TIME, StringComparison.OrdinalIgnoreCase))
			{
				EventTime((long)d.Value);
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_SEQUENCE_ID, StringComparison.OrdinalIgnoreCase))
			{
				SequenceId((long)d.Value);
            }
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_REPLICATION_CLUSTERS, StringComparison.OrdinalIgnoreCase))
			{
				ReplicationClusters((IList<string>)d.Value);
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_DISABLE_REPLICATION, StringComparison.OrdinalIgnoreCase))
			{
				var disableReplication = (bool)d.Value;
				if (disableReplication)
				{
					DisableReplication();
				}
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_DELIVERY_AFTER_SECONDS, StringComparison.OrdinalIgnoreCase))
			{
				DeliverAfter((long)d.Value);
			}
			else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_DELIVERY_AT, StringComparison.OrdinalIgnoreCase))
			{
				DeliverAt((long)d.Value);
			}
            
            else
			{
				throw new Exception("Invalid message config key '" + d.Key + "'");
			}
			});
			return this;
		}

		public async Task<IMessage<T>> Message()
		{
			await BeforeSend().ConfigureAwait(false);
			return Message<T>.Create(_metadata, _content, _schema);
		}

		public long PublishTime => (long)_metadata.PublishTime;

        public bool HasKey()
		{
			return !string.IsNullOrWhiteSpace(_metadata.PartitionKey);
		}


        public string GetKey => _metadata.PartitionKey;
    }

}