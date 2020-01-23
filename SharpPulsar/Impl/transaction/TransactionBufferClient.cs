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
	/// The transaction buffer client to commit and abort transactions on topics.
	/// </summary>
	public interface TransactionBufferClient
	{

		/// <summary>
		/// Commit the transaction associated with the topic.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="txnIdMostBits"> the most bits of txn id </param>
		/// <param name="txnIdLeastBits"> the least bits of txn id </param>
		/// <returns> the future represents the commit result </returns>
		CompletableFuture<Void> CommitTxnOnTopic(string Topic, long TxnIdMostBits, long TxnIdLeastBits);

		/// <summary>
		/// Abort the transaction associated with the topic.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="txnIdMostBits"> the most bits of txn id </param>
		/// <param name="txnIdLeastBits"> the least bits of txn id </param>
		/// <returns> the future represents the abort result </returns>
		CompletableFuture<Void> AbortTxnOnTopic(string Topic, long TxnIdMostBits, long TxnIdLeastBits);

		/// <summary>
		/// Commit the transaction associated with the topic subscription.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="subscription"> subscription name </param>
		/// <param name="txnIdMostBits"> the most bits of txn id </param>
		/// <param name="txnIdLeastBits"> the least bits of txn id </param>
		/// <returns> the future represents the commit result </returns>
		CompletableFuture<Void> CommitTxnOnSubscription(string Topic, string Subscription, long TxnIdMostBits, long TxnIdLeastBits);

		/// <summary>
		/// Abort the transaction associated with the topic subscription.
		/// </summary>
		/// <param name="topic"> topic name </param>
		/// <param name="subscription"> subscription name </param>
		/// <param name="txnIdMostBits"> the most bits of txn id </param>
		/// <param name="txnIdLeastBits"> the least bits of txn id </param>
		/// <returns> the future represents the abort result </returns>
		CompletableFuture<Void> AbortTxnOnSubscription(string Topic, string Subscription, long TxnIdMostBits, long TxnIdLeastBits);

	}

}