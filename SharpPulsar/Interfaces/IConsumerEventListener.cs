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

using System;
using SharpPulsar.Messages.Consumer;

namespace SharpPulsar.Interfaces
{

	/// <summary>
	/// Listener on the consumer state changes.
	/// </summary>
	public interface IConsumerEventListener
	{

		/// <summary>
		/// Notified when the consumer group is changed, and the consumer becomes the active consumer.
		/// </summary>
		/// <param name="consumer">
		///            the consumer that originated the event </param>
		/// <param name="partitionId">
		///            the id of the partition that became active </param>
		void BecameActive(string consumer, int partitionId);

		/// <summary>
		/// Notified when the consumer group is changed, and the consumer is still inactive or becomes inactive.
		/// </summary>
		/// <param name="consumer">
		///            the consumer that originated the event </param>
		/// <param name="partitionId">
		///            the id of the partition that became inactive </param>
		void BecameInactive(string consumer, int partitionId);

        void Error(Exception ex);
        void Log(string log);

        void Created(CreatedConsumer createdConsumer);
    }

}