using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar.Batch.Api
{

    public interface IBatchMessageContainerBase : IBatchMessageContainer
	{

		/// <summary>
		/// Add message to the batch message container.
		/// </summary>
		/// <param name="msg"> message will add to the batch message container </param>
		/// <param name="callback"> message send callback </param>
		/// <returns> true if the batch is full, otherwise false </returns>
		bool Add<T>(Message<T> msg, Action<object, Exception> callback);

		/// <summary>
		/// Check the batch message container have enough space for the message want to add.
		/// </summary>
		/// <param name="msg"> the message want to add </param>
		/// <returns> return true if the container have enough space for the specific message,
		///         otherwise return false. </returns>
		bool HaveEnoughSpace<T>(Message<T> msg);

		/// <summary>
		/// Check the batch message container has same schema with the message want to add.
		/// </summary>
		/// <param name="msg"> the message want to add </param>
		/// <returns> return true if the container has same schema with the specific message,
		///         otherwise return false. </returns>
		bool HasSameSchema<T>(Message<T> msg);

		/// <summary>
		/// Set producer of the message batch container.
		/// </summary>
		/// <param name="producer"> producer </param>
		IActorRef Producer {set;}

		ProducerContainer Container { get; set; }

		/// <summary>
		/// Create list of OpSendMsg, producer use OpSendMsg to send to the broker.
		/// </summary>
		/// <returns> list of OpSendMsg </returns>
		/// <exception cref="IOException"> </exception>
		IList<OpSendMsg<T>> CreateOpSendMsgs<T>();

		/// <summary>
		/// Create OpSendMsg, producer use OpSendMsg to send to the broker.
		/// </summary>
		/// <returns> OpSendMsg </returns>
		/// <exception cref="IOException"> </exception>
		OpSendMsg<T> CreateOpSendMsg<T>();

		/// <summary>
		/// Check whether the added message belong to the same txn with batch message container.
		/// </summary>
		/// <param name="msg"> added message </param>
		/// <returns> belong to the same txn or not </returns>
		bool HasSameTxn<T>(Message<T> msg);
	}

}