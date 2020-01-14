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
namespace org.apache.pulsar.client.api
{

	/// <summary>
	/// A listener that will be called in order for every message received.
	/// </summary>
	public interface MessageListener<T>
	{
		/// <summary>
		/// This method is called whenever a new message is received.
		/// 
		/// <para>Messages are guaranteed to be delivered in order and from the same thread for a single consumer
		/// 
		/// </para>
		/// <para>This method will only be called once for each message, unless either application or broker crashes.
		/// 
		/// </para>
		/// <para>Application is responsible for acking message by calling any of consumer acknowledgement methods.
		/// 
		/// </para>
		/// <para>Application is responsible of handling any exception that could be thrown while processing the message.
		/// 
		/// </para>
		/// </summary>
		/// <param name="consumer">
		///            the consumer that received the message </param>
		/// <param name="msg">
		///            the message object </param>
		void received(Consumer<T> consumer, Message<T> msg);

		/// <summary>
		/// Get the notification when a topic is terminated.
		/// </summary>
		/// <param name="consumer">
		///            the Consumer object associated with the terminated topic </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods unless the C#8 option for this is selected:
//		default void reachedEndOfTopic(Consumer<T> consumer)
	//	{
	//		// By default ignore the notification
	//	}
	}

}