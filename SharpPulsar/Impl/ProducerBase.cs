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
namespace SharpPulsar.Impl
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using SharpPulsar.Api;
	using MessageId = SharpPulsar.Api.MessageId;
	using Producer = SharpPulsar.Api.Producer;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using SharpPulsar.Api;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using SharpPulsar.Api;
	using Transaction = SharpPulsar.Api.Transaction.Transaction;
	using ProducerConfigurationData = SharpPulsar.Impl.Conf.ProducerConfigurationData;
	using TransactionImpl = SharpPulsar.Impl.Transaction.TransactionImpl;
	using SchemaHash = Org.Apache.Pulsar.Common.Protocol.Schema.SchemaHash;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Org.Apache.Pulsar.Common.Util.Collections;

	public abstract class ProducerBase<T> : HandlerState, Producer<T>
	{
		public abstract bool Connected {get;}
		public abstract ProducerStats Stats {get;}
		public abstract long LastSequenceId {get;}
		public abstract CompletableFuture<Void> FlushAsync();
		public abstract CompletableFuture<MessageId> SendAsync(sbyte[] Message);
		public abstract MessageId Send(sbyte[] Message);
		public abstract string ProducerName {get;}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal readonly CompletableFuture<Producer<T>> ProducerCreatedFutureConflict;
		protected internal readonly ProducerConfigurationData Conf;
		protected internal readonly Schema<T> Schema;
		protected internal readonly ProducerInterceptors Interceptors;
		protected internal readonly ConcurrentOpenHashMap<SchemaHash, sbyte[]> SchemaCache;
		protected internal volatile MultiSchemaMode MultiSchemaMode = MultiSchemaMode.Auto;

		public ProducerBase(PulsarClientImpl Client, string Topic, ProducerConfigurationData Conf, CompletableFuture<Producer<T>> ProducerCreatedFuture, Schema<T> Schema, ProducerInterceptors Interceptors) : base(Client, Topic)
		{
			this.ProducerCreatedFutureConflict = ProducerCreatedFuture;
			this.Conf = Conf;
			this.Schema = Schema;
			this.Interceptors = Interceptors;
			this.SchemaCache = new ConcurrentOpenHashMap<SchemaHash, sbyte[]>();
			if (!Conf.MultiSchema)
			{
				MultiSchemaMode = MultiSchemaMode.Disabled;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public SharpPulsar.api.MessageId send(T message) throws SharpPulsar.api.PulsarClientException
		public override MessageId Send(T Message)
		{
			return NewMessage().value(Message).send();
		}

		public override CompletableFuture<MessageId> SendAsync(T Message)
		{
			try
			{
				return NewMessage().value(Message).sendAsync();
			}
			catch (SchemaSerializationException E)
			{
				return FutureUtil.failedFuture(E);
			}
		}

		public virtual CompletableFuture<MessageId> SendAsync<T1>(Message<T1> Message)
		{
			return InternalSendAsync(Message);
		}

		public override TypedMessageBuilder<T> NewMessage()
		{
			return new TypedMessageBuilderImpl<T>(this, Schema);
		}

		public virtual TypedMessageBuilder<V> NewMessage<V>(Schema<V> Schema)
		{
			checkArgument(Schema != null);
			return new TypedMessageBuilderImpl<V>(this, Schema);
		}

		// TODO: add this method to the Producer interface
		// @Override
		public virtual TypedMessageBuilder<T> NewMessage(Transaction Txn)
		{
			checkArgument(Txn is TransactionImpl);

			// check the producer has proper settings to send transactional messages
			if (Conf.SendTimeoutMs > 0)
			{
				throw new System.ArgumentException("Only producers disabled sendTimeout are allowed to" + " produce transactional messages");
			}

			return new TypedMessageBuilderImpl<T>(this, Schema, (TransactionImpl) Txn);
		}

		public abstract CompletableFuture<MessageId> internalSendAsync<T1>(Message<T1> Message);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SharpPulsar.api.MessageId send(SharpPulsar.api.Message<?> message) throws SharpPulsar.api.PulsarClientException
		public virtual MessageId Send<T1>(Message<T1> Message)
		{
			try
			{
				// enqueue the message to the buffer
				CompletableFuture<MessageId> SendFuture = InternalSendAsync(Message);

				if (!SendFuture.Done)
				{
					// the send request wasn't completed yet (e.g. not failing at enqueuing), then attempt to triggerFlush it out
					TriggerFlush();
				}

				return SendFuture.get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void flush() throws SharpPulsar.api.PulsarClientException
		public override void Flush()
		{
			try
			{
				FlushAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public abstract void TriggerFlush();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws SharpPulsar.api.PulsarClientException
		public override void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception E)
			{
				throw PulsarClientException.unwrap(E);
			}
		}

		public override abstract CompletableFuture<Void> CloseAsync();

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

		public virtual CompletableFuture<Producer<T>> ProducerCreatedFuture()
		{
			return ProducerCreatedFutureConflict;
		}

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected SharpPulsar.api.Message<?> beforeSend(SharpPulsar.api.Message<?> message)
		public virtual Message<object> BeforeSend<T1>(Message<T1> Message)
		{
			if (Interceptors != null)
			{
				return Interceptors.beforeSend(this, Message);
			}
			else
			{
				return Message;
			}
		}

		public virtual void OnSendAcknowledgement<T1>(Message<T1> Message, MessageId MsgId, Exception Exception)
		{
			if (Interceptors != null)
			{
				Interceptors.onSendAcknowledgement(this, Message, MsgId, Exception);
			}
		}

		public override string ToString()
		{
			return "ProducerBase{" + "topic='" + Topic + '\'' + '}';
		}

		public enum MultiSchemaMode
		{
			Auto,
			Enabled,
			Disabled
		}
	}

}