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
namespace org.apache.pulsar.client.impl.transaction
{
	using PulsarClient = org.apache.pulsar.client.api.PulsarClient;
	using TransactionCoordinatorClient = org.apache.pulsar.client.api.transaction.TransactionCoordinatorClient;
	using TransactionCoordinatorClientException = org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException;
	using CoordinatorClientStateException = org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException.CoordinatorClientStateException;
	using MathUtils = org.apache.pulsar.client.util.MathUtils;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using ConcurrentLongHashMap = org.apache.pulsar.common.util.collections.ConcurrentLongHashMap;
	using TxnID = org.apache.pulsar.transaction.impl.common.TxnID;
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

		private static readonly AtomicReferenceFieldUpdater<TransactionCoordinatorClientImpl, State> STATE_UPDATER = AtomicReferenceFieldUpdater.newUpdater(typeof(TransactionCoordinatorClientImpl), typeof(State), "state");
		private volatile State state = State.NONE;

		public TransactionCoordinatorClientImpl(PulsarClient pulsarClient)
		{
			this.pulsarClient = (PulsarClientImpl) pulsarClient;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void start() throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override void start()
		{
			try
			{
				startAsync().get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> startAsync()
		{
			if (STATE_UPDATER.compareAndSet(this, State.NONE, State.STARTING))
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
					for (int i = 0; i < partitionMeta.partitions; i++)
					{
						TransactionMetaStoreHandler handler = new TransactionMetaStoreHandler(i, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString() + TopicName.PARTITIONED_TOPIC_SUFFIX + i);
						handlers[i] = handler;
						handlerMap.put(i, handler);
					}
				}
				else
				{
					handlers = new TransactionMetaStoreHandler[1];
					TransactionMetaStoreHandler handler = new TransactionMetaStoreHandler(0, pulsarClient, TopicName.TRANSACTION_COORDINATOR_ASSIGN.ToString());
					handlers[0] = handler;
					handlerMap.put(0, handler);
				}
				STATE_UPDATER.set(TransactionCoordinatorClientImpl.this, State.READY);
				});
			}
			else
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.CoordinatorClientStateException("Can not start while current state is " + state));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override void close()
		{
			try
			{
				closeAsync().get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> closeAsync()
		{
			CompletableFuture<Void> result = new CompletableFuture<Void>();
			if (State == State.CLOSING || State == State.CLOSED)
			{
				LOG.warn("The transaction meta store is closing or closed, doing nothing.");
				result.complete(null);
			}
			else
			{
				foreach (TransactionMetaStoreHandler handler in handlers)
				{
					try
					{
						handler.Dispose();
					}
					catch (IOException e)
					{
						LOG.warn("Close transaction meta store handler error", e);
					}
				}
				this.handlers = null;
				result.complete(null);
			}
			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.transaction.impl.common.TxnID newTransaction() throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override TxnID newTransaction()
		{
			try
			{
				return newTransactionAsync().get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<TxnID> newTransactionAsync()
		{
			return newTransactionAsync(DEFAULT_TXN_TTL_MS, TimeUnit.MILLISECONDS);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.transaction.impl.common.TxnID newTransaction(long timeout, java.util.concurrent.TimeUnit unit) throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override TxnID newTransaction(long timeout, TimeUnit unit)
		{
			try
			{
				return newTransactionAsync(timeout, unit).get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<TxnID> newTransactionAsync(long timeout, TimeUnit unit)
		{
			return nextHandler().newTransactionAsync(timeout, unit);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void addPublishPartitionToTxn(org.apache.pulsar.transaction.impl.common.TxnID txnID, java.util.List<String> partitions) throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override void addPublishPartitionToTxn(TxnID txnID, IList<string> partitions)
		{
			try
			{
				addPublishPartitionToTxnAsync(txnID, partitions).get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> addPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions)
		{
			TransactionMetaStoreHandler handler = handlerMap.get(txnID.MostSigBits);
			if (handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.addPublishPartitionToTxnAsync(txnID, partitions);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void commit(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override void commit(TxnID txnID)
		{
			try
			{
				commitAsync(txnID).get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> commitAsync(TxnID txnID)
		{
			TransactionMetaStoreHandler handler = handlerMap.get(txnID.MostSigBits);
			if (handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.commitAsync(txnID);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void abort(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws org.apache.pulsar.client.api.transaction.TransactionCoordinatorClientException
		public override void abort(TxnID txnID)
		{
			try
			{
				abortAsync(txnID).get();
			}
			catch (Exception e)
			{
				throw TransactionCoordinatorClientException.unwrap(e);
			}
		}

		public override CompletableFuture<Void> abortAsync(TxnID txnID)
		{
			TransactionMetaStoreHandler handler = handlerMap.get(txnID.MostSigBits);
			if (handler == null)
			{
				return FutureUtil.failedFuture(new TransactionCoordinatorClientException.MetaStoreHandlerNotExistsException(txnID.MostSigBits));
			}
			return handler.abortAsync(txnID);
		}

		public override State State
		{
			get
			{
				return state;
			}
		}

		private TransactionMetaStoreHandler nextHandler()
		{
			int index = MathUtils.signSafeMod(epoch.incrementAndGet(), handlers.Length);
			return handlers[index];
		}
	}

}