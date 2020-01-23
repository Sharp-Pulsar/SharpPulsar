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
namespace SharpPulsar.Impl
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Recycler = io.netty.util.Recycler;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;
	using Timeout = io.netty.util.Timeout;
	using TimerTask = io.netty.util.TimerTask;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using TransactionCoordinatorClientException = SharpPulsar.Api.Transaction.TransactionCoordinatorClientException;

	using RequestTime = SharpPulsar.Impl.ClientCnx.RequestTime;
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using Org.Apache.Pulsar.Common.Util.Collections;
	using TxnID = Org.Apache.Pulsar.Transaction.Impl.Common.TxnID;
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

		public TransactionMetaStoreHandler(long TransactionCoordinatorId, PulsarClientImpl PulsarClient, string Topic) : base(PulsarClient, Topic)
		{
			this.transactionCoordinatorId = TransactionCoordinatorId;
			this.timeoutQueue = new ConcurrentLinkedQueue<RequestTime>();
			this.blockIfReachMaxPendingOps = true;
			this.semaphore = new Semaphore(1000);
			this.requestTimeout = PulsarClient.timer().newTimeout(this, PulsarClient.Configuration.OperationTimeoutMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			this.connectionHandler = new ConnectionHandler(this, new BackoffBuilder()
					.setInitialTime(PulsarClient.Configuration.InitialBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMax(PulsarClient.Configuration.MaxBackoffIntervalNanos, BAMCIS.Util.Concurrent.TimeUnit.NANOSECONDS).setMandatoryStop(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create(), this);
			this.connectionHandler.GrabCnx();
		}

		public override void ConnectionFailed(PulsarClientException Exception)
		{
			LOG.error("Transaction meta handler with transaction coordinator id {} connection failed.", transactionCoordinatorId, Exception);
			State = State.Failed;
		}

		public override void ConnectionOpened(ClientCnx Cnx)
		{
			LOG.info("Transaction meta handler with transaction coordinator id {} connection opened.", transactionCoordinatorId);
			connectionHandler.ClientCnx = Cnx;
			Cnx.registerTransactionMetaStoreHandler(transactionCoordinatorId, this);
			if (!ChangeToReadyState())
			{
				Cnx.channel().close();
			}
		}

		public virtual CompletableFuture<TxnID> NewTransactionAsync(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("New transaction with timeout in ms {}", Unit.toMillis(Timeout));
			}
			CompletableFuture<TxnID> Callback = new CompletableFuture<TxnID>();

			if (!CanSendRequest(Callback))
			{
				return Callback;
			}
			long RequestId = ClientConflict.newRequestId();
			ByteBuf Cmd = Commands.newTxn(transactionCoordinatorId, RequestId, Unit.toMillis(Timeout));
			OpForTxnIdCallBack Op = OpForTxnIdCallBack.Create(Cmd, Callback);
			pendingRequests.Put(RequestId, Op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), RequestId));
			Cmd.retain();
			Cnx().ctx().writeAndFlush(Cmd, Cnx().ctx().voidPromise());
			return Callback;
		}

		public virtual void HandleNewTxnResponse(PulsarApi.CommandNewTxnResponse Response)
		{
			OpForTxnIdCallBack Op = (OpForTxnIdCallBack) pendingRequests.Remove(Response.RequestId);
			if (Op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got new txn response for timeout {} - {}", Response.TxnidMostBits, Response.TxnidLeastBits);
				}
				return;
			}
			if (!Response.hasError())
			{
				TxnID TxnID = new TxnID(Response.TxnidMostBits, Response.TxnidLeastBits);
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got new txn response {} for request {}", TxnID, Response.RequestId);
				}
				Op.Callback.complete(TxnID);
			}
			else
			{
				LOG.error("Got new txn for request {} error {}", Response.RequestId, Response.Error);
				Op.Callback.completeExceptionally(GetExceptionByServerError(Response.Error, Response.Message));
			}

			OnResponse(Op);
		}

		public virtual CompletableFuture<Void> AddPublishPartitionToTxnAsync(TxnID TxnID, IList<string> Partitions)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Add publish partition to txn request with txnId, with partitions", TxnID, Partitions);
			}
			CompletableFuture<Void> Callback = new CompletableFuture<Void>();

			if (!CanSendRequest(Callback))
			{
				return Callback;
			}
			long RequestId = ClientConflict.newRequestId();
			ByteBuf Cmd = Commands.newAddPartitionToTxn(RequestId, TxnID.LeastSigBits, TxnID.MostSigBits);
			OpForVoidCallBack Op = OpForVoidCallBack.Create(Cmd, Callback);
			pendingRequests.Put(RequestId, Op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), RequestId));
			Cmd.retain();
			Cnx().ctx().writeAndFlush(Cmd, Cnx().ctx().voidPromise());
			return Callback;
		}

		public virtual void HandleAddPublishPartitionToTxnResponse(PulsarApi.CommandAddPartitionToTxnResponse Response)
		{
			OpForVoidCallBack Op = (OpForVoidCallBack) pendingRequests.Remove(Response.RequestId);
			if (Op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got add publish partition to txn response for timeout {} - {}", Response.TxnidMostBits, Response.TxnidLeastBits);
				}
				return;
			}
			if (!Response.hasError())
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Add publish partition for request {} success.", Response.RequestId);
				}
				Op.Callback.complete(null);
			}
			else
			{
				LOG.error("Add publish partition for request {} error {}.", Response.RequestId, Response.Error);
				Op.Callback.completeExceptionally(GetExceptionByServerError(Response.Error, Response.Message));
			}

			OnResponse(Op);
		}

		public virtual CompletableFuture<Void> CommitAsync(TxnID TxnID)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Commit txn {}", TxnID);
			}
			CompletableFuture<Void> Callback = new CompletableFuture<Void>();

			if (!CanSendRequest(Callback))
			{
				return Callback;
			}
			long RequestId = ClientConflict.newRequestId();
			ByteBuf Cmd = Commands.newEndTxn(RequestId, TxnID.LeastSigBits, TxnID.MostSigBits, PulsarApi.TxnAction.COMMIT);
			OpForVoidCallBack Op = OpForVoidCallBack.Create(Cmd, Callback);
			pendingRequests.Put(RequestId, Op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), RequestId));
			Cmd.retain();
			Cnx().ctx().writeAndFlush(Cmd, Cnx().ctx().voidPromise());
			return Callback;
		}

		public virtual CompletableFuture<Void> AbortAsync(TxnID TxnID)
		{
			if (LOG.DebugEnabled)
			{
				LOG.debug("Abort txn {}", TxnID);
			}
			CompletableFuture<Void> Callback = new CompletableFuture<Void>();

			if (!CanSendRequest(Callback))
			{
				return Callback;
			}
			long RequestId = ClientConflict.newRequestId();
			ByteBuf Cmd = Commands.newEndTxn(RequestId, TxnID.LeastSigBits, TxnID.MostSigBits, PulsarApi.TxnAction.ABORT);
			OpForVoidCallBack Op = OpForVoidCallBack.Create(Cmd, Callback);
			pendingRequests.Put(RequestId, Op);
			timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), RequestId));
			Cmd.retain();
			Cnx().ctx().writeAndFlush(Cmd, Cnx().ctx().voidPromise());
			return Callback;
		}

		public virtual void HandleEndTxnResponse(PulsarApi.CommandEndTxnResponse Response)
		{
			OpForVoidCallBack Op = (OpForVoidCallBack) pendingRequests.Remove(Response.RequestId);
			if (Op == null)
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got end txn response for timeout {} - {}", Response.TxnidMostBits, Response.TxnidLeastBits);
				}
				return;
			}
			if (!Response.hasError())
			{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Got end txn response success for request {}", Response.RequestId);
				}
				Op.Callback.complete(null);
			}
			else
			{
				LOG.error("Got end txn response for request {} error {}", Response.RequestId, Response.Error);
				Op.Callback.completeExceptionally(GetExceptionByServerError(Response.Error, Response.Message));
			}

			OnResponse(Op);
		}

		public abstract class OpBase<T>
		{
			protected internal ByteBuf Cmd;
			protected internal CompletableFuture<T> Callback;

			public abstract void Recycle();
		}

		public class OpForTxnIdCallBack : OpBase<TxnID>
		{

			internal static OpForTxnIdCallBack Create(ByteBuf Cmd, CompletableFuture<TxnID> Callback)
			{
				OpForTxnIdCallBack Op = RECYCLER.get();
				Op.Callback = Callback;
				Op.Cmd = Cmd;
				return Op;
			}

			public OpForTxnIdCallBack(Recycler.Handle<OpForTxnIdCallBack> RecyclerHandle)
			{
				this.RecyclerHandle = RecyclerHandle;
			}

			public override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForTxnIdCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForTxnIdCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<OpForTxnIdCallBack>
			{
				public override OpForTxnIdCallBack newObject(Handle<OpForTxnIdCallBack> Handle)
				{
					return new OpForTxnIdCallBack(Handle);
				}
			}
		}

		public class OpForVoidCallBack : OpBase<Void>
		{

			internal static OpForVoidCallBack Create(ByteBuf Cmd, CompletableFuture<Void> Callback)
			{
				OpForVoidCallBack Op = RECYCLER.get();
				Op.Callback = Callback;
				Op.Cmd = Cmd;
				return Op;
			}
			public OpForVoidCallBack(Recycler.Handle<OpForVoidCallBack> RecyclerHandle)
			{
				this.RecyclerHandle = RecyclerHandle;
			}

			public override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForVoidCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForVoidCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			public class RecyclerAnonymousInnerClass : Recycler<OpForVoidCallBack>
			{
				public override OpForVoidCallBack newObject(Handle<OpForVoidCallBack> Handle)
				{
					return new OpForVoidCallBack(Handle);
				}
			}
		}

		private TransactionCoordinatorClientException GetExceptionByServerError(PulsarApi.ServerError ServerError, string Msg)
		{
			switch (ServerError.innerEnumValue)
			{
				case PulsarApi.ServerError.InnerEnum.TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(Msg);
				case PulsarApi.ServerError.InnerEnum.InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(Msg);
				default:
					return new TransactionCoordinatorClientException(Msg);
			}
		}

		private void OnResponse<T1>(OpBase<T1> Op)
		{
			ReferenceCountUtil.safeRelease(Op.Cmd);
			Op.recycle();
			semaphore.release();
		}

		private bool CanSendRequest<T1>(CompletableFuture<T1> Callback)
		{
			if (!IsValidHandlerState(Callback))
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
						Callback.completeExceptionally(new TransactionCoordinatorClientException("Reach max pending ops."));
						return false;
					}
				}
			}
			catch (InterruptedException E)
			{
				Thread.CurrentThread.Interrupt();
				Callback.completeExceptionally(TransactionCoordinatorClientException.unwrap(E));
				return false;
			}
			return true;
		}

		private bool IsValidHandlerState<T1>(CompletableFuture<T1> Callback)
		{
			switch (State)
			{
				case Ready:
					return true;
				case Connecting:
					Callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " is connecting now, please try later."));
					return false;
				case Closing:
				case Closed:
					Callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " is closing or closed."));
					return false;
				case Failed:
				case Uninitialized:
					Callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + transactionCoordinatorId + " not connected."));
					return false;
				default:
					Callback.completeExceptionally(new TransactionCoordinatorClientException.MetaStoreHandlerNotReadyException(transactionCoordinatorId));
					return false;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void run(io.netty.util.Timeout timeout) throws Exception
		public override void Run(Timeout Timeout)
		{
			if (Timeout.Cancelled)
			{
				return;
			}
			long TimeToWaitMs;
			if (State == State.Closing || State == State.Closed)
			{
				return;
			}
			RequestTime Peeked = timeoutQueue.peek();
			while (Peeked != null && Peeked.CreationTimeMs + ClientConflict.Configuration.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				RequestTime LastPolled = timeoutQueue.poll();
				if (LastPolled != null)
				{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: OpBase<?> op = pendingRequests.remove(lastPolled.requestId);
					OpBase<object> Op = pendingRequests.Remove(LastPolled.RequestId);
					if (!Op.Callback.Done)
					{
						Op.Callback.completeExceptionally(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout."));
						if (LOG.DebugEnabled)
						{
							LOG.debug("Transaction coordinator request {} is timeout.", LastPolled.RequestId);
						}
						OnResponse(Op);
					}
				}
				else
				{
					break;
				}
				Peeked = timeoutQueue.peek();
			}

			if (Peeked == null)
			{
				TimeToWaitMs = ClientConflict.Configuration.OperationTimeoutMs;
			}
			else
			{
				long Diff = (Peeked.CreationTimeMs + ClientConflict.Configuration.OperationTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
				if (Diff <= 0)
				{
					TimeToWaitMs = ClientConflict.Configuration.OperationTimeoutMs;
				}
				else
				{
					TimeToWaitMs = Diff;
				}
			}
			requestTimeout = ClientConflict.timer().newTimeout(this, TimeToWaitMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

		private ClientCnx Cnx()
		{
			return this.connectionHandler.Cnx();
		}

		public virtual void ConnectionClosed(ClientCnx Cnx)
		{
			this.connectionHandler.ConnectionClosed(Cnx);
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
				return "Transaction meta store handler [" + transactionCoordinatorId + "]";
			}
		}
	}

}