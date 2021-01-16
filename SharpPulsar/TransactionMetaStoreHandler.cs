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
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Transaction;
using System;
using System.Collections.Concurrent;
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
namespace SharpPulsar
{
	/// <summary>
	/// Handler for transaction meta store.
	/// </summary>
	public class TransactionMetaStoreHandler : ReceiveActor
	{
		private readonly long _transactionCoordinatorId;
		private ConnectionHandler _connectionHandler;
		private HandlerState _state;
		private ActorSystem _system;
		private readonly Dictionary<long, (byte[] Command, IActorRef ReplyTo)> _pendingRequests = new Dictionary<long, (byte[] Command, IActorRef ReplyTo)>();
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
		private readonly IActorRef _pulsarClient;
		private readonly ClientConfigurationData _conf;
		private IAdvancedScheduler _scheduler;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef pulsarClient, string topic, ClientConfigurationData conf)
		{
			_scheduler = Context.System.Scheduler.Advanced;
			_conf = conf;
			_pulsarClient = pulsarClient;
			_log = Context.System.Log;
			_state = new HandlerState(pulsarClient, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<RequestTime>();
			_blockIfReachMaxPendingOps = true;
			_requestTimeout = _scheduler.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(conf.OperationTimeoutMs)), RunRequestTime);
			_connectionHandler = new ConnectionHandler(_state, (new BackoffBuilder()).SetInitialTime(conf.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(conf.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(100, TimeUnit.MILLISECONDS).Create(), Self);
			_connectionHandler.GrabCnx();
			Receive<NewTxn>(t => 
			{
				NewTransaction(t.Timeout, t.TimeUnit);
			});
			Receive<NewTxnResponse>(r=> 
			{
				HandleNewTxnResponse(r.Response);
			});
			Receive<EndTxnResponse>(r=> 
			{
				HandleEndTxnResponse(r.Response);
			});
			Receive<AddPublishPartitionToTxnResponse>(a => {
				HandleAddPublishPartitionToTxnResponse(a.Response);
			
			});
			Receive<AddPublishPartitionToTxn>(p => {
				AddPublishPartitionToTxn(p.TxnID, p.Topics);
			});
			Receive<AddSubscriptionToTxnResponse>(p => {
				HandleAddSubscriptionToTxnResponse(p.Response);
			});
			Receive<Abort>(a => {
				Abort(a.TxnID, a.MessageIds);
			});
			Receive<Commit>(c => {
				Commit(c.TxnID, c.MessageIds);
			});
			Receive<AddSubscriptionToTxn>(s => {
				AddSubscriptionToTxn(s.TxnID, s.Subscriptions);
			});
			Receive<ConnectionOpened>(o => {
				HandleConnectionOpened(o.ClientCnx);
			});
			Receive<ConnectionClosed>(o => {
				HandleConnectionClosed(o.ClientCnx);
			});
			Receive<ConnectionFailed>(f => {
				HandleConnectionFailed(f.Exception);
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

		private void HandleConnectionOpened(IActorRef cnx)
		{
			_system.Log.Info("Transaction meta handler with transaction coordinator id {} connection opened.", _transactionCoordinatorId);
			_connectionHandler.ClientCnx = cnx;
			cnx.Tell(new RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, Self));
			if(!_state.ChangeToReadyState())
			{
				cnx.GracefulStop(TimeSpan.FromSeconds(2));
			}
		}

		private void NewTransaction(long timeout, TimeUnit unit)
		{
			if(_system.Log.IsDebugEnabled)
			{
				_system.Log.Debug("New transaction with timeout in ms {}", unit.ToMilliseconds(timeout));
			}
			long? requestId = null;
			_pulsarClient.Ask<long>(NewRequestId.Instance).ContinueWith(task => {

				requestId = task.Result;
			});
            if (requestId.HasValue)
            {
				_pendingRequests.Add(requestId.Value, (new byte[] { (byte)timeout }, Sender));
				var cmd = Commands.NewTxn(_transactionCoordinatorId, requestId.Value, unit.ToMilliseconds(timeout));
				_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId.Value));
				Cnx().Tell(new Payload(cmd, requestId.Value, "NewTxn"));
			}
		}

		private void HandleNewTxnResponse(CommandNewTxnResponse response)
		{
			var requestId = (long)response.RequestId;
			if(!_pendingRequests.TryGetValue(requestId, out var sender))
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got new txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
			}
            else
            {
				_pendingRequests.Remove(requestId);
				if (response?.Error != null)
				{
					var txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
					if (_log.IsDebugEnabled)
					{
						_log.Debug("Got new txn response {} for request {}", txnID, response.RequestId);
					}
					sender.ReplyTo.Tell(txnID);
				}
				else
				{
					_log.Error("Got new txn for request {} error {}", response.RequestId, response.Error);
				}
			}
		}

		private void AddPublishPartitionToTxn(TxnID txnID, IList<string> partitions)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add publish partition {} to txn {}", partitions, txnID);
			}
			long requestId = 0L;
			_state.Client.Ask<long>(NewRequestId.Instance).ContinueWith(task => 
			{
				if (!task.IsFaulted)
					requestId = task.Result;
			
			});
			var cmd = Commands.NewAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Tell(new Payload(cmd, requestId, "NewAddPartitionToTxn"));
		}

		private void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			var request = (long)response.RequestId;
			if(!_pendingRequests.TryGetValue(request, out var s))
            {
				if (_log.IsDebugEnabled)
				{
					_log.Debug("Got add publish partition to txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
					return;
				}
			}
			if(response?.Error != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add publish partition for request {} success.", response.RequestId);
				}
			}
			else
			{
				_log.Error("Add publish partition for request {} error {}.", response.RequestId, response.Error);
			}
		}

		private void AddSubscriptionToTxn(TxnID txnID, IList<Subscription> subscriptionList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add subscription {} to txn {}.", subscriptionList, txnID);
			}
			long requestId = 0L;
			_state.Client.Ask<long>(NewRequestId.Instance).ContinueWith(task =>
			{
				requestId = task.Result;
			});
			var cmd = Commands.NewAddSubscriptionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Tell(new Payload(cmd, requestId, "NewAddSubscriptionToTxn"));
		}

		private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
		{
			var request = (long)response.RequestId;
			if (!_pendingRequests.TryGetValue(request, out var s))
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add subscription to txn timeout for request {}.", response.RequestId);
				}
				return;
			}
			if(response?.Error != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add subscription to txn success for request {}.", response.RequestId);
				}
			}
			else
			{
				_log.Error("Add subscription to txn failed for request {} error {}.", response.RequestId, response.Error);
			}
		}

		private void Commit(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Commit txn {}", txnID);
			}
			long requestId = 0L;
			_state.Client.Ask<long>(NewRequestId.Instance).ContinueWith(task =>
			{
				requestId = task.Result;
			});
			var messageIdDataList = new List<MessageIdData>();
			foreach(MessageId messageId in sendMessageIdList)
			{
				messageIdDataList.Add(new MessageIdData 
				{ 
					ledgerId = (ulong)messageId.LedgerId,
					entryId = (ulong)messageId.EntryId,
					Partition = messageId.PartitionIndex
				});
			}
			var cmd = Commands.NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Commit, messageIdDataList);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Tell(new Payload(cmd, requestId, "NewEndTxn"));
		}

		private void Abort(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Abort txn {}", txnID);
			}

			long requestId = 0L;
			_state.Client.Ask<long>(NewRequestId.Instance).ContinueWith(task =>
			{
				requestId = task.Result;
			});

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
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			Cnx().Tell(new Payload(cmd, requestId, "NewEndTxn"));
		}

		private void HandleEndTxnResponse(CommandEndTxnResponse response)
		{
			var request = (long)response.RequestId;
			if (!_pendingRequests.TryGetValue(request, out var s))
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got end txn response for timeout {} - {}", response.TxnidMostBits, response.TxnidLeastBits);
				}
				return;
			}
			if(response?.Error != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got end txn response success for request {}", response.RequestId);
				}
			}
			else
			{
				_log.Error("Got end txn response for request {} error {}", response.RequestId, response.Error);
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



		private void RunRequestTime()
		{
			if(_requestTimeout.IsCancellationRequested)
			{
				return;
			}
			long timeToWaitMs;
			if(_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
			{
				return;
			}
			RequestTime peeked;
			while(_timeoutQueue.TryPeek(out peeked) && peeked.CreationTimeMs + _conf.OperationTimeoutMs - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
			{
				if(_timeoutQueue.TryDequeue(out var lastPolled))
				{
					if(_pendingRequests.Remove(lastPolled.RequestId))
					{
						_log.Error(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout.").ToString());
						if(_log.IsDebugEnabled)
						{
							_log.Debug("Transaction coordinator request {} is timeout.", lastPolled.RequestId);
						}
					}
				}
				else
				{
					break;
				}
			}

			if(peeked == null)
			{
				timeToWaitMs = _conf.OperationTimeoutMs;
			}
			else
			{
				long diff = (peeked.CreationTimeMs + _conf.OperationTimeoutMs) - DateTimeHelper.CurrentUnixTimeMillis();
				if(diff <= 0)
				{
					timeToWaitMs = _conf.OperationTimeoutMs;
				}
				else
				{
					timeToWaitMs = diff;
				}
			}
			_requestTimeout = _scheduler.ScheduleOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), RunRequestTime);
		}

		private IActorRef Cnx()
		{
			return _connectionHandler.Cnx();
		}

		private void HandleConnectionClosed(IActorRef cnx)
		{
			_connectionHandler.ConnectionClosed(cnx);
		}
		private void HandleConnectionFailed(PulsarClientException ex)
		{
			_log.Error(ex.ToString());
		}

	}

}