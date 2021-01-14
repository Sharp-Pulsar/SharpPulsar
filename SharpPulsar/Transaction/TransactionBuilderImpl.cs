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
namespace Org.Apache.Pulsar.Client.Impl.Transaction
{
	using Slf4j = lombok.@extern.slf4j.Slf4j;
	using Transaction = Org.Apache.Pulsar.Client.Api.Transaction.Transaction;
	using TransactionBuilder = Org.Apache.Pulsar.Client.Api.Transaction.TransactionBuilder;
	using PulsarClientImpl = Org.Apache.Pulsar.Client.Impl.PulsarClientImpl;


	/// <summary>
	/// The default implementation of transaction builder to build transactions.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Slf4j public class TransactionBuilderImpl implements org.apache.pulsar.client.api.transaction.TransactionBuilder
	public class TransactionBuilderImpl : TransactionBuilder
	{

		private readonly PulsarClientImpl _client;
		private readonly TransactionCoordinatorClientImpl _transactionCoordinatorClient;
		private long _txnTimeoutMs = 60000; // 1 minute
		private const long TxnRequestTimeoutMs = 1000 * 30; // 30 seconds

		public TransactionBuilderImpl(PulsarClientImpl client, TransactionCoordinatorClientImpl tcClient)
		{
			this._client = client;
			this._transactionCoordinatorClient = tcClient;
		}

		public virtual TransactionBuilder WithTransactionTimeout(long timeout, TimeUnit timeoutUnit)
		{
			this._txnTimeoutMs = timeoutUnit.toMillis(timeout);
			return this;
		}

		public virtual CompletableFuture<Transaction> Build()
		{
			// talk to TC to begin a transaction
			//       the builder is responsible for locating the transaction coorindator (TC)
			//       and start the transaction to get the transaction id.
			//       After getting the transaction id, all the operations are handled by the
			//       `TransactionImpl`
			CompletableFuture<Transaction> future = new CompletableFuture<Transaction>();
			_transactionCoordinatorClient.NewTransactionAsync(TxnRequestTimeoutMs, TimeUnit.MILLISECONDS).whenComplete((txnID, throwable) =>
			{
			if(log.DebugEnabled)
			{
				log.debug("Success to new txn. txnID: {}", txnID);
			}
			if(throwable != null)
			{
				log.error("New transaction error.", throwable);
				future.completeExceptionally(throwable);
				return;
			}
			future.complete(new TransactionImpl(_client, _txnTimeoutMs, txnID.LeastSigBits, txnID.MostSigBits));
			});
			return future;
		}
	}

}