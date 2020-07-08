
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Extension;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Shared;
using SharpPulsar.Utility;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
    using Api;
	public class TypedMessageBuilder : ITypedMessageBuilder
	{
        private string _topic;
		private readonly string _producer;//topic
		private readonly MessageMetadata _metadata  = new MessageMetadata();
		private readonly ISchema _schema;
		private byte[] _content;

        public TypedMessageBuilder(string producer, ISchema schema)
		{
			_producer = producer;
			_schema = schema;
			_content = new byte[0]{};
		}

		private long BeforeSend()
		{
			/*
			 if (_txn == null)
			{
				return -1L;
			}
			_metadata.SetTxnidLeastBits(_txn.TxnIdLeastBits);
			_metadata.SetTxnidMostBits(_txn.TxnIdMostBits);
			var sequenceId = _txn.NextSequenceId();
			_metadata.SetSequenceId(sequenceId);
			return sequenceId;
			 */
			return -1L;
		}

        public ITypedMessageBuilder Topic(string topic)
        {
			if(string.IsNullOrWhiteSpace(topic))
				throw new ArgumentException("Topic cannot be null");
            _topic = topic;
            return this;
        }
		public ITypedMessageBuilder Key(string key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
				throw new PulsarClientException.NotSupportedException("KeyValue not supported");
				//KeyValueSchema kvSchema = (KeyValueSchema) _schema;
				//checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			_metadata.PartitionKey = key;
			_metadata.PartitionKeyB64Encoded = false;
			return this;
		}

		public ITypedMessageBuilder KeyBytes(sbyte[] key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
                throw new PulsarClientException.NotSupportedException("KeyValue not supported");
				//KeyValueSchema kvSchema = (KeyValueSchema) _schema;
				//checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			_metadata.PartitionKey = Convert.ToBase64String((byte[])(object)key);
			_metadata.PartitionKeyB64Encoded = true;
			return this;
		}

		public ITypedMessageBuilder OrderingKey(sbyte[] orderingKey)
		{
			_metadata.OrderingKey = (byte[])(object)orderingKey;
			return this;
		}

		public ITypedMessageBuilder Value(object value)
		{

			if(value == null)
                throw new NullReferenceException("Need Non-Null content value");
			if (_schema.SchemaInfo != null && _schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
                throw new PulsarClientException.NotSupportedException("KeyValue not supported");
				
			}

            var data = (byte[])(object)_schema.Encode(value);
            _content = data;
			return this;
		}

		public  ITypedMessageBuilder Property(string name, string value)
		{
			if(ReferenceEquals(name, null))
                throw new NullReferenceException("Need Non-Null name");
			if(ReferenceEquals(value, null))
                throw new NullReferenceException("Need Non-Null value for name: " + name);
			_metadata.Properties.Add(new KeyValue{ Key = name, Value = value});
			return this;
		}

		public ITypedMessageBuilder Properties(IDictionary<string, string> properties)
		{
			foreach (var entry in properties.SetOfKeyValuePairs())
			{
				if(entry.Key == null)
					throw new NullReferenceException("Need Non-Null name");
				if(entry.Value == null)
					throw new NullReferenceException("Need Non-Null value for name: " + entry.Key); ;
				_metadata.Properties.Add(new KeyValue {Key = entry.Key, Value = entry.Value});
			}

			return this;
		}

		public ITypedMessageBuilder EventTime(long timestamp)
		{
			if(timestamp <= 0)
                throw new ArgumentException("Invalid timestamp : "+ timestamp);
			_metadata.EventTime = (ulong)timestamp;
			return this;
		}
        public ITypedMessageBuilder ProducerName(string producer)
        {
            _metadata.ProducerName = producer;
            return this;
        }
		public ITypedMessageBuilder SequenceId(long sequenceId)
		{
			if(sequenceId < 0)
				throw new ArgumentException();
			_metadata.SequenceId = (ulong)sequenceId;
			return this;
		}

		public ITypedMessageBuilder ReplicationClusters(IList<string> clusters)
		{
			if(clusters == null)
				throw new NullReferenceException();
			_metadata.ReplicateToes.AddRange(clusters.ToList());
			return this;
		}

		public ITypedMessageBuilder DisableReplication()
		{
			_metadata.ReplicateToes.Add("__local__");
			return this;
		}

		public ITypedMessageBuilder DeliverAfter(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + unit.ToMillis(delay));
		}

		public ITypedMessageBuilder DeliverAt(long timestamp)
		{
			_metadata.DeliverAtTime = timestamp;
			return this;
		}

		public ITypedMessageBuilder LoadConf(IDictionary<string, object> config)
		{
			config.ToList().ForEach(d =>
			{
			if (d.Key.Equals(TypedMessageBuilderFields.ConfKey, StringComparison.OrdinalIgnoreCase))
			{
				Key(d.Value.ToString());
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfProperties, StringComparison.OrdinalIgnoreCase))
			{
				Properties((IDictionary<string, string>)d.Value);
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfEventTime, StringComparison.OrdinalIgnoreCase))
			{
				EventTime((long)d.Value);
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfSequenceId, StringComparison.OrdinalIgnoreCase))
			{
				SequenceId((long)d.Value);
            }
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfReplicationClusters, StringComparison.OrdinalIgnoreCase))
			{
				ReplicationClusters((IList<string>)d.Value);
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfDisableReplication, StringComparison.OrdinalIgnoreCase))
			{
				var disableReplication = (bool)d.Value;
				if (disableReplication)
				{
					DisableReplication();
				}
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfDeliveryAfterSeconds, StringComparison.OrdinalIgnoreCase))
			{
				DeliverAfter((long)d.Value, BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
			}
			else if (d.Key.Equals(TypedMessageBuilderFields.ConfDeliveryAt, StringComparison.OrdinalIgnoreCase))
			{
				DeliverAt((long)d.Value);
			}
            
            /*else
			{
				throw new Exception("Invalid message config key '" + d.Key + "'");
			}*/
			});
			return this;
		}


		public IMessage Message
		{
			get
			{
				BeforeSend();
				return  Impl.Message.Create(_metadata, _content, _schema, _topic);
            }
		}

		public virtual long PublishTime => (long)_metadata.PublishTime;

        public virtual bool HasKey()
		{
			return !string.IsNullOrWhiteSpace(_metadata.PartitionKey);
		}

		public virtual string GetKey => _metadata.PartitionKey;
    }

}