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


        public Task<HttpOperationResponse<bool?>> GetIsAllowAutoUpdateSchemaWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetLastMessageId1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetLastMessageIdWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetList1WithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetListFromBundleWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetListWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<LoadReport>> GetLoadReportWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetManagedLedgerInfo1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetManagedLedgerInfoWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxConsumers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxConsumersPerSubscription1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxConsumersPerSubscription2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxConsumersPerSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxConsumersPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxConsumersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxMessageSize1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxMessageSizeWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxProducers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxProducersPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxProducersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxSubscriptionsPerTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxSubscriptionsPerTopic2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxSubscriptionsPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxUnackedMessagesOnConsumer1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxUnackedMessagesOnConsumerWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxUnackedMessagesOnSubscription1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMaxUnackedMessagesOnSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxUnackedMessagesPerConsumerWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMaxUnackedmessagesPerSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<Metrics>>> GetMBeansWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMessageById1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetMessageByIdWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, long ledgerId, long entryId, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMessageTTL1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<Metrics>>> GetMetricsWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> GetNamespaceAntiAffinityGroupWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<ResourceQuota>> GetNamespaceBundleResourceQuotaWithHttpMessagesAsync(string tenant, string namespaceParameter, string bundle, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, NamespaceIsolationData>>> GetNamespaceIsolationPoliciesWithHttpMessagesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<NamespaceIsolationData>> GetNamespaceIsolationPolicyWithHttpMessagesAsync(string cluster, string policyName, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetNamespaceMessageTTLWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetNamespaceReplicationClustersWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<long?>> GetOffloadDeletionLagWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetOffloadPolicies1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetOffloadPolicies2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<OffloadPolicies>> GetOffloadPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<long?>> GetOffloadThresholdWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, NamespaceOwnershipStatus>>> GetOwnedNamespacesWithHttpMessagesAsync(string clusterName, string brokerWebserviceurl, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<PartitionedTopicMetadata>> GetPartitionedMetadata1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<PartitionedTopicMetadata>> GetPartitionedMetadataWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? checkAllowAutoCreation = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPartitionedStats1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPartitionedStatsWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? perPartition = true, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetPartitionedTopicList1WithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetPartitionedTopicListWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetPeerClusterWithHttpMessagesAsync(string cluster, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, PendingBookieOpsStats>>> GetPendingBookieOpsStatsWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, object>>> GetPermissionsOnTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, object>>> GetPermissionsOnTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, object>>> GetPermissionsWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPersistence1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPersistence2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<PersistencePolicies>> GetPersistenceWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<Policies>> GetPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPublishRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetPublishRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<DispatchRate>> GetReplicatorDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetRetention1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetRetention2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<RetentionPolicies>> GetRetentionWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IDictionary<string, object>>> GetRuntimeConfigurationWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<GetSchemaResponse>> GetSchema1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string version, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> GetSchemaAutoUpdateCompatibilityStrategyWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<string>> GetSchemaCompatibilityStrategyWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<bool?>> GetSchemaValidtionEnforcedWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<GetSchemaResponse>> GetSchemaWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<TopicStats>> GetStats1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<TopicStats>> GetStatsWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, bool? getPreciseBacklog = false, bool? subscriptionBacklogSize = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscribeRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscribeRate2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<SubscribeRate>> GetSubscribeRateWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscriptionDispatchRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscriptionDispatchRate2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<DispatchRate>> GetSubscriptionDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<int?>> GetSubscriptionExpirationTimeWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscriptions1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GetSubscriptionsWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<TenantInfo>> GetTenantAdminWithHttpMessagesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetTenantNamespacesWithHttpMessagesAsync(string tenant, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetTenantsWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<object>> GetTopics2WithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<IList<string>>> GetTopicsWithHttpMessagesAsync(string tenant, string namespaceParameter, string mode = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<LongSchemaVersion>> GetVersionBySchemaWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload body = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GrantPermissionOnNamespaceWithHttpMessagesAsync(string tenant, string namespaceParameter, string role, IList<string> body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GrantPermissionsOnTopic1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string role, IList<string> body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> GrantPermissionsOnTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string role, IList<string> body = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> HealthcheckWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> IsReadyWithHttpMessagesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ModifyDeduplicationWithHttpMessagesAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> ModifyEncryptionRequiredWithHttpMessagesAsync(string tenant, string namespaceParameter, bool body, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<OffloadProcessStatus>> OffloadStatus1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<OffloadProcessStatus>> OffloadStatusWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> PeekNthMessage1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> PeekNthMessageWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string subName, int messagePosition, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse<PostSchemaResponse>> PostSchemaWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, PostSchemaPayload body = null, bool? authoritative = false, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveAutoSubscriptionCreationWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveAutoTopicCreationWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveBacklogQuota1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveBacklogQuota2WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveBacklogQuotaWithHttpMessagesAsync(string tenant, string namespaceParameter, string backlogQuotaType = null, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveCompactionThreshold1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveCompactionThresholdWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveDeduplicationEnabled1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveDeduplicationEnabledWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveDispatchRate1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveDispatchRateWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveInactiveTopicPoliciesWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxConsumers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxConsumersPerSubscription1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxConsumersPerSubscriptionWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxConsumersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxMessageSize1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxMessageSizeWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxProducers1WithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxProducersPerTopicWithHttpMessagesAsync(string tenant, string namespaceParameter, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<HttpOperationResponse> RemoveMaxProducersWithHttpMessagesAsync(string tenant, string namespaceParameter, string topic, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
