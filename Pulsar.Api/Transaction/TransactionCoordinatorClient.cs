using System;
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
namespace Pulsar.Api.Transaction
{

	using TxnID = org.apache.pulsar.transaction.impl.common.TxnID;

	/// <summary>
	/// Transaction coordinator client.
	/// </summary>
	public interface TransactionCoordinatorClient : System.IDisposable
	{

		/// <summary>
		/// Default transaction ttl in mills.
		/// </summary>

		/// <summary>
		/// State of the transaction coordinator client.
		/// </summary>

		/// <summary>
		/// Start transaction meta store client.
		/// 
		/// <para>This will create connections to transaction meta store service.
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="TransactionCoordinatorClientException"> exception occur while start </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void start() throws TransactionCoordinatorClientException;
		void Start();

		/// <summary>
		/// Start transaction meta store client asynchronous.
		/// 
		/// <para>This will create connections to transaction meta store service.
		/// 
		/// </para>
		/// </summary>
		/// <returns> a future represents the result of start transaction meta store </returns>
		ValueTask StartAsync();

		/// <summary>
		/// Close the transaction meta store client asynchronous.
		/// </summary>
		/// <returns> a future represents the result of close transaction meta store </returns>
		ValueTask CloseAsync();

		/// <summary>
		/// Create a new transaction.
		/// </summary>
		/// <returns> <seealso cref="TxnID"/> as the identifier for identifying the transaction. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.transaction.impl.common.TxnID newTransaction() throws TransactionCoordinatorClientException;
		TxnID NewTransaction();

		/// <summary>
		/// Create a new transaction asynchronously.
		/// </summary>
		/// <returns> a future represents the result of creating a new transaction.
		///         it returns <seealso cref="TxnID"/> as the identifier for identifying the
		///         transaction. </returns>
		ValueTask<TxnID> NewTransactionAsync();

		/// <summary>
		/// Create a new transaction.
		/// </summary>
		/// <param name="timeout"> timeout for new transaction </param>
		/// <param name="unit"> time unit for new transaction
		/// </param>
		/// <returns> <seealso cref="TxnID"/> as the identifier for identifying the transaction. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.transaction.impl.common.TxnID newTransaction(long timeout, java.util.concurrent.TimeUnit unit) throws TransactionCoordinatorClientException;
		TxnID NewTransaction(long timeout, TimeSpan unit);

		/// <summary>
		/// Create a new transaction asynchronously.
		/// </summary>
		/// <param name="timeout"> timeout for new transaction </param>
		/// <param name="unit"> time unit for new transaction
		/// </param>
		/// <returns> a future represents the result of creating a new transaction.
		///         it returns <seealso cref="TxnID"/> as the identifier for identifying the
		///         transaction. </returns>
		ValueTask<TxnID> NewTransactionAsync(long timeout, TimeSpan unit);

		/// <summary>
		/// Add publish partition to txn.
		/// </summary>
		/// <param name="txnID"> txn id which add partitions to. </param>
		/// <param name="partitions"> partitions add to the txn. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addPublishPartitionToTxn(org.apache.pulsar.transaction.impl.common.TxnID txnID, java.util.List<String> partitions) throws TransactionCoordinatorClientException;
		void AddPublishPartitionToTxn(TxnID txnID, IList<string> partitions);

		/// <summary>
		/// Add publish partition to txn asynchronously.
		/// </summary>
		/// <param name="txnID"> txn id which add partitions to. </param>
		/// <param name="partitions"> partitions add to the txn.
		/// </param>
		/// <returns> a future represents the result of add publish partition to txn. </returns>
		ValueTask AddPublishPartitionToTxnAsync(TxnID txnID, IList<string> partitions);

		/// <summary>
		/// Commit txn. </summary>
		/// <param name="txnID"> txn id to commit. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void commit(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws TransactionCoordinatorClientException;
		void Commit(TxnID txnID);

		/// <summary>
		/// Commit txn asynchronously. </summary>
		/// <param name="txnID"> txn id to commit. </param>
		/// <returns> a future represents the result of commit txn. </returns>
		ValueTask CommitAsync(TxnID txnID);

		/// <summary>
		/// Abort txn. </summary>
		/// <param name="txnID"> txn id to abort. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void abort(org.apache.pulsar.transaction.impl.common.TxnID txnID) throws TransactionCoordinatorClientException;
		void Abort(TxnID txnID);

		/// <summary>
		/// Abort txn asynchronously. </summary>
		/// <param name="txnID"> txn id to abort. </param>
		/// <returns> a future represents the result of abort txn. </returns>
		ValueTask AbortAsync(TxnID txnID);

		/// <summary>
		/// Get current state of the transaction meta store.
		/// </summary>
		/// <returns> current state <seealso cref="State"/> of the transaction meta store </returns>
		TransactionCoordinatorClient_State State {get;}
	}

	public static class TransactionCoordinatorClient_Fields
	{
		public const long DEFAULT_TXN_TTL_MS = 60000L;
	}

	public enum TransactionCoordinatorClient_State
	{
		NONE,
		STARTING,
		READY,
		CLOSING,
		CLOSED
	}

}