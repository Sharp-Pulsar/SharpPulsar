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
namespace org.apache.pulsar.client.impl
{
	using BatchMessageContainer = org.apache.pulsar.client.api.BatchMessageContainer;
	using OpSendMsg = org.apache.pulsar.client.impl.ProducerImpl.OpSendMsg;


	public interface BatchMessageContainerBase : BatchMessageContainer
	{

		/// <summary>
		/// Add message to the batch message container.
		/// </summary>
		/// <param name="msg"> message will add to the batch message container </param>
		/// <param name="callback"> message send callback </param>
		/// <returns> true if the batch is full, otherwise false </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: boolean add(MessageImpl<?> msg, SendCallback callback);
		bool add<T1>(MessageImpl<T1> msg, SendCallback callback);

		/// <summary>
		/// Check the batch message container have enough space for the message want to add.
		/// </summary>
		/// <param name="msg"> the message want to add </param>
		/// <returns> return true if the container have enough space for the specific message,
		///         otherwise return false. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: boolean haveEnoughSpace(MessageImpl<?> msg);
		bool haveEnoughSpace<T1>(MessageImpl<T1> msg);

		/// <summary>
		/// Check the batch message container has same schema with the message want to add.
		/// </summary>
		/// <param name="msg"> the message want to add </param>
		/// <returns> return true if the container has same schema with the specific message,
		///         otherwise return false. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: boolean hasSameSchema(MessageImpl<?> msg);
		bool hasSameSchema<T1>(MessageImpl<T1> msg);

		/// <summary>
		/// Set producer of the message batch container.
		/// </summary>
		/// <param name="producer"> producer </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void setProducer(ProducerImpl<?> producer);
		ProducerImpl<T1> Producer<T1> {set;}

		/// <summary>
		/// Create list of OpSendMsg, producer use OpSendMsg to send to the broker.
		/// </summary>
		/// <returns> list of OpSendMsg </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.client.impl.ProducerImpl.OpSendMsg> createOpSendMsgs() throws java.io.IOException;
		IList<OpSendMsg> createOpSendMsgs();

		/// <summary>
		/// Create OpSendMsg, producer use OpSendMsg to send to the broker.
		/// </summary>
		/// <returns> OpSendMsg </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.client.impl.ProducerImpl.OpSendMsg createOpSendMsg() throws java.io.IOException;
		OpSendMsg createOpSendMsg();
	}

}