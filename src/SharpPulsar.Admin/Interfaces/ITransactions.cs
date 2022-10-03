using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;

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
namespace SharpPulsar.Admin.Interfaces
{
	
	public interface ITransactions
	{

		/// <summary>
		/// Get transaction metadataStore stats.
		/// </summary>
		/// <param name="coordinatorId"> the id which get transaction coordinator </param>
		/// <returns> the future of transaction metadata store stats. </returns>
		ValueTask<TransactionCoordinatorStats> GetCoordinatorStatsByIdAsync(int coordinatorId);

		/// <summary>
		/// Get transaction metadataStore stats.
		/// </summary>
		/// <param name="coordinatorId"> the id which get transaction coordinator </param>
		/// <returns> the transaction metadata store stats. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionCoordinatorStats getCoordinatorStatsById(int coordinatorId) throws PulsarAdminException;
		TransactionCoordinatorStats GetCoordinatorStatsById(int coordinatorId);

		/// <summary>
		/// Get transaction metadataStore stats.
		/// </summary>
		/// <returns> the map future of transaction metadata store stats. </returns>
		ValueTask<IDictionary<int, TransactionCoordinatorStats>> CoordinatorStatsAsync {get;}

		/// <summary>
		/// Get transaction metadataStore stats.
		/// </summary>
		/// <returns> the map of transaction metadata store stats. </returns>

// ORIGINAL LINE: java.util.Map<int, org.apache.pulsar.common.policies.data.TransactionCoordinatorStats> getCoordinatorStats() throws PulsarAdminException;
		IDictionary<int, TransactionCoordinatorStats> CoordinatorStats {get;}

		/// <summary>
		/// Get transaction in buffer stats.
		/// </summary>
		/// <param name="txnID"> the txnId </param>
		/// <param name="topic"> the produce topic </param>
		/// <returns> the future stats of transaction in buffer. </returns>
		ValueTask<TransactionInBufferStats> GetTransactionInBufferStatsAsync(TxnID txnID, string topic);

		/// <summary>
		/// Get transaction in buffer stats.
		/// </summary>
		/// <param name="txnID"> the txnId </param>
		/// <param name="topic"> the produce topic </param>
		/// <returns> the stats of transaction in buffer. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionInBufferStats getTransactionInBufferStats(org.apache.pulsar.client.api.transaction.TxnID txnID, String topic) throws PulsarAdminException;
		TransactionInBufferStats GetTransactionInBufferStats(TxnID txnID, string topic);

		/// <summary>
		/// Get transaction in pending ack stats.
		/// </summary>
		/// <param name="txnID"> the txnId </param>
		/// <param name="topic"> the ack topic </param>
		/// <param name="subName"> the subscription name of this transaction ack </param>
		/// <returns> the future stats of transaction in pending ack. </returns>
		ValueTask<TransactionInPendingAckStats> GetTransactionInPendingAckStatsAsync(TxnID txnID, string topic, string subName);
		/// <summary>
		/// Get transaction in pending ack stats.
		/// </summary>
		/// <param name="txnID"> the txnId </param>
		/// <param name="topic"> the ack topic </param>
		/// <param name="subName"> the subscription name of this transaction ack </param>
		/// <returns> the stats of transaction in pending ack. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionInPendingAckStats getTransactionInPendingAckStats(org.apache.pulsar.client.api.transaction.TxnID txnID, String topic, String subName) throws PulsarAdminException;
		TransactionInPendingAckStats GetTransactionInPendingAckStats(TxnID txnID, string topic, string subName);
		/// <summary>
		/// Get transaction metadata.
		/// </summary>
		/// <param name="txnID"> the ID of this transaction </param>
		/// <returns> the future metadata of this transaction. </returns>
		ValueTask<TransactionMetadata> GetTransactionMetadataAsync(TxnID txnID);

		/// <summary>
		/// Get transaction metadata.
		/// </summary>
		/// <param name="txnID"> the ID of this transaction </param>
		/// <returns> the metadata of this transaction. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionMetadata getTransactionMetadata(org.apache.pulsar.client.api.transaction.TxnID txnID) throws PulsarAdminException;
		TransactionMetadata GetTransactionMetadata(TxnID txnID);

		/// <summary>
		/// Get transaction buffer stats.
		/// </summary>
		/// <param name="topic"> the topic of getting transaction buffer stats </param>
		/// <param name="lowWaterMarks"> Whether to get information about lowWaterMarks stored in transaction pending ack. </param>
		/// <returns> the future stats of transaction buffer in topic. </returns>
		ValueTask<TransactionBufferStats> GetTransactionBufferStatsAsync(string topic, bool lowWaterMarks);

		/// <summary>
		/// Get transaction buffer stats.
		/// </summary>
		/// <param name="topic"> the topic of getting transaction buffer stats </param>
		/// <returns> the future stats of transaction buffer in topic. </returns>
		ValueTask<TransactionBufferStats> GetTransactionBufferStatsAsync(string topic)
		{
			return GetTransactionBufferStatsAsync(topic, false);
		}

		/// <summary>
		/// Get transaction buffer stats.
		/// </summary>
		/// <param name="topic"> the topic of getting transaction buffer stats </param>
		/// <param name="lowWaterMarks"> Whether to get information about lowWaterMarks stored in transaction buffer. </param>
		/// <returns> the stats of transaction buffer in topic. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionBufferStats getTransactionBufferStats(String topic, boolean lowWaterMarks) throws PulsarAdminException;
		TransactionBufferStats GetTransactionBufferStats(string topic, bool lowWaterMarks);

		/// <summary>
		/// Get transaction buffer stats.
		/// </summary>
		/// <param name="topic"> the topic of getting transaction buffer stats </param>
		/// <returns> the stats of transaction buffer in topic. </returns>

// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TransactionBufferStats getTransactionBufferStats(String topic) throws PulsarAdminException
		TransactionBufferStats GetTransactionBufferStats(string topic)
		{
			return GetTransactionBufferStats(topic, false);
		}

		/// <summary>
		/// Get transaction pending ack stats.
		/// </summary>
		/// <param name="topic"> the topic of this transaction pending ack stats </param>
		/// <param name="subName"> the subscription name of this transaction pending ack stats </param>
		/// <param name="lowWaterMarks"> Whether to get information about lowWaterMarks stored in transaction pending ack. </param>
		/// <returns> the stats of transaction pending ack. </returns>
		ValueTask<TransactionPendingAckStats> GetPendingAckStatsAsync(string topic, string subName, bool lowWaterMarks);

		/// <summary>
		/// Get transaction pending ack stats.
		/// </summary>
		/// <param name="topic"> the topic of this transaction pending ack stats </param>
		/// <param name="subName"> the subscription name of this transaction pending ack stats </param>
		/// <returns> the stats of transaction pending ack. </returns>
		ValueTask<TransactionPendingAckStats> GetPendingAckStatsAsync(string topic, string subName)
		{
			return GetPendingAckStatsAsync(topic, subName, false);
		}

		/// <summary>
		/// Get transaction pending ack stats.
		/// </summary>
		/// <param name="topic"> the topic of this transaction pending ack stats </param>
		/// <param name="subName"> the subscription name of this transaction pending ack stats </param>
		/// <param name="lowWaterMarks"> Whether to get information about lowWaterMarks stored in transaction pending ack. </param>
		/// <returns> the stats of transaction pending ack. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionPendingAckStats getPendingAckStats(String topic, String subName, boolean lowWaterMarks) throws PulsarAdminException;
		TransactionPendingAckStats GetPendingAckStats(string topic, string subName, bool lowWaterMarks);

		/// <summary>
		/// Get transaction pending ack stats.
		/// </summary>
		/// <param name="topic"> the topic of this transaction pending ack stats </param>
		/// <param name="subName"> the subscription name of this transaction pending ack stats </param>
		/// <returns> the stats of transaction pending ack. </returns>

// ORIGINAL LINE: default org.apache.pulsar.common.policies.data.TransactionPendingAckStats getPendingAckStats(String topic, String subName) throws PulsarAdminException
		TransactionPendingAckStats GetPendingAckStats(string topic, string subName)
		{
			return GetPendingAckStats(topic, subName, false);
		}

		/// <summary>
		/// Get slow transactions by coordinator id.
		/// </summary>
		/// <param name="coordinatorId"> the coordinator id of getting slow transaction status. </param>
		/// <param name="timeout"> the timeout </param>
		/// <param name="TimeSpan"> the timeout TimeSpan </param>
		/// <returns> the future metadata of slow transactions. </returns>
		ValueTask<IDictionary<string, TransactionMetadata>> GetSlowTransactionsByCoordinatorIdAsync(int? coordinatorId, long timeout, TimeSpan TimeSpan);

		/// <summary>
		/// Get slow transactions by coordinator id.
		/// </summary>
		/// <param name="coordinatorId"> the coordinator id of getting slow transaction status. </param>
		/// <param name="timeout"> the timeout </param>
		/// <param name="TimeSpan"> the timeout TimeSpan </param>
		/// <returns> the metadata of slow transactions. </returns>

// ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.TransactionMetadata> getSlowTransactionsByCoordinatorId(System.Nullable<int> coordinatorId, long timeout, java.util.concurrent.TimeSpan TimeSpan) throws PulsarAdminException;
		IDictionary<string, TransactionMetadata> GetSlowTransactionsByCoordinatorId(int? coordinatorId, long timeout, TimeSpan TimeSpan);

		/// <summary>
		/// Get slow transactions.
		/// </summary>
		/// <param name="timeout"> the timeout </param>
		/// <param name="TimeSpan"> the timeout TimeSpan
		/// </param>
		/// <returns> the future metadata of slow transactions. </returns>
		ValueTask<IDictionary<string, TransactionMetadata>> GetSlowTransactionsAsync(long timeout, TimeSpan TimeSpan);


		/// <summary>
		/// Get slow transactions.
		/// </summary>
		/// <param name="timeout"> the timeout </param>
		/// <param name="TimeSpan"> the timeout TimeSpan
		/// </param>
		/// <returns> the metadata of slow transactions. </returns>

// ORIGINAL LINE: java.util.Map<String, org.apache.pulsar.common.policies.data.TransactionMetadata> getSlowTransactions(long timeout, java.util.concurrent.TimeSpan TimeSpan) throws PulsarAdminException;
		IDictionary<string, TransactionMetadata> GetSlowTransactions(long timeout, TimeSpan TimeSpan);

		/// <summary>
		/// Get transaction coordinator internal stats.
		/// </summary>
		/// <param name="coordinatorId"> the coordinator ID </param>
		/// <param name="metadata"> is get ledger metadata
		/// </param>
		/// <returns> the future internal stats of this coordinator </returns>
		ValueTask<TransactionCoordinatorInternalStats> GetCoordinatorInternalStatsAsync(int coordinatorId, bool metadata);

		/// <summary>
		/// Get transaction coordinator internal stats.
		/// </summary>
		/// <param name="coordinatorId"> the coordinator ID </param>
		/// <param name="metadata"> whether to obtain ledger metadata
		/// </param>
		/// <returns> the internal stats of this coordinator </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionCoordinatorInternalStats getCoordinatorInternalStats(int coordinatorId, boolean metadata) throws PulsarAdminException;
		TransactionCoordinatorInternalStats GetCoordinatorInternalStats(int coordinatorId, bool metadata);

		/// <summary>
		/// Get pending ack internal stats.
		/// </summary>
		/// <param name="topic"> the topic of get pending ack internal stats </param>
		/// <param name="subName"> the subscription name of this pending ack </param>
		/// <param name="metadata"> whether to obtain ledger metadata
		/// </param>
		/// <returns> the future internal stats of pending ack </returns>
		ValueTask<TransactionPendingAckInternalStats> GetPendingAckInternalStatsAsync(string topic, string subName, bool metadata);

		/// <summary>
		/// Get pending ack internal stats.
		/// </summary>
		/// <param name="topic"> the topic of get pending ack internal stats </param>
		/// <param name="subName"> the subscription name of this pending ack </param>
		/// <param name="metadata"> whether to obtain ledger metadata
		/// </param>
		/// <returns> the internal stats of pending ack </returns>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.TransactionPendingAckInternalStats getPendingAckInternalStats(String topic, String subName, boolean metadata) throws PulsarAdminException;
		TransactionPendingAckInternalStats GetPendingAckInternalStats(string topic, string subName, bool metadata);

		/// <summary>
		/// Sets the scale of the transaction coordinators.
		/// And currently, we can only support scale-up. </summary>
		/// <param name="replicas"> the new transaction coordinators size. </param>

// ORIGINAL LINE: void scaleTransactionCoordinators(int replicas) throws PulsarAdminException;
		void ScaleTransactionCoordinators(int replicas);

		/// <summary>
		/// Asynchronously sets the size of the transaction coordinators.
		/// And currently, we can only support scale-up. </summary>
		/// <param name="replicas"> the new transaction coordinators size. </param>
		/// <returns> a future that can be used to track when the transaction coordinator number is updated. </returns>
		ValueTask ScaleTransactionCoordinatorsAsync(int replicas);

		/// <summary>
		/// Get the position stats in transaction pending ack. </summary>
		/// <param name="topic"> the topic of checking position in pending ack state </param>
		/// <param name="subName"> the subscription name of this pending ack </param>
		/// <param name="ledgerId"> the ledger id of the message position. </param>
		/// <param name="entryId"> the entry id of the message position. </param>
		/// <param name="batchIndex"> the batch index of the message position, `null` means not batch message. </param>
		/// <returns> <seealso cref="PositionInPendingAckStats"/> a state identified whether the position state. </returns>

// ORIGINAL LINE: org.apache.pulsar.common.stats.PositionInPendingAckStats getPositionStatsInPendingAck(String topic, String subName, System.Nullable<long> ledgerId, System.Nullable<long> entryId, System.Nullable<int> batchIndex) throws PulsarAdminException;
		PositionInPendingAckStats GetPositionStatsInPendingAck(string topic, string subName, long? ledgerId, long? entryId, int? batchIndex);

		/// <summary>
		/// Get the position stats in transaction pending ack.
		/// </summary>
		/// <param name="topic"> the topic of checking position in pending ack state </param>
		/// <param name="subName"> the subscription name of this pending ack </param>
		/// <param name="ledgerId"> the ledger id of the message position. </param>
		/// <param name="entryId"> the entry id of the message position. </param>
		/// <param name="batchIndex"> the batch index of the message position, `null` means not batch message. </param>
		/// <returns> <seealso cref="PositionInPendingAckStats"/> a state identified whether the position state. </returns>
		ValueTask<PositionInPendingAckStats> GetPositionStatsInPendingAckAsync(string topic, string subName, long? ledgerId, long? entryId, int? batchIndex);
	}

}