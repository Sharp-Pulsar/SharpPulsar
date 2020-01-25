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
namespace SharpPulsar.Api
{
	/// <summary>
	/// Batch message container for individual messages being published until they are batched and sent to broker.
	/// </summary>
	public interface BatchMessageContainer
	{

		/// <summary>
		/// Clear the message batch container.
		/// </summary>
		void Clear();

		/// <summary>
		/// Check the message batch container is empty.
		/// </summary>
		/// <returns> return true if empty, otherwise return false. </returns>
		bool Empty {get;}

		/// <summary>
		/// Get count of messages in the message batch container.
		/// </summary>
		/// <returns> messages count </returns>
		int NumMessagesInBatch {get;}

		/// <summary>
		/// Get current message batch size of the message batch container in bytes.
		/// </summary>
		/// <returns> message batch size in bytes </returns>
		long CurrentBatchSize {get;}

		/// <summary>
		/// Release the payload and clear the container.
		/// </summary>
		/// <param name="ex"> cause </param>
		void Discard(System.Exception Ex);

		/// <summary>
		/// Return the batch container batch message in multiple batches.
		/// @return
		/// </summary>
		bool MultiBatches {get;}
	}

}