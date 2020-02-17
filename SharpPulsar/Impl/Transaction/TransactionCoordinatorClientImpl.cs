using SharpPulsar.Api.Transaction;

using System;
using System.Collections.Generic;
using SharpPulsar.Utility;
using SharpPulsar.Utility.Atomic;

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
	using MathUtils = MathUtils;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using SharpPulsar.Common.Naming;
    using System.Collections.Concurrent;
    using SharpPulsar.Transaction;


    /// <summary>
    /// Transaction coordinator client based topic assigned.
    /// </summary>
    public class TransactionCoordinatorClientImpl : TransactionCoordinatorClient
	{

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(TransactionCoordinatorClientImpl));

		private readonly PulsarClientImpl _pulsarClient;
		private TransactionMetaStoreHandler[] _handlers;
		private ConcurrentDictionary<long, TransactionMetaStoreHandler> _handlerMap = new ConcurrentDictionary<long, TransactionMetaStoreHandler>(1, 16);
		private readonly AtomicLong _epoch = new AtomicLong(0);

		private static readonly ConcurrentDictionary<TransactionCoordinatorClientImpl, TransactionCoordinatorClientState> StateUpdater = new ConcurrentDictionary<TransactionCoordinatorClientImpl, TransactionCoordinatorClientState>();
		private volatile TransactionCoordinatorClientState _state = TransactionCoordinatorClientState.None;

		public TransactionCoordinatorClientImpl(IPulsarClient pulsarClient)
		{
			this._pulsarClient = (PulsarClientImpl) pulsarClient;
		}

		public void Start()
		{
			try
			{
				StartAsync();
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask StartAsync()
		{
			if (StateUpdater.TryUpdate(this, TransactionCoordinatorClientState.Starting, TransactionCoordinatorClientState.None))
			{
				var tsk = _pulsarClient.Lookup.GetPartitionedTopicMetadata(TopicName.TRANSACTION_COORDINATOR_ASSIGN).AsTask().ContinueWith(partitionMeta =>
				{
					if (Log.IsEnabled(LogLevel.Debug))
					{
						Log.LogDebug("Transaction meta store assign partition is {}.", partitionMeta.Result.partitions);
					}
					if (partitionMeta.Result.partitions > 0)
					{
						_handlers = new TransactionMetaStoreHandler[partitionMeta.Result.partitions];
						for (var i = 0; i < partitionMeta.Result.partitions; i++)
						{
							var handler = new TransactionMetaStoreHandler(i, _pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString() + TopicName.PARTITIONED_TOPIC_SUFFIX + i);
							_handlers[i] = handler;
							_handlerMap.TryAdd(i, handler);
						}
					}
					else
					{
						_handlers = new TransactionMetaStoreHandler[1];
						var handler = new TransactionMetaStoreHandler(0, _pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString());
						_handlers[0] = handler;
						_handlerMap.TryAdd(0, handler);
					}
					StateUpdater[this]  = TransactionCoordinatorClientState.Ready;
				});
				return new ValueTask(tsk);
			}
			else
			{
				return new ValueTask(Task.FromException(new CoordinatorClientStateException("Can not start while current state is " + _state)));
			}
		}

		public  void Close()
		{
			try
			{
				CloseAsync();
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask CloseAsync()
		{
			var result = new TaskCompletionSource<Task>();
			if (State == TransactionCoordinatorClientState.Closing || State == TransactionCoordinatorClientState.Closed)
			{
				Log.LogWarning("The transaction meta store is closing or closed, doing nothing.");
				result.SetResult(null);
			}
			else
			{
				foreach (var handler in _handlers)
				{
					try
					{
						handler.Close();
					}
					catch (System.Exception e)
					{
						Log.LogWarning("Close transaction meta store handler error", e);
					}
				}
				this._handlers = null;
				result.SetResult(null);
			}
			return new ValueTask(result.Task);
		}

		public TxnID NewTransaction()
		{
			try
			{
				return NewTransactionAsync().Result;
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask<TxnID> NewTransactionAsync()
		{
			return NewTransactionAsync(TransactionCoordinatorClientFields.DefaultTxnTtlMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		public TxnID NewTransaction(long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			try
			{
				return NewTransactionAsync(timeout, unit).Result;
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask<TxnID> NewTransactionAsync(long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			return NextHandler().NewTransactionAsync(timeout, unit);
		}

		public void AddPublishPartitionToTxn(TxnID txnId, IList<string> partitions)
		{
			try
			{
				AddPublishPartitionToTxnAsync(txnId, partitions);
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask AddPublishPartitionToTxnAsync(TxnID txnId, IList<string> partitions)
		{
			var handler = _handlerMap[txnId.MostSigBits];
			if (handler == null)
			{
				return new ValueTask(Task.FromException(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnId.MostSigBits)));
			}
			return handler.AddPublishPartitionToTxnAsync(txnId, partitions);
		}

		public void Commit(TxnID txnId)
		{
			try
			{
				CommitAsync(txnId);
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask CommitAsync(TxnID txnId)
		{
			var handler = _handlerMap[txnId.MostSigBits];
			if (handler == null)
			{
				return new ValueTask(Task.FromException(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnId.MostSigBits)));
			}
			return handler.CommitAsync(txnId);
		}

		public void Abort(TxnID txnId)
		{
			try
			{
				AbortAsync(txnId);
			}
			catch (System.Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public ValueTask AbortAsync(TxnID txnId)
		{
			var handler = _handlerMap[txnId.MostSigBits];
			if (handler == null)
			{
				return new ValueTask(Task.FromException(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnId.MostSigBits)));
			}
			return handler.AbortAsync(txnId);
		}

		public virtual TransactionCoordinatorClientState? State
		{
			get
			{
				return _state;
			}
		}

		private TransactionMetaStoreHandler NextHandler()
		{
			var index = MathUtils.SignSafeMod(_epoch.Increment(), _handlers.Length);
			return _handlers[index];
		}

        public void Dispose()
        {
            Close();
        }
    }

}