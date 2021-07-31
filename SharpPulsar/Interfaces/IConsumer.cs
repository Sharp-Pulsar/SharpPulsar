using SharpPulsar.Stats.Consumer.Api;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Exceptions;

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
    /// An interface that abstracts behavior of Pulsar's consumer.
    /// 
    /// <para>All the operations on the consumer instance are thread safe.
    /// </para>
    /// </summary>
    public interface IConsumer<T>
    {

        /// <summary>
        /// Get a topic for the consumer.
        /// </summary>
        /// <returns> topic for the consumer </returns>
        string Topic { get; }

        /// <summary>
        /// Get a topic for the consumer.
        /// </summary>
        /// <returns> topic for the consumer </returns>
        ValueTask<string> TopicAsync();

        /// <summary>
        /// Get a subscription for the consumer.
        /// </summary>
        /// <returns> subscription for the consumer </returns>
        string Subscription { get; }

        /// <summary>
        /// Get a subscription for the consumer.
        /// </summary>
        /// <returns> subscription for the consumer </returns>
        ValueTask<string> SubscriptionAsync();

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
        void Unsubscribe();

        /// <summary>
        /// Receive a single message.
        /// 
        /// <para>Retrieves a message, waiting up to the specified wait time if necessary.
        /// 
        /// </para>
        /// </summary>
        /// <param name="timeout">0 or less means immediate rather than infinite </param>
        /// <returns> the received <seealso cref="IMessage<T>"/> or null if no message available before timeout </returns>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// <exception cref="PulsarClientException.InvalidConfigurationException">if a message listener was defined in the configuration </exception>
        ///             
        IMessage<T> Receive();

        /// <summary>
        /// Receive a single message.
        /// 
        /// <para>Retrieves a message, waiting up to the specified wait time if necessary.
        /// 
        /// </para>
        /// </summary>
        /// <param name="timeout">0 or less means immediate rather than infinite </param>
        /// <returns> the received <seealso cref="IMessage<T>"/> or null if no message available before timeout </returns>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// <exception cref="PulsarClientException.InvalidConfigurationException">if a message listener was defined in the configuration </exception>
        ///          
        ValueTask<IMessage<T>> ReceiveAsync();

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
        /// 
        IMessages<T> BatchReceive();

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
        /// 
        ValueTask<IMessages<T>> BatchReceiveAsync();


        /// <summary>
        /// Acknowledge the consumption of a single message.
        /// </summary>
        /// <param name="message">The {@code Message} to be acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException"> if the consumer was already closed </exception>
        void Acknowledge(IMessage<T> message);

        /// <summary>
        /// Acknowledge the consumption of a single message.
        /// </summary>
        /// <param name="message">The {@code Message} to be acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ValueTask AcknowledgeAsync(IMessage<T> message);

        /// <summary>
        /// Acknowledge the consumption of a single message, identified by its <seealso cref="IMessageId"/>.
        /// </summary>
        /// <param name="messageId">The <seealso cref="IMessageId"/> to be acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///             
        void Acknowledge(IMessageId messageId);

        /// <summary>
        /// Acknowledge the consumption of a single message, identified by its <seealso cref="IMessageId"/>.
        /// </summary>
        /// <param name="messageId">The <seealso cref="IMessageId"/> to be acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// 
        ValueTask AcknowledgeAsync(IMessageId messageId);

        /// <summary>
        /// Acknowledge the consumption of <seealso cref="IMessages"/>.
        /// </summary>
        /// <param name="messages"> messages </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///              
        void Acknowledge(IMessages<T> messages);

        /// <summary>
        /// Acknowledge the consumption of <seealso cref="IMessages"/>.
        /// </summary>
        /// <param name="messages"> messages </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///    
        ValueTask AcknowledgeAsync(IMessages<T> messages);

        /// <summary>
        /// Acknowledge the consumption of a list of message. </summary>
        /// <param name="messageIdList"> </param>
        /// <exception cref="PulsarClientException"> </exception>
        /// 
        void Acknowledge(IList<IMessageId> messageIdList);

        /// <summary>
        /// Acknowledge the consumption of a list of message. </summary>
        /// <param name="messageIdList"> </param>
        /// <exception cref="PulsarClientException"> </exception>
        /// 
        ValueTask AcknowledgeAsync(IList<IMessageId> messageIdList);

        /// <summary>
        /// Acknowledge the failure to process a single message.
        /// 
        /// <para>When a message is "negatively acked" it will be marked for redelivery after
        /// some fixed delay. The delay is configurable when constructing the consumer
        /// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
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
        /// The {@code Message} to be acknowledged </param>
        ///            
        void NegativeAcknowledge(IMessage<T> message);

        /// <summary>
        /// Acknowledge the failure to process a single message.
        /// 
        /// <para>When a message is "negatively acked" it will be marked for redelivery after
        /// some fixed delay. The delay is configurable when constructing the consumer
        /// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
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
        /// The {@code Message} to be acknowledged </param>
        /// 
        ValueTask NegativeAcknowledgeAsync(IMessage<T> message);


        /// <summary>
        /// Acknowledge the failure to process <seealso cref="Messages"/>.
        /// 
        /// <para>When messages is "negatively acked" it will be marked for redelivery after
        /// some fixed delay. The delay is configurable when constructing the consumer
        /// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
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
        /// The {@code Message} to be acknowledged </param>
        ///            
        void NegativeAcknowledge(IMessages<T> messages);


        /// <summary>
        /// Acknowledge the failure to process <seealso cref="Messages"/>.
        /// 
        /// <para>When messages is "negatively acked" it will be marked for redelivery after
        /// some fixed delay. The delay is configurable when constructing the consumer
        /// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
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
        /// The {@code Message} to be acknowledged </param>
        ///
        ValueTask NegativeAcknowledgeAsync(IMessages<T> messages);

        /// <summary>
		/// Acknowledge the failure to process a single message.
		/// 
		/// <para>When a message is "negatively acked" it will be marked for redelivery after
		/// some fixed delay. The delay is configurable when constructing the consumer
		/// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
		/// 
		/// </para>
		/// <para>This call is not blocking.
		/// 
		/// </para>
		/// <para>This variation allows to pass a <seealso cref="IMessageId"/> rather than a <seealso cref="IMessage"/>
		/// object, in order to avoid keeping the payload in memory for extended amount
		/// of time
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref=NegativeAcknowledge(IMessage)
		/// </seealso>
		/// <param name="messageId">
		///            The {@code MessageId} to be acknowledged </param>
		void NegativeAcknowledge(IMessageId messageId);
        /// <summary>
		/// Acknowledge the failure to process a single message.
		/// 
		/// <para>When a message is "negatively acked" it will be marked for redelivery after
		/// some fixed delay. The delay is configurable when constructing the consumer
		/// with <seealso cref="ConsumerBuilder.negativeAckRedeliveryDelay(long)"/>.
		/// 
		/// </para>
		/// <para>This call is not blocking.
		/// 
		/// </para>
		/// <para>This variation allows to pass a <seealso cref="IMessageId"/> rather than a <seealso cref="IMessage"/>
		/// object, in order to avoid keeping the payload in memory for extended amount
		/// of time
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref=NegativeAcknowledge(IMessage)
		/// </seealso>
		/// <param name="messageId">
		///            The {@code MessageId} to be acknowledged </param>
		ValueTask NegativeAcknowledgeAsync(IMessageId messageId);

        /// <summary>
        /// reconsumeLater the consumption of <seealso cref="Messages"/>.
        /// 
        /// <para>When a message is "reconsumeLater" it will be marked for redelivery after
        /// some custom delay.
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
        ///          consumer.reconsumeLater(msg, 1000 , TimeUnit.MILLISECONDS);
        ///     }
        /// }
        /// </code></pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="message">the {@code Message} to be reconsumeLater </param>
        /// <param name="delayTime">the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///              

        void ReconsumeLater(IMessage<T> message, TimeSpan delayTimeInMs);

        /// <summary>
        /// reconsumeLater the consumption of <seealso cref="Messages"/>.
        /// 
        /// <para>When a message is "reconsumeLater" it will be marked for redelivery after
        /// some custom delay.
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
        ///          consumer.reconsumeLater(msg, 1000 , TimeUnit.MILLISECONDS);
        ///     }
        /// }
        /// </code></pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="message">the {@code Message} to be reconsumeLater </param>
        /// <param name="delayTime">the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///  
        ValueTask ReconsumeLaterAsync(IMessage<T> message, TimeSpan delayTimeInMs);

        /// <summary>
        /// reconsumeLater the consumption of <seealso cref="Messages"/>.
        /// </summary>
        /// <param name="messages">the {@code messages} to be reconsumeLater </param>
        /// <param name="delayTime">the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///              
        void ReconsumeLater(IMessages<T> messages, TimeSpan delayTime);

        /// <summary>
        /// reconsumeLater the consumption of <seealso cref="Messages"/>.
        /// </summary>
        /// <param name="messages">the {@code messages} to be reconsumeLater </param>
        /// <param name="delayTime">the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///    
        ValueTask ReconsumeLaterAsync(IMessages<T> messages, TimeSpan delayTime);

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
        /// <param name="message">The {@code Message} to be cumulatively acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///             
        void AcknowledgeCumulative(IMessage<T> message);

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
        /// <param name="message">The {@code Message} to be cumulatively acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///  
        ValueTask AcknowledgeCumulativeAsync(IMessage<T> message);

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
        /// <param name="messageId">The {@code MessageId} to be cumulatively acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        ///             
        void AcknowledgeCumulative(IMessageId messageId);


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
        /// <param name="messageId">The {@code MessageId} to be cumulatively acknowledged </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// 
        ValueTask AcknowledgeCumulativeAsync(IMessageId messageId);

        /// <summary>
        /// Acknowledge the reception of all the messages in the stream up to (and including) the provided message with this
        /// transaction, it will store in transaction pending ack.
        /// 
        /// <para>After the transaction commit, the end of previous transaction acked message until this transaction
        /// acked message will actually ack.
        /// 
        /// </para>
        /// <para>After the transaction abort, the end of previous transaction acked message until this transaction
        /// acked message will be redelivered to this consumer.
        /// 
        /// </para>
        /// <para>Cumulative acknowledge with transaction only support cumulative ack and now have not support individual and
        /// cumulative ack sharing.
        /// 
        /// </para>
        /// <para>If cumulative ack with a transaction success, we can cumulative ack messageId with the same transaction
        /// more than previous messageId.
        /// 
        /// </para>
        /// <para>It will not be allowed to cumulative ack with a transaction different from the previous one when the previous
        /// transaction haven't commit or abort.
        /// 
        /// </para>
        /// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
        /// 
        /// </para>
        /// </summary>
        /// <param name="messageId">The {@code MessageId} to be cumulatively acknowledged </param>
        /// <param name="txn"> <seealso cref="ITransaction"/> the transaction to cumulative ack </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// <exception cref="PulsarClientException.TransactionConflictException">if the ack with messageId is less than the messageId in pending ack state or ack with transaction is 
        /// different from the transaction in pending ack. </exception>
        /// <exception cref="PulsarClientException.NotAllowedException">broker don't support transaction </exception>
        /// 
        /// @since 2.7.0 </returns>
        void AcknowledgeCumulative(IMessageId messageId, User.Transaction txn);

        /// <summary>
        /// Acknowledge the reception of all the messages in the stream up to (and including) the provided message with this
        /// transaction, it will store in transaction pending ack.
        /// 
        /// <para>After the transaction commit, the end of previous transaction acked message until this transaction
        /// acked message will actually ack.
        /// 
        /// </para>
        /// <para>After the transaction abort, the end of previous transaction acked message until this transaction
        /// acked message will be redelivered to this consumer.
        /// 
        /// </para>
        /// <para>Cumulative acknowledge with transaction only support cumulative ack and now have not support individual and
        /// cumulative ack sharing.
        /// 
        /// </para>
        /// <para>If cumulative ack with a transaction success, we can cumulative ack messageId with the same transaction
        /// more than previous messageId.
        /// 
        /// </para>
        /// <para>It will not be allowed to cumulative ack with a transaction different from the previous one when the previous
        /// transaction haven't commit or abort.
        /// 
        /// </para>
        /// <para>Cumulative acknowledge cannot be used when the consumer type is set to ConsumerShared.
        /// 
        /// </para>
        /// </summary>
        /// <param name="messageId">The {@code MessageId} to be cumulatively acknowledged </param>
        /// <param name="txn"> <seealso cref="ITransaction"/> the transaction to cumulative ack </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">if the consumer was already closed </exception>
        /// <exception cref="PulsarClientException.TransactionConflictException">if the ack with messageId is less than the messageId in pending ack state or ack with transaction is 
        /// different from the transaction in pending ack. </exception>
        /// <exception cref="PulsarClientException.NotAllowedException">broker don't support transaction </exception>
        /// 
        /// @since 2.7.0 </returns>
        ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, User.Transaction txn);

        /// <summary>
        /// reconsumeLater the reception of all the messages in the stream up to (and including) the provided message.
        /// </summary>
        /// <param name="message">
        ///            The {@code message} to be cumulatively reconsumeLater </param>
        /// <param name="delayTime">
        ///            the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException"> if the consumer was already closed </exception>
        ///             
        void ReconsumeLaterCumulative(IMessage<T> message, TimeSpan delayTimeInMs);

        /// <summary>
        /// reconsumeLater the reception of all the messages in the stream up to (and including) the provided message.
        /// </summary>
        /// <param name="message">
        ///            The {@code message} to be cumulatively reconsumeLater </param>
        /// <param name="delayTime">
        ///            the amount of delay before the message will be delivered </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException"> if the consumer was already closed </exception>
        /// 
        ValueTask ReconsumeLaterCumulativeAsync(IMessage<T> message, TimeSpan delayTimeInMs);

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
        IConsumerStats Stats { get; }

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
        /// <returns> statistic for the consumer </returns>s
        ValueTask<IConsumerStats> StatsAsync();

        /// <summary>
        /// Close the consumer and stop the broker to push more messages.
        /// </summary>
        /// 
        void Close();
        /// <summary>
        /// Close the consumer and stop the broker to push more messages.
        /// </summary>
        /// 
        ValueTask CloseAsync();

        /// <summary>
        /// Return true if the topic was terminated and this consumer has already consumed all the messages in the topic.
        /// 
        /// <para>Please note that this does not simply mean that the consumer is caught up with the last message published by
        /// producers, rather the topic needs to be explicitly "terminated".
        /// </para>
        /// </summary>
        bool HasReachedEndOfTopic();

        /// <summary>
        /// Return true if the topic was terminated and this consumer has already consumed all the messages in the topic.
        /// 
        /// <para>Please note that this does not simply mean that the consumer is caught up with the last message published by
        /// producers, rather the topic needs to be explicitly "terminated".
        /// </para>
        /// </summary>
        ValueTask<bool> HasReachedEndOfTopicAsync();

        /// <summary>
        /// Redelivers all the unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
        /// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
        /// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
        /// breaks, the messages are redelivered after reconnect.
        /// </summary>
        void RedeliverUnacknowledgedMessages();
        /// <summary>
        /// Redelivers all the unacknowledged messages. In Failover mode, the request is ignored if the consumer is not
        /// active for the given topic. In Shared mode, the consumers messages to be redelivered are distributed across all
        /// the connected consumers. This is a non blocking call and doesn't throw an exception. In case the connection
        /// breaks, the messages are redelivered after reconnect.
        /// </summary>
        ValueTask RedeliverUnacknowledgedMessagesAsync();

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
        /// <param name="messageId">the message id where to reposition the subscription </param>
        ///            
        void Seek(IMessageId messageId);

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
        /// <param name="messageId">the message id where to reposition the subscription </param>
        /// 
        ValueTask SeekAsync(IMessageId messageId);

        /// <summary>
        /// Reset the subscription associated with this consumer to a specific message publish time.
        /// </summary>
        /// <param name="timestamp">the message publish time where to reposition the subscription </param>
        ///            
        void Seek(long timestamp);

        /// <summary>
        /// Reset the subscription associated with this consumer to a specific message publish time.
        /// </summary>
        /// <param name="timestamp">the message publish time where to reposition the subscription </param>
        ///    
        ValueTask SeekAsync(long timestamp);



        /// <summary>
        /// Reset the subscription associated with this consumer to a specific message ID or message publish time.
        /// <para>
        /// The Function input is topic+partition. It returns only timestamp or MessageId.
        /// </para>
        /// <para>
        /// The return value is the seek position/timestamp of the current partition.
        /// Exception is thrown if other object types are returned.
        /// </para>
        /// <para>
        /// If returns null, the current partition will not do any processing.
        /// Exception in a partition may affect other partitions.
        /// </para>
        /// </summary>
        /// <param name="function"> </param>
        /// <exception cref="PulsarClientException"> </exception>
        void Seek(Func<string, object> function);

        /// <summary>
        /// Reset the subscription associated with this consumer to a specific message ID
        /// or message publish time asynchronously.
        /// <para>
        /// The Function input is topic+partition. It returns only timestamp or MessageId.
        /// </para>
        /// <para>
        /// The return value is the seek position/timestamp of the current partition.
        /// Exception is thrown if other object types are returned.
        /// </para>
        /// <para>
        /// If returns null, the current partition will not do any processing.
        /// Exception in a partition may affect other partitions.
        /// </para>
        /// </summary>
        /// <param name="function">
        /// @return </param>
        ValueTask SeekAsync(Func<string, object> function);


        /// <summary>
        /// Get the last message id available available for consume.
        /// </summary>
        /// <returns> the last message id. </returns>
        /// 
        IMessageId LastMessageId { get; }

        /// <summary>
        /// Get the last message id available available for consume.
        /// </summary>
        /// <returns> the last message id. </returns>
        /// 
        ValueTask<IMessageId> LastMessageIdAsync();

        /// <returns> Whether the consumer is connected to the broker </returns>
        bool Connected { get; }


        /// <returns> Whether the consumer is connected to the broker </returns>
        ValueTask<bool> ConnectedAsync();

        /// <summary>
        /// Get the name of consumer. </summary>
        /// <returns> consumer name. </returns>
        string ConsumerName { get; }

        /// <summary>
        /// Get the name of consumer. </summary>
        /// <returns> consumer name. </returns>
        ValueTask<string> ConsumerNameAsync();

        /// <summary>
        /// Stop requesting new messages from the broker until <seealso cref="resume()"/> is called. Note that this might cause
        /// <seealso cref="receive()"/> to block until <seealso cref="resume()"/> is called and new messages are pushed by the broker.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume requesting messages from the broker.
        /// </summary>
        void Resume();

        /// <returns> The last disconnected timestamp of the consumer </returns>
        long LastDisconnectedTimestamp { get; }

        /// <returns> The last disconnected timestamp of the consumer </returns>
        ValueTask<long> LastDisconnectedTimestampAsync();
    }

}