
namespace SharpPulsar.Akka.Admin
{
    public enum AdminCommands
    {
        /// <summary>
        /// Arguments[]
        /// </summary>
        GetBookiesRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// </summary>
        GetBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie, string group]
        /// </summary>
        UpdateBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// </summary>
        DeleteBookieRackInfo,

        /// <summary>
        /// Arguments[string allocator]
        /// </summary>
        GetAllocatorStats,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetPendingBookieOpsStats,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetBrokerResourceAvailability,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetLoadReport,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetMBeans,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetMetrics,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetTopics2,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetDynamicConfigurationName,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetRuntimeConfiguration,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetAllDynamicConfigurations,

        /// <summary>
        /// Arguments[string configName]
        /// </summary>
        DeleteDynamicConfiguration,

        /// <summary>
        /// Arguments[string configName, string configValue]
        /// </summary>
        UpdateDynamicConfiguration,

        /// <summary>
        /// Arguments[]
        /// </summary>
        Healthcheck,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetInternalConfigurationData,

        /// <summary>
        /// Arguments[string clusterName, string brokerWebserviceurl]
        /// </summary>
        GetOwnedNamespaces,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetActiveBrokers,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetClusters,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// </summary>
        UpdateCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// </summary>
        CreateCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        DeleteCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetFailureDomains,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// </summary>
        GetDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName, FailureDomain body]
        /// </summary>
        SetFailureDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// </summary>
        DeleteFailureDomain,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetNamespaceIsolationPolicies,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetBrokersWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string broker]
        /// </summary>
        GetBrokerWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// </summary>
        GetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName, NamespaceIsolationData body]
        /// </summary>
        SetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// </summary>
        DeleteNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        GetPeerCluster,

        /// <summary>
        /// Arguments[string cluster, IList<string> body]
        /// </summary>
        SetPeerClusterNames,

        /// <summary>
        /// Arguments[string cluster, string group, string tenant]
        /// </summary>
        GetAntiAffinityNamespaces,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// </summary>
        GetBookieAffinityGroup,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// </summary>
        DeleteBookieAffinityGroup,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        GetTenantNamespaces,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        CreateNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// </summary>
        DeleteNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        RemoveNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// </summary>
        SetBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// </summary>
        RemoveBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetBacklogQuotaMap,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetBundlesData,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// </summary>
        ClearNamespaceBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, bool authoritative]
        /// </summary>
        ClearNamespaceBacklogForSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool enableDeduplication]
        /// </summary>
        ModifyDeduplication,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool encryptionRequired]
        /// </summary>
        ModifyEncryptionRequired,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerSubscription]
        /// </summary>
        SetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetMaxConsumersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerTopic]
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
        /// Arguments[string tenant, string namespace]
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
        /// Arguments[string tenant, string namespace]
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
        /// Arguments[string tenant, string namespace]
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
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetSchemaAutoUpdateCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        SetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        GetSchemaValidtionEnforced,

        /// <summary>
        /// Arguments[string tenant, string namespace]
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
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative, bool unload]
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
        /// </summary>
        GetStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetPersistentStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        DeleteSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        DeleteSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        ExpireTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        ExpirePersistentTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// </summary>
        PeekNthMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// </summary>
        PeekNthPersistentMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// </summary>
        ResetCursorOnPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// </summary>
        ResetCursorOnPersistentPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// </summary>
        ResetCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// </summary>
        ResetPersistentCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// </summary>
        SkipMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// </summary>
        SkipPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        SkipAllMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        SkipAllPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// </summary>
        CreateSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// </summary>
        CreateSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetSubscriptions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetSubscriptionsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        Terminate,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        TerminatePersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        UnloadTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        UnloadPersistentTopic,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetDefaultResourceQuota,

        /// <summary>
        /// Arguments[]
        /// </summary>
        SetDefaultResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        GetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        SetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        RemoveNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        TestCompatibility,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        PostSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        GetVersionBySchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        DeleteSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string version, bool authoritative]
        /// </summary>
        GetSchemaVersion,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        GetAllSchemas,

        /// <summary>
        /// Arguments[]
        /// </summary>
        GetTenants,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        GetTenantAdmin,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// </summary>
        UpdateTenant,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// </summary>
        CreateTenant,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        DeleteTenant
    }
}
