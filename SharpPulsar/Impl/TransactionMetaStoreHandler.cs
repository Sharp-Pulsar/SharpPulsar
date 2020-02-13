using System;
using System.Collections.Concurrent;
using SharpPulsar.Protocol.Proto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using SharpPulsar.Exception;
using SharpPulsar.Protocol;
using SharpPulsar.Transaction;
using SharpPulsar.Util;

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
	public class TransactionMetaStoreHandler : HandlerState, IConnection, System.IDisposable, ITimerTask
	{

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(TransactionMetaStoreHandler));

		private readonly long _transactionCoordinatorId;
		private ConnectionHandler _connectionHandler;
		private readonly ConcurrentDictionary<long, OpBase<object>> _pendingRequests = new ConcurrentDictionary<long, OpBase<object>>();
		private readonly ConcurrentDictionary<long, ClientCnx.RequestTime> _timeoutQueue;

		private readonly bool _blockIfReachMaxPendingOps;
		private readonly Semaphore _semaphore;

		private ITimeout _requestTimeout;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, PulsarClientImpl pulsarClient, string topic) : base(pulsarClient, topic)
		{
			this._transactionCoordinatorId = transactionCoordinatorId;
			this._timeoutQueue = new ConcurrentDictionary<long, ClientCnx.RequestTime>();
			this._blockIfReachMaxPendingOps = true;
			this._semaphore = new Semaphore(0, 1000);
			this._requestTimeout = pulsarClient.Timer.NewTimeout(this, TimeSpan.FromMilliseconds(pulsarClient.Configuration.OperationTimeoutMs));
			this._connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.SetInitialTime(pulsarClient.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMax(pulsarClient.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).SetMandatoryStop(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create(), this);
			this._connectionHandler.GrabCnx();
		}

		public  void ConnectionFailed(PulsarClientException exception)
		{
			Log.LogError("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			State = State.Failed;
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
			_pendingRequests.TryAdd(requestId, op);
			_timeoutQueue.TryAdd(requestId, new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.Retain();
			Cnx().Ctx().WriteAndFlushAsync(cmd);
			return callback;
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
				op.Callback.complete(txnId);
			}
			else
			{
				Log.LogError("Got new txn for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
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

			if (!CanSendRequest<Task>(callback))
			{
				return callback;
			}
			long requestId = Client.NewRequestId();
			var cmd = Commands.NewAddPartitionToTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits);
			var op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().ctx().writeAndFlush(cmd, Cnx().ctx().voidPromise());
			return callback;
		}

		public virtual void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			var op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if (op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got add publish partition to txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.hasError())
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Add publish partition for request {} success.", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				LOG.error("Add publish partition for request {} error {}.", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public virtual CompletableFuture<Void> CommitAsync(TxnID txnId)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Commit txn {}", txnId);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if (!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = Client.newRequestId();
			ByteBuf cmd = Commands.newEndTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits, TxnAction.COMMIT);
			var op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().ctx().writeAndFlush(cmd, Cnx().ctx().voidPromise());
			return callback;
		}

		public virtual CompletableFuture<Void> AbortAsync(TxnID txnId)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Abort txn {}", txnId);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if (!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = Client.newRequestId();
			ByteBuf cmd = Commands.newEndTxn(requestId, txnId.LeastSigBits, txnId.MostSigBits, TxnAction.ABORT);
			var op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new ClientCnx.RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().ctx().writeAndFlush(cmd, Cnx().ctx().voidPromise());
			return callback;
		}

		public virtual void HandleEndTxnResponse(CommandEndTxnResponse response)
		{
			var op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if (op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got end txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.hasError())
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got end txn response success for request {}", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				LOG.error("Got end txn response for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public abstract class OpBase<T>
		{
			protected internal ByteBuf Cmd;
			protected internal CompletableFuture<T> Callback;

			public abstract void Recycle();
		}

		public class OpForTxnIdCallBack : OpBase<TxnID>
		{

			internal static OpForTxnIdCallBack Create(ByteBuf cmd, CompletableFuture<TxnID> callback)
			{
				OpForTxnIdCallBack op = Recycler.get();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}

			public OpForTxnIdCallBack(Recycler.Handle<OpForTxnIdCallBack> recyclerHandle)
			{
				this.RecyclerHandle = recyclerHandle;
			}

			public override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForTxnIdCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForTxnIdCallBack> Recycler = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<OpForTxnIdCallBack>
			{
				public override OpForTxnIdCallBack newObject(Handle<OpForTxnIdCallBack> handle)
				{
					return new OpForTxnIdCallBack(handle);
				}
			}
		}

		public class OpForVoidCallBack : OpBase<object>
		{

			internal static OpForVoidCallBack Create(IByteBuffer cmd, TaskCompletionSource<Task> callback)
			{
				OpForVoidCallBack op = Recycler.get();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}
			public OpForVoidCallBack(Recycler.Handle<OpForVoidCallBack> recyclerHandle)
			{
				this.RecyclerHandle = recyclerHandle;
			}

			public override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForVoidCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForVoidCallBack> Recycler = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<OpForVoidCallBack>
			{
				public override OpForVoidCallBack newObject(Handle<OpForVoidCallBack> handle)
				{
					return new OpForVoidCallBack(handle);
				}
			}
		}

		private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
		{
			switch (serverError.innerEnumValue)
			{
				case serverError.InnerEnum.TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
				case serverError.InnerEnum.InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
				default:
					return new TransactionCoordinatorClientException(msg);
			}
		}

		private void OnResponse<T1>(OpBase<T1> op)
		{
			ReferenceCountUtil.safeRelease(op.Cmd);
			op.recycle();
			_semaphore.release();
		}

		private bool CanSendRequest<T1>(CompletableFuture<T1> callback)
		{
			if (!IsValidHandlerState(callback))
			{
				return false;
			}
			try
			{
				if (_blockIfReachMaxPendingOps)
				{
					_semaphore.acquire();
				}
				else
				{
					if (!_semaphore.tryAcquire())
					{
						callback.completeExceptionally(new TransactionCoordinatorClientException("Reach max pending ops."));
						return false;
					}
				}
			}
			catch (InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				callback.completeExceptionally(TransactionCoordinatorClientException.unwrap(e));
				return false;
			}
			return true;
		}

		private bool IsValidHandlerState<T1>(CompletableFuture<T1> callback)
		{
			switch (State)
			{
				case Ready:
					return true;
				case Connecting:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " is connecting now, please try later."));
					return false;
				case Closing:
				case Closed:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " is closing or closed."));
					return false;
				case Failed:
				case Uninitialized:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " not connected."));
					return false;
				default:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException(_transactionCoordinatorId));
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
			if (State == State.Closing || State == State.Closed)
			{
				return;
			}
			ClientCnx.RequestTime peeked = _timeoutQueue.peek();
			while (peeked != null && peeked.CreationTimeMs + Client.Configuration.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				ClientCnx.RequestTime lastPolled = _timeoutQueue.poll();
				if (lastPolled != null)
				{
					OpBase<object> op = _pendingRequests.Remove(lastPolled.RequestId);
					if (!op.Callback.Done)
					{
						op.Callback.completeExceptionally(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout."));
						if (LOG.DebugEnabled)
						{
							LOG.debug("Transaction coordinator request {} is timeout.", lastPolled.RequestId);
						}
						OnResponse(op);
					}
				}
				else
				{
					break;
				}
				peeked = _timeoutQueue.peek();
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
			return this._connectionHandler.Cnx();
		}

		public virtual void ConnectionClosed(ClientCnx cnx)
		{
			this._connectionHandler.ConnectionClosed(cnx);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void Close()
		{
		}

		public override string HandlerName
		{
			get
			{
				return "Transaction meta store handler [" + _transactionCoordinatorId + "]";
			}
		}
	}

}