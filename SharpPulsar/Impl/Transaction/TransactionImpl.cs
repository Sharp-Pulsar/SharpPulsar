using SharpPulsar.Api;
using SharpPulsar.Api.Transaction;
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
	/// The default implementation of <seealso cref="ITransaction"/>.
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
		public class TransactionalSendOp
		{
			internal readonly TaskCompletionSource<IMessageId> SendSource;
			internal readonly TaskCompletionSource<IMessageId> TransactionalSendSource;
			public TransactionalSendOp(TaskCompletionSource<IMessageId> sendSource, TaskCompletionSource<IMessageId> transactionalSendSource)
			{
				SendSource = sendSource;
				TransactionalSendSource = transactionalSendSource;
			}
		}

		public class TransactionalAckOp
		{
			internal readonly TaskCompletionSource<Task> AckTask;
			internal readonly TaskCompletionSource<Task> TransactionalAck;
			public TransactionalAckOp(TaskCompletionSource<Task> ack, TaskCompletionSource<Task> trans)
			{
				AckTask = ack;
				TransactionalAck = trans;
			}
		}

		public readonly PulsarClientImpl Client;
		public readonly long TransactionTimeoutMs;
		public readonly long TxnIdLeastBits;
		public readonly long TxnIdMostBits;
		public readonly AtomicLong SequenceId = new AtomicLong(0L);
		public readonly Dictionary<long, TransactionalSendOp> SendOps;
		public readonly ISet<string> ProducedTopics;
		public readonly ISet<TransactionalAckOp> AckOps;
		public readonly ISet<string> AckedTopics;

		public TransactionImpl(PulsarClientImpl client, long transactionTimeoutMs, long txnIdLeastBits, long txnIdMostBits)
		{
			Client = client;
			TransactionTimeoutMs = transactionTimeoutMs;
			TxnIdLeastBits = txnIdLeastBits;
			TxnIdMostBits = txnIdMostBits;
			SendOps = new Dictionary<long, TransactionalSendOp>();
			ProducedTopics = new HashSet<string>();
			AckOps = new HashSet<TransactionalAckOp>();
			AckedTopics = new HashSet<string>();
		}

		public virtual long NextSequenceId()
		{
			return SequenceId.Increment();
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterProducedTopic(string Topic)
		{
			lock (this)
			{
				if (ProducedTopics.Add(Topic))
				{
					// TODO: we need to issue the request to TC to register the produced topic
				}
			}
		}

		public virtual TaskCompletionSource<IMessageId> RegisterSendOp(long sequenceId, TaskCompletionSource<IMessageId> send)
		{
			lock (this)
			{
				var transactional = new TaskCompletionSource<IMessageId>();
				var SendOp = new TransactionalSendOp(send, transactional);
				SendOps.Add(SequenceId, SendOp);
				return transactional;
			}
		}

		// register the topics that will be modified by this transaction
		public virtual void RegisterAckedTopic(string Topic)
		{
			lock (this)
			{
				if (AckedTopics.Add(Topic))
				{
					// TODO: we need to issue the request to TC to register the acked topic
				}
			}
		}

		public virtual TaskCompletionSource<Task> RegisterAckOp(TaskCompletionSource<Task> ack)
		{
			lock (this)
			{
				var transactional = new TaskCompletionSource<Task>();
				var AckOp = new TransactionalAckOp(ack, transactional);
				AckOps.Add(AckOp);
				return transactional;
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