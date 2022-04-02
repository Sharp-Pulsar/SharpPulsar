using Akka.Util;
using SharpPulsar.Auth;
using System.Buffers;
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
namespace SharpPulsar.Interfaces
{
	/// <summary>
	/// The message abstraction used in Pulsar.
	/// </summary>
	public interface IMessage<T>
	{

		/// <summary>
		/// Return the properties attached to the message.
		/// 
		/// <para>Properties are application defined key/value pairs that will be attached to the message.
		/// 
		/// </para>
		/// </summary>
		/// <returns> an unmodifiable view of the properties map </returns>
		IDictionary<string, string> Properties { get; }

		/// <summary>
		/// Check whether the message has a specific property attached.
		/// </summary>
		/// <param name="name"> the name of the property to check </param>
		/// <returns> true if the message has the specified property and false if the properties is not defined </returns>
		bool HasProperty(string name);

		/// <summary>
		/// Get the value of a specific property.
		/// </summary>
		/// <param name="name"> the name of the property </param>
		/// <returns> the value of the property or null if the property was not defined </returns>
		string GetProperty(string name);

        /// <summary>
        /// Get the raw payload of the message.
        /// 
        /// <para>Even when using the Schema and type-safe API, an application
        /// has access to the underlying raw message payload.
        /// 
        /// To avoid allocation: 
        /// var pool = ArrayPool<byte>.Shared;
        /// var payload = pool.Rent((int)msg.Data.Length);
        /// </para>
        /// </summary>
        /// <returns> the byte array with the message payload </returns>
        ReadOnlySequence<byte> Data { get; }

        long Size();

		/// <summary>
		/// Get the de-serialized value of the message, according the configured <seealso cref="Schema"/>.
		/// </summary>
		/// <returns> the deserialized value of the message </returns>
		T Value { get; }

		/// <summary>
		/// Get the unique message ID associated with this message.
		/// 
		/// <para>The message id can be used to univocally refer to a message without having the keep
		/// the entire payload in memory.
		/// 
		/// </para>
		/// <para>Only messages received from the consumer will have a message id assigned.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the message id null if this message was not received by this client instance </returns>
		IMessageId MessageId { get; }

        /// <summary>
        /// Get the schema associated to the message.
        /// Please note that this schema is usually equal to the Schema you passed
        /// during the construction of the Consumer or the Reader.
        /// But if you are consuming the topic using the GenericObject interface
        /// this method will return the schema associated with the message. </summary>
        /// <returns> The schema used to decode the payload of message. </returns>
        /// <seealso cref= Schema#AUTO_CONSUME() </seealso>
        virtual Option<ISchema<T>> ReaderSchema
        {
            get
            {
                return Option<ISchema<T>>.None;
            }
        }

        /// <summary>
        /// Get the publish time of this message. The publish time is the timestamp that a client publish the message.
        /// </summary>
        /// <returns> publish time of this message. </returns>
        /// <seealso cref= #getEventTime() </seealso>
        long PublishTime { get; }

		/// <summary>
		/// Get the event time associated with this message. It is typically set by the applications via
		/// <seealso cref="MessageBuilder.setEventTime(long)"/>.
		/// 
		/// <para>If there isn't any event time associated with this event, it will return 0.
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref= MessageBuilder#setEventTime(long)
		/// @since 1.20.0 </seealso>
		/// <returns> the message event time or 0 if event time wasn't set </returns>
		long EventTime { get; }

		/// <summary>
		/// Get the sequence id associated with this message. It is typically set by the applications via
		/// <seealso cref="MessageBuilder.setSequenceId(long)"/>.
		/// </summary>
		/// <returns> sequence id associated with this message. </returns>
		/// <seealso cref= MessageBuilder#setEventTime(long)
		/// @since 1.22.0 </seealso>
		long SequenceId { get; }

		/// <summary>
		/// Get the producer name who produced this message.
		/// </summary>
		/// <returns> producer name who produced this message, null if producer name is not set.
		/// @since 1.22.0 </returns>
		string ProducerName { get; }

		/// <summary>
		/// Check whether the message has a key.
		/// </summary>
		/// <returns> true if the key was set while creating the message and false if the key was not set
		/// while creating the message </returns>
		bool HasKey();

		/// <summary>
		/// Get the key of the message.
		/// </summary>
		/// <returns> the key of the message </returns>
		string Key { get; }

		/// <summary>
		/// Check whether the key has been base64 encoded.
		/// </summary>
		/// <returns> true if the key is base64 encoded, false otherwise </returns>
		bool HasBase64EncodedKey();

		/// <summary>
		/// Get bytes in key. If the key has been base64 encoded, it is decoded before being returned.
		/// Otherwise, if the key is a plain string, this method returns the UTF_8 encoded bytes of the string. </summary>
		/// <returns> the key in byte[] form </returns>
		byte[] KeyBytes { get; }

		/// <summary>
		/// Check whether the message has a ordering key.
		/// </summary>
		/// <returns> true if the ordering key was set while creating the message
		///         false if the ordering key was not set while creating the message </returns>
		bool HasOrderingKey();

		/// <summary>
		/// Get the ordering key of the message.
		/// </summary>
		/// <returns> the ordering key of the message </returns>
		byte[] OrderingKey { get; }

		/// <summary>
		/// Get the topic the message was published to.
		/// </summary>
		/// <returns> the topic the message was published to </returns>
		string Topic { get; }

		/// <summary>
		/// <seealso cref="EncryptionContext"/> contains encryption and compression information in it using which application can
		/// decrypt consumed message with encrypted-payload.
		/// </summary>
		/// <returns> the optiona encryption context </returns>
		Option<EncryptionContext> EncryptionCtx { get; }

		/// <summary>
		/// Get message redelivery count, redelivery count maintain in pulsar broker. When client acknowledge message
		/// timeout, broker will dispatch message again with message redelivery count in CommandMessage defined.
		/// 
		/// <para>Message redelivery increases monotonically in a broker, when topic switch ownership to a another broker
		/// redelivery count will be recalculated.
		/// 
		/// @since 2.3.0
		/// </para>
		/// </summary>
		/// <returns> message redelivery count </returns>
		int RedeliveryCount { get; }

		/// <summary>
		/// Get schema version of the message.
		/// @since 2.4.0 </summary>
		/// <returns> Schema version of the message if the message is produced with schema otherwise null. </returns>
		byte[] SchemaVersion { get; }

		/// <summary>
		/// Check whether the message is replicated from other cluster.
		/// 
		/// @since 2.4.0 </summary>
		/// <returns> true if the message is replicated from other cluster.
		///         false otherwise. </returns>
		bool Replicated { get; }

		/// <summary>
		/// Get name of cluster, from which the message is replicated.
		/// 
		/// @since 2.4.0 </summary>
		/// <returns> the name of cluster, from which the message is replicated. </returns>
		string ReplicatedFrom { get; }

        void AddProperty(IDictionary<string, string> props);
	}

}