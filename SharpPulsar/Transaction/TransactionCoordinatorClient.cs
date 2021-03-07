using Akka.Actor;
using Akka.Event;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.Transaction;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Transaction;
using SharpPulsar.Model;
using SharpPulsar.Utility;
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

		private IActorRef _pulsarClient;
		private List<IActorRef> _handlers;
		private Dictionary<long, IActorRef> _handlerMap = new Dictionary<long, IActorRef>();
		private ILoggingAdapter _log;
		private long _epoch = 0L;
		private ClientConfigurationData _clientConfigurationData;
		private IActorRef _generator;

		private TransactionCoordinatorClientState _state = TransactionCoordinatorClientState.None;

		public TransactionCoordinatorClient(IActorRef idGenerator, ClientConfigurationData conf)
		{
			_generator = idGenerator;
			_clientConfigurationData = conf;
			_log = Context.GetLogger();
			Receive<NewTxn>(n => {
				var nt = NewTransaction(n);
				Sender.Tell(nt);
			});
			Receive<AddPublishPartitionToTxn>(n => {
				AddPublishPartitionToTxn(n);
			});
			Receive<SubscriptionToTxn>(n => {
				AddSubscriptionToTxn(n);
			});
			Receive<AbortTxnID>(n => {
				Abort(n);
			});
			Receive<CommitTxnID>(n => {
				Commit(n);
			});
			Receive<StartTransactionCoordinatorClient>(m => 
			{
				if (_pulsarClient == null)
					_pulsarClient = m.Client;
				StartCoordinator();
			});
		}

		private void StartCoordinator()
		{
			_state = TransactionCoordinatorClientState.Starting;
			var started = Start();
			while (!started)
			{
				_log.Info("Transaction coordinator not started...retrying");
				started = Start();
			}
			_state = TransactionCoordinatorClientState.Ready;
		}
		private bool Start()
		{
			try
			{
				var result = _pulsarClient.AskFor(new GetPartitionedTopicMetadata(TopicName.TransactionCoordinatorAssign));
				if (result is PartitionedTopicMetadata lkup)
				{

					var partitionMeta = lkup;
					if (_log.IsDebugEnabled)
					{
						_log.Debug($"Transaction meta store assign partition is {partitionMeta.Partitions}.");
					}
					if (partitionMeta.Partitions > 0)
					{
						_handlers = new List<IActorRef>(partitionMeta.Partitions);
						for (int i = 0; i < partitionMeta.Partitions; i++)
						{
							var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(i, _pulsarClient, _generator, GetTCAssignTopicName(i), _clientConfigurationData), $"handler_{i}");
							_handlers.Add(handler);
							_handlerMap.Add(i, handler);
						}
					}
					else
					{
						_handlers = new List<IActorRef>(1);
						var handler = Context.ActorOf(TransactionMetaStoreHandler.Prop(0, _pulsarClient, _generator, GetTCAssignTopicName(-1), _clientConfigurationData), $"handler_{0}");
						_handlers[0] = handler;
						_handlerMap.Add(0, handler);
					}
					return true;
				}
                else
                {
					var ex = result as ClientExceptions;
					_log.Error(ex.ToString());
					return false;
                }
			}
			catch(Exception ex)
            {
				_log.Error(ex.ToString());
				return false;
			}
		}
		public static Props Prop(IActorRef idGenerator, ClientConfigurationData conf)
        {
			return Props.Create(() => new TransactionCoordinatorClient(idGenerator, conf));
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
		
		protected override void PostStop()
        {
			Close();
        }

		private void Close()
		{
			if(State ==TransactionCoordinatorClientState.Closing || State == TransactionCoordinatorClientState.Closed)
			{
				_log.Warning("The transaction meta store is closing or closed, doing nothing.");
			}
			else
			{
				foreach(var handler in _handlers)
				{
					handler.GracefulStop(TimeSpan.FromSeconds(1));
				}
			}
		}

		private NewTxnResponse NewTransaction(NewTxn txn)
		{
			NewTxnResponse txnid = null;
            try
            {
				var next = NextHandler().AskFor<NewTxnResponse>(txn);
				return next;
			}
			catch (Exception ex)
            {
				_log.Error(ex.ToString());
            }
			return txnid;
		}

		private void AddPublishPartitionToTxn(AddPublishPartitionToTxn pub)
		{
			if (!_handlerMap.TryGetValue(pub.TxnID.MostSigBits, out var handler))
			{
				_log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(pub.TxnID.MostSigBits).ToString());
			}
			else
				handler.Tell(pub);
		}

		private void AddSubscriptionToTxn(SubscriptionToTxn subToTxn)
		{
			if (!_handlerMap.TryGetValue(subToTxn.TxnID.MostSigBits, out var handler))
			{
				_log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(subToTxn.TxnID.MostSigBits).ToString());
			}
            else
            {
				var sub = new Protocol.Proto.Subscription
				{
					Topic = subToTxn.Topic,
					subscription = subToTxn.Subscription,
				};
				handler.Tell(new AddSubscriptionToTxn(subToTxn.TxnID, new List<Protocol.Proto.Subscription> { sub }));

			}
		}

		private void Commit(CommitTxnID commit)
		{
			if (!_handlerMap.TryGetValue(commit.TxnID.MostSigBits, out var handler))
			{
				_log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(commit.TxnID.MostSigBits).ToString());
			}
			else
				handler.Tell(commit);
		}

		private void Abort(AbortTxnID abort)
		{
			if(!_handlerMap.TryGetValue(abort.TxnID.MostSigBits, out var handler))
			{
				_log.Error(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(abort.TxnID.MostSigBits).ToString());
			}
			else
				handler.Tell(abort);
		}

		private TransactionCoordinatorClientState State
		{
			get
			{
				return _state;
			}
		}

		private IActorRef NextHandler()
		{
			int index = MathUtils.SignSafeMod(++_epoch, _handlers.Count);
			return _handlers[index];
		}
	}

}