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
namespace SharpPulsar.Api.Interceptor
{

	/// <summary>
	/// A plugin interface that allows you to intercept (and possibly mutate) the
	/// messages received by the producer before they are published to the Pulsar
	/// brokers.
	/// 
	/// <para>Exceptions thrown by ProducerInterceptor methods will be caught, logged, but
	/// not propagated further.
	/// 
	/// </para>
	/// <para>ProducerInterceptor callbacks may be called from multiple threads. Interceptor
	/// implementation must ensure thread-safety, if needed.
	/// 
	/// </para>
	/// <para>Since the producer may run multiple interceptors, a particular
	/// interceptor will be called in the order specified by
	/// <seealso cref="IProducerBuilder.intercept(IProducerInterceptor...)"/>.
	/// </para>
	/// </summary>
	public interface IProducerInterceptor : IDisposable
	{

		/// <summary>
		/// Close the interceptor.
		/// </summary>
		void Close();

		/// <summary>
		/// Check whether the interceptor is eligible for this message.
		/// </summary>
		/// <param name="message"> message to send </param>
		/// <returns> whether the interceptor can be applied to this particular message. </returns>
		bool Eligible<T>(Message<T> message);

		/// <summary>
		/// This is called from <seealso cref="IProducer.send(object)"/> and {@link
		/// Producer#sendAsync(Object)} methods, before
		/// send the message to the brokers. This method is allowed to modify the
		/// record, in which case, the new record
		/// will be returned.
		/// 
		/// <para>Any exception thrown by this method will be caught by the caller and
		/// logged, but not propagated further.
		/// 
		/// </para>
		/// <para>Each interceptor will be passed the message returned by the
		/// last successful executed interceptor or the original message if it is the first one.
		/// Since interceptors are allowed to modify messages,
		/// interceptors may potentially get the message already modified by other interceptors.
		/// However, building a pipeline of mutable interceptors
		/// that depend on the output of the previous interceptor is discouraged,
		/// because of potential side-effects caused by interceptors potentially
		/// failing to modify the message and throwing an exception.
		/// 
		/// </para>
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> message to send </param>
		/// <returns> the intercepted message </returns>
		Message<T> BeforeSend<T>(IProducer<T> producer, Message<T> message);

		/// <summary>
		/// This method is called when the message sent to the broker has been
		/// acknowledged, or when sending the message fails.
		/// This method is generally called just before the user callback is
		/// called, and in additional cases when an exception on the producer side.
		/// 
		/// <para>Any exception thrown by this method will be ignored by the caller.
		/// 
		/// </para>
		/// <para>This method will generally execute in the background I/O thread, so the
		/// implementation should be reasonably fast. Otherwise, sending of messages
		/// from other threads could be delayed.
		/// 
		/// </para>
		/// </summary>
		/// <param name="producer"> the producer which contains the interceptor. </param>
		/// <param name="message"> the message that application sends </param>
		/// <param name="msgId"> the message id that assigned by the broker; null if send failed. </param>
		/// <param name="exception"> the exception on sending messages, null indicates send has succeed. </param>
		void OnSendAcknowledgement<T>(IProducer<T> producer, Message<T> message, IMessageId msgId, System.Exception exception);
	}

}