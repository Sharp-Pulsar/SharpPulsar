using Akka.Actor;
using Akka.Event;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Transaction;
using System;
using System.Threading.Tasks;
using static SharpPulsar.Exceptions.TransactionCoordinatorClientException;
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
        private TimeSpan _txnTimeoutMs = TimeSpan.FromSeconds(60); // 1 minute
		private ILoggingAdapter _log;

		public TransactionBuilder(ActorSystem actorSystem, IActorRef client, IActorRef tcClient, ILoggingAdapter log)
		{
			_log = log;
			_actorSystem = actorSystem;
			_transactionCoordinatorClient = tcClient;
			_client = client;
		}

		public virtual ITransactionBuilder WithTransactionTimeout(TimeSpan timeout)
		{
			_txnTimeoutMs = timeout;
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
            var timeout = (long)_txnTimeoutMs.TotalMilliseconds;
            var ask = await _transactionCoordinatorClient.Ask<AskResponse>(new NewTxn(timeout)).ConfigureAwait(false);

            if (ask.Failed)
                throw ask.Exception;

            var txnID = ask.ConvertTo<TxnID>();

            if (_log.IsDebugEnabled)
                _log.Debug($"Success to new txn. txnID: ({txnID.MostSigBits}:{txnID.LeastSigBits})");

            var transaction = _actorSystem.ActorOf(TransactionActor.Prop(_client, timeout, txnID.LeastSigBits, txnID.MostSigBits));
            var tcOk = await transaction.Ask<TcClientOk>(GetTcClient.Instance).ConfigureAwait(false);
            return new User.Transaction(txnID.LeastSigBits, txnID.MostSigBits, transaction);	
		}
	}

}