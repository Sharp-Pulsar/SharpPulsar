using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Admin;
using Xunit;
using Xunit.Abstractions;

namespace SharpPulsar.Test.Admin
{
    public class NamespacesTest
    {
        private readonly ITestOutputHelper _output;
        private readonly Namespaces _namespaces;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        public NamespacesTest(ITestOutputHelper output)
        {
            _output = output;
            _namespaces = new Namespaces("http://localhost:8080/", new HttpClient());
        }
        [Fact]
        public async Task ClearNamespaceBacklog()
        {
            var namespaces = await _namespaces.ClearNamespaceBacklogAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task ClearNamespaceBacklogForSubscription()
        {
            var namespaces = await _namespaces.ClearNamespaceBacklogForSubscriptionAsync("public", "default", "subscription");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task ClearNamespaceBundleBacklog()
        {
            var namespaces = await _namespaces.ClearNamespaceBundleBacklogAsync("public", "default", "");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task ClearNamespaceBundleBacklogForSubscription()
        {
            var namespaces = await _namespaces.ClearNamespaceBundleBacklogForSubscriptionAsync("public", "default", "", "");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }

        [Fact]
        public async Task ClearOffloadDeletionLag()
        {
            var namespaces = await _namespaces.ClearOffloadDeletionLagAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }

        [Fact]
        public async Task ClearProperties()
        {
            var namespaces = await _namespaces.ClearPropertiesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task CreateNamespaceAsync()
        {
            var namespaces = await _namespaces.CreateNamespaceAsync("public", "default", null);
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteBookieAffinityGroup()
        {
            var namespaces = await _namespaces.DeleteBookieAffinityGroupAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteCompactionThreshold()
        {
            var namespaces = await _namespaces.DeleteCompactionThresholdAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteDispatchRate()
        {
            var namespaces = await _namespaces.DeleteDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteNamespaceAsync()
        {
            var namespaces = await _namespaces.DeleteNamespaceAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteNamespaceBundle()
        {
            var namespaces = await _namespaces.DeleteNamespaceBundleAsync("public", "default", "");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeletePersistence()
        {
            var namespaces = await _namespaces.DeletePersistenceAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteSubscribeRate()
        {
            var namespaces = await _namespaces.DeleteSubscribeRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task DeleteSubscriptionDispatchRate()
        {
            var namespaces = await _namespaces.DeleteSubscriptionDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetAntiAffinityNamespaces()
        {
            var namespaces = await _namespaces.GetAntiAffinityNamespacesAsync("cluster", "group", "public");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetAutoSubscriptionCreation()
        {
            var namespaces = await _namespaces.GetAutoSubscriptionCreationAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetAutoTopicCreation()
        {
            var namespaces = await _namespaces.GetAutoTopicCreationAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetBacklogQuotaMap()
        {
            var namespaces = await _namespaces.GetBacklogQuotaMapAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetBookieAffinityGroup()
        {
            var namespaces = await _namespaces.GetBookieAffinityGroupAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetBundlesData()
        {
            var namespaces = await _namespaces.GetBundlesDataAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetCompactionThreshold()
        {
            var namespaces = await _namespaces.GetCompactionThresholdAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetDeduplication()
        {
            var namespaces = await _namespaces.GetDeduplicationAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetDeduplicationSnapshotInterval()
        {
            var namespaces = await _namespaces.GetDeduplicationSnapshotIntervalAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetDelayedDeliveryPolicies()
        {
            var namespaces = await _namespaces.GetDelayedDeliveryPoliciesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetDispatchRate()
        {
            var namespaces = await _namespaces.GetDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetEncryptionRequired()
        {
            var namespaces = await _namespaces.GetEncryptionRequiredAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetEntryFiltersPerTopic()
        {
            var namespaces = await _namespaces.GetEntryFiltersPerTopicAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetInactiveTopicPolicies()
        {
            var namespaces = await _namespaces.GetInactiveTopicPoliciesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetIsAllowAutoUpdateSchema()
        {
            var namespaces = await _namespaces.GetIsAllowAutoUpdateSchemaAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxConsumersPerSubscription()
        {
            var namespaces = await _namespaces.GetMaxConsumersPerSubscriptionAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxConsumersPerTopic()
        {
            var namespaces = await _namespaces.GetMaxConsumersPerTopicAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxProducersPerTopic()
        {
            var namespaces = await _namespaces.GetMaxProducersPerTopicAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxSubscriptionsPerTopic()
        {
            var namespaces = await _namespaces.GetMaxSubscriptionsPerTopicAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxTopicsPerNamespace()
        {
            var namespaces = await _namespaces.GetMaxTopicsPerNamespaceAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxUnackedMessagesPerConsumer()
        {
            var namespaces = await _namespaces.GetMaxUnackedMessagesPerConsumerAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetMaxUnackedmessagesPerSubscription()
        {
            var namespaces = await _namespaces.GetMaxUnackedmessagesPerSubscriptionAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetNamespaceAntiAffinityGroup()
        {
            var namespaces = await _namespaces.GetNamespaceAntiAffinityGroupAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetNamespaceMessageTTL()
        {
            var namespaces = await _namespaces.GetNamespaceMessageTTLAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetNamespaceReplicationClusters()
        {
            var namespaces = await _namespaces.GetNamespaceReplicationClustersAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetNamespaceResourceGroup()
        {
            var namespaces = await _namespaces.GetNamespaceResourceGroupAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetOffloadDeletionLag()
        {
            var namespaces = await _namespaces.GetOffloadDeletionLagAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetOffloadPolicies()
        {
            var namespaces = await _namespaces.GetOffloadPoliciesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetOffloadThreshold()
        {
            var namespaces = await _namespaces.GetOffloadThresholdAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetPermissionOnSubscription()
        {
            var namespaces = await _namespaces.GetPermissionOnSubscriptionAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetPermissions()
        {
            var namespaces = await _namespaces.GetPermissionsAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetPersistence()
        {
            var namespaces = await _namespaces.GetPersistenceAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetPolicies()
        {
            var namespaces = await _namespaces.GetPoliciesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetProperties()
        {
            var namespaces = await _namespaces.GetPropertiesAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetProperty()
        {
            var namespaces = await _namespaces.GetPropertyAsync("public", "default", "key");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetPublishRate()
        {
            var namespaces = await _namespaces.GetPublishRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetReplicatorDispatchRate()
        {
            var namespaces = await _namespaces.GetReplicatorDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetRetention()
        {
            var namespaces = await _namespaces.GetRetentionAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetScanOffloadedLedgers()
        {
            var namespaces = await _namespaces.GetScanOffloadedLedgersAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSchemaAutoUpdateCompatibilityStrategy()
        {
            var namespaces = await _namespaces.GetSchemaAutoUpdateCompatibilityStrategyAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSchemaCompatibilityStrategy()
        {
            var namespaces = await _namespaces.GetSchemaCompatibilityStrategyAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSchemaValidtionEnforced()
        {
            var namespaces = await _namespaces.GetSchemaValidtionEnforcedAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSubscribeRate()
        {
            var namespaces = await _namespaces.GetSubscribeRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSubscriptionAuthMode()
        {
            var namespaces = await _namespaces.GetSubscriptionAuthModeAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSubscriptionDispatchRate()
        {
            var namespaces = await _namespaces.GetSubscriptionDispatchRateAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSubscriptionExpirationTime()
        {
            var namespaces = await _namespaces.GetSubscriptionExpirationTimeAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetSubscriptionTypesEnabled()
        {
            var namespaces = await _namespaces.GetSubscriptionTypesEnabledAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetTenantNamespaces()
        {
            var namespaces = await _namespaces.GetTenantNamespacesAsync("public");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetTopicHashPositions()
        {
            var namespaces = await _namespaces.GetTopicHashPositionsAsync("public", "default", "");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GetTopics()
        {
            var namespaces = await _namespaces.GetTopicsAsync("public", "default");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GrantPermissionOnNamespace()
        {
            var namespaces = await _namespaces.GrantPermissionOnNamespaceAsync("public", "default", "role");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task GrantPermissionOnSubscription()
        {
            var namespaces = await _namespaces.GrantPermissionOnSubscriptionAsync("public", "default", "");
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task ModifyDeduplication()
        {
            var namespaces = await _namespaces.ModifyDeduplicationAsync("public", "default", true);
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
        [Fact]
        public async Task ModifyEncryptionRequired()
        {
            var namespaces = await _namespaces.ModifyEncryptionRequiredAsync("public", "default", true);
            _output.WriteLine(JsonSerializer.Serialize(namespaces, _jsonSerializerOptions));
            Assert.True(namespaces != null);
        }
    }
}
