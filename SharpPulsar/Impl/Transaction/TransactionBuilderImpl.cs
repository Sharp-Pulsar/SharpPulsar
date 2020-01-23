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
	using Transaction = SharpPulsar.Api.Transaction.Transaction;
	using TransactionBuilder = SharpPulsar.Api.Transaction.TransactionBuilder;

	/// <summary>
	/// The default implementation of transaction builder to build transactions.
	/// </summary>
	public class TransactionBuilderImpl : TransactionBuilder
	{

		private readonly PulsarClientImpl client;
		private long txnTimeoutMs = 60000; // 1 minute

		public TransactionBuilderImpl(PulsarClientImpl Client)
		{
			this.client = Client;
		}

		public override TransactionBuilder WithTransactionTimeout(long Timeout, BAMCIS.Util.Concurrent.TimeUnit TimeoutUnit)
		{
			this.txnTimeoutMs = TimeoutUnit.toMillis(Timeout);
			return this;
		}

		public override CompletableFuture<Transaction> Build()
		{
			// TODO: talk to TC to begin a transaction
			//       the builder is responsible for locating the transaction coorindator (TC)
			//       and start the transaction to get the transaction id.
			//       After getting the transaction id, all the operations are handled by the
			//       `TransactionImpl`
			return CompletableFuture.completedFuture(new TransactionImpl(client, txnTimeoutMs, -1L, -1L));
		}
	}

}