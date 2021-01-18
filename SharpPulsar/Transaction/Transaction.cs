using Akka.Actor;
using Akka.Event;
using SharpPulsar.Interfaces;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Transaction;
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
namespace SharpPulsar.Transaction
{
    /// <summary>
    /// The default implementation of <seealso cref="SharpPulsar.Transaction"/>.
    /// 
    /// <para>All the error handling and retry logic are handled by this class.
    /// The original pulsar client doesn't handle any transaction logic. It is only responsible
    /// for sending the messages and acknowledgements carrying the transaction id and retrying on
    /// failures. This decouples the transactional operations from non-transactional operations as
    /// much as possible.
    /// </para>
    /// </summary>
    public class Transaction : ReceiveActor
	{

		private readonly IActorRef _client;
		private readonly long _transactionTimeoutMs;
		private readonly long _txnIdLeastBits;
		private readonly long _txnIdMostBits;
		private readonly ILoggingAdapter _log;
		private long _sequenceId = 0L;

		private readonly ISet<string> _producedTopics;
		private readonly ISet<string> _ackedTopics;
		private IActorRef _tcClient; //TransactionCoordinatorClientImpl
		private IDictionary<IActorRef, int> _cumulativeAckConsumers;

		private readonly List<IMessageId> _sendList;

		internal Transaction(IActorRef client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
		{
			_log = Context.System.Log;
			_client = client;
			_transactionTimeoutMs = transactionTimeoutMs;
			_txnIdLeastBits = txnIdLeastBits;
			_txnIdMostBits = txnIdMostBits;

			_producedTopics = new HashSet<string>();
			_ackedTopics = new HashSet<string>();
			client.Ask<TcClient>(GetTcClient.Instance).ContinueWith(task => 
			{
                if (!task.IsFaulted)
                {
					var actor = task.Result.TCClient;
					_log.Info($"Successfully Asked {actor.Path.Name} TC from Client Actor");
					_tcClient = actor;
                }
				else
					_log.Error($"Error while Asking for TC from Client Actor: {task.Exception}");
			});
			Receive<NextSequenceId>(_ =>
			{
				Sender.Tell(NextSequenceId());
			});
			Receive<GetTxnIdLeastBits>(_ =>
			{
				Sender.Tell(_txnIdLeastBits);
			});
			Receive<GetTxnIdMostBits>(_ =>
			{
				Sender.Tell(_txnIdMostBits);
			});
			Receive<Abort>(_ =>
			{
				Abort();
			});
			Receive<Commit>(_ =>
			{
				Commit();
			});
			Receive<RegisterSendOp>(s =>
			{
				RegisterSendOp(s.MessageId);
			});
			_sendList = new List<IMessageId>();
		}
		public static Props Prop(IActorRef client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
        {
			return Props.Create(() => new Transaction(client, transactionTimeoutMs, txnIdLeastBits, txnIdMostBits));
        }
		private long NextSequenceId()
		{
			return _sequenceId++;
		}

		// register the topics that will be modified by this transaction
		private void RegisterProducedTopic(string topic)
		{
			if (_producedTopics.Add(topic))
			{
				// we need to issue the request to TC to register the produced topic
				_tcClient.Tell(new AddPublishPartitionToTxn(new TxnID(_txnIdMostBits, _txnIdLeastBits), new List<string> { topic }));
			}
		}

		private void RegisterSendOp(IMessageId send)
		{
			_sendList.Add(send);
		}

		// register the topics that will be modified by this transaction
		private void RegisterAckedTopic(string topic, string subscription)
		{
			if (_ackedTopics.Add(topic))
			{
				// we need to issue the request to TC to register the acked topic
				_tcClient.Tell(new SubscriptionToTxn(new TxnID(_txnIdMostBits, _txnIdLeastBits), topic, subscription));
			}
		}

		public virtual void RegisterAckOp(CompletableFuture<Void> ackFuture)
		{
			lock(this)
			{
				_ackFutureList.Add(ackFuture);
			}
		}

		private void RegisterCumulativeAckConsumer(IActorRef consumer)
		{
			if (_cumulativeAckConsumers == null)
			{
				_cumulativeAckConsumers = new Dictionary<IActorRef, int>();
			}
			_cumulativeAckConsumers[consumer] = 0;
		}

		private void Commit()
		{
			IList<IMessageId> sendMessageIdList = new List<IMessageId>(_sendList.Count);
			foreach (var msgid in _sendList)
			{
				sendMessageIdList.Add(msgid);
			}
			_tcClient.Tell(new Commit(new TxnID(_txnIdMostBits, _txnIdLeastBits), sendMessageIdList));
		}

		private void Abort()
		{
			IList<IMessageId> sendMessageIdList = new List<IMessageId>(_sendList.Count);
			CompletableFuture<Void> abortFuture = new CompletableFuture<Void>();
			AllOpComplete().whenComplete((v, e) =>
			{
					if(e != null)
					{
						log.error(e.Message);
					}
					foreach(CompletableFuture<MessageId> future in _sendFutureList)
					{
						future.thenAccept(sendMessageIdList.add);
					}
					if(_cumulativeAckConsumers != null)
					{
						_cumulativeAckConsumers.forEach((consumer, integer) => _cumulativeAckConsumers.putIfAbsent(consumer, consumer.clearIncomingMessagesAndGetMessageNumber()));
					}
					_tcClient.AbortAsync(new TxnID(_txnIdMostBits, _txnIdLeastBits), sendMessageIdList).whenComplete((vx, ex) =>
					{
						if(_cumulativeAckConsumers != null)
						{
							_cumulativeAckConsumers.forEach(ConsumerImpl.increaseAvailablePermits);
							_cumulativeAckConsumers.Clear();
						}
						if(ex != null)
						{
							abortFuture.completeExceptionally(ex);
						}
						else
						{
							abortFuture.complete(null);
						}
					});
			});

			return abortFuture;
		}

		private CompletableFuture<Void> AllOpComplete()
		{
			IList<CompletableFuture<object>> futureList = new List<CompletableFuture<object>>();
			((List<CompletableFuture<object>>)futureList).AddRange(_sendFutureList);
			((List<CompletableFuture<object>>)futureList).AddRange(_ackFutureList);
			return CompletableFuture.allOf(((List<CompletableFuture<object>>)futureList).ToArray());
		}

    }

}