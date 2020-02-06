using SharpPulsar.Api.Transaction;

using System;
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
namespace SharpPulsar.Impl.Transaction
{
	using IPulsarClient = SharpPulsar.Api.IPulsarClient;
	using TransactionCoordinatorClient = SharpPulsar.Api.Transaction.TransactionCoordinatorClient;
	using TransactionCoordinatorClientException = SharpPulsar.Api.Transaction.TransactionCoordinatorClientException;
	using CoordinatorClientStateException = SharpPulsar.Api.Transaction.TransactionCoordinatorClientException.CoordinatorClientStateException;
	using MathUtils = SharpPulsar.Util.MathUtils;
    using Microsoft.Extensions.Logging;
    using SharpPulsar.Util.Collections;
    using SharpPulsar.Util.Atomic;
    using System.Threading.Tasks;
    using SharpPulsar.Common.Naming;
    using System.Collections.Concurrent;
    using SharpPulsar.Transaction;


    /// <summary>
    /// Transaction coordinator client based topic assigned.
    /// </summary>
    public class TransactionCoordinatorClientImpl : TransactionCoordinatorClient
	{

		private static readonly ILogger log = new LoggerFactory().CreateLogger(typeof(TransactionCoordinatorClientImpl));

		private readonly PulsarClientImpl pulsarClient;
		private TransactionMetaStoreHandler[] handlers;
		private ConcurrentLongHashMap<TransactionMetaStoreHandler> handlerMap = new ConcurrentLongHashMap<TransactionMetaStoreHandler>(16, 1);
		private readonly AtomicLong epoch = new AtomicLong(0);

		private static readonly ConcurrentDictionary<TransactionCoordinatorClientImpl, TransactionCoordinatorClientState> STATE_UPDATER = new ConcurrentDictionary<TransactionCoordinatorClientImpl, TransactionCoordinatorClientState>();
		private volatile TransactionCoordinatorClientState state = TransactionCoordinatorClientState.NONE;

		public TransactionCoordinatorClientImpl(IPulsarClient PulsarClient)
		{
			this.pulsarClient = (PulsarClientImpl) PulsarClient;
		}

		public void Start()
		{
			try
			{
				StartAsync();
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask StartAsync()
		{
			if (STATE_UPDATER.TryUpdate(this, TransactionCoordinatorClientState.STARTING, TransactionCoordinatorClientState.NONE))
			{
				var tsk = pulsarClient.Lookup.GetPartitionedTopicMetadata(TopicName.TRANSACTION_COORDINATOR_ASSIGN).AsTask().ContinueWith(partitionMeta =>
				{
					if (log.IsEnabled(LogLevel.Debug))
					{
						log.LogDebug("Transaction meta store assign partition is {}.", partitionMeta.Result.partitions);
					}
					if (partitionMeta.Result.partitions > 0)
					{
						handlers = new TransactionMetaStoreHandler[partitionMeta.Result.partitions];
						for (int I = 0; I < partitionMeta.Result.partitions; I++)
						{
							TransactionMetaStoreHandler Handler = new TransactionMetaStoreHandler(I, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString() + TopicName.PARTITIONED_TOPIC_SUFFIX + I);
							handlers[I] = Handler;
							handlerMap.put(I, Handler);
						}
					}
					else
					{
						handlers = new TransactionMetaStoreHandler[1];
						TransactionMetaStoreHandler Handler = new TransactionMetaStoreHandler(0, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString());
						handlers[0] = Handler;
						handlerMap.put(0, Handler);
					}
					STATE_UPDATER[this]  = TransactionCoordinatorClientState.READY;
				});
				return new ValueTask(tsk);
			}
			else
			{
				return new ValueTask(Task.FromException(new TransactionCoordinatorClientException.CoordinatorClientStateException("Can not start while current state is " + state)));
			}
		}

		public  void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask CloseAsync()
		{
			TaskCompletionSource<Task> Result = new TaskCompletionSource<Task>();
			if (State == TransactionCoordinatorClientState.CLOSING || State == TransactionCoordinatorClientState.CLOSED)
			{
				log.LogWarning("The transaction meta store is closing or closed, doing nothing.");
				Result.SetResult(null);
			}
			else
			{
				foreach (TransactionMetaStoreHandler Handler in handlers)
				{
					try
					{
						Handler.Close();
					}
					catch (System.Exception E)
					{
						log.LogWarning("Close transaction meta store handler error", E);
					}
				}
				this.handlers = null;
				Result.SetResult(null);
			}
			return new ValueTask(Result.Task);
		}

		public TxnID NewTransaction()
		{
			try
			{
				return NewTransactionAsync().get();
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public override CompletableFuture<TxnID> NewTransactionAsync()
		{
			return NewTransactionAsync(TransactionCoordinatorClientFields.DefaultTxnTtlMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		public override TxnID NewTransaction(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			try
			{
				return NewTransactionAsync(Timeout, Unit).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<TxnID> NewTransactionAsync(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			return NextHandler().newTransactionAsync(Timeout, Unit);
		}

		public override void AddPublishPartitionToTxn(TxnID TxnID, IList<string> Partitions)
		{
			try
			{
				AddPublishPartitionToTxnAsync(TxnID, Partitions).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> AddPublishPartitionToTxnAsync(TxnID TxnID, IList<string> Partitions)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.addPublishPartitionToTxnAsync(TxnID, Partitions);
		}

		public override void Commit(TxnID TxnID)
		{
			try
			{
				CommitAsync(TxnID).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> CommitAsync(TxnID TxnID)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.commitAsync(TxnID);
		}

		public override void Abort(TxnID TxnID)
		{
			try
			{
				AbortAsync(TxnID).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> AbortAsync(TxnID TxnID)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.abortAsync(TxnID);
		}

		public virtual TransactionCoordinatorClientState? State
		{
			get
			{
				return state;
			}
		}

		private TransactionMetaStoreHandler NextHandler()
		{
			int Index = MathUtils.signSafeMod(epoch.incrementAndGet(), handlers.Length);
			return handlers[Index];
		}
	}

}