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
	/// Types of subscription supported by Pulsar.
	/// </summary>
	public enum SubscriptionType
	{
		/// <summary>
		/// There can be only 1 consumer on the same topic with the same subscription name.
		/// </summary>
		Exclusive,

		/// <summary>
		/// Multiple consumer will be able to use the same subscription name and the messages will be dispatched
		/// according to a round-robin rotation between the connected consumers.
		/// 
		/// <para>In this mode, the consumption order is not guaranteed.
		/// </para>
		/// </summary>
		Shared,

		/// <summary>
		/// Multiple consumer will be able to use the same subscription name but only 1 consumer will receive the messages.
		/// If that consumer disconnects, one of the other connected consumers will start receiving messages.
		/// 
		/// <para>In failover mode, the consumption ordering is guaranteed.
		/// 
		/// </para>
		/// <para>In case of partitioned topics, the ordering is guaranteed on a per-partition basis.
		/// The partitions assignments will be split across the available consumers. On each partition,
		/// at most one consumer will be active at a given point in time.
		/// </para>
		/// </summary>
		Failover,

		/// <summary>
		/// Multiple consumer will be able to use the same subscription and all messages with the same key
		/// will be dispatched to only one consumer.
		/// 
		/// <para>Use ordering_key to overwrite the message key for message ordering.
		/// </para>
		/// </summary>
		KeyShared
	}
}