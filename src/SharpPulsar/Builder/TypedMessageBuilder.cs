
using System;
using System.Collections.Generic;
using System.Linq;
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
namespace SharpPulsar.Builder
{
    using Akka.Actor;
    using SharpPulsar.Messages.Transaction;
    using SharpPulsar.Common.Precondition;
    using SharpPulsar.Schemas;
    using System.Threading.Tasks;
    using SharpPulsar.API;
    using SharpPulsar.Common.Protocol.Proto;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Shared.Buf;
    using SharpPulsar.Messages.Requests;

    [Serializable]
    internal class TypedMessageBuilder<T> : ITypedMessageBuilder<T>
    {
        [NonSerialized]
        private readonly IActorRef _producer;//topic
        [NonSerialized]
        private readonly MessageMetadata _metadata = new MessageMetadata();
        [NonSerialized]
        private readonly ISchema<T> _schema;
        [NonSerialized]
        private ByteBuf _content;
        [NonSerialized]
        private readonly TransactionImpl.Transaction _txn;
        [NonSerialized]
        private T _value;

        public TypedMessageBuilder(IActorRef producer, ISchema<T> schema) : this(producer, schema, null)
        {
        }

        public TypedMessageBuilder(IActorRef producer, ISchema<T> schema, TransactionImpl.Transaction txn)
        {
            _producer = producer;
            _schema = schema;
            _content = new ByteBuf();
            _txn = txn;
        }

        private async Task<long> BeforeSend()
        {
            if (_value == null)
            {
                _metadata.NullValue = true;
            }
            else
            {
                GetKeyValueSchema().Map(keyValueSchema =>
                {
                    if (keyValueSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
                    {
                        SetSeparateKeyValue(_value, keyValueSchema);
                        return this;
                    }
                    else
                    {
                        return null;
                    }
                }).OrElseGet(() =>
                {
                    var encodeData = _schema.Encode(Topic, _value);
                    _content = new ByteBuf(encodeData.Data);
                    if (encodeData.HasSchemaId())
                    {
                        _metadata.SchemaId = encodeData.SchemaId;
                    }
                    return this;
                });
            }

            if (_txn == null)
            {
                return -1L;
            }
            var bits = await _txn.Txn.Ask<GetTxnIdBitsResponse>(GetTxnIdBits.Instance).ConfigureAwait(false);
            var sequence = await _txn.Txn.Ask<long>(NextSequenceId.Instance).ConfigureAwait(false);
            _metadata.TxnidLeastBits = (ulong)bits.LeastBits;
            _metadata.TxnidMostBits = (ulong)bits.MostBits;
            return -1L;
        }

        public IMessageIdAdv Send()
        {
            return SendAsync().GetAwaiter().GetResult();
        }
        public async ValueTask<IMessageIdAdv> SendAsync()
        {
            try
            {
                var message = await Message().ConfigureAwait(false);
                var tcs = new TaskCompletionSource<IMessageId>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (_txn != null)
                {
                    _producer.Tell(new InternalSendWithTxn<T>(message, _txn.Txn, tcs));
                }
                else
                {
                    _producer.Tell(new InternalSend<T>(message, tcs));

                }
                var response = await tcs.Task;
                if (response == null)
                    return null;

                return (IMessageIdAdv)response;
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
            _value = value;
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

        public ITypedMessageBuilder<T> EventTime(DateTime timestamp)
        {
            _metadata.EventTime = (ulong)DateTimeHelper.CurrentUnixTimeMillis(timestamp);
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
        /// <summary>
        /// delay is added to the current unix time in MILLISECONDS.
        /// TotalMilliseconds is called on delay 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
		public ITypedMessageBuilder<T> DeliverAfter(TimeSpan delay)
        {
            return DeliverAt(DateTimeOffset.UtcNow.AddMilliseconds(delay.TotalMilliseconds));
        }

        public ITypedMessageBuilder<T> DeliverAt(DateTimeOffset dateTime)
        {
            var unix = dateTime.ToUnixTimeMilliseconds();

            _metadata.DeliverAtTime = unix;
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
                    if (d.Value is DateTime offset)
                        EventTime(offset);
                    else
                        throw new ArgumentException($"{d.Key} must of type DateTime");
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
                    DeliverAfter(TimeSpan.FromMilliseconds((long)d.Value));
                }
                else if (d.Key.Equals(ITypedMessageBuilder<T>.CONF_DELIVERY_AT, StringComparison.OrdinalIgnoreCase))
                {
                    if (d.Value is DateTimeOffset offset)
                        DeliverAt(offset);
                    else
                        throw new ArgumentException($"{d.Key} must of type DateTime");
                }

                else
                {
                    throw new Exception("Invalid message config key '" + d.Key + "'");
                }
            });
            return this;
        }
        public MessageMetadata GetMetadataBuilder()
        {
            return _metadata;
        }
        public async Task<IMessage<T>> Message()
        {
            await BeforeSend().ConfigureAwait(false);
            return Message<T>.Create(Topic, _metadata, _content, _schema);
        }

        public long PublishTime => (long)_metadata.PublishTime;

        public bool HasKey()
        {
            return !string.IsNullOrWhiteSpace(_metadata.PartitionKey);
        }
        public ByteBuf GetContent()
        {
            return _content;
        }
        private Optional<KeyValueSchema<object, object>> GetKeyValueSchema()
        {
            if (_schema.SchemaInfo != null && _schema.SchemaInfo.Type == SchemaType.KeyValue && _schema is KeyValueSchema<object, object>)
            {
                return new Optional<KeyValueSchema<object, object>>((KeyValueSchema<object, object>)_schema);
            }
            else
            {
                return Optional<KeyValueSchema<object, object>>.None;
            }
        }
        private void SetSeparateKeyValue<K, V>(T value, KeyValueSchema<K, V> keyValueSchema)
        {
            Condition.CheckArgument(value is KeyValue);
            var keyValue = (KeyValue<K, V>)(object)value;

            EncodeData keyEncoded = null;
            // set key as the message key
            if (keyValue.Key != null)
            {
                keyEncoded = keyValueSchema.KeySchema.Encode(Topic, keyValue.Key);
                _metadata.PartitionKey = BitConverter.ToString(keyEncoded.Data);
                _metadata.PartitionKeyB64Encoded = true;
            }
            else
            {
                _metadata.NullPartitionKey = true;
            }

            EncodeData valueEncoded = null;
            // set value as the payload
            if (keyValue.Value != null)
            {
                valueEncoded = keyValueSchema.ValueSchema.Encode(Topic, keyValue.Value);
                _content = new ByteBuf(valueEncoded.Data);
            }
            else
            {
                _metadata.NullValue = true;
            }

            var schemaId = KeyValue<K, V>.GenerateKVSchemaId(keyEncoded != null && keyEncoded.HasSchemaId() ? keyEncoded.SchemaId : null, valueEncoded != null && valueEncoded.HasSchemaId() ? valueEncoded.SchemaId : null);
            if (EncodeData.IsValidSchemaId(schemaId))
            {
                _metadata.SchemaId = schemaId;
            }
        }
        private string Topic
        {
            get
            {
                return _producer != null ? _producer.Ask<string>(GetTopic.Instance).GetAwaiter().GetResult() : null;
            }
        }


        public string GetKey => _metadata.PartitionKey;
    }

}