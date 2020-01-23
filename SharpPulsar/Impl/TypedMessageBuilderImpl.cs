using Org.Apache.Pulsar.Common.Schema;

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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static SharpPulsar.util.TypeCheckUtil.checkType;

	using Preconditions = com.google.common.@base.Preconditions;


	using SharpPulsar.Api;
	using MessageId = SharpPulsar.Api.MessageId;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using SharpPulsar.Api;
	using SharpPulsar.Api;
	using SharpPulsar.Impl.Schema;
	using TransactionImpl = SharpPulsar.Impl.Transaction.TransactionImpl;
	using KeyValue = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.KeyValue;
	using MessageMetadata = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.MessageMetadata;
	using KeyValueEncodingType = Org.Apache.Pulsar.Common.Schema.KeyValueEncodingType;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
	using ByteString = Org.Apache.Pulsar.shaded.com.google.protobuf.v241.ByteString;

	[Serializable]
	public class TypedMessageBuilderImpl<T> : TypedMessageBuilder<T>
	{

		private const long SerialVersionUID = 0L;

		private static readonly ByteBuffer EMPTY_CONTENT = ByteBuffer.allocate(0);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final ProducerBase<?> producer;
		private readonly ProducerBase<object> producer;
		public virtual MetadataBuilder {get;} = MessageMetadata.newBuilder();
		private readonly Schema<T> schema;
		public virtual Content {get;}
		private readonly TransactionImpl txn;

		public TypedMessageBuilderImpl<T1>(ProducerBase<T1> Producer, Schema<T> Schema) : this(Producer, Schema, null)
		{
		}

		public TypedMessageBuilderImpl<T1>(ProducerBase<T1> Producer, Schema<T> Schema, TransactionImpl Txn)
		{
			this.producer = Producer;
			this.schema = Schema;
			this.Content = EMPTY_CONTENT;
			this.txn = Txn;
		}

		private long BeforeSend()
		{
			if (txn == null)
			{
				return -1L;
			}
			MetadataBuilder.TxnidLeastBits = txn.TxnIdLeastBits;
			MetadataBuilder.TxnidMostBits = txn.TxnIdMostBits;
			long SequenceId = txn.NextSequenceId();
			MetadataBuilder.SequenceId = SequenceId;
			return SequenceId;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.MessageId send() throws SharpPulsar.api.PulsarClientException
		public override MessageId Send()
		{
			if (null != txn)
			{
				// NOTE: it makes no sense to send a transactional message in a blocking way.
				//       because #send only completes when a transaction is committed or aborted.
				throw new System.InvalidOperationException("Use sendAsync to send a transactional message");
			}
			return producer.Send(Message);
		}

		public override CompletableFuture<MessageId> SendAsync()
		{
			long SequenceId = BeforeSend();
			CompletableFuture<MessageId> SendFuture = producer.InternalSendAsync(Message);
			if (txn != null)
			{
				// it is okay that we register produced topic after sending the messages. because
				// the transactional messages will not be visible for consumers until the transaction
				// is committed.
				txn.RegisterProducedTopic(producer.Topic);
				// register the sendFuture as part of the transaction
				return txn.RegisterSendOp(SequenceId, SendFuture);
			}
			else
			{
				return SendFuture;
			}
		}

		public override TypedMessageBuilder<T> Key(string Key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema KvSchema = (KeyValueSchema) schema;
				checkArgument(!(KvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			MetadataBuilder.setPartitionKey(Key);
			MetadataBuilder.PartitionKeyB64Encoded = false;
			return this;
		}

		public override TypedMessageBuilder<T> KeyBytes(sbyte[] Key)
		{
			if (schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema KvSchema = (KeyValueSchema) schema;
				checkArgument(!(KvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED), "This method is not allowed to set keys when in encoding type is SEPARATED");
			}
			MetadataBuilder.setPartitionKey(Base64.Encoder.encodeToString(Key));
			MetadataBuilder.PartitionKeyB64Encoded = true;
			return this;
		}

		public override TypedMessageBuilder<T> OrderingKey(sbyte[] OrderingKey)
		{
			MetadataBuilder.OrderingKey = ByteString.copyFrom(OrderingKey);
			return this;
		}

		public override TypedMessageBuilder<T> Value(T Value)
		{

			checkArgument(Value != null, "Need Non-Null content value");
			if (schema.SchemaInfo != null && schema.SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
				KeyValueSchema KvSchema = (KeyValueSchema) schema;
				KeyValue Kv = (KeyValue) Value;
				if (KvSchema.KeyValueEncodingType == KeyValueEncodingType.SEPARATED)
				{
					// set key as the message key
					MetadataBuilder.setPartitionKey(Base64.Encoder.encodeToString(KvSchema.KeySchema.encode(Kv.Key)));
					MetadataBuilder.PartitionKeyB64Encoded = true;
					// set value as the payload
					this.Content = ByteBuffer.wrap(KvSchema.ValueSchema.encode(Kv.Value));
					return this;
				}
			}
			this.Content = ByteBuffer.wrap(schema.Encode(Value));
			return this;
		}

		public override TypedMessageBuilder<T> Property(string Name, string Value)
		{
			checkArgument(!string.ReferenceEquals(Name, null), "Need Non-Null name");
			checkArgument(!string.ReferenceEquals(Value, null), "Need Non-Null value for name: " + Name);
			MetadataBuilder.AddProperties(KeyValue.newBuilder().setKey(Name).setValue(Value).build());
			return this;
		}

		public override TypedMessageBuilder<T> Properties(IDictionary<string, string> Properties)
		{
			foreach (KeyValuePair<string, string> Entry in Properties.SetOfKeyValuePairs())
			{
				checkArgument(Entry.Key != null, "Need Non-Null key");
				checkArgument(Entry.Value != null, "Need Non-Null value for key: " + Entry.Key);
				MetadataBuilder.AddProperties(KeyValue.newBuilder().setKey(Entry.Key).setValue(Entry.Value).build());
			}

			return this;
		}

		public override TypedMessageBuilder<T> EventTime(long Timestamp)
		{
			checkArgument(Timestamp > 0, "Invalid timestamp : '%s'", Timestamp);
			MetadataBuilder.EventTime = Timestamp;
			return this;
		}

		public override TypedMessageBuilder<T> SequenceId(long SequenceId)
		{
			checkArgument(SequenceId >= 0);
			MetadataBuilder.SequenceId = SequenceId;
			return this;
		}

		public override TypedMessageBuilder<T> ReplicationClusters(IList<string> Clusters)
		{
			Preconditions.checkNotNull(Clusters);
			MetadataBuilder.ClearReplicateTo();
			MetadataBuilder.AddAllReplicateTo(Clusters);
			return this;
		}

		public override TypedMessageBuilder<T> DisableReplication()
		{
			MetadataBuilder.ClearReplicateTo();
			MetadataBuilder.AddReplicateTo("__local__");
			return this;
		}

		public override TypedMessageBuilder<T> DeliverAfter(long Delay, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + Unit.toMillis(Delay));
		}

		public override TypedMessageBuilder<T> DeliverAt(long Timestamp)
		{
			MetadataBuilder.DeliverAtTime = Timestamp;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public SharpPulsar.api.TypedMessageBuilder<T> loadConf(java.util.Map<String, Object> config)
		public override TypedMessageBuilder<T> LoadConf(IDictionary<string, object> Config)
		{
			Config.forEach((key, value) =>
			{
			if (key.Equals(TypedMessageBuilderFields.ConfKey))
			{
				this.Key(checkType(value, typeof(string)));
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfProperties))
			{
				this.Properties(checkType(value, typeof(System.Collections.IDictionary)));
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfEventTime))
			{
				this.EventTime(checkType(value, typeof(Long)));
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfSequenceId))
			{
				this.SequenceId(checkType(value, typeof(Long)));
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfReplicationClusters))
			{
				this.ReplicationClusters(checkType(value, typeof(System.Collections.IList)));
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfDisableReplication))
			{
				bool DisableReplication = checkType(value, typeof(Boolean));
				if (DisableReplication)
				{
					this.DisableReplication();
				}
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfDeliveryAfterSeconds))
			{
				this.DeliverAfter(checkType(value, typeof(Long)), BAMCIS.Util.Concurrent.TimeUnit.SECONDS);
			}
			else if (key.Equals(TypedMessageBuilderFields.ConfDeliveryAt))
			{
				this.DeliverAt(checkType(value, typeof(Long)));
			}
			else
			{
				throw new Exception("Invalid message config key '" + key + "'");
			}
			});
			return this;
		}


		public virtual Message<T> Message
		{
			get
			{
				BeforeSend();
				return MessageImpl.Create(MetadataBuilder, Content, schema);
			}
		}

		public virtual long PublishTime
		{
			get
			{
				return MetadataBuilder.PublishTime;
			}
		}

		public virtual bool HasKey()
		{
			return MetadataBuilder.HasPartitionKey();
		}

		public virtual string Key
		{
			get
			{
				return MetadataBuilder.getPartitionKey();
			}
		}

	}

}