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
namespace SharpPulsar.Impl.transaction
{
	using Data = lombok.Data;
	using Getter = lombok.Getter;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using Transaction = org.apache.pulsar.client.api.transaction.Transaction;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;

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
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter public class TransactionImpl implements org.apache.pulsar.client.api.transaction.Transaction
	public class TransactionImpl : Transaction
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class TransactionalSendOp
		private class TransactionalSendOp
		{
			internal readonly CompletableFuture<MessageId> sendFuture;
			internal readonly CompletableFuture<MessageId> transactionalSendFuture;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class TransactionalAckOp
		private class TransactionalAckOp
		{
			internal readonly CompletableFuture<Void> ackFuture;
			internal readonly CompletableFuture<Void> transactionalAckFuture;
		}

		private readonly PulsarClientImpl client;
		private readonly long transactionTimeoutMs;
		private readonly long txnIdLeastBits;
		private readonly long txnIdMostBits;
		private readonly AtomicLong sequenceId = new AtomicLong(0L);
		private readonly LinkedHashMap<long, TransactionalSendOp> sendOps;
		private readonly ISet<string> producedTopics;
		private readonly ISet<TransactionalAckOp> ackOps;
		private readonly ISet<string> ackedTopics;

		internal TransactionImpl(PulsarClientImpl client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
		{
			this.client = client;
			this.transactionTimeoutMs = transactionTimeoutMs;
			this.txnIdLeastBits = txnIdLeastBits;
			this.txnIdMostBits = txnIdMostBits;
			this.sendOps = new LinkedHashMap<long, TransactionalSendOp>();
			this.producedTopics = new HashSet<string>();
			this.ackOps = new HashSet<TransactionalAckOp>();
			this.ackedTopics = new HashSet<string>();
		}

		public virtual long nextSequenceId()
		{
			return sequenceId.AndIncrement;
		}

		// register the topics that will be modified by this transaction
		public virtual void registerProducedTopic(string topic)
		{
			lock (this)
			{
				if (producedTopics.Add(topic))
				{
					// TODO: we need to issue the request to TC to register the produced topic
				}
			}
		}

		public virtual CompletableFuture<MessageId> registerSendOp(long sequenceId, CompletableFuture<MessageId> sendFuture)
		{
			lock (this)
			{
				CompletableFuture<MessageId> transactionalSendFuture = new CompletableFuture<MessageId>();
				TransactionalSendOp sendOp = new TransactionalSendOp(sendFuture, transactionalSendFuture);
				sendOps.put(sequenceId, sendOp);
				return transactionalSendFuture;
			}
		}

		// register the topics that will be modified by this transaction
		public virtual void registerAckedTopic(string topic)
		{
			lock (this)
			{
				if (ackedTopics.Add(topic))
				{
					// TODO: we need to issue the request to TC to register the acked topic
				}
			}
		}

		public virtual CompletableFuture<Void> registerAckOp(CompletableFuture<Void> ackFuture)
		{
			lock (this)
			{
				CompletableFuture<Void> transactionalAckFuture = new CompletableFuture<Void>();
				TransactionalAckOp ackOp = new TransactionalAckOp(ackFuture, transactionalAckFuture);
				ackOps.Add(ackOp);
				return transactionalAckFuture;
			}
		}

		public override CompletableFuture<Void> commit()
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public override CompletableFuture<Void> abort()
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}
	}

}