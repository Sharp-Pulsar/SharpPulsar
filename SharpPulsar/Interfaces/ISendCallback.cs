﻿using System.Threading.Tasks;
using SharpPulsar.Impl;

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
    /// 
	public interface ISendCallback
	{

		/// <summary>
		/// invoked when send operation completes
		/// </summary>
		/// <param name="e"> </param>
		void SendComplete(System.Exception e);

		/// <summary>
		/// used to specify a callback to be invoked on completion of a send operation for individual messages sent in a
		/// batch. Callbacks for messages in a batch get chained
		/// </summary>
		/// <param name="msg"> message sent </param>
		/// <param name="scb"> callback associated with the message </param>
		/// 
		void AddCallback(Message Msg, ISendCallback Scb);

		/// 
		/// <returns> next callback in chain </returns>
		ISendCallback NextSendCallback {get;}

		/// <summary>
		/// Return next message in chain
		/// </summary>
		/// <returns> next message in chain </returns>
		Message NextMessage {get;}

        void LastSequencePushed(long sequence);

		/// 
		/// <returns> future associated with callback </returns>
		TaskCompletionSource<IMessageId> Task {get;}
	}

}