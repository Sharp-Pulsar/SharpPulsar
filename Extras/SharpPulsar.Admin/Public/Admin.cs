using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.Admin.Model;

namespace SharpPulsar.Admin.Public
{
    public class Admin
    {
        private readonly PulsarAdminRESTAPI _api;
        public Admin(string brokerWebServiceUrl, HttpClient httpClient, bool disposeHttpClient = true)
        {
            _api = new PulsarAdminRESTAPI(brokerWebServiceUrl, httpClient, disposeHttpClient);
        } 
        public Admin(string brokerWebServiceUrl, params DelegatingHandler[] handlers)
        {
            _api = new PulsarAdminRESTAPI(brokerWebServiceUrl, handlers);
        }
        public Admin(string brokerwebserviceurl, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
        {
            _api = new PulsarAdminRESTAPI(brokerwebserviceurl, rootHandler, handlers);
        }
        public Admin(Uri baseUri, params DelegatingHandler[] handlers)
        {
            _api = new PulsarAdminRESTAPI(baseUri, handlers);
        }
        public Admin(Uri baseUri, HttpClientHandler rootHandler, params DelegatingHandler[] handlers)
        {
            _api = new PulsarAdminRESTAPI(baseUri, rootHandler, handlers);
        }

        /// <summary>
        /// An REST endpoint to trigger backlogQuotaCheck
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse BacklogQuotaCheck(Dictionary<string, List<string>> customHeaders = null)
        {
            return BacklogQuotaCheckAsync(customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// An REST endpoint to trigger backlogQuotaCheck
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse> BacklogQuotaCheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.BacklogQuotaCheckWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse ClearNamespaceBacklogForSubscription(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> ClearNamespaceBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBacklogForSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse ClearNamespaceBacklog(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBacklogAsync(tenant, namespaceParameter, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> ClearNamespaceBacklogAsync(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBacklogWithHttpMessagesAsync(tenant, namespaceParameter, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse ClearNamespaceBundleBacklogForSubscription(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBundleBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for a given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> ClearNamespaceBundleBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBundleBacklogForSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse ClearNamespaceBundleBacklog(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBundleBacklogAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear backlog for all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> ClearNamespaceBundleBacklogAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBundleBacklogWithHttpMessagesAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear the namespace configured offload deletion lag. The topics in the
        /// namespace will fallback to using the default configured deletion lag for
        /// the broker
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse ClearOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear the namespace configured offload deletion lag. The topics in the
        /// namespace will fallback to using the default configured deletion lag for
        /// the broker
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> ClearOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearOffloadDeletionLagWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Trigger a compaction operation on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse Compact(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CompactAsync(tenant, namespaceParameter, topic, authoritative, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
            
        }
        /// <summary>
        /// Trigger a compaction operation on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> CompactAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.Compact1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            else
                return await _api.CompactWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the status of a compaction operation for a topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse<LongRunningProcessStatus> CompactionStatus(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
                return CompactionStatusAsync(tenant, namespaceParameter, topic, authoritative, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the status of a compaction operation for a topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse<LongRunningProcessStatus>> CompactionStatusAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.CompactionStatus1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            else
                return await _api.CompactionStatusWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Create a new cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges, and the name cannot
        /// contain the '/' characters.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='body'>
        /// The cluster data
        /// </param>
        public HttpOperationResponse CreateCluster(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateClusterAsync(cluster, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create a new cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges, and the name cannot
        /// contain the '/' characters.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='body'>
        /// The cluster data
        /// </param>
        public async Task<HttpOperationResponse> CreateClusterAsync(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateClusterWithHttpMessagesAsync(cluster, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Create missed partitions of an existing partitioned topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public HttpOperationResponse CreateMissedPartitions(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateMissedPartitionsAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create missed partitions of an existing partitioned topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public async Task<HttpOperationResponse> CreateMissedPartitionsAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.CreateMissedPartitions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken);
            else
                return await _api.CreateMissedPartitionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken);
        }
        /// <summary>
        /// Creates a new namespace with the specified policies
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Policies for the namespace
        /// </param>
        public HttpOperationResponse CreateNamespace(string tenant, string namespaceParameter, Policies body = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateNamespaceAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Creates a new namespace with the specified policies
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Policies for the namespace
        /// </param>
        public async Task<HttpOperationResponse> CreateNamespaceAsync(string tenant, string namespaceParameter, Policies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Create a non-partitioned topic.
        /// </summary>
        /// <remarks>
        /// This is the only REST endpoint from which non-partitioned topics could be
        /// created.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse CreateNonPartitionedTopic(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateNonPartitionedTopicAsync(tenant, namespaceParameter, topic, authoritative, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create a non-partitioned topic.
        /// </summary>
        /// <remarks>
        /// This is the only REST endpoint from which non-partitioned topics could be
        /// created.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> CreateNonPartitionedTopicAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.CreateNonPartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.CreateNonPartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Create a partitioned topic.
        /// </summary>
        /// <remarks>
        /// It needs to be called before creating a producer on a partitioned topic.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='partitions'>
        /// The number of partitions for the topic
        /// </param>
        public HttpOperationResponse CreatePartitionedTopic(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreatePartitionedTopicAsync(tenant, namespaceParameter, topic, partitions, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create a partitioned topic.
        /// </summary>
        /// <remarks>
        /// It needs to be called before creating a producer on a partitioned topic.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='partitions'>
        /// The number of partitions for the topic
        /// </param>
        public async Task<HttpOperationResponse> CreatePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.CreatePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.CreatePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Create a subscription on the topic.
        /// </summary>
        /// <remarks>
        /// Creates a subscription on the topic at the specified message id
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subscriptionName'>
        /// Subscription to create position on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId where to create the subscription. It can be 'latest', 'earliest'
        /// or (ledgerId:entryId)
        /// </param>
        /// <param name='replicated'>
        /// Is replicated required to perform this operation
        /// </param>
        public HttpOperationResponse CreateSubscription(string tenant, string namespaceParameter, string topic, string subscriptionName, bool? authoritative = false, bool isPersistentTopic = true, MessageIdImpl messageId = null, bool? replicated = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateSubscriptionAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, isPersistentTopic, messageId, replicated, customHeaders).GetAwaiter().GetResult();

        }
        /// <summary>
        /// Create a subscription on the topic.
        /// </summary>
        /// <remarks>
        /// Creates a subscription on the topic at the specified message id
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subscriptionName'>
        /// Subscription to create position on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId where to create the subscription. It can be 'latest', 'earliest'
        /// or (ledgerId:entryId)
        /// </param>
        /// <param name='replicated'>
        /// Is replicated required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> CreateSubscriptionAsync(string tenant, string namespaceParameter, string topic, string subscriptionName, bool? authoritative = false, bool isPersistentTopic = true, MessageIdImpl messageId = null, bool? replicated = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.CreateSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, messageId, replicated, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.CreateSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, messageId, replicated, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        /// <summary>
        /// Create a new tenant.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar super-user privileges.
        /// </remarks>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='tenantInfo'>
        /// TenantInfo
        /// </param>
        public HttpOperationResponse CreateTenant(string tenant, TenantInfo body = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateTenantAsync(tenant, body, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Create a new tenant.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar super-user privileges.
        /// </remarks>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='tenantInfo'>
        /// TenantInfo
        /// </param>
        public async Task<HttpOperationResponse> CreateTenantAsync(string tenant, TenantInfo body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateTenantWithHttpMessagesAsync(tenant, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeleteBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteBookieAffinityGroupWithHttpMessagesAsync(property, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removed the rack placement information for a specific bookie in the cluster
        /// </summary>
        /// <param name='bookie'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse DeleteBookieRackInfo(string bookie, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteBookieRackInfoAsync(bookie, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Removed the rack placement information for a specific bookie in the cluster
        /// </summary>
        /// <param name='bookie'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse> DeleteBookieRackInfoAsync(string bookie, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteBookieRackInfoWithHttpMessagesAsync(bookie, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete an existing cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse DeleteCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete an existing cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public async Task<HttpOperationResponse> DeleteClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse DeleteDeduplicationSnapshotInterval(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteDeduplicationSnapshotInterval1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteDeduplicationSnapshotIntervalWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse DeleteDelayedDeliveryPolicies(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();

        }
        /// <summary>
        /// Set delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteDelayedDeliveryPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.DeleteDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        /// <summary>
        /// Delete dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        public HttpOperationResponse DeleteDynamicConfiguration(string configName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDynamicConfigurationAsync(configName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        public async Task<HttpOperationResponse> DeleteDynamicConfigurationAsync(string configName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteDynamicConfigurationWithHttpMessagesAsync(configName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete the failure domain of the cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>
        public HttpOperationResponse DeleteFailureDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteFailureDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the failure domain of the cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>
        public async Task<HttpOperationResponse> DeleteFailureDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteFailureDomainWithHttpMessagesAsync(cluster, domainName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse DeleteInactiveTopicPolicies(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteInactiveTopicPoliciesAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteInactiveTopicPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse DeleteMaxUnackedMessagesOnConsumer(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteMaxUnackedMessagesOnConsumerAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteMaxUnackedMessagesOnConsumerAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse DeleteMaxUnackedMessagesOnSubscription(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteMaxUnackedMessagesOnSubscriptionAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteMaxUnackedMessagesOnSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.DeleteMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        /// <summary>
        /// Delete a namespace bundle and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse DeleteNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceBundleAsync(tenant, namespaceParameter, bundle, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a namespace bundle and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
        /// </param>

        public HttpOperationResponse DeleteNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
        /// </param>

        public async Task<HttpOperationResponse> DeleteNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, policyName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a namespace and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse DeleteNamespace(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceAsync(tenant, namespaceParameter, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a namespace and all the topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='force'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteNamespaceAsync(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a partitioned topic.
        /// </summary>
        /// <remarks>
        /// It will also delete all the partitions of the topic if it exists.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='force'>
        /// Stop all producer/consumer/replicator and delete topic forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='deleteSchema'>
        /// Delete the topic's schema storage
        /// </param>
        public HttpOperationResponse DeletePartitionedTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeletePartitionedTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, force, authoritative, deleteSchema, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a partitioned topic.
        /// </summary>
        /// <remarks>
        /// It will also delete all the partitions of the topic if it exists.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='force'>
        /// Stop all producer/consumer/replicator and delete topic forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='deleteSchema'>
        /// Delete the topic's schema storage
        /// </param>
        public async Task<HttpOperationResponse> DeletePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.DeletePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
           
            return await _api.DeletePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<DeleteSchemaResponse> DeleteSchema(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSchemaAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<DeleteSchemaResponse>> DeleteSchemaAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a subscription.
        /// </summary>
        /// <remarks>
        /// The subscription cannot be deleted if delete is not forcefully and there
        /// are any active consumers attached to it. Force delete ignores connected
        /// consumers and deletes subscription by explicitly closing them.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be deleted
        /// </param>
        /// <param name='force'>
        /// Disconnect and close all consumers and delete subscription forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse DeleteSubscription(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSubscriptionAsync(tenant, namespaceParameter, topic, subName, isPersistentTopic, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a subscription.
        /// </summary>
        /// <remarks>
        /// The subscription cannot be deleted if delete is not forcefully and there
        /// are any active consumers attached to it. Force delete ignores connected
        /// consumers and deletes subscription by explicitly closing them.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be deleted
        /// </param>
        /// <param name='force'>
        /// Disconnect and close all consumers and delete subscription forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> DeleteSubscriptionAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a tenant and all namespaces and topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='force'>
        /// </param>
        public HttpOperationResponse DeleteTenant(string tenant, bool forced = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteTenantAsync(tenant, forced, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a tenant and all namespaces and topics under it.
        /// </summary>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='force'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteTenantAsync(string tenant, bool forced = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteTenantWithHttpMessagesAsync(tenant, forced, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get metrics for all functions owned by worker
        /// </summary>
        /// <remarks>
        /// Requested should be executed by Monitoring agent on each worker to fetch
        /// the metrics
        /// </remarks>
        public HttpOperationResponse<IList<WorkerFunctionInstanceStats>> GetWorkerFunctionInstanceStats(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetWorkerFunctionInstanceStatsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get metrics for all functions owned by worker
        /// </summary>
        /// <remarks>
        /// Requested should be executed by Monitoring agent on each worker to fetch
        /// the metrics
        /// </remarks>
        public async Task<HttpOperationResponse<IList<WorkerFunctionInstanceStats>>> GetWorkerFunctionInstanceStatsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetStats2WithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Gets the metrics for Monitoring
        /// </summary>
        /// <remarks>
        /// Request should be executed by Monitoring agent on each worker to fetch the
        /// worker-metrics
        /// </remarks>
        public HttpOperationResponse<IList<Metrics>> GetMetric(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMetricAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the metrics for Monitoring
        /// </summary>
        /// <remarks>
        /// Request should be executed by Monitoring agent on each worker to fetch the
        /// worker-metrics
        /// </remarks>
        public async Task<HttpOperationResponse<IList<Metrics>>> GetMetricAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMetrics1WithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Fetches information about the Pulsar cluster running Pulsar Functions
        /// </summary>
        public HttpOperationResponse<IList<WorkerInfo>> GetCluster(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClusterAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Fetches information about the Pulsar cluster running Pulsar Functions
        /// </summary>
        public async Task<HttpOperationResponse<IList<WorkerInfo>>> GetClusterAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetCluster1WithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Fetches a list of supported Pulsar IO connectors currently running in
        /// cluster mode
        /// </summary>
        public HttpOperationResponse<IList<object>> GetConnectorsList(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetConnectorsListAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Fetches a list of supported Pulsar IO connectors currently running in
        /// cluster mode
        /// </summary>
        public async Task<HttpOperationResponse<IList<object>>> GetConnectorsListAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetConnectorsListWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Fetches info about the leader node of the Pulsar cluster running Pulsar
        /// Functions
        /// </summary>
        public HttpOperationResponse<WorkerInfo> GetClusterLeader(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClusterLeaderAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Fetches info about the leader node of the Pulsar cluster running Pulsar
        /// Functions
        /// </summary>
        public async Task<HttpOperationResponse<WorkerInfo>> GetClusterLeaderAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetClusterLeaderWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Checks if this node is the leader and is ready to service requests
        /// </summary>
        public HttpOperationResponse<bool?> IsLeaderReady(Dictionary<string, List<string>> customHeaders = null)
        {
            return IsLeaderReadyAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Checks if this node is the leader and is ready to service requests
        /// </summary>
        public async Task<HttpOperationResponse<bool?>> IsLeaderReadyAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.IsLeaderReadyWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Triggers a rebalance of functions to workers
        /// </summary>
        public HttpOperationResponse Rebalance(Dictionary<string, List<string>> customHeaders = null)
        {
            return RebalanceAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Triggers a rebalance of functions to workers
        /// </summary>
        public async Task<HttpOperationResponse> RebalanceAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RebalanceWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Fetches information about which Pulsar Functions are assigned to which
        /// Pulsar clusters
        /// </summary>
        public HttpOperationResponse<IDictionary<string, object>> GetAssignments(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAssignmentsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Fetches information about which Pulsar Functions are assigned to which
        /// Pulsar clusters
        /// </summary>
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetAssignmentsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAssignmentsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a topic.
        /// </summary>
        /// <remarks>
        /// The topic cannot be deleted if delete is not forcefully and there's any
        /// active subscription or producer connected to the it. Force delete ignores
        /// connected clients and deletes topic by explicitly closing them.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='force'>
        /// Stop all producer/consumer/replicator and delete topic forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='deleteSchema'>
        /// Delete the topic's schema storage
        /// </param>
        public HttpOperationResponse DeleteTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, force, authoritative, deleteSchema, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a topic.
        /// </summary>
        /// <remarks>
        /// The topic cannot be deleted if delete is not forcefully and there's any
        /// active subscription or producer connected to the it. Force delete ignores
        /// connected clients and deletes topic by explicitly closing them.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='force'>
        /// Stop all producer/consumer/replicator and delete topic forcefully
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='deleteSchema'>
        /// Delete the topic's schema storage
        /// </param>
        public async Task<HttpOperationResponse> DeleteTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.DeleteTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Examine a specific message on a topic by position relative to the earliest
        /// or the latest message.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='initialPosition'>
        /// Relative start position to examine message.It can be 'latest' or
        /// 'earliest'. Possible values include: 'latest', 'earliest'
        /// </param>
        /// <param name='messagePosition'>
        /// The position of messages (default 1)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse ExamineMessage(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string initialPosition = null, long? messagePosition = 1, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return ExamineMessageAsync(tenant, namespaceParameter, topic, isPersistentTopic, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Examine a specific message on a topic by position relative to the earliest
        /// or the latest message.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='initialPosition'>
        /// Relative start position to examine message.It can be 'latest' or
        /// 'earliest'. Possible values include: 'latest', 'earliest'
        /// </param>
        /// <param name='messagePosition'>
        /// The position of messages (default 1)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> ExamineMessageAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string initialPosition = null, long? messagePosition = 1, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.ExamineMessage1WithHttpMessagesAsync(tenant, namespaceParameter, topic, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.ExamineMessageWithHttpMessagesAsync(tenant, namespaceParameter, topic, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Expiry messages on all subscriptions of topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='expireTimeInSeconds'>
        /// Expires beyond the specified number of seconds
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse ExpireMessagesForAllSubscriptions(string tenant, string namespaceParameter, string topic, int expireTimeInSeconds, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireMessagesForAllSubscriptionsAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();            
        }
        /// <summary>
        /// Expiry messages on all subscriptions of topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='expireTimeInSeconds'>
        /// Expires beyond the specified number of seconds
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> ExpireMessagesForAllSubscriptionsAsync(string tenant, string namespaceParameter, string topic, int expireTimeInSeconds, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.ExpireMessagesForAllSubscriptions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ExpireMessagesForAllSubscriptionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
        }
        /// <summary>
        /// Expiry messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be Expiry messages on
        /// </param>
        /// <param name='expireTimeInSeconds'>
        /// Expires beyond the specified number of seconds
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse ExpireTopicMessages(string tenant, string namespaceParameter, string topic, string subName, int expireTimeInSeconds, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireTopicMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Expiry messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be Expiry messages on
        /// </param>
        /// <param name='expireTimeInSeconds'>
        /// Expires beyond the specified number of seconds
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> ExpireTopicMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int expireTimeInSeconds, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.ExpireTopicMessages3WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ExpireTopicMessages1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Expiry messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be Expiry messages on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId to reset back to (ledgerId:entryId)
        /// </param>s
        public HttpOperationResponse ExpireTopicMessages(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireTopicMessagesAsync(tenant, namespaceParameter, topic, subName, isPersistentTopic, authoritative, messageId, customHeaders).GetAwaiter().GetResult();

        }
        /// <summary>
        /// Expiry messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to be Expiry messages on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId to reset back to (ledgerId:entryId)
        /// </param>
        public async Task<HttpOperationResponse> ExpireTopicMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.ExpireTopicMessages2WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);
           
            return await _api.ExpireTopicMessagesWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        /// <summary>
        /// Get the list of active brokers (web service addresses) in the cluster.If
        /// authorization is not enabled, any cluster name is valid.
        /// </summary>
        /// <param name='cluster'>
        /// </param>
        public HttpOperationResponse<IList<string>> GetActiveBrokers(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetActiveBrokersAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of active brokers (web service addresses) in the cluster.If
        /// authorization is not enabled, any cluster name is valid.
        /// </summary>
        /// <param name='cluster'>
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetActiveBrokersAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetActiveBrokersWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get value of all dynamic configurations' value overridden on local config
        /// </summary>
        public HttpOperationResponse<IDictionary<string, string>> GetAllDynamicConfigurations(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllDynamicConfigurationsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get value of all dynamic configurations' value overridden on local config
        /// </summary>
        public async Task<HttpOperationResponse<IDictionary<string, string>>> GetAllDynamicConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllDynamicConfigurationsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the stats for the Netty allocator. Available allocators are 'default'
        /// and 'ml-cache'
        /// </summary>
        /// <param name='allocator'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<AllocatorStats> GetAllocatorStats(string allocator, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllocatorStatsAsync(allocator, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the stats for the Netty allocator. Available allocators are 'default'
        /// and 'ml-cache'
        /// </summary>
        /// <param name='allocator'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<AllocatorStats>> GetAllocatorStatsAsync(string allocator, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllocatorStatsWithHttpMessagesAsync(allocator, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the all schemas of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<GetAllVersionsSchemaResponse> GetAllSchemas(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllSchemasAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the all schemas of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<GetAllVersionsSchemaResponse>> GetAllSchemasAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllSchemasWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get all namespaces that are grouped by given anti-affinity group in a given
        /// cluster. api can be only accessed by admin of any of the existing tenant
        /// </summary>
        /// <param name='cluster'>
        /// </param>
        /// <param name='group'>
        /// </param>
        /// <param name='tenant'>
        /// </param>
        public HttpOperationResponse<IList<string>> GetAntiAffinityNamespaces(string cluster, string group, string tenant = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAntiAffinityNamespacesAsync(cluster, group, tenant, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all namespaces that are grouped by given anti-affinity group in a given
        /// cluster. api can be only accessed by admin of any of the existing tenant
        /// </summary>
        /// <param name='cluster'>
        /// </param>
        /// <param name='group'>
        /// </param>
        /// <param name='tenant'>
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetAntiAffinityNamespacesAsync(string cluster, string group, string tenant = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAntiAffinityNamespacesWithHttpMessagesAsync(cluster, group, tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get estimated backlog for offline topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse<PersistentOfflineTopicStats> GetBacklog(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get estimated backlog for offline topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse<PersistentOfflineTopicStats>> GetBacklogAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetBacklog1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
                
           return await _api.GetBacklogWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get backlog quota map on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse<IDictionary<string, BacklogQuota>> GetBacklogQuotaMap(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? applied = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogQuotaMapAsync(tenant, namespaceParameter, topic, isPersistentTopic, applied, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get backlog quota map on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, BacklogQuota>>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? applied = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetBacklogQuotaMap2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetBacklogQuotaMap1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get backlog quota map on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<IDictionary<string, BacklogQuota>> GetBacklogQuotaMap(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogQuotaMapAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get backlog quota map on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, BacklogQuota>>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBacklogQuotaMapWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<BookieAffinityGroupData> GetBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the bookie-affinity-group from namespace-local policy.
        /// </summary>
        /// <param name='property'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<BookieAffinityGroupData>> GetBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBookieAffinityGroupWithHttpMessagesAsync(property, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<BookieInfo> GetBookieRackInfo(string bookie, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookieRackInfoAsync(bookie, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<BookieInfo>> GetBookieRackInfoAsync(string bookie, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBookieRackInfoWithHttpMessagesAsync(bookie, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Gets the rack placement information for all the bookies in the cluster
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<IDictionary<string, IDictionary<string, BookieInfo>>> GetBookiesRackInfo(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookiesRackInfoAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the rack placement information for all the bookies in the cluster
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<IDictionary<string, IDictionary<string, BookieInfo>>>> GetBookiesRackInfoAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBookiesRackInfoWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Gets raw information for all the bookies in the cluster
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<BookiesClusterInfo> GetAllBookies(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllBookiesAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets raw information for all the bookies in the cluster
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<BookiesClusterInfo>> GetAllBookiesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllBookiesWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Broker availability report
        /// </summary>
        /// <remarks>
        /// This API gives the current broker availability in percent, each resource
        /// percentage usage is calculated and thensum of all of the resource usage
        /// percent is called broker-resource-availability&lt;br/&gt;&lt;br/&gt;THIS
        /// API IS ONLY FOR USE BY TESTING FOR CONFIRMING NAMESPACE ALLOCATION
        /// ALGORITHM
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<IDictionary<string, ResourceUnit>> GetBrokerResourceAvailability(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerResourceAvailabilityAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Broker availability report
        /// </summary>
        /// <remarks>
        /// This API gives the current broker availability in percent, each resource
        /// percentage usage is calculated and thensum of all of the resource usage
        /// percent is called broker-resource-availability&lt;br/&gt;&lt;br/&gt;THIS
        /// API IS ONLY FOR USE BY TESTING FOR CONFIRMING NAMESPACE ALLOCATION
        /// ALGORITHM
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<IDictionary<string, ResourceUnit>>> GetBrokerResourceAvailabilityAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokerResourceAvailabilityWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get list of brokers with namespace-isolation policies attached to them.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse<IList<BrokerNamespaceIsolationData>> GetBrokersNamespaceIsolationPolicy(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokersNamespaceIsolationPolicyAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get list of brokers with namespace-isolation policies attached to them.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public async Task<HttpOperationResponse<IList<BrokerNamespaceIsolationData>>> GetBrokersNamespaceIsolationPolicyAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokersWithNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get a broker with namespace-isolation policies attached to it.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='broker'>
        /// The broker name (&lt;broker-hostname&gt;:&lt;web-service-port&gt;)
        /// </param>
        public HttpOperationResponse<BrokerNamespaceIsolationData> GetBrokerNamespaceIsolationPolicy(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerNamespaceIsolationPolicyAsync(cluster, broker, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get a broker with namespace-isolation policies attached to it.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='broker'>
        /// The broker name (&lt;broker-hostname&gt;:&lt;web-service-port&gt;)
        /// </param>
        public async Task<HttpOperationResponse<BrokerNamespaceIsolationData>> GetBrokerNamespaceIsolationPolicyAsync(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokerWithNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, broker, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        // <summary>
        /// Get the bundles split data.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<BundlesData> GetBundlesData(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBundlesDataAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        // <summary>
        /// Get the bundles split data.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<BundlesData>> GetBundlesDataAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBundlesDataWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of all the Pulsar clusters.
        /// </summary>
        public HttpOperationResponse<IList<string>> GetClusters(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClustersAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the Pulsar clusters.
        /// </summary>
        public async Task<HttpOperationResponse<IList<string>>> GetClustersAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetClustersWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the configuration for the specified cluster.
        /// </summary>
        public HttpOperationResponse<ClusterData> GetCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the configuration for the specified cluster.
        /// </summary>
        public async Task<HttpOperationResponse<ClusterData>> GetClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetCompactionThreshold(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        { 
            return GetCompactionThresholdAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetCompactionThresholdAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetCompactionThreshold2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetCompactionThreshold1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Maximum number of uncompacted bytes in topics before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<long?> GetCompactionThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetCompactionThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Maximum number of uncompacted bytes in topics before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<long?>> GetCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse GetDeduplicationEnabled(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationEnabledAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<bool?>> GetDeduplicationEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDeduplicationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get deduplication configuration of a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetDeduplicationEnabled(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationEnabledAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get deduplication configuration of a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetDeduplicationEnabledAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetDeduplication2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDeduplication1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set deduplication enabled on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='enableDeduplication'>
        /// DeduplicationEnabled policies for the specified topic
        /// </param>
        public HttpOperationResponse SetDeduplication(string tenant, string namespaceParameter, string topic, bool? enableDeduplication = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDeduplicationAsync(tenant, namespaceParameter, topic, enableDeduplication, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set deduplication enabled on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='enableDeduplication'>
        /// DeduplicationEnabled policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetDeduplicationAsync(string tenant, string namespaceParameter, string topic, bool? enableDeduplication = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetDeduplication1WithHttpMessagesAsync(tenant, namespaceParameter, topic, enableDeduplication, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetDeduplicationWithHttpMessagesAsync(tenant, namespaceParameter, topic, enableDeduplication, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetDeduplicationSnapshotInterval2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDeduplicationSnapshotInterval1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get deduplicationSnapshotInterval config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get deduplicationSnapshotInterval config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDeduplicationSnapshotIntervalWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the default quota
        /// </summary>
        public HttpOperationResponse<IList<string>> GetDefaultResourceQuota(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDefaultResourceQuotaAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the default quota
        /// </summary>
        public async Task<HttpOperationResponse<IList<string>>> GetDefaultResourceQuotaAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDefaultResourceQuotaWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetDelayedDeliveryPolicies(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        { 
            return GetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetDelayedDeliveryPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDelayedDeliveryPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<DelayedDeliveryPolicies> GetDelayedDeliveryPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<DelayedDeliveryPolicies>> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveDelayedDeliveryPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetDispatchRate(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDispatchRateAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
             return await _api.GetDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get dispatch-rate configured for the namespace, -1 represents not
        /// configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<DispatchRate> GetDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get dispatch-rate configured for the namespace, -1 represents not
        /// configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<DispatchRate>> GetDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get a domain in a cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>

        public HttpOperationResponse<FailureDomain> GetDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get a domain in a cluster
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>

        public async Task<HttpOperationResponse<FailureDomain>> GetDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDomainWithHttpMessagesAsync(cluster, domainName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get all updatable dynamic configurations's name
        /// </summary>
        public HttpOperationResponse<IList<string>> GetDynamicConfigurationName(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDynamicConfigurationNameAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all updatable dynamic configurations's name
        /// </summary>
        public async Task<HttpOperationResponse<IList<string>>> GetDynamicConfigurationNameAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDynamicConfigurationNameWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the cluster failure domains.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse<IDictionary<string, FailureDomain>> GetFailureDomains(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetFailureDomainsAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the cluster failure domains.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, FailureDomain>>> GetFailureDomainsAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetFailureDomainsWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetInactiveTopicPolicies(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInactiveTopicPoliciesAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetInactiveTopicPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetInactiveTopicPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get inactive topic policies config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<InactiveTopicPolicies> GetInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get inactive topic policies config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<InactiveTopicPolicies>> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the internal configuration data
        /// </summary>
        public HttpOperationResponse<InternalConfigurationData> GetInternalConfigurationData(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInternalConfigurationDataAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the internal configuration data
        /// </summary>
        public async Task<HttpOperationResponse<InternalConfigurationData>> GetInternalConfigurationDataAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetInternalConfigurationDataWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the information of the leader broker.
        /// </summary>
        public HttpOperationResponse<BrokerInfo> GetLeaderBroker(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLeaderBrokerAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the information of the leader broker.
        /// </summary>
        public async Task<HttpOperationResponse<BrokerInfo>> GetLeaderBrokerAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetLeaderBrokerWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the internal stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='metadata'>
        /// </param>
        public HttpOperationResponse<PersistentTopicInternalStats> GetInternalStats(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInternalStatsAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, metadata, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the internal stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='metadata'>
        /// </param>
        public async Task<HttpOperationResponse<PersistentTopicInternalStats>> GetInternalStatsAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetInternalStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, metadata, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetInternalStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, metadata, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// The flag of whether allow auto update schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<bool?> GetIsAllowAutoUpdateSchema(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetIsAllowAutoUpdateSchemaAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The flag of whether allow auto update schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<bool?>> GetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetIsAllowAutoUpdateSchemaWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Return the last commit message id of topic
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse GetLastMessageId(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLastMessageIdAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
            
        }
        /// <summary>
        /// Return the last commit message id of topic
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> GetLastMessageIdAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.GetLastMessageId1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
                
            return await _api.GetLastMessageIdWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        public HttpOperationResponse<IList<string>> GetList(string tenant, string namespaceParameter, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetListAsync(tenant, namespaceParameter, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetListAsync(string tenant, string namespaceParameter, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetList1WithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetListWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='bundle'>
        /// Bundle range of a topic
        /// </param>
        public HttpOperationResponse<IList<string>> GetListFromBundle(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetListFromBundleAsync(tenant, namespaceParameter, bundle, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of non-persistent topics under a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='bundle'>
        /// Bundle range of a topic
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetListFromBundleAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetListFromBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Load for this broker
        /// </summary>
        /// <remarks>
        /// consists of topics stats &amp; systemResourceUsage
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<LoadReport> GetLoadReport(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLoadReportAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get Load for this broker
        /// </summary>
        /// <remarks>
        /// consists of topics stats &amp; systemResourceUsage
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<LoadReport>> GetLoadReportAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetLoadReportWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the stored topic metadata.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse GetManagedLedgerInfo(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetManagedLedgerInfoAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the stored topic metadata.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> GetManagedLedgerInfoAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetManagedLedgerInfo1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetManagedLedgerInfoWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetMaxConsumers(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxConsumersAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxConsumers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxConsumersWithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetMaxConsumersPerSubscription(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxConsumersPerSubscription2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxConsumersPerSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxConsumersPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxConsumersPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxConsumersPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxConsumersPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxConsumersPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxConsumersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxConsumersPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async  Task<HttpOperationResponse<int?>> GetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxConsumersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetMaxMessageSize(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxMessageSizeAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxMessageSizeAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxMessageSize1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxMessageSizeWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetMaxProducers(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxProducersAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxProducersAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxProducers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxProducersWithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxProducersPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxProducersPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxProducersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxSubscriptionsPerTopic2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxSubscriptionsPerTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxSubscriptionsPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxSubscriptionsPerTopic config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxSubscriptionsPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetMaxUnackedMessagesOnConsumer(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedMessagesOnConsumerAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxUnackedMessagesOnConsumerAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetMaxUnackedMessagesOnSubscription(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        { 
            return GetMaxUnackedMessagesOnSubscriptionAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxUnackedMessagesOnSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxUnackedMessagesPerConsumer(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedMessagesPerConsumerAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxUnackedMessagesPerConsumerWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetMaxUnackedmessagesPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedmessagesPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMaxUnackedmessagesPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxUnackedmessagesPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(true);
        }
        /// <summary>
        /// Get all the mbean details of this broker JVM
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<IList<Metrics>> GetMBeans(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMBeansAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all the mbean details of this broker JVM
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<IList<Metrics>>> GetMBeansAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMBeansWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get message by its messageId.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='ledgerId'>
        /// The ledger id
        /// </param>
        /// <param name='entryId'>
        /// The entry id
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse GetMessageById(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetMessageByIdAsync(tenant, namespaceParameter, topic, ledgerId, entryId, isPersistentTopic, authoritative, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get message by its messageId.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='ledgerId'>
        /// The ledger id
        /// </param>
        /// <param name='entryId'>
        /// The entry id
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> GetMessageByIdAsync(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMessageById1WithHttpMessagesAsync(tenant, namespaceParameter, topic, ledgerId, entryId, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMessageByIdWithHttpMessagesAsync(tenant, namespaceParameter, topic, ledgerId, entryId, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse<int?> GetMessageTTL(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMessageTTLAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetMessageTTLAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetMessageTTL1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Gets the metrics for Monitoring
        /// </summary>
        /// <remarks>
        /// Requested should be executed by Monitoring agent on each broker to fetch
        /// the metrics
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// 
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<IList<Metrics>> GetMetrics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMetricsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the metrics for Monitoring
        /// </summary>
        /// <remarks>
        /// Requested should be executed by Monitoring agent on each broker to fetch
        /// the metrics
        /// </remarks>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<IList<Metrics>>> GetMetricsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMetricsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get anti-affinity group of a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<string> GetNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get anti-affinity group of a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<string>> GetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceAntiAffinityGroupWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<ResourceQuota> GetNamespaceBundleResourceQuota(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceBundleResourceQuotaAsync(tenant, namespaceParameter, bundle, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<ResourceQuota>> GetNamespaceBundleResourceQuotaAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceBundleResourceQuotaWithHttpMessagesAsync(tenant, namespaceParameter, bundle, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the namespace isolation policies assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse<IDictionary<string, NamespaceIsolationData>> GetNamespaceIsolationPolicies(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPoliciesAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the namespace isolation policies assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>s
        public async Task<HttpOperationResponse<IDictionary<string, NamespaceIsolationData>>> GetNamespaceIsolationPoliciesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceIsolationPoliciesWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the single namespace isolation policy assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The name of the namespace isolation policy
        /// </param>
        public HttpOperationResponse<NamespaceIsolationData> GetNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the single namespace isolation policy assigned to the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The name of the namespace isolation policy
        /// </param>
        public async Task<HttpOperationResponse<NamespaceIsolationData>> GetNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, policyName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the message TTL for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetNamespaceMessageTTL(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceMessageTTLAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the message TTL for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the replication clusters for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<IList<string>> GetNamespaceReplicationClusters(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceReplicationClustersAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the replication clusters for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceReplicationClustersWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Number of milliseconds to wait before deleting a ledger segment which has
        /// been offloaded from the Pulsar cluster's local storage (i.e. BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value denotes that deletion has been completely disabled. 'null'
        /// denotes that the topics in the namespace will fall back to the broker
        /// default for deletion lag.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<long?> GetOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Number of milliseconds to wait before deleting a ledger segment which has
        /// been offloaded from the Pulsar cluster's local storage (i.e. BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value denotes that deletion has been completely disabled. 'null'
        /// denotes that the topics in the namespace will fall back to the broker
        /// default for deletion lag.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<long?>> GetOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadDeletionLagWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetOffloadPolicies(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadPoliciesAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetOffloadPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetOffloadPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<OffloadPoliciesImpl> GetOffloadPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<OffloadPoliciesImpl>> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Maximum number of bytes stored on the pulsar cluster for a topic, before
        /// the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// A negative value disables automatic offloading
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<long?> GetOffloadThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Maximum number of bytes stored on the pulsar cluster for a topic, before
        /// the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// A negative value disables automatic offloading
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<long?>> GetOffloadThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadThresholdWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of namespaces served by the specific broker
        /// </summary>
        /// <param name='clusterName'>
        /// </param>
        /// <param name='brokerWebserviceurl'>
        /// </param>
        public HttpOperationResponse<IDictionary<string, NamespaceOwnershipStatus>> GetOwnedNamespaces(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOwnedNamespacesAsync(clusterName, brokerWebserviceurl, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of namespaces served by the specific broker
        /// </summary>
        /// <param name='clusterName'>
        /// </param>
        /// <param name='brokerWebserviceurl'>
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, NamespaceOwnershipStatus>>> GetOwnedNamespacesAsync(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOwnedNamespacesWithHttpMessagesAsync(clusterName, brokerWebserviceurl, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get partitioned topic metadata.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='checkAllowAutoCreation'>
        /// Is check configuration required to automatically create topic
        /// </param>
        public HttpOperationResponse<PartitionedTopicMetadata> GetPartitionedMetadata(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedMetadataAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, checkAllowAutoCreation, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get partitioned topic metadata.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='checkAllowAutoCreation'>
        /// Is check configuration required to automatically create topic
        /// </param>
        public async Task<HttpOperationResponse<PartitionedTopicMetadata>> GetPartitionedMetadataAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPartitionedMetadata1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, checkAllowAutoCreation, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedMetadataWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, checkAllowAutoCreation, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the stats for the partitioned topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='perPartition'>
        /// Get per partition stats
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public HttpOperationResponse GetPartitionedStats(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedStatsAsync(tenant, namespaceParameter, topic, isPersistentTopic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the stats for the partitioned topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='perPartition'>
        /// Get per partition stats
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public async Task<HttpOperationResponse> GetPartitionedStatsAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPartitionedStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of partitioned topics under a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        public HttpOperationResponse<IList<string>> GetPartitionedTopicList(string tenant, string namespaceParameter, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedTopicListAsync(tenant, namespaceParameter, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of partitioned topics under a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetPartitionedTopicListAsync(string tenant, string namespaceParameter, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPartitionedTopicList1WithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedTopicListWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the peer-cluster data for the specified cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse<IList<string>> GetPeerCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPeerClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the peer-cluster data for the specified cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetPeerClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPeerClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get pending bookie client op stats by namesapce
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<IDictionary<string, PendingBookieOpsStats>> GetPendingBookieOpsStats(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPendingBookieOpsStatsAsync(customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get pending bookie client op stats by namesapce
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<IDictionary<string, PendingBookieOpsStats>>> GetPendingBookieOpsStatsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPendingBookieOpsStatsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get permissions on a topic.
        /// </summary>
        /// <remarks>
        /// Retrieve the effective permissions for a topic. These permissions are
        /// defined by the permissions set at thenamespace level combined (union) with
        /// any eventual specific permission set on the topic.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public HttpOperationResponse<IDictionary<string, IList<string>>> GetPermissionsOnTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPermissionsOnTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get permissions on a topic.
        /// </summary>
        /// <remarks>
        /// Retrieve the effective permissions for a topic. These permissions are
        /// defined by the permissions set at thenamespace level combined (union) with
        /// any eventual specific permission set on the topic.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, IList<string>>>> GetPermissionsOnTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPermissionsOnTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPermissionsOnTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Retrieve the permissions for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<IDictionary<string, IList<string>>> GetPermissions(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPermissionsAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Retrieve the permissions for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<IDictionary<string, IList<string>>>> GetPermissionsAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPermissionsWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetPersistence(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPersistenceAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetPersistenceAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPersistence2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPersistence1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the persistence configuration for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<PersistencePolicies> GetPersistences(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPersistencesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the persistence configuration for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<PersistencePolicies>> GetPersistencesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPersistenceWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the dump all the policies specified for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<Policies> GetPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the dump all the policies specified for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<Policies>> GetPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetPublishRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPublishRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetPublishRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetPublishRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPublishRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get replicator dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<DispatchRate> GetReplicatorDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetReplicatorDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get replicator dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<DispatchRate>> GetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetReplicatorDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetRetention(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRetentionAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetRetentionAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetRetention2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetRetention1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get retention config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        public HttpOperationResponse<RetentionPolicies> GetRetention(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRetentionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get retention config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        public async Task<HttpOperationResponse<RetentionPolicies>> GetRetentionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetRetentionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get all runtime configurations. This operation requires Pulsar super-user
        /// privileges.
        /// </summary>
        public HttpOperationResponse<IDictionary<string, string>> GetRuntimeConfiguration(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRuntimeConfigurationAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all runtime configurations. This operation requires Pulsar super-user
        /// privileges.
        /// </summary>
        public async Task<HttpOperationResponse<IDictionary<string, string>>> GetRuntimeConfigurationAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetRuntimeConfigurationWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the schema of a topic at a given version
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='version'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<GetSchemaResponse> GetSchema(string tenant, string namespaceParameter, string topic, string version, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAsync(tenant, namespaceParameter, topic, version, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the schema of a topic at a given version
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='version'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<GetSchemaResponse>> GetSchemaAsync(string tenant, string namespaceParameter, string topic, string version, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchema1WithHttpMessagesAsync(tenant, namespaceParameter, topic, version, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// The strategy used to check the compatibility of new schemas, provided by
        /// producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<string> GetSchemaAutoUpdateCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAutoUpdateCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The strategy used to check the compatibility of new schemas, provided by
        /// producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<string>> GetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaAutoUpdateCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// The strategy of the namespace schema compatibility
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<string> GetSchemaCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The strategy of the namespace schema compatibility
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<string>> GetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get schema validation enforced flag for namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<bool?> GetSchemaValidtionEnforced(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaValidtionEnforcedAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get schema validation enforced flag for namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<bool?>> GetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaValidtionEnforcedWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<GetSchemaResponse> GetSchema(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<GetSchemaResponse>> GetSchemaAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public HttpOperationResponse<TopicStats> GetPersistentTopicStats(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPersistentTopicStatsAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public async Task<HttpOperationResponse<TopicStats>> GetPersistentTopicStatsAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        /// <summary>
        /// Get the stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public HttpOperationResponse<NonPersistentTopicStats> GetNonPersistentTopicStats(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNonPersistentTopicStatsAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the stats for the topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='getPreciseBacklog'>
        /// If return precise backlog or imprecise backlog
        /// </param>
        /// <param name='subscriptionBacklogSize'>
        /// If return backlog size for each subscription, require locking on ledger so
        /// be careful not to use when there's heavy traffic.
        /// </param>
        public async Task<HttpOperationResponse<NonPersistentTopicStats>> GetNonPersistentTopicStatsAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetSubscribeRate(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscribeRateAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetSubscribeRateAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetSubscribeRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscribeRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get subscribe-rate configured for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<SubscribeRate> GetSubscribeRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscribeRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get subscribe-rate configured for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<SubscribeRate>> GetSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscribeRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get subscription message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetSubscriptionDispatchRate(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionDispatchRateAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get subscription message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetSubscriptionDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscriptionDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get Subscription dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<DispatchRate> GetSubscriptionDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get Subscription dispatch-rate configured for the namespace, -1 represents
        /// not configured yet
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<DispatchRate>> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscriptionDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the subscription expiration time for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<int?> GetSubscriptionExpirationTime(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionExpirationTimeAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the subscription expiration time for the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<int?>> GetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscriptionExpirationTimeWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of persistent subscriptions for a given topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse GetSubscriptions(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionsAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of persistent subscriptions for a given topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> GetSubscriptionsAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GetSubscriptions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscriptionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the admin configuration for a given tenant.
        /// </summary>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        public HttpOperationResponse GetTenantAdmin(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantAdminAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the admin configuration for a given tenant.
        /// </summary>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        public async Task<HttpOperationResponse> GetTenantAdminAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantAdminWithHttpMessagesAsync(tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of all the namespaces for a certain tenant.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        public HttpOperationResponse<IList<string>> GetTenantNamespaces(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantNamespacesAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the namespaces for a certain tenant.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetTenantNamespacesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantNamespacesWithHttpMessagesAsync(tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of existing tenants.
        /// </summary>
        public HttpOperationResponse<IList<string>> GetTenants(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of existing tenants.
        /// </summary>
        public async Task<HttpOperationResponse<IList<string>>> GetTenantsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get all the topic stats by namespace
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse<object> GetTopics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get all the topic stats by namespace
        /// </summary>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.SerializationException">
        /// Thrown when unable to deserialize the response
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse<object>> GetTopicsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTopics2WithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of all the topics under a certain namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='mode'>
        /// Possible values include: 'PERSISTENT', 'NON_PERSISTENT', 'ALL'
        /// </param>
        public HttpOperationResponse<IList<string>> GetTopics(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(tenant, namespaceParameter, mode, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the topics under a certain namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='mode'>
        /// Possible values include: 'PERSISTENT', 'NON_PERSISTENT', 'ALL'
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetTopicsAsync(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTopicsWithHttpMessagesAsync(tenant, namespaceParameter, mode, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// get the version of the schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='schemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<LongSchemaVersion> GetVersionBySchema(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetVersionBySchemaAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// get the version of the schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='schemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<LongSchemaVersion>> GetVersionBySchemaAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetVersionBySchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Grant a new permission to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        /// <param name='permissions'>
        /// List of permissions for the specified role
        /// </param>
        public HttpOperationResponse GrantPermissionOnNamespace(string tenant, string namespaceParameter, string role, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GrantPermissionOnNamespaceAsync(tenant, namespaceParameter, role, permissions, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Grant a new permission to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        /// <param name='permissions'>
        /// List of permissions for the specified role
        /// </param>
        public async Task<HttpOperationResponse> GrantPermissionOnNamespaceAsync(string tenant, string namespaceParameter, string role, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GrantPermissionOnNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Grant a new permission to a role on a single topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='role'>
        /// Client role to which grant permissions
        /// </param>
        /// <param name='permissions'>
        /// Actions to be granted (produce,functions,consume)
        /// </param>
        public HttpOperationResponse GrantPermissionsOnTopic(string tenant, string namespaceParameter, string topic, string role, bool isPersistentTopic = true, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GrantPermissionsOnTopicAsync(tenant, namespaceParameter, topic, role, isPersistentTopic, permissions, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Grant a new permission to a role on a single topic.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='role'>
        /// Client role to which grant permissions
        /// </param>
        /// <param name='permissions'>
        /// Actions to be granted (produce,functions,consume)
        /// </param>
        public async Task<HttpOperationResponse> GrantPermissionsOnTopicAsync(string tenant, string namespaceParameter, string topic, string role, bool isPersistentTopic = true, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.GrantPermissionsOnTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GrantPermissionsOnTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Run a healthcheck against the broker
        /// </summary>
        public HttpOperationResponse Healthcheck(Dictionary<string, List<string>> customHeaders = null)
        {
            return HealthcheckAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Run a healthcheck against the broker
        /// </summary>
        public async Task<HttpOperationResponse> HealthcheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.HealthcheckWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Check if the broker is fully initialized
        /// </summary>
        public HttpOperationResponse IsReady(Dictionary<string, List<string>> customHeaders = null)
        {
            return IsReadyAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Check if the broker is fully initialized
        /// </summary>
        public async Task<HttpOperationResponse> IsReadyAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.IsReadyWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get version of current broker
        /// </summary>
        public HttpOperationResponse<string> Version(Dictionary<string, List<string>> customHeaders = null)
        {
            return VersionAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get version of current broker
        /// </summary>
        public async Task<HttpOperationResponse<string>> VersionAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.VersionWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Enable or disable broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='deduplicate'>
        /// Flag for disabling or enabling broker side deduplication for all topics in
        /// the specified namespace
        /// </param>
        public HttpOperationResponse ModifyDeduplication(string tenant, string namespaceParameter, bool deduplicate, Dictionary<string, List<string>> customHeaders = null)
        {
            return ModifyDeduplicationAsync(tenant, namespaceParameter, deduplicate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Enable or disable broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='deduplicate'>
        /// Flag for disabling or enabling broker side deduplication for all topics in
        /// the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> ModifyDeduplicationAsync(string tenant, string namespaceParameter, bool deduplicate, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ModifyDeduplicationWithHttpMessagesAsync(tenant, namespaceParameter, deduplicate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Message encryption is required or not for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='encryptionRequired'>
        /// Flag defining if message encryption is required
        /// </param>
        public HttpOperationResponse ModifyEncryptionRequired(string tenant, string namespaceParameter, bool encryptionRequired, Dictionary<string, List<string>> customHeaders = null)
        {
            return ModifyEncryptionRequiredAsync(tenant, namespaceParameter, encryptionRequired, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Message encryption is required or not for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='encryptionRequired'>
        /// Flag defining if message encryption is required
        /// </param>
        public async Task<HttpOperationResponse> ModifyEncryptionRequiredAsync(string tenant, string namespaceParameter, bool encryptionRequired, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ModifyEncryptionRequiredWithHttpMessagesAsync(tenant, namespaceParameter, encryptionRequired, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Offload a prefix of a topic to long term storage
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse<OffloadProcessStatus> OffloadStatus(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return OffloadStatusAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Offload a prefix of a topic to long term storage
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse<OffloadProcessStatus>> OffloadStatusAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.OffloadStatus1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.OffloadStatusWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Peek nth message on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscribed message expired
        /// </param>
        /// <param name='messagePosition'>
        /// The number of messages (default 1)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse PeekNthMessages(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {            
            return PeekNthMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Peek nth message on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscribed message expired
        /// </param>
        /// <param name='messagePosition'>
        /// The number of messages (default 1)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> PeekNthMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.PeekNthMessage1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, authoritative, customHeaders, cancellationToken ).ConfigureAwait(false);
            
            return await _api.PeekNthMessageWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, authoritative, customHeaders, cancellationToken ).ConfigureAwait(false);
        }
        /// <summary>
        /// Update the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='schemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<PostSchemaResponse> PostSchema(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return PostSchemaAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the schema of a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='schemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<PostSchemaResponse>> PostSchemaAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.PostSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove override of broker's allowAutoSubscriptionCreation in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveAutoSubscriptionCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveAutoSubscriptionCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoSubscriptionCreation in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveAutoSubscriptionCreationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove override of broker's allowAutoTopicCreation in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveAutoTopicCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveAutoTopicCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove override of broker's allowAutoTopicCreation in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveAutoTopicCreationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove a backlog quota policy from a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public HttpOperationResponse RemoveBacklogQuota(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveBacklogQuotaAsync(tenant, namespaceParameter, topic, isPersistentTopic, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove a backlog quota policy from a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public async Task<HttpOperationResponse> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveBacklogQuota2WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveBacklogQuota1WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove a backlog quota policy from a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public HttpOperationResponse RemoveBacklogQuota(string tenant, string namespaceParameter, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveBacklogQuotaAsync(tenant, namespaceParameter, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove a backlog quota policy from a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public async Task<HttpOperationResponse> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveBacklogQuotaWithHttpMessagesAsync(tenant, namespaceParameter, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveCompactionThreshold(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveCompactionThresholdAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveCompactionThresholdAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveCompactionThreshold1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name="isPersistentTopic"></param>
        public HttpOperationResponse RemoveDeduplication(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDeduplicationAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Remove broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name="isPersistentTopic"></param>
        public async Task<HttpOperationResponse> RemoveDeduplicationAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveDeduplication2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveDeduplication1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveDeduplication(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDeduplicationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Remove broker side deduplication for all topics in a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveDeduplicationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveDeduplicationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDispatchRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove inactive topic policies from a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove inactive topic policies from a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMaxConsumers(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxConsumersAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveMaxConsumers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveMaxConsumersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMaxConsumersPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxConsumersPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter,customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMaxConsumersPerSubscription(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveMaxConsumersPerSubscription2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxConsumersPerSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMaxMessageSize(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxMessageSizeAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxMessageSizeAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveMaxMessageSize1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxMessageSizeWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>

        public HttpOperationResponse RemoveMaxProducers(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxProducersAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxProducersAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveMaxProducers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxProducersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxProducersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxProducersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxProducersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveMaxSubscriptionsPerTopic2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveMaxSubscriptionsPerTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxSubscriptionsPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Get maxTopicsPerNamespace config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse GetMaxTopicsPerNamespace(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxTopicsPerNamespaceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get maxTopicsPerNamespace config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> GetMaxTopicsPerNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxTopicsPerNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveMessageTTL(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMessageTTLAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMessageTTLAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RemoveMessageTTL1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove anti-affinity group of a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove anti-affinity group of a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveNamespaceAntiAffinityGroupWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveNamespaceBundleResourceQuota(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveNamespaceBundleResourceQuotaAsync(tenant, namespaceParameter, bundle, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveNamespaceBundleResourceQuotaAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveNamespaceBundleResourceQuotaWithHttpMessagesAsync(tenant, namespaceParameter, bundle, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove message TTL for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveNamespaceMessageTTL(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveNamespaceMessageTTLAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove message TTL for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveNamespaceMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveOffloadPolicies(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveOffloadPoliciesAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveOffloadPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveOffloadPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveOffloadPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveOffloadPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveOffloadPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveOffloadPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemovePersistence(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemovePersistenceAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemovePersistenceAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemovePersistence1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemovePersistenceWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemovePublishRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemovePublishRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemovePublishRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemovePublishRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemovePublishRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public HttpOperationResponse GetReplicatorDispatchRate(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetReplicatorDispatchRateAsync(tenant, namespaceParameter, topic, applied, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='applied'>
        /// </param>
        public async Task<HttpOperationResponse> GetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool? applied = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.GetReplicatorDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.GetReplicatorDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, applied, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveReplicatorDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveReplicatorDispatchRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveReplicatorDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveReplicatorDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveReplicatorDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Replicator dispatch rate of the topic
        /// </param>
        public HttpOperationResponse SetReplicatorDispatchRate(string tenant, string namespaceParameter, string topic, DispatchRateImpl dispatchRate = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetReplicatorDispatchRateAsync(tenant, namespaceParameter, topic, dispatchRate, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set replicatorDispatchRate config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Replicator dispatch rate of the topic
        /// </param>
        public async Task<HttpOperationResponse> SetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, string topic, DispatchRateImpl dispatchRate = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.SetReplicatorDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.SetReplicatorDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveRetention(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveRetentionAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveRetentionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveRetention2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveRetention1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public HttpOperationResponse RemoveRetention(string tenant, string namespaceParameter, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveRetentionAsync(tenant, namespaceParameter, retentionPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> RemoveRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveRetentionWithHttpMessagesAsync(tenant, namespaceParameter, retentionPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveSubscribeRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveSubscribeRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveSubscribeRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveSubscribeRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveSubscribeRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove subscription message dispatch rate configuration for specified
        /// topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse RemoveSubscriptionDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveSubscriptionDispatchRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove subscription message dispatch rate configuration for specified
        /// topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.RemoveSubscriptionDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveSubscriptionDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove subscription message dispatch rate configuration for specified
        /// topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public HttpOperationResponse GetSubscriptionTypesEnabled(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, topic, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get is enable sub type fors specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        public async Task<HttpOperationResponse> GetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.GetSubscriptionTypesEnabled2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.GetSubscriptionTypesEnabled1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set is enable sub types for specified topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='subTypes'>
        /// Enable sub types for the specified topic
        /// </param>
        public HttpOperationResponse SetSubscriptionTypesEnabled(string tenant, string namespaceParameter, string topic, IList<string> subTypes, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, topic, subTypes, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set is enable sub types for specified topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='subTypes'>
        /// Enable sub types for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, string topic, IList<string> subTypes, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.SetSubscriptionTypesEnabled2WithHttpMessagesAsync(tenant, namespaceParameter, topic, subTypes, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.SetSubscriptionTypesEnabled1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subTypes, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Reset subscription to message position closest to absolute timestamp (in
        /// ms).
        /// </summary>
        /// <remarks>
        /// It fence cursor and disconnects all active consumers before reseting
        /// cursor.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to reset position on
        /// </param>
        /// <param name='timestamp'>
        /// time in minutes to reset back to (or minutes, hours, days, weeks eg:100m,
        /// 3h, 2d, 5w)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse ResetCursor(string tenant, string namespaceParameter, string topic, string subName, long timestamp, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ResetCursorAsync(tenant, namespaceParameter, topic, subName, timestamp, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Reset subscription to message position closest to absolute timestamp (in
        /// ms).
        /// </summary>
        /// <remarks>
        /// It fence cursor and disconnects all active consumers before reseting
        /// cursor.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to reset position on
        /// </param>
        /// <param name='timestamp'>
        /// time in minutes to reset back to (or minutes, hours, days, weeks eg:100m,
        /// 3h, 2d, 5w)
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> ResetCursorAsync(string tenant, string namespaceParameter, string topic, string subName, long timestamp, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.ResetCursor1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, timestamp, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ResetCursorWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, timestamp, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Reset subscription to message position closest to given position.
        /// </summary>
        /// <remarks>
        /// It fence cursor and disconnects all active consumers before reseting
        /// cursor.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to reset position on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId to reset back to (ledgerId:entryId)
        /// </param>
        public HttpOperationResponse RResetCursorOnPosition(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return ResetCursorOnPositionAsync(tenant, namespaceParameter, topic, subName, isPersistentTopic, authoritative, messageId, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Reset subscription to message position closest to given position.
        /// </summary>
        /// <remarks>
        /// It fence cursor and disconnects all active consumers before reseting
        /// cursor.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Subscription to reset position on
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        /// <param name='messageId'>
        /// messageId to reset back to (ledgerId:entryId)
        /// </param>
        public async Task<HttpOperationResponse> ResetCursorOnPositionAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.ResetCursorOnPosition1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ResetCursorOnPositionWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Revoke all permissions to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        public HttpOperationResponse RevokePermissionsOnNamespace(string tenant, string namespaceParameter, string role, Dictionary<string, List<string>> customHeaders = null)
        {
            return RevokePermissionsOnNamespaceAsync(tenant, namespaceParameter, role, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Revoke all permissions to a role on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='role'>
        /// </param>
        public async Task<HttpOperationResponse> RevokePermissionsOnNamespaceAsync(string tenant, string namespaceParameter, string role, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RevokePermissionsOnNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, role, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Revoke permissions on a topic.
        /// </summary>
        /// <remarks>
        /// Revoke permissions to a role on a single topic. If the permission was not
        /// set at the topiclevel, but rather at the namespace level, this operation
        /// will return an error (HTTP status code 412).
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='role'>
        /// Client role to which grant permissions
        /// </param>
        public HttpOperationResponse RevokePermissionsOnTopic(string tenant, string namespaceParameter, string topic, string role, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RevokePermissionsOnTopicAsync(tenant, namespaceParameter, topic, role, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Revoke permissions on a topic.
        /// </summary>
        /// <remarks>
        /// Revoke permissions to a role on a single topic. If the permission was not
        /// set at the topiclevel, but rather at the namespace level, this operation
        /// will return an error (HTTP status code 412).
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='role'>
        /// Client role to which grant permissions
        /// </param>
        public async Task<HttpOperationResponse> RevokePermissionsOnTopicAsync(string tenant, string namespaceParameter, string topic, string role, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.RevokePermissionsOnTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, role, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RevokePermissionsOnTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, role, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Override broker's allowAutoSubscriptionCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoSubscriptionCreationOverride'>
        /// Settings for automatic subscription creation
        /// </param>
        public HttpOperationResponse SetAutoSubscriptionCreation(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride autoSubscriptionCreationOverride = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetAutoSubscriptionCreationAsync(tenant, namespaceParameter, autoSubscriptionCreationOverride, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Override broker's allowAutoSubscriptionCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoSubscriptionCreationOverride'>
        /// Settings for automatic subscription creation
        /// </param>
        public async Task<HttpOperationResponse> SetAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride autoSubscriptionCreationOverride = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetAutoSubscriptionCreationWithHttpMessagesAsync(tenant, namespaceParameter, autoSubscriptionCreationOverride, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Override broker's allowAutoTopicCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoTopicCreation'>
        /// Settings for automatic topic creation
        /// </param>
        public HttpOperationResponse SetAutoTopicCreation(string tenant, string namespaceParameter, AutoTopicCreationOverride autoTopicCreation, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetAutoTopicCreationAsync(tenant, namespaceParameter, autoTopicCreation, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Override broker's allowAutoTopicCreation setting for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoTopicCreation'>
        /// Settings for automatic topic creation
        /// </param>
        public async Task<HttpOperationResponse> SetAutoTopicCreationAsync(string tenant, string namespaceParameter, AutoTopicCreationOverride autoTopicCreation, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetAutoTopicCreationWithHttpMessagesAsync(tenant, namespaceParameter, autoTopicCreation, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set a backlog quota for a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public HttpOperationResponse SetBacklogQuota(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetBacklogQuotaAsync(tenant, namespaceParameter, topic, isPersistentTopic, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a backlog quota for a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        public async Task<HttpOperationResponse> SetBacklogQuotaAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetBacklogQuota2WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetBacklogQuota1WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set a backlog quota for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        /// <param name='backlogQuota'>
        /// Backlog quota for all topics of the specified namespace
        /// </param>
        public HttpOperationResponse SetBacklogQuota(string tenant, string namespaceParameter, string backlogQuotaType = null, BacklogQuota backlogQuota = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetBacklogQuotaAsync(tenant, namespaceParameter, backlogQuotaType, backlogQuota, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a backlog quota for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='backlogQuotaType'>
        /// Possible values include: 'destination_storage', 'message_age'
        /// </param>
        /// <param name='backlogQuota'>
        /// Backlog quota for all topics of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = null, BacklogQuota backlogQuota = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetBacklogQuotaWithHttpMessagesAsync(tenant, namespaceParameter, backlogQuotaType, backlogQuota, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set the bookie-affinity-group to namespace-persistent policy.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bookieAffinityGroupData'>
        /// Bookie affinity group for the specified namespace
        /// </param>
        public HttpOperationResponse SetBookieAffinityGroup(string tenant, string namespaceParameter, BookieAffinityGroupData bookieAffinityGroupData = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetBookieAffinityGroupAsync(tenant, namespaceParameter, bookieAffinityGroupData, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the bookie-affinity-group to namespace-persistent policy.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bookieAffinityGroupData'>
        /// Bookie affinity group for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetBookieAffinityGroupAsync(string tenant, string namespaceParameter, BookieAffinityGroupData bookieAffinityGroupData = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetBookieAffinityGroupWithHttpMessagesAsync(tenant, namespaceParameter, bookieAffinityGroupData, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get key value pair properties for a given namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse GetProperties(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPropertiesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get key value pair properties for a given namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> GetPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPropertiesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Put key value pairs property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='properties'>
        /// Key value pair properties for the namespace
        /// </param>
        public HttpOperationResponse SetProperties(string tenant, string namespaceParameter, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPropertiesAsync(tenant, namespaceParameter, properties, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Put key value pairs property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='properties'>
        /// Key value pair properties for the namespace
        /// </param>
        public async Task<HttpOperationResponse> SetPropertiesAsync(string tenant, string namespaceParameter, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetPropertiesWithHttpMessagesAsync(tenant, namespaceParameter, properties, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Clear properties on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse ClearProperties(string tenant, string namespaceParameter, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearPropertiesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Clear properties on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> ClearPropertiesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearPropertiesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        public HttpOperationResponse GetProperty(string tenant, string namespaceParameter, string key, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPropertyAsync(tenant, namespaceParameter, key, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        public async Task<HttpOperationResponse> GetPropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPropertyWithHttpMessagesAsync(tenant, namespaceParameter, key, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        public HttpOperationResponse RemoveProperty(string tenant, string namespaceParameter, string key, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemovePropertyAsync(tenant, namespaceParameter, key, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove property value for a given key on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        public async Task<HttpOperationResponse> RemovePropertyAsync(string tenant, string namespaceParameter, string key, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemovePropertyWithHttpMessagesAsync(tenant, namespaceParameter, key, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Put a key value pair property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        /// <param name='value'>
        /// </param>
        public HttpOperationResponse SetProperty(string tenant, string namespaceParameter, string key, string value, IDictionary<string, string> properties, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPropertyAsync(tenant, namespaceParameter, key, value, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Put a key value pair property on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='key'>
        /// </param>
        /// <param name='value'>
        /// </param>
        public async Task<HttpOperationResponse> SetPropertyAsync(string tenant, string namespaceParameter, string key, string value, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetPropertyWithHttpMessagesAsync(tenant, namespaceParameter, key, value, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='body'>
        /// Dispatch rate for the specified topic
        /// </param>
        public HttpOperationResponse SetCompactionThreshold(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, long? threshold = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetCompactionThresholdAsync(tenant, namespaceParameter, topic, isPersistentTopic, threshold, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set compaction threshold configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='body'>
        /// Dispatch rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetCompactionThresholdAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, long? threshold = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetCompactionThreshold2WithHttpMessagesAsync(tenant, namespaceParameter, topic, threshold, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetCompactionThreshold1WithHttpMessagesAsync(tenant, namespaceParameter, topic, threshold, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='threshold'>
        /// Maximum number of uncompacted bytes in a topic of the specified namespace
        /// </param>
        public HttpOperationResponse SetCompactionThreshold(string tenant, string namespaceParameter, long threshold, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetCompactionThresholdAsync(tenant, namespaceParameter, threshold, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='threshold'>
        /// Maximum number of uncompacted bytes in a topic of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetCompactionThresholdAsync(string tenant, string namespaceParameter, long threshold, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, threshold, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeleteCompactionThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteCompactionThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete maximum number of uncompacted bytes in a topic before compaction is
        /// triggered.
        /// </summary>
        /// <remarks>
        /// The backlog size is compared to the threshold periodically. A threshold of
        /// 0 disabled automatic compaction
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='interval'>
        /// Interval to take deduplication snapshot for the specified topic
        /// </param>
        public HttpOperationResponse SetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? interval = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, topic, isPersistentTopic, interval, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set deduplicationSnapshotInterval config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='interval'>
        /// Interval to take deduplication snapshot for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? interval = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetDeduplicationSnapshotInterval2WithHttpMessagesAsync(tenant, namespaceParameter, topic, interval, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetDeduplicationSnapshotInterval1WithHttpMessagesAsync(tenant, namespaceParameter, topic, interval, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set deduplicationSnapshotInterval config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='interval'>
        /// Interval to take deduplication snapshot per topic
        /// </param>
        public HttpOperationResponse SetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, int interval, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, interval, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set deduplicationSnapshotInterval config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='interval'>
        /// Interval to take deduplication snapshot per topic
        /// </param>
        public async Task<HttpOperationResponse> SetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, int interval, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetDeduplicationSnapshotIntervalWithHttpMessagesAsync(tenant, namespaceParameter, interval, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set the default quota
        /// </summary>
        /// <param name='resourceQuota'>
        /// Default resource quota
        /// </param>
        public HttpOperationResponse<IList<string>> SetDefaultResourceQuota(ResourceQuota resourceQuota = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDefaultResourceQuotaAsync(resourceQuota, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the default quota
        /// </summary>
        /// <param name='resourceQuota'>
        /// Default resource quota
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> SetDefaultResourceQuotaAsync(ResourceQuota resourceQuota = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetDefaultResourceQuotaWithHttpMessagesAsync(resourceQuota, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the list of all the resourcegroups.
        /// </summary>
        public HttpOperationResponse<IList<string>> GetResourceGroups(ResourceQuota resourceQuota = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetResourceGroupsAsync(customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the list of all the resourcegroups.
        /// </summary>
        public async Task<HttpOperationResponse<IList<string>>> GetResourceGroupsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetResourceGroupsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the rate limiters specified for a resourcegroup.
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        public HttpOperationResponse<ResourceGroup> GetResourceGroup(string resourceGroup, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetResourceGroupAsync(resourceGroup, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the rate limiters specified for a resourcegroup.
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        public async Task<HttpOperationResponse<ResourceGroup>> GetResourceGroupAsync(string resourceGroup, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetResourceGroupWithHttpMessagesAsync(resourceGroup, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete a resourcegroup.
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        public HttpOperationResponse DeleteResourceGroup(string resourceGroup, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteResourceGroupAsync(resourceGroup, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete a resourcegroup.
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteResourceGroupAsync(string resourceGroup, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteResourceGroupWithHttpMessagesAsync(resourceGroup, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Creates a new resourcegroup with the specified rate limiters
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        /// <param name='resource'>
        /// Rate limiters for the resourcegroup
        /// </param>
        public HttpOperationResponse CreateOrUpdateResourceGroup(string resourceGroup, ResourceGroup resource, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateOrUpdateResourceGroupAsync(resourceGroup, resource, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Creates a new resourcegroup with the specified rate limiters
        /// </summary>
        /// <param name='resourcegroup'>
        /// </param>
        /// <param name='resource'>
        /// Rate limiters for the resourcegroup
        /// </param>
        public async Task<HttpOperationResponse> CreateOrUpdateResourceGroupAsync(string resourceGroup, ResourceGroup resource, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateOrUpdateResourceGroupWithHttpMessagesAsync(resourceGroup, resource, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='delayedDeliveryPolicies'>
        /// Delayed delivery policies for the specified topic
        /// </param>
        public HttpOperationResponse SetDelayedDeliveryPolicies(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DelayedDeliveryPolicies delayedDeliveryPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, topic, isPersistentTopic, delayedDeliveryPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set delayed delivery messages config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='delayedDeliveryPolicies'>
        /// Delayed delivery policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DelayedDeliveryPolicies delayedDeliveryPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetDelayedDeliveryPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, delayedDeliveryPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetDelayedDeliveryPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, delayedDeliveryPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='delayedDeliveryPolicies'>
        /// Delayed delivery policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetDelayedDeliveryPolicies(string tenant, string namespaceParameter, DelayedDeliveryPolicies delayedDeliveryPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, delayedDeliveryPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set delayed delivery messages config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='delayedDeliveryPolicies'>
        /// Delayed delivery policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, DelayedDeliveryPolicies delayedDeliveryPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, delayedDeliveryPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Dispatch rate for the specified topic
        /// </param>
        public HttpOperationResponse SetDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDispatchRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, dispatchRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Dispatch rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Dispatch rate for all topics of the specified namespace
        /// </param>
        public HttpOperationResponse SetDispatchRate(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetDispatchRateAsync(tenant, namespaceParameter, dispatchRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Dispatch rate for all topics of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeleteDispatchRate(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set the failure domain of the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>
        /// <param name='failureDomain'>
        /// The configuration data of a failure domain
        /// </param>

        public HttpOperationResponse SetFailureDomain(string cluster, string domainName, FailureDomain failureDomain, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetFailureDomainAsync(cluster, domainName, failureDomain, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the failure domain of the cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='domainName'>
        /// The failure domain name
        /// </param>
        /// <param name='failureDomain'>
        /// The configuration data of a failure domain
        /// </param>
        public async Task<HttpOperationResponse> SetFailureDomainAsync(string cluster, string domainName, FailureDomain failureDomain, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetFailureDomainWithHttpMessagesAsync(cluster, domainName, failureDomain, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxTopicsPerNamespace config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='maxTopicsPerNamespace'>
        /// Number of maximum topics for specific namespace
        /// </param>
        public HttpOperationResponse SetInactiveTopicPolicies(string tenant, string namespaceParameter, int maxTopicsPerNamespace, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, maxTopicsPerNamespace, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxTopicsPerNamespace config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='maxTopicsPerNamespace'>
        /// Number of maximum topics for specific namespace
        /// </param>
        public async Task<HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, int maxTopicsPerNamespace, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetInactiveTopicPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetInactiveTopicPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, maxTopicsPerNamespace, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='policies'>
        /// inactive topic policies for the specified topic
        /// </param>
        public HttpOperationResponse SetInactiveTopicPolicies(string tenant, string namespaceParameter, string topic, InactiveTopicPolicies policies = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, topic, policies, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set inactive topic policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='policies'>
        /// inactive topic policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, string topic, InactiveTopicPolicies policies = null, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetInactiveTopicPolicies4WithHttpMessagesAsync(tenant, namespaceParameter, topic, policies, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetInactiveTopicPolicies3WithHttpMessagesAsync(tenant, namespaceParameter, topic, policies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set inactive topic policies config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='inactiveTopicPolicies'>
        /// Inactive topic policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetInactiveTopicPolicies(string tenant, string namespaceParameter, InactiveTopicPolicies inactiveTopicPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetInactiveTopicPoliciesAsync(tenant, namespaceParameter, inactiveTopicPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set inactive topic policies config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='inactiveTopicPolicies'>
        /// Inactive topic policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, InactiveTopicPolicies inactiveTopicPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {

            return await _api.SetInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, inactiveTopicPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update flag of whether allow auto update schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoUpdate'>
        /// Flag of whether to allow auto update schema
        /// </param>
        public HttpOperationResponse SetIsAllowAutoUpdateSchema(string tenant, string namespaceParameter, bool autoUpdate, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetIsAllowAutoUpdateSchemaAsync(tenant, namespaceParameter, autoUpdate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update flag of whether allow auto update schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='autoUpdate'>
        /// Flag of whether to allow auto update schema
        /// </param>
        public async Task<HttpOperationResponse> SetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, bool autoUpdate, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetIsAllowAutoUpdateSchemaWithHttpMessagesAsync(tenant, namespaceParameter, autoUpdate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// The max consumers of the topic
        /// </param>
        public HttpOperationResponse SetMaxConsumer(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxConsumerAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// The max consumers of the topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxConsumerAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetMaxConsumers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetMaxConsumersWithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Dispatch rate for the specified topic
        /// </param>
        public HttpOperationResponse SetMaxConsumersPerSubscription(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set max consumers per subscription configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Dispatch rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetMaxConsumersPerSubscription2WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetMaxConsumersPerSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum consumers per subscription
        /// </param>
        public HttpOperationResponse SetMaxConsumersPerSubscription(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum consumers per subscription
        /// </param>
        public async Task<HttpOperationResponse> SetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxConsumersPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum consumers per topic
        /// </param>
        public HttpOperationResponse SetMaxConsumersPerTopic(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxConsumersPerTopicAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum consumers per topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxConsumersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveMaxConsumersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxConsumersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='messageSize'>
        /// The max message size of the topic
        /// </param>
        public HttpOperationResponse SetMaxMessageSize(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? messageSize = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxMessageSizeAsync(tenant, namespaceParameter, topic, isPersistentTopic, messageSize, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxMessageSize config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='messageSize'>
        /// The max message size of the topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxMessageSizeAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? messageSize = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetMaxMessageSize1WithHttpMessagesAsync(tenant, namespaceParameter, topic, messageSize, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetMaxMessageSizeWithHttpMessagesAsync(tenant, namespaceParameter, topic, messageSize, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// The max producers of the topic
        /// </param>
        public HttpOperationResponse SetMaxProducers(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxProducersAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Set maxProducers config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// The max producers of the topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxProducersAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetMaxProducers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetMaxProducersWithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxProducersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum producers per topic
        /// </param>
        public HttpOperationResponse SetMaxProducersPerTopic(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxProducersPerTopicAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxProducersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum producers per topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxProducersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='body'>
        /// The max subscriptions of the topic
        /// </param>
        public HttpOperationResponse SetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxSubscriptionsPerTopic config for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// The max subscriptions of the topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetMaxSubscriptionsPerTopic2WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetMaxSubscriptionsPerTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum subscriptions per topic
        /// </param>
        public HttpOperationResponse SetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxSubscriptionsPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum subscriptions per topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxSubscriptionsPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Max unacked messages on consumer policies for the specified topic
        /// </param>
        public HttpOperationResponse SetMaxUnackedMessagesOnConsumer(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxUnackedMessagesOnConsumerAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set max unacked messages per consumer config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Max unacked messages on consumer policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxUnackedMessagesOnConsumerAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.SetMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.SetMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Max unacked messages on subscription policies for the specified topic
        /// </param>
        public HttpOperationResponse SetMaxUnackedMessagesOnSubscription(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxUnackedMessagesOnSubscriptionAsync(tenant, namespaceParameter, topic, isPersistentTopic, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set max unacked messages per subscription config on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='max'>
        /// Max unacked messages on subscription policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetMaxUnackedMessagesOnSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, int? max = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.SetMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.SetMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum unacked messages per consumer
        /// </param>
        public HttpOperationResponse SetMaxUnackedMessagesPerConsumer(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxUnackedMessagesPerConsumerAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxConsumersPerTopic configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum unacked messages per consumer
        /// </param>
        public async Task<HttpOperationResponse> SetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxUnackedMessagesPerConsumerWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveMaxUnackedmessagesPerConsumer(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxUnackedmessagesPerConsumerAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerConsumer config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxUnackedmessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxUnackedmessagesPerConsumerWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maxUnackedMessagesPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum unacked messages per subscription
        /// </param>
        public HttpOperationResponse SetMaxUnackedMessagesPerSubscription(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMaxUnackedMessagesPerSubscriptionAsync(tenant, namespaceParameter, max, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maxUnackedMessagesPerSubscription configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='max'>
        /// Number of maximum unacked messages per subscription
        /// </param>
        public async Task<HttpOperationResponse> SetMaxUnackedMessagesPerSubscriptionAsync(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetMaxUnackedMessagesPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, max, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveMaxUnackedmessagesPerSubscription(string tenant, string namespaceParameter, int max, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxUnackedmessagesPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove maxUnackedMessagesPerSubscription config on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveMaxUnackedmessagesPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxUnackedmessagesPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='messageTTL'>
        /// TTL in seconds for the specified namespace
        /// </param>
        public HttpOperationResponse SetMessageTTL(string tenant, string namespaceParameter, string topic, int messageTTL, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetMessageTTLAsync(tenant, namespaceParameter, topic, messageTTL, isPersistentTopic, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message TTL in seconds for a topic
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='messageTTL'>
        /// TTL in seconds for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetMessageTTLAsync(string tenant, string namespaceParameter, string topic, int messageTTL, bool isPersistentTopic = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.SetMessageTTL1WithHttpMessagesAsync(tenant, namespaceParameter, topic, messageTTL, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.SetMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, topic, messageTTL, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set anti-affinity group for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Anti-affinity group for the specified namespace
        /// </param>
        public HttpOperationResponse SetNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, string antiAffinityGroup, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, antiAffinityGroup, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set anti-affinity group for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Anti-affinity group for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetNamespaceAntiAffinityGroupAsync(string tenant, string namespaceParameter, string antiAffinityGroup, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceAntiAffinityGroupWithHttpMessagesAsync(tenant, namespaceParameter, antiAffinityGroup, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse SetNamespaceBundleResourceQuota(string tenant, string namespaceParameter, string bundle, ResourceQuota resourceQuota = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceBundleResourceQuotaAsync(tenant, namespaceParameter, bundle, resourceQuota, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> SetNamespaceBundleResourceQuotaAsync(string tenant, string namespaceParameter, string bundle, ResourceQuota resourceQuota = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceBundleResourceQuotaWithHttpMessagesAsync(tenant, namespaceParameter, bundle, resourceQuota, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
        /// </param>
        /// <param name='namespaceIsolationData'>
        /// The namespace isolation policy data
        /// </param>
        public HttpOperationResponse SetNamespaceIsolationPolicy(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceIsolationPolicyAsync(cluster, policyName, namespaceIsolationData, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set namespace isolation policy.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='policyName'>
        /// The namespace isolation policy name
        /// </param>
        /// <param name='namespaceIsolationData'>
        /// The namespace isolation policy data
        /// </param>
        public async Task<HttpOperationResponse> SetNamespaceIsolationPolicyAsync(string cluster, string policyName, NamespaceIsolationData namespaceIsolationData, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, policyName, namespaceIsolationData, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set message TTL in seconds for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// TTL in seconds for the specified namespace
        /// </param>
        public HttpOperationResponse SetNamespaceMessageTTL(string tenant, string namespaceParameter, int ttl, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceMessageTTLAsync(tenant, namespaceParameter, ttl, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message TTL in seconds for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// TTL in seconds for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, int ttl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, ttl, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set the replication clusters for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='clusters'>
        /// List of replication clusters
        /// </param>
        public HttpOperationResponse SetNamespaceReplicationClusters(string tenant, string namespaceParameter, IList<string> clusters, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceReplicationClustersAsync(tenant, namespaceParameter, clusters, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the replication clusters for a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='clusters'>
        /// List of replication clusters
        /// </param>
        public async Task<HttpOperationResponse> SetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, IList<string> clusters, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceReplicationClustersWithHttpMessagesAsync(tenant, namespaceParameter, clusters, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded from the Pulsar cluster's local storage (i.e.
        /// BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value disables the deletion completely.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='deletionLag'>
        /// New number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded
        /// </param>
        public HttpOperationResponse SetOffloadDeletionLag(string tenant, string namespaceParameter, long deletionLag, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetOffloadDeletionLagAsync(tenant, namespaceParameter, deletionLag, customHeaders).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Set number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded from the Pulsar cluster's local storage (i.e.
        /// BookKeeper)
        /// </summary>
        /// <remarks>
        /// A negative value disables the deletion completely.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='deletionLag'>
        /// New number of milliseconds to wait before deleting a ledger segment which
        /// has been offloaded
        /// </param>
        public async Task<HttpOperationResponse> SetOffloadDeletionLagAsync(string tenant, string namespaceParameter, long deletionLag, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetOffloadDeletionLagWithHttpMessagesAsync(tenant, namespaceParameter, deletionLag, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='offloadPolicies'>
        /// Offload policies for the specified topic
        /// </param>
        public HttpOperationResponse SetOffloadPolicies(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, OffloadPoliciesImpl offloadPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetOffloadPoliciesAsync(tenant, namespaceParameter, topic, isPersistentTopic, offloadPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set offload policies on a topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='offloadPolicies'>
        /// Offload policies for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetOffloadPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, OffloadPoliciesImpl offloadPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetOffloadPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, offloadPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetOffloadPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, offloadPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Offload policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetOffloadPolicies(string tenant, string namespaceParameter, OffloadPoliciesImpl offloadPolicies, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetOffloadPoliciesAsync(tenant, namespaceParameter, offloadPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set offload configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Offload policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetOffloadPoliciesAsync(string tenant, string namespaceParameter, OffloadPoliciesImpl offloadPolicies, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetOffloadPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, offloadPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set maximum number of bytes stored on the pulsar cluster for a topic,
        /// before the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// -1 will revert to using the cluster default. A negative value disables
        /// automatic offloading.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='threshold'>
        /// Maximum number of bytes stored on the pulsar cluster for a topic of the
        /// specified namespace
        /// </param>
        public HttpOperationResponse SetOffloadThreshold(string tenant, string namespaceParameter, long threshold, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetOffloadThresholdAsync(tenant, namespaceParameter, threshold, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set maximum number of bytes stored on the pulsar cluster for a topic,
        /// before the broker will start offloading to longterm storage
        /// </summary>
        /// <remarks>
        /// -1 will revert to using the cluster default. A negative value disables
        /// automatic offloading.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='threshold'>
        /// Maximum number of bytes stored on the pulsar cluster for a topic of the
        /// specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetOffloadThresholdAsync(string tenant, string namespaceParameter, long threshold, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetOffloadThresholdWithHttpMessagesAsync(tenant, namespaceParameter, threshold, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update peer-cluster-list for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='clusterNames'>
        /// The list of peer cluster names
        /// </param>
        public HttpOperationResponse SetPeerClusterNames(string cluster, IList<string> clusterNames, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPeerClusterNamesAsync(cluster, clusterNames, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update peer-cluster-list for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        /// <param name='clusterNames'>
        /// The list of peer cluster names
        /// </param>
        public async Task<HttpOperationResponse> SetPeerClusterNamesAsync(string cluster, IList<string> clusterNames, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetPeerClusterNamesWithHttpMessagesAsync(cluster, clusterNames, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='persistencePolicies'>
        /// Bookkeeper persistence policies for specified topic
        /// </param>
        public HttpOperationResponse SetPersistence(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, PersistencePolicies persistencePolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPersistenceAsync(tenant, namespaceParameter, topic, isPersistentTopic, persistencePolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set configuration of persistence policies for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='persistencePolicies'>
        /// Bookkeeper persistence policies for specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetPersistenceAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, PersistencePolicies persistencePolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetPersistence2WithHttpMessagesAsync(tenant, namespaceParameter, topic, persistencePolicies, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetPersistence1WithHttpMessagesAsync(tenant, namespaceParameter, topic, persistencePolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set the persistence configuration for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='persistencePolicies'>
        /// Persistence policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetPersistence(string tenant, string namespaceParameter, PersistencePolicies persistencePolicies, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPersistenceAsync(tenant, namespaceParameter, persistencePolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set the persistence configuration for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='persistencePolicies'>
        /// Persistence policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetPersistenceAsync(string tenant, string namespaceParameter, PersistencePolicies persistencePolicies, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetPersistenceWithHttpMessagesAsync(tenant, namespaceParameter, persistencePolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete the persistence configuration for all topics on a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeletePersistence(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeletePersistenceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete the persistence configuration for all topics on a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeletePersistenceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeletePersistenceWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='publishRate'>
        /// Dispatch rate for the specified topic
        /// </param>
        public HttpOperationResponse SetPublishRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, PublishRate publishRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetPublishRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, publishRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set message publish rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='publishRate'>
        /// Dispatch rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetPublishRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, PublishRate publishRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetPublishRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, publishRate, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetPublishRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, publishRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Replicator dispatch rate for all topics of the specified namespace
        /// </param>
        public HttpOperationResponse SetReplicatorDispatchRate(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetReplicatorDispatchRateAsync(tenant, namespaceParameter, dispatchRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Replicator dispatch rate for all topics of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetReplicatorDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Remove replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveReplicatorDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveReplicatorDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Remove replicator dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveReplicatorDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the resourcegroup attached to the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<string> GetNamespaceResourceGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceResourceGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Get the resourcegroup attached to the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<string>> GetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceResourceGroupWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete resourcegroup for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse RemoveNamespaceResourceGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveNamespaceResourceGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete resourcegroup for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> RemoveNamespaceResourceGroupAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveNamespaceResourceGroupWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set resourcegroup for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='group'>
        /// Name of resourcegroup
        /// </param>
        public HttpOperationResponse SetNamespaceResourceGroup(string tenant, string namespaceParameter, string group, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetNamespaceResourceGroupAsync(tenant, namespaceParameter, group, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set resourcegroup for a namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='group'>
        /// Name of resourcegroup
        /// </param>
        public async Task<HttpOperationResponse> SetNamespaceResourceGroupAsync(string tenant, string namespaceParameter, string group, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetNamespaceResourceGroupWithHttpMessagesAsync(tenant, namespaceParameter, group, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetRetention(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetRetentionAsync(tenant, namespaceParameter, topic, isPersistentTopic, retentionPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set retention configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetRetentionAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetRetention2WithHttpMessagesAsync(tenant, namespaceParameter, topic, retentionPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetRetention1WithHttpMessagesAsync(tenant, namespaceParameter, topic, retentionPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public HttpOperationResponse SetRetention(string tenant, string namespaceParameter, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetRetentionAsync(tenant, namespaceParameter, retentionPolicies, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set retention configuration on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='retentionPolicies'>
        /// Retention policies for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetRetentionAsync(string tenant, string namespaceParameter, RetentionPolicies retentionPolicies = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetRetentionWithHttpMessagesAsync(tenant, namespaceParameter, retentionPolicies, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schemas,
        /// provided by producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='strategy'>
        /// Strategy used to check the compatibility of new schemas
        /// </param>
        public HttpOperationResponse SetSchemaAutoUpdateCompatibilityStrategy(string tenant, string namespaceParameter, string strategy = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSchemaAutoUpdateCompatibilityStrategyAsync(tenant, namespaceParameter, strategy, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schemas,
        /// provided by producers, before automatically updating the schema
        /// </summary>
        /// <remarks>
        /// The value AutoUpdateDisabled prevents producers from updating the schema.
        /// If set to AutoUpdateDisabled, schemas must be updated through the REST api
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='strategy'>
        /// Strategy used to check the compatibility of new schemas
        /// </param>
        public async Task<HttpOperationResponse> SetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, string strategy = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSchemaAutoUpdateCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, strategy, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='strategy'>
        /// Strategy used to check the compatibility of new schema
        /// </param>
        public HttpOperationResponse SetSchemaCompatibilityStrategy(string tenant, string namespaceParameter, string strategy = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSchemaCompatibilityStrategyAsync(tenant, namespaceParameter, strategy, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the strategy used to check the compatibility of new schema
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='strategy'>
        /// Strategy used to check the compatibility of new schema
        /// </param>
        public async Task<HttpOperationResponse> SetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, string strategy = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSchemaCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, strategy, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set schema validation enforced flag on namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='enforced'>
        /// Flag of whether validation is enforced on the specified namespace
        /// </param>
        public HttpOperationResponse SetSchemaValidtionEnforced(string tenant, string namespaceParameter, bool enforced, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSchemaValidtionEnforcedAsync(tenant, namespaceParameter, enforced, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set schema validation enforced flag on namespace.
        /// </summary>
        /// <remarks>
        /// If the flag is set to true, when a producer without a schema attempts to
        /// produce to a topic with schema in this namespace, the producer will be
        /// failed to connect. PLEASE be carefully on using this, since non-java
        /// clients don't support schema.if you enable this setting, it will cause
        /// non-java clients failed to produce.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='enforced'>
        /// Flag of whether validation is enforced on the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, bool enforced, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSchemaValidtionEnforcedWithHttpMessagesAsync(tenant, namespaceParameter, enforced, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='subscribeRate'>
        /// Subscribe rate for the specified topic
        /// </param>
        public HttpOperationResponse SetSubscribeRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, SubscribeRate subscribeRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscribeRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, subscribeRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscribe rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='subscribeRate'>
        /// Subscribe rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetSubscribeRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, SubscribeRate subscribeRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetSubscribeRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, subscribeRate, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetSubscribeRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subscribeRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set subscribe-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscribeRate'>
        /// Subscribe rate for all topics of the specified namespace
        /// </param>
        public HttpOperationResponse SetSubscribeRate(string tenant, string namespaceParameter, SubscribeRate subscribeRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscribeRateAsync(tenant, namespaceParameter, subscribeRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscribe-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscribeRate'>
        /// Subscribe rate for all topics of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetSubscribeRateAsync(string tenant, string namespaceParameter, SubscribeRate subscribeRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSubscribeRateWithHttpMessagesAsync(tenant, namespaceParameter, subscribeRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete subscribe-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeleteSubscribeRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSubscribeRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete subscribe-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteSubscribeRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set a subscription auth mode for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Subscription auth mode for all topics of the specified namespace
        /// </param>
        public HttpOperationResponse SetSubscriptionAuthMode(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionAuthModeAsync(tenant, namespaceParameter, mode, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set a subscription auth mode for all the topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='body'>
        /// Subscription auth mode for all topics of the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetSubscriptionAuthModeAsync(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSubscriptionAuthModeWithHttpMessagesAsync(tenant, namespaceParameter, mode, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Set subscription message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Subscription message dispatch rate for the specified topic
        /// </param>
        public HttpOperationResponse SetSubscriptionDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionDispatchRateAsync(tenant, namespaceParameter, topic, isPersistentTopic, dispatchRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscription message dispatch rate configuration for specified topic.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='dispatchRate'>
        /// Subscription message dispatch rate for the specified topic
        /// </param>
        public async Task<HttpOperationResponse> SetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SetSubscriptionDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SetSubscriptionDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set Subscription dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        public HttpOperationResponse SetSubscriptionDispatchRate(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionDispatchRateAsync(tenant, namespaceParameter, dispatchRate, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set Subscription dispatch-rate throttling for all topics of the namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='dispatchRate'>
        public async Task<HttpOperationResponse> SetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, DispatchRateImpl dispatchRate = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSubscriptionDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, dispatchRate, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Delete Subscription dispatch-rate throttling for all topics of the
        /// namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse DeleteSubscriptionDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSubscriptionDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Delete Subscription dispatch-rate throttling for all topics of the
        /// namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> DeleteSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteSubscriptionDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Set subscription expiration time in minutes for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='expirationTime'>
        /// Expiration time in minutes for the specified namespace
        /// </param>
        public HttpOperationResponse SetSubscriptionExpirationTime(string tenant, string namespaceParameter, int expirationTime, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionExpirationTimeAsync(tenant, namespaceParameter, expirationTime, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Set subscription expiration time in minutes for namespace
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='expirationTime'>
        /// Expiration time in minutes for the specified namespace
        /// </param>
        public async Task<HttpOperationResponse> SetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, int expirationTime, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSubscriptionExpirationTimeWithHttpMessagesAsync(tenant, namespaceParameter, expirationTime, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update set of whether allow share sub type
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='allowedSubs'>
        /// Set of whether allow subscription types
        /// </param>
        public HttpOperationResponse SetSubscriptionTypesEnabled(string tenant, string namespaceParameter, IList<string> allowedSubs, Dictionary<string, List<string>> customHeaders = null)
        {
            return SetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, allowedSubs, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update set of whether allow share sub type
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='allowedSubs'>
        /// Set of whether allow subscription types
        /// </param>
        public async Task<HttpOperationResponse> SetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, IList<string> allowedSubs, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SetSubscriptionTypesEnabledWithHttpMessagesAsync(tenant, namespaceParameter, allowedSubs, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// The set of whether allow subscription types
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse<IList<string>> GetSubscriptionTypesEnabled(string tenant, string namespaceParameter, int expirationTime, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionTypesEnabledAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// The set of whether allow subscription types
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse<IList<string>>> GetSubscriptionTypesEnabledAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscriptionTypesEnabledWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Skip all messages on a topic subscription.
        /// </summary>
        /// <remarks>
        /// Completely clears the backlog on the subscription.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Name of subscription
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse SkipAllMessages(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return SkipAllMessagesAsync(tenant, namespaceParameter, topic, subName, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Skip all messages on a topic subscription.
        /// </summary>
        /// <remarks>
        /// Completely clears the backlog on the subscription.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Name of subscription
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> SkipAllMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SkipAllMessages1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SkipAllMessagesWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Skipping messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Name of subscription
        /// </param>
        /// <param name='numMessages'>
        /// The number of messages to skip
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse SkipMessages(string tenant, string namespaceParameter, string topic, string subName, int numMessages, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return SkipMessagesAsync(tenant, namespaceParameter, topic, subName, numMessages, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Skipping messages on a topic subscription.
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='subName'>
        /// Name of subscription
        /// </param>
        /// <param name='numMessages'>
        /// The number of messages to skip
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> SkipMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int numMessages, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.SkipMessages1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, numMessages, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.SkipMessagesWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, numMessages, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Split a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        /// <param name='unload'>
        /// </param>
        /// <param name='splitAlgorithmName'>
        /// </param>
        public HttpOperationResponse SplitNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return SplitNamespaceBundleAsync(tenant, namespaceParameter, bundle, authoritative, unload, splitAlgorithmName, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Split a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        /// <param name='unload'>
        /// </param>
        /// <param name='splitAlgorithmName'>
        /// </param>
        public async Task<HttpOperationResponse> SplitNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.SplitNamespaceBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, authoritative, unload, splitAlgorithmName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Terminate a topic. A topic that is terminated will not accept any more
        /// messages to be published and will let consumer to drain existing messages
        /// in backlog
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse<object> Terminate(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return TerminateAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Terminate a topic. A topic that is terminated will not accept any more
        /// messages to be published and will let consumer to drain existing messages
        /// in backlog
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse<object>> TerminateAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.Terminate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.TerminateWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Terminate all partitioned topic. A topic that is terminated will not accept
        /// any more messages to be published and will let consumer to drain existing
        /// messages in backlog
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse TerminatePartitionedTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return TerminatePartitionedTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Terminate all partitioned topic. A topic that is terminated will not accept
        /// any more messages to be published and will let consumer to drain existing
        /// messages in backlog
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> TerminatePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.TerminatePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.TerminatePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Truncate a topic.
        /// </summary>
        /// <remarks>
        /// NonPersistentTopic does not support truncate.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse TruncateTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return TruncateTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Truncate a topic.
        /// </summary>
        /// <remarks>
        /// NonPersistentTopic does not support truncate.
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> TruncateTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistentTopic)
                return await _api.TruncateTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
           
            return await _api.TruncateTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// test the schema compatibility
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='postSchemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse<IsCompatibilityResponse> TestCompatibility(string tenant, string namespaceParameter, string topic, PostSchemaPayload postSchemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return TestCompatibilityAsync(tenant, namespaceParameter, topic, postSchemaPayload, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// test the schema compatibility
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='topic'>
        /// </param>
        /// <param name='postSchemaPayload'>
        /// A JSON value presenting a schema playload. An example of the expected
        /// schema can be found down here.
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse<IsCompatibilityResponse>> TestCompatibilityAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload postSchemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.TestCompatibilityWithHttpMessagesAsync(tenant, namespaceParameter, topic, postSchemaPayload, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Offload a prefix of a topic to long term storage
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse TriggerOffload(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return TriggerOffloadAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Offload a prefix of a topic to long term storage
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> TriggerOffloadAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.TriggerOffload1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.TriggerOffloadWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Unload a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse UnloadNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return UnloadNamespaceBundleAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload a namespace bundle
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> UnloadNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UnloadNamespaceBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Unload namespace
        /// </summary>
        /// <remarks>
        /// Unload an active namespace from the current broker serving it. Performing
        /// this operation will let the brokerremoves all producers, consumers, and
        /// connections using this namespace, and close all topics (includingtheir
        /// persistent store). During that operation, the namespace is marked as
        /// tentatively unavailable until thebroker completes the unloading action.
        /// This operation requires strictly super user privileges, since it
        /// wouldresult in non-persistent message loss and unexpected connection
        /// closure to the clients.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public HttpOperationResponse UnloadNamespace(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return UnloadNamespaceAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload namespace
        /// </summary>
        /// <remarks>
        /// Unload an active namespace from the current broker serving it. Performing
        /// this operation will let the brokerremoves all producers, consumers, and
        /// connections using this namespace, and close all topics (includingtheir
        /// persistent store). During that operation, the namespace is marked as
        /// tentatively unavailable until thebroker completes the unloading action.
        /// This operation requires strictly super user privileges, since it
        /// wouldresult in non-persistent message loss and unexpected connection
        /// closure to the clients.
        /// </remarks>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        public async Task<HttpOperationResponse> UnloadNamespaceAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UnloadNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Unload a topic
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public HttpOperationResponse UnloadTopic(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return UnloadTopicAsync(tenant, namespaceParameter, topic, isPersistentTopic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unload a topic
        /// </summary>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        public async Task<HttpOperationResponse> UnloadTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistentTopic = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.UnloadTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.UnloadTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public HttpOperationResponse UnsubscribeNamespaceBundle(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return UnsubscribeNamespaceBundleAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace bundle.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='bundle'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        public async Task<HttpOperationResponse> UnsubscribeNamespaceBundleAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UnsubscribeNamespaceBundleWithHttpMessagesAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        public HttpOperationResponse UnsubscribeNamespace(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return UnsubscribeNamespaceAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Unsubscribes the given subscription on all topics on a namespace.
        /// </summary>
        /// <param name='tenant'>
        /// </param>
        /// <param name='namespaceParameter'>
        /// </param>
        /// <param name='subscription'>
        /// </param>
        /// <param name='authoritative'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        public async Task<HttpOperationResponse> UnsubscribeNamespaceAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UnsubscribeNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Updates the rack placement information for a specific bookie in the cluster
        /// (note. bookie address format:`address:port`)
        /// </summary>
        /// <param name='bookie'>
        /// </param>
        /// <param name='group'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public HttpOperationResponse UpdateBookieRackInfo(string bookie, string group = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdateBookieRackInfoAsync(bookie, group, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Updates the rack placement information for a specific bookie in the cluster
        /// (note. bookie address format:`address:port`)
        /// </summary>
        /// <param name='bookie'>
        /// </param>
        /// <param name='group'>
        /// </param>
        /// <param name='customHeaders'>
        /// Headers that will be added to request.
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        /// <exception cref="Microsoft.Rest.HttpOperationException">
        /// Thrown when the operation returned an invalid status code
        /// </exception>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when a required parameter is null
        /// </exception>
        /// <return>
        /// A response object containing the response body and response headers.
        /// </return>
        public async Task<HttpOperationResponse> UpdateBookieRackInfoAsync(string bookie, string group = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UpdateBookieRackInfoWithHttpMessagesAsync(bookie, group, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update the configuration for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public HttpOperationResponse UpdateCluster(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdateClusterAsync(cluster, clusterData, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the configuration for a cluster.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar superuser privileges.
        /// </remarks>
        /// <param name='cluster'>
        /// The cluster name
        /// </param>
        public async Task<HttpOperationResponse> UpdateClusterAsync(string cluster, ClusterData clusterData, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UpdateClusterWithHttpMessagesAsync(cluster, clusterData, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        /// <param name='configName'>
        /// </param>
        /// <param name='configValue'>
        /// </param>
        public HttpOperationResponse UpdateDynamicConfiguration(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdateDynamicConfigurationAsync(configName, configValue, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update dynamic serviceconfiguration into zk only. This operation requires
        /// Pulsar super-user privileges.
        /// </summary>
        /// <param name='configName'>
        /// </param>
        /// <param name='configValue'>
        /// </param>
        public async Task<HttpOperationResponse> UpdateDynamicConfigurationAsync(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UpdateDynamicConfigurationWithHttpMessagesAsync(configName, configValue, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Increment partitions of an existing partitioned topic.
        /// </summary>
        /// <remarks>
        /// It only increments partitions of existing non-global partitioned-topic
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='body'>
        /// The number of partitions for the topic
        /// </param>
        /// <param name='updateLocalTopicOnly'>
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public HttpOperationResponse UpdatePartitionedTopic(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistentTopic = true, bool? updateLocalTopicOnly = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdatePartitionedTopicAsync(tenant, namespaceParameter, topic, partitions, isPersistentTopic, updateLocalTopicOnly, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Increment partitions of an existing partitioned topic.
        /// </summary>
        /// <remarks>
        /// It only increments partitions of existing non-global partitioned-topic
        /// </remarks>
        /// <param name='tenant'>
        /// Specify the tenant
        /// </param>
        /// <param name='namespaceParameter'>
        /// Specify the namespace
        /// </param>
        /// <param name='topic'>
        /// Specify topic name
        /// </param>
        /// <param name='body'>
        /// The number of partitions for the topic
        /// </param>
        /// <param name='updateLocalTopicOnly'>
        /// </param>
        /// <param name='authoritative'>
        /// Is authentication required to perform this operation
        /// </param>
        public async Task<HttpOperationResponse> UpdatePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistentTopic = true, bool? updateLocalTopicOnly = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistentTopic)
                return await _api.UpdatePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, updateLocalTopicOnly, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.UpdatePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, updateLocalTopicOnly, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Update the admins for a tenant.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar super-user privileges.
        /// </remarks>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='tenantInfo'>
        /// TenantInfo
        /// </param>
        public HttpOperationResponse UpdateTenantWithHttpMessages(string tenant, TenantInfo tenantInfo = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return UpdateTenantWithHttpMessagesAsync(tenant, tenantInfo, customHeaders).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Update the admins for a tenant.
        /// </summary>
        /// <remarks>
        /// This operation requires Pulsar super-user privileges.
        /// </remarks>
        /// <param name='tenant'>
        /// The tenant name
        /// </param>
        /// <param name='tenantInfo'>
        /// TenantInfo
        /// </param>
        public async Task<HttpOperationResponse> UpdateTenantWithHttpMessagesAsync(string tenant, TenantInfo tenantInfo = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.UpdateTenantWithHttpMessagesAsync(tenant, tenantInfo, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public async Task<HttpOperationResponse<TransactionCoordinatorStats>> GetCoordinatorStatsByIdWithHttpMessagesAsync(int coordinatorId, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetCoordinatorStatsByIdAsync(coordinatorId, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionCoordinatorStats> GetCoordinatorStatsByIdWithHttpMessages(int coordinatorId, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetCoordinatorStatsByIdWithHttpMessagesAsync(coordinatorId, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<int, TransactionCoordinatorStats>>> GetCoordinatorStatsWithHttpMessagesAsync(bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CoordinatorStatsAsync(authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<IDictionary<int, TransactionCoordinatorStats>> GetCoordinatorStatsWithHttpMessages(bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetCoordinatorStatsWithHttpMessagesAsync(authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionInBufferStats>> GetTransactionInBufferStatsWithHttpMessagesAsync(string tenant, string nameSpace, TxnID tx, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTransactionInBufferStatsAsync(tenant, nameSpace, tx, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionInBufferStats> GetTransactionInBufferStatsWithHttpMessages(string tenant, string nameSpace, TxnID tx, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetTransactionInBufferStatsWithHttpMessagesAsync(tenant, nameSpace, tx, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionInPendingAckStats>> GetTransactionInPendingAckStatsWithHttpMessagesAsync(string tenant, string nameSpace, TxnID tx, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTransactionInPendingAckStatsAsync(tenant, nameSpace, tx, topic, subName, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionInPendingAckStats> GetTransactionInPendingAckStatsWithHttpMessages(string tenant, string nameSpace, TxnID tx, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetTransactionInPendingAckStatsWithHttpMessagesAsync(tenant, nameSpace, tx, topic, subName, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionMetadata>> GetTransactionMetadataWithHttpMessagesAsync(TxnID tx, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTransactionMetadataAsync(tx, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionMetadata> GetTransactionMetadataWithHttpMessages(TxnID tx, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetTransactionMetadataWithHttpMessagesAsync(tx, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionBufferStats>> GetTransactionBufferStatsWithHttpMessagesAsync(string tenant, string nameSpace, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTransactionBufferStatsAsync(tenant, nameSpace, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionBufferStats> GetTransactionBufferStatsWithHttpMessages(string tenant, string nameSpace, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetTransactionBufferStatsWithHttpMessagesAsync(tenant, nameSpace, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionPendingAckStats>> GetPendingAckStatsWithHttpMessagesAsync(string tenant, string nameSpace, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPendingAckStatsAsync(tenant, nameSpace, topic, subName, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionPendingAckStats> GetPendingAckStatsWithHttpMessages(string tenant, string nameSpace, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetPendingAckStatsWithHttpMessagesAsync(tenant, nameSpace, topic, subName, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, TransactionMetadata>>> GetSlowTransactionsByCoordinatorIdWithHttpMessagesAsync(int coordinatorId, long timeout, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSlowTransactionsByCoordinatorIdAsync(coordinatorId, timeout, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<IDictionary<string, TransactionMetadata>> GetSlowTransactionsByCoordinatorIdWithHttpMessages(int coordinatorId, long timeout, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetSlowTransactionsByCoordinatorIdWithHttpMessagesAsync(coordinatorId, timeout, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, TransactionMetadata>>> GetSlowTransactionsWithHttpMessagesAsync(long timeout, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSlowTransactionsAsync(timeout, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<IDictionary<string, TransactionMetadata>> GetSlowTransactionsWithHttpMessages(long timeout, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetSlowTransactionsWithHttpMessagesAsync(timeout, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionCoordinatorInternalStats>> GetCoordinatorInternalStatsWithHttpMessagesAsync(int coordinatorId, bool metadata = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetCoordinatorInternalStatsAsync(coordinatorId, metadata, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionCoordinatorInternalStats> GetCoordinatorInternalStatsWithHttpMessages(int coordinatorId, bool metadata = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetCoordinatorInternalStatsWithHttpMessagesAsync(coordinatorId, metadata, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TransactionPendingAckInternalStats>> GetPendingAckInternalStatsWithHttpMessagesAsync(string tenant, string nameSpace, string topic, string subName, bool metadata = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPendingAckInternalStatsAsync(tenant, nameSpace, topic, subName, metadata, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<TransactionPendingAckInternalStats> GetPendingAckInternalStatsWithHttpMessages(string tenant, string nameSpace, string topic, string subName, bool metadata = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetPendingAckInternalStatsWithHttpMessagesAsync(tenant, nameSpace, topic, subName, metadata, authoritative, customHeaders).GetAwaiter().GetResult();
        }
    }
}
