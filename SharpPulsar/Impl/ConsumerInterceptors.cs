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
	using Consumer = SharpPulsar.Api.IConsumer;
	using SharpPulsar.Api;
	using SharpPulsar.Api;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using SharpPulsar.Api;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// A container that hold the list <seealso cref="SharpPulsar.api.ConsumerInterceptor"/> and wraps calls to the chain
	/// of custom interceptors.
	/// </summary>
	public class ConsumerInterceptors<T> : System.IDisposable
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ConsumerInterceptors));

		private readonly IList<ConsumerInterceptor<T>> interceptors;

		public ConsumerInterceptors(IList<ConsumerInterceptor<T>> Interceptors)
		{
			this.interceptors = Interceptors;
		}

		/// <summary>
		/// This is called just before the message is returned by <seealso cref="IConsumer.receive()"/>,
		/// <seealso cref="MessageListener.received(IConsumer, Message)"/> or the <seealso cref="java.util.concurrent.CompletableFuture"/>
		/// returned by <seealso cref="IConsumer.receiveAsync()"/> completes.
		/// <para>
		/// This method calls <seealso cref="ConsumerInterceptor.beforeConsume(IConsumer, Message)"/> for each interceptor. Messages returned
		/// from each interceptor get passed to beforeConsume() of the next interceptor in the chain of interceptors.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. If any of the interceptors in the chain throws an exception, it gets
		/// caught and logged, and next interceptor in int the chain is called with 'messages' returned by the previous
		/// successful interceptor beforeConsume call.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors </param>
		/// <param name="message"> message to be consume by the client. </param>
		/// <returns> messages that are either modified by interceptors or same as messages passed to this method. </returns>
		public virtual Message<T> BeforeConsume(IConsumer<T> Consumer, Message<T> Message)
		{
			Message<T> InterceptorMessage = Message;
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					InterceptorMessage = interceptors[I].beforeConsume(Consumer, InterceptorMessage);
				}
				catch (Exception E)
				{
					if (Consumer != null)
					{
						log.warn("Error executing interceptor beforeConsume callback topic: {} consumerName: {}", Consumer.Topic, Consumer.ConsumerName, E);
					}
					else
					{
						log.warn("Error executing interceptor beforeConsume callback", E);
					}
				}
			}
			return InterceptorMessage;
		}

		/// <summary>
		/// This is called when acknowledge request return from the broker.
		/// <para>
		/// This method calls <seealso cref="ConsumerInterceptor.onAcknowledge(IConsumer, IMessageId, System.Exception)"/> method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors </param>
		/// <param name="messageId"> message to acknowledge. </param>
		/// <param name="exception"> exception returned by broker. </param>
		public virtual void OnAcknowledge(IConsumer<T> Consumer, IMessageId MessageId, Exception Exception)
		{
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					interceptors[I].onAcknowledge(Consumer, MessageId, Exception);
				}
				catch (Exception E)
				{
					log.warn("Error executing interceptor onAcknowledge callback ", E);
				}
			}
		}

		/// <summary>
		/// This is called when acknowledge cumulative request return from the broker.
		/// <para>
		/// This method calls <seealso cref="ConsumerInterceptor.onAcknowledgeCumulative(IConsumer, IMessageId, System.Exception)"/> (Message, Throwable)} method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors </param>
		/// <param name="messageId"> messages to acknowledge. </param>
		/// <param name="exception"> exception returned by broker. </param>
		public virtual void OnAcknowledgeCumulative(IConsumer<T> Consumer, IMessageId MessageId, Exception Exception)
		{
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					interceptors[I].onAcknowledgeCumulative(Consumer, MessageId, Exception);
				}
				catch (Exception E)
				{
					log.warn("Error executing interceptor onAcknowledgeCumulative callback ", E);
				}
			}
		}

		/// <summary>
		/// This is called when a redelivery from a negative acknowledge occurs.
		/// <para>
		/// This method calls {@link ConsumerInterceptor#onNegativeAcksSend(Consumer, Set)
		/// onNegativeAcksSend(Consumer, Set&lt;MessageId&gt;)} method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors. </param>
		/// <param name="messageIds"> set of message IDs being redelivery due a negative acknowledge. </param>
		public virtual void OnNegativeAcksSend(IConsumer<T> Consumer, ISet<IMessageId> MessageIds)
		{
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					interceptors[I].onNegativeAcksSend(Consumer, MessageIds);
				}
				catch (Exception E)
				{
					log.warn("Error executing interceptor onNegativeAcksSend callback", E);
				}
			}
		}

		/// <summary>
		/// This is called when a redelivery from an acknowledge timeout occurs.
		/// <para>
		/// This method calls {@link ConsumerInterceptor#onAckTimeoutSend(Consumer, Set)
		/// onAckTimeoutSend(Consumer, Set&lt;MessageId&gt;)} method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors. </param>
		/// <param name="messageIds"> set of message IDs being redelivery due an acknowledge timeout. </param>
		public virtual void OnAckTimeoutSend(IConsumer<T> Consumer, ISet<IMessageId> MessageIds)
		{
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					interceptors[I].onAckTimeoutSend(Consumer, MessageIds);
				}
				catch (Exception E)
				{
					log.warn("Error executing interceptor onAckTimeoutSend callback", E);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
			for (int I = 0, interceptorsSize = interceptors.Count; I < interceptorsSize; I++)
			{
				try
				{
					interceptors[I].close();
				}
				catch (Exception E)
				{
					log.error("Fail to close consumer interceptor ", E);
				}
			}
		}

	}

}