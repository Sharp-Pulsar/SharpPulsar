using SharpPulsar.Interface.Message;
using SharpPulsar.Interface.Transaction;
using SharpPulsar.Util.Atomic;
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
namespace SharpPulsar.Impl.Transaction
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
	public class TransactionImpl : ITransaction
	{
		private class TransactionalSendOp
		{
			internal ValueTask<IMessageId> SendAsync;
			internal ValueTask<IMessageId> TransactionalSendAsync;
		}

		private class TransactionalAckOp
		{
			internal ValueTask AckAsync;
			internal ValueTask TransactionalAckAsync;
		}

		private readonly PulsarClientImpl client;
		private readonly long transactionTimeoutMs;
		private readonly long txnIdLeastBits;
		private readonly long txnIdMostBits;
		private readonly AtomicLong sequenceId = new AtomicLong(0L);
		private readonly IDictionary<long, TransactionalSendOp> sendOps;
		private readonly ISet<string> producedTopics;
		private readonly ISet<TransactionalAckOp> ackOps;
		private readonly ISet<string> ackedTopics;

		internal TransactionImpl(PulsarClientImpl client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
		{
			this.client = client;
			this.transactionTimeoutMs = transactionTimeoutMs;
			this.txnIdLeastBits = txnIdLeastBits;
			this.txnIdMostBits = txnIdMostBits;
			this.sendOps = new Dictionary<long, TransactionalSendOp>();
			this.producedTopics = new HashSet<string>();
			this.ackOps = new HashSet<TransactionalAckOp>();
			this.ackedTopics = new HashSet<string>();
		}

		public virtual long NextSequenceId()
		{
			return sequenceId.Increment();
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterProducedTopic(string topic)
		{
			lock (this)
			{
				if (producedTopics.Add(topic))
				{
					// TODO: we need to issue the request to TC to register the produced topic
				}
			}
		}

		public virtual ValueTask<IMessageId> RegisterSendOp(long sequenceId, ValueTask<IMessageId> send)
		{
			lock (this)
			{
				var transactionalSend = new ValueTask<IMessageId>();
				TransactionalSendOp sendOp = new TransactionalSendOp
				{
					SendAsync = send,
					TransactionalSendAsync = transactionalSend
				};
				sendOps.Add(sequenceId, sendOp);
				return transactionalSend;
			}
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterAckedTopic(string topic)
		{
			lock (this)
			{
				if (ackedTopics.Add(topic))
				{
					// TODO: we need to issue the request to TC to register the acked topic
				}
			}
		}

		public virtual ValueTask RegisterAckOp(ValueTask ack)
		{
			lock (this)
			{
				var transactionalAck = new ValueTask();
				TransactionalAckOp ackOp = new TransactionalAckOp
				{
					AckAsync = ack,
					TransactionalAckAsync = transactionalAck
				};
				ackOps.Add(ackOp);
				return transactionalAck;
			}
		}

		public ValueTask Commit()
		{
			return new ValueTask(Task.FromException(new System.NotSupportedException("Not Implemented Yet")));
		}

		public ValueTask Abort()
		{
			return new ValueTask(Task.FromException(new System.NotSupportedException("Not Implemented Yet")));
		}
	}

}