﻿using System;
using System.Collections.Generic;
using Akka.Actor;

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
namespace SharpPulsar.Interfaces
{

    /// <summary>
    /// A plugin interface that allows you to intercept (and possibly mutate)
    /// messages received by the consumer.
    /// 
    /// <para>A primary use case is to hook into consumer applications for custom
    /// monitoring, logging, etc.
    /// 
    /// </para>
    /// <para>Exceptions thrown by interceptor methods will be caught, logged, but
    /// not propagated further.
    /// </para>
    /// </summary>
    public interface IConsumerInterceptor<T>: IDisposable
	{
		/// <summary>
		/// <para>This method is allowed to modify message, in which case the new message
		/// will be returned.
		/// 
		/// </para>
		/// <para>Any exception thrown by this method will be caught by the caller, logged,
		/// but not propagated to client.
		/// 
		/// </para>
		/// <para>Since the consumer may run multiple interceptors, a particular
		/// interceptor's
		/// <tt>beforeConsume</tt> callback will be called in the order specified by
		/// <seealso cref="IConsumerBuilder.intercept(IConsumerInterceptor[])"/>. The first
		/// interceptor in the list gets the consumed message, the following
		/// interceptor will be passed
		/// the message returned by the previous interceptor, and so on. Since
		/// interceptors are allowed to modify message, interceptors may potentially
		/// get the messages already modified by other interceptors. However building a
		/// pipeline of mutable
		/// interceptors that depend on the output of the previous interceptor is
		/// discouraged, because of potential side-effects caused by interceptors
		/// potentially failing to modify the message and throwing an exception.
		/// if one of interceptors in the list throws an exception from
		/// <tt>beforeConsume</tt>, the exception is caught, logged,
		/// and the next interceptor is called with the message returned by the last
		/// successful interceptor in the list, or otherwise the original consumed
		/// message.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptor </param>
		/// <param name="message"> the message to be consumed by the client. </param>
		/// <returns> message that is either modified by the interceptor or same message
		///         passed into the method. </returns>
		IMessage<T> BeforeConsume(IActorRef consumer, IMessage<T> message);

		/// <summary>
		/// This is called consumer sends the acknowledgment to the broker.
		/// 
		/// <para>Any exception thrown by this method will be ignored by the caller.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptor </param>
		/// <param name="messageId"> message to ack, null if acknowledge fail. </param>
		/// <param name="exception"> the exception on acknowledge. </param>
		void OnAcknowledge(IActorRef consumer, IMessageId messageId, Exception exception);

		/// <summary>
		/// This is called consumer send the cumulative acknowledgment to the broker.
		/// 
		/// <para>Any exception thrown by this method will be ignored by the caller.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptor </param>
		/// <param name="messageId"> message to ack, null if acknowledge fail. </param>
		/// <param name="exception"> the exception on acknowledge. </param>
		void OnAcknowledgeCumulative(IActorRef consumer, IMessageId messageId, Exception exception);

		/// <summary>
		/// This method will be called when a redelivery from a negative acknowledge occurs.
		/// 
		/// <para>Any exception thrown by this method will be ignored by the caller.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptor </param>
		/// <param name="messageIds"> message to ack, null if acknowledge fail. </param>
		void OnNegativeAcksSend(IActorRef consumer, ISet<IMessageId> messageIds);

		/// <summary>
		/// This method will be called when a redelivery from an acknowledge timeout occurs.
		/// 
		/// <para>Any exception thrown by this method will be ignored by the caller.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer"> the consumer which contains the interceptor </param>
		/// <param name="messageIds"> message to ack, null if acknowledge fail. </param>
		void OnAckTimeoutSend(IActorRef consumer, ISet<IMessageId> messageIds);

        /// <summary>
		/// This method is called when partitions of the topic (partitioned-topic) changes.
		/// </summary>
		/// <param name="topicName"> topic name </param>
		/// <param name="partitions"> new updated number of partitions </param>
		void OnPartitionsChange(string topicName, int partitions)
        {
        }

    }

}