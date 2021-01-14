using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
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
namespace SharpPulsar
{
	/// <summary>
	/// Handler for transaction meta store.
	/// </summary>
	public class TransactionMetaStoreHandler : ReceiveActor,  ConnectionHandler.IConnection
	{
		private readonly long _transactionCoordinatorId;
		private ConnectionHandler _connectionHandler;
		private HandlerState _state;
		private ActorSystem _system;
		private readonly Dictionary<long, byte[]> _pendingRequests = new Dictionary<long, byte[]>();
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
		private readonly ILoggingAdapter _log;

		private ICancelable _requestTimeout;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef pulsarClient, string topic, ClientConfigurationData conf)
		{
			_log = Context.System.Log;
			_state = new HandlerState(pulsarClient, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<RequestTime>();
			_blockIfReachMaxPendingOps = true;
			_requestTimeout = pulsarClient.Timer().newTimeout(this, conf.OperationTimeoutMs, TimeUnit.MILLISECONDS);
			_connectionHandler = new ConnectionHandler(_state, (new BackoffBuilder()).SetInitialTime(conf.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(conf.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(100, TimeUnit.MILLISECONDS).Create(), this);
			_connectionHandler.GrabCnx();
			Receive<NewTransaction>(t => 
			{
				var txId = NewTransaction(t.Timeout, t.Unit);
				Sender.Tell(txId);
			});
		}
		public static Props Prop(long transactionCoordinatorId, IActorRef pulsarClient, string topic, ClientConfigurationData conf)
        {
			return Props.Create(()=> new TransactionMetaStoreHandler(transactionCoordinatorId, pulsarClient, topic, conf));
        }
		public virtual void ConnectionFailed(PulsarClientException exception)
		{
			_system.Log.Error("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			_state.ConnectionState = HandlerState.State.Failed;
			//send message to parent for exception
			//_connectFuture.completeExceptionally(exception);
		}

		public virtual void ConnectionOpened(ClientCnx cnx)
		{
			_system.Log.Info("Transaction meta handler with transaction coordinator id {} connection opened.", _transactionCoordinatorId);
			_connectionHandler.ClientCnx = cnx;
			cnx.RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, this);
			if(!_state.ChangeToReadyState())
			{
				cnx.Channel().close();
			}
			//send completion message to parent
			//_connectFuture.complete(null);
		}

		private TxnID NewTransaction(long timeout, TimeUnit unit)
		{
			if(_system.Log.IsDebugEnabled)
			{
				_system.Log.Debug("New transaction with timeout in ms {}", unit.ToMilliseconds(timeout));
			}
			CompletableFuture<TxnID> callback = new CompletableFuture<TxnID>();

			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = ClientConflict.NewRequestId();
			var cmd = Commands.NewTxn(_transactionCoordinatorId, requestId, unit.ToMilliseconds(timeout));
			OpForTxnIdCallBack op = OpForTxnIdCallBack.Create(cmd, callback);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return callback;
		}

		internal virtual void HandleNewTxnResponse(CommandNewTxnResponse response)
		{
			var op = _pendingRequests.Remove((long)response.RequestId);
			if(!op)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got new txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			
			if(response.Error != null)
			{
				TxnID txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got new txn response {} for request {}", txnID, response.RequestId);
				}
				op.Callback.complete(txnID);
			}
			else
			{
				_log.Error("Got new txn for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		private void AddPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add publish partition {} to txn {}", partitions, txnID);
			}
			if(!CanSendRequest(callback))
			{
				return callback;
			}
			long requestId = 0L;
			_state.Client.Ask<long>(NewRequestId.Instance).ContinueWith(task => 
			{
				if (!task.IsFaulted)
					requestId = task.Result;
			
			});
			var cmd = Commands.NewAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, callback);
			_pendingRequests.Add(requestId, cmd);
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Send(cmd, Cnx());
			return callback;
		}

		internal virtual void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got add publish partition to txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add publish partition for request {} success.", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_log.Error("Add publish partition for request {} error {}.", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}

			OnResponse(op);
		}

		private void AddSubscriptionToTxn(TxnID txnID, IList<Subscription> subscriptionList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add subscription {} to txn {}.", subscriptionList, txnID);
			}
			long requestId = ClientConflict.NewRequestId();
			var cmd = Commands.NewAddSubscriptionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
			OpForVoidCallBack op = OpForVoidCallBack.Create(cmd, completableFuture);
			_pendingRequests.Put(requestId, op);
			_timeoutQueue.add(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			cmd.retain();
			Cnx().Ctx().writeAndFlush(cmd, Cnx().Ctx().voidPromise());
			return completableFuture;
		}

		private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add subscription to txn timeout for request {}.", response.RequestId);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add subscription to txn success for request {}.", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_log.Error("Add subscription to txn failed for request {} error {}.", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
			}
			OnResponse(op);
		}

		private void Commit(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Commit txn {}", txnID);
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

		private void Abort(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Abort txn {}", txnID);
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

		private void HandleEndTxnResponse(CommandEndTxnResponse response)
		{
			OpForVoidCallBack op = (OpForVoidCallBack) _pendingRequests.Remove(response.RequestId);
			if(op == null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got end txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if(!response.HasError())
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got end txn response success for request {}", response.RequestId);
				}
				op.Callback.complete(null);
			}
			else
			{
				_log.Error("Got end txn response for request {} error {}", response.RequestId, response.Error);
				op.Callback.completeExceptionally(GetExceptionByServerError(response.Error, response.Message));
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
						if(_log.IsDebugEnabled)
						{
							_log.Debug("Transaction coordinator request {} is timeout.", lastPolled.RequestId);
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

		protected internal override string HandlerName
		{
			get
			{
				return "Transaction meta store handler [" + _transactionCoordinatorId + "]";
			}
		}
	}

}