using SharpPulsar.API.Transaction;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.API
{
    /// <summary>
    /// The interface to acknowledge one or more messages individually or cumulatively.
    /// <para>
    /// It contains two methods of various overloads:
    /// - `acknowledge`: acknowledge individually
    /// - `acknowledgeCumulative`: acknowledge cumulatively
    /// Each of them has an associated asynchronous API that has the "Async" suffix in the name.
    /// </para>
    /// <para>
    /// The 1st method parameter is
    /// - <seealso cref="MessageId"/> or <seealso cref="Message"/> when acknowledging a single message
    /// - <seealso cref="System.Collections.Generic.IList<MessageId>"/> or <seealso cref="Messages"/> when acknowledging multiple messages
    /// </para>
    /// <para>
    /// The 2nd method parameter is optional. Specify a non-null <seealso cref="Transaction"/> instance for transaction usages:
    /// - After the transaction is committed, the message will be actually acknowledged (individually or cumulatively).
    /// - After the transaction is aborted, the message will be redelivered.
    /// </para>
    /// </summary>
    /// <seealso cref="Transaction.commit()"/>
    /// <seealso cref="Transaction.abort()"/>
    public interface IMessageAcknowledger
    {

        /// <summary>
        /// Acknowledge the consumption of a single message.
        /// </summary>
        /// <param name="messageId"> <seealso cref="MessageId"/> to be individual acknowledged
        /// </param>
        /// <exception cref="PulsarClientException.AlreadyClosedException">}
        ///             if the consumer was already closed </exception>
        /// <exception cref="PulsarClientException.NotAllowedException">
        ///             if `messageId` is not a <seealso cref="TopicMessageId"/> when multiple topics are subscribed </exception>
        void Acknowledge(IMessageId messageId);
        void Acknowledge<T1>(IMessage<T1> message)
        {
            Acknowledge(message.MessageId);
        }

        /// <summary>
        /// Acknowledge the consumption of a list of message. </summary>
        /// <param name="messageIdList"> the list of message IDs. </param>
        /// <exception cref="PulsarClientException.NotAllowedException">
        ///     if any message id in the list is not a <seealso cref="TopicMessageId"/> when multiple topics are subscribed </exception>
        ///     
        void Acknowledge(IList<IMessageId> messageIdList);
        void Acknowledge<T1>(IMessages<T1> messages)
        {
            //JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            //ORIGINAL LINE: for (Message<?> message : messages)
            foreach (IMessage<object> message in messages)
            {
                Acknowledge(message.MessageId);
            }
        }

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
        /// <exception cref="PulsarClientException.NotAllowedException">
        ///             if `messageId` is not a <seealso cref="TopicMessageId"/> when multiple topics are subscribed </exception>
        void AcknowledgeCumulative(IMessageId messageId);

        void AcknowledgeCumulative<T1>(IMessage<T1> message)
        {
            AcknowledgeCumulative(message.MessageId);
        }

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(MessageId)"/> with transaction support.
        /// </summary>
        ValueTask AcknowledgeAsync(IMessageId messageId, ITransaction txn);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(MessageId)"/>.
        /// </summary>
        ValueTask AcknowledgeAsync(IMessageId messageId)
        {
            return AcknowledgeAsync(messageId, null);
        }

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(List)"/> with transaction support.
        /// </summary>
        ValueTask AcknowledgeAsync(IList<IMessageId> messageIdList, ITransaction txn);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(List)"/>.
        /// </summary>
        ValueTask AcknowledgeAsync(IList<IMessageId> messageIdList);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(Message)"/>.
        /// </summary>
        ValueTask AcknowledgeAsync<T1>(IMessage<T1> message);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(Messages)"/>.
        /// </summary>
        ValueTask AcknowledgeAsync<T1>(IMessages<T1> messages);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledge(Messages)"/> with transaction support.
        /// </summary>
        ValueTask AcknowledgeAsync<T1>(IMessages<T1> messages, ITransaction txn);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledgeCumulative(MessageId)"/> with transaction support.
        /// 
        /// @apiNote It's not allowed to cumulative ack with a transaction different from the previous one when the previous
        /// transaction is not committed or aborted.
        /// @apiNote It cannot be used for <seealso cref="SubscriptionType.Shared"/> subscription.
        /// </summary>
        /// <param name="messageId">
        ///            The {@code MessageId} to be cumulatively acknowledged </param>
        /// <param name="txn"> <seealso cref="ITransaction"/> the transaction to cumulative ack </param>
        ValueTask AcknowledgeCumulativeAsync(IMessageId messageId, ITransaction txn);

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledgeCumulative(Message)"/>.
        /// </summary>
        ValueTask AcknowledgeCumulativeAsync<T1>(IMessage<T1> message)
        {
            return AcknowledgeCumulativeAsync(message.MessageId);
        }

        /// <summary>
        /// The asynchronous version of <seealso cref="acknowledgeCumulative(MessageId)"/>.
        /// </summary>
        ValueTask AcknowledgeCumulativeAsync(IMessageId messageId)
        {
            return AcknowledgeCumulativeAsync(messageId, null);
        }
    }
}
