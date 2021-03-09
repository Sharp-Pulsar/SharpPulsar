using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages.Transaction;
using System.Threading.Tasks;
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
namespace SharpPulsar.Transaction
{
    /// <summary>
    /// The default implementation of transaction builder to build transactions.
    /// </summary>
    public class TransactionBuilder : ITransactionBuilder
	{

		private readonly ActorSystem _actorSystem;
		private readonly IActorRef _transactionCoordinatorClient;
		private readonly IActorRef _client;
		private long _txnTimeoutMs = 60000; // 1 minute
		private const long TxnRequestTimeoutMs = 1000 * 30; // 30 seconds
		private ILoggingAdapter _log;

		public TransactionBuilder(ActorSystem actorSystem, IActorRef client, IActorRef tcClient, ILoggingAdapter log)
		{
			_log = log;
			_actorSystem = actorSystem;
			_transactionCoordinatorClient = tcClient;
			_client = client;
		}

		public virtual ITransactionBuilder WithTransactionTimeout(long timeout, TimeUnit timeoutUnit)
		{
			_txnTimeoutMs = timeoutUnit.ToMilliseconds(timeout);
			return this;
		}

		public ITransaction Build()
		{
			return BuildAsync().GetAwaiter().GetResult();
		}
		public async Task<ITransaction> BuildAsync()
		{
			// talk to TC to begin a transaction
			//       the builder is responsible for locating the transaction coorindator (TC)
			//       and start the transaction to get the transaction id.
			//       After getting the transaction id, all the operations are handled by the
			//       `Transaction`
			var result = await _transactionCoordinatorClient.AskFor<NewTxnResponse>(new NewTxn(TxnRequestTimeoutMs, TimeUnit.MILLISECONDS));
			var txnID = result.Response;
			var transaction = _actorSystem.ActorOf(Transaction.Prop(_client, _txnTimeoutMs, (long)txnID.TxnidLeastBits, (long)txnID.TxnidMostBits));
			return new User.Transaction(transaction);	
		}
	}

}