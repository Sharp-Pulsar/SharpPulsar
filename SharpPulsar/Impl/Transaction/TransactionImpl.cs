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
	using Data = lombok.Data;
	using Getter = lombok.Getter;
	using MessageId = SharpPulsar.Api.MessageId;
	using Transaction = SharpPulsar.Api.Transaction.Transaction;
	using FutureUtil = Org.Apache.Pulsar.Common.Util.FutureUtil;

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
//ORIGINAL LINE: @Getter public class TransactionImpl implements SharpPulsar.api.transaction.Transaction
	public class TransactionImpl : Transaction
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class TransactionalSendOp
		public class TransactionalSendOp
		{
			internal readonly CompletableFuture<MessageId> SendFuture;
			internal readonly CompletableFuture<MessageId> TransactionalSendFuture;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Data private static class TransactionalAckOp
		public class TransactionalAckOp
		{
			internal readonly CompletableFuture<Void> AckFuture;
			internal readonly CompletableFuture<Void> TransactionalAckFuture;
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

		public TransactionImpl(PulsarClientImpl Client, long TransactionTimeoutMs, long TxnIdLeastBits, long TxnIdMostBits)
		{
			this.client = Client;
			this.transactionTimeoutMs = TransactionTimeoutMs;
			this.txnIdLeastBits = TxnIdLeastBits;
			this.txnIdMostBits = TxnIdMostBits;
			this.sendOps = new LinkedHashMap<long, TransactionalSendOp>();
			this.producedTopics = new HashSet<string>();
			this.ackOps = new HashSet<TransactionalAckOp>();
			this.ackedTopics = new HashSet<string>();
		}

		public virtual long NextSequenceId()
		{
			return sequenceId.AndIncrement;
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterProducedTopic(string Topic)
		{
			lock (this)
			{
				if (producedTopics.Add(Topic))
				{
					// TODO: we need to issue the request to TC to register the produced topic
				}
			}
		}

		public virtual CompletableFuture<MessageId> RegisterSendOp(long SequenceId, CompletableFuture<MessageId> SendFuture)
		{
			lock (this)
			{
				CompletableFuture<MessageId> TransactionalSendFuture = new CompletableFuture<MessageId>();
				TransactionalSendOp SendOp = new TransactionalSendOp(SendFuture, TransactionalSendFuture);
				sendOps.put(SequenceId, SendOp);
				return TransactionalSendFuture;
			}
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterAckedTopic(string Topic)
		{
			lock (this)
			{
				if (ackedTopics.Add(Topic))
				{
					// TODO: we need to issue the request to TC to register the acked topic
				}
			}
		}

		public virtual CompletableFuture<Void> RegisterAckOp(CompletableFuture<Void> AckFuture)
		{
			lock (this)
			{
				CompletableFuture<Void> TransactionalAckFuture = new CompletableFuture<Void>();
				TransactionalAckOp AckOp = new TransactionalAckOp(AckFuture, TransactionalAckFuture);
				ackOps.Add(AckOp);
				return TransactionalAckFuture;
			}
		}

		public override CompletableFuture<Void> Commit()
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}

		public override CompletableFuture<Void> Abort()
		{
			return FutureUtil.failedFuture(new System.NotSupportedException("Not Implemented Yet"));
		}
	}

}