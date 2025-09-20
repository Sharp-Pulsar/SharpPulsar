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
namespace SharpPulsar.Common.Entity
{
	//using Builder = lombok.Builder;
	//using Data = lombok.Data;

	/// <summary>
	/// Configuration for the "dead letter queue" feature in consumer.
	/// </summary>
	/// <seealso cref= ConsumerBuilder#deadLetterPolicy(DeadLetterPolicy) </seealso>
	public class DeadLetterPolicy
	{

		/// <summary>
		/// Maximum number of times that a message will be redelivered before being sent to the dead letter queue.
		/// </summary>
		public int MaxRedeliverCount { get; set; }

		/// <summary>
		/// Name of the topic where the failing messages will be sent.
		/// </summary>
		public string DeadLetterTopic { get; set; }

	}

}