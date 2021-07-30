using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Transactions.Models;

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
namespace SharpPulsar.Admin
{
    public interface ITransactions : System.IDisposable
    {
        /// <summary>
        /// The base URI of the service.
        /// </summary>
        System.Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets json serialization settings.
        /// </summary>
        Newtonsoft.Json.JsonSerializerSettings SerializationSettings { get; }

        /// <summary>
        /// Gets or sets json deserialization settings.
        /// </summary>
        Newtonsoft.Json.JsonSerializerSettings DeserializationSettings { get; }

        /// <summary>
        /// Get transaction metadataStore stats.
        /// </summary>
        /// <param name="coordinatorId"> the id which get transaction coordinator </param>
        /// <returns> the future of transaction metadata store stats. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionCoordinatorStats>> GetCoordinatorStatsByIdAsync(int coordinatorId, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction metadataStore stats.
        /// </summary>
        /// <param name="coordinatorId"> the id which get transaction coordinator </param>
        /// <returns> the transaction metadata store stats. </returns>
        Microsoft.Rest.HttpOperationResponse<TransactionCoordinatorStats> GetCoordinatorStatsById(int coordinatorId, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction metadataStore stats.
        /// </summary>
        /// <returns> the map future of transaction metadata store stats. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<IDictionary<int, TransactionCoordinatorStats>>> CoordinatorStatsAsync(Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction metadataStore stats.
        /// </summary>
        /// <returns> the map of transaction metadata store stats. </returns>

        Microsoft.Rest.HttpOperationResponse<IDictionary<int, TransactionCoordinatorStats>> CoordinatorStats(Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction in buffer stats.
        /// </summary>
        /// <param name="txnID"> the txnId </param>
        /// <param name="topic"> the produce topic </param>
        /// <returns> the future stats of transaction in buffer. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionInBufferStats>> GetTransactionInBufferStatsAsync(TxnID txnID, string topic, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction in buffer stats.
        /// </summary>
        /// <param name="txnID"> the txnId </param>
        /// <param name="topic"> the produce topic </param>
        /// <returns> the stats of transaction in buffer. </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionInBufferStats> GetTransactionInBufferStats(TxnID txnID, string topic, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction in pending ack stats.
        /// </summary>
        /// <param name="txnID"> the txnId </param>
        /// <param name="topic"> the ack topic </param>
        /// <param name="subName"> the subscription name of this transaction ack </param>
        /// <returns> the future stats of transaction in pending ack. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionInPendingAckStats>> GetTransactionInPendingAckStatsAsync(TxnID txnID, string topic, string subName, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get transaction in pending ack stats.
        /// </summary>
        /// <param name="txnID"> the txnId </param>
        /// <param name="topic"> the ack topic </param>
        /// <param name="subName"> the subscription name of this transaction ack </param>
        /// <returns> the stats of transaction in pending ack. </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionInPendingAckStats> GetTransactionInPendingAckStats(TxnID txnID, string topic, string subName, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Get transaction metadata.
        /// </summary>
        /// <param name="txnID"> the ID of this transaction </param>
        /// <returns> the future metadata of this transaction. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionMetadata>> GetTransactionMetadataAsync(TxnID txnID, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction metadata.
        /// </summary>
        /// <param name="txnID"> the ID of this transaction </param>
        /// <returns> the metadata of this transaction. </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionMetadata> GetTransactionMetadata(TxnID txnID, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction buffer stats.
        /// </summary>
        /// <param name="topic"> the topic of getting transaction buffer stats </param>
        /// <returns> the future stats of transaction buffer in topic. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionBufferStats>> GetTransactionBufferStatsAsync(string topic, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction buffer stats.
        /// </summary>
        /// <param name="topic"> the topic of getting transaction buffer stats </param>
        /// <returns> the stats of transaction buffer in topic. </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionBufferStats> GetTransactionBufferStats(string topic, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction pending ack stats.
        /// </summary>
        /// <param name="topic"> the topic of this transaction pending ack stats </param>
        /// <param name="subName"> the subscription name of this transaction pending ack stats </param>
        /// <returns> the stats of transaction pending ack. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionPendingAckStats>> GetPendingAckStatsAsync(string topic, string subName, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction pending ack stats.
        /// </summary>
        /// <param name="topic"> the topic of this transaction pending ack stats </param>
        /// <param name="subName"> the subscription name of this transaction pending ack stats </param>
        /// <returns> the stats of transaction pending ack. </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionPendingAckStats> GetPendingAckStats(string topic, string subName, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get slow transactions by coordinator id.
        /// </summary>
        /// <param name="coordinatorId"> the coordinator id of getting slow transaction status. </param>
        /// <param name="timeout"> the timeout </param>
        /// <param name="timeUnit"> the timeout timeUnit </param>
        /// <returns> the future metadata of slow transactions. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<IDictionary<string, TransactionMetadata>>> GetSlowTransactionsByCoordinatorIdAsync(int? coordinatorId, long timeout, long timeUnit, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get slow transactions by coordinator id.
        /// </summary>
        /// <param name="coordinatorId"> the coordinator id of getting slow transaction status. </param>
        /// <param name="timeout"> the timeout </param>
        /// <param name="timeUnit"> the timeout timeUnit </param>
        /// <returns> the metadata of slow transactions. </returns>

        Microsoft.Rest.HttpOperationResponse<IDictionary<string, TransactionMetadata>> GetSlowTransactionsByCoordinatorId(int? coordinatorId, long timeout, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get slow transactions.
        /// </summary>
        /// <param name="timeout"> the timeout </param>
        /// <param name="timeUnit"> the timeout timeUnit
        /// </param>
        /// <returns> the future metadata of slow transactions. </returns>
        Task<Microsoft.Rest.HttpOperationResponse<IDictionary<string, TransactionMetadata>>> GetSlowTransactionsAsync(long timeout, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));


        /// <summary>
        /// Get slow transactions.
        /// </summary>
        /// <param name="timeout"> the timeout </param>
        /// <param name="timeUnit"> the timeout timeUnit
        /// </param>
        /// <returns> the metadata of slow transactions. </returns>

        Microsoft.Rest.HttpOperationResponse<IDictionary<string, TransactionMetadata>> GetSlowTransactions(long timeout, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction coordinator internal stats.
        /// </summary>
        /// <param name="coordinatorId"> the coordinator ID </param>
        /// <param name="metadata"> is get ledger metadata
        /// </param>
        /// <returns> the future internal stats of this coordinator </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionCoordinatorInternalStats>> GetCoordinatorInternalStatsAsync(int coordinatorId, bool metadata, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get transaction coordinator internal stats.
        /// </summary>
        /// <param name="coordinatorId"> the coordinator ID </param>
        /// <param name="metadata"> whether to obtain ledger metadata
        /// </param>
        /// <returns> the internal stats of this coordinator </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionCoordinatorInternalStats> GetCoordinatorInternalStats(int coordinatorId, bool metadata, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get pending ack internal stats.
        /// </summary>
        /// <param name="topic"> the topic of get pending ack internal stats </param>
        /// <param name="subName"> the subscription name of this pending ack </param>
        /// <param name="metadata"> whether to obtain ledger metadata
        /// </param>
        /// <returns> the future internal stats of pending ack </returns>
        Task<Microsoft.Rest.HttpOperationResponse<TransactionPendingAckInternalStats>> GetPendingAckInternalStatsAsync(string topic, string subName, bool metadata, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Get pending ack internal stats.
        /// </summary>
        /// <param name="topic"> the topic of get pending ack internal stats </param>
        /// <param name="subName"> the subscription name of this pending ack </param>
        /// <param name="metadata"> whether to obtain ledger metadata
        /// </param>
        /// <returns> the internal stats of pending ack </returns>

        Microsoft.Rest.HttpOperationResponse<TransactionPendingAckInternalStats> GetPendingAckInternalStats(string topic, string subName, bool metadata, Dictionary<string, List<string>> customHeaders = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

    }
}
