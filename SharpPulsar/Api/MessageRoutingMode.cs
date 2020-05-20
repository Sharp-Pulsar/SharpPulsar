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
	/// Default routing mode for messages to partition.
	/// 
	/// <para>This logic is applied when the application is not setting a key <seealso cref="MessageBuilder.setKey(string)"/>
	/// on a particular message.
	/// </para>
	/// </summary>
	public enum MessageRoutingMode
	{
		/// <summary>
		//RoundRobinMode is a router that sends messages to routees(Producers) in round-robin order.
		//It's the simplest way to distribute messages to multiple worker actors, on a best-effort basis
		/// </summary>
		RoundRobinMode,

		/// <summary>
		/// BroadcastMode router will, as the name implies,
		/// broadcast any message to all of its routees(Producers).
		/// </summary>
		BroadcastMode,

		/// <summary>
		/// The RandomMode router will forward messages to routees(Producers) in random order.
		/// </summary>
		RandomMode,

		/// <summary>
		/// The ConsistentHashingMode is router that use a consistent hashing algorithm to select a routee(Producer) to forward the message.
		/// The idea is that messages with the same key are forwarded to the same routee(Producer).
		/// Any .NET object can be used as a key, although it's usually a number, string or Guid.
		/// </summary>
		ConsistentHashingMode
	}

}