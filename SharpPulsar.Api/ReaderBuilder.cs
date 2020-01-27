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
namespace SharpPulsar.Api
{

	/// <summary>
	/// <seealso cref="ReaderBuilder"/> is used to configure and create instances of <seealso cref="Reader"/>.
	/// </summary>
	/// <seealso cref= IPulsarClient#newReader()
	/// 
	/// @since 2.0.0 </seealso>
	public interface ReaderBuilder<T> : ICloneable
	{

		/// <summary>
		/// Finalize the creation of the <seealso cref="Reader"/> instance.
		/// 
		/// <para>This method will block until the reader is created successfully or an exception is thrown.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the reader instance </returns>
		/// <exception cref="PulsarClientException">
		///             if the reader creation fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Reader<T> create() throws PulsarClientException;
		IReader<T> Create();

		/// <summary>
		/// Finalize the creation of the <seealso cref="Reader"/> instance in asynchronous mode.
		/// 
		/// <para>This method will return a <seealso cref="ValueTask"/> that can be used to access the instance when it's ready.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the reader instance </returns>
		/// <exception cref="PulsarClientException">
		///             if the reader creation fails </exception>
		ValueTask<IReader<T>> CreateAsync();

		/// <summary>
		/// Load the configuration from provided <tt>config</tt> map.
		/// 
		/// <para>Example:
		/// 
		/// <pre>{@code
		/// Map<String, Object> config = new HashMap<>();
		/// config.put("topicName", "test-topic");
		/// config.put("receiverQueueSize", 2000);
		/// 
		/// ReaderBuilder<byte[]> builder = ...;
		/// builder = builder.loadConf(config);
		/// 
		/// Reader<byte[]> reader = builder.create();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="config">
		///            configuration to load </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> LoadConf(IDictionary<string, object> Config);

		/// <summary>
		/// Create a copy of the current <seealso cref="ReaderBuilder"/>.
		/// 
		/// <para>Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
		/// example:
		/// 
		/// <pre>{@code
		/// ReaderBuilder<String> builder = client.newReader(Schema.STRING)
		///             .readerName("my-reader")
		///             .receiverQueueSize(10);
		/// 
		/// Reader<String> reader1 = builder.clone().topic("topic-1").create();
		/// Reader<String> reader2 = builder.clone().topic("topic-2").create();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a clone of the reader builder instance </returns>
		ReaderBuilder<T> Clone();

		/// <summary>
		/// Specify the topic this reader will read from.
		/// 
		/// <para>This argument is required when constructing the reader.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topicName">
		///            the name of the topic </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> Topic(string TopicName);

		/// <summary>
		/// The initial reader positioning is done by specifying a message id. The options are:
		/// <ul>
		/// <li><seealso cref="IMessageId.earliest"/>: Start reading from the earliest message available in the topic</li>
		/// <li><seealso cref="IMessageId.latest"/>: Start reading from end of the topic. The first message read will be the one
		/// published <b>*after*</b> the creation of the builder</li>
		/// <li><seealso cref="IMessageId"/>: Position the reader on a particular message. The first message read will be the one
		/// immediately <b>*after*</b> the specified message</li>
		/// </ul>
		/// 
		/// <para>If the first message <b>*after*</b> the specified message is not the desired behaviour, use
		/// <seealso cref="ReaderBuilder.startMessageIdInclusive()"/>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="startMessageId"> the message id where the reader will be initially positioned on </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> StartMessageId(IMessageId StartMessageId);

		/// <summary>
		/// The initial reader positioning can be set at specific timestamp by providing total rollback duration. so, broker
		/// can find a latest message that was published before given duration. <br/>
		/// eg: rollbackDuration in minute = 5 suggests broker to find message which was published 5 mins back and set the
		/// inital position on that messageId.
		/// </summary>
		/// <param name="rollbackDuration">
		///            duration which position should be rolled back.
		/// @return </param>
		ReaderBuilder<T> StartMessageFromRollbackDuration(long RollbackDuration, TimeUnit Timeunit);

		/// <summary>
		/// Set the reader to include the given position of <seealso cref="ReaderBuilder.startMessageId(IMessageId)"/>
		/// 
		/// <para>This configuration option also applies for any cursor reset operation like <seealso cref="IReader.seek(IMessageId)"/>.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> StartMessageIdInclusive();

		/// <summary>
		/// Sets a <seealso cref="ReaderListener"/> for the reader.
		/// 
		/// <para>When a <seealso cref="ReaderListener"/> is set, application will receive messages through it. Calls to
		/// <seealso cref="IReader.readNext()"/> will not be allowed.
		/// 
		/// </para>
		/// </summary>
		/// <param name="readerListener">
		///            the listener object </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> ReaderListener(ReaderListener<T> ReaderListener);

		/// <summary>
		/// Sets a <seealso cref="CryptoKeyReader"/> to decrypt the message payloads.
		/// </summary>
		/// <param name="cryptoKeyReader">
		///            CryptoKeyReader object </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> CryptoKeyReader(CryptoKeyReader CryptoKeyReader);

		/// <summary>
		/// Sets the <seealso cref="ConsumerCryptoFailureAction"/> to specify.
		/// </summary>
		/// <param name="action">
		///            The action to take when the decoding fails </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> CryptoFailureAction(ConsumerCryptoFailureAction Action);

		/// <summary>
		/// Sets the size of the consumer receive queue.
		/// 
		/// <para>The consumer receive queue controls how many messages can be accumulated by the <seealso cref="Consumer"/> before the
		/// application calls <seealso cref="IConsumer.receive()"/>. Using a higher value could potentially increase the consumer
		/// throughput at the expense of bigger memory utilization.
		/// 
		/// </para>
		/// <para>Default value is {@code 1000} messages and should be good for most use cases.
		/// 
		/// </para>
		/// </summary>
		/// <param name="receiverQueueSize">
		///            the new receiver queue size value </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> ReceiverQueueSize(int ReceiverQueueSize);

		/// <summary>
		/// Specify a reader name.
		/// 
		/// <para>The reader name is purely informational and can used to track a particular reader in the reported stats.
		/// By default a randomly generated name is used.
		/// 
		/// </para>
		/// </summary>
		/// <param name="readerName">
		///            the name to use for the reader </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> ReaderName(string ReaderName);

		/// <summary>
		/// Set the subscription role prefix. The default prefix is "reader".
		/// </summary>
		/// <param name="subscriptionRolePrefix"> </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> SubscriptionRolePrefix(string SubscriptionRolePrefix);

		/// <summary>
		/// If enabled, the reader will read messages from the compacted topic rather than reading the full message backlog
		/// of the topic. This means that, if the topic has been compacted, the reader will only see the latest value for
		/// each key in the topic, up until the point in the topic message backlog that has been compacted. Beyond that
		/// point, the messages will be sent as normal.
		/// 
		/// <para>readCompacted can only be enabled when reading from a persistent topic. Attempting to enable it
		/// on non-persistent topics will lead to the reader create call throwing a <seealso cref="PulsarClientException"/>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="readCompacted">
		///            whether to read from the compacted topic </param>
		/// <returns> the reader builder instance </returns>
		ReaderBuilder<T> ReadCompacted(bool ReadCompacted);
	}

}