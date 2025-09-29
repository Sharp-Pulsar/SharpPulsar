using SharpPulsar.API.Internal;
using SharpPulsar.API.Transaction;

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
namespace SharpPulsar.API
{

    /// <summary>
    /// Class that provides a client interface to Pulsar.
    /// 
    /// <para>Client instances are thread-safe and can be reused for managing multiple <seealso cref="Producer{T}"/>, <seealso cref="Consumer{T}"/> and
    /// <seealso cref="Reader{T}"/> instances.
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
    public interface IPulsarClient
    {

        /// <summary>
        /// Get a new builder instance that can used to configure and build a <seealso cref="PulsarClient"/> instance.
        /// </summary>
        /// <returns> the <seealso cref="ClientBuilder"/>
        /// 
        /// @since 2.0.0 </returns>
        static IClientBuilder Builder()
        {
            return DefaultImplementation.GetDefaultImplementation.NewClientBuilder();
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
        IProducerBuilder<byte[]> NewProducer();

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
        /// <returns> a <seealso cref="Producer{T}"/> instance
        /// 
        /// @since 2.0.0 </returns>
        IProducerBuilder<T> NewProducer<T>(ISchema<T> schema);

        /// <inheritdoc cref="NewProducer{T}(ISchema{T}, ProducerConfigBuilder{T})"/>
		ValueTask<IProducerBuilder<T>> NewProducerAsync<T>(ISchema<T> schema);

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
        IConsumerBuilder<byte[]> NewConsumer();

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
        /// <returns> a <seealso cref="Consumer{T}"/> instance
        /// 
        /// @since 2.0.0 </returns>
        IConsumerBuilder<T> NewConsumer<T>(ISchema<T> schema);

        /// <inheritdoc cref="NewConsumerAsync{T}(ISchema{T}, ConsumerConfigBuilder{T})"/>
		ValueTask<IConsumerBuilder<T>> NewConsumerAsync<T>(ISchema<T> schema);

        /// <summary>
        /// Create a topic reader builder with no schema (<seealso cref="ISchema{T}.Bytes"/>) to read from the specified topic.
        /// 
        /// <para>The Reader provides a low-level abstraction that allows for manual positioning in the topic, without using a
        /// subscription. A reader needs to be specified a <seealso cref="ReaderBuilder.startMessageId(MessageIdAdv)"/>
        /// that can either be:
        /// <ul>
        /// <li><seealso cref="MessageIdAdv.earliest"/>: Start reading from the earliest message available in the topic</li>
        /// <li><seealso cref="MessageIdAdv.latest"/>: Start reading from end of the topic. The first message read will be the one
        /// published <b>*after*</b> the creation of the builder</li>
        /// <li><seealso cref="MessageIdAdv"/>: Position the reader on a particular message. The first message read will be the one
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
        IReaderBuilder<byte[]> NewReader();
        ValueTask<IReaderBuilder<byte[]>> NewReaderAsync();

        /// <summary>
        /// Create a topic reader builder with a specific <seealso cref="Schema"/>) to read from the specified topic.
        /// 
        /// <para>The Reader provides a low-level abstraction that allows for manual positioning in the topic, without using a
        /// subscription. A reader needs to be specified a <seealso cref="ReaderBuilder.startMessageId(MessageIdAdv)"/> that can either
        /// be:
        /// <ul>
        /// <li><seealso cref="IMessageId.Earliest"/>: Start reading from the earliest message available in the topic</li>
        /// <li><seealso cref="IMessageId.Latest"/>: Start reading from end of the topic. The first message read will be the one
        /// published <b>*after*</b> the creation of the builder</li>
        /// <li><seealso cref="IMessageId"/>: Position the reader on a particular message. The first message read will be the one
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
        IReaderBuilder<T> NewReader<T>(ISchema<T> schema);
        ValueTask<IReaderBuilder<T>> NewReaderAsync<T>(ISchema<T> schema);

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
        ///             

        /// <summary>
        /// Create a table view builder with a specific schema for subscribing on a specific topic.
        /// 
        /// <para>The TableView provides a key-value map view of a compacted topic. Messages without keys will
        /// be ignored.
        /// 
        /// </para>
        /// <para>Example:
        /// <pre>{@code
        ///  TableView<byte[]> tableView = client.newTableView(Schema.BYTES)
        ///            .topic("my-topic")
        ///            .autoUpdatePartitionsInterval(5, TimeUnit.SECONDS)
        ///            .create();
        /// 
        ///  tableView.forEach((k, v) -> System.out.println(k + ":" + v));
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="schema"> provide a way to convert between serialized data and domain objects </param>
        /// <returns> a <seealso cref="ITableViewBuilder{T}"/> instance </returns>
        /// 
        [Obsolete]
        ITableViewBuilder<T> NewTableViewBuilder<T>(ISchema<T> schema);
        /// <summary>
        /// Create a table view builder for subscribing on a specific topic.
        ///     
        /// <para>The TableView provides a key-value map view of a compacted topic. Messages without keys will
        /// be ignored.
        ///     
        /// </para>
        /// <para>Example:
        /// <pre>{@code
        ///  TableView<byte[]> tableView = client.newTableView()
        ///            .topic("my-topic")
        ///            .autoUpdatePartitionsInterval(5, TimeUnit.SECONDS)
        ///            .create();
        ///     
        ///  tableView.forEach((k, v) -> System.out.println(k + ":" + v));
        /// }</pre>
        ///     
        /// </para>
        /// </summary>
        /// <returns> a <seealso cref="ITableViewBuilder{}"/> object to configure and construct the <seealso cref="TableView"/> instance </returns>
        ITableViewBuilder<byte[]> NewTableView();

        /// <summary>
        /// Create a table view builder with a specific schema for subscribing on a specific topic.
        /// 
        /// <para>The TableView provides a key-value map view of a compacted topic. Messages without keys will
        /// be ignored.
        /// 
        /// </para>
        /// <para>Example:
        /// <pre>{@code
        ///  TableView<byte[]> tableView = client.newTableView(Schema.BYTES)
        ///            .topic("my-topic")
        ///            .autoUpdatePartitionsInterval(5, TimeUnit.SECONDS)
        ///            .create();
        /// 
        ///  tableView.forEach((k, v) -> System.out.println(k + ":" + v));
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <param name="schema"> provide a way to convert between serialized data and domain objects </param>
        /// <returns> a <seealso cref="ITableViewBuilder{T}"/> object to configure and construct the <seealso cref="TableView"/> instance </returns>
        ITableViewBuilder<T> NewTableView<T>(ISchema<T> schema);

        void UpdateServiceUrl(string serviceUrl);

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
        IList<string> GetPartitionsForTopic(string topic, bool metadataAutoCreationEnabled);

        /// <summary>
        /// 1. Get the partitions if the topic exists. Return "[{partition-0}, {partition-1}....{partition-n}}]" if a
        ///   partitioned topic exists; return "[{topic}]" if a non-partitioned topic exists. </summary>
        /// 2. When {<param name="metadataAutoCreationEnabled">} is "false", neither the partitioned topic nor non-partitioned
        ///   topic does not exist. You will get an <seealso cref="PulsarClientException.NotFoundException"/> or a
        ///   <seealso cref="PulsarClientException.TopicDoesNotExistException"/>.
        ///  2-1. You will get a <seealso cref="PulsarClientException.NotSupportedException"/> with metadataAutoCreationEnabled=false
        ///    on an old broker version which does not support getting partitions without partitioned metadata auto-creation. </param>
        /// 3. When {<param name="metadataAutoCreationEnabled">} is "true," it will trigger an auto-creation for this topic(using
        ///   the default topic auto-creation strategy you set for the broker), and the corresponding result is returned.
        ///   For the result, see case 1.
        /// @version 3.3.0. </param>

        ValueTask<IList<string>> GetPartitionsForTopicAsync(string topic, bool metadataAutoCreationEnabled);


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
        void Shutdown();
        Task ShutdownAsync();

        /// <summary>
        /// Return internal state of the client. Useful if you want to check that current client is valid. </summary>
        /// <returns> true is the client has been closed </returns>
        /// <seealso cref=".shutdown()"/>
        /// <seealso cref=".close()"/>
        /// <seealso cref=".closeAsync()"/>
        bool IsClosed();



        /// <summary>
        /// Create a transaction builder that can be used to configure
        /// and construct a transaction.
        /// 
        /// <para>Example:
        /// 
        /// <pre>{@code
        /// Transaction txn = client.newTransaction()
        ///                         .withTransactionTimeout(1, TimeUnit.MINUTES)
        ///                         .build().get();
        /// }</pre>
        /// 
        /// </para>
        /// </summary>
        /// <returns> a <seealso cref="TransactionBuilder"/> object to configure and construct
        /// the <seealso cref="org.apache.pulsar.client.api.transaction.Transaction"/> instance
        /// 
        /// @since 2.7.0 </returns>
        ITransactionBuilder NewTransaction();
    }

}