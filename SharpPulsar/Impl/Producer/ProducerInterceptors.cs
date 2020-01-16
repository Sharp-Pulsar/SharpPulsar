using SharpPulsar.Interface.Interceptor;
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
namespace SharpPulsar.Impl.Producer
{
	using Message = org.apache.pulsar.client.api.Message;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Producer = org.apache.pulsar.client.api.Producer;
	using ProducerInterceptor = org.apache.pulsar.client.api.interceptor.ProducerInterceptor;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// A container that holds the list<seealso cref="ProducerInterceptor"/>
	/// and wraps calls to the chain of custom interceptors.
	/// </summary>
	public class ProducerInterceptors : IDisposable
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProducerInterceptors));

		private readonly IList<ProducerInterceptor> interceptors;

		public ProducerInterceptors(IList<ProducerInterceptor> interceptors)
		{
			this.interceptors = interceptors;
		}

		/// <summary>
		/// This is called when client sends message to pulsar broker, before key and value gets serialized.
		/// The method calls <seealso cref="ProducerInterceptor.beforeSend(Producer,Message)"/> method. Message returned from
		/// first interceptor's beforeSend() is passed to the second interceptor beforeSend(), and so on in the
		/// interceptor chain. The message returned from the last interceptor is returned from this method.
		/// 
		/// This method does not throw exceptions. Exceptions thrown by any interceptor methods are caught and ignored.
		/// If a interceptor in the middle of the chain, that normally modifies the message, throws an exception,
		/// the next interceptor in the chain will be called with a message returned by the previous interceptor that did
		/// not throw an exception.
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> the message from client </param>
		/// <returns> the message to send to topic/partition </returns>
		public virtual Message BeforeSend(Producer producer, Message message)
		{
			Message interceptorMessage = message;
			foreach (ProducerInterceptor interceptor in interceptors)
			{
				if (!interceptor.eligible(message))
				{
					continue;
				}
				try
				{
					interceptorMessage = interceptor.beforeSend(producer, interceptorMessage);
				}
				catch (Exception e)
				{
					if (producer != null)
					{
						log.warn("Error executing interceptor beforeSend callback for topicName:{} ", producer.Topic, e);
					}
					else
					{
						log.warn("Error Error executing interceptor beforeSend callback ", e);
					}
				}
			}
			return interceptorMessage;
		}

		/// <summary>
		/// This method is called when the message send to the broker has been acknowledged, or when sending the record fails
		/// before it gets send to the broker.
		/// This method calls <seealso cref="ProducerInterceptor.onSendAcknowledgement(Producer, Message, MessageId, System.Exception)"/> method for
		/// each interceptor.
		/// 
		/// This method does not throw exceptions. Exceptions thrown by any of interceptor methods are caught and ignored.
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> The message returned from the last interceptor is returned from <seealso cref="ProducerInterceptor.beforeSend(Producer, Message)"/> </param>
		/// <param name="msgId"> The message id that broker returned. Null if has error occurred. </param>
		/// <param name="exception"> The exception thrown during processing of this message. Null if no error occurred. </param>
		public virtual void onSendAcknowledgement(Producer producer, Message message, MessageId msgId, Exception exception)
		{
			foreach (ProducerInterceptor interceptor in interceptors)
			{
				if (!interceptor.eligible(message))
				{
					continue;
				}
				try
				{
					interceptor.onSendAcknowledgement(producer, message, msgId, exception);
				}
				catch (Exception e)
				{
					log.warn("Error executing interceptor onSendAcknowledgement callback ", e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void Dispose()
		{
			foreach (ProducerInterceptor interceptor in interceptors)
			{
				try
				{
					interceptor.close();
				}
				catch (Exception e)
				{
					log.error("Fail to close producer interceptor ", e);
				}
			}
		}
	}

}