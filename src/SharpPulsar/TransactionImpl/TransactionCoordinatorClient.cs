using Akka.Actor;
using Akka.Event;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Utility;
using System;
using System.Collections.Generic;
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
namespace SharpPulsar.TransactionImpl
{
    /// <summary>
    /// Transaction coordinator client based topic assigned.
    /// </summary>
    internal class TransactionCoordinatorClient : ReceiveActor, IWithUnboundedStash
    {
        private List<IActorRef> _handlers;
        private readonly Dictionary<long, IActorRef> _handlerMap = new Dictionary<long, IActorRef>();
        private readonly ILoggingAdapter _log;
        private long _epoch = 0L;
        private readonly ClientConfigurationData _clientConfigurationData;
        private readonly IActorRef _generator;
        private readonly IActorRef _lookup;
        private readonly IActorRef _cnxPool;
        private IActorRef _replyTo;
        private TransactionCoordinatorClientState _state = TransactionCoordinatorClientState.None;

        public TransactionCoordinatorClient(IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ClientConfigurationData conf, TaskCompletionSource<object> tcs)
        {
            _cnxPool = cnxPool;
            _lookup = lookup;
            _generator = idGenerator;
            _clientConfigurationData = conf;
            _log = Context.GetLogger();
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () => await StartCoordinator(tcs));
            Ready();
        }

        private void Ready()
        {
            ReceiveAsync<NewTxn>(async n =>
            {
                _replyTo = Sender;
                var ask = await NextHandler().Ask<AskResponse>(n, TimeSpan.FromMilliseconds(10000));
                _replyTo.Tell(ask);
            });
            Receive<AddPublishPartitionToTxn>(AddPublishPartitionToTxn);
            Receive<SubscriptionToTxn>(AddSubscriptionToTxn);
            Receive<AbortTxnID>(Abort);
            Receive<CommitTxnID>(Commit);
            //Stash?.UnstashAll();
        }
        private async ValueTask StartCoordinator(TaskCompletionSource<object> tc)
        {
            var retryCount = 0;
            _state = TransactionCoordinatorClientState.Starting;
            var result = await _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(TopicName.TransactionCoordinatorAssign));
            /*while (result.Failed && retryCount < 10)
            {
                _log.Error(result.Exception.ToString());
                _log.Info("Transaction coordinator not started...retrying");
                result = await _lookup.Ask<AskResponse>(new GetPartitionedTopicMetadata(TopicName.TransactionCoordinatorAssign));
                retryCount++;
            }
            */
            if (result.Failed)
            {
                tc.TrySetException(result.Exception);
                return;
            }

            var partitionMeta = result.ConvertTo<PartitionedTopicMetadata>();
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Transaction meta store assign partition is {partitionMeta.Partitions}.");
            }
            var connectFutureList = new List<Task<object>>();
            if (partitionMeta.Partitions > 0)
            {
                _handlers = new List<IActorRef>(partitionMeta.Partitions);
                for (var i = 0; i < partitionMeta.Partitions; i++)
                {
                    try
                    {
                        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                        connectFutureList.Add(tcs.Task);
                        var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(i, _lookup, _cnxPool, _generator, GetTCAssignTopicName(i), _clientConfigurationData, tcs), $"handler_{i}");
                        _handlers.Add(handler);
                        _handlerMap.Add(i, handler);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.ToString());
                        tc.TrySetException(ex);
                        break;
                    }
                }
            }
            else
            {
                _handlers = new List<IActorRef>(1);
                try
                {
                    var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                    connectFutureList.Add(tcs.Task);
                    var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(0, _lookup, _cnxPool, _generator, GetTCAssignTopicName(-1), _clientConfigurationData, tcs), $"handler_{0}");
                    var ask = tcs.Task;
                    _handlers.Add(handler);
                    _handlerMap.Add(0, handler);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.ToString());
                }
            }
            await Task.WhenAll(connectFutureList.ToArray()).ContinueWith(task =>
            {
                if (task.Exception != null)
                    tc.TrySetException(task.Exception);
                else
                    tc.TrySetResult(_handlers.Count);
            });
        }
        public static Props Prop(IActorRef lookup, IActorRef cnxPool, IActorRef idGenerator, ClientConfigurationData conf, TaskCompletionSource<object> tcs)
        {
            return Props.Create(() => new TransactionCoordinatorClient(lookup, cnxPool, idGenerator, conf, tcs));
        }
        private string GetTCAssignTopicName(int partition)
        {
            if (partition >= 0)
            {
                return TopicName.TransactionCoordinatorAssign.ToString() + TopicName.PartitionedTopicSuffix + partition;
            }
            else
            {
                return TopicName.TransactionCoordinatorAssign.ToString();
            }
        }
        
        protected override void PostStop()
        {
            Close();
        }

        private void Close()
        {
            if (State == TransactionCoordinatorClientState.Closing || State == TransactionCoordinatorClientState.Closed)
            {
                _log.Warning("The transaction meta store is closing or closed, doing nothing.");
            }
            else
            {
                if (_handlers != null)
                {
                    foreach (var handler in _handlers)
                    {
                        handler.GracefulStop(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    }
                }
            }
        }

        private void AddPublishPartitionToTxn(AddPublishPartitionToTxn pub)
        {
            if (!_handlerMap.TryGetValue(pub.TxnID.MostSigBits, out var handler))
            {
                _log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(pub.TxnID.MostSigBits).ToString());

                Sender.Tell(new RegisterProducedTopicResponse(Protocol.Proto.ServerError.UnknownError));
            }
            else
                handler.Forward(pub);
        }

        private void AddSubscriptionToTxn(SubscriptionToTxn subToTxn)
        {
            if (!_handlerMap.TryGetValue(subToTxn.TxnID.MostSigBits, out var handler))
            {
                var error = new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(subToTxn.TxnID.MostSigBits);

                _log.Error(error.ToString());

                Sender.Tell(new AskResponse(new PulsarClientException(error.ToString())));
            }
            else
            {
                var sub = new Protocol.Proto.Subscription
                {
                    Topic = subToTxn.Topic,
                    subscription = subToTxn.Subscription,
                };
                handler.Tell(new AddSubscriptionToTxn(subToTxn.TxnID, new List<Protocol.Proto.Subscription> { sub }), Sender);

            }
        }

        private void Commit(CommitTxnID commit)
        {
            if (!_handlerMap.TryGetValue(commit.TxnID.MostSigBits, out var handler))
            {
                _log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(commit.TxnID.MostSigBits).ToString());
                Sender.Tell(new EndTxnResponse(new TransactionCoordinatorClientException("")));
            }
            else
                handler.Forward(commit);
        }

        private void Abort(AbortTxnID abort)
        {
            if (!_handlerMap.TryGetValue(abort.TxnID.MostSigBits, out var handler))
            {
                _log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(abort.TxnID.MostSigBits).ToString());
                Sender.Tell(new EndTxnResponse(new TransactionCoordinatorClientException("")));
            }
            else
                handler.Forward(abort);
        }

        private TransactionCoordinatorClientState State
        {
            get
            {
                return _state;
            }
        }

        public IStash Stash { get; set; }

        private IActorRef NextHandler()
        {
            var index = MathUtils.SignSafeMod(++_epoch, _handlers.Count);
            return _handlers[index];
        }
    }

}