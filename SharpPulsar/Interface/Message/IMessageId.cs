using System;

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
namespace Pulsar.Api
{

	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;

	/// <summary>
	/// Opaque unique identifier of a single message
	/// 
	/// <para>The MessageId can be used to reference a specific message, for example when acknowledging, without having
	/// to retain the message content in memory for an extended period of time.
	/// 
	/// </para>
	/// <para>Message ids are <seealso cref="System.IComparable"/> and a bigger message id will imply that a message was published "after"
	/// the other one.
	/// </para>
	/// </summary>
	public interface IMessageId : IComparable<IMessageId>
	{

		/// <summary>
		/// Serialize the message ID into a byte array.
		/// 
		/// <para>The serialized message id can be stored away and later get deserialized by
		/// using <seealso cref="fromByteArray(sbyte[])"/>.
		/// </para>
		/// </summary>
		sbyte[] ToByteArray();

		/// <summary>
		/// De-serialize a message id from a byte array.
		/// </summary>
		/// <param name="data">
		///            byte array containing the serialized message id </param>
		/// <returns> the de-serialized messageId object </returns>
		/// <exception cref="IOException"> if the de-serialization fails </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static MessageId fromByteArray(byte[] data) throws java.io.IOException
	//	{
	//		return DefaultImplementation.newMessageIdFromByteArray(data);
	//	}

		/// <summary>
		/// De-serialize a message id from a byte array with its topic
		/// information attached.
		/// 
		/// <para>The topic information is needed when acknowledging a <seealso cref="MessageId"/> on
		/// a consumer that is consuming from multiple topics.
		/// 
		/// </para>
		/// </summary>
		/// <param name="data"> the byte array with the serialized message id </param>
		/// <param name="topicName"> the topic name </param>
		/// <returns> a <seealso cref="MessageId instance"/> </returns>
		/// <exception cref="IOException"> if the de-serialization fails </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods unless the C#8 option for this is selected:
//		static MessageId fromByteArrayWithTopic(byte[] data, String topicName) throws java.io.IOException
	//	{
	//		return DefaultImplementation.newMessageIdFromByteArrayWithTopic(data, topicName);
	//	}

		// CHECKSTYLE.OFF: ConstantName

		/// <summary>
		/// MessageId that represents the oldest message available in the topic.
		/// </summary>

		/// <summary>
		/// MessageId that represents the next message published in the topic.
		/// </summary>

		// CHECKSTYLE.ON: ConstantName
	}

	public static class MessageId_Fields
	{
		public static readonly IMessageId earliest = DefaultImplementation.newMessageId(-1, -1, -1);
		public static readonly IMessageId latest = DefaultImplementation.newMessageId(long.MaxValue, long.MaxValue, -1);
	}

}