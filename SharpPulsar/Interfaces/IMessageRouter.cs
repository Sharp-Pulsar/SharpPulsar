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
namespace SharpPulsar.Interfaces
{

	/// <summary>
	/// Interface for custom message router that can be passed
	/// to a producer to select the partition that a particular
	/// messsage should be published on.
	/// </summary>
	/// <seealso cref= ProducerBuilder#messageRouter(MessageRouter) </seealso>
	public interface IMessageRouter
	{

		/// 
		/// <param name="msg">
		///            Message object </param>
		/// <returns> The index of the partition to use for the message </returns>
		/// @deprecated since 1.22.0. Please use <seealso cref="choosePartition(IMessage, ITopicMetadata)"/> instead. 
		[Obsolete(@"since 1.22.0. Please use <seealso cref=""choosePartition(Message, TopicMetadata)""/> instead.")]
		virtual int ChoosePartition<T>(IMessage<T> msg)
		{
			throw new NotSupportedException("Use #choosePartition(Message, TopicMetadata) instead");
		}

		/// <summary>
		/// Choose a partition based on msg and the topic metadata.
		/// </summary>
		/// <param name="msg"> message to route </param>
		/// <param name="metadata"> topic metadata </param>
		/// <returns> the partition to route the message.
		/// @since 1.22.0 </returns>
		virtual int ChoosePartition<T>(IMessage<T> msg, ITopicMetadata metadata)
		{
			return ChoosePartition(msg);
		}

	}

}