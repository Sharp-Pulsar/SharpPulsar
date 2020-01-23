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
	using DefaultImplementation = Org.Apache.Pulsar.Client.@internal.DefaultImplementation;

	/// <summary>
	/// Class that provides a client interface to Pulsar.
	/// 
	/// <para>Client instances are thread-safe and can be reused for managing multiple <seealso cref="Producer"/>, <seealso cref="Consumer"/> and
	/// <seealso cref="Reader"/> instances.
	/// 
	/// </para>
	/// <para>Example of constructing a client:
	/// 
	/// <pre>{@code
	/// PulsarClient client = PulsarClient.builder()
	///                              .serviceUrl("pulsar://broker:6650")
	///                              .build();
	/// }</pre>
	/// </para>
	/// </summary>
	public interface PulsarClient : System.IDisposable
	{

		/// <summary>
		/// Get a new builder instance that can used to configure and build a <seealso cref="PulsarClient"/> instance.
		/// </summary>
		/// <returns> the <seealso cref="IClientBuilder"/>
		/// 
		/// @since 2.0.0 </returns>
		static IClientBuilder Builder()
		{
			return DefaultImplementation.newClientBuilder();
		}

		/// <summary>
		/// Create a producer builder that can be used to configure
		/// and construct a producer with default <seealso cref="Schema.BYTES"/>.
		/// 
		/// <para>Example:
		/// 
		/// <pre>{@code
		/// Producer<byte[]> producer = client.newProducer()
		///                  .topic("my-topic")
		///                  .create();
		/// producer.send("test".getBytes());
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a <seealso cref="ProducerBuilder"/> object to configure and construct the <seealso cref="Producer"/> instance
		/// 
		/// @since 2.0.0 </returns>
		ProducerBuilder<sbyte[]> NewProducer();

		/// <summary>
		/// Create a producer builder that can be used to configure
		/// and construct a producer with the specified schema.
		/// 
		/// <para>Example:
		/// 
		/// <pre>{@code
		/// Producer<String> producer = client.newProducer(Schema.STRING)
		///                  .topic("my-topic")
		///                  .create();
		/// producer.send("test");
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="schema">
		///          provide a way to convert between serialized data and domain objects
		/// </param>
		/// <returns> a <seealso cref="ProducerBuilder"/> object to configure and construct the <seealso cref="Producer"/> instance
		/// 
		/// @since 2.0.0 </returns>
		ProducerBuilder<T> newProducer<T>(Schema<T> Schema);

		/// <summary>
		/// Create a consumer builder with no schema (<seealso cref="Schema.BYTES"/>) for subscribing to
		/// one or more topics.
		/// 
		/// <pre>{@code
		/// Consumer<byte[]> consumer = client.newConsumer()
		///        .topic("my-topic")
		///        .subscriptionName("my-subscription-name")
		///        .subscribe();
		/// 
		/// while (true) {
		///     Message<byte[]> message = consumer.receive();
		///     System.out.println("Got message: " + message.getValue());
		///     consumer.acknowledge(message);
		/// }
		/// }</pre>
		/// </summary>
		/// <returns> a <seealso cref="ConsumerBuilder"/> object to configure and construct the <seealso cref="Consumer"/> instance
		/// 
		/// @since 2.0.0 </returns>
		IConsumerBuilder<sbyte[]> NewConsumer();

		/// <summary>
		/// Create a consumer builder with a specific schema for subscribing on a specific topic
		/// 
		/// <para>Since 2.2, if you are creating a consumer with non-bytes schema on a non-existence topic, it will
		/// automatically create the topic with the provided schema.
		/// 
		/// <pre>{@code
		/// Consumer<String> consumer = client.newConsumer(Schema.STRING)
		///        .topic("my-topic")
		///        .subscriptionName("my-subscription-name")
		///        .subscribe();
		/// 
		/// while (true) {
		///     Message<String> message = consumer.receive();
		///     System.out.println("Got message: " + message.getValue());
		///     consumer.acknowledge(message);
		/// }
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="schema">
		///          provide a way to convert between serialized data and domain objects </param>
		/// <returns> a <seealso cref="ConsumerBuilder"/> object to configure and construct the <seealso cref="Consumer"/> instance
		/// 
		/// @since 2.0.0 </returns>
		IConsumerBuilder<T> newConsumer<T>(Schema<T> Schema);

		/// <summary>
		/// Create a topic reader builder with no schema (<seealso cref="Schema.BYTES"/>) to read from the specified topic.
		/// 
		/// <para>The Reader provides a low-level abstraction that allows for manual positioning in the topic, without using a
		/// subscription. A reader needs to be specified a <seealso cref="ReaderBuilder.startMessageId(MessageId)"/>
		/// that can either be:
		/// <ul>
		/// <li><seealso cref="MessageId.earliest"/>: Start reading from the earliest message available in the topic</li>
		/// <li><seealso cref="MessageId.latest"/>: Start reading from end of the topic. The first message read will be the one
		/// published <b>*after*</b> the creation of the builder</li>
		/// <li><seealso cref="MessageId"/>: Position the reader on a particular message. The first message read will be the one
		/// immediately <b>*after*</b> the specified message</li>
		/// </ul>
		/// 
		/// </para>
		/// <para>A Reader can only from non-partitioned topics. In case of partitioned topics, one can create the readers
		/// directly on the individual partitions. See <seealso cref="getPartitionsForTopic(string)"/> for how to get the
		/// topic partitions names.
		/// 
		/// </para>
		/// <para>Example of usage of Reader:
		/// <pre>{@code
		/// Reader<byte[]> reader = client.newReader()
		///        .topic("my-topic")
		///        .startMessageId(MessageId.earliest)
		///        .create();
		/// 
		/// while (true) {
		///     Message<byte[]> message = reader.readNext();
		///     System.out.println("Got message: " + message.getValue());
		///     // Reader doesn't need acknowledgments
		/// }
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a <seealso cref="ReaderBuilder"/> that can be used to configure and construct a <seealso cref="Reader"/> instance
		/// @since 2.0.0 </returns>
		ReaderBuilder<sbyte[]> NewReader();

		/// <summary>
		/// Create a topic reader builder with a specific <seealso cref="Schema"/>) to read from the specified topic.
		/// 
		/// <para>The Reader provides a low-level abstraction that allows for manual positioning in the topic, without using a
		/// subscription. A reader needs to be specified a <seealso cref="ReaderBuilder.startMessageId(MessageId)"/> that can either
		/// be:
		/// <ul>
		/// <li><seealso cref="MessageId.earliest"/>: Start reading from the earliest message available in the topic</li>
		/// <li><seealso cref="MessageId.latest"/>: Start reading from end of the topic. The first message read will be the one
		/// published <b>*after*</b> the creation of the builder</li>
		/// <li><seealso cref="MessageId"/>: Position the reader on a particular message. The first message read will be the one
		/// immediately <b>*after*</b> the specified message</li>
		/// </ul>
		/// 
		/// </para>
		/// <para>A Reader can only from non-partitioned topics. In case of partitioned topics, one can create the readers
		/// directly on the individual partitions. See <seealso cref="getPartitionsForTopic(string)"/> for how to get the
		/// topic partitions names.
		/// 
		/// </para>
		/// <para>Example of usage of Reader:
		/// <pre>
		/// {@code
		/// Reader<String> reader = client.newReader(Schema.STRING)
		///        .topic("my-topic")
		///        .startMessageId(MessageId.earliest)
		///        .create();
		/// 
		/// while (true) {
		///     Message<String> message = reader.readNext();
		///     System.out.println("Got message: " + message.getValue());
		///     // Reader doesn't need acknowledgments
		/// }
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a <seealso cref="ReaderBuilder"/> that can be used to configure and construct a <seealso cref="Reader"/> instance
		/// 
		/// @since 2.0.0 </returns>
		ReaderBuilder<T> newReader<T>(Schema<T> Schema);

		/// <summary>
		/// Update the service URL this client is using.
		/// 
		/// <para>This will force the client close all existing connections and to restart service discovery to the new service
		/// endpoint.
		/// 
		/// </para>
		/// </summary>
		/// <param name="serviceUrl">
		///            the new service URL this client should connect to </param>
		/// <exception cref="PulsarClientException">
		///             in case the serviceUrl is not valid </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateServiceUrl(String serviceUrl) throws PulsarClientException;
		void UpdateServiceUrl(string ServiceUrl);

		/// <summary>
		/// Get the list of partitions for a given topic.
		/// 
		/// <para>If the topic is partitioned, this will return a list of partition names. If the topic is not partitioned, the
		/// returned list will contain the topic name itself.
		/// 
		/// </para>
		/// <para>This can be used to discover the partitions and create <seealso cref="Reader"/>, <seealso cref="Consumer"/> or <seealso cref="Producer"/>
		/// instances directly on a particular partition.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topic">
		///            the topic name </param>
		/// <returns> a future that will yield a list of the topic partitions or <seealso cref="PulsarClientException"/> if there was any
		///         error in the operation.
		/// @since 2.3.0 </returns>
		ValueTask<IList<string>> GetPartitionsForTopic(string Topic);

		/// <summary>
		/// Close the PulsarClient and release all the resources.
		/// 
		/// <para>This operation will trigger a graceful close of all producer, consumer and reader instances that
		/// this client has currently active. That implies that close will block and wait until all pending producer
		/// send requests are persisted.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="PulsarClientException">
		///             if the close operation fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override void close() throws PulsarClientException;
		void Close();

		/// <summary>
		/// Asynchronously close the PulsarClient and release all the resources.
		/// 
		/// <para>This operation will trigger a graceful close of all producer, consumer and reader instances that
		/// this client has currently active. That implies that close and wait, asynchronously, until all pending producer
		/// send requests are persisted.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="PulsarClientException">
		///             if the close operation fails </exception>
		ValueTask<Void> CloseAsync();

		/// <summary>
		/// Perform immediate shutdown of PulsarClient.
		/// 
		/// <para>Release all the resources and close all the producer, consumer and reader instances without waiting
		/// for ongoing operations to complete.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="PulsarClientException">
		///             if the forceful shutdown fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shutdown() throws PulsarClientException;
		void Shutdown();
	}

}