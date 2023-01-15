using Akka.Actor;
using Akka.Event;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpPulsar.Messages.Client;
using System.Threading.Tasks;
using System.Buffers;
using static SharpPulsar.Exceptions.TransactionCoordinatorClientException;
using SharpPulsar.Messages.Consumer;
using Akka.Util.Internal;
using SharpPulsar.TransactionImpl;

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
        private readonly IActorRef _connectionHandler;
        private readonly HandlerState _state;
        private readonly IActorRef _generator;
        private object[] _invokeArg;
        private IActorRef _replyTo;
        private readonly IActorRef _self;
        //private long _requestId = -1;
        private IActorRef _clientCnx;
        private readonly IActorContext _context;
        private readonly ConcurrentDictionary<long, OpBase<object>> pendingRequests = new ConcurrentDictionary<long, OpBase<object>>();

        private readonly Dictionary<long, (ReadOnlySequence<byte> Command, IActorRef ReplyTo)> _pendingRequests = new Dictionary<long, (ReadOnlySequence<byte> Command, IActorRef ReplyTo)>();
        private readonly ConcurrentQueue<RequestTime> _timeoutQueue;
        private readonly Commands _commands = new Commands();
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
        private readonly IScheduler _scheduler;
        private readonly TaskCompletionSource<object> _connectFuture;

        public IStash Stash { get; set; }

        public TransactionMetaStoreHandler(long transactionCoordinatorId, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ClientConfigurationData conf, TaskCompletionSource<object> completionSource)
        {
            _context = Context;
            _generator = idGenerator;
            _scheduler = Context.System.Scheduler;
            _conf = conf;
            _self = Self;
            _log = Context.System.Log;
            _connectFuture = completionSource;
            _state = new HandlerState(lookup, cnxPool, topic, Context.System, "Transaction meta store handler [" + _transactionCoordinatorId + "]");
            _transactionCoordinatorId = transactionCoordinatorId;
            _timeoutQueue = new ConcurrentQueue<RequestTime>();
            //_blockIfReachMaxPendingOps = true;
            _requestTimeout = _scheduler.ScheduleTellOnceCancelable(conf.OperationTimeout, Self, RunRequestTimeout.Instance, Nobody.Instance);

            _connectionHandler = Context.ActorOf(ConnectionHandler.Prop(_conf, _state, (new BackoffBuilder()).SetInitialTime(TimeSpan.FromMilliseconds(_conf.InitialBackoffIntervalMs)).SetMax(TimeSpan.FromMilliseconds(_conf.MaxBackoffIntervalMs)).SetMandatoryStop(TimeSpan.FromMilliseconds(100)).Create(), Self), "TransactionMetaStoreHandler");
            _connectionHandler.Tell(new GrabCnx("TransactionMetaStoreHandler"));
            Listening();
        }
        private void Listening()
        {
            ReceiveAsync<AskResponse>(async askResponse =>
            {
                if (askResponse.Failed)
                {
                    _connectFuture.TrySetException(askResponse.Exception);
                }
                var o = askResponse.ConvertTo<ConnectionOpened>();
                await HandleConnectionOpened(o.ClientCnx);
            });
            ReceiveAsync<ConnectionOpened>(async o => {
                await HandleConnectionOpened(o.ClientCnx);
            });
            Receive<ConnectionClosed>(o => {
                HandleConnectionClosed(o.ClientCnx);
            });
            Receive<ConnectionFailed>(f => {
                _log.Error("Transaction meta handler with transaction coordinator id {} connection failed.", _transactionCoordinatorId, f.Exception);
                if (!_connectFuture.Task.IsCompleted)
                {
                    _connectFuture.TrySetException(f.Exception);
                }
            });
            Receive<RunRequestTimeout>(t =>
            {
                RunRequestTime();
            });
            Receive<NewTxn>(t =>
            {
                _replyTo = Sender;
                _invokeArg = new object[] { t.TxnRequestTimeoutMs };

                GetCnxAndRequestId(() => { return NewTransaction(_invokeArg); });
            });
            Receive<AddPublishPartitionToTxn>(p =>
            {
                _replyTo = Sender;
                _invokeArg = new object[] { p.TxnID, p.Topics, Sender };
                GetCnxAndRequestId(() => { return AddPublishPartitionToTxn(_invokeArg); });
            });
            Receive<AbortTxnID>(a =>
            {
                _replyTo = Sender;
                _invokeArg = new object[] { a.TxnID, Sender, TxnAction.Abort };
                GetCnxAndRequestId(() => { return EndTxn(_invokeArg); });
            });
            Receive<CommitTxnID>(c =>
            {
                _replyTo = Sender;
                _invokeArg = new object[] { c.TxnID, Sender, TxnAction.Commit };
                GetCnxAndRequestId(() => { return EndTxn(_invokeArg); });
            });
            Receive<AddSubscriptionToTxn>(s =>
            {
                _replyTo = Sender;
                _invokeArg = new object[] { s.TxnID, s.Subscriptions };
                GetCnxAndRequestId(() => { return AddSubscriptionToTxn(_invokeArg); });
            });
            Stash?.UnstashAll();
        }
        private void GetCnxAndRequestId(Func<Task<object>> action)
        {
            action();
        }
        public static Props Prop(long transactionCoordinatorId, IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, string topic, ClientConfigurationData conf, TaskCompletionSource<object> connectFuture)
        {
            return Props.Create(() => new TransactionMetaStoreHandler(transactionCoordinatorId, lookup, cnxPool, idGenerator, topic, conf, connectFuture));
        }
        private Task<object> EndTxn(object[] args)
        {
            var txnID = (TxnID)args[0];
            var replyTo = (IActorRef)args[1];
            var action = (TxnAction)args[2];
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"End txn {txnID}, action {action}", txnID, action);
            }
            var callback = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!CanSendRequest(callback))
            {
                return callback.Task;
            }
            var reid = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
            var requestId = reid.Id;
            var cmd = _commands.NewEndTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, action);
            var op = OpForTxnIdCallBack.Create(cmd, callback, _conf, requestId, "NewEndTxn");
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
            {
                pendingRequests.TryAdd(requestId, op);
                _timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
                if (!await CheckStateAndSendRequest(op))
                {
                    pendingRequests.TryRemove(requestId, out _);
                }
            });

            return callback.Task;
        }

        private void HandleEndTxnResponse(CommandEndTxnResponse response)
        {
            var hasError = response.Error != ServerError.UnknownError;
            ServerError? error;
            string message;
            if (hasError)
            {
                error = response.Error;
                message = response.Message;
            }
            else
            {
                error = null;
                message = null;
            }
            var requestId = (long)response.RequestId;
            var txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                pendingRequests.TryRemove(requestId, out OpBase<object> opB);
                var op = (OpForTxnIdCallBack)opB;
                if (op == null)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Got end txn response for timeout {txnID.MostSigBits} - {txnID.LeastSigBits}");
                    }
                    return;
                }
                if (!hasError)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Got end txn response success for request {requestId}");
                    }
                    op.Callback.TrySetResult(null);
                }
                else
                {
                    if (CheckIfNeedRetryByError(error.Value, message, op))
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"Get a response for the {BaseCommand.Type.EndTxn}  request {requestId} error TransactionCoordinatorNotFound and try it again");
                        }
                        pendingRequests.TryAdd(requestId, op);
                        _context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromMilliseconds(op.Backoff.Next()), async () =>
                        {
                            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
                            {
                                if (!pendingRequests.ContainsKey(requestId))
                                {
                                    if (_log.IsDebugEnabled)
                                    {
                                        _log.Debug($"The request {requestId} already timeout");
                                    }
                                    return;
                                }
                                if (!await CheckStateAndSendRequest(op))
                                {
                                    pendingRequests.TryRemove(requestId, out _);
                                }
                            });
                            if (!pendingRequests.ContainsKey(requestId))
                            {
                                if (_log.IsDebugEnabled)
                                {
                                    _log.Debug($"The request {requestId} already timeout");
                                }
                                return;
                            }
                            if (!await CheckStateAndSendRequest(op))
                            {
                                pendingRequests.TryRemove(requestId, out _);
                            }
                        });
                        return;
                    }
                    _log.Error($"Got {BaseCommand.Type.EndTxn} for request {requestId} error {error}");
                }
            });
        }
        private async ValueTask HandleConnectionOpened(IActorRef cnx)
        {
            _log.Info($"Transaction meta handler with transaction coordinator id {_transactionCoordinatorId} connection opened.");
            if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
            {
                _state.ConnectionState = HandlerState.State.Closed;
                FailPendingRequest();
                pendingRequests.Clear();
                return;
            }
            _clientCnx = cnx;
            _connectionHandler.Tell(new SetCnx(cnx));
            cnx.Tell(new RegisterTransactionMetaStoreHandler(_transactionCoordinatorId, _self));
            var protocolVersionResponse = await cnx.Ask<RemoteEndpointProtocolVersionResponse>(RemoteEndpointProtocolVersion.Instance).ConfigureAwait(false);
            var protocolVersion = protocolVersionResponse.Version;
            if (protocolVersion > ((int)ProtocolVersion.V18))
            {
                var reid = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                var requestId = reid.Id;
                var cmd = _commands.NewTcClientConnectRequest(_transactionCoordinatorId, requestId);
                var response = await _clientCnx.Ask<AskResponse>(new Payload(cmd, requestId, "NewTcClientConnectRequest"));
                if (!response.Failed)
                {
                    _log.Info($"Transaction coordinator client connect success! tcId : {_transactionCoordinatorId}");
                    if (!_state.ChangeToReadyState())
                    {
                        _state.ConnectionState = HandlerState.State.Closed;
                        await cnx.GracefulStop(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                        _connectFuture.TrySetResult(null);
                    }

                    if (!_connectFuture.Task.IsCompletedSuccessfully)
                    {
                        _connectFuture.TrySetResult(null);
                    }
                    _connectionHandler.Tell(ResetBackoff.Instance);
                }
                else
                {
                    _log.Error($"Transaction coordinator client connect fail! tcId : {_transactionCoordinatorId}. Cause: {response.Exception}");
                    if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed
                            || response.Exception is PulsarClientException.NotAllowedException)
                    {
                        _state.ConnectionState = HandlerState.State.Closed;
                        await cnx.GracefulStop(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    }
                    else
                    {
                        _connectionHandler.Tell(new ReconnectLater(response.Exception));
                    }
                }
            }
            else
            {
                if (!_state.ChangeToReadyState())
                {
                    await cnx.GracefulStop(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
                    _connectFuture.TrySetResult(null);
                }
            }
        }

        private Task<object> NewTransaction(object[] args)
        {
            var timeout = (long)args[0];
            if (_log.IsDebugEnabled)
            {
                _log.Debug("New transaction with timeout in secs {}", timeout);
            }
            var callback = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!CanSendRequest(callback))
            {
                return callback.Task;
            }
            var reid = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
            var requestId = reid.Id;
            var cmd = _commands.NewTxn(_transactionCoordinatorId, requestId, timeout);
            var op = OpForTxnIdCallBack.Create(cmd, callback, _conf, requestId, "NewTxn");
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
            {
                pendingRequests.TryAdd(requestId, op);
                _timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
                if (!await CheckStateAndSendRequest(op))
                {
                    pendingRequests.TryRemove(requestId, out _);
                }
            });
            return callback.Task;
        }

        private void HandleNewTxnResponse(CommandNewTxnResponse response)
        {
            var hasError = response.Error != ServerError.UnknownError;
            ServerError? error;
            string message;
            if (hasError)
            {
                error = response.Error;
                message = response.Message;
            }
            else
            {
                error = null;
                message = null;
            }
            var txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
            var requestId = (long)response.RequestId;
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                pendingRequests.TryRemove(requestId, out OpBase<object> opB);
                var op = (OpForTxnIdCallBack)opB;
                if (op == null)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Got new txn response for timeout {txnID.MostSigBits} - {txnID.LeastSigBits}");
                    }
                    return;
                }
                if (!hasError)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Got new txn response {txnID} for request {requestId}");
                    }
                    op.Callback.TrySetResult(txnID);
                }
                else
                {
                    if (CheckIfNeedRetryByError(error.Value, message, op))
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"Get a response for the {BaseCommand.Type.NewTxn.GetType().Name}  request {requestId} error TransactionCoordinatorNotFound and try it again");
                        }
                        pendingRequests.TryAdd(requestId, op);
                        _context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromMilliseconds(op.Backoff.Next()), async () =>
                        {
                            if (!pendingRequests.ContainsKey(requestId))
                            {
                                if (_log.IsDebugEnabled)
                                {
                                    _log.Debug($"The request {requestId} already timeout");
                                }
                                return;
                            }
                            if (!await CheckStateAndSendRequest(op))
                            {
                                pendingRequests.TryRemove(requestId, out _);
                            }
                        });
                        return;
                    }
                    _log.Error($"Got {BaseCommand.Type.NewTxn.GetType().Name} for request {requestId} error {error}");
                }
            });
        }
        private bool CheckIfNeedRetryByError<T>(ServerError error, string Message, OpBase<T> op)
        {
            if (error == ServerError.TransactionCoordinatorNotFound)
            {
                if (_state.ConnectionState != HandlerState.State.Connecting)
                {
                    _connectionHandler.Tell(new ReconnectLater(new CoordinatorNotFoundException(Message)));
                }
                return true;
            }

            if (op != null)
            {
                op.Callback.TrySetException(GetExceptionByServerError(error, Message));
            }
            return false;
        }

        public Task<object> AddPublishPartitionToTxn(object[] args)
        {
            var txnID = (TxnID)args[0];
            var partitions = (IList<string>)args[1];
            var replyTo = (IActorRef)args[2];
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Add publish partition {partitions} to txn {txnID}");
            }
            var callback = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!CanSendRequest(callback))
            {
                return callback.Task;
            }
            var reid = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
            var requestId = reid.Id;
            var cmd = _commands.NewAddPartitionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, partitions);
            var op = OpForVoidCallBack.Create(cmd, callback, _conf, requestId, "NewAddPartitionToTxn");
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
            {
                pendingRequests.TryAdd(requestId, op);
                _timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
                if (!await CheckStateAndSendRequest(op))
                {
                    pendingRequests.TryRemove(requestId, out _);
                }
            });
            return callback.Task;
        }

        private void HandleAddPublishPartitionToTxnResponse(CommandAddPartitionToTxnResponse response)
        {
            var hasError = response.Error != ServerError.UnknownError;
            ServerError? error;
            string message;
            if (hasError)
            {
                error = response.Error;
                message = response.Message;
            }
            else
            {
                error = null;
                message = null;
            }
            var requestId = (long)response.RequestId;
            var txnID = new TxnID((long)response.TxnidMostBits, (long)response.TxnidLeastBits);
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                pendingRequests.TryRemove(requestId, out OpBase<object> opB);
                var op = (OpForVoidCallBack)opB;
                if (op == null)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Got add publish partition to txn response for timeout {txnID.MostSigBits} - {txnID.LeastSigBits}");
                    }
                    return;
                }
                if (!hasError)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Add publish partition for request {requestId} success.");
                    }
                    op.Callback.TrySetResult(new RegisterProducedTopicResponse(null));
                }
                else
                {
                    if (CheckIfNeedRetryByError(error.Value, message, op))
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"Get a response for the {BaseCommand.Type.AddPartitionToTxn} request {requestId} error TransactionCoordinatorNotFound and try it again");
                        }
                        pendingRequests.TryAdd(requestId, op);
                        _context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromMilliseconds(op.Backoff.Next()), () =>
                        {
                            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
                            {
                                if (!pendingRequests.ContainsKey(requestId))
                                {
                                    if (_log.IsDebugEnabled)
                                    {
                                        _log.Debug($"The request {requestId} already timeout");
                                    }
                                    return;
                                }
                                if (!await CheckStateAndSendRequest(op))
                                {
                                    pendingRequests.TryRemove(requestId, out _);
                                }
                            });
                        });
                        return;
                    }
                    else
                        op.Callback.TrySetResult(new RegisterProducedTopicResponse(error.Value));

                    _log.Error($"{BaseCommand.Type.AddPartitionToTxn} for request {requestId} error {error} with txnID {txnID}.");
                }
            });
        }

        private Task<object> AddSubscriptionToTxn(object[] args)
        {
            var txnID = (TxnID)args[0];
            var subscriptionList = (IList<Subscription>)args[1];
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Add subscription {subscriptionList} to txn {txnID}.");
            }
            var callback = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!CanSendRequest(callback))
            {
                return callback.Task;
            }
            var reid = _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance).GetAwaiter().GetResult();
            var requestId = reid.Id;
            var cmd = _commands.NewAddSubscriptionToTxn(requestId, txnID.LeastSigBits, txnID.MostSigBits, subscriptionList);
            var op = OpForVoidCallBack.Create(cmd, callback, _conf, requestId, "NewAddSubscriptionToTxn");
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
            {
                pendingRequests.TryAdd(requestId, op);
                _timeoutQueue.Enqueue(new RequestTime(DateTimeHelper.CurrentUnixTimeMillis(), requestId));
                if (!await CheckStateAndSendRequest(op))
                {
                    pendingRequests.TryRemove(requestId, out _);
                }
            });
            return callback.Task;
        }
        private void HandleAddSubscriptionToTxnResponse(CommandAddSubscriptionToTxnResponse response)
        {
            var hasError = response.Error != ServerError.UnknownError;
            ServerError? error;
            string message;
            if (hasError)
            {
                error = response.Error;
                message = response.Message;
            }
            else
            {
                error = null;
                message = null;
            }
            var requestId = (long)response.RequestId;
            Akka.Dispatch.ActorTaskScheduler.RunTask(() =>
            {
                pendingRequests.TryRemove(requestId, out OpBase<object> opB);
                var op = (OpForVoidCallBack)opB;
                if (op == null)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Add subscription to txn timeout for request {requestId}.");
                    }
                    return;
                }
                if (!hasError)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"Add subscription to txn success for request {requestId}.");
                    }
                    op.Callback.TrySetResult(null);
                }
                else
                {
                    _log.Error($"Add subscription to txn failed for request {requestId} error {error}.");
                    if (CheckIfNeedRetryByError(error.Value, message, op))
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug($"Get a response for {BaseCommand.Type.AddSubscriptionToTxn} request {error} error TransactionCoordinatorNotFound and try it again");
                        }
                        pendingRequests.TryAdd(requestId, op);
                        _context.System.Scheduler.Advanced.ScheduleOnce(TimeSpan.FromMilliseconds(op.Backoff.Next()), () =>
                        {
                            Akka.Dispatch.ActorTaskScheduler.RunTask(async () =>
                            {
                                if (!pendingRequests.ContainsKey(requestId))
                                {
                                    if (_log.IsDebugEnabled)
                                    {
                                        _log.Debug($"The request {requestId} already timeout");
                                    }
                                    return;
                                }
                                if (!await CheckStateAndSendRequest(op))
                                {
                                    pendingRequests.TryRemove(requestId, out _);
                                }
                            });
                        });
                        return;
                    }
                    _log.Error($"{BaseCommand.Type.AddSubscriptionToTxn} failed for request {requestId} error {error}.");
                }
            });
        }

        private bool CanSendRequest<T>(TaskCompletionSource<T> callback)
        {
            /*try
            {
                if (_blockIfReachMaxPendingOps)
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
                Callback.completeExceptionally(TransactionCoordinatorClientException.Unwrap(E));
                return false;
            }*/
            return true;
        }
        private async Task<bool> CheckStateAndSendRequest(OpBase<object> op)
        {
            switch (_state.ConnectionState)
            {
                case HandlerState.State.Ready:
                    if (_clientCnx != null)
                    {
                        var response = await _clientCnx.Ask<object>(new Payload(op.Cmd, op.RequestId, op.Method));
                        switch (response)
                        {
                            case NewTxnResponse newTxn:
                                try
                                {
                                    HandleNewTxnResponse(newTxn.Response);
                                    var result = op.Callback.Task.Result;
                                    _replyTo.Tell(new AskResponse(result));
                                }
                                catch (Exception ex)
                                {
                                    _replyTo.Tell(new AskResponse(ex));
                                }
                                break;
                            case AddSubscriptionToTxnResponse subRes:
                                try
                                {
                                    HandleAddSubscriptionToTxnResponse(subRes.Response);
                                    var result = op.Callback.Task.Result;
                                    _replyTo.Tell(new AskResponse(true));
                                }
                                catch (Exception ex)
                                {
                                    _replyTo.Tell(new AskResponse(new PulsarClientException(ex.ToString())));
                                }
                                break;
                            case AddPublishPartitionToTxnResponse pubRes:
                                try
                                {
                                    HandleAddPublishPartitionToTxnResponse(pubRes.Response);
                                    var result = op.Callback.Task.Result;
                                    _replyTo.Tell(new RegisterProducedTopicResponse(null));
                                }
                                catch
                                {
                                    _replyTo.Tell(new RegisterProducedTopicResponse(ServerError.UnknownError));
                                }
                                break;
                            case EndTxnResponse endRes:
                                try
                                {
                                    HandleEndTxnResponse(endRes.Response);
                                    var result = op.Callback.Task.Result;
                                    _replyTo.Tell(new EndTxnResponse(NoException.Instance));
                                }
                                catch (Exception ex)
                                {
                                    _replyTo.Tell(new EndTxnResponse(new TransactionCoordinatorClientException(ex)));
                                }
                                break;
                            default:
                                break;
                        }

                        OnResponse(op);
                    }
                    else
                    {
                        _log.Error("The cnx was null when the TC handler was ready", new NullReferenceException());
                    }
                    return true;
                case HandlerState.State.Connecting:
                    return true;
                case HandlerState.State.Closing:
                case HandlerState.State.Closed:
                    op.Callback.TrySetException(new MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " is closing or closed."));
                    OnResponse(op);
                    return false;
                case HandlerState.State.Failed:
                case HandlerState.State.Uninitialized:
                    op.Callback.TrySetException(new MetaStoreHandlerNotReadyException("Transaction meta store handler for tcId " + _transactionCoordinatorId + " not connected."));
                    OnResponse(op);
                    return false;
                default:
                    op.Callback.TrySetException(new MetaStoreHandlerNotReadyException(_transactionCoordinatorId));
                    OnResponse(op);
                    return false;
            }
        }
        private void OnResponse<T>(OpBase<T> op)
        {
            op.Recycle();
        }

        private void FailPendingRequest()
        {
            pendingRequests.Keys.ForEach(k =>
            {
                pendingRequests.TryRemove(k, out OpBase<object> op);
                if (op != null && !op.Callback.Task.IsCompleted)
                {
                    op.Callback.TrySetException(new PulsarClientException.AlreadyClosedException("Could not get response from transaction meta store when " + "the transaction meta store has already close."));
                    OnResponse(op);
                }
            });
        }

        private TransactionCoordinatorClientException GetExceptionByServerError(ServerError serverError, string msg)
        {
            switch (serverError)
            {
                case ServerError.TransactionCoordinatorNotFound:
                    return new CoordinatorNotFoundException(msg);
                case ServerError.InvalidTxnStatus:
                    return new InvalidTxnStatusException(msg);
                case ServerError.TransactionNotFound:
                    return new TransactionNotFoundException(msg);
                default:
                    return new TransactionCoordinatorClientException(msg);
            }
        }



        private void RunRequestTime()
        {
            if (_requestTimeout.IsCancellationRequested)
            {
                return;
            }
            TimeSpan timeToWaitMs;
            if (_state.ConnectionState == HandlerState.State.Closing || _state.ConnectionState == HandlerState.State.Closed)
            {
                return;
            }
            RequestTime peeked;
            while (_timeoutQueue.TryPeek(out peeked) && peeked.CreationTimeMs + _conf.OperationTimeout.TotalMilliseconds - DateTimeHelper.CurrentUnixTimeMillis() <= 0)
            {
                if (_timeoutQueue.TryDequeue(out var lastPolled))
                {
                    if (_pendingRequests.Remove(lastPolled.RequestId))
                    {
                        _log.Error(new PulsarClientException.TimeoutException("Could not get response from transaction meta store within given timeout.").ToString());
                        if (_log.IsDebugEnabled)
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

            if (peeked == null)
            {
                timeToWaitMs = _conf.OperationTimeout;
            }
            else
            {
                var diff = (peeked.CreationTimeMs + _conf.OperationTimeout.TotalMilliseconds) - DateTimeHelper.CurrentUnixTimeMillis();
                if (diff <= 0)
                {
                    timeToWaitMs = _conf.OperationTimeout;
                }
                else
                {
                    timeToWaitMs = TimeSpan.FromMilliseconds(diff);
                }
            }
            _requestTimeout = _scheduler.ScheduleTellOnceCancelable(timeToWaitMs, _self, RunRequestTimeout.Instance, Nobody.Instance);
        }

        private void HandleConnectionClosed(IActorRef cnx)
        {
            _connectionHandler.Tell(new ConnectionClosed(cnx));
        }

        internal class RunRequestTimeout
        {
            internal static RunRequestTimeout Instance = new RunRequestTimeout();
        }

        private abstract class OpBase<T>
        {
            protected internal ReadOnlySequence<byte> Cmd;
            protected internal TaskCompletionSource<T> Callback;
            protected internal Backoff Backoff;
            protected internal long RequestId;
            protected internal string Method;

            internal abstract void Recycle();
        }
        private class OpForTxnIdCallBack : OpBase<object>
        {

            internal static OpForTxnIdCallBack Create(ReadOnlySequence<byte> cmd, TaskCompletionSource<object> callback, ClientConfigurationData client, long requestid, string method)
            {
                var Op = new OpForTxnIdCallBack
                {
                    Callback = callback,
                    Cmd = cmd,
                    Backoff = (new BackoffBuilder())
                    .SetInitialTime(TimeSpan.FromMilliseconds(client.InitialBackoffIntervalMs))
                    .SetMax(TimeSpan.FromMilliseconds(client.MaxBackoffIntervalMs / 10))
                    .SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(),
                    RequestId = requestid,
                    Method = method
                };
                return Op;
            }

            internal override void Recycle()
            {
                Backoff = null;
                Cmd = ReadOnlySequence<byte>.Empty;
                Callback = null;
                RequestId = -1;
                Method = string.Empty;
            }
        }
        private class OpForVoidCallBack : OpBase<object>
        {
            internal static OpForVoidCallBack Create(ReadOnlySequence<byte> cmd, TaskCompletionSource<object> callback, ClientConfigurationData client, long requestid, string method)
            {
                var Op = new OpForVoidCallBack
                {
                    Callback = callback,
                    Cmd = cmd,
                    Backoff = (new BackoffBuilder())
                    .SetInitialTime(TimeSpan.FromMilliseconds(client.InitialBackoffIntervalMs))
                    .SetMax(TimeSpan.FromMilliseconds(client.MaxBackoffIntervalMs / 10))
                    .SetMandatoryStop(TimeSpan.FromMilliseconds(0)).Create(),
                    RequestId = requestid,
                    Method = method
                };
                return Op;
            }

            internal override void Recycle()
            {
                Backoff = null;
                Cmd = ReadOnlySequence<byte>.Empty;
                Callback = null;
                RequestId = -1;
                Method = string.Empty;
            }

        }
    }
}