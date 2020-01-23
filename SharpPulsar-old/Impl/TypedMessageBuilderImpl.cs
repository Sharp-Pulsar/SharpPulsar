using System;
using System.Collections.Generic;
using System.Transactions;

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
	//using Preconditions = com.google.common.@base.Preconditions;
	using TransactionImpl = Transaction.TransactionImpl;
	using SharpPulsar.Interface;
	using SharpPulsar.Impl.Producer;
	using SharpPulsar.Interface.Schema;
	using SharpPulsar.Interface.Message;
    using System.Threading.Tasks;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Impl.Schema;
    using SharpPulsar.Enum;
    using BAMCIS.Util.Concurrent;
    using SharpPulsar.Util;

    public class TypedMessageBuilderImpl<T> : ITypedMessageBuilder<T>
	{

		private const long serialVersionUID = 0L;

		private static readonly ByteBuffer EMPTY_CONTENT = ByteBuffer.Allocate(0);
		private readonly ProducerBase<T> producer;
		private readonly MessageMetadata.Builder msgMetadataBuilder = MessageMetadata.newBuilder();
		private readonly ISchema<T> schema;
		private ByteBuffer content;
		private readonly TransactionImpl txn;

		public TypedMessageBuilderImpl(ProducerBase<T> producer, ISchema<T> schema):this(producer, schema, null)
		{
		}

		public TypedMessageBuilderImpl(ProducerBase<T> producer, ISchema<T> schema, TransactionImpl txn)
		{
			this.producer = producer;
			this.schema = schema;
			this.content = EMPTY_CONTENT;
			this.txn = txn;
		}
		private long BeforeSend()
		{
			if (txn == null)
			{
				return -1L;
			}
			msgMetadataBuilder.TxnidLeastBits = txn.TxnIdLeastBits;
			msgMetadataBuilder.TxnidMostBits = txn.TxnIdMostBits;
			long sequenceId = txn.nextSequenceId();
			msgMetadataBuilder.SequenceId = sequenceId;
			return sequenceId;
		}
		public IMessageId Send()
		{
			if (null != txn)
			{
				// NOTE: it makes no sense to send a transactional message in a blocking way.
				//       because #send only completes when a transaction is committed or aborted.
				throw new System.InvalidOperationException("Use sendAsync to send a transactional message");
			}
			return producer.Send(Message);
		}

		public async ValueTask<IMessageId> SendAsync()
		{
			long sequenceId = BeforeSend();
			var sendFuture = await producer.InternalSendAsync(Message);
			if (txn != null)
			{
				// it is okay that we register produced topic after sending the messages. because
				// the transactional messages will not be visible for consumers until the transaction
				// is committed.
				txn.RegisterProducedTopic(producer.Topic);
				// register the sendFuture as part of the transaction
				return txn.RegisterSendOp(sequenceId, sendFuture);
			}
			else
			{
				return sendFuture;
			}
		}

		public ITypedMessageBuilder<T> GetKey(string key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema kvSchema = (KeyValueSchema)schema;
				checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			msgMetadataBuilder.PartitionKey = key;
			msgMetadataBuilder.PartitionKeyB64Encoded = false;
			return this;
		}

		public override ITypedMessageBuilder<T> KeyBytes(sbyte[] key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema kvSchema = (KeyValueSchema)schema;
				checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			msgMetadataBuilder.PartitionKey = Base64.Encoder.encodeToString(key);
			msgMetadataBuilder.PartitionKeyB64Encoded = true;
			return this;
		}

		public override TypedMessageBuilder<T> orderingKey(sbyte[] orderingKey)
		{
			msgMetadataBuilder.OrderingKey = ByteString.copyFrom(orderingKey);
			return this;
		}

		public override TypedMessageBuilder<T> value(T value)
		{

			checkArgument(value != null, "Need Non-Null content value");
			if (schema.SchemaInfo != null && schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema kvSchema = (KeyValueSchema)schema;
				org.apache.pulsar.common.schema.KeyValue kv = (org.apache.pulsar.common.schema.KeyValue)value;
				if (kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					// set key as the message key
					msgMetadataBuilder.PartitionKey = Base64.Encoder.encodeToString(kvSchema.KeySchema.encode(kv.Key));
					msgMetadataBuilder.PartitionKeyB64Encoded = true;
					// set value as the payload
					this.content = ByteBuffer.wrap(kvSchema.ValueSchema.encode(kv.Value));
					return this;
				}
			}
			this.content = ByteBuffer.wrap(schema.encode(value));
			return this;
		}

		public override TypedMessageBuilder<T> property(string name, string value)
		{
			checkArgument(!string.ReferenceEquals(name, null), "Need Non-Null name");
			checkArgument(!string.ReferenceEquals(value, null), "Need Non-Null value for name: " + name);
			msgMetadataBuilder.addProperties(KeyValue.newBuilder().setKey(name).setValue(value).build());
			return this;
		}

		public override TypedMessageBuilder<T> properties(IDictionary<string, string> properties)
		{
			foreach (KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs())
			{
				checkArgument(entry.Key != null, "Need Non-Null key");
				checkArgument(entry.Value != null, "Need Non-Null value for key: " + entry.Key);
				msgMetadataBuilder.addProperties(KeyValue.newBuilder().setKey(entry.Key).setValue(entry.Value).build());
			}

			return this;
		}

		public override TypedMessageBuilder<T> eventTime(long timestamp)
		{
			checkArgument(timestamp > 0, "Invalid timestamp : '%s'", timestamp);
			msgMetadataBuilder.EventTime = timestamp;
			return this;
		}

		public override TypedMessageBuilder<T> sequenceId(long sequenceId)
		{
			checkArgument(sequenceId >= 0);
			msgMetadataBuilder.SequenceId = sequenceId;
			return this;
		}

		public override TypedMessageBuilder<T> replicationClusters(IList<string> clusters)
		{
			Preconditions.checkNotNull(clusters);
			msgMetadataBuilder.clearReplicateTo();
			msgMetadataBuilder.addAllReplicateTo(clusters);
			return this;
		}

		public override TypedMessageBuilder<T> disableReplication()
		{
			msgMetadataBuilder.clearReplicateTo();
			msgMetadataBuilder.addReplicateTo("__local__");
			return this;
		}

		public override ITypedMessageBuilder<T> DeliverAfter(long delay, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			return deliverAt(DateTimeHelper.CurrentUnixTimeMillis() + unit.ToMillis(delay));
		}

		public override TypedMessageBuilder<T> deliverAt(long timestamp)
		{
			msgMetadataBuilder.DeliverAtTime = timestamp;
			return this;
		}

		public override TypedMessageBuilder<T> loadConf(IDictionary<string, object> config)
		{
			config.forEach((key, value) =>
			{
				if (key.Equals(CONF_KEY))
				{
					this.key(checkType(value, typeof(string)));
				}
				else if (key.Equals(CONF_PROPERTIES))
				{
					this.properties(checkType(value, typeof(System.Collections.IDictionary)));
				}
				else if (key.Equals(CONF_EVENT_TIME))
				{
					this.eventTime(checkType(value, typeof(Long)));
				}
				else if (key.Equals(CONF_SEQUENCE_ID))
				{
					this.sequenceId(checkType(value, typeof(Long)));
				}
				else if (key.Equals(CONF_REPLICATION_CLUSTERS))
				{
					this.replicationClusters(checkType(value, typeof(System.Collections.IList)));
				}
				else if (key.Equals(CONF_DISABLE_REPLICATION))
				{
					bool disableReplication = checkType(value, typeof(Boolean));
					if (disableReplication)
					{
						this.disableReplication();
					}
				}
				else if (key.Equals(CONF_DELIVERY_AFTER_SECONDS))
				{
					this.deliverAfter(checkType(value, typeof(Long)), TimeUnit.SECONDS);
				}
				else if (key.Equals(CONF_DELIVERY_AT))
				{
					this.deliverAt(checkType(value, typeof(Long)));
				}
				else
				{
					throw new Exception("Invalid message config key '" + key + "'");
				}
			});
			return this;
		}

		public virtual MessageMetadata.Builder MetadataBuilder
		{
			get
			{
				return msgMetadataBuilder;
			}
		}

		public virtual Message<T> Message
		{
			get
			{
				beforeSend();
				return MessageImpl.create(msgMetadataBuilder, content, schema);
			}
		}

		public virtual long PublishTime
		{
			get
			{
				return msgMetadataBuilder.PublishTime;
			}
		}

		public virtual bool hasKey()
		{
			return msgMetadataBuilder.hasPartitionKey();
		}

		public virtual string Key
		{
			get
			{
				return msgMetadataBuilder.PartitionKey;
			}
		}

		public virtual ByteBuffer Content
		{
			get
			{
				return content;
			}
		}
}
}