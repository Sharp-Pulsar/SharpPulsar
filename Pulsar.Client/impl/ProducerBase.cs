using System;

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

	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Producer = org.apache.pulsar.client.api.Producer;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using TypedMessageBuilder = org.apache.pulsar.client.api.TypedMessageBuilder;
	using Transaction = org.apache.pulsar.client.api.transaction.Transaction;
	using ProducerConfigurationData = org.apache.pulsar.client.impl.conf.ProducerConfigurationData;
	using TransactionImpl = org.apache.pulsar.client.impl.transaction.TransactionImpl;
	using SchemaHash = org.apache.pulsar.common.protocol.schema.SchemaHash;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using ConcurrentOpenHashMap = org.apache.pulsar.common.util.collections.ConcurrentOpenHashMap;

	public abstract class ProducerBase<T> : HandlerState, Producer<T>
	{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly CompletableFuture<Producer<T>> producerCreatedFuture_Conflict;
		protected internal readonly ProducerConfigurationData conf;
		protected internal readonly Schema<T> schema;
		protected internal readonly ProducerInterceptors interceptors;
		protected internal readonly ConcurrentOpenHashMap<SchemaHash, sbyte[]> schemaCache;
		protected internal volatile MultiSchemaMode multiSchemaMode = MultiSchemaMode.Auto;

		protected internal ProducerBase(PulsarClientImpl client, string topic, ProducerConfigurationData conf, CompletableFuture<Producer<T>> producerCreatedFuture, Schema<T> schema, ProducerInterceptors interceptors) : base(client, topic)
		{
			this.producerCreatedFuture_Conflict = producerCreatedFuture;
			this.conf = conf;
			this.schema = schema;
			this.interceptors = interceptors;
			this.schemaCache = new ConcurrentOpenHashMap<SchemaHash, sbyte[]>();
			if (!conf.MultiSchema)
			{
				multiSchemaMode = MultiSchemaMode.Disabled;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.client.api.MessageId send(T message) throws org.apache.pulsar.client.api.PulsarClientException
		public override MessageId send(T message)
		{
			return newMessage().value(message).send();
		}

		public override CompletableFuture<MessageId> sendAsync(T message)
		{
			try
			{
				return newMessage().value(message).sendAsync();
			}
			catch (SchemaSerializationException e)
			{
				return FutureUtil.failedFuture(e);
			}
		}

		public virtual CompletableFuture<MessageId> sendAsync<T1>(Message<T1> message)
		{
			return internalSendAsync(message);
		}

		public override TypedMessageBuilder<T> newMessage()
		{
			return new TypedMessageBuilderImpl<T>(this, schema);
		}

		public virtual TypedMessageBuilder<V> newMessage<V>(Schema<V> schema)
		{
			checkArgument(schema != null);
			return new TypedMessageBuilderImpl<V>(this, schema);
		}

		// TODO: add this method to the Producer interface
		// @Override
		public virtual TypedMessageBuilder<T> newMessage(Transaction txn)
		{
			checkArgument(txn is TransactionImpl);

			// check the producer has proper settings to send transactional messages
			if (conf.SendTimeoutMs > 0)
			{
				throw new System.ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
			}

			return new TypedMessageBuilderImpl<T>(this, schema, (TransactionImpl) txn);
		}

		internal abstract CompletableFuture<MessageId> internalSendAsync<T1>(Message<T1> message);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.apache.pulsar.client.api.MessageId send(org.apache.pulsar.client.api.Message<?> message) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual MessageId send<T1>(Message<T1> message)
		{
			try
			{
				// enqueue the message to the buffer
				CompletableFuture<MessageId> sendFuture = internalSendAsync(message);

				if (!sendFuture.Done)
				{
					// the send request wasn't completed yet (e.g. not failing at enqueuing), then attempt to triggerFlush it out
					triggerFlush();
				}

				return sendFuture.get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void flush() throws org.apache.pulsar.client.api.PulsarClientException
		public override void flush()
		{
			try
			{
				flushAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		internal abstract void triggerFlush();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.PulsarClientException
		public override void close()
		{
			try
			{
				closeAsync().get();
			}
			catch (Exception e)
			{
				throw PulsarClientException.unwrap(e);
			}
		}

		public override abstract CompletableFuture<Void> closeAsync();

		public override string Topic
		{
			get
			{
				return topic;
			}
		}

		public virtual ProducerConfigurationData Configuration
		{
			get
			{
				return conf;
			}
		}

		public virtual CompletableFuture<Producer<T>> producerCreatedFuture()
		{
			return producerCreatedFuture_Conflict;
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected org.apache.pulsar.client.api.Message<?> beforeSend(org.apache.pulsar.client.api.Message<?> message)
		protected internal virtual Message<object> beforeSend<T1>(Message<T1> message)
		{
			if (interceptors != null)
			{
				return interceptors.beforeSend(this, message);
			}
			else
			{
				return message;
			}
		}

		protected internal virtual void onSendAcknowledgement<T1>(Message<T1> message, MessageId msgId, Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.onSendAcknowledgement(this, message, msgId, exception);
			}
		}

		public override string ToString()
		{
			return "ProducerBase{" + "topic='" + topic + '\'' + '}';
		}

		public enum MultiSchemaMode
		{
			Auto,
			Enabled,
			Disabled
		}
	}

}