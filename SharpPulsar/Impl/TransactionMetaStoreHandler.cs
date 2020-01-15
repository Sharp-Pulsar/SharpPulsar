using System.Collections.Generic;
using System.Threading;

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
namespace org.apache.pulsar.client.impl
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Recycler = io.netty.util.Recycler;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using TransactionCoordinatorClientException = org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException;

	using RequestTime = org.apache.pulsar.client.impl.ClientCnx.RequestTime;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using ConcurrentLongHashMap = org.apache.pulsar.common.util.collections.ConcurrentLongHashMap;
	using TxnID = org.apache.pulsar.transaction.impl.common.TxnID;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// Handler for transaction meta store.
	/// </summary>
	public class TransactionMetaStoreHandler : HandlerState, ConnectionHandler.Connection, System.IDisposable, TimerTask
	{

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(TransactionMetaStoreHandler));

		private readonly long transactionCoordinatorId;
		private ConnectionHandler connectionHandler;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<OpBase<?>> pendingRequests = new org.apache.pulsar.common.util.collections.ConcurrentLongHashMap<>(16, 1);
		private readonly ConcurrentLongHashMap<OpBase<object>> pendingRequests = new ConcurrentLongHashMap<OpBase<object>>(16, 1);
		private readonly ConcurrentLinkedQueue<RequestTime> timeoutQueue;

		private readonly bool blockIfReachMaxPendingOps;
		private readonly Semaphore semaphore;

		private Timeout requestTimeout;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, PulsarClientImpl pulsarClient, string topic) : base(pulsarClient, topic)
		{
			this.transactionCoordinatorId = transactionCoordinatorId;
			this.timeoutQueue = new ConcurrentLinkedQueue<RequestTime>();
			this.blockIfReachMaxPendingOps = true;
			this.semaphore = new Semaphore(1000);
			this.requestTimeout = pulsarClient.timer().newTimeout(this, pulsarClient.Configuration.OperationTimeoutMs, TimeUnit.MILLISECONDS);
			this.connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.setInitialTime(pulsarClient.Configuration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMax(pulsarClient.Configuration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMandatoryStop(100, TimeUnit.MILLISECONDS).create(), this);
			this.connectionHandler.grabCnx();
		}

		public virtual void connectionFailed(PulsarClientException exception)
		{
			LOG.error("Transaction meta handler with transaction coordinator id {} connection failed.", transactionCoordinatorId, exception);
			State = State.Failed;
		}

		public virtual void connectionOpened(ClientCnx cnx)
		{
			LOG.info("Transaction meta handler with transaction coordinator id {} connection opened.", transactionCoordinatorId);
			connectionHandler.ClientCnx = cnx;
			cnx.registerTransactionMetaStoreHandler(transactionCoordinatorId, this);
			if (!changeToReadyState())
			{
				cnx.channel().close();
			}
		}

		public virtual CompletableFuture<TxnID> newTransactionAsync(long timeout, TimeUnit unit)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("New transaction with timeout in ms {}", unit.toMillis(timeout));
			}
			CompletableFuture<TxnID> callback = new CompletableFuture<TxnID>();

			if (!canSendRequest(callback))
			{
				return callback;
			}
			long requestId = client.newRequestId();
			ByteBuf cmd = Commands.newTxn(transactionCoordinatorId, requestId, unit.toMillis(timeout));
			OpForTxnIdCallBack op = OpForTxnIdCallBack.create(cmd, callback);
			pendingRequests.put(requestId, op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			cnx().ctx().writeAndFlush(cmd, cnx().ctx().voidPromise());
			return callback;
		}

		internal virtual void handleNewTxnResponse(PulsarApi.CommandNewTxnResponse response)
		{
			OpForTxnIdCallBack op = (OpForTxnIdCallBack) pendingRequests.remove(response.RequestId);
			if (op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got new txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if (!response.hasError())
			{
				TxnID txnID = new TxnID(response.TxnidMostBits, response.TxnidLeastBits);
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got new txn response {} for request {}", txnID, response.RequestId);
				}
				op.callback.complete(txnID);
			}
			else
			{
				LOG.error("Got new txn for request {} error {}", response.RequestId, response.Error);
				op.callback.completeExceptionally(getExceptionByServerError(response.Error, response.Message));
			}

			onResponse(op);
		}

		public virtual CompletableFuture<Void> addPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Add publish partition to txn request with txnId, with partitions", txnID, partitions);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if (!canSendRequest(callback))
			{
				return callback;
			}
			long requestId = client.newRequestId();
			ByteBuf cmd = Commands.newAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits);
			OpForVoidCallBack op = OpForVoidCallBack.create(cmd, callback);
			pendingRequests.put(requestId, op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			cnx().ctx().writeAndFlush(cmd, cnx().ctx().voidPromise());
			return callback;
		}

		internal virtual void handleAddPublishPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) pendingRequests.remove(response.RequestId);
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
				op.callback.complete(null);
			}
			else
			{
				LOG.error("Add publish partition for request {} error {}.", response.RequestId, response.Error);
				op.callback.completeExceptionally(getExceptionByServerError(response.Error, response.Message));
			}

			onResponse(op);
		}

		public virtual CompletableFuture<Void> commitAsync(TxnID txnID)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Commit txn {}", txnID);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if (!canSendRequest(callback))
			{
				return callback;
			}
			long requestId = client.newRequestId();
			ByteBuf cmd = Commands.newEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, PulsarApi.TxnAction.COMMIT);
			OpForVoidCallBack op = OpForVoidCallBack.create(cmd, callback);
			pendingRequests.put(requestId, op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			cnx().ctx().writeAndFlush(cmd, cnx().ctx().voidPromise());
			return callback;
		}

		public virtual CompletableFuture<Void> abortAsync(TxnID txnID)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Abort txn {}", txnID);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if (!canSendRequest(callback))
			{
				return callback;
			}
			long requestId = client.newRequestId();
			ByteBuf cmd = Commands.newEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, PulsarApi.TxnAction.ABORT);
			OpForVoidCallBack op = OpForVoidCallBack.create(cmd, callback);
			pendingRequests.put(requestId, op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			cnx().ctx().writeAndFlush(cmd, cnx().ctx().voidPromise());
			return callback;
		}

		internal virtual void handleEndTxnResponse(PulsarApi.CommandEndTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) pendingRequests.remove(response.RequestId);
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
				op.callback.complete(null);
			}
			else
			{
				LOG.error("Got end txn response for request {} error {}", response.RequestId, response.Error);
				op.callback.completeExceptionally(getExceptionByServerError(response.Error, response.Message));
			}

			onResponse(op);
		}

		private abstract class OpBase<T>
		{
			protected internal ByteBuf cmd;
			protected internal CompletableFuture<T> callback;

			internal abstract void recycle();
		}

		private class OpForTxnIdCallBack : OpBase<TxnID>
		{

			internal static OpForTxnIdCallBack create(ByteBuf cmd, CompletableFuture<TxnID> callback)
			{
				OpForTxnIdCallBack op = RECYCLER.get();
				op.callback = callback;
				op.cmd = cmd;
				return op;
			}

			internal OpForTxnIdCallBack(Recycler.Handle<OpForTxnIdCallBack> recyclerHandle)
			{
				this.recyclerHandle = recyclerHandle;
			}

			internal override void recycle()
			{
				recyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForTxnIdCallBack> recyclerHandle;
			internal static readonly Recycler<OpForTxnIdCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpForTxnIdCallBack>
			{
				protected internal override OpForTxnIdCallBack newObject(Handle<OpForTxnIdCallBack> handle)
				{
					return new OpForTxnIdCallBack(handle);
				}
			}
		}

		private class OpForVoidCallBack : OpBase<Void>
		{

			internal static OpForVoidCallBack create(ByteBuf cmd, CompletableFuture<Void> callback)
			{
				OpForVoidCallBack op = RECYCLER.get();
				op.callback = callback;
				op.cmd = cmd;
				return op;
			}
			internal OpForVoidCallBack(Recycler.Handle<OpForVoidCallBack> recyclerHandle)
			{
				this.recyclerHandle = recyclerHandle;
			}

			internal override void recycle()
			{
				recyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForVoidCallBack> recyclerHandle;
			internal static readonly Recycler<OpForVoidCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpForVoidCallBack>
			{
				protected internal override OpForVoidCallBack newObject(Handle<OpForVoidCallBack> handle)
				{
					return new OpForVoidCallBack(handle);
				}
			}
		}

		private TransactionCoordinatorClientException getExceptionByServerError(PulsarApi.ServerError serverError, string msg)
		{
			switch (serverError)
			{
				case TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
				case InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
				default:
					return new TransactionCoordinatorClientException(msg);
			}
		}

		private void onResponse<T1>(OpBase<T1> op)
		{
			ReferenceCountUtil.safeRelease(op.cmd);
			op.recycle();
			semaphore.release();
		}

		private bool canSendRequest<T1>(CompletableFuture<T1> callback)
		{
			if (!isValidHandlerState(callback))
			{
				return false;
			}
			try
			{
				if (blockIfReachMaxPendingOps)
				{
					semaphore.acquire();
				}
				else
				{
					if (!semaphore.tryAcquire())
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

		private bool isValidHandlerState<T1>(CompletableFuture<T1> callback)
		{
			switch (State)
			{
				case Ready:
					return true;
				case Connecting:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " is connecting now, please try later."));
					return false;
				case Closing:
				case Closed:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " is closing or closed."));
					return false;
				case Failed:
				case Uninitialized:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " not connected."));
					return false;
				default:
					callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException(transactionCoordinatorId));
					return false;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void run(Timeout timeout)
		{
			if (timeout.Cancelled)
			{
				return;
			}
			long timeToWaitMs;
			if (State == State.Closing || State == State.Closed)
			{
				return;
			}
			RequestTime peeked = timeoutQueue.peek();
			while (peeked != null && peeked.creationTimeMs + client.Configuration.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				RequestTime lastPolled = timeoutQueue.poll();
				if (lastPolled != null)
				{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: OpBase<?> op = pendingRequests.remove(lastPolled.requestId);
					OpBase<object> op = pendingRequests.remove(lastPolled.requestId);
					if (!op.callback.Done)
					{
						op.callback.completeExceptionally(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout."));
						if (LOG.DebugEnabled)
						{
							LOG.debug("Transaction coordinator request {} is timeout.", lastPolled.requestId);
						}
						onResponse(op);
					}
				}
				else
				{
					break;
				}
				peeked = timeoutQueue.peek();
			}

			if (peeked == null)
			{
				timeToWaitMs = client.Configuration.OperationTimeoutMs;
			}
			else
			{
				long diff = (peeked.creationTimeMs + client.Configuration.OperationTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
				if (diff <= 0)
				{
					timeToWaitMs = client.Configuration.OperationTimeoutMs;
				}
				else
				{
					timeToWaitMs = diff;
				}
			}
			requestTimeout = client.timer().newTimeout(this, timeToWaitMs, TimeUnit.MILLISECONDS);
		}

		private ClientCnx cnx()
		{
			return this.connectionHandler.cnx();
		}

		internal virtual void connectionClosed(ClientCnx cnx)
		{
			this.connectionHandler.connectionClosed(cnx);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void Dispose()
		{
		}

		public override string HandlerName
		{
			get
			{
				return "Transaction meta store handler [" + transactionCoordinatorId + "]";
			}
		}
	}

}