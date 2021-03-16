using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
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
using SharpPulsar.Messages.Client;
using SharpPulsar.Extension;
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
	public class TransactionMetaStoreHandler : ReceiveActor
	{
		private readonly long _transactionCoordinatorId;
		private IActorRef _connectionHandler;
		private HandlerState _state;
		private IActorRef _generator;
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
		private IScheduler _scheduler;

		public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef pulsarClient, IActorRef idGenerator, string topic, ClientConfigurationData conf)
		{
			_generator = idGenerator;
			_scheduler = Context.System.Scheduler;
			_conf = conf;
			_pulsarClient = pulsarClient;
			_log = Context.System.Log;
			_state = new HandlerState(pulsarClient, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<RequestTime>();
			_blockIfReachMaxPendingOps = true;
			_requestTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(conf.OperationTimeoutMs), Self, RunRequestTimeout.Instance, Nobody.Instance);
			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(conf, _state, (new BackoffBuilder()).SetInitialTime(conf.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(conf.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(100, TimeUnit.MILLISECONDS).Create(), Self), "TransactionMetaStoreHandler");
			_connectionHandler.Tell(new GrabCnx("TransactionMetaStoreHandler"));
			Receive<RunRequestTimeout>(t => 
			{
				RunRequestTime();
			});
			ReceiveAsync<NewTxn>(async t => 
			{
				await NewTransaction(t.TxnRequestTimeoutMs);
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
			ReceiveAsync<AddPublishPartitionToTxn>(async p => {
				await AddPublishPartitionToTxn(p.TxnID, p.Topics);
			});
			Receive<AddSubscriptionToTxnResponse>(p => {
				HandleAddSubscriptionToTxnResponse(p.Response);
			});
			ReceiveAsync<AbortTxnID>(async a => {
				await Abort(a.TxnID, a.MessageIds);
			});
			ReceiveAsync<CommitTxnID>(async c => {
				await Commit(c.TxnID, c.MessageIds);
			});
			ReceiveAsync<AddSubscriptionToTxn>(async s => {
				await AddSubscriptionToTxn(s.TxnID, s.Subscriptions);
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
		public static Props Prop(long transactionCoordinatorId, IActorRef pulsarClient, IActorRef idGenerator, string topic, ClientConfigurationData conf)
        {
			return Props.Create(()=> new TransactionMetaStoreHandler(transactionCoordinatorId, pulsarClient, idGenerator, topic, conf));
        }
		public virtual void ConnectionFailed(PulsarClientException exception)
		{
			_log.Error("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			_state.ConnectionState = HandlerState.State.Failed;
			//send message to parent for exception
			//_connectFuture.completeExceptionally(exception);
		}

		private void HandleConnectionOpened(IActorRef cnx)
		{
			_log.Info($"Transaction meta handler with transaction coordinator id {_transactionCoordinatorId} connection opened.");
			_connectionHandler.Tell(new SetCnx(cnx));
			cnx.Tell(new RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, Self));
			if(!_state.ChangeToReadyState())
			{
				cnx.GracefulStop(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
			}
		}

		private async ValueTask NewTransaction(long timeout)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("New transaction with timeout in ms {}", timeout);
			}

			var request = await _generator.AskFor<NewRequestIdResponse>(NewRequestId.Instance);
			long requestId = request.Id;
			_pendingRequests.Add(requestId, (new byte[] { (byte)timeout }, Sender));
			var cmd = new Commands().NewTxn(_transactionCoordinatorId, requestId, timeout);
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			var cnx = await Cnx();
			cnx.Tell(new Payload(cmd, requestId, "NewTxn"));
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
					var txnID = new NewTxnResponse(response);
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

		private async ValueTask AddPublishPartitionToTxn(TxnID txnID, IList<string> partitions)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add publish partition {} to txn {}", partitions, txnID);
			}
			var request = await _generator.AskFor<NewRequestIdResponse>(NewRequestId.Instance);
			long requestId = request.Id;
			var cmd = new Commands().NewAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			var cnx = await Cnx();
			cnx.Tell(new Payload(cmd, requestId, "NewAddPartitionToTxn"));
		}

		private void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			var request = (long)response.RequestId;
			if(!_pendingRequests.TryGetValue(request, out var _))
            {
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Got add publish partition to txn response for timeout {response.TxnidMostBits} - {response.TxnidLeastBits}");
					return;
				}
			}
			if(response?.Error != null)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"Add publish partition for request {response.RequestId} success.");
				}
			}
			else
			{
				_log.Error($"Add publish partition for request {response.RequestId} error {response.Error}.");
			}
		}

		private async ValueTask AddSubscriptionToTxn(TxnID txnID, IList<Subscription> subscriptionList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Add subscription {} to txn {}.", subscriptionList, txnID);
			}
			var request = await _generator.AskFor<NewRequestIdResponse>(NewRequestId.Instance);
			long requestId = request.Id;
			var cmd = new Commands().NewAddSubscriptionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			var cnx = await Cnx();
			cnx.Tell(new Payload(cmd, requestId, "NewAddSubscriptionToTxn"));
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

		private async ValueTask Commit(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Commit txn {}", txnID);
			}
			var request = await _generator.AskFor<NewRequestIdResponse>(NewRequestId.Instance);
			long requestId = request.Id;
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
			var cmd = new Commands().NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Commit, messageIdDataList);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			var cnx = await Cnx();
			cnx.Tell(new Payload(cmd, requestId, "NewEndTxn"));
		}

		private async ValueTask Abort(TxnID txnID, IList<IMessageId> sendMessageIdList)
		{
			if(_log.IsDebugEnabled)
			{
				_log.Debug("Abort txn {}", txnID);
			}

			var request = await _generator.AskFor<NewRequestIdResponse>(NewRequestId.Instance);
			long requestId = request.Id;

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
			var cmd = new Commands().NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Abort, messageIdDataList);
			_pendingRequests.Add(requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
			var cnx = await Cnx();
			cnx.Tell(new Payload(cmd, requestId, "NewEndTxn"));
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
			_pendingRequests.Remove(request);
		}

		private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
		{
			switch(serverError)
			{
				case ServerError.TransactionCoordinatorNotFound:
					return new TransactionCoordinatorClientException.CoordinatorNotFoundException(msg);
				case ServerError.InvalidTxnStatus:
					return new TransactionCoordinatorClientException.InvalidTxnStatusException(msg);
				case ServerError.TransactionNotFound:
					return new TransactionCoordinatorClientException.TransactionNotFoundException(msg);
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
			_requestTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), Self, RunRequestTimeout.Instance, Nobody.Instance);
		}

		private async ValueTask<IActorRef> Cnx()
		{
			return await _connectionHandler.AskFor<IActorRef>(GetCnx.Instance);
		}

		private void HandleConnectionClosed(IActorRef cnx)
		{
			_connectionHandler.Tell(new ConnectionClosed(cnx));
		}
		private void HandleConnectionFailed(PulsarClientException ex)
		{
			_log.Error(ex.ToString());
		}
		internal class RunRequestTimeout
        {
			internal static RunRequestTimeout Instance = new RunRequestTimeout();
		}
	}

}