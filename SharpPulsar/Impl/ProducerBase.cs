using SharpPulsar.Api;
using SharpPulsar.Api.Transaction;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Protocol.Schema;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;

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

	public abstract class ProducerBase<T> : HandlerState, IProducer<T>
	{
		public abstract bool Connected { get; set; }
		public  IProducerStatsRecorder Stats { get; set;}
		public long LastSequenceId { get; set; }
		public abstract ValueTask FlushAsync();
		public abstract string ProducerName { get; set; }
		public MultiSchemaMode MultiSchemaMode;

		protected internal readonly TaskCompletionSource<IProducer<T>> ProducerCreatedTask;
		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ProducerInterceptors Interceptors;
		protected internal readonly ConcurrentDictionary<SchemaHash, sbyte[]> SchemaCache;
		public MultiSchemaMode ProducerMultiSchemaMode;


        protected ProducerBase(PulsarClientImpl client, string topic, ProducerConfigurationData conf, TaskCompletionSource<IProducer<T>> producerCreatedTask, ISchema<T> schema, ProducerInterceptors interceptors) : base(client, topic)
        {
			Stats = new ProducerStatsRecorderImpl<T>(client, conf, this);
			ProducerCreatedTask = producerCreatedTask;
			Conf = conf;
			Schema = schema;
			Interceptors = interceptors;
			SchemaCache = new ConcurrentDictionary<SchemaHash, sbyte[]>();
			if (!conf.MultiSchema)
			{
				ProducerMultiSchemaMode = MultiSchemaMode.Disabled;
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
				return Task.FromException<IMessageId>(e).Result;
			}
		}

		public virtual TaskCompletionSource<IMessageId> SendAsync(IMessage message)
		{
			return InternalSendAsync(message);
		}

		public  ITypedMessageBuilder NewMessage()
		{
			return new TypedMessageBuilderImpl("", Schema);
		}

		public virtual ITypedMessageBuilder NewMessage(ISchema schema)
		{
			if (schema == null)
				throw new  NullReferenceException("Schema is null");
			return new TypedMessageBuilderImpl("", schema);
		}

		// TODO: add this method to the Producer interface
		// @Override
		public virtual ITypedMessageBuilder NewMessage(ITransaction txn)
		{
            if (!(txn is TransactionImpl impl)) throw new ArgumentException("Only transactional messages supported");
            // check the producer has proper settings to send transactional messages
            if (Conf.SendTimeoutMs > 0)
            {
                throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
            }

            return new TypedMessageBuilderImpl("", Schema, impl);

        }

		public abstract TaskCompletionSource<IMessageId> InternalSendAsync(IMessage message);
		public virtual IMessageId Send(IMessage message)
		{
			try
			{
				// enqueue the message to the buffer
				var sendTask = InternalSendAsync(message);

				if (!sendTask.Task.IsCompleted)
				{
					// the send request wasn't completed yet (e.g. not failing at enqueuing), then attempt to triggerFlush it out
					TriggerFlush();
				}

				return sendTask.Task.Result ;
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public void Flush()
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

		public abstract void TriggerFlush();
		public void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception e)
			{
				throw PulsarClientException.Unwrap(e);
			}
		}

		public abstract ValueTask CloseAsync();

        public new virtual string Topic { get; set; }

        public virtual ProducerConfigurationData Configuration => Conf;


        public virtual TaskCompletionSource<IProducer<T>> ProducerCreated()
		{
			return ProducerCreatedTask;
		}

		public virtual IMessage BeforeSend(IMessage message)
        {
            if (Interceptors != null)
			{
				return Interceptors.BeforeSend(this, message);
			}

            return message;
        }

		public virtual void OnSendAcknowledgement(IMessage message, IMessageId msgId, System.Exception exception)
        {
            Interceptors?.OnSendAcknowledgement(this, message, msgId,exception);
        }

		public override string ToString()
		{
			return "ProducerBase{" + "topic='" + Topic + '\'' + '}';
		}

        public ValueTask DisposeAsync()
        {
            return CloseAsync();
        }

		
    }
	public enum MultiSchemaMode
	{
		Auto,
		Enabled,
		Disabled
	}
}