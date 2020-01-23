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
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;

	/// <summary>
	/// The implementation of <seealso cref="TransactionBufferClient"/>.
	/// </summary>
	public class TransactionBufferClientImpl : TransactionBufferClient
	{

		private readonly PulsarClientImpl client;

		public TransactionBufferClientImpl(PulsarClientImpl Client)
		{
			this.client = Client;
		}

		public override CompletableFuture<Void> CommitTxnOnTopic(string Topic, long TxnIdMostBits, long TxnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public override CompletableFuture<Void> AbortTxnOnTopic(string Topic, long TxnIdMostBits, long TxnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public override CompletableFuture<Void> CommitTxnOnSubscription(string Topic, string Subscription, long TxnIdMostBits, long TxnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public override CompletableFuture<Void> AbortTxnOnSubscription(string Topic, string Subscription, long TxnIdMostBits, long TxnIdLeastBits)
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}
	}

}