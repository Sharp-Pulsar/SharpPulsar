using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
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
using System.Buffers;
using static SharpPulsar.Exceptions.TransactionCoordinatorClientException;
using SharpPulsar.Messages.Consumer;

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
		private IActorRef _self;
		private long _requestId = -1;
		private IActorRef _clientCnx;
		private readonly Dictionary<long, (ReadOnlySequence<byte> Command, IActorRef ReplyTo)> _pendingRequests = new Dictionary<long, (ReadOnlySequence<byte> Command, IActorRef ReplyTo)>();
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
        private IActorRef _sender;

		private ICancelable _requestTimeout;
		private readonly ClientConfigurationData _conf;
		private IScheduler _scheduler;

        public IStash Stash { get; set; }

        public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ClientConfigurationData conf)
		{
			_generator = idGenerator;
			_scheduler = Context.System.Scheduler;
			_conf = conf;
			_self = Self;
			_log = Context.System.Log;
			_state = new HandlerState(lookup, cnxPool, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
			_transactionCoordinatorId = transactionCoordinatorId;
			_timeoutQueue = new ConcurrentQueue<RequestTime>();
			//_blockIfReachMaxPendingOps = true;
			_requestTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(conf.OperationTimeoutMs), Self, RunRequestTimeout.Instance, Nobody.Instance);
            ReceiveAsync<GrabCnx>(async _ => 
            {
                _sender = Sender;
                _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(_conf, _state, (new BackoffBuilder()).SetInitialTime(TimeSpan.FromMilliseconds(_conf.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(_conf.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(100)).Create(), Self), "TransactionMetaStoreHandler");

                var askResponse = await _connectionHandler.Ask<AskResponse>(new GrabCnx("TransactionMetaStoreHandler"));
                if(askResponse.Failed)
                    _sender.Tell(askResponse);
                var o = askResponse.ConvertTo<ConnectionOpened>();
                await HandleConnectionOpened(o.ClientCnx);
            });

		}
		private void Listening()
        {
            ReceiveAsync<ConnectionOpened>(async o => {
                await HandleConnectionOpened(o.ClientCnx);
            });
            Receive<ConnectionClosed>(o => {
                HandleConnectionClosed(o.ClientCnx);
            });
            Receive<ConnectionFailed>(f => {
                HandleConnectionFailed(f.Exception);
            });
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
				_invokeArg = new object[] { p.TxnID, p.Topics, Sender };
				_nextBecome = AddPublishPartitionToTxn;
				Become(GetCnxAndRequestId);
			});
			Receive<AbortTxnID>(a => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { a.TxnID, Sender };
				_nextBecome = Abort;
				Become(GetCnxAndRequestId);
			});
			Receive<CommitTxnID>(c => 
			{
				_invokeArg = new object[] { c.TxnID, Sender};
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
			Stash?.UnstashAll();
		}
		private void GetCnxAndRequestId()
		{
			_clientCnx = null;
			_requestId = -1;
			Receive<AskResponse>(m =>
			{
                if(m.Data != null)
				    _clientCnx = m.ConvertTo<IActorRef>();

				_generator.Tell(NewRequestId.Instance);
			});
			Receive<NewRequestIdResponse>(m =>
			{
				_requestId = m.Id;
				Become(() => _nextBecome(_invokeArg));
			});
			ReceiveAny(_ => Stash.Stash());
			_connectionHandler.Tell(GetCnx.Instance);
		}
		public static Props Prop(long transactionCoordinatorId, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ClientConfigurationData conf)
        {
			return Props.Create(()=> new TransactionMetaStoreHandler(transactionCoordinatorId, lookup, cnxPool, idGenerator, topic, conf));
        }
		public virtual void ConnectionFailed(PulsarClientException exception)
		{
			_log.Error("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, exception);
			_state.ConnectionState = HandlerState.State.Failed;
			//send message to parent for exception
			//_connectFuture.completeExceptionally(exception);
		}

		private async ValueTask HandleConnectionOpened(IActorRef cnx)
		{
			_log.Info($"Transaction meta handler with transaction coordinator id {_transactionCoordinatorId} connection opened.");
			_connectionHandler.Tell(new SetCnx(cnx));
			cnx.Tell(new RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, _self));
            if (!_state.ChangeToReadyState())
            {
                await cnx.GracefulStop(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
            else
            {
                _sender.Tell(new AskResponse("Ready"));
                Become(Listening);
            }
		}

		private void NewTransaction(object[] args)
		{
			Receive<NewTxnResponse>(r =>
			{
				HandleNewTxnResponse(r);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var timeout = (long)args[0];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("New transaction with timeout in secs {}", timeout);
			}
			_pendingRequests.Add(_requestId, (new ReadOnlySequence<byte>(new byte[] { (byte)timeout }), _replyTo));
			var cmd = Commands.NewTxn(_transactionCoordinatorId, _requestId, timeout);
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewTxn"));
		}

		private void HandleNewTxnResponse(NewTxnResponse response)
		{
			var requestId = response.RequestId;
			if(!_pendingRequests.TryGetValue(requestId, out var sender))
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got new txn response for timeout {} - {}", response.MostSigBits, response.LeastSigBits);
				}
			}
            else
            {
				_pendingRequests.Remove(requestId);
				if (response?.Error is NoException)
				{
					var txnID = response;
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"Got new txn response ({txnID.MostSigBits}:{txnID.LeastSigBits}) for request {response.RequestId}");
					}
					sender.ReplyTo.Tell(txnID);
				}
				else
				{
					_log.Error("Got new txn for request {} error {}", response.RequestId, response.Error);
                    sender.ReplyTo.Tell(response);
                }
			}
		}

		private void AddPublishPartitionToTxn(object[] args)
		{
			var txnID = (TxnID)args[0];
			var partitions = (IList<string>)args[1];
            var replyTo = (IActorRef)args[2];

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
			var cmd = Commands.NewAddPartitionToTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
			_pendingRequests.Add(_requestId, (cmd, replyTo));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewAddPartitionToTxn"));
		}

		private void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
		{
			var request = (long)response.RequestId;
			if(!_pendingRequests.TryGetValue(request, out var sdr))
            {
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"Got add publish partition to txn response for timeout {response.TxnidMostBits} - {response.TxnidLeastBits}");
					
				}
                sdr.ReplyTo.Tell(new RegisterProducedTopicResponse(null));
                return;
            }
			if(response?.Error == ServerError.UnknownError)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug($"Add publish partition for request {response.RequestId} success.");
				}
			}
			else
			{
                if (response.Error == ServerError.TransactionCoordinatorNotFound)
                    _connectionHandler.Tell(new ReconnectLater(new TransactionCoordinatorClientException.CoordinatorNotFoundException(response.Message)));

                _log.Error($"Add publish partition for request {response.RequestId} error {response.Error}.");
			}
            sdr.ReplyTo.Tell(new RegisterProducedTopicResponse(response.Error));
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
			var cmd = Commands.NewAddSubscriptionToTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
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
                _replyTo.Tell(new AskResponse(new PulsarClientException("Add subscription to txn timeout")));
                return;
			}
			if(response.Error == ServerError.UnknownError)
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Add subscription to txn success for request {}.", response.RequestId);
				}

                _replyTo.Tell(new AskResponse(true));
            }
			else
			{
				_log.Error("Add subscription to txn failed for request {} error {}.", response.RequestId, response.Error);
                _replyTo.Tell(new AskResponse(new PulsarClientException(response.Error.ToString())));
            }
		}

		private void Commit(object[] args)
		{

			Receive<EndTxnResponse>(response =>
			{
				HandleEndTxnResponse(response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var txnID = (TxnID)args[0];
            var replyTo = (IActorRef)args[1];
			if (_log.IsDebugEnabled)
			{
				_log.Debug("Commit txn {}", txnID);
			}
			var cmd = Commands.NewEndTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Commit);
			_pendingRequests.Add(_requestId, (cmd, replyTo));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewEndTxn"));
		}

		private void Abort(object[] args)
		{
			Receive<EndTxnResponse>(response =>
			{
				HandleEndTxnResponse(response);
				Become(Listening);
			});
			ReceiveAny(_ => Stash.Stash());
			var txnID = (TxnID)args[0];
            var replyTo = (IActorRef)args[1];
            if (_log.IsDebugEnabled)
			{
				_log.Debug("Abort txn {}", txnID);
			}
			var cmd = Commands.NewEndTxn(_requestId, txnID.LeastSigBits, txnID.MostSigBits, TxnAction.Abort);
			_pendingRequests.Add(_requestId, (cmd, replyTo));
			_timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), _requestId));
			_clientCnx.Tell(new Payload(cmd, _requestId, "NewEndTxn"));
		}

		private void HandleEndTxnResponse(EndTxnResponse response)
		{
			var request = response.RequestId;
			if (!_pendingRequests.TryGetValue(request, out var msg))
			{
				if(_log.IsDebugEnabled)
				{
					_log.Debug("Got end txn response for timeout {} - {}", response.MostBits, response.LeastBits);
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
            //callback
            msg.ReplyTo.Tell(response);
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
			_requestTimeout = _scheduler.ScheduleTellOnceCancelable(TimeSpan.FromMilliseconds(timeToWaitMs), _self, RunRequestTimeout.Instance, Nobody.Instance);
		}
        
		private void HandleConnectionClosed(IActorRef cnx)
		{
			_connectionHandler.Tell(new ConnectionClosed(cnx));
		}
		private void HandleConnectionFailed(PulsarClientException ex)
		{
            _sender?.Tell(ex.ToString());
			_log.Error(ex.ToString());
            _sender = null;
		}
		internal class RunRequestTimeout
        {
			internal static RunRequestTimeout Instance = new RunRequestTimeout();
		}
	}

}