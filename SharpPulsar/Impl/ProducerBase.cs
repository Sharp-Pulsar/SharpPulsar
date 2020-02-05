using SharpPulsar.Api;
using SharpPulsar.Api.Transaction;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Impl.Transaction;
using SharpPulsar.Protocol.Schema;
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
namespace SharpPulsar.Impl
{

	public abstract class ProducerBase<T> : HandlerState, IProducer<T>
	{
		public abstract bool Connected { get; set; }
		public abstract IProducerStatsRecorder Stats { get; set; }
		public abstract long LastSequenceId { get; set; }
		public abstract ValueTask FlushAsync();
		public abstract TaskCompletionSource<IMessageId> SendAsync(sbyte[] Message);
		public abstract IMessageId Send(sbyte[] Message);
		public abstract string ProducerName { get; set; }
		public MultiSchemaMode MultiSchemaMode;

		protected internal readonly TaskCompletionSource<IProducer<T>> _producerCreatedTask;
		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly ISchema<T> Schema;
		protected internal readonly ProducerInterceptors Interceptors;
		protected internal readonly ConcurrentDictionary<SchemaHash, sbyte[]> SchemaCache;
		public MultiSchemaMode ProducerMultiSchemaMode;


		public ProducerBase(PulsarClientImpl Client, string Topic, ProducerConfigurationData Conf, TaskCompletionSource<IProducer<T>> ProducerCreatedFuture, ISchema<T> Schema, ProducerInterceptors Interceptors) : base(Client, Topic)
		{
			_producerCreatedTask = ProducerCreatedFuture;
			this.Conf = Conf;
			this.Schema = Schema;
			this.Interceptors = Interceptors;
			this.SchemaCache = new ConcurrentDictionary<SchemaHash, sbyte[]>();
			if (!Conf.MultiSchema)
			{
				ProducerMultiSchemaMode = MultiSchemaMode.Disabled;
			}
		}
		
		public IMessageId Send(T Message)
		{
			return NewMessage().Value(Message).Send();
		}

		public async ValueTask<IMessageId> SendAsync(T Message)
		{
			try
			{
				return await NewMessage().Value(Message).SendAsync();
			}
			catch (SchemaSerializationException e)
			{
				return Task.FromException<IMessageId>(e).Result;
			}
		}

		public virtual TaskCompletionSource<IMessageId> SendAsync<T1>(Message<T1> Message)
		{
			return InternalSendAsync(Message);
		}

		public  ITypedMessageBuilder<T> NewMessage()
		{
			return new TypedMessageBuilderImpl<T>(this, Schema);
		}

		public virtual ITypedMessageBuilder<T> NewMessage(ISchema<T> schema)
		{
			if (schema == null)
				throw new  NullReferenceException("Schema is null");
			return new TypedMessageBuilderImpl<T>(this, schema);
		}

		// TODO: add this method to the Producer interface
		// @Override
		public virtual ITypedMessageBuilder<T> NewMessage(ITransaction txn)
		{
			if(txn is TransactionImpl)
			{
				// check the producer has proper settings to send transactional messages
				if (Conf.SendTimeoutMs > 0)
				{
					throw new ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
				}

				return new TypedMessageBuilderImpl<T>(this, Schema, (TransactionImpl)txn);
			}
			throw new ArgumentException("Only transactional messages supported");

		}

		public abstract TaskCompletionSource<IMessageId> InternalSendAsync<T1>(Message<T1> Message);
		public virtual IMessageId Send<T1>(Message<T1> Message)
		{
			try
			{
				// enqueue the message to the buffer
				TaskCompletionSource<IMessageId> sendTask = InternalSendAsync(Message);

				if (!sendTask.Task.IsCompleted)
				{
					// the send request wasn't completed yet (e.g. not failing at enqueuing), then attempt to triggerFlush it out
					TriggerFlush();
				}

				return sendTask.Task.Result ;
			}
			catch (System.Exception E)
			{
				throw PulsarClientException.Unwrap(E);
			}
		}

		public void Flush()
		{
			try
			{
				FlushAsync();
			}
			catch (System.Exception E)
			{
				throw PulsarClientException.Unwrap(E);
			}
		}

		public abstract void TriggerFlush();
		public void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception E)
			{
				throw PulsarClientException.Unwrap(E);
			}
		}

		public abstract ValueTask CloseAsync();

		public virtual string Topic
		{
			get
			{
				return Topic;
			}
		}

		public virtual ProducerConfigurationData Configuration
		{
			get
			{
				return Conf;
			}
		}

		public virtual TaskCompletionSource<IProducer<T>> ProducerCreated()
		{
			return _producerCreatedTask;
		}

		public virtual Message<T> BeforeSend(Message<T> message)
		{
			if (Interceptors != null)
			{
				return Interceptors.BeforeSend(this, message);
			}
			else
			{
				return message;
			}
		}

		public virtual void OnSendAcknowledgement(Message<T> message, IMessageId msgId, System.Exception exception)
		{
			if (Interceptors != null)
			{
				Interceptors.OnSendAcknowledgement(this, message, msgId,exception);
			}
		}

		public override string ToString()
		{
			return "ProducerBase{" + "topic='" + Topic + '\'' + '}';
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}
		

        public static explicit operator ProducerBase<object>(ProducerBase<T> v)
        {
            throw new NotImplementedException();
        }

        /*public static explicit operator ProducerBase<object>(ProducerBase<T> v)
        {
            throw new NotImplementedException();
        }

        public static explicit operator ProducerBase<T>(ProducerBase<T> v)
        {
            throw new NotImplementedException();
        }*/
    }
	public enum MultiSchemaMode
	{
		Auto,
		Enabled,
		Disabled
	}
}