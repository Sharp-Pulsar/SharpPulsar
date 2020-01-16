using SharpPulsar.Common.Protocol.Schema;
using SharpPulsar.Configuration;
using SharpPulsar.Exception;
using SharpPulsar.Interface;
using SharpPulsar.Interface.Message;
using SharpPulsar.Interface.Schema;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
namespace SharpPulsar.Impl.Producer
{

	public abstract class ProducerBase<T> : HandlerState, Producer<T>
	{

	
		protected internal readonly CompletableFuture<Producer<T>> producerCreatedFuture_Conflict;
		protected internal readonly ProducerConfigurationData conf;
		protected internal readonly ISchema<T> schema;
		protected internal readonly ProducerInterceptors interceptors;
		protected internal readonly ConcurrentDictionary<SchemaHash, sbyte[]> schemaCache;
		protected internal volatile MultiSchemaMode multiSchemaMode = MultiSchemaMode.Auto;
		private string topic;

		protected internal ProducerBase(PulsarClientImpl client, string topic, ProducerConfigurationData conf, CompletableFuture<Producer<T>> producerCreatedFuture, ISchema<T> schema, ProducerInterceptors interceptors) : base(client, topic)
		{
			this.producerCreatedFuture_Conflict = producerCreatedFuture;
			this.conf = conf;
			this.schema = schema;
			this.interceptors = interceptors;
			this.schemaCache = new ConcurrentDictionary<SchemaHash, sbyte[]>();
			if (!conf.MultiSchema)
			{
				multiSchemaMode = MultiSchemaMode.Disabled;
			}
		}
		public IMessageId Send(T message)
		{
			return NewMessage().Value(message).Send();
		}

		public async ValueTask<IMessageId> SendAsync(T message)
		{
			try
			{
				return await NewMessage().Value(message).SendAsync();
			}
			catch (SchemaSerializationException e)
			{
				return FutureUtil.failedFuture(e);
			}
		}

		public virtual async ValueTask<IMessageId> SendAsync<T1>(IMessage<T1> message)
		{
			return await InternalSendAsync(message);
		}

		public ITypedMessageBuilder<T> NewMessage()
		{
			return new TypedMessageBuilderImpl<T>(this, schema);
		}

		public virtual ITypedMessageBuilder<V> NewMessage<V>(ISchema<V> schema)
		{
			checkArgument(schema != null);
			return new TypedMessageBuilderImpl<V>(this, schema);
		}

		
		public virtual ITypedMessageBuilder<T> NewMessage(Transaction txn)
		{
			checkArgument(txn is TransactionImpl);

			// check the producer has proper settings to send transactional messages
			if (conf.SendTimeoutMs > 0)
			{
				throw new System.ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
			}

			return new TypedMessageBuilderImpl<T>(this, schema, (TransactionImpl) txn);
		}

		internal abstract ValueTask<IMessageId> InternalSendAsync<T1>(IMessage<T1> message);

		public virtual IMessageId Send<T1>(IMessage<T1> message)
		{
			try
			{
				// enqueue the message to the buffer
				var sendFuture = InternalSendAsync(message);

				if (!sendFuture.IsCompleted)
				{
					// the send request wasn't completed yet (e.g. not failing at enqueuing), then attempt to triggerFlush it out
					triggerFlush();
				}

				return sendFuture.Result;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public  void Flush()
		{
			try
			{
				FlushAsync();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		internal abstract void TriggerFlush();
		public void Close()
		{
			try
			{
				CloseAsync().GetAwaiter();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public abstract ValueTask CloseAsync();

		public string Topic
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

		public virtual ValueTask<Producer<T>> ProducerCreatedAsync()
		{
			return producerCreatedFuture_Conflict;
		}

		protected internal virtual IMessage<object> BeforeSend<T1>(IMessage<T1> message)
		{
			if (interceptors != null)
			{
				return interceptors.beforeSend(this, message);
			}
			else
			{
				return (IMessage<object>)message;
			}
		}

		protected internal virtual void OnSendAcknowledgement<T1>(IMessage<T1> message, IMessageId msgId,System.Exception exception)
		{
			if (interceptors != null)
			{
				interceptors.onSendAcknowledgement(this, message, msgId, exception);
			}
		}

		public string ToString()
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