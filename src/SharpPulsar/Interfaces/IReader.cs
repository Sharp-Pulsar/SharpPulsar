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
namespace SharpPulsar.Interfaces
{

    /// <summary>
    /// A Reader can be used to scan through all the messages currently available in a topic.
    /// </summary>
    public interface IReader<T>
	{

		/// <returns> the topic from which this reader is reading from </returns>
		string Topic {get;}
		ValueTask<string> TopicAsync();

		/// <summary>
		/// Read the next message in the topic.
		/// 
		/// <para>This method will block until a message is available.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the next message </returns>
		/// <exception cref="PulsarClientException"> </exception>
		IMessage<T> ReadNext();
		ValueTask<IMessage<T>> ReadNextAsync();

		/// <summary>
		/// Read the next message in the topic waiting for a maximum time.
		/// 
		/// <para>Returns null if no message is received before the timeout.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the next message(Could be null if none received in time) </returns>
		/// <exception cref="PulsarClientException"> </exception>
		IMessage<T> ReadNext(TimeSpan timeSpan);
		ValueTask<IMessage<T>> ReadNextAsync(TimeSpan timeSpan);

		
		bool HasReachedEndOfTopic();
		ValueTask<bool> HasReachedEndOfTopicAsync();

		/// <summary>
		/// Check if there is any message available to read from the current position.
		/// 
		/// <para>This check can be used by an application to scan through a topic and stop
		/// when the reader reaches the current last published message. For example:
		/// 
		/// <pre>{@code
		/// while (reader.hasMessageAvailable()) {
		///     Message<String> msg = reader.readNext();
		///     // Do something
		/// }
		/// 
		/// // Done reading
		/// }</pre>
		/// 
		/// </para>
		/// <para>Note that this call might be blocking (see <seealso cref="hasMessageAvailableAsync()"/> for async version) and
		/// that even if this call returns true, that will not guarantee that a subsequent call to <seealso cref="readNext()"/>
		/// will not block.
		/// 
		/// </para>
		/// </summary>
		/// <returns> true if the are messages available to be read, false otherwise </returns>
		/// <exception cref="PulsarClientException"> if there was any error in the operation </exception>
		bool HasMessageAvailable();
		ValueTask<bool> HasMessageAvailableAsync();


		/// <returns> Whether the reader is connected to the broker </returns>
		bool Connected {get;}
		ValueTask<bool> ConnectedAsync();

		/// <summary>
		/// Reset the subscription associated with this reader to a specific message id.
		/// 
		/// <para>The message id can either be a specific message or represent the first or last messages in the topic.
		/// <ul>
		/// <li><code>MessageId.earliest</code> : Reset the reader on the earliest message available in the topic
		/// <li><code>MessageId.latest</code> : Reset the reader on the latest message in the topic
		/// </ul>
		/// 
		/// </para>
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageId"> the message id where to reposition the reader </param>
		void Seek(IMessageId messageId);
		ValueTask SeekAsync(IMessageId messageId);

		/// <summary>
		/// Reset the subscription associated with this reader to a specific message publish time.
		/// 
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timestamp"> the message publish time where to reposition the reader </param>
		void Seek(long timestamp);
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

    }

}