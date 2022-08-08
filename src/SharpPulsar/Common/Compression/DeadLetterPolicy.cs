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
    /// Configuration for the "dead letter queue" feature in consumer.
    /// </summary>
    /// <seealso cref= DeadLetterPolicy </seealso>
    public class DeadLetterPolicy
    {

        /// <summary>
        /// Maximum number of times that a message will be redelivered before being sent to the dead letter queue.
        /// </summary>
        public int MaxRedeliverCount { get; set; }

        /// <summary>
		/// Name of the retry topic where the failing messages will be sent.
		/// </summary>
		public string RetryLetterTopic { get; set; }

        /// <summary>
        /// Name of the topic where the failing messages will be sent.
        /// </summary>
        public string DeadLetterTopic { get; set; }
        
        /// <summary>
		/// Name of the initial subscription name of the dead letter topic.
		/// If this field is not set, the initial subscription for the dead letter topic will not be created.
		/// If this field is set but the broker's `allowAutoSubscriptionCreation` is disabled, the DLQ producer will fail
		/// to be created.
		/// </summary>
		public string InitialSubscriptionName { get; set; }

    }

}