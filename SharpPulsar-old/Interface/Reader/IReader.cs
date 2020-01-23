using BAMCIS.Util.Concurrent;
using SharpPulsar.Interface.Message;
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
namespace SharpPulsar.Interface.Reader
{

	/// <summary>
	/// A Reader can be used to scan through all the messages currently available in a topic.
	/// </summary>
	public interface IReader<T> : System.IDisposable
	{

		/// <returns> the topic from which this reader is reading from </returns>
		string Topic {get;}

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

		/// <summary>
		/// Read the next message in the topic waiting for a maximum time.
		/// 
		/// <para>Returns null if no message is received before the timeout.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the next message(Could be null if none received in time) </returns>
		/// <exception cref="PulsarClientException"> </exception>
		IMessage<T> ReadNext(int timeout, TimeUnit unit);

		/// <summary>
		/// Read asynchronously the next message in the topic.
		/// </summary>
		/// <returns> a future that will yield a message (when it's available) or <seealso cref="PulsarClientException"/> if the reader
		///         is already closed. </returns>
		ValueTask<IMessage<T>> ReadNextAsync();

		/// <summary>
		/// Asynchronously close the reader and stop the broker to push more messages.
		/// </summary>
		/// <returns> a future that can be used to track the completion of the operation </returns>
		ValueTask CloseAsync();

		/// <summary>
		/// Return true if the topic was terminated and this reader has reached the end of the topic.
		/// 
		/// <para>Note that this only applies to a "terminated" topic (where the topic is "sealed" and no
		/// more messages can be published) and not just that the reader is simply caught up with
		/// the publishers. Use <seealso cref="hasMessageAvailable()"/> to check for for that.
		/// </para>
		/// </summary>
		bool HasReachedEndOfTopic();

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

		/// <summary>
		/// Asynchronously check if there is any message available to read from the current position.
		/// 
		/// <para>This check can be used by an application to scan through a topic and stop when the reader reaches the current
		/// last published message.
		/// 
		/// </para>
		/// </summary>
		/// <returns> a future that will yield true if the are messages available to be read, false otherwise, or a
		///         <seealso cref="PulsarClientException"/> if there was any error in the operation </returns>
		ValueTask<bool> HasMessageAvailableAsync();

		/// <returns> Whether the reader is connected to the broker </returns>
		bool Connected {get;}

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

		/// <summary>
		/// Reset the subscription associated with this reader to a specific message publish time.
		/// 
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timestamp"> the message publish time where to reposition the reader </param>
		/// <exception cref="PulsarClientException">PulsarClientException</exception>
		void Seek(long timestamp);

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
		/// <param name="messageId"> the message id where to position the reader </param>
		/// <returns> a future to track the completion of the seek operation </returns>
		ValueTask SeekAsync(IMessageId messageId);

		/// <summary>
		/// Reset the subscription associated with this reader to a specific message publish time.
		/// 
		/// <para>Note: this operation can only be done on non-partitioned topics. For these, one can rather perform
		/// the seek() on the individual partitions.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timestamp">
		///            the message publish time where to position the reader </param>
		/// <returns> a future to track the completion of the seek operation </returns>
		ValueTask SeekAsync(long timestamp);
	}

}