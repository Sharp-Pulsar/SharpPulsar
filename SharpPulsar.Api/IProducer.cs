using System;
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
namespace SharpPulsar.Api
{

	/// <summary>
	/// Producer is used to publish messages on a topic.
	/// 
	/// <para>A single producer instance can be used across multiple threads.
	/// </para>
	/// </summary>
	public interface IProducer<T> : IDisposable
	{

		/// <returns> the topic which producer is publishing to </returns>
		string Topic {get;}

		/// <returns> the producer name which could have been assigned by the system or specified by the client </returns>
		string ProducerName {get;}

		/// <summary>
		/// Sends a message.
		/// 
		/// <para>This call will be blocking until is successfully acknowledged by the Pulsar broker.
		/// 
		/// </para>
		/// <para>Use <seealso cref="newMessage()"/> to specify more properties than just the value on the message to be sent.
		/// 
		/// </para>
		/// </summary>
		/// <param name="message">
		///            a message </param>
		/// <returns> the message id assigned to the published message </returns>
		/// <exception cref="PulsarClientException.TimeoutException">
		///             if the message was not correctly received by the system within the timeout period </exception>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the producer was already closed </exception>
		IMessageId Send(T Message);

		/// <summary>
		/// Send a message asynchronously.
		/// 
		/// <para>When the producer queue is full, by default this method will complete the future with an exception
		/// <seealso cref="PulsarClientException.ProducerQueueIsFullError"/>
		/// 
		/// </para>
		/// <para>See <seealso cref="ProducerBuilder.maxPendingMessages(int)"/> to configure the producer queue size and
		/// <seealso cref="ProducerBuilder.blockIfQueueFull(bool)"/> to change the blocking behavior.
		/// 
		/// </para>
		/// <para>Use <seealso cref="newMessage()"/> to specify more properties than just the value on the message to be sent.
		/// 
		/// </para>
		/// </summary>
		/// <param name="message">
		///            a byte array with the payload of the message </param>
		/// <returns> a future that can be used to track when the message will have been safely persisted </returns>
		ValueTask<IMessageId> SendAsync(T Message);

		/// <summary>
		/// Flush all the messages buffered in the client and wait until all messages have been successfully persisted.
		/// </summary>
		/// <exception cref="PulsarClientException">
		/// @since 2.1.0 </exception>
		/// <seealso cref= #flushAsync() </seealso>
		void Flush();

		/// <summary>
		/// Flush all the messages buffered in the client and wait until all messages have been successfully persisted.
		/// </summary>
		/// <returns> a future that can be used to track when all the messages have been safely persisted.
		/// @since 2.1.0 </returns>
		/// <seealso cref= #flush() </seealso>
		ValueTask FlushAsync();

		/// <summary>
		/// Create a new message builder.
		/// 
		/// <para>This message builder allows to specify additional properties on the message. For example:
		/// <pre>{@code
		/// producer.newMessage()
		///       .key(messageKey)
		///       .value(myValue)
		///       .property("user-defined-property", "value")
		///       .send();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a typed message builder that can be used to construct the message to be sent through this producer </returns>
		TypedMessageBuilder<T> NewMessage();

		/// <summary>
		/// Create a new message builder with schema, not required same parameterized type with the producer.
		/// </summary>
		/// <returns> a typed message builder that can be used to construct the message to be sent through this producer </returns>
		/// <seealso cref= #newMessage() </seealso>
		TypedMessageBuilder<V> NewMessage<V>(Schema<V> Schema);

		/// <summary>
		/// Get the last sequence id that was published by this producer.
		/// 
		/// <para>This represent either the automatically assigned
		/// or custom sequence id (set on the <seealso cref="TypedMessageBuilder"/>)
		/// that was published and acknowledged by the broker.
		/// 
		/// </para>
		/// <para>After recreating a producer with the same producer name, this will return the last message that was
		/// published in the previous producer session, or -1 if there no message was ever published.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the last sequence id published by this producer </returns>
		long LastSequenceId {get;}

		/// <summary>
		/// Get statistics for the producer.
		/// <ul>
		/// <li>numMsgsSent : Number of messages sent in the current interval
		/// <li>numBytesSent : Number of bytes sent in the current interval
		/// <li>numSendFailed : Number of messages failed to send in the current interval
		/// <li>numAcksReceived : Number of acks received in the current interval
		/// <li>totalMsgsSent : Total number of messages sent
		/// <li>totalBytesSent : Total number of bytes sent
		/// <li>totalSendFailed : Total number of messages failed to send
		/// <li>totalAcksReceived: Total number of acks received
		/// </ul>
		/// </summary>
		/// <returns> statistic for the producer or null if ProducerStatsRecorderImpl is disabled. </returns>
		ProducerStats Stats {get;}

		/// <summary>
		/// Close the producer and releases resources allocated.
		/// 
		/// <para>No more writes will be accepted from this producer. Waits until all pending write request are persisted.
		/// In case of errors, pending writes will not be retried.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the producer was already closed </exception>
		void Close();

		/// <summary>
		/// Close the producer and releases resources allocated.
		/// 
		/// <para>No more writes will be accepted from this producer. Waits until all pending write request are persisted.
		/// In case of errors, pending writes will not be retried.
		/// 
		/// </para>
		/// </summary>
		/// <returns> a future that can used to track when the producer has been closed </returns>
		ValueTask CloseAsync();

		/// <returns> Whether the producer is currently connected to the broker </returns>
		bool Connected {get;}
	}

}