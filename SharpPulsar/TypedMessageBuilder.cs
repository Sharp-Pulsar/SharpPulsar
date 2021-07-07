
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
    using SharpPulsar.Interfaces;
    using SharpPulsar.Messages.Transaction;
    using SharpPulsar.Precondition;
    using SharpPulsar.Schemas;
    using System.Buffers;
    using System.Threading.Tasks;

    [Serializable]
	public class TypedMessageBuilder<T> : ITypedMessageBuilder<T>
	{
		private readonly IActorRef _producer;//topic
		public readonly MessageMetadata Metadata  = new MessageMetadata();
		private readonly ISchema<T> _schema;
		private ReadOnlySequence<byte> _content;
		private readonly User.Transaction _txn;

		public TypedMessageBuilder(IActorRef producer, ISchema<T> schema) : this(producer, schema, null)
		{
		}

		public TypedMessageBuilder(IActorRef producer, ISchema<T> schema, User.Transaction txn)
		{
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
			Metadata.TxnidLeastBits = (ulong)bits.LeastBits;
			Metadata.TxnidMostBits = (ulong)bits.MostBits;
			long sequenceId = sequence;
			Metadata.SequenceId = (ulong)sequenceId;
			return sequenceId;
		}
		public MessageId Send(TimeSpan sendTimeout, bool isDeadLetter = false)
		{
			return SendAsync(sendTimeout, isDeadLetter).GetAwaiter().GetResult();
		}
		public async ValueTask<MessageId> SendAsync(TimeSpan sendTimeout, bool isDeadLetter = false)
		{
            try
            {
                var message = await Message().ConfigureAwait(false);
                if (_txn != null)
                {
                    return await _producer.Ask<MessageId>(new InternalSendWithTxn<T>(message, _txn.Txn, isDeadLetter), sendTimeout).ConfigureAwait(false);
                }
                else
                {
                    return await _producer.Ask<MessageId>(new InternalSend<T>(message, isDeadLetter), sendTimeout).ConfigureAwait(false);
                }
            }
            catch(AskTimeoutException)
            {
                return null;
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
				var kvSchema = (KeyValueSchema<object,object>)_schema;
				Condition.CheckArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
				if (string.IsNullOrWhiteSpace(key))
				{
					Metadata.NullPartitionKey = true;
					return this;
				}
			}
			Metadata.PartitionKey = key;
			Metadata.PartitionKeyB64Encoded = false;
			return this;
		}
		public ITypedMessageBuilder<T> KeyBytes(byte[] key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
				var kvSchema = (KeyValueSchema<object, object>)_schema;
				Condition.CheckArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
				if (key == null)
				{
					Metadata.NullPartitionKey = true;
					return this;
				}
			}
			Metadata.PartitionKey = Convert.ToBase64String(key);
			Metadata.PartitionKeyB64Encoded = true;
			return this;
		}
		public ITypedMessageBuilder<T> OrderingKey(byte[] orderingKey)
		{
			Metadata.OrderingKey = orderingKey;
			return this;
		}

		public ITypedMessageBuilder<T> Value(T value)
		{
			if (value == null)
			{
				Metadata.NullValue = true;
				return this;
			}
			if (_schema.SchemaInfo != null && _schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
				var kvSchema = (KeyValueSchema<object,object>)_schema;
				var kv = (KeyValue<object, object>)(object)value;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					// set key as the message key
					if (kv.Key != null)
					{
						Metadata.PartitionKey = Convert.ToBase64String(kvSchema.KeySchema.Encode(kv.Key));
						Metadata.PartitionKeyB64Encoded = true;
					}
					else
					{
						Metadata.NullPartitionKey = true;
					}

					// set value as the payload
					if (kv.Value != null)
					{
						_content = new ReadOnlySequence<byte>(kvSchema.ValueSchema.Encode(kv.Value));
					}
					else
					{
						Metadata.NullValue = true;
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
			Metadata.Properties.Add(new KeyValue { Key = name, Value = value });
			return this;
		}
		public ITypedMessageBuilder<T> Properties(IDictionary<string, string> properties)
		{
			foreach (KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs())
			{
				Condition.CheckArgument(entry.Key != null, "Need Non-Null key");
				Condition.CheckArgument(entry.Value != null, "Need Non-Null value for key: " + entry.Key);
				Metadata.Properties.Add(new KeyValue { Key = entry.Key, Value = entry.Value });
			}

			return this;
		}

		public ITypedMessageBuilder<T> EventTime(long timestamp)
		{
			Condition.CheckArgument(timestamp > 0, "Invalid timestamp : '%s'", timestamp);
			Metadata.EventTime = (ulong)timestamp;
			return this;
		}

		public ITypedMessageBuilder<T> SequenceId(long sequenceId)
		{
			Condition.CheckArgument(sequenceId >= 0);
			Metadata.SequenceId = (ulong)sequenceId;
			return this;
		}

		public ITypedMessageBuilder<T> ReplicationClusters(IList<string> clusters)
		{
			Condition.CheckNotNull(clusters);
			Metadata.ReplicateToes.Clear();
			Metadata.ReplicateToes.AddRange(clusters);
			return this;
		}
		public ITypedMessageBuilder<T> DisableReplication()
		{
			Metadata.ReplicateToes.Clear();
			Metadata.ReplicateToes.Add("__local__");
			return this;
		}
		public ITypedMessageBuilder<T> DeliverAfter(long delay)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + delay);
		}

		public ITypedMessageBuilder<T> DeliverAt(long timestamp)
		{
			Metadata.DeliverAtTime = timestamp;
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
			return Message<T>.Create(Metadata, _content, _schema);
		}

		public long PublishTime => (long)Metadata.PublishTime;

        public bool HasKey()
		{
			return !string.IsNullOrWhiteSpace(Metadata.PartitionKey);
		}


        public string GetKey => Metadata.PartitionKey;
    }

}