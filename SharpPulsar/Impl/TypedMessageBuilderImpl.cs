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
    using System.Threading.Tasks;
    using SharpPulsar.Api;
    using SharpPulsar.Impl.Transaction;
    using SharpPulsar.Protocol.Builder;

    [Serializable]
	public class TypedMessageBuilderImpl<T> : ITypedMessageBuilder<T>
	{

		private const long SerialVersionUID = 0L;

		private static readonly ByteBuffer EMPTY_CONTENT = ByteBuffer.Allocate(0);
		[NonSerialized]
		private readonly ProducerBase<T> _producer;
		[NonSerialized]
		public MessageMetadataBuilder Builder  = new MessageMetadataBuilder();
		private readonly ISchema<T> schema;
		[NonSerialized]
		public  ByteBuffer Content;
		private readonly TransactionImpl txn;

		public TypedMessageBuilderImpl(ProducerBase<T> producer, ISchema<T> schema) : this(producer, schema, null)
		{
		}

		public TypedMessageBuilderImpl(ProducerBase<T> producer, ISchema<T> schema, TransactionImpl Txn)
		{
			_producer = producer;
			this.schema = schema;
			this.Content = EMPTY_CONTENT;
			this.txn = Txn;
		}

		private long BeforeSend()
		{
			if (txn == null)
			{
				return -1L;
			}
			Builder.SetTxnidLeastBits(txn.TxnIdLeastBits);
			MetadataBuilder.TxnidMostBits = txn.TxnIdMostBits;
			long SequenceId = txn.NextSequenceId();
			MetadataBuilder.SequenceId = SequenceId;
			return SequenceId;
		}

		public MessageId Send()
		{
			if (null != txn)
			{
				// NOTE: it makes no sense to send a transactional message in a blocking way.
				//       because #send only completes when a transaction is committed or aborted.
				throw new InvalidOperationException("Use sendAsync to send a transactional message");
			}
			return _producer.Send(Message);
		}

		public TaskCompletionSource<MessageId> SendAsync()
		{
			long sequenceId = BeforeSend();
			TaskCompletionSource<MessageId> sendTask = _producer.InternalSendAsync(Message);
			if (txn != null)
			{
				// it is okay that we register produced topic after sending the messages. because
				// the transactional messages will not be visible for consumers until the transaction
				// is committed.
				txn.RegisterProducedTopic(_producer.Topic);
				// register the sendFuture as part of the transaction
				return txn.RegisterSendOp(sequenceId, sendTask);
			}
			else
			{
				return sendTask;
			}
		}

		public ITypedMessageBuilder<T> Key(string Key)
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

		public override ITypedMessageBuilder<T> KeyBytes(sbyte[] Key)
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

		public override ITypedMessageBuilder<T> OrderingKey(sbyte[] OrderingKey)
		{
			MetadataBuilder.OrderingKey = ByteString.copyFrom(OrderingKey);
			return this;
		}

		public override ITypedMessageBuilder<T> Value(T Value)
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

		public override ITypedMessageBuilder<T> Property(string Name, string Value)
		{
			checkArgument(!string.ReferenceEquals(Name, null), "Need Non-Null name");
			checkArgument(!string.ReferenceEquals(Value, null), "Need Non-Null value for name: " + Name);
			MetadataBuilder.AddProperties(KeyValue.newBuilder().setKey(Name).setValue(Value).build());
			return this;
		}

		public override ITypedMessageBuilder<T> Properties(IDictionary<string, string> Properties)
		{
			foreach (KeyValuePair<string, string> Entry in Properties.SetOfKeyValuePairs())
			{
				checkArgument(Entry.Key != null, "Need Non-Null key");
				checkArgument(Entry.Value != null, "Need Non-Null value for key: " + Entry.Key);
				MetadataBuilder.AddProperties(KeyValue.newBuilder().setKey(Entry.Key).setValue(Entry.Value).build());
			}

			return this;
		}

		public override ITypedMessageBuilder<T> EventTime(long Timestamp)
		{
			checkArgument(Timestamp > 0, "Invalid timestamp : '%s'", Timestamp);
			MetadataBuilder.EventTime = Timestamp;
			return this;
		}

		public override ITypedMessageBuilder<T> SequenceId(long SequenceId)
		{
			checkArgument(SequenceId >= 0);
			MetadataBuilder.SequenceId = SequenceId;
			return this;
		}

		public override ITypedMessageBuilder<T> ReplicationClusters(IList<string> Clusters)
		{
			Preconditions.checkNotNull(Clusters);
			MetadataBuilder.ClearReplicateTo();
			MetadataBuilder.AddAllReplicateTo(Clusters);
			return this;
		}

		public override ITypedMessageBuilder<T> DisableReplication()
		{
			MetadataBuilder.ClearReplicateTo();
			MetadataBuilder.AddReplicateTo("__local__");
			return this;
		}

		public override ITypedMessageBuilder<T> DeliverAfter(long Delay, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			return DeliverAt(DateTimeHelper.CurrentUnixTimeMillis() + Unit.toMillis(Delay));
		}

		public override ITypedMessageBuilder<T> DeliverAt(long Timestamp)
		{
			MetadataBuilder.DeliverAtTime = Timestamp;
			return this;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public SharpPulsar.api.TypedMessageBuilder<T> loadConf(java.util.Map<String, Object> config)
		public override ITypedMessageBuilder<T> LoadConf(IDictionary<string, object> Config)
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