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
    public class TransactionMetaStoreHandler : ReceiveActor, IWithUnboundedStash
	{
		private readonly long _transactionCoordinatorId;
		private IActorRef _connectionHandler;
		private HandlerState _state;
		private IActorRef _generator;
		private Action<object[]> _nextBecome;
		private object[] _invokeArg;
		private IActorRef _replyTo;
		private long _requestId = -1;
		private IActorRef _clientCnx;
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

		private readonly ILoggingAdapter _log;

		private ICancelable _requestTimeout;
		private readonly ClientConfigurationData _conf;
		private IScheduler _scheduler;

        public IStash Stash { get; set; }

        public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef lookup, IActorRef idGenerator, string topic, ClientConfigurationData conf)
		{
			_generator = idGenerator;
			_scheduler = Context.System.Scheduler;
			_conf = conf;
			_log = Context.System.Log;
			_state = new HandlerState(lookup, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<RequestTime>();
			//_blockIfReachMaxPendingOps = true;
			_requestTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(conf.OperationTimeoutMs), Self, RunRequestTimeout.Instance, Nobody.Instance);
			_connectionHandler = Context.ActorOf(ConnectionHandler.Prop(conf, _state, (new BackoffBuilder()).SetInitialTime(conf.InitialBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMax(conf.MaxBackoffIntervalNanos, TimeUnit.NANOSECONDS).SetMandatoryStop(100, TimeUnit.MILLISECONDS).Create(), Self), "TransactionMetaStoreHandler");
			Listening(); 
			_connectionHandler.Tell(new GrabCnx("TransactionMetaStoreHandler"));

		}
		private void Listening()
        {
			Receive<RunRequestTimeout>(t =>
			{
				RunRequestTime();
			});
			Receive<NewTxn>(t =>
			{
				_replyTo = Sender;
				_invokeArg = new object[] { t.TxnRequestTimeoutMs };
				_nextBecome = NewTransaction;
				Become(GetCnxAndRequestId);
			});
			Receive<AddPublishPartitionToTxn>(p => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { p.TxnID, p.Topics };
				_nextBecome = AddPublishPartitionToTxn;
				Become(GetCnxAndRequestId);
			});
			Receive<AbortTxnID>(a => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { a.TxnID, a.MessageIds };
				_nextBecome = Abort;
				Become(GetCnxAndRequestId);
			});
			Receive<CommitTxnID>(c => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { c.TxnID, c.MessageIds };
				_nextBecome = Commit;
				Become(GetCnxAndRequestId);
			});
			Receive<AddSubscriptionToTxn>(s => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { s.TxnID, s.Subscriptions };
				_nextBecome = AddSubscriptionToTxn;
				Become(GetCnxAndRequestId);
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
			Stash?.UnstashAll();
		}
		private void GetCnxAndRequestId()
		{
			_clientCnx = null;
			_requestId = -1;
			Receive<IActorRef>(m =>
			{
				_clientCnx = m;
				if (_requestId > -1)
					Become(() => _nextBecome(_invokeArg));
			});
			Receive<NewRequestIdResponse>(m =>
			{
				_requestId = m.Id;
				if (_clientCnx != null)
					Become(() => _nextBecome(_invokeArg));
			});
			ReceiveAny(_ => Stash.Stash());
			_connectionHandler.Tell(GetCnx.Instance);
			_generator.Tell(NewRequestId.Instance);
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

		private void NewTransaction(object[] args)
		{

			Receive<NewTxnResponse>(r =>
			{
				HandleNewTxnResponse(r.Response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var timeout = (long)args[0];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("New transaction with timeout in ms {}", timeout);
			}
			_pendingRequests.Add(_requestId, (new byte[] { (byte)timeout }, _replyTo));
			var cmd = new Commands().NewTxn(_transactionCoordinatorId, _requestId, timeout);
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewTxn"));
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

		private void AddPublishPartitionToTxn(object[] args)
		{
			var txnID = (TxnID)args[0];
			var partitions = (IList<string>)args[0];

			Receive<AddPublishPartitionToTxnResponse>(a => 
			{
				HandleAddPublishPartitionToTxnResponse(a.Response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Add publish partition {} to txn {}", partitions, txnID);
			}
			var cmd = new Commands().NewAddPartitionToTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			_pendingRequests.Add(_requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewAddPartitionToTxn"));
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

		private void AddSubscriptionToTxn(object[] args)
		{
			var txnID = (TxnID)args[0];
			var subscriptionList = (IList<Subscription>)args[1];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Add subscription {} to txn {}.", subscriptionList, txnID);
			}
			Receive<AddSubscriptionToTxnResponse>(p => 
			{
				HandleAddSubscriptionToTxnResponse(p.Response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var cmd = new Commands().NewAddSubscriptionToTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
			_pendingRequests.Add(_requestId, (cmd, Sender));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewAddSubscriptionToTxn"));
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

		private void Commit(object[] args)
		{

			Receive<EndTxnResponse>(r =>
			{
				HandleEndTxnResponse(r.Response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var txnID = (TxnID)args[0];
			var sendMessageIdList = (IList<IMessageId>)args[1];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Commit txn {}", txnID);
			}
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
			var cmd = new Commands().NewEndTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Commit, messageIdDataList);
			_pendingRequests.Add(_requestId, (cmd, _replyTo));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewEndTxn"));
		}

		private void Abort(object[] args)
		{
			Receive<EndTxnResponse>(r =>
			{
				HandleEndTxnResponse(r.Response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var txnID = (TxnID)args[0];
			var sendMessageIdList = (IList<IMessageId>)args[1];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Abort txn {}", txnID);
			}
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
			var cmd = new Commands().NewEndTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Abort, messageIdDataList);
			_pendingRequests.Add(_requestId, (cmd, _replyTo));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewEndTxn"));
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
			return await _connectionHandler.Ask<IActorRef>(GetCnx.Instance);
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