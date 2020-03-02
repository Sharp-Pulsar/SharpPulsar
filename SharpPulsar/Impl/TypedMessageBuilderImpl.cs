
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using Google.Protobuf;
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
    using System.Threading.Tasks;
    using Api;
    using Transaction;
	public class TypedMessageBuilderImpl : ITypedMessageBuilder
	{
        private static readonly IByteBuffer EmptyContent = Unpooled.WrappedBuffer(new byte[0]);
		
		private readonly string _producer;//topic
		public MessageMetadata.Builder Builder  = MessageMetadata.NewBuilder();
		private readonly ISchema _schema;
		public  IByteBuffer Content;
		private readonly TransactionImpl _txn;

		public TypedMessageBuilderImpl(string producer, ISchema schema) : this(producer, schema, null)
		{
		}

		public TypedMessageBuilderImpl(string producer, ISchema schema, TransactionImpl txn)
		{
			_producer = producer;
			_schema = schema;
			Content = EmptyContent;
			_txn = txn;
		}

		private long BeforeSend()
		{
			if (_txn == null)
			{
				return -1L;
			}
			Builder.SetTxnidLeastBits(_txn.TxnIdLeastBits);
			Builder.SetTxnidMostBits(_txn.TxnIdMostBits);
			var sequenceId = _txn.NextSequenceId();
			Builder.SetSequenceId(sequenceId);
			return sequenceId;
		}

		public IMessageId Send()
		{
			if (null != _txn)
			{
				// NOTE: it makes no sense to send a transactional message in a blocking way.
				//       because #send only completes when a transaction is committed or aborted.
				throw new InvalidOperationException("Use sendAsync to send a transactional message");
			}
			return _producer.Send(Message);
		}

		public ValueTask<IMessageId> SendAsync()
		{
			var sequenceId = BeforeSend();
			var sendTask = _producer.InternalSendAsync(Message);
			if (_txn != null)
			{
				// it is okay that we register produced topic after sending the messages. because
				// the transactional messages will not be visible for consumers until the transaction
				// is committed.
				_txn.RegisterProducedTopic(_producer.Topic);
				// register the sendFuture as part of the transaction
				var t = _txn.RegisterSendOp(sequenceId, sendTask);
				return new ValueTask<IMessageId>(t.Task);
			}
            return new ValueTask<IMessageId>(sendTask.Task);
		}

		public ITypedMessageBuilder Key(string key)
		{
			if (_schema.SchemaInfo.Type == SchemaType.KeyValue)
			{
				throw new PulsarClientException.NotSupportedException("KeyValue not supported");
				//KeyValueSchema kvSchema = (KeyValueSchema) _schema;
				//checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			Builder.SetPartitionKey(key);
			Builder.SetPartitionKeyB64Encoded(false);
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
			Builder.SetPartitionKey(Convert.ToBase64String((byte[])(object)key));
			Builder.SetPartitionKeyB64Encoded(true);
			return this;
		}

		public ITypedMessageBuilder OrderingKey(sbyte[] orderingKey)
		{
			Builder.SetOrderingKey(ByteString.CopyFrom((byte[])(object)orderingKey));
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
            Content = Unpooled.WrappedBuffer(data);
			return this;
		}

		public  ITypedMessageBuilder Property(string name, string value)
		{
			if(ReferenceEquals(name, null))
                throw new NullReferenceException("Need Non-Null name");
			if(ReferenceEquals(value, null))
                throw new NullReferenceException("Need Non-Null value for name: " + name);
			Builder.AddProperties(KeyValue.NewBuilder().SetKey(name).SetValue(value).Build());
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
				Builder.AddProperties(KeyValue.NewBuilder().SetKey(entry.Key).SetValue(entry.Value).Build());
			}

			return this;
		}

		public ITypedMessageBuilder EventTime(long timestamp)
		{
			if(timestamp <= 0)
                throw new ArgumentException("Invalid timestamp : "+ timestamp);
			Builder.SetEventTime(timestamp);
			return this;
		}

		public ITypedMessageBuilder SequenceId(long sequenceId)
		{
			if(sequenceId < 0)
				throw new ArgumentException();
			Builder.SetSequenceId(sequenceId);
			return this;
		}

		public ITypedMessageBuilder ReplicationClusters(IList<string> clusters)
		{
			if(clusters == null)
				throw new NullReferenceException();
			Builder.AddAllReplicateTo(clusters);
			return this;
		}

		public ITypedMessageBuilder DisableReplication()
		{
			Builder.AddReplicateTo("__local__");
			return this;
		}

		public ITypedMessageBuilder DeliverAfter(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + unit.ToMillis(delay));
		}

		public ITypedMessageBuilder DeliverAt(long timestamp)
		{
			Builder.SetDeliverAtTime(timestamp);
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
			else
			{
				throw new System.Exception("Invalid message config key '" + d.Key + "'");
			}
			});
			return this;
		}


		public IMessage Message
		{
			get
			{
				BeforeSend();
				return Impl.Message.Create(Builder, Content, _schema);
			}
		}

		public virtual long PublishTime => Builder.GetPublishTime();

        public virtual bool HasKey()
		{
			return Builder.HasPartitionKey();
		}

		public virtual string GetKey => Builder.GetPartitionKey();
    }

}