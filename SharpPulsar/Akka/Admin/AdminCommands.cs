
namespace SharpPulsar.Akka.Admin
{
    public enum AdminCommands
    {
        /// <summary>
        /// Arguments[]
        /// returns IDictionary<string, IDictionary<string, BookieInfo>>
        /// </summary>
        GetBookiesRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// returns BookieInfo
        /// </summary>
        GetBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie, string group]
        /// void
        /// </summary>
        UpdateBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// void
        /// </summary>
        DeleteBookieRackInfo,

        /// <summary>
        /// Arguments[string allocator]
        /// returns AllocatorStats
        /// </summary>
        GetAllocatorStats,

        /// <summary>
        /// Arguments[]
        /// returns IDictionary<string, PendingBookieOpsStats>
        /// </summary>
        GetPendingBookieOpsStats,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns  IDictionary<string, ResourceUnit>
        /// </summary>
        GetBrokerResourceAvailability,

        /// <summary>
        /// Arguments[]
        /// returns LoadReport
        /// </summary>
        GetLoadReport,

        /// <summary>
        /// Arguments[]
        /// returns IList<Metrics>
        /// </summary>
        GetMBeans,

        /// <summary>
        /// Arguments[]
        /// returns IList<Metrics>
        /// </summary>
        GetMetrics,

        /// <summary>
        /// Arguments[]
        /// returns object
        /// </summary>
        GetTopics2,

        /// <summary>
        /// Arguments[]
        /// returns IList<object>
        /// </summary>
        GetDynamicConfigurationName,

        /// <summary>
        /// Arguments[]
        /// returns IDictionary<string, object>
        /// </summary>
        GetRuntimeConfiguration,

        /// <summary>
        /// Arguments[]
        /// returns IDictionary<string, object>
        /// </summary>
        GetAllDynamicConfigurations,

        /// <summary>
        /// Arguments[string configName]
        /// void
        /// </summary>
        DeleteDynamicConfiguration,

        /// <summary>
        /// Arguments[string configName, string configValue]
        /// void
        /// </summary>
        UpdateDynamicConfiguration,

        /// <summary>
        /// Arguments[]
        /// void
        /// </summary>
        Healthcheck,

        /// <summary>
        /// Arguments[]
        /// returns InternalConfigurationData
        /// </summary>
        GetInternalConfigurationData,

        /// <summary>
        /// Arguments[string clusterName, string brokerWebserviceurl]
        /// returns IDictionary<string, NamespaceOwnershipStatus>
        /// </summary>
        GetOwnedNamespaces,

        /// <summary>
        /// Arguments[string cluster]
        /// returns IList<string>
        /// </summary>
        GetActiveBrokers,

        /// <summary>
        /// Arguments[]
        /// returns IList<string>
        /// </summary>
        GetClusters,

        /// <summary>
        /// Arguments[string cluster]
        /// returns ClusterData
        /// </summary>
        GetCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// void
        /// </summary>
        UpdateCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// void
        /// </summary>
        CreateCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// void
        /// </summary>
        DeleteCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// returns IDictionary<string, FailureDomain>
        /// </summary>
        GetFailureDomains,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// returns FailureDomain
        /// </summary>
        GetDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName, FailureDomain body]
        /// void
        /// </summary>
        SetFailureDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// void
        /// </summary>
        DeleteFailureDomain,

        /// <summary>
        /// Arguments[string cluster]
        /// returns IDictionary<string, NamespaceIsolationData>
        /// </summary>
        GetNamespaceIsolationPolicies,

        /// <summary>
        /// Arguments[string cluster]
        /// returns IList<BrokerNamespaceIsolationData>
        /// </summary>
        GetBrokersWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string broker]
        /// returns BrokerNamespaceIsolationData
        /// </summary>
        GetBrokerWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// returns  NamespaceIsolationData
        /// </summary>
        GetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName, NamespaceIsolationData body]
        /// void
        /// </summary>
        SetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// void
        /// </summary>
        DeleteNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster]
        /// returns IList<string>
        /// </summary>
        GetPeerCluster,

        /// <summary>
        /// Arguments[string cluster, IList<string> body]
        /// void
        /// </summary>
        SetPeerClusterNames,

        /// <summary>
        /// Arguments[string cluster, string group, string tenant]
        /// returns IList<object>
        /// </summary>
        GetAntiAffinityNamespaces,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// returns BookieAffinityGroupData 
        /// </summary>
        GetBookieAffinityGroup,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// void
        /// </summary>
        DeleteBookieAffinityGroup,

        /// <summary>
        /// Arguments[string tenant]
        /// returns IList<string>
        /// </summary>
        GetTenantNamespaces,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns Policies
        /// </summary>
        GetPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace, Policies policies]
        /// You can create namespace without policies, default value will be used at the server-side
        /// void
        /// </summary>
        CreateNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// void
        /// </summary>
        DeleteNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns string
        /// </summary>
        GetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace, string antiAffinityGroup]
        /// void
        /// </summary>
        SetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// void
        /// </summary>
        RemoveNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// void
        /// </summary>
        SetBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// void
        /// </summary>
        RemoveBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns IDictionary<string, object>
        /// </summary>
        GetBacklogQuotaMap,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns BundlesData
        /// </summary>
        GetBundlesData,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// void
        /// </summary>
        ClearNamespaceBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, bool authoritative]
        /// void
        /// </summary>
        ClearNamespaceBacklogForSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns long
        /// </summary>
        GetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newThreshold]
        /// void
        /// </summary>
        SetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool enableDeduplication]
        /// void
        /// </summary>
        ModifyDeduplication,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns DispatchRate
        /// </summary>
        GetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate dispatchRate]
        /// </summary>
        SetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool encryptionRequired]
        /// void
        /// </summary>
        ModifyEncryptionRequired,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns bool
        /// </summary>
        GetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool isAllowAutoUpdateSchema]
        /// void
        /// </summary>
        SetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns int
        /// </summary>
        GetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerSubscription]
        /// void
        /// </summary>
        SetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns int
        /// </summary>
        GetMaxConsumersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerTopic]
        /// void
        /// </summary>
        SetMaxConsumersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetMaxProducersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxProducersPerTopic]
        /// </summary>
        SetMaxProducersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetNamespaceMessageTTL,

        /// <summary>
        /// Arguments[string tenant, string namespace, int messageTTL]
        /// </summary>
        SetNamespaceMessageTTL,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newDeletionLagMs]
        /// </summary>
        SetOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        ClearOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetOffloadThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newThreshold]
        /// </summary>
        SetOffloadThreshold,

        /// <summary>
        /// Arguments[string tenant, string cluster, string namespace]
        /// </summary>
        GetPermissions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string role]
        /// </summary>
        GrantPermissionOnNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, string role]
        /// </summary>
        RevokePermissionsOnNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, PersistencePolicies persistence]
        /// </summary>
        SetPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, BookieAffinityGroupData bookieAffinity]
        /// </summary>
        SetBookieAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetNamespaceReplicationClusters,

        /// <summary>
        /// Arguments[string tenant, string namespace, List<string> clusterIds]
        /// </summary>
        SetNamespaceReplicationClusters,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetReplicatorDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate dispatchRate]
        /// </summary>
        SetReplicatorDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetRetention,

        /// <summary>
        /// Arguments[string tenant, string namespace, RetentionPolicies retention]
        /// </summary>
        SetRetention,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSchemaAutoUpdateCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace, SchemaAutoUpdateCompatibilityStrategy strategy]
        /// </summary>
        SetSchemaAutoUpdateCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace, SchemaCompatibilityStrategy strategy]
        /// </summary>
        SetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSchemaValidtionEnforced,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool schemaValidationEnforced]
        /// </summary>
        SetSchemaValidtionEnforced,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSubscribeRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, SubscribeRate rate]
        /// </summary>
        SetSubscribeRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, SubscriptionAuthMode subscriptionAuthMode]
        /// </summary>
        SetSubscriptionAuthMode,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSubscriptionDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate rate]
        /// </summary>
        SetSubscriptionDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, string mode]
        /// mode possible values include: 'PERSISTENT', 'NON_PERSISTENT', 'ALL'
        /// </summary>
        GetTopics,

        /// <summary>
        /// Arguments[string tenant, string namespaceParameter]
        /// </summary>
        UnloadNamespace,

        /// <summary>
        /// Arguments[string tenant, string cluster, string namespace, string subscription, bool authoritative]
        /// </summary>
        UnsubscribeNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        DeleteNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        ClearNamespaceBundleBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, string bundle, bool authoritative]
        /// </summary>
        ClearNamespaceBundleBacklogForSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative, bool unload, string splitAlgorithmName]
        /// </summary>
        SplitNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        UnloadNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, string bundle, bool authoritative]
        /// </summary>
        UnsubscribeNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetList,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// Persistent Topic
        /// </summary>
        GetListPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetPartitionedTopicList,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// Persistent Topic
        /// </summary>
        GetPartitionedTopicListPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        GetListFromBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        CreateNonPartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        CreateNonPartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        DeleteTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        DeletePersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        ExpireMessagesForAllSubscriptions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        ExpireMessagesForAllSubscriptionsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetBacklogPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        CompactionStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        CompactionStatusPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        Compact,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        CompactPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        GetManagedLedgerInfo,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        GetManagedLedgerInfoPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetInternalStats,
        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetInternalStatsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetLastMessageId,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetLastMessageIdPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        OffloadStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        OffloadStatusPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        TriggerOffload,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        TriggerOffloadPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool perPartition, bool authoritative]
        /// </summary>
        GetPartitionedStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool perPartition, bool authoritative]
        /// </summary>
        GetPartitionedStatsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative, bool checkAllowAutoCreation]
        /// </summary>
        GetPartitionedMetadata,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative, bool checkAllowAutoCreation]
        /// </summary>
        GetPartitionedMetadataPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body, bool updateLocalTopicOnly]
        /// </summary>
        UpdatePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body, bool updateLocalTopicOnly]
        /// </summary>
        UpdatePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body]
        /// </summary>
        CreatePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body]
        /// </summary>
        CreatePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        DeletePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        DeletePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        GetPermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        GetPermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role, IList<string> action]
        /// action = Actions to be granted (produce,functions,consume) 
        /// </summary>
        GrantPermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role, IList<string> action]
        /// action = Actions to be granted (produce,functions,consume) 
        /// </summary>
        GrantPermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role]
        /// </summary>
        RevokePermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role]
        /// </summary>
        RevokePermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns TopicStats
        /// </summary>
        GetStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns TopicStats
        /// </summary>
        GetPersistentStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// void
        /// </summary>
        DeleteSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// void
        /// </summary>
        DeleteSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// void
        /// </summary>
        ExpireTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// void
        /// </summary>
        ExpirePersistentTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// void
        /// </summary>
        PeekNthMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// void
        /// </summary>
        PeekNthPersistentMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// void
        /// </summary>
        ResetCursorOnPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// void
        /// </summary>
        ResetCursorOnPersistentPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// void
        /// </summary>
        ResetCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// void
        /// </summary>
        ResetPersistentCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// void
        /// </summary>
        SkipMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// void
        /// </summary>
        SkipPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// void
        /// </summary>
        SkipAllMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// void
        /// </summary>
        SkipAllPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// void
        /// </summary>
        CreateSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// void
        /// </summary>
        CreateSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// void
        /// </summary>
        GetSubscriptions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// void
        /// void
        /// </summary>
        GetSubscriptionsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns object
        /// </summary>
        Terminate,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns object
        /// </summary>
        TerminatePersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// void
        /// </summary>
        UnloadTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// void
        /// </summary>
        UnloadPersistentTopic,

        /// <summary>
        /// Arguments[]
        /// returns IList<string>
        /// </summary>
        GetDefaultResourceQuota,

        /// <summary>
        /// Arguments[ResourceQuota quota]
        /// returns IList<string>
        /// </summary>
        SetDefaultResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// returns  ResourceQuota
        /// </summary>
        GetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// void
        /// </summary>
        SetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// void
        /// </summary>
        RemoveNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// returns IsCompatibilityResponse
        /// </summary>
        TestCompatibility,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// returns PostSchemaResponse
        /// </summary>
        PostSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// returns LongSchemaVersion
        /// </summary>
        GetVersionBySchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns DeleteSchemaResponse
        /// </summary>
        DeleteSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns  GetSchemaResponse
        /// </summary>
        GetSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string version, bool authoritative]
        /// return GetSchemaResponse
        /// </summary>
        GetSchemaVersion,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// returns GetAllVersionsSchemaResponse
        /// </summary>
        GetAllSchemas,

        /// <summary>
        /// Arguments[]
        /// returns IList<string>
        /// </summary>
        GetTenants,

        /// <summary>
        /// Arguments[string tenant]
        /// returns TenantInfo
        /// </summary>
        GetTenantAdmin,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// void
        /// </summary>
        UpdateTenant,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// void
        /// </summary>
        CreateTenant,

        /// <summary>
        /// Arguments[string tenant]
        /// void
        /// </summary>
        DeleteTenant,

        /// <summary>
        /// Arguments[string tenant, string namespace, DelayedDeliveryPolicies deliveryPolicies]
        /// void
        /// </summary>
        SetDelayedDeliveryPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// returns DelayedDeliveryPolicies
        /// </summary>
        GetDelayedDeliveryPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace, OffloadPolicies offload]
        /// void
        /// </summary>
        SetOffloadPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// return OffloadPolicies
        /// </summary>
        GetOffloadPolicies
    }
}
