using Microsoft.Rest;
using Newtonsoft.Json;
using SharpPulsar.Admin;
using SharpPulsar.Admin.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Admin
    {
        private readonly PulsarAdminRESTAPI _api;
        public Admin(string brokerWebServiceUrl, HttpClient httpClient, bool disposeHttpClient)
        {
            _api = new PulsarAdminRESTAPI(brokerWebServiceUrl, httpClient, true);
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
        public HttpOperationResponse BacklogQuotaCheck(Dictionary<string, List<string>> customHeaders = null)
        {
            return BacklogQuotaCheckAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> BacklogQuotaCheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.BacklogQuotaCheckWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ClearNamespaceBacklogForSubscription(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ClearNamespaceBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBacklogForSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, subscription, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ClearNamespaceBacklog(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBacklogAsync(tenant, namespaceParameter, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ClearNamespaceBacklogAsync(string tenant, string namespaceParameter, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBacklogWithHttpMessagesAsync(tenant, namespaceParameter, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ClearNamespaceBundleBacklogForSubscription(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBundleBacklogForSubscriptionAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ClearNamespaceBundleBacklogForSubscriptionAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBundleBacklogForSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, subscription, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ClearNamespaceBundleBacklog(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearNamespaceBundleBacklogAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ClearNamespaceBundleBacklogAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearNamespaceBundleBacklogWithHttpMessagesAsync(tenant, namespaceParameter, bundle, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ClearOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return ClearOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ClearOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ClearOffloadDeletionLagWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse Compact(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CompactAsync(tenant, namespaceParameter, topic, authoritative, isPersistent, customHeaders).GetAwaiter().GetResult();
            
        }
        public async Task<HttpOperationResponse> CompactAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.Compact1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            else
                return await _api.CompactWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<LongRunningProcessStatus> CompactionStatus(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
                return CompactionStatusAsync(tenant, namespaceParameter, topic, authoritative, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<LongRunningProcessStatus>> CompactionStatusAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.CompactionStatus1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            else
                return await _api.CompactionStatusWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse CreateCluster(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return CreateClusterAsync(cluster, body, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> CreateClusterAsync(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateClusterWithHttpMessagesAsync(cluster, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse CreateMissedPartitions(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return CreateMissedPartitionsAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> CreateMissedPartitionsAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.CreateMissedPartitions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken);
            else
                return await _api.CreateMissedPartitionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken);
        }
        public HttpOperationResponse CreateNamespace(string tenant, string namespaceParameter, Policies body = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateNamespaceAsync(tenant, namespaceParameter, body, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> CreateNamespaceAsync(string tenant, string namespaceParameter, Policies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse CreateNonPartitionedTopic(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateNonPartitionedTopicAsync(tenant, namespaceParameter, topic, authoritative, isPersistent, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> CreateNonPartitionedTopicAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.CreateNonPartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.CreateNonPartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse CreatePartitionedTopic(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreatePartitionedTopicAsync(tenant, namespaceParameter, topic, partitions, isPersistent, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> CreatePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, int partitions, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.CreatePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.CreatePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, partitions, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse CreateSubscription(string tenant, string namespaceParameter, string topic, string subscriptionName, bool? authoritative = false, bool isPersistent = true, MessageIdImpl messageId = null, bool? replicated = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateSubscriptionAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, isPersistent, messageId, replicated, customHeaders).GetAwaiter().GetResult();

        }
        
        public async Task<HttpOperationResponse> CreateSubscriptionAsync(string tenant, string namespaceParameter, string topic, string subscriptionName, bool? authoritative = false, bool isPersistent = true, MessageIdImpl messageId = null, bool? replicated = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.CreateSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, messageId, replicated, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.CreateSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, subscriptionName, authoritative, messageId, replicated, customHeaders, cancellationToken).ConfigureAwait(false);

        }

        public HttpOperationResponse CreateTenant(string tenant, TenantInfo body = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return CreateTenantAsync(tenant, body, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> CreateTenantAsync(string tenant, TenantInfo body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.CreateTenantWithHttpMessagesAsync(tenant, body, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteBookieAffinityGroupAsync(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteBookieAffinityGroupWithHttpMessagesAsync(property, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteBookieRackInfo(string bookie, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteBookieRackInfoAsync(bookie, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteBookieRackInfoAsync(string bookie, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteBookieRackInfoWithHttpMessagesAsync(bookie, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteDeduplicationSnapshotInterval(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteDeduplicationSnapshotInterval1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteDeduplicationSnapshotIntervalWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteDelayedDeliveryPolicies(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();

        }
        public async Task<HttpOperationResponse> DeleteDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteDelayedDeliveryPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.DeleteDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

        }

        public HttpOperationResponse DeleteDynamicConfiguration(string configName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteDynamicConfigurationAsync(configName, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> DeleteDynamicConfigurationAsync(string configName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteDynamicConfigurationWithHttpMessagesAsync(configName, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse DeleteFailureDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteFailureDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteFailureDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteFailureDomainWithHttpMessagesAsync(cluster, domainName, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteInactiveTopicPolicies(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteInactiveTopicPoliciesAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteInactiveTopicPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteMaxUnackedMessagesOnConsumer(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteMaxUnackedMessagesOnConsumerAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteMaxUnackedMessagesOnConsumerAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse DeleteMaxUnackedMessagesOnSubscription(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteMaxUnackedMessagesOnSubscriptionAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteMaxUnackedMessagesOnSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.DeleteMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

        }
        public HttpOperationResponse DeleteNamespaceBundle(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceBundleAsync(tenant, namespaceParameter, bundle, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteNamespaceBundleAsync(string tenant, string namespaceParameter, string bundle, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse DeleteNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, policyName, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteNamespace(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteNamespaceAsync(tenant, namespaceParameter, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> DeleteNamespaceAsync(string tenant, string namespaceParameter, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeletePartitionedTopic(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeletePartitionedTopicAsync(tenant, namespaceParameter, topic, isPersistent, force, authoritative, deleteSchema, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse> DeletePartitionedTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.DeletePartitionedTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
           
            return await _api.DeletePartitionedTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<DeleteSchemaResponse> DeleteSchema(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSchemaAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<DeleteSchemaResponse>> DeleteSchemaAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse DeleteSubscription(string tenant, string namespaceParameter, string topic, string subName, bool isPersistent = true, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteSubscriptionAsync(tenant, namespaceParameter, topic, subName, isPersistent, force, authoritative, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> DeleteSubscriptionAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistent = true, bool? force = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, force, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse DeleteTenant(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteTenantAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> DeleteTenantAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.DeleteTenantWithHttpMessagesAsync(tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse DeleteTopic(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return DeleteTopicAsync(tenant, namespaceParameter, topic, isPersistent, force, authoritative, deleteSchema, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> DeleteTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? force = false, bool? authoritative = false, bool? deleteSchema = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.DeleteTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.DeleteTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, force, authoritative, deleteSchema, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ExamineMessage(string tenant, string namespaceParameter, string topic, bool isPersistent = true, string initialPosition = null, long? messagePosition = 1, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return ExamineMessageAsync(tenant, namespaceParameter, topic, isPersistent, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ExamineMessageAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, string initialPosition = null, long? messagePosition = 1, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.ExamineMessage1WithHttpMessagesAsync(tenant, namespaceParameter, topic, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.ExamineMessageWithHttpMessagesAsync(tenant, namespaceParameter, topic, initialPosition, messagePosition, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ExpireMessagesForAllSubscriptions(string tenant, string namespaceParameter, string topic, int expireTimeInSeconds, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireMessagesForAllSubscriptionsAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();            
        }
        
        public async Task<HttpOperationResponse> ExpireMessagesForAllSubscriptionsAsync(string tenant, string namespaceParameter, string topic, int expireTimeInSeconds, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.ExpireMessagesForAllSubscriptions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ExpireMessagesForAllSubscriptionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
        }
        public HttpOperationResponse ExpireTopicMessages(string tenant, string namespaceParameter, string topic, string subName, int expireTimeInSeconds, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireTopicMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ExpireTopicMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int expireTimeInSeconds, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.ExpireTopicMessages3WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.ExpireTopicMessages1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, expireTimeInSeconds, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ExpireTopicMessages(string tenant, string namespaceParameter, string topic, string subName, bool isPersistent = true, bool? authoritative = false, SharpPulsar.Admin.Models.ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return ExpireTopicMessagesAsync(tenant, namespaceParameter, topic, subName, isPersistent, authoritative, messageId, customHeaders).GetAwaiter().GetResult();

        }
        public async Task<HttpOperationResponse> ExpireTopicMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool isPersistent = true, bool? authoritative = false, SharpPulsar.Admin.Models.ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.ExpireTopicMessages2WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);
           
            return await _api.ExpireTopicMessagesWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, authoritative, messageId, customHeaders, cancellationToken).ConfigureAwait(false);

        }

        public HttpOperationResponse<IList<string>> GetActiveBrokers(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetActiveBrokersAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetActiveBrokersAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetActiveBrokersWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<IDictionary<string, object>> GetAllDynamicConfigurations(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllDynamicConfigurationsAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetAllDynamicConfigurationsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllDynamicConfigurationsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<AllocatorStats> GetAllocatorStats(string allocator, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllocatorStatsAsync(allocator, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<AllocatorStats>> GetAllocatorStatsAsync(string allocator, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllocatorStatsWithHttpMessagesAsync(allocator, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<GetAllVersionsSchemaResponse> GetAllSchemas(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAllSchemasAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<GetAllVersionsSchemaResponse>> GetAllSchemasAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAllSchemasWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<IList<object>> GetAntiAffinityNamespaces(string cluster, string group, string tenant = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetAntiAffinityNamespacesAsync(cluster, group, tenant, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse<IList<object>>> GetAntiAffinityNamespacesAsync(string cluster, string group, string tenant = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetAntiAffinityNamespacesWithHttpMessagesAsync(cluster, group, tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<PersistentOfflineTopicStats> GetBacklog(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<PersistentOfflineTopicStats>> GetBacklogAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetBacklog1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
                
           return await _api.GetBacklogWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, object>> GetBacklogQuotaMap(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogQuotaMapAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetBacklogQuotaMap2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetBacklogQuotaMap1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, object>> GetBacklogQuotaMap(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBacklogQuotaMapAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetBacklogQuotaMapAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBacklogQuotaMapWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<BookieAffinityGroupData> GetBookieAffinityGroup(string property, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookieAffinityGroupAsync(property, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
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

        public HttpOperationResponse<IDictionary<string, IDictionary<string, BookieInfo>>> GetBookiesRackInfo(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBookiesRackInfoAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, IDictionary<string, BookieInfo>>>> GetBookiesRackInfoAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBookiesRackInfoWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, ResourceUnit>> GetBrokerResourceAvailability(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerResourceAvailabilityAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, ResourceUnit>>> GetBrokerResourceAvailabilityAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokerResourceAvailabilityWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<BrokerNamespaceIsolationData>> GetBrokersWithNamespaceIsolationPolicy(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokersWithNamespaceIsolationPolicyAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<BrokerNamespaceIsolationData>>> GetBrokersWithNamespaceIsolationPolicyAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokersWithNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<BrokerNamespaceIsolationData> GetBrokerWithNamespaceIsolationPolicy(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBrokerWithNamespaceIsolationPolicyAsync(cluster, broker, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<BrokerNamespaceIsolationData>> GetBrokerWithNamespaceIsolationPolicyAsync(string cluster, string broker, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBrokerWithNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, broker, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<BundlesData> GetBundlesData(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetBundlesDataAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<BundlesData>> GetBundlesDataAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetBundlesDataWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetClusters(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClustersAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetClustersAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetClustersWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<ClusterData> GetCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<ClusterData>> GetClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetCompactionThreshold(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        { 
            return GetCompactionThresholdAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetCompactionThresholdAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetCompactionThreshold2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetCompactionThreshold1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<long?> GetCompactionThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetCompactionThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<long?>> GetCompactionThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetDeduplicationEnabled(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationEnabledAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetDeduplicationEnabledAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetDeduplicationEnabled1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDeduplicationEnabledWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetDeduplicationSnapshotInterval2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDeduplicationSnapshotInterval1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetDeduplicationSnapshotInterval(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDeduplicationSnapshotIntervalAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetDeduplicationSnapshotIntervalAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDeduplicationSnapshotIntervalWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetDefaultResourceQuota(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDefaultResourceQuotaAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetDefaultResourceQuotaAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDefaultResourceQuotaWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetDelayedDeliveryPolicies(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        { 
            return GetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetDelayedDeliveryPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDelayedDeliveryPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<DelayedDeliveryPolicies> GetDelayedDeliveryPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDelayedDeliveryPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<DelayedDeliveryPolicies>> GetDelayedDeliveryPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDelayedDeliveryPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDispatchRateAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
             return await _api.GetDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<DispatchRate> GetDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<DispatchRate>> GetDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<FailureDomain> GetDomain(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDomainAsync(cluster, domainName, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<FailureDomain>> GetDomainAsync(string cluster, string domainName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDomainWithHttpMessagesAsync(cluster, domainName, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<object>> GetDynamicConfigurationName(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetDynamicConfigurationNameAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<object>>> GetDynamicConfigurationNameAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetDynamicConfigurationNameWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, FailureDomain>> GetFailureDomains(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetFailureDomainsAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, FailureDomain>>> GetFailureDomainsAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetFailureDomainsWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetInactiveTopicPolicies(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInactiveTopicPoliciesAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetInactiveTopicPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetInactiveTopicPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<InactiveTopicPolicies> GetInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<InactiveTopicPolicies>> GetInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse<InternalConfigurationData> GetInternalConfigurationData(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInternalConfigurationDataAsync(customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse<InternalConfigurationData>> GetInternalConfigurationDataAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetInternalConfigurationDataWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<PersistentTopicInternalStats> GetInternalStats(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetInternalStatsAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, metadata, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<PersistentTopicInternalStats>> GetInternalStatsAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? metadata = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetInternalStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, metadata, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetInternalStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, metadata, customHeaders, cancellationToken).ConfigureAwait(false);
        }


        public HttpOperationResponse<bool?> GetIsAllowAutoUpdateSchema(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetIsAllowAutoUpdateSchemaAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse<bool?>> GetIsAllowAutoUpdateSchemaAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetIsAllowAutoUpdateSchemaWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetLastMessageId(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLastMessageIdAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
            
        }
        public async Task<HttpOperationResponse> GetLastMessageIdAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.GetLastMessageId1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
                
            return await _api.GetLastMessageIdWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
        }

        public HttpOperationResponse<IList<string>> GetList(string tenant, string namespaceParameter, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetListAsync(tenant, namespaceParameter, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetListAsync(string tenant, string namespaceParameter, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetList1WithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetListWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetListFromBundle(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetListFromBundleAsync(tenant, namespaceParameter, bundle, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetListFromBundleAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetListFromBundleWithHttpMessagesAsync(tenant, namespaceParameter, bundle, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<LoadReport> GetLoadReport(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetLoadReportAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<LoadReport>> GetLoadReportAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetLoadReportWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetManagedLedgerInfo(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetManagedLedgerInfoAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetManagedLedgerInfoAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetManagedLedgerInfo1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetManagedLedgerInfoWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxConsumers(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMaxConsumersAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxConsumers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxConsumersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxConsumersPerSubscription(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxConsumersPerSubscription2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxConsumersPerSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMaxConsumersPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxConsumersPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMaxConsumersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxConsumersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async  Task<HttpOperationResponse<int?>> GetMaxConsumersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxConsumersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxMessageSize(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxMessageSizeAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse> GetMaxMessageSizeAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxMessageSize1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxMessageSizeWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxProducers(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxProducersAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMaxProducersAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxProducers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxProducersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxProducersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxSubscriptionsPerTopic2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxSubscriptionsPerTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }
        
        public HttpOperationResponse<int?> GetMaxSubscriptionsPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxSubscriptionsPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMaxSubscriptionsPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxSubscriptionsPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMaxUnackedMessagesOnConsumer(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedMessagesOnConsumerAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMaxUnackedMessagesOnConsumerAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }


        public HttpOperationResponse GetMaxUnackedMessagesOnSubscription(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        { 
            return GetMaxUnackedMessagesOnSubscriptionAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }

        public async Task<HttpOperationResponse> GetMaxUnackedMessagesOnSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMaxUnackedMessagesPerConsumer(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedMessagesPerConsumerAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMaxUnackedMessagesPerConsumerAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxUnackedMessagesPerConsumerWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMaxUnackedmessagesPerSubscription(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMaxUnackedmessagesPerSubscriptionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMaxUnackedmessagesPerSubscriptionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMaxUnackedmessagesPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(true);
        }

        public HttpOperationResponse<IList<Metrics>> GetMBeans(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMBeansAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<Metrics>>> GetMBeansAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMBeansWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetMessageById(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return GetMessageByIdAsync(tenant, namespaceParameter, topic, ledgerId, entryId, isPersistent, authoritative, customHeaders, cancellationToken).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetMessageByIdAsync(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMessageById1WithHttpMessagesAsync(tenant, namespaceParameter, topic, ledgerId, entryId, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMessageByIdWithHttpMessagesAsync(tenant, namespaceParameter, topic, ledgerId, entryId, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetMessageTTL(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMessageTTLAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetMessageTTLAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetMessageTTL1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<Metrics>> GetMetrics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetMetricsAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<Metrics>>> GetMetricsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetMetricsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<string> GetNamespaceAntiAffinityGroup(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceAntiAffinityGroupAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
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

        public HttpOperationResponse<IDictionary<string, NamespaceIsolationData>> GetNamespaceIsolationPolicies(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPoliciesAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, NamespaceIsolationData>>> GetNamespaceIsolationPoliciesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceIsolationPoliciesWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<NamespaceIsolationData> GetNamespaceIsolationPolicy(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceIsolationPolicyAsync(cluster, policyName, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<NamespaceIsolationData>> GetNamespaceIsolationPolicyAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceIsolationPolicyWithHttpMessagesAsync(cluster, policyName, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetNamespaceMessageTTL(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceMessageTTLAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetNamespaceMessageTTLAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceMessageTTLWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetNamespaceReplicationClusters(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetNamespaceReplicationClustersAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetNamespaceReplicationClustersAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetNamespaceReplicationClustersWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<long?> GetOffloadDeletionLag(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadDeletionLagAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<long?>> GetOffloadDeletionLagAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadDeletionLagWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetOffloadPolicies(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadPoliciesAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetOffloadPolicies2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetOffloadPolicies1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<OffloadPolicies> GetOffloadPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<OffloadPolicies>> GetOffloadPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<long?> GetOffloadThreshold(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOffloadThresholdAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<long?>> GetOffloadThresholdAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOffloadThresholdWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, NamespaceOwnershipStatus>> GetOwnedNamespaces(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetOwnedNamespacesAsync(clusterName, brokerWebserviceurl, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, NamespaceOwnershipStatus>>> GetOwnedNamespacesAsync(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetOwnedNamespacesWithHttpMessagesAsync(clusterName, brokerWebserviceurl, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<PartitionedTopicMetadata> GetPartitionedMetadata(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedMetadataAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, checkAllowAutoCreation, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<PartitionedTopicMetadata>> GetPartitionedMetadataAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPartitionedMetadata1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, checkAllowAutoCreation, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedMetadataWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, checkAllowAutoCreation, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetPartitionedStats(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedStatsAsync(tenant, namespaceParameter, topic, isPersistent, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetPartitionedStatsAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPartitionedStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, perPartition, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetPartitionedTopicList(string tenant, string namespaceParameter, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPartitionedTopicListAsync(tenant, namespaceParameter, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetPartitionedTopicListAsync(string tenant, string namespaceParameter, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPartitionedTopicList1WithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPartitionedTopicListWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetPeerCluster(string cluster, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPeerClusterAsync(cluster, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetPeerClusterAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPeerClusterWithHttpMessagesAsync(cluster, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, PendingBookieOpsStats>> GetPendingBookieOpsStats(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPendingBookieOpsStatsAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, PendingBookieOpsStats>>> GetPendingBookieOpsStatsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPendingBookieOpsStatsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, object>> GetPermissionsOnTopic(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPermissionsOnTopicAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetPermissionsOnTopicAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPermissionsOnTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPermissionsOnTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IDictionary<string, object>> GetPermissions(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPermissionsAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetPermissionsAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPermissionsWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetPersistence(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPersistenceAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetPersistenceAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPersistence2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPersistence1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<PersistencePolicies> GetPersistences(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPersistencesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<PersistencePolicies>> GetPersistencesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPersistenceWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<Policies> GetPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<Policies>> GetPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetPublishRate(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetPublishRateAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetPublishRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetPublishRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetPublishRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<DispatchRate> GetReplicatorDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetReplicatorDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<DispatchRate>> GetReplicatorDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetReplicatorDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetRetention(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRetentionAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetRetentionAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetRetention2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetRetention1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<RetentionPolicies> GetRetention(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRetentionAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<RetentionPolicies>> GetRetentionAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetRetentionWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public  HttpOperationResponse<IDictionary<string, object>> GetRuntimeConfiguration(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetRuntimeConfigurationAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IDictionary<string, object>>> GetRuntimeConfigurationAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetRuntimeConfigurationWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<GetSchemaResponse> GetSchema(string tenant, string namespaceParameter, string topic, string version, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAsync(tenant, namespaceParameter, topic, version, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<GetSchemaResponse>> GetSchemaAsync(string tenant, string namespaceParameter, string topic, string version, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchema1WithHttpMessagesAsync(tenant, namespaceParameter, topic, version, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<string> GetSchemaAutoUpdateCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAutoUpdateCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<string>> GetSchemaAutoUpdateCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaAutoUpdateCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<string> GetSchemaCompatibilityStrategy(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaCompatibilityStrategyAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<string>> GetSchemaCompatibilityStrategyAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaCompatibilityStrategyWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<bool?> GetSchemaValidtionEnforced(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaValidtionEnforcedAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<bool?>> GetSchemaValidtionEnforcedAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaValidtionEnforcedWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<GetSchemaResponse> GetSchema(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSchemaAsync(tenant, namespaceParameter, topic, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<GetSchemaResponse>> GetSchemaAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<TopicStats> GetStats(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetStatsAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TopicStats>> GetStatsAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetStats1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetStatsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, getPreciseBacklog, subscriptionBacklogSize, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetSubscribeRate(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscribeRateAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetSubscribeRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetSubscribeRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscribeRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<SubscribeRate> GetSubscribeRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscribeRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<SubscribeRate>> GetSubscribeRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscribeRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetSubscriptionDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionDispatchRateAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetSubscriptionDispatchRate2WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscriptionDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<DispatchRate> GetSubscriptionDispatchRate(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionDispatchRateAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<DispatchRate>> GetSubscriptionDispatchRateAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscriptionDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<int?> GetSubscriptionExpirationTime(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionExpirationTimeAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<int?>> GetSubscriptionExpirationTimeAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetSubscriptionExpirationTimeWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GetSubscriptions(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetSubscriptionsAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GetSubscriptionsAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GetSubscriptions1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GetSubscriptionsWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<TenantInfo> GetTenantAdmin(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantAdminAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<TenantInfo>> GetTenantAdminAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantAdminWithHttpMessagesAsync(tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetTenantNamespaces(string tenant, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantNamespacesAsync(tenant, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetTenantNamespacesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantNamespacesWithHttpMessagesAsync(tenant, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetTenants(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTenantsAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetTenantsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTenantsWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<object> GetTopics(Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<object>> GetTopicsAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTopics2WithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<IList<string>> GetTopics(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetTopicsAsync(tenant, namespaceParameter, mode, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<IList<string>>> GetTopicsAsync(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetTopicsWithHttpMessagesAsync(tenant, namespaceParameter, mode, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<LongSchemaVersion> GetVersionBySchema(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return GetVersionBySchemaAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<LongSchemaVersion>> GetVersionBySchemaAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GetVersionBySchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GrantPermissionOnNamespace(string tenant, string namespaceParameter, string role, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GrantPermissionOnNamespaceAsync(tenant, namespaceParameter, role, permissions, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GrantPermissionOnNamespaceAsync(string tenant, string namespaceParameter, string role, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.GrantPermissionOnNamespaceWithHttpMessagesAsync(tenant, namespaceParameter, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse GrantPermissionsOnTopic(string tenant, string namespaceParameter, string topic, string role, bool isPersistent = true, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return GrantPermissionsOnTopicAsync(tenant, namespaceParameter, topic, role, isPersistent, permissions, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> GrantPermissionsOnTopicAsync(string tenant, string namespaceParameter, string topic, string role, bool isPersistent = true, IList<string> permissions = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.GrantPermissionsOnTopic1WithHttpMessagesAsync(tenant, namespaceParameter, topic, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.GrantPermissionsOnTopicWithHttpMessagesAsync(tenant, namespaceParameter, topic, role, permissions, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse Healthcheck(Dictionary<string, List<string>> customHeaders = null)
        {
            return HealthcheckAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> HealthcheckAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.HealthcheckWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }
        public HttpOperationResponse IsReady(Dictionary<string, List<string>> customHeaders = null)
        {
            return IsReadyAsync(customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> IsReadyAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.IsReadyWithHttpMessagesAsync(customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ModifyDeduplication(string tenant, string namespaceParameter, bool deduplicate, Dictionary<string, List<string>> customHeaders = null)
        {
            return ModifyDeduplicationAsync(tenant, namespaceParameter, deduplicate, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ModifyDeduplicationAsync(string tenant, string namespaceParameter, bool deduplicate, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ModifyDeduplicationWithHttpMessagesAsync(tenant, namespaceParameter, deduplicate, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse ModifyEncryptionRequired(string tenant, string namespaceParameter, bool encryptionRequired, Dictionary<string, List<string>> customHeaders = null)
        {
            return ModifyEncryptionRequiredAsync(tenant, namespaceParameter, encryptionRequired, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> ModifyEncryptionRequiredAsync(string tenant, string namespaceParameter, bool encryptionRequired, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.ModifyEncryptionRequiredWithHttpMessagesAsync(tenant, namespaceParameter, encryptionRequired, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse<OffloadProcessStatus> OffloadStatus(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return OffloadStatusAsync(tenant, namespaceParameter, topic, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        
        public async Task<HttpOperationResponse<OffloadProcessStatus>> OffloadStatusAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.OffloadStatus1WithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.OffloadStatusWithHttpMessagesAsync(tenant, namespaceParameter, topic, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse PeekNthMessages(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {            
            return PeekNthMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, isPersistent, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> PeekNthMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool isPersistent = true, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.PeekNthMessage1WithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, authoritative, customHeaders, cancellationToken ).ConfigureAwait(false);
            
            return await _api.PeekNthMessageWithHttpMessagesAsync(tenant, namespaceParameter, topic, subName, messagePosition, authoritative, customHeaders, cancellationToken ).ConfigureAwait(false);
        }

        public HttpOperationResponse<PostSchemaResponse> PostSchema(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null)
        {
            return PostSchemaAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse<PostSchemaResponse>> PostSchemaAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload schemaPayload = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.PostSchemaWithHttpMessagesAsync(tenant, namespaceParameter, topic, schemaPayload, authoritative, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveAutoSubscriptionCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveAutoSubscriptionCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveAutoSubscriptionCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveAutoSubscriptionCreationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveAutoTopicCreation(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveAutoTopicCreationAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveAutoTopicCreationAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveAutoTopicCreationWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveBacklogQuota(string tenant, string namespaceParameter, string topic, bool isPersistent = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveBacklogQuotaAsync(tenant, namespaceParameter, topic, isPersistent, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.RemoveBacklogQuota2WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveBacklogQuota1WithHttpMessagesAsync(tenant, namespaceParameter, topic, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveBacklogQuota(string tenant, string namespaceParameter, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveBacklogQuotaAsync(tenant, namespaceParameter, backlogQuotaType, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveBacklogQuotaAsync(string tenant, string namespaceParameter, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveBacklogQuotaWithHttpMessagesAsync(tenant, namespaceParameter, backlogQuotaType, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveCompactionThreshold(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveCompactionThresholdAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveCompactionThresholdAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.RemoveCompactionThreshold1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveCompactionThresholdWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveDeduplicationEnabled(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDeduplicationEnabledAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveDeduplicationEnabledAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.RemoveDeduplicationEnabled1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveDeduplicationEnabledWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveDispatchRate(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveDispatchRateAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveDispatchRateAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.RemoveDispatchRate1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveDispatchRateWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveInactiveTopicPolicies(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveInactiveTopicPoliciesAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveInactiveTopicPoliciesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveInactiveTopicPoliciesWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveMaxConsumers(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveMaxConsumersAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if(isPersistent)
                return await _api.RemoveMaxConsumers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
            
            return await _api.RemoveMaxConsumersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveMaxConsumersPerSubscription(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxConsumersPerSubscriptionAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveMaxConsumersPerSubscriptionAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.RemoveMaxConsumersPerSubscription1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxConsumersPerSubscriptionWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveMaxMessageSize(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxMessageSizeAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveMaxMessageSizeAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.RemoveMaxMessageSize1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxMessageSizeWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }


        public HttpOperationResponse RemoveMaxProducers(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxProducersAsync(tenant, namespaceParameter, topic, isPersistent, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveMaxProducersAsync(string tenant, string namespaceParameter, string topic, bool isPersistent = true, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            if (isPersistent)
                return await _api.RemoveMaxProducers1WithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);

            return await _api.RemoveMaxProducersWithHttpMessagesAsync(tenant, namespaceParameter, topic, customHeaders, cancellationToken).ConfigureAwait(false);
        }

        public HttpOperationResponse RemoveMaxProducersPerTopic(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null)
        {
            return RemoveMaxProducersPerTopicAsync(tenant, namespaceParameter, customHeaders).GetAwaiter().GetResult();
        }
        public async Task<HttpOperationResponse> RemoveMaxProducersPerTopicAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            return await _api.RemoveMaxProducersPerTopicWithHttpMessagesAsync(tenant, namespaceParameter, customHeaders, cancellationToken).ConfigureAwait(false);
        }


        public Task<HttpOperationResponse> RemoveMaxSubscriptionsPerTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxSubscriptionsPerTopic2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxSubscriptionsPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMessageTTL1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveNamespaceAntiAffinityGroupWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveNamespaceBundleResourceQuotaWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveNamespaceMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveOffloadPolicies1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveOffloadPolicies2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveOffloadPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemovePersistence1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemovePersistenceWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemovePublishRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemovePublishRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveRetention1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveRetentionWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveSubscribeRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveSubscribeRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveSubscriptionDispatchRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveSubscriptionDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ResetCursor1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, long timestamp, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ResetCursorOnPosition1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool? authoritative = false, SharpPulsar.Admin.Models.ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ResetCursorOnPositionWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool? authoritative = false, SharpPulsar.Admin.Models.ResetCursorData messageId = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ResetCursorWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, long timestamp, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RevokePermissionsOnNamespaceWithHttpMessagesAsync(string tenant, string namespaceParameter, string role, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RevokePermissionsOnTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string role, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RevokePermissionsOnTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string role, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetAutoSubscriptionCreationWithHttpMessagesAsync(string tenant, string namespaceParameter, AutoSubscriptionCreationOverride body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetAutoTopicCreationWithHttpMessagesAsync(string tenant, string namespaceParameter, AutoTopicCreationOverride body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetBacklogQuota1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetBacklogQuota2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetBacklogQuotaWithHttpMessagesAsync(string tenant, string namespaceParameter, string backlogQuotaType = null, BacklogQuota body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetBookieAffinityGroupWithHttpMessagesAsync(string tenant, string namespaceParameter, BookieAffinityGroupData body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetCompactionThreshold1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, long? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetCompactionThreshold2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, long? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetCompactionThresholdWithHttpMessagesAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDeduplicationEnabled1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDeduplicationEnabledWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDeduplicationSnapshotInterval1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDeduplicationSnapshotInterval2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDeduplicationSnapshotIntervalWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> SetDefaultResourceQuotaWithHttpMessagesAsync(ResourceQuota body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDelayedDeliveryPolicies1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DelayedDeliveryPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDelayedDeliveryPolicies2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DelayedDeliveryPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDelayedDeliveryPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, DelayedDeliveryPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDispatchRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDispatchRate2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetFailureDomainWithHttpMessagesAsync(string cluster, string domainName, FailureDomain body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetInactiveTopicPolicies1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, InactiveTopicPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetInactiveTopicPolicies2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, InactiveTopicPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetInactiveTopicPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, InactiveTopicPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetIsAllowAutoUpdateSchemaWithHttpMessagesAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumersPerSubscription1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumersPerSubscription2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumersPerSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumersPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxConsumersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxMessageSize1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxMessageSizeWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxProducers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxProducersPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxProducersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxSubscriptionsPerTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxSubscriptionsPerTopic2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxSubscriptionsPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int? body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesPerConsumerWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMaxUnackedMessagesPerSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMessageTTL1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int messageTTL, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int messageTTL, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetNamespaceAntiAffinityGroupWithHttpMessagesAsync(string tenant, string namespaceParameter, string body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetNamespaceBundleResourceQuotaWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, ResourceQuota body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetNamespaceIsolationPolicyWithHttpMessagesAsync(string cluster, string policyName, NamespaceIsolationData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetNamespaceMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetNamespaceReplicationClustersWithHttpMessagesAsync(string tenant, string namespaceParameter, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetOffloadDeletionLagWithHttpMessagesAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetOffloadPolicies1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, OffloadPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetOffloadPolicies2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, OffloadPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetOffloadPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, OffloadPolicies body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetOffloadThresholdWithHttpMessagesAsync(string tenant, string namespaceParameter, long body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPeerClusterNamesWithHttpMessagesAsync(string cluster, IList<string> body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPersistence1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PersistencePolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPersistence2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PersistencePolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPersistenceWithHttpMessagesAsync(string tenant, string namespaceParameter, PersistencePolicies body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPublishRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PublishRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetPublishRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PublishRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetReplicatorDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetRetention1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, RetentionPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetRetention2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, RetentionPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetRetentionWithHttpMessagesAsync(string tenant, string namespaceParameter, RetentionPolicies body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSchemaAutoUpdateCompatibilityStrategyWithHttpMessagesAsync(string tenant, string namespaceParameter, string body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSchemaCompatibilityStrategyWithHttpMessagesAsync(string tenant, string namespaceParameter, string body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSchemaValidtionEnforcedWithHttpMessagesAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscribeRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, SubscribeRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscribeRate2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, SubscribeRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscribeRateWithHttpMessagesAsync(string tenant, string namespaceParameter, SubscribeRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscriptionAuthModeWithHttpMessagesAsync(string tenant, string namespaceParameter, string body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscriptionDispatchRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscriptionDispatchRate2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscriptionDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, DispatchRate body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SetSubscriptionExpirationTimeWithHttpMessagesAsync(string tenant, string namespaceParameter, int body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SkipAllMessages1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SkipAllMessagesWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SkipMessages1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int numMessages, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SkipMessagesWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int numMessages, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> SplitNamespaceBundleWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, bool? unload = false, string splitAlgorithmName = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> Terminate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> TerminatePartitionedTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> TerminatePartitionedTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> TerminateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IsCompatibilityResponse>> TestCompatibilityWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload body = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> TriggerOffload1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> TriggerOffloadWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnloadNamespaceBundleWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnloadNamespaceWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnloadTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnloadTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnsubscribeNamespaceBundleWithHttpMessagesAsync(string tenant, string namespaceParameter, string subscription, string bundle, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UnsubscribeNamespaceWithHttpMessagesAsync(string tenant, string namespaceParameter, string subscription, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdateBookieRackInfoWithHttpMessagesAsync(string bookie, string group = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdateClusterWithHttpMessagesAsync(string cluster, ClusterData body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdateDynamicConfigurationWithHttpMessagesAsync(string configName, string configValue, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdatePartitionedTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int body, bool? updateLocalTopicOnly = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdatePartitionedTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, int body, bool? updateLocalTopicOnly = false, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> UpdateTenantWithHttpMessagesAsync(string tenant, TenantInfo body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
