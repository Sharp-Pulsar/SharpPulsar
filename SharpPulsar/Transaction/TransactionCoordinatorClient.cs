using Akka.Actor;
using Akka.Event;
using SharpPulsar.Common.Naming;
using SharpPulsar.Configuration;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages.Client;
using SharpPulsar.Model;
using System;
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
namespace SharpPulsar.Transaction
{
	/// <summary>
	/// Transaction coordinator client based topic assigned.
	/// </summary>
	public class TransactionCoordinatorClient : ReceiveActor
	{

		private readonly IActorRef _pulsarClient;
		private List<IActorRef> _handlers;
		private Dictionary<long, IActorRef> _handlerMap = new Dictionary<long, IActorRef>();
		private ILoggingAdapter _log;
		private long _epoch = 0L;
		private ClientConfigurationData _clientConfigurationData;

		private volatile TransactionCoordinatorClientState _state = TransactionCoordinatorClientState.None;

		public TransactionCoordinatorClient(IActorRef pulsarClient, ClientConfigurationData conf)
		{
			_clientConfigurationData = conf;
			_log = Context.GetLogger();
			_pulsarClient = pulsarClient;
		}

		public virtual void Start()
		{
			try
			{
				StartCoordination();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		private void StartCoordination()
		{
			_state = TransactionCoordinatorClientState.Starting;
			_pulsarClient.Ask<LookupDataResult>(new GetPartitionedTopicMetadata(TopicName.TransactionCoordinatorAssign))
				.ContinueWith(task =>
			{
                if (!task.IsFaulted)
                {
					var partitionMeta = task.Result;
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"Transaction meta store assign partition is {partitionMeta.Partitions}.");
					}
					if (partitionMeta.Partitions > 0)
					{
						_handlers = new List<IActorRef>(partitionMeta.Partitions);
						for (int i = 0; i < partitionMeta.Partitions; i++)
						{
							var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(i, _pulsarClient, GetTCAssignTopicName(i), _clientConfigurationData), $"handler_{i}");
							_handlers[i] = handler;
							_handlerMap.Add(i, handler);
						}
					}
					else
					{
						_handlers = new List<IActorRef>(1); 
						var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(0, _pulsarClient, GetTCAssignTopicName(-1), _clientConfigurationData), $"handler_{0}");
						_handlers[0] = handler;
						_handlerMap.Add(0, handler);
					}
					_state = TransactionCoordinatorClientState.Ready;
				}
				
			});

		}

		private string GetTCAssignTopicName(int partition)
		{
			if(partition >= 0)
			{
				return TopicName.TransactionCoordinatorAssign.ToString() + TopicName.PartitionedTopicSuffix + partition;
			}
			else
			{
				return TopicName.TransactionCoordinatorAssign.ToString();
			}
		}
		public virtual void Dispose()
		{
			try
			{
				CloseAsync().get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<Void> CloseAsync()
		{
			CompletableFuture<Void> result = new CompletableFuture<Void>();
			if(State == Org.Apache.Pulsar.Client.Api.Transaction.TransactionCoordinatorClientState.CLOSING || State == Org.Apache.Pulsar.Client.Api.Transaction.TransactionCoordinatorClientState.CLOSED)
			{
				_lOG.warn("The transaction meta store is closing or closed, doing nothing.");
				result.complete(null);
			}
			else
			{
				foreach(TransactionMetaStoreHandler handler in _handlers)
				{
					try
					{
						handler.Dispose();
					}
					catch(IOException e)
					{
						_lOG.warn("Close transaction meta store handler error", e);
					}
				}
				this._handlers = null;
				result.complete(null);
			}
			return result;
		}

		public virtual TxnID NewTransaction()
		{
			try
			{
				return NewTransactionAsync().get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<TxnID> NewTransactionAsync()
		{
			return NewTransactionAsync(DEFAULT_TXN_TTL_MS, TimeUnit.MILLISECONDS);
		}

		public virtual TxnID NewTransaction(long timeout, TimeUnit unit)
		{
			try
			{
				return NewTransactionAsync(timeout, unit).get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<TxnID> NewTransactionAsync(long timeout, TimeUnit unit)
		{
			return NextHandler().NewTransactionAsync(timeout, unit);
		}

		public virtual void AddPublishPartitionToTxn(TxnID txnID, IList<string> partitions)
		{
			try
			{
				AddPublishPartitionToTxnAsync(txnID, partitions).get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<Void> AddPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions)
		{
			TransactionMetaStoreHandler handler = _handlerMap.Get(txnID.MostSigBits);
			if(handler == null)
			{
				return FutureUtil.FailedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.AddPublishPartitionToTxnAsync(txnID, partitions);
		}

		public virtual void AddSubscriptionToTxn(TxnID txnID, string topic, string subscription)
		{
			try
			{
				AddSubscriptionToTxnAsync(txnID, topic, subscription).get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<Void> AddSubscriptionToTxnAsync(TxnID txnID, string topic, string subscription)
		{
			TransactionMetaStoreHandler handler = _handlerMap.Get(txnID.MostSigBits);
			if(handler == null)
			{
				return FutureUtil.FailedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			PulsarApi.Subscription sub = PulsarApi.Subscription.NewBuilder().setTopic(topic).setSubscription(subscription).Build();
			return handler.AddSubscriptionToTxn(txnID, Collections.singletonList(sub));
		}

		public virtual void Commit(TxnID txnID, IList<MessageId> messageIdList)
		{
			try
			{
				CommitAsync(txnID, messageIdList).get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<Void> CommitAsync(TxnID txnID, IList<MessageId> messageIdList)
		{
			TransactionMetaStoreHandler handler = _handlerMap.Get(txnID.MostSigBits);
			if(handler == null)
			{
				return FutureUtil.FailedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.CommitAsync(txnID, messageIdList);
		}

		public virtual void Abort(TxnID txnID, IList<MessageId> messageIdList)
		{
			try
			{
				AbortAsync(txnID, messageIdList).get();
			}
			catch(Exception e)
			{
				throw TransactionCoordinatorClientException.Unwrap(e);
			}
		}

		public virtual CompletableFuture<Void> AbortAsync(TxnID txnID, IList<MessageId> messageIdList)
		{
			TransactionMetaStoreHandler handler = _handlerMap.Get(txnID.MostSigBits);
			if(handler == null)
			{
				return FutureUtil.FailedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.AbortAsync(txnID, messageIdList);
		}

		public virtual Org.Apache.Pulsar.Client.Api.Transaction.TransactionCoordinatorClientState State
		{
			get
			{
				return _state;
			}
		}

		private TransactionMetaStoreHandler NextHandler()
		{
			int index = MathUtils.SignSafeMod(_epoch.incrementAndGet(), _handlers.Length);
			return _handlers[index];
		}
	}

}