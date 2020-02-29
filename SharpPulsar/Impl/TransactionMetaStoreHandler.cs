using System;
using System.Collections.Concurrent;
using SharpPulsar.Protocol.Proto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Common;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Protocol;
using SharpPulsar.Transaction;
using SharpPulsar.Utility;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;
using TransactionCoordinatorClientException = SharpPulsar.Exceptions.TransactionCoordinatorClientException;

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
namespace SharpPulsar.Impl
{

	/// <summary>
	/// Handler for transaction meta store.
	/// </summary>
	public class TransactionMetaStoreHandler : HandlerState, IConnection, IDisposable, ITimerTask
	{

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(TransactionMetaStoreHandler));

		private readonly long _transactionCoordinatorId;
		private ConnectionHandler _connectionHandler;
		private readonly ConcurrentDictionary<long, OpBase<object>> _pendingRequests = new ConcurrentDictionary<long, OpBase<object>>();
		private readonly ConcurrentQueue<ClientCnx.RequestTime> _timeoutQueue;
        public State TranState;
		private readonly bool _blockIfReachMaxPendingOps;
		private readonly Semaphore _semaphore;

		private ITimeout _requestTimeout;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, PulsarClientImpl pulsarClient, string topic) : base(pulsarClient, topic)
		{
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<ClientCnx.RequestTime>();
			_blockIfReachMaxPendingOps = true;
			_semaphore = new Semaphore(0, 1000);
			_requestTimeout = pulsarClient.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(pulsarClient.Configuration.OperationTimeoutMs));
			_connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.SetInitialTime(pulsarClient.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMax(pulsarClient.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMandatoryStop(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create(), this);
			_connectionHandler.GrabCnx();
		}

		public  void ConnectionFailed(PulsarClientException exception)
		{
			Log.LogError("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			TranState = State.Failed;
		}

		public void ConnectionOpened(ClientCnx cnx)
		{
			Log.LogInformation("Transaction meta handler with transaction coordinator id {} connection opened.", _transactionCoordinatorId);
			_connectionHandler.ClientCnx = cnx;
			cnx.RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, this);
			if (!ChangeToReadyState())
			{
				cnx.Channel().CloseAsync();
			}
		}

		public virtual ValueTask<TxnID> NewTransactionAsync(long timeout, BAMCIS.Util.Concurrent.TimeUnit unit)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("New transaction with timeout in ms {}", unit.ToMillis(timeout));
			}
			var callback = new TaskCompletionSource<TxnID>();

			if (!CanSendRequest<TxnID>(callback))
			{
				return new ValueTask<TxnID>(callback.Task); 
			}
			long requestId = Client.NewRequestId();
			var cmd = Commands.NewTxn(_transactionCoordinatorId, requestId, unit.ToMillis(timeout));
			var op = OpForTxnIdCallBack.Create(cmd, callback);
			_pendingRequests.TryAdd(requestId, (OpBase<object>)Convert.ChangeType(op, typeof(OpBase<object>)));
			_timeoutQueue.Enqueue(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.Retain();
			Cnx().Ctx().WriteAndFlushAsync(cmd);
			return new ValueTask<TxnID>(callback.Task);
		}

		public virtual void HandleNewTxnResponse(CommandNewTxnResponse response)
		{
			if (_pendingRequests.Remove((long)response.RequestId, out var op))
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Got new txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.HasError)
			{
				TxnID txnId = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
                if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Got new txn response {} for request {}", txnId, response.RequestId);
				}
				op.Callback.SetResult(txnId);
			}
			else
			{
				Log.LogError("Got new txn for request {} error {}", response.RequestId, response.Error);
				op.Callback.SetException(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public virtual ValueTask AddPublishPartitionToTxnAsync(TxnID txnId, IList<string> partitions)
		{
            if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Add publish partition to txn request with txnId, with partitions", txnId, partitions);
			}
			var callback = new TaskCompletionSource<Task>();

			if (!CanSendRequest(callback))
			{
				return new ValueTask(callback.Task);
			}
			long requestId = Client.NewRequestId();
			var cmd = Commands.NewAddPartitionToTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits);
			var op = OpForVoidCallBack<Task>.Create(cmd, callback);
			_pendingRequests[requestId] = (OpBase<object>)Convert.ChangeType(op, typeof(OpBase<object>)); ;
			_timeoutQueue.Enqueue(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.Retain();
			Cnx().Ctx().WriteAndFlushAsync(cmd);
			return new ValueTask(callback.Task);
		}

		public virtual void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			if (_pendingRequests.TryRemove((long)response.RequestId, out var op))
			{
				if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Got add publish partition to txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.HasError)
			{
                if (Log.IsEnabled(LogLevel.Debug))
				{
                    Log.LogDebug("Add publish partition for request {} success.", response.RequestId);
				}
				op.Callback.SetResult(null);
			}
			else
			{
				Log.LogError("Add publish partition for request {} error {}.", response.RequestId, response.Error);
				op.Callback.SetException(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public virtual ValueTask CommitAsync(TxnID txnId)
		{
            if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Commit txn {}", txnId);
			}
			var callback = new TaskCompletionSource<TxnID>();

			if (!CanSendRequest(callback))
            {
                return new ValueTask(callback.Task);
            }
			long requestId = Client.NewRequestId();
			var cmd = Commands.NewEndTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits, TxnAction.Commit);
			var op = OpForVoidCallBack<TxnID>.Create(cmd, callback);
			_pendingRequests[requestId] = (OpBase<object>)Convert.ChangeType(op, typeof(OpBase<object>)); ;
			_timeoutQueue.Enqueue(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.Retain();
			Cnx().Ctx().WriteAndFlushAsync(cmd);
            return new ValueTask(callback.Task);
        }

		public virtual ValueTask AbortAsync(TxnID txnId)
		{
            if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("Abort txn {}", txnId);
			}
			var callback = new TaskCompletionSource<TxnID>();

			if (!CanSendRequest(callback))
			{
				return new ValueTask(callback.Task);
			}
			long requestId = Client.NewRequestId();
			var cmd = Commands.NewEndTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits, TxnAction.Abort);
			var op = OpForVoidCallBack<TxnID>.Create(cmd, callback);
			_pendingRequests[requestId] = (OpBase<object>)Convert.ChangeType(op, typeof(OpBase<object>));
			_timeoutQueue.Enqueue(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.Retain();
			Cnx().Ctx().WriteAndFlushAsync(cmd);
            return new ValueTask(callback.Task);
		}

		public virtual void HandleEndTxnResponse(CommandEndTxnResponse response)
		{
			if (_pendingRequests.TryRemove((long)response.RequestId, out var op))
			{
                if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Got end txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.HasError)
			{
                if (Log.IsEnabled(LogLevel.Debug))
				{
					Log.LogDebug("Got end txn response success for request {}", response.RequestId);
				}
				op.Callback.SetResult(null);
			}
			else
			{
				Log.LogError("Got end txn response for request {} error {}", response.RequestId, response.Error);
				op.Callback.SetException(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public abstract class OpBase
		{
			protected internal IByteBuffer Cmd;
			protected internal TaskCompletionSource Callback;

			public abstract void Recycle();
		}

		public class OpForTxnIdCallBack : OpBase<TxnID>
		{
            internal static ThreadLocalPool<OpForTxnIdCallBack> _pool = new ThreadLocalPool<OpForTxnIdCallBack>(handle => new OpForTxnIdCallBack(handle), 1, true);

            internal ThreadLocalPool.Handle _handle;
            private OpForTxnIdCallBack(ThreadLocalPool.Handle handle)
            {
                _handle = handle;
            }
			internal static OpForTxnIdCallBack Create(IByteBuffer cmd, TaskCompletionSource<TxnID> callback)
			{
				OpForTxnIdCallBack op = _pool.Take();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}

			
			public override void Recycle()
			{
				_handle.Release(this);
			}


		}

		public class OpForVoidCallBack : OpBase
		{
            internal static ThreadLocalPool<OpForVoidCallBack> _pool = new ThreadLocalPool<OpForVoidCallBack>(handle => new OpForVoidCallBack(handle), 1, true);

            internal ThreadLocalPool.Handle _handle;
            private OpForVoidCallBack(ThreadLocalPool.Handle handle)
            {
                _handle = handle;
            }
			internal static OpForVoidCallBack Create(IByteBuffer cmd, TaskCompletionSource callback)
			{
				var op = _pool.Take();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}
			
			public override void Recycle()
			{
				_handle.Release(this);
			}

		}

		private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
		{
			switch (serverError)
			{
				case ServerError.TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
				case ServerError.InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
				default:
					return new TransactionCoordinatorClientException(msg);
			}
		}

		private void OnResponse(OpBase op)
		{
			op.Cmd.SafeRelease();
			op.Recycle();
			_semaphore.Release();
		}

		private bool CanSendRequest(TaskCompletionSource callback)
		{
			if (!IsValidHandlerState(callback))
			{
				return false;
			}
			try
			{
				if (_blockIfReachMaxPendingOps)
				{
					_semaphore.WaitOne();
				}
				else
				{
					if (!_semaphore.WaitOne(2000))
					{
						callback.SetException(new TransactionCoordinatorClientException("Reach max pending ops."));
						return false;
					}
				}
			}
			catch (ThreadInterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				callback.SetException(TransactionCoordinatorClientException.Unwrap(e));
				return false;
			}
			return true;
		}

		private bool IsValidHandlerState(TaskCompletionSource callback)
		{
			switch (TranState)
			{
				case State.Ready:
					return true;
				case State.Connecting:
					callback.SetException(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " is connecting now, please try later."));
					return false;
				case State.Closing:
				case State.Closed:
					callback.SetException(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " is closing or closed."));
					return false;
				case State.Failed:
				case State.Uninitialized:
					callback.SetException(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " not connected."));
					return false;
				default:
					callback.SetException(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException(_transactionCoordinatorId));
					return false;
			}
		}

		public  void Run(ITimeout timeout)
		{
			if (timeout.Canceled)
			{
				return;
			}
			long timeToWaitMs;
			if (TranState == State.Closing || TranState == State.Closed)
			{
				return;
			}
			_timeoutQueue.TryPeek(out var peeked);
			while (peeked != null && peeked.CreationTimeMs + Client.Configuration.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				 _timeoutQueue.TryDequeue(out var lastPolled);
				if (lastPolled != null)
				{
					_pendingRequests.TryRemove(lastPolled.RequestId, out var op);
					if (!op.Callback.Task.IsCompleted)
					{
						op.Callback.SetException(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout."));
						if (Log.IsEnabled(LogLevel.Debug))
						{
							Log.LogDebug("Transaction coordinator request {} is timeout.", lastPolled.RequestId);
						}
						OnResponse(op);
					}
				}
				else
				{
					break;
				}
				 _timeoutQueue.TryPeek(out peeked);
			}

			if (peeked == null)
			{
				timeToWaitMs = Client.Configuration.OperationTimeoutMs;
			}
			else
			{
				var diff = (peeked.CreationTimeMs + Client.Configuration.OperationTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
				if (diff <= 0)
				{
					timeToWaitMs = Client.Configuration.OperationTimeoutMs;
				}
				else
				{
					timeToWaitMs = diff;
				}
			}
			_requestTimeout = Client.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(timeToWaitMs));
		}

		private ClientCnx Cnx()
		{
			return _connectionHandler.Cnx();
		}

		public virtual void ConnectionClosed(ClientCnx cnx)
		{
			_connectionHandler.ConnectionClosed(cnx);
		}

		public void Close()
		{
		}

		public new string HandlerName => "Transaction meta store handler [" + _transactionCoordinatorId + "]";
        public void Dispose()
        {
            Close();
        }
    }

}