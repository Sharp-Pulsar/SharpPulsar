using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SharpPulsar.Api;

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
	/// <summary>
	/// A container that hold the list <seealso cref="IConsumerInterceptor{T}"/> and wraps calls to the chain
	/// of custom interceptors.
	/// </summary>
	public class ConsumerInterceptors<T> : IDisposable
    {

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(ConsumerInterceptors<T>));

		private readonly IList<IConsumerInterceptor<T>> _interceptors;

		public ConsumerInterceptors(IList<IConsumerInterceptor<T>> interceptors)
		{
			this._interceptors = interceptors;
		}

		/// <summary>
		/// This is called just before the message is returned by <seealso cref="IConsumer.receive()"/>,
		/// <seealso cref="IMessageListener.received(IConsumer, IMessage)"/> or the <seealso cref="java.util.concurrent.CompletableFuture"/>
		/// returned by <seealso cref="IConsumer.receiveAsync()"/> completes.
		/// <para>
		/// This method calls <seealso cref="IConsumerInterceptor.beforeConsume(IConsumer, IMessage)"/> for each interceptor. Messages returned
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
		public virtual IMessage<T> BeforeConsume(IConsumer<T> consumer, IMessage<T> message)
		{
			var interceptorMessage = message;
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					interceptorMessage = _interceptors[i].BeforeConsume(consumer, interceptorMessage);
				}
				catch (System.Exception e)
				{
					if (consumer != null)
					{
						Log.LogWarning("Error executing interceptor beforeConsume callback topic: {} consumerName: {}", consumer.Topic, consumer.ConsumerName, e);
					}
					else
					{
                        Log.LogWarning("Error executing interceptor beforeConsume callback", e);
					}
				}
			}
			return interceptorMessage;
		}

		/// <summary>
		/// This is called when acknowledge request return from the broker.
		/// <para>
		/// This method calls <seealso cref="IConsumerInterceptor.onAcknowledge(IConsumer, IMessageId, System.Exception)"/> method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors </param>
		/// <param name="messageId"> message to acknowledge. </param>
		/// <param name="exception"> exception returned by broker. </param>
		public virtual void OnAcknowledge(IConsumer<T> consumer, IMessageId messageId, System.Exception exception)
		{
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					_interceptors[i].OnAcknowledge(consumer, messageId, exception);
				}
				catch (System.Exception e)
				{
					Log.LogWarning("Error executing interceptor onAcknowledge callback ", e);
				}
			}
		}

		/// <summary>
		/// This is called when acknowledge cumulative request return from the broker.
		/// <para>
		/// This method calls <seealso cref="IConsumerInterceptor.onAcknowledgeCumulative(IConsumer, IMessageId, System.Exception)"/> (Message, Throwable)} method for each interceptor.
		/// </para>
		/// <para>
		/// This method does not throw exceptions. Exceptions thrown by any of interceptors in the chain are logged, but not propagated.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptors </param>
		/// <param name="messageId"> messages to acknowledge. </param>
		/// <param name="exception"> exception returned by broker. </param>
		public virtual void OnAcknowledgeCumulative(IConsumer<T> consumer, IMessageId messageId, System.Exception exception)
		{
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					_interceptors[i].OnAcknowledgeCumulative(consumer, messageId, exception);
				}
				catch (System.Exception e)
				{
					Log.LogWarning("Error executing interceptor onAcknowledgeCumulative callback ", e);
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
		public virtual void OnNegativeAcksSend(IConsumer<T> consumer, ISet<IMessageId> messageIds)
		{
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					_interceptors[i].OnNegativeAcksSend(consumer, messageIds);
				}
				catch (System.Exception e)
				{
					Log.LogWarning("Error executing interceptor onNegativeAcksSend callback", e);
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
		public virtual void OnAckTimeoutSend(IConsumer<T> consumer, ISet<IMessageId> messageIds)
		{
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					_interceptors[i].OnAckTimeoutSend(consumer, messageIds);
				}
				catch (System.Exception e)
				{
					Log.LogWarning("Error executing interceptor onAckTimeoutSend callback", e);
				}
			}
		}

		public void Close()
		{
			for (int i = 0, interceptorsSize = _interceptors.Count; i < interceptorsSize; i++)
			{
				try
				{
					_interceptors[i].Close();
				}
				catch (System.Exception e)
				{
					Log.LogError("Fail to close consumer interceptor ", e);
				}
			}
		}

        public void Dispose()
        {
           Close();
        }
    }

}