using BAMCIS.Util.Concurrent;
using SharpPulsar.Api.Interceptor;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;

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
	/// <seealso cref="ProducerBuilder"/> is used to configure and create instances of <seealso cref="Producer"/>.
	/// </summary>
	/// <seealso cref= IPulsarClient#newProducer() </seealso>
	/// <seealso cref= PulsarClient#newProducer(Schema) </seealso>
	public interface IProducerBuilder<T> : ICloneable
	{

		/// <summary>
		/// Finalize the creation of the <seealso cref="Producer"/> instance.
		/// 
		/// <para>This method will block until the producer is created successfully.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the producer instance </returns>
		/// <exception cref="PulsarClientException.ProducerBusyException">
		///             if a producer with the same "producer name" is already connected to the topic </exception>
		/// <exception cref="PulsarClientException">
		///             if the producer creation fails </exception>
		///             
		ValueTask<IProducer<T>> Create();

		/// <summary>
		/// Load the configuration from provided <tt>config</tt> map.
		/// 
		/// <para>Example:
		/// <pre>{@code
		/// Map<String, Object> config = new HashMap<>();
		/// config.put("producerName", "test-producer");
		/// config.put("sendTimeoutMs", 2000);
		/// 
		/// ProducerBuilder<byte[]> builder = client.newProducer()
		///                  .loadConf(config);
		/// 
		/// Producer<byte[]> producer = builder.create();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="config"> configuration map to load </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> LoadConf(IDictionary<string, object> config);

		/// <summary>
		/// Create a copy of the current <seealso cref="ProducerBuilder"/>.
		/// 
		/// <para>Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
		/// example:
		/// <pre>{@code
		/// ProducerBuilder<String> builder = client.newProducer(Schema.STRING)
		///                  .sendTimeout(10, TimeUnit.SECONDS)
		///                  .blockIfQueueFull(true);
		/// 
		/// Producer<String> producer1 = builder.clone().topic("topic-1").create();
		/// Producer<String> producer2 = builder.clone().topic("topic-2").create();
		/// }</pre>
		/// 
		/// </para>
		/// </summary>
		/// <returns> a clone of the producer builder instance </returns>
		IProducerBuilder<T> Clone();

		/// <summary>
		/// Specify the topic this producer will be publishing on.
		/// 
		/// <para>This argument is required when constructing the produce.
		/// 
		/// </para>
		/// </summary>
		/// <param name="topicName"> the name of the topic </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> Topic(string topicName);

		/// <summary>
		/// Specify a name for the producer.
		/// 
		/// <para>If not assigned, the system will generate a globally unique name which can be accessed with
		/// <seealso cref="IProducer.getProducerName()"/>.
		/// 
		/// </para>
		/// <para><b>Warning</b>: When specifying a name, it is up to the user to ensure that, for a given topic,
		/// the producer name is unique across all Pulsar's clusters.
		/// Brokers will enforce that only a single producer a given name can be publishing on a topic.
		/// 
		/// </para>
		/// </summary>
		/// <param name="producerName">
		///            the custom name to use for the producer </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> ProducerName(string producerName);

		/// <summary>
		/// Set the send timeout <i>(default: 30 seconds)</i>.
		/// 
		/// <para>If a message is not acknowledged by the server before the sendTimeout expires, an error will be reported.
		/// 
		/// </para>
		/// <para>Setting the timeout to zero, for example {@code setTimeout(0, TimeUnit.SECONDS)} will set the timeout
		/// to infinity, which can be useful when using Pulsar's message deduplication feature, since the client
		/// library will retry forever to publish a message. No errors will be propagated back to the application.
		/// 
		/// </para>
		/// </summary>
		/// <param name="sendTimeout">
		///            the send timeout </param>
		/// <param name="unit">
		///            the time unit of the {@code sendTimeout} </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> SendTimeout(int sendTimeout, TimeUnit unit);

		/// <summary>
		/// Set the max size of the queue holding the messages pending to receive an acknowledgment from the broker.
		/// 
		/// <para>When the queue is full, by default, all calls to <seealso cref="IProducer.send"/> and <seealso cref="IProducer.sendAsync"/>
		/// will fail unless {@code blockIfQueueFull=true}. Use <seealso cref="blockIfQueueFull(bool)"/>
		/// to change the blocking behavior.
		/// 
		/// </para>
		/// <para>The producer queue size also determines the max amount of memory that will be required by
		/// the client application. Until, the producer gets a successful acknowledgment back from the broker,
		/// it will keep in memory (direct memory pool) all the messages in the pending queue.
		/// 
		/// </para>
		/// <para>Default is 1000.
		/// 
		/// </para>
		/// </summary>
		/// <param name="maxPendingMessages">
		///            the max size of the pending messages queue for the producer </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> MaxPendingMessages(int maxPendingMessages);

		/// <summary>
		/// Set the number of max pending messages across all the partitions.
		/// 
		/// <para>This setting will be used to lower the max pending messages for each partition
		/// (<seealso cref="maxPendingMessages(int)"/>), if the total exceeds the configured value.
		/// The purpose of this setting is to have an upper-limit on the number
		/// of pending messages when publishing on a partitioned topic.
		/// 
		/// </para>
		/// <para>Default is 50000.
		/// 
		/// </para>
		/// <para>If publishing at high rate over a topic with many partitions (especially when publishing messages without a
		/// partitioning key), it might be beneficial to increase this parameter to allow for more pipelining within the
		/// individual partitions producers.
		/// 
		/// </para>
		/// </summary>
		/// <param name="maxPendingMessagesAcrossPartitions">
		///            max pending messages across all the partitions </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> MaxPendingMessagesAcrossPartitions(int maxPendingMessagesAcrossPartitions);

		/// <summary>
		/// Set whether the <seealso cref="IProducer.send"/> and <seealso cref="IProducer.sendAsync"/> operations should block when the outgoing
		/// message queue is full.
		/// 
		/// <para>Default is {@code false}. If set to {@code false}, send operations will immediately fail with
		/// <seealso cref="PulsarClientException.ProducerQueueIsFullError"/> when there is no space left in pending queue. If set to
		/// {@code true}, the <seealso cref="IProducer.sendAsync"/> operation will instead block.
		/// 
		/// </para>
		/// <para>Setting {@code blockIfQueueFull=true} simplifies the task of an application that
		/// just wants to publish messages as fast as possible, without having to worry
		/// about overflowing the producer send queue.
		/// 
		/// </para>
		/// <para>For example:
		/// <pre><code>
		/// Producer&lt;String&gt; producer = client.newProducer()
		///                  .topic("my-topic")
		///                  .blockIfQueueFull(true)
		///                  .create();
		/// 
		/// while (true) {
		///     producer.sendAsync("my-message")
		///          .thenAccept(messageId -> {
		///              System.out.println("Published message: " + messageId);
		///          })
		///          .exceptionally(ex -> {
		///              System.err.println("Failed to publish: " + e);
		///              return null;
		///          });
		/// }
		/// </code></pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="blockIfQueueFull">
		///            whether to block <seealso cref="IProducer.send"/> and <seealso cref="IProducer.sendAsync"/> operations on queue full </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> BlockIfQueueFull(bool blockIfQueueFull);

		/// <summary>
		/// Set the <seealso cref="MessageRoutingMode"/> for a partitioned producer.
		/// 
		/// <para>Default routing mode is to round-robin across the available partitions.
		/// 
		/// </para>
		/// <para>This logic is applied when the application is not setting a key on a
		/// particular message. If the key is set with <seealso cref="MessageBuilder.setKey(string)"/>,
		/// then the hash of the key will be used to select a partition for the message.
		/// 
		/// </para>
		/// </summary>
		/// <param name="messageRoutingMode">
		///            the message routing mode </param>
		/// <returns> the producer builder instance </returns>
		/// <seealso cref= MessageRoutingMode </seealso>
		IProducerBuilder<T> MessageRoutingMode(MessageRoutingMode messageRoutingMode);

		/// <summary>
		/// Change the <seealso cref="HashingScheme"/> used to chose the partition on where to publish a particular message.
		/// 
		/// <para>Standard hashing functions available are:
		/// <ul>
		/// <li><seealso cref="JavaStringHash"/>: Java {@code String.hashCode()} (Default)
		/// <li><seealso cref="HashingScheme.Murmur3_32Hash"/>: Use Murmur3 hashing function.
		/// <a href="https://en.wikipedia.org/wiki/MurmurHash">https://en.wikipedia.org/wiki/MurmurHash</a>
		/// </ul>
		/// 
		/// </para>
		/// </summary>
		/// <param name="hashingScheme">
		///            the chosen <seealso cref="HashingScheme"/> </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> HashingScheme(HashingScheme hashingScheme);

		/// <summary>
		/// Set the compression type for the producer.
		/// 
		/// <para>By default, message payloads are not compressed. Supported compression types are:
		/// <ul>
		/// <li><seealso cref="ICompressionType.None"/>: No compression (Default)</li>
		/// <li><seealso cref="ICompressionType.Lz4"/>: Compress with LZ4 algorithm. Faster but lower compression than ZLib</li>
		/// <li><seealso cref="ICompressionType.Zlib"/>: Standard ZLib compression</li>
		/// <li><seealso cref="ICompressionType.Zstd"/> Compress with Zstandard codec. Since Pulsar 2.3. Zstd cannot be used if consumer
		/// applications are not in version >= 2.3 as well</li>
		/// <li><seealso cref="ICompressionType.Snappy"/> Compress with Snappy codec. Since Pulsar 2.4. Snappy cannot be used if
		/// consumer applications are not in version >= 2.4 as well</li>
		/// </ul>
		/// 
		/// </para>
		/// </summary>
		/// <param name="compressionType">
		///            the selected compression type </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> CompressionType(ICompressionType compressionType);

		/// <summary>
		/// Set a custom message routing policy by passing an implementation of MessageRouter.
		/// </summary>
		/// <param name="messageRouter"> </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> MessageRouter(IMessageRouter messageRouter);

		/// <summary>
		/// Control whether automatic batching of messages is enabled for the producer. <i>default: enabled</i>
		/// 
		/// <para>When batching is enabled, multiple calls to <seealso cref="IProducer.sendAsync"/> can result in a single batch
		/// to be sent to the broker, leading to better throughput, especially when publishing small messages.
		/// If compression is enabled, messages will be compressed at the batch level, leading to a much better
		/// compression ratio for similar headers or contents.
		/// 
		/// </para>
		/// <para>When enabled default batch delay is set to 1 ms and default batch size is 1000 messages
		/// 
		/// </para>
		/// <para>Batching is enabled by default since 2.0.0.
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref= #batchingMaxPublishDelay(long, TimeUnit) </seealso>
		/// <seealso cref= #batchingMaxMessages(int) </seealso>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> EnableBatching(bool enableBatching);

		/// <summary>
		/// Sets a <seealso cref="CryptoKeyReader"/>.
		/// 
		/// <para>Configure the key reader to be used to encrypt the message payloads.
		/// 
		/// </para>
		/// </summary>
		/// <param name="cryptoKeyReader">
		///            CryptoKeyReader object </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> CryptoKeyReader(ICryptoKeyReader cryptoKeyReader);

		/// <summary>
		/// Add public encryption key, used by producer to encrypt the data key.
		/// 
		/// <para>At the time of producer creation, Pulsar client checks if there are keys added to encryptionKeys. If keys are
		/// found, a callback <seealso cref="ICryptoKeyReader.getPrivateKey(string, System.Collections.IDictionary)"/> and
		/// <seealso cref="ICryptoKeyReader.getPublicKey(string, System.Collections.IDictionary)"/> is invoked against each key to load the values of the key.
		/// Application should implement this callback to return the key in pkcs8 format. If compression is enabled, message
		/// is encrypted after compression. If batch messaging is enabled, the batched message is encrypted.
		/// 
		/// </para>
		/// </summary>
		/// <param name="key">
		///            the name of the encryption key in the key store </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> AddEncryptionKey(string key);

		/// <summary>
		/// Sets the ProducerCryptoFailureAction to the value specified.
		/// </summary>
		/// <param name="action">
		///            the action the producer will take in case of encryption failures </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> CryptoFailureAction(ProducerCryptoFailureAction action);

		/// <summary>
		/// Set the time period within which the messages sent will be batched <i>default: 1 ms</i> if batch messages are
		/// enabled. If set to a non zero value, messages will be queued until either:
		/// <ul>
		/// <li>this time interval expires</li>
		/// <li>the max number of messages in a batch is reached (<seealso cref="batchingMaxMessages(int)"/>)
		/// <li>the max size of batch is reached
		/// </ul>
		/// 
		/// <para>All messages will be published as a single batch message. The consumer will be delivered individual
		/// messages in the batch in the same order they were enqueued.
		/// 
		/// </para>
		/// </summary>
		/// <param name="batchDelay">
		///            the batch delay </param>
		/// <param name="timeUnit">
		///            the time unit of the {@code batchDelay} </param>
		/// <returns> the producer builder instance </returns>
		/// <seealso cref= #batchingMaxMessages(int) </seealso>
		/// <seealso cref= #batchingMaxBytes(int) </seealso>
		IProducerBuilder<T> BatchingMaxPublishDelay(long batchDelay, TimeUnit timeUnit);

		/// <summary>
		/// Set the partition switch frequency while batching of messages is enabled and
		/// using round-robin routing mode for non-keyed message <i>default: 10</i>.
		/// 
		/// <para>The time period of partition switch is frequency * batchingMaxPublishDelay. During this period,
		/// all messages arrives will be route to the same partition.
		/// 
		/// </para>
		/// </summary>
		/// <param name="frequency"> the frequency of partition switch </param>
		/// <returns> the producer builder instance </returns>
		/// <seealso cref= #messageRoutingMode(MessageRoutingMode) </seealso>
		/// <seealso cref= #batchingMaxPublishDelay(long, TimeUnit) </seealso>
		IProducerBuilder<T> RoundRobinRouterBatchingPartitionSwitchFrequency(int frequency);

		/// <summary>
		/// Set the maximum number of messages permitted in a batch. <i>default: 1000</i> If set to a value greater than 1,
		/// messages will be queued until this threshold is reached or batch interval has elapsed.
		/// 
		/// <para>All messages in batch will be published as a single batch message. The consumer will be delivered individual
		/// messages in the batch in the same order they were enqueued.
		/// 
		/// </para>
		/// </summary>
		/// <param name="batchMessagesMaxMessagesPerBatch">
		///            maximum number of messages in a batch </param>
		/// <returns> the producer builder instance </returns>
		/// <seealso cref= #batchingMaxPublishDelay(long, TimeUnit) </seealso>
		/// <seealso cref= #batchingMaxBytes(int) </seealso>
		IProducerBuilder<T> BatchingMaxMessages(int batchMessagesMaxMessagesPerBatch);

		/// <summary>
		/// Set the maximum number of bytes permitted in a batch. <i>default: 128KB</i>
		/// If set to a value greater than 0, messages will be queued until this threshold is reached
		/// or other batching conditions are met.
		/// 
		/// <para>All messages in a batch will be published as a single batched message. The consumer will be delivered
		/// individual messages in the batch in the same order they were enqueued.
		/// 
		/// </para>
		/// </summary>
		/// <param name="batchingMaxBytes"> maximum number of bytes in a batch </param>
		/// <returns> the producer builder instance </returns>
		/// <seealso cref= #batchingMaxPublishDelay(long, TimeUnit) </seealso>
		/// <seealso cref= #batchingMaxMessages(int) </seealso>
		IProducerBuilder<T> BatchingMaxBytes(int batchingMaxBytes);

		/// <summary>
		/// Set the batcher builder <seealso cref="BatcherBuilder"/> of the producer. Producer will use the batcher builder to
		/// build a batch message container.This is only be used when batching is enabled.
		/// </summary>
		/// <param name="batcherBuilder">
		///          batcher builder </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> BatcherBuilder(IBatcherBuilder batcherBuilder);

		/// <summary>
		/// Set the baseline for the sequence ids for messages published by the producer.
		/// 
		/// <para>First message will be using {@code (initialSequenceId + 1)} as its sequence id and
		/// subsequent messages will be assigned incremental sequence ids, if not otherwise specified.
		/// 
		/// </para>
		/// </summary>
		/// <param name="initialSequenceId"> the initial sequence id for the producer </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> InitialSequenceId(long initialSequenceId);

		/// <summary>
		/// Set a name/value property with this producer.
		/// 
		/// <para>Properties are application defined metadata that can be attached to the producer.
		/// When getting the topic stats, this metadata will be associated to the producer
		/// stats for easier identification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="key">
		///            the property key </param>
		/// <param name="value">
		///            the property value </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> Property(string key, string value);

		/// <summary>
		/// Add all the properties in the provided map to the producer.
		/// 
		/// <para>Properties are application defined metadata that can be attached to the producer.
		/// When getting the topic stats, this metadata will be associated to the producer
		/// stats for easier identification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="properties"> the map of properties </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> Properties(IDictionary<string, string> properties);


        /// <summary>
        /// Add a set of <seealso cref="Api.interceptor.ProducerInterceptor"/> to the producer.
        /// 
        /// <para>Interceptors can be used to trace the publish and acknowledgments operation happening in a producer.
        /// 
        /// </para>
        /// </summary>
        /// <param name="interceptors">
        ///            the list of interceptors to intercept the producer created by this builder. </param>
        /// <returns> the producer builder instance </returns>
        IProducerBuilder<T> Intercept(params IProducerInterceptor[] interceptors);

		/// <summary>
		/// If enabled, partitioned producer will automatically discover new partitions at runtime. This is only applied on
		/// partitioned topics.
		/// 
		/// <para>Default is true.
		/// 
		/// </para>
		/// </summary>
		/// <param name="autoUpdate">
		///            whether to auto discover the partition configuration changes </param>
		/// <returns> the producer builder instance </returns>
		IProducerBuilder<T> AutoUpdatePartitions(bool autoUpdate);

		/// <summary>
		/// Control whether enable the multiple schema mode for producer.
		/// If enabled, producer can send a message with different schema from that specified just when it is created,
		/// otherwise a invalid message exception would be threw
		/// if the producer want to send a message with different schema.
		/// 
		/// <para>Enabled by default.
		/// 
		/// </para>
		/// </summary>
		/// <param name="multiSchema">
		///            indicates to enable or disable multiple schema mode </param>
		/// <returns> the producer builder instance
		/// @since 2.5.0 </returns>
		IProducerBuilder<T> EnableMultiSchema(bool multiSchema);
	}

}