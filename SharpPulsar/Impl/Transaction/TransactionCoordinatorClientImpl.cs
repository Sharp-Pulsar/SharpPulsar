using SharpPulsar.Api.Transaction;

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
namespace SharpPulsar.Impl.Transaction
{
	using IPulsarClient = SharpPulsar.Api.IPulsarClient;
	using TransactionCoordinatorClient = SharpPulsar.Api.Transaction.TransactionCoordinatorClient;
	using TransactionCoordinatorClientException = SharpPulsar.Api.Transaction.TransactionCoordinatorClientException;
	using CoordinatorClientStateException = SharpPulsar.Api.Transaction.TransactionCoordinatorClientException.CoordinatorClientStateException;
	using MathUtils = SharpPulsar.Util.MathUtils;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;
	using Org.Apache.Pulsar.Common.Util.Collections;
	using TxnID = Org.Apache.Pulsar.Transaction.Impl.Common.TxnID;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// Transaction coordinator client based topic assigned.
	/// </summary>
	public class TransactionCoordinatorClientImpl : TransactionCoordinatorClient
	{

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(TransactionCoordinatorClientImpl));

		private readonly PulsarClientImpl pulsarClient;
		private TransactionMetaStoreHandler[] handlers;
		private ConcurrentLongHashMap<TransactionMetaStoreHandler> handlerMap = new ConcurrentLongHashMap<TransactionMetaStoreHandler>(16, 1);
		private readonly AtomicLong epoch = new AtomicLong(0);

		private static readonly AtomicReferenceFieldUpdater<TransactionCoordinatorClientImpl, TransactionCoordinatorClientState> STATE_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(TransactionCoordinatorClientImpl), typeof(TransactionCoordinatorClientState), "state");
		private volatile TransactionCoordinatorClientState state = TransactionCoordinatorClientState.NONE;

		public TransactionCoordinatorClientImpl(IPulsarClient PulsarClient)
		{
			this.pulsarClient = (PulsarClientImpl) PulsarClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override void Start()
		{
			try
			{
				StartAsync().get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> StartAsync()
		{
			if (STATE_UPDATER.compareAndSet(this, TransactionCoordinatorClientState.NONE, TransactionCoordinatorClientState.STARTING))
			{
				return pulsarClient.Lookup.getPartitionedTopicMetadata(TopicName.TRANSACTION_COORDINATOR_ASSIGN).thenAccept(partitionMeta =>
				{
				if (LOG.DebugEnabled)
				{
					LOG.debug("Transaction meta store assign partition is {}.", partitionMeta.partitions);
				}
				if (partitionMeta.partitions > 0)
				{
					handlers = new TransactionMetaStoreHandler[partitionMeta.partitions];
					for (int I = 0; I < partitionMeta.partitions; I++)
					{
						TransactionMetaStoreHandler Handler = new TransactionMetaStoreHandler(I, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString() + TopicName.PARTITIONED_TOPIC_SUFFIX + I);
						handlers[I] = Handler;
						handlerMap.Put(I, Handler);
					}
				}
				else
				{
					handlers = new TransactionMetaStoreHandler[1];
					TransactionMetaStoreHandler Handler = new TransactionMetaStoreHandler(0, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString());
					handlers[0] = Handler;
					handlerMap.Put(0, Handler);
				}
				STATE_UPDATER.set(TransactionCoordinatorClientImpl.this, TransactionCoordinatorClientState.READY);
				});
			}
			else
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.CoordinatorClientStateException("Can not start while current state is " + state));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override void Close()
		{
			try
			{
				CloseAsync().get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> CloseAsync()
		{
			CompletableFuture<Void> Result = new CompletableFuture<Void>();
			if (State == TransactionCoordinatorClientState.CLOSING || State == TransactionCoordinatorClientState.CLOSED)
			{
				LOG.warn("The transaction meta store is closing or closed, doing nothing.");
				Result.complete(null);
			}
			else
			{
				foreach (TransactionMetaStoreHandler Handler in handlers)
				{
					try
					{
						Handler.Dispose();
					}
					catch (IOException E)
					{
						LOG.warn("Close transaction meta store handler error", E);
					}
				}
				this.handlers = null;
				Result.complete(null);
			}
			return Result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.transaction.impl.common.TxnID newTransaction() throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override TxnID NewTransaction()
		{
			try
			{
				return NewTransactionAsync().get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<TxnID> NewTransactionAsync()
		{
			return NewTransactionAsync(TransactionCoordinatorClientFields.DefaultTxnTtlMs, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.transaction.impl.common.TxnID newTransaction(long timeout, java.util.concurrent.BAMCIS.Util.Concurrent.TimeUnit unit) throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override TxnID NewTransaction(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			try
			{
				return NewTransactionAsync(Timeout, Unit).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<TxnID> NewTransactionAsync(long Timeout, BAMCIS.Util.Concurrent.TimeUnit Unit)
		{
			return NextHandler().newTransactionAsync(Timeout, Unit);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void addPublishPartitionToTxn(org.apache.pulsar.transaction.impl.common.TxnID txnID, java.util.List<String> partitions) throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override void AddPublishPartitionToTxn(TxnID TxnID, IList<string> Partitions)
		{
			try
			{
				AddPublishPartitionToTxnAsync(TxnID, Partitions).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> AddPublishPartitionToTxnAsync(TxnID TxnID, IList<string> Partitions)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.addPublishPartitionToTxnAsync(TxnID, Partitions);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void commit(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override void Commit(TxnID TxnID)
		{
			try
			{
				CommitAsync(TxnID).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> CommitAsync(TxnID TxnID)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.commitAsync(TxnID);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void abort(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws SharpPulsar.api.transaction.TransactionCoordinatorClientException
		public override void Abort(TxnID TxnID)
		{
			try
			{
				AbortAsync(TxnID).get();
			}
			catch (Exception E)
			{
				throw TransactionCoordinatorClientException.unwrap(E);
			}
		}

		public override CompletableFuture<Void> AbortAsync(TxnID TxnID)
		{
			TransactionMetaStoreHandler Handler = handlerMap.Get(TxnID.MostSigBits);
			if (Handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(TxnID.MostSigBits));
			}
			return Handler.abortAsync(TxnID);
		}

		public virtual TransactionCoordinatorClientState? State
		{
			get
			{
				return state;
			}
		}

		private TransactionMetaStoreHandler NextHandler()
		{
			int Index = MathUtils.signSafeMod(epoch.incrementAndGet(), handlers.Length);
			return handlers[Index];
		}
	}

}