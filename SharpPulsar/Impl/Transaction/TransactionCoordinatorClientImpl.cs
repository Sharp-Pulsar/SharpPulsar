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
	using IPulsarClient = Api.IPulsarClient;
	using TransactionCoordinatorClient = TransactionCoordinatorClient;
	using TransactionCoordinatorClientException = TransactionCoordinatorClientException;
	using CoordinatorClientStateException = TransactionCoordinatorClientException.CoordinatorClientStateException;
	using MathUtils = Util.MathUtils;
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
		private volatile TransactionCoordinatorClientState state = TransactionCoordinatorClientState.None;

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
			if (STATE_UPDATER.TryUpdate(this, TransactionCoordinatorClientState.Starting, TransactionCoordinatorClientState.None))
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
					STATE_UPDATER[this]  = TransactionCoordinatorClientState.Ready;
				});
				return new ValueTask(tsk);
			}
			else
			{
				return new ValueTask(Task.FromException(new CoordinatorClientStateException("Can not start while current state is " + state)));
			}
		}

		public  void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask CloseAsync()
		{
			TaskCompletionSource<Task> Result = new TaskCompletionSource<Task>();
			if (State == TransactionCoordinatorClientState.Closing || State == TransactionCoordinatorClientState.Closed)
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
				return NewTransactionAsync().Result;
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask<TxnID> NewTransactionAsync()
		{
			return NewTransactionAsync(TransactionCoordinatorClientFields.DefaultTxnTtlMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		public TxnID NewTransaction(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			try
			{
				return NewTransactionAsync(Timeout, Unit);
			}
			catch (System.Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask<TxnID> NewTransactionAsync(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			return NextHandler().NewTransactionAsync(Timeout, Unit);
		}

		public void AddPublishPartitionToTxn(TxnID TxnID, IList<string> Partitions)
		{
			try
			{
				AddPublishPartitionToTxnAsync(TxnID, Partitions);
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.Unwrap(E);
			}
		}

		public ValueTask AddPublishPartitionToTxnAsync(TxnID TxnID, IList<string> Partitions)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.AddPublishPartitionToTxnAsync(TxnID, Partitions);
		}

		public void Commit(TxnID TxnID)
		{
			try
			{
				CommitAsync(TxnID).Result;
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public ValueTask CommitAsync(TxnID TxnID)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.CommitAsync(TxnID);
		}

		public void Abort(TxnID TxnID)
		{
			try
			{
				AbortAsync(TxnID).Result;
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public ValueTask<Void> AbortAsync(TxnID TxnID)
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