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
	/// An interface that abstracts behavior of Pulsar's consumer.
	/// 
	/// <para>All the operations on the consumer instance are thread safe.
	/// </para>
	/// </summary>
	public interface Consumer<T> : System.IDisposable
	{

		/// <summary>
		/// Get a topic for the consumer.
		/// </summary>
		/// <returns> topic for the consumer </returns>
		string Topic {get;}

		/// <summary>
		/// Get a subscription for the consumer.
		/// </summary>
		/// <returns> subscription for the consumer </returns>
		string Subscription {get;}

		/// <summary>
		/// Unsubscribe the consumer.
		/// 
		/// <para>This call blocks until the consumer is unsubscribed.
		/// 
		/// </para>
		/// <para>Unsubscribing will the subscription to be deleted and all the
		/// data retained can potentially be deleted as well.
		/// 
		/// </para>
		/// <para>The operation will fail when performed on a shared subscription
		/// where multiple consumers are currently connected.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="PulsarClientException"> if the operation fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void unsubscribe() throws PulsarClientException;
		void Unsubscribe();

		/// <summary>
		/// Asynchronously unsubscribe the consumer.
		/// </summary>
		/// <seealso cref= Consumer#unsubscribe() </seealso>
		/// <returns> <seealso cref="ValueTask"/> to track the operation </returns>
		ValueTask<Void> UnsubscribeAsync();

		/// <summary>
		/// Receives a single message.
		/// 
		/// <para>This calls blocks until a message is available.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the received message </returns>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
		/// <exception cref="PulsarClientException.InvalidConfigurationException">
		///             if a message listener was defined in the configuration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Message<T> receive() throws PulsarClientException;
		Message<T> Receive();

		/// <summary>
		/// Receive a single message
		/// 
		/// <para>Retrieves a message when it will be available and completes <seealso cref="ValueTask"/> with received message.
		/// 
		/// </para>
		/// <para>{@code receiveAsync()} should be called subsequently once returned {@code ValueTask} gets complete
		/// with received message. Else it creates <i> backlog of receive requests </i> in the application.
		/// 
		/// </para>
		/// </summary>
		/// <returns> <seealso cref="ValueTask"/><<seealso cref="Message"/>> will be completed when message is available </returns>
		ValueTask<Message<T>> ReceiveAsync();

		/// <summary>
		/// Receive a single message.
		/// 
		/// <para>Retrieves a message, waiting up to the specified wait time if necessary.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timeout">
		///            0 or less means immediate rather than infinite </param>
		/// <param name="unit"> </param>
		/// <returns> the received <seealso cref="Message"/> or null if no message available before timeout </returns>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
		/// <exception cref="PulsarClientException.InvalidConfigurationException">
		///             if a message listener was defined in the configuration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Message<T> receive(int timeout, java.util.concurrent.TimeUnit unit) throws PulsarClientException;
		Message<T> Receive(int Timeout, TimeUnit Unit);

		/// <summary>
		/// Batch receiving messages.
		/// 
		/// <para>This calls blocks until has enough messages or wait timeout, more details to see <seealso cref="BatchReceivePolicy"/>.
		/// 
		/// </para>
		/// </summary>
		/// <returns> messages
		/// @since 2.4.1 </returns>
		/// <exception cref="PulsarClientException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Messages<T> batchReceive() throws PulsarClientException;
		Messages<T> BatchReceive();

		/// <summary>
		/// Batch receiving messages.
		/// <para>
		/// Retrieves messages when has enough messages or wait timeout and
		/// completes <seealso cref="ValueTask"/> with received messages.
		/// </para>
		/// <para>
		/// {@code batchReceiveAsync()} should be called subsequently once returned {@code ValueTask} gets complete
		/// with received messages. Else it creates <i> backlog of receive requests </i> in the application.
		/// </para> </summary>
		/// <returns> messages
		/// @since 2.4.1 </returns>
		/// <exception cref="PulsarClientException"> </exception>
		ValueTask<Messages<T>> BatchReceiveAsync();

		/// <summary>
		/// Acknowledge the consumption of a single message.
		/// </summary>
		/// <param name="message">
		///            The {@code Message} to be acknowledged </param>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acknowledge(Message<?> message) throws PulsarClientException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		void acknowledge<T1>(Message<T1> Message);

		/// <summary>
		/// Acknowledge the consumption of a single message, identified by its <seealso cref="MessageId"/>.
		/// </summary>
		/// <param name="messageId">
		///            The <seealso cref="MessageId"/> to be acknowledged </param>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acknowledge(MessageId messageId) throws PulsarClientException;
		void Acknowledge(MessageId MessageId);

		/// <summary>
		/// Acknowledge the consumption of <seealso cref="Messages"/>.
		/// </summary>
		/// <param name="messages"> messages </param>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///              if the consumer was already closed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acknowledge(Messages<?> messages) throws PulsarClientException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		void acknowledge<T1>(Messages<T1> Messages);

		/// <summary>
		/// Acknowledge the failure to process a single message.
		/// 
		/// <para>When a message is "negatively acked" it will be marked for redelivery after
		/// some fixed delay. The delay is configurable when constructing the consumer
		/// with <seealso cref="IConsumerBuilder.negativeAckRedeliveryDelay(long, TimeUnit)"/>.
		/// 
		/// </para>
		/// <para>This call is not blocking.
		/// 
		/// </para>
		/// <para>Example of usage:
		/// <pre><code>
		/// while (true) {
		///     Message&lt;String&gt; msg = consumer.receive();
		/// 
		///     try {
		///          // Process message...
		/// 
		///          consumer.acknowledge(msg);
		///     } catch (Throwable t) {
		///          log.warn("Failed to process message");
		///          consumer.negativeAcknowledge(msg);
		///     }
		/// }
		/// </code></pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="message">
		///            The {@code Message} to be acknowledged </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void negativeAcknowledge(Message<?> message);
		void negativeAcknowledge<T1>(Message<T1> Message);

		/// <summary>
		/// Acknowledge the failure to process a single message.
		/// 
		/// <para>When a message is "negatively acked" it will be marked for redelivery after
		/// some fixed delay. The delay is configurable when constructing the consumer
		/// with <seealso cref="IConsumerBuilder.negativeAckRedeliveryDelay(long, TimeUnit)"/>.
		/// 
		/// </para>
		/// <para>This call is not blocking.
		/// 
		/// </para>
		/// <para>This variation allows to pass a <seealso cref="MessageId"/> rather than a <seealso cref="Message"/>
		/// object, in order to avoid keeping the payload in memory for extended amount
		/// of time
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref= #negativeAcknowledge(Message)
		/// </seealso>
		/// <param name="messageId">
		///            The {@code MessageId} to be acknowledged </param>
		void NegativeAcknowledge(MessageId MessageId);

		/// <summary>
		/// Acknowledge the failure to process <seealso cref="Messages"/>.
		/// 
		/// <para>When messages is "negatively acked" it will be marked for redelivery after
		/// some fixed delay. The delay is configurable when constructing the consumer
		/// with <seealso cref="IConsumerBuilder.negativeAckRedeliveryDelay(long, TimeUnit)"/>.
		/// 
		/// </para>
		/// <para>This call is not blocking.
		/// 
		/// </para>
		/// <para>Example of usage:
		/// <pre><code>
		/// while (true) {
		///     Messages&lt;String&gt; msgs = consumer.batchReceive();
		/// 
		///     try {
		///          // Process message...
		/// 
		///          consumer.acknowledge(msgs);
		///     } catch (Throwable t) {
		///          log.warn("Failed to process message");
		///          consumer.negativeAcknowledge(msgs);
		///     }
		/// }
		/// </code></pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="messages">
		///            The {@code Message} to be acknowledged </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void negativeAcknowledge(Messages<?> messages);
		void negativeAcknowledge<T1>(Messages<T1> Messages);

		/// <summary>
		/// Acknowledge the reception of all the messages in the stream up to (and including) the provided message.
		/// 
		/// <para>This method will block until the acknowledge has been sent to the broker. After that, the messages will not be
		/// re-delivered to this consumer.
		/// 
		/// </para>
		/// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
		/// 
		/// </para>
		/// <para>It's equivalent to calling asyncAcknowledgeCumulative(Message) and waiting for the callback to be triggered.
		/// 
		/// </para>
		/// </summary>
		/// <param name="message">
		///            The {@code Message} to be cumulatively acknowledged </param>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acknowledgeCumulative(Message<?> message) throws PulsarClientException;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		void acknowledgeCumulative<T1>(Message<T1> Message);

		/// <summary>
		/// Acknowledge the reception of all the messages in the stream up to (and including) the provided message.
		/// 
		/// <para>This method will block until the acknowledge has been sent to the broker. After that, the messages will not be
		/// re-delivered to this consumer.
		/// 
		/// </para>
		/// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
		/// 
		/// </para>
		/// <para>It's equivalent to calling asyncAcknowledgeCumulative(MessageId) and waiting for the callback to be triggered.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageId">
		///            The {@code MessageId} to be cumulatively acknowledged </param>
		/// <exception cref="PulsarClientException.AlreadyClosedException">
		///             if the consumer was already closed </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void acknowledgeCumulative(MessageId messageId) throws PulsarClientException;
		void AcknowledgeCumulative(MessageId MessageId);

		/// <summary>
		/// Asynchronously acknowledge the consumption of a single message.
		/// </summary>
		/// <param name="message">
		///            The {@code Message} to be acknowledged </param>
		/// <returns> a future that can be used to track the completion of the operation </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.ValueTask<Void> acknowledgeAsync(Message<?> message);
		ValueTask<Void> acknowledgeAsync<T1>(Message<T1> Message);

		/// <summary>
		/// Asynchronously acknowledge the consumption of a single message.
		/// </summary>
		/// <param name="messageId">
		///            The {@code MessageId} to be acknowledged </param>
		/// <returns> a future that can be used to track the completion of the operation </returns>
		ValueTask<Void> AcknowledgeAsync(MessageId MessageId);

		/// <summary>
		/// Asynchronously acknowledge the consumption of <seealso cref="Messages"/>.
		/// </summary>
		/// <param name="messages">
		///            The <seealso cref="Messages"/> to be acknowledged </param>
		/// <returns> a future that can be used to track the completion of the operation </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.ValueTask<Void> acknowledgeAsync(Messages<?> messages);
		ValueTask<Void> acknowledgeAsync<T1>(Messages<T1> Messages);

		/// <summary>
		/// Asynchronously Acknowledge the reception of all the messages in the stream up to (and including) the provided
		/// message.
		/// 
		/// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
		/// 
		/// </para>
		/// </summary>
		/// <param name="message">
		///            The {@code Message} to be cumulatively acknowledged </param>
		/// <returns> a future that can be used to track the completion of the operation </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.ValueTask<Void> acknowledgeCumulativeAsync(Message<?> message);
		ValueTask<Void> acknowledgeCumulativeAsync<T1>(Message<T1> Message);

		/// <summary>
		/// Asynchronously Acknowledge the reception of all the messages in the stream up to (and including) the provided
		/// message.
		/// 
		/// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageId">
		///            The {@code MessageId} to be cumulatively acknowledged </param>
		/// <returns> a future that can be used to track the completion of the operation </returns>
		ValueTask<Void> AcknowledgeCumulativeAsync(MessageId MessageId);

		/// <summary>
		/// Get statistics for the consumer.
		/// <ul>
		/// <li>numMsgsReceived : Number of messages received in the current interval
		/// <li>numBytesReceived : Number of bytes received in the current interval
		/// <li>numReceiveFailed : Number of messages failed to receive in the current interval
		/// <li>numAcksSent : Number of acks sent in the current interval
		/// <li>numAcksFailed : Number of acks failed to send in the current interval
		/// <li>totalMsgsReceived : Total number of messages received
		/// <li>totalBytesReceived : Total number of bytes received
		/// <li>totalReceiveFailed : Total number of messages failed to receive
		/// <li>totalAcksSent : Total number of acks sent
		/// <li>totalAcksFailed : Total number of acks failed to sent
		/// </ul>
		/// </summary>
		/// <returns> statistic for the consumer </returns>
		ConsumerStats Stats {get;}

		/// <summary>
		/// Close the consumer and stop the broker to push more messages.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override void close() throws PulsarClientException;
		void Close();

		/// <summary>
		/// Asynchronously close the consumer and stop the broker to push more messages.
		/// </summary>
		/// <returns> a future that can be used to track the completion of the operation </returns>
		ValueTask<Void> CloseAsync();

		/// <summary>
		/// Return true if the topic was terminated and this consumer has already consumed all the messages in the topic.
		/// 
		/// <para>Please note that this does not simply mean that the consumer is caught up with the last message published by
		/// producers, rather the topic needs to be explicitly "terminated".
		/// </para>
		/// </summary>
		bool HasReachedEndOfTopic();

		/// <summary>
		/// Redelivers all the unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
		/// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
		/// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
		/// breaks, the messages are redelivered after reconnect.
		/// </summary>
		void RedeliverUnacknowledgedMessages();

		/// <summary>
		/// Reset the subscription associated with this consumer to a specific message id.
		/// 
		/// <para>The message id can either be a specific message or represent the first or last messages in the topic.
		/// <ul>
		/// <li><code>MessageId.earliest</code> : Reset the subscription on the earliest message available in the topic
		/// <li><code>MessageId.latest</code> : Reset the subscription on the latest message in the topic
		/// </ul>
		/// 
		/// </para>
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageId">
		///            the message id where to reposition the subscription </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void seek(MessageId messageId) throws PulsarClientException;
		void Seek(MessageId MessageId);

		/// <summary>
		/// Reset the subscription associated with this consumer to a specific message publish time.
		/// </summary>
		/// <param name="timestamp">
		///            the message publish time where to reposition the subscription </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void seek(long timestamp) throws PulsarClientException;
		void Seek(long Timestamp);

		/// <summary>
		/// Reset the subscription associated with this consumer to a specific message id.
		/// 
		/// <para>The message id can either be a specific message or represent the first or last messages in the topic.
		/// <ul>
		/// <li><code>MessageId.earliest</code> : Reset the subscription on the earliest message available in the topic
		/// <li><code>MessageId.latest</code> : Reset the subscription on the latest message in the topic
		/// </ul>
		/// 
		/// </para>
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageId">
		///            the message id where to reposition the subscription </param>
		/// <returns> a future to track the completion of the seek operation </returns>
		ValueTask<Void> SeekAsync(MessageId MessageId);

		/// <summary>
		/// Reset the subscription associated with this consumer to a specific message publish time.
		/// </summary>
		/// <param name="timestamp">
		///            the message publish time where to reposition the subscription </param>
		/// <returns> a future to track the completion of the seek operation </returns>
		ValueTask<Void> SeekAsync(long Timestamp);

		/// <summary>
		/// Get the last message id available available for consume.
		/// </summary>
		/// <returns> the last message id. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: MessageId getLastMessageId() throws PulsarClientException;
		MessageId LastMessageId {get;}

		/// <summary>
		/// Get the last message id available available for consume.
		/// </summary>
		/// <returns> a future that can be used to track the completion of the operation. </returns>
		ValueTask<MessageId> LastMessageIdAsync {get;}

		/// <returns> Whether the consumer is connected to the broker </returns>
		bool Connected {get;}

		/// <summary>
		/// Get the name of consumer. </summary>
		/// <returns> consumer name. </returns>
		string ConsumerName {get;}

		/// <summary>
		/// Stop requesting new messages from the broker until <seealso cref="resume()"/> is called. Note that this might cause
		/// <seealso cref="receive()"/> to block until <seealso cref="resume()"/> is called and new messages are pushed by the broker.
		/// </summary>
		void Pause();

		/// <summary>
		/// Resume requesting messages from the broker.
		/// </summary>
		void Resume();
	}

}