using Akka.Actor;
using SharpPulsar;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Transaction;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
namespace Org.Apache.Pulsar.Client.Impl
{
	/// <summary>
	/// Handler for transaction meta store.
	/// </summary>
	public class TransactionMetaStoreHandler :  HandlerState, IConnection, System.IDisposable
	{
		private readonly long _transactionCoordinatorId;
		private ConnectionHandler _connectionHandler;
		private ActorSystem _system;
		private readonly ConcurrentLongHashMap<OpBase<object>> _pendingRequests = new ConcurrentLongHashMap<OpBase<object>>(16, 1);
		private readonly ConcurrentQueue<RequestTime> _timeoutQueue;

		private class RequestTime
		{
			internal readonly long CreationTimeMs;
			internal readonly long RequestId;

			public RequestTime(long creationTime, long requestId)
			{
				CreationTimeMs = creationTime;
				RequestId = requestId;
			}
		}

		private readonly bool _blockIfReachMaxPendingOps;
		private readonly Semaphore _semaphore;

		private Timeout _requestTimeout;

		private CompletableFuture<Void> _connectFuture;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef pulsarClient, string topic, CompletableFuture<Void> connectFuture) : base(pulsarClient, topic)
		{
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentLinkedQueue<RequestTime>();
			_blockIfReachMaxPendingOps = true;
			_semaphore = new Semaphore(1000);
			_requestTimeout = pulsarClient.Timer().newTimeout(this, pulsarClient.Configuration.OperationTimeoutMs, TimeUnit.MILLISECONDS);
			_connectionHandler = new ConnectionHandler(this, (new BackoffBuilder()).SetInitialTime(pulsarClient.Configuration.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(pulsarClient.Configuration.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).setMandatoryStop(100, TimeUnit.MILLISECONDS).create(), this);
			_connectionHandler.GrabCnx();
			_connectFuture = connectFuture;
		}

		public virtual void ConnectionFailed(PulsarClientException exception)
		{
			_lOG.error("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			State = State.Failed;
			_connectFuture.completeExceptionally(exception);
		}

		public virtual void ConnectionOpened(ClientCnx cnx)
		{
			_lOG.info("Transaction meta handler with transaction coordinator id {} connection opened.", _transactionCoordinatorId);
			_connectionHandler.ClientCnx = cnx;
			cnx.RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, this);
			if(!ChangeToReadyState())
			{
				cnx.Channel().close();
			}
			_connectFuture.complete(null);
		}

		public virtual CompletableFuture<TxnID> NewTransactionAsync(long timeout, TimeUnit unit)
		{
			if(_lOG.DebugEnabled)
			{
				_lOG.debug("New transaction with timeout in ms {}", unit.toMillis(timeout));
			}
			CompletableFuture<TxnID> callback = new CompletableFuture<TxnID>();

			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = ClientConflict.NewRequestId();
			ByteBuf cmd = Commands.NewTxn(_transactionCoordinatorId, requestId, unit.toMillis(timeout));
			OpForTxnIdCallBack op = OpForTxnIdCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callback;
		}

		internal virtual void HandleNewTxnResponse(CommandNewTxnResponse response)
		{
			OpForTxnIdCallBack op = (OpForTxnIdCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Got new txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			
			if(response.Error != null)
			{
				TxnID txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Got new txn response {} for request {}", txnID, response.RequestId);
				}
				op.Callback.complete(txnID);
			}
			else
			{
				_lOG.error("Got new txn for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public async Task AddPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions)
		{
			if(_lOG.DebugEnabled)
			{
				_lOG.debug("Add publish partition {} to txn {}", partitions, txnID);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = ClientConflict.NewRequestId();
			var cmd = Commands.NewAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callback;
		}

		internal virtual void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Got add publish partition to txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Add publish partition for request {} success.", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_lOG.error("Add publish partition for request {} error {}.", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		public virtual CompletableFuture<Void> AddSubscriptionToTxn(TxnID txnID, IList<PulsarApi.Subscription> subscriptionList)
		{
			if(_lOG.DebugEnabled)
			{
				_lOG.debug("Add subscription {} to txn {}.", subscriptionList, txnID);
			}
			CompletableFuture<Void> completableFuture = new CompletableFuture<Void>();
			long requestId = ClientConflict.NewRequestId();
			ByteBuf cmd = Commands.NewAddSubscriptionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, completableFuture);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return completableFuture;
		}

		public virtual void HandleAddSubscriptionToTxnResponse(PulsarApi.CommandAddSubscriptionToTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Add subscription to txn timeout for request {}.", response.RequestId);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Add subscription to txn success for request {}.", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_lOG.error("Add subscription to txn failed for request {} error {}.", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}
			OnResponse(op);
		}

		public virtual CompletableFuture<Void> CommitAsync(TxnID txnID, IList<MessageId> sendMessageIdList)
		{
			if(_lOG.DebugEnabled)
			{
				_lOG.debug("Commit txn {}", txnID);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = ClientConflict.NewRequestId();
			IList<PulsarApi.MessageIdData> messageIdDataList = new List<PulsarApi.MessageIdData>();
			foreach(MessageId messageId in sendMessageIdList)
			{
				messageIdDataList.Add(PulsarApi.MessageIdData.NewBuilder().setLedgerId(((MessageIdImpl) messageId).LedgerId).setEntryId(((MessageIdImpl) messageId).EntryId).setPartition(((MessageIdImpl) messageId).PartitionIndex).build());
			}
			ByteBuf cmd = Commands.NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, PulsarApi.TxnAction.COMMIT, messageIdDataList);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callback;
		}

		public virtual CompletableFuture<Void> AbortAsync(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_lOG.DebugEnabled)
			{
				_lOG.debug("Abort txn {}", txnID);
			}
			CompletableFuture<Void> callback = new CompletableFuture<Void>();

			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = ClientConflict.NewRequestId();

			IList<MessageIdData> messageIdDataList = new List<MessageIdData>();
			foreach(IMessageId messageId in sendMessageIdList)
			{
				var msgIdData = new MessageIdData
				{
					ledgerId = (ulong)((MessageId)messageId).LedgerId,
					entryId = (ulong)((MessageId)messageId).EntryId,
					Partition = ((MessageId)messageId).PartitionIndex,

				};
				messageIdDataList.Add(msgIdData);
			}
			var cmd = Commands.NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Abort, messageIdDataList);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callback;
		}

		internal virtual void HandleEndTxnResponse(CommandEndTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Got end txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_lOG.DebugEnabled)
				{
					_lOG.debug("Got end txn response success for request {}", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_lOG.error("Got end txn response for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		private abstract class OpBase<T>
		{
			protected internal ByteBuf Cmd;
			protected internal CompletableFuture<T> Callback;

			internal abstract void Recycle();
		}

		private class OpForTxnIdCallBack : OpBase<TxnID>
		{

			internal static OpForTxnIdCallBack Create(ByteBuf cmd, CompletableFuture<TxnID> callback)
			{
				OpForTxnIdCallBack op = RECYCLER.get();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}

			internal OpForTxnIdCallBack(Recycler.Handle<OpForTxnIdCallBack> recyclerHandle)
			{
				RecyclerHandle = recyclerHandle;
			}

			internal override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForTxnIdCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForTxnIdCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpForTxnIdCallBack>
			{
				protected internal override OpForTxnIdCallBack NewObject(Handle<OpForTxnIdCallBack> handle)
				{
					return new OpForTxnIdCallBack(handle);
				}
			}
		}

		private class OpForVoidCallBack : OpBase<Void>
		{

			internal static OpForVoidCallBack Create(ByteBuf cmd, CompletableFuture<Void> callback)
			{
				OpForVoidCallBack op = RECYCLER.get();
				op.Callback = callback;
				op.Cmd = cmd;
				return op;
			}
			internal OpForVoidCallBack(Recycler.Handle<OpForVoidCallBack> recyclerHandle)
			{
				RecyclerHandle = recyclerHandle;
			}

			internal override void Recycle()
			{
				RecyclerHandle.recycle(this);
			}

			internal readonly Recycler.Handle<OpForVoidCallBack> RecyclerHandle;
			internal static readonly Recycler<OpForVoidCallBack> RECYCLER = new RecyclerAnonymousInnerClass();

			private class RecyclerAnonymousInnerClass : Recycler<OpForVoidCallBack>
			{
				protected internal override OpForVoidCallBack NewObject(Handle<OpForVoidCallBack> handle)
				{
					return new OpForVoidCallBack(handle);
				}
			}
		}

		private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
		{
			switch(serverError)
			{
				case ServerError.TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
				case ServerError.InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
				default:
					return new TransactionCoordinatorClientException(msg);
			}
		}

		private void OnResponse<T1>(OpBase<T1> op)
		{
			ReferenceCountUtil.safeRelease(op.Cmd);
			op.Recycle();
			_semaphore.release();
		}

		private bool CanSendRequest<T1>(CompletableFuture<T1> callback)
		{
			if(!IsValidHandlerState(callback))
			{
				return false;
			}
			try
			{
				if(_blockIfReachMaxPendingOps)
				{
					_semaphore.acquire();
				}
				else
				{
					if(!_semaphore.tryAcquire())
					{
						callback.completeExceptionally(new TransactionCoordinatorClientException("Reach max pending ops."));
						return false;
					}
				}
			}
			catch(InterruptedException e)
			{
				Thread.CurrentThread.Interrupt();
				callback.completeExceptionally(TransactionCoordinatorClientException.Unwrap(e));
				return false;
			}
			return true;
		}

		private bool IsValidHandlerState<T1>(CompletableFuture<T1> callback)
		{
			switch(State)
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

		public override void Run(Timeout timeout)
		{
			if(timeout.Cancelled)
			{
				return;
			}
			long timeToWaitMs;
			if(State == State.Closing || State == State.Closed)
			{
				return;
			}
			RequestTime peeked = _timeoutQueue.peek();
			while(peeked != null && peeked.CreationTimeMs + ClientConflict.Configuration.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				RequestTime lastPolled = _timeoutQueue.poll();
				if(lastPolled != null)
				{
					OpBase<object> op = _pendingRequests.Remove(lastPolled.RequestId);
					if(op != null && !op.Callback.Done)
					{
						op.Callback.completeExceptionally(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout."));
						if(_lOG.DebugEnabled)
						{
							_lOG.debug("Transaction coordinator request {} is timeout.", lastPolled.RequestId);
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

			if(peeked == null)
			{
				timeToWaitMs = ClientConflict.Configuration.OperationTimeoutMs;
			}
			else
			{
				long diff = (peeked.CreationTimeMs + ClientConflict.Configuration.OperationTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
				if(diff <= 0)
				{
					timeToWaitMs = ClientConflict.Configuration.OperationTimeoutMs;
				}
				else
				{
					timeToWaitMs = diff;
				}
			}
			_requestTimeout = ClientConflict.Timer().newTimeout(this, timeToWaitMs, TimeUnit.MILLISECONDS);
		}

		private ClientCnx Cnx()
		{
			return _connectionHandler.Cnx();
		}

		internal virtual void ConnectionClosed(ClientCnx cnx)
		{
			_connectionHandler.ConnectionClosed(cnx);
		}

		public virtual void Dispose()
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