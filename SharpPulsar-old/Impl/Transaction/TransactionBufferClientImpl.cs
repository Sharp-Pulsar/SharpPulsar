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
namespace SharpPulsar.Impl.Transaction
{
	using SharpPulsar.Interface;
	using System.Threading.Tasks;

	/// <summary>
	/// The implementation of <seealso cref="TransactionBufferClient"/>.
	/// </summary>
	public class TransactionBufferClientImpl : ITransactionBufferClient
	{

		private readonly PulsarClientImpl client;

		public TransactionBufferClientImpl(PulsarClientImpl client)
		{
			this.client = client;
		}

		public virtual ValueTask CommitTxnOnTopic(string topic, long txnIdMostBits, long txnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public virtual ValueTask AbortTxnOnTopic(string topic, long txnIdMostBits, long txnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public virtual ValueTask CommitTxnOnSubscription(string topic, string subscription, long txnIdMostBits, long txnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public virtual ValueTask AbortTxnOnSubscription(string topic, string subscription, long txnIdMostBits, long txnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}
	}

}