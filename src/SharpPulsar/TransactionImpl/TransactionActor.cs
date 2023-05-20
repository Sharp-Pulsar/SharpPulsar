using Akka.Actor;
using Akka.Util.Internal;
using SharpPulsar.Interfaces;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Messages.Transaction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SharpPulsar.Exceptions.TransactionCoordinatorClientException;

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
    /// The default implementation of <seealso cref="Transaction"/>.
    /// 
    /// <para>All the error handling and retry logic are handled by this class.
    /// The original pulsar client doesn't handle any transaction logic. It is only responsible
    /// for sending the messages and acknowledgements carrying the transaction id and retrying on
    /// failures. This decouples the transactional operations from non-transactional operations as
    /// much as possible.
    /// </para>
    /// </summary>
    internal class TransactionActor : ReceiveActor, IWithUnboundedStash
    {
        private long _opCount = 0L;
        private readonly IActorRef _client;
        private readonly long _transactionTimeoutMs;
        private readonly long _txnIdLeastBits;
        private readonly long _txnIdMostBits;
        private readonly ILoggingAdapter _log;
        private long _sequenceId = 0L;
        private TransactionState _state;
        private readonly IActorRef _self;
        private IActorRef _sender;
        private bool _hasOpsFailed = false;
        private readonly ISet<string> _registerPartitionMaps;
        private readonly Dictionary<string, List<string>> _registerSubscriptionMap;
        private IActorRef _tcClient; //TransactionCoordinatorClientImpl
        private IDictionary<IActorRef, int> _cumulativeAckConsumers;
        private readonly List<IMessageId> _sendList;
        private ICancelable _timeout = null;
        public IStash Stash { get; set; }

        public TransactionActor(IActorRef client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
        {
            _self = Self;
            _state = TransactionState.OPEN;
            _log = Context.System.Log;
            _client = client;
            _transactionTimeoutMs = transactionTimeoutMs;
            _txnIdLeastBits = txnIdLeastBits;
            _txnIdMostBits = txnIdMostBits;
            _sendList = new List<IMessageId>();
            _registerPartitionMaps = new HashSet<string>();
            _registerSubscriptionMap = new Dictionary<string, List<string>>();
            TcClient();
        }
        private void TcClient()
        {
            Receive<TcClient>(tc =>
            {
                var actor = tc.TCClient;
                _log.Info($"Successfully Asked {actor.Path.Name} TC from Client Actor");
                _tcClient = actor;
                _sender.Tell(TcClientOk.Instance);
                Become(Ready);
            });
            Receive<GetTcClient>(gtc =>
            {
                _sender = Sender;
                _client.Tell(gtc);
            });
            ReceiveAny(_ => Stash.Stash());

        }
        private void Ready()
        {
            Receive<NextSequenceId>(_ =>
            {
                Sender.Tell(NextSequenceId());
            });
            Receive<RegisterCumulativeAckConsumer>(r =>
            {
                RegisterCumulativeAckConsumer(r.Consumer);
            });
            Receive<GetTxnIdBits>(_ =>
            {
                Sender.Tell(new GetTxnIdBitsResponse(_txnIdMostBits, _txnIdLeastBits));
            });
            Receive<Abort>(_ =>
            {
                if (CheckIfOpen())
                {
                    _sender = Sender;
                    Become(Abort);
                }
                else
                    Sender.Tell(new TransactionNotOpenedException(_state.ToString()));
            });
            Receive<RegisterAckedTopic>(r =>
            {
                RegisterAckedTopic(r.Topic, r.Subscription);
            });
            Receive<IncomingMessagesCleared>(c =>
            {
                _cumulativeAckConsumers[Sender] = c.Cleared;
            });
            Receive<TransState>(_ =>
            {
                Sender.Tell(_state);
            });
            Receive<Commit>(_ =>
            {
                if (CheckIfOpen())
                {
                    _sender = Sender;
                    Become(Commit);
                }
                else
                    Sender.Tell(new TransactionNotOpenedException(_state.ToString()));
            });
            Receive<RegisterSendOp>(s =>
            {
                _opCount++;
                if (s.MessageId == null)
                {
                    _log.Error($"The transaction [{_txnIdMostBits}:{_txnIdLeastBits}] get an exception when send messages.");
                    if (!_hasOpsFailed)
                    {
                        _hasOpsFailed = true;
                    }
                    _opCount--;

                }
            });
            Receive<RegisterAckOp>(s =>
            {
                _opCount++;
                s.Task.Task.ContinueWith(t =>
                {
                    if(t.Exception != null)
                    {
                        _log.Error($"The transaction [{_txnIdMostBits}:{_txnIdLeastBits}] get an exception when ack messages. {t.Exception}");
                        if (!_hasOpsFailed)
                        {
                            _hasOpsFailed = true;
                        }
                        _opCount--;
                    }
                });
            });
            Receive<RegisterProducedTopic>(p =>
            {
                RegisterProducedTopic(p.Topic);

            });
            Stash?.UnstashAll();
        }
        public static Props Prop(IActorRef client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
        {
            return Props.Create(() => new TransactionActor(client, transactionTimeoutMs, txnIdLeastBits, txnIdMostBits));
        }
        private long NextSequenceId()
        {
            return _sequenceId++;
        }

        // register the topics that will be modified by this transaction
        private void RegisterProducedTopic(string topic)
        {
            if (CheckIfOpen())
            {
                if (!_registerPartitionMaps.Contains(topic))
                {
                    // we need to issue the request to TC to register the produced topic
                    _tcClient.Forward(new AddPublishPartitionToTxn(new TxnID(_txnIdMostBits, _txnIdLeastBits), new List<string> { topic }));
                    _registerPartitionMaps.Add(topic);
                }
                else
                    Sender.Tell(new RegisterProducedTopicResponse(Protocol.Proto.ServerError.UnknownError));
            }
            else
                Sender.Tell(new RegisterProducedTopicResponse(null));
        }

        // register the topics that will be modified by this transaction
        private void RegisterAckedTopic(string topic, string subscription)
        {
            if (CheckIfOpen())
            {
                if (!_registerSubscriptionMap.TryGetValue(topic, out var subs))
                {
                    _registerSubscriptionMap.Add(topic, new List<string> { subscription });
                    _tcClient.Tell(new SubscriptionToTxn(new TxnID(_txnIdMostBits, _txnIdLeastBits), topic, subscription), Sender);
                }
                else if (!subs.Contains(subscription))
                {
                    _registerSubscriptionMap[topic].Add(subscription);
                    // we need to issue the request to TC to register the acked topic
                    _tcClient.Tell(new SubscriptionToTxn(new TxnID(_txnIdMostBits, _txnIdLeastBits), topic, subscription), Sender);
                }
                else
                    Sender.Tell(new AskResponse(true));
            }
            else
                Sender.Tell(new AskResponse(false));
        }

        private void RegisterCumulativeAckConsumer(IActorRef consumer)
        {
            if (CheckIfOpen())
            {
                if (_cumulativeAckConsumers == null)
                {
                    _cumulativeAckConsumers = new Dictionary<IActorRef, int>();
                }
                _cumulativeAckConsumers[consumer] = 0;
            }
        }

        private void Commit()
        {
            _state = TransactionState.COMMITTING;
            Receive<EndTxnResponse>(e =>
            {
                _log.Info("Got EndTxnResponse in Commit()");
                if (e.Error != null)
                {
                    var error = e.Error;
                    if (error is TransactionNotFoundException || error is InvalidTxnStatusException)
                    {
                        _state = TransactionState.ERROR;
                        _sender.Tell(error);
                    }
                    else
                    {
                        _state = TransactionState.COMMITTED;
                        _sender.Tell(NoException.Instance);
                    }
                }
                else
                    _sender.Tell(NoException.Instance);

                Become(Ready);
            });
            ReceiveAny(any => Stash.Stash());
            _tcClient.Tell(new CommitTxnID(new TxnID(_txnIdMostBits, _txnIdLeastBits)), Self);
        }

        private void Abort()
        {
            _state = TransactionState.ABORTING;
            Receive<EndTxnResponse>(e =>
            {
                _log.Info("Got EndTxnResponse in Commit()");
                CheckState(TransactionState.OPEN, TransactionState.ABORTING);
                if (_cumulativeAckConsumers != null)
                {
                    _cumulativeAckConsumers.ForEach(x => x.Key.Tell(new IncreaseAvailablePermits(1)));
                    _cumulativeAckConsumers.Clear();
                }
                if (e.Error != null)
                {
                    var error = e.Error;
                    if (error is TransactionNotFoundException || error is InvalidTxnStatusException)
                    {
                        _state = TransactionState.ERROR;
                        _sender.Tell(error);
                    }
                    else
                    {
                        _state = TransactionState.ABORTED;
                        _sender.Tell(NoException.Instance);
                    }
                }
                else
                    _sender.Tell(NoException.Instance);
                Become(Ready);
            });
            ReceiveAny(any => Stash.Stash());
            if (_cumulativeAckConsumers != null)
            {
                foreach (var c in _cumulativeAckConsumers)
                {
                    c.Key.Tell(ClearIncomingMessagesAndGetMessageNumber.Instance);
                }
            }
            _tcClient.Tell(new AbortTxnID(new TxnID(_txnIdMostBits, _txnIdLeastBits)), Self);

        }
        private bool CheckIfOpen()
        {
            if (_state == TransactionState.OPEN)
                return true;

            _log.Error($"InvalidTxnStatusException({_txnIdMostBits}: {_txnIdLeastBits}] with unexpected state : {_state}, expect OPEN state!");
            return false;
        }

        
        private void CheckState(params TransactionState[] expectedStates)
        {
            var actualState = _state;
            foreach (var expectedState in expectedStates)
            {
                if (actualState == expectedState)
                {
                    return;
                }

            }
        }

        private void InvalidTxnStatusFuture()
        {
            throw new InvalidTxnStatusException("[" + _txnIdMostBits + ":" + _txnIdLeastBits + "] with unexpected state : " + _state.ToString() + ", expect " + TransactionState.OPEN + " state!");
        }


    }
}