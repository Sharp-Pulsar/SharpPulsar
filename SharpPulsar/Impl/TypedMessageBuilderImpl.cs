using System;
using System.Collections.Generic;

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
namespace org.apache.pulsar.client.impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.pulsar.client.util.TypeCheckUtil.checkType;

	using Preconditions = com.google.common.@base.Preconditions;


	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using TypedMessageBuilder = org.apache.pulsar.client.api.TypedMessageBuilder;
	using SharpPulsar.Impl.Schema;
	using TransactionImpl = org.apache.pulsar.client.impl.transaction.TransactionImpl;
	using KeyValue = org.apache.pulsar.common.api.proto.PulsarApi.KeyValue;
	using MessageMetadata = org.apache.pulsar.common.api.proto.PulsarApi.MessageMetadata;
	using KeyValueEncodingType = org.apache.pulsar.common.schema.KeyValueEncodingType;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using ByteString = org.apache.pulsar.shaded.com.google.protobuf.v241.ByteString;

	public class TypedMessageBuilderImpl<T> : TypedMessageBuilder<T>
	{

		private const long serialVersionUID = 0L;

		private static readonly ByteBuffer EMPTY_CONTENT = ByteBuffer.allocate(0);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ProducerBase<?> producer;
		private readonly ProducerBase<object> producer;
		private readonly MessageMetadata.Builder msgMetadataBuilder = MessageMetadata.newBuilder();
		private readonly Schema<T> schema;
		private ByteBuffer content;
		private readonly TransactionImpl txn;

		public TypedMessageBuilderImpl<T1>(ProducerBase<T1> producer, Schema<T> schema) : this(producer, schema, null)
		{
		}

		public TypedMessageBuilderImpl<T1>(ProducerBase<T1> producer, Schema<T> schema, TransactionImpl txn)
		{
			this.producer = producer;
			this.schema = schema;
			this.content = EMPTY_CONTENT;
			this.txn = txn;
		}

		private long beforeSend()
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId send() throws org.apache.pulsar.client.api.PulsarClientException
		public override MessageId send()
		{
			if (null != txn)
			{
				// NOTE: it makes no sense to send a transactional message in a blocking way.
				//       because #send only completes when a transaction is committed or aborted.
				throw new System.InvalidOperationException("Use sendAsync to send a transactional message");
			}
			return producer.send(Message);
		}

		public override CompletableFuture<MessageId> sendAsync()
		{
			long sequenceId = beforeSend();
			CompletableFuture<MessageId> sendFuture = producer.internalSendAsync(Message);
			if (txn != null)
			{
				// it is okay that we register produced topic after sending the messages. because
				// the transactional messages will not be visible for consumers until the transaction
				// is committed.
				txn.registerProducedTopic(producer.Topic);
				// register the sendFuture as part of the transaction
				return txn.registerSendOp(sequenceId, sendFuture);
			}
			else
			{
				return sendFuture;
			}
		}

		public override TypedMessageBuilder<T> key(string key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema kvSchema = (KeyValueSchema) schema;
				checkArgument(!(kvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			msgMetadataBuilder.PartitionKey = key;
			msgMetadataBuilder.PartitionKeyB64Encoded = false;
			return this;
		}

		public override TypedMessageBuilder<T> keyBytes(sbyte[] key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema kvSchema = (KeyValueSchema) schema;
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
				KeyValueSchema kvSchema = (KeyValueSchema) schema;
				org.apache.pulsar.common.schema.KeyValue kv = (org.apache.pulsar.common.schema.KeyValue) value;
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

		public override TypedMessageBuilder<T> deliverAfter(long delay, TimeUnit unit)
		{
			return deliverAt(DateTimeHelper.CurrentUnixTimeMillis() + unit.toMillis(delay));
		}

		public override TypedMessageBuilder<T> deliverAt(long timestamp)
		{
			msgMetadataBuilder.DeliverAtTime = timestamp;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public org.apache.pulsar.client.api.TypedMessageBuilder<T> loadConf(java.util.Map<String, Object> config)
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