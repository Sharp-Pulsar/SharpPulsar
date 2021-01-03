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
	/// Message builder that constructs a message to be published through a producer.
	/// 
	/// <para>Usage example:
	/// <pre><code>
	/// producer.newMessage().key(myKey).value(myValue).send();
	/// </code></pre>
	/// </para>
	/// </summary>
	public interface ITypedMessageBuilder
	{
		/// <summary>
		/// Sets the key of the message for routing policy.
		/// </summary>
		/// <param name="key"> the partitioning key for the message </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder Key(string key);

		/// <summary>
		/// Sets the bytes of the key of the message for routing policy.
		/// Internally the bytes will be base64 encoded.
		/// </summary>
		/// <param name="key"> routing key for message, in byte array form </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder KeyBytes(sbyte[] key);

		/// <summary>
		/// Sets the ordering key of the message for message dispatch in <seealso cref="SubscriptionType.Key_Shared"/> mode.
		/// Partition key Will be used if ordering key not specified.
		/// </summary>
		/// <param name="orderingKey"> the ordering key for the message </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder OrderingKey(sbyte[] orderingKey);

		/// <summary>
		/// Set a domain object on the message.
		/// </summary>
		/// <param name="value">
		///            the domain object </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder Value(object value);

		/// <summary>
		/// Sets a new property on a message.
		/// </summary>
		/// <param name="name">
		///            the name of the property </param>
		/// <param name="value">
		///            the associated value </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder Property(string name, string value);

		/// <summary>
		/// Add all the properties in the provided map. </summary>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder Properties(IDictionary<string, string> properties);

		/// <summary>
		/// Set the event time for a given message.
		/// 
		/// <para>Applications can retrieve the event time by calling <seealso cref="IMessage.getEventTime()"/>.
		/// 
		/// </para>
		/// <para>Note: currently pulsar doesn't support event-time based index. so the subscribers
		/// can't seek the messages by event time.
		/// </para>
		/// </summary>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder EventTime(long timestamp);

		/// <summary>
		/// Specify a custom sequence id for the message being published.
		/// 
		/// <para>The sequence id can be used for deduplication purposes and it needs to follow these rules:
		/// <ol>
		/// <li><code>sequenceId >= 0</code>
		/// <li>Sequence id for a message needs to be greater than sequence id for earlier messages:
		/// <code>sequenceId(N+1) > sequenceId(N)</code>
		/// <li>It's not necessary for sequence ids to be consecutive. There can be holes between messages. Eg. the
		/// <code>sequenceId</code> could represent an offset or a cumulative Size.
		/// </ol>
		/// 
		/// </para>
		/// </summary>
		/// <param name="sequenceId">
		///            the sequence id to assign to the current message </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder SequenceId(long sequenceId);

		/// <summary>
		/// Override the geo-replication clusters for this message.
		/// </summary>
		/// <param name="clusters"> the list of clusters. </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder ReplicationClusters(IList<string> clusters);

		/// <summary>
		/// Disable geo-replication for this message.
		/// </summary>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder DisableReplication();

		/// <summary>
		/// Deliver the message only at or after the specified absolute timestamp.
		/// 
		/// <para>The timestamp is milliseconds and based on UTC (eg: <seealso cref="System.currentTimeMillis()"/>.
		/// 
		/// </para>
		/// <para><b>Note</b>: messages are only delivered with delay when a consumer is consuming
		/// through a <seealso cref="SubscriptionType.Shared"/> subscription. With other subscription
		/// types, the messages will still be delivered immediately.
		/// 
		/// </para>
		/// </summary>
		/// <param name="timestamp">
		///            absolute timestamp indicating when the message should be delivered to consumers </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder DeliverAt(long timestamp);

		/// <summary>
		/// Request to deliver the message only after the specified relative delay.
		/// 
		/// <para><b>Note</b>: messages are only delivered with delay when a consumer is consuming
		/// through a <seealso cref="SubscriptionType.Shared"/> subscription. With other subscription
		/// types, the messages will still be delivered immediately.
		/// 
		/// </para>
		/// </summary>
		/// <param name="delay">
		///            the amount of delay before the message will be delivered </param>
		/// <param name="unit">
		///            the time unit for the delay </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder DeliverAfter(long delay);

		/// <summary>
		/// Configure the <seealso cref="TypedMessageBuilder"/> from a config map, as an alternative compared
		/// to call the individual builder methods.
		/// 
		/// <para>The "value" of the message itself cannot be set on the config map.
		/// 
		/// </para>
		/// <para>Example:
		/// 
		/// <pre>{@code
		/// Map<String, Object> conf = new HashMap<>();
		/// conf.put("key", "my-key");
		/// conf.put("eventTime", System.currentTimeMillis());
		/// 
		/// producer.newMessage()
		///             .value("my-message")
		///             .loadConf(conf)
		///             .send();
		/// }</pre>
		/// 
		/// </para>
		/// <para>The available options are:
		/// <table border="1">
		///  <tr>
		///    <th>Constant</th>
		///    <th>Name</th>
		///    <th>Type</th>
		///    <th>Doc</th>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_KEY"/></td>
		///    <td>{@code key}</td>
		///    <td>{@code String}</td>
		///    <td><seealso cref="key(string)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_PROPERTIES"/></td>
		///    <td>{@code properties}</td>
		///    <td>{@code Map<String,String>}</td>
		///    <td><seealso cref="properties(System.Collections.IDictionary)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_EVENT_TIME"/></td>
		///    <td>{@code eventTime}</td>
		///    <td>{@code long}</td>
		///    <td><seealso cref="eventTime(long)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_SEQUENCE_ID"/></td>
		///    <td>{@code sequenceId}</td>
		///    <td>{@code long}</td>
		///    <td><seealso cref="sequenceId(long)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_REPLICATION_CLUSTERS"/></td>
		///    <td>{@code replicationClusters}</td>
		///    <td>{@code List<String>}</td>
		///    <td><seealso cref="replicationClusters(System.Collections.IList)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_DISABLE_REPLICATION"/></td>
		///    <td>{@code disableReplication}</td>
		///    <td>{@code boolean}</td>
		///    <td><seealso cref="disableReplication()"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_DELIVERY_AFTER_SECONDS"/></td>
		///    <td>{@code deliverAfterSeconds}</td>
		///    <td>{@code long}</td>
		///    <td><seealso cref="deliverAfter(long, TimeUnit)"/></td>
		///  </tr>
		///  <tr>
		///    <td><seealso cref="CONF_DELIVERY_AT"/></td>
		///    <td>{@code deliverAt}</td>
		///    <td>{@code long}</td>
		///    <td><seealso cref="deliverAt(long)"/></td>
		///  </tr>
		/// </table>
		/// 
		/// </para>
		/// </summary>
		/// <param name="config"> a map with the configuration options for the message </param>
		/// <returns> the message builder instance </returns>
		ITypedMessageBuilder LoadConf(IDictionary<string, object> config);
	}

	public static class TypedMessageBuilderFields
	{
		public const string ConfKey = "key";
		public const string ConfProperties = "properties";
		public const string ConfEventTime = "eventTime";
		public const string ConfSequenceId = "sequenceId";
		public const string ConfReplicationClusters = "replicationClusters";
		public const string ConfDisableReplication = "disableReplication";
		public const string ConfDeliveryAfterSeconds = "deliverAfterSeconds";
		public const string ConfDeliveryAt = "deliverAt";
	}

}