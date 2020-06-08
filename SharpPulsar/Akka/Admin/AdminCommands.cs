
namespace SharpPulsar.Akka.Admin
{
    public enum AdminCommands
    {
        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IDictionary<string, IDictionary<string, BookieInfo>></returns>
        GetBookiesRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// </summary>
        /// <returns>BookieInfo</returns>
        GetBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie, string group]
        /// </summary>
        /// <returns></returns>
        UpdateBookieRackInfo,

        /// <summary>
        /// Arguments[string bookie]
        /// </summary>
        /// <returns></returns>
        DeleteBookieRackInfo,

        /// <summary>
        /// Arguments[string allocator]
        /// </summary>
        /// <returns>AllocatorStats</returns>
        GetAllocatorStats,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IDictionary<string, PendingBookieOpsStats></returns>
        GetPendingBookieOpsStats,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>IDictionary<string, ResourceUnit></returns>
        GetBrokerResourceAvailability,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>LoadReport</returns>
        GetLoadReport,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<Metrics></returns>
        GetMBeans,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<Metrics></returns>
        GetMetrics,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>object</returns>
        GetTopics2,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<object></returns>
        GetDynamicConfigurationName,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetRuntimeConfiguration,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetAllDynamicConfigurations,

        /// <summary>
        /// Arguments[string configName]
        /// </summary>
        /// <returns></returns>
        DeleteDynamicConfiguration,

        /// <summary>
        /// Arguments[string configName, string configValue]
        /// </summary>
        /// <returns></returns>
        UpdateDynamicConfiguration,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns></returns>
        Healthcheck,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>InternalConfigurationData</returns>
        GetInternalConfigurationData,

        /// <summary>
        /// Arguments[string clusterName, string brokerWebserviceurl]
        /// </summary>
        /// <returns>IDictionary<string, NamespaceOwnershipStatus></returns>
        GetOwnedNamespaces,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>IList<string></returns>
        GetActiveBrokers,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<string></returns>
        GetClusters,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>ClusterData</returns>
        GetCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// </summary>
        /// <returns></returns>
        UpdateCluster,

        /// <summary>
        /// Arguments[string cluster, ClusterData body]
        /// </summary>
        /// <returns></returns>
        CreateCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns></returns>
        DeleteCluster,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>IDictionary<string, FailureDomain></returns>
        GetFailureDomains,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// </summary>
        /// <returns>FailureDomain</returns>
        GetDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName, FailureDomain body]
        /// </summary>
        /// <returns></returns>
        SetFailureDomain,

        /// <summary>
        /// Arguments[string cluster, string domainName]
        /// </summary>
        /// <returns></returns>
        DeleteFailureDomain,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>IDictionary<string, NamespaceIsolationData></returns>
        GetNamespaceIsolationPolicies,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>IList<BrokerNamespaceIsolationData></returns>
        GetBrokersWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string broker]
        /// </summary>
        /// <returns>BrokerNamespaceIsolationData</returns>
        GetBrokerWithNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// </summary>
        /// <returns>NamespaceIsolationData</returns>
        GetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName, NamespaceIsolationData body]
        /// </summary>
        /// <returns></returns>
        SetNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster, string policyName]
        /// </summary>
        /// <returns></returns>
        DeleteNamespaceIsolationPolicy,

        /// <summary>
        /// Arguments[string cluster]
        /// </summary>
        /// <returns>IList<string></returns>
        GetPeerCluster,

        /// <summary>
        /// Arguments[string cluster, IList<string> body]
        /// </summary>
        /// <returns></returns>
        SetPeerClusterNames,

        /// <summary>
        /// Arguments[string cluster, string group, string tenant]
        /// </summary>
        /// <returns>IList<object></returns>
        GetAntiAffinityNamespaces,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// </summary>
        /// <returns>BookieAffinityGroupData</returns>
        GetBookieAffinityGroup,

        /// <summary>
        /// Arguments[string property, string namespace]
        /// </summary>
        /// <returns></returns>
        DeleteBookieAffinityGroup,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        /// <returns>IList<string></returns>
        GetTenantNamespaces,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>Policies</returns>
        GetPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace, Policies policies]
        /// You can create namespace without policies, default value will be used at the server-side
        /// </summary>
        /// <returns></returns>
        CreateNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeleteNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>string</returns>
        GetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace, string antiAffinityGroup]
        /// </summary>
        /// <returns></returns>
        SetNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns></returns>
        RemoveNamespaceAntiAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// </summary>
        /// <returns></returns>
        SetBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string backlogQuotaType]
        /// </summary>
        /// <returns></returns>
        RemoveBacklogQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetBacklogQuotaMap,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>BundlesData</returns>
        GetBundlesData,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ClearNamespaceBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ClearNamespaceBacklogForSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>long</returns>
        GetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newThreshold]
        /// </summary>
        /// <returns></returns>
        SetCompactionThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool enableDeduplication]
        /// </summary>
        /// <returns></returns>
        ModifyDeduplication,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>DispatchRate</returns>
        GetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate dispatchRate]
        /// </summary>
        /// <returns></returns>
        SetDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool encryptionRequired]
        /// </summary>
        /// <returns></returns>
        ModifyEncryptionRequired,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>bool</returns>
        GetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool isAllowAutoUpdateSchema]
        /// </summary>
        /// <returns></returns>
        SetIsAllowAutoUpdateSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>int</returns>
        GetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerSubscription]
        /// </summary>
        /// <returns></returns>
        SetMaxConsumersPerSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>int</returns>
        GetMaxConsumersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxConsumersPerTopic]
        /// </summary>
        /// <returns></returns>
        SetMaxConsumersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>int</returns>
        GetMaxProducersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, int maxProducersPerTopic]
        /// </summary>
        /// <returns></returns>
        SetMaxProducersPerTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>int</returns>
        GetNamespaceMessageTTL,

        /// <summary>
        /// Arguments[string tenant, string namespace, int messageTTL]
        /// </summary>
        /// <returns></returns>
        SetNamespaceMessageTTL,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>long</returns>
        GetOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newDeletionLagMs]
        /// </summary>
        /// <returns></returns>
        SetOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns></returns>
        ClearOffloadDeletionLag,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>long</returns>
        GetOffloadThreshold,

        /// <summary>
        /// Arguments[string tenant, string namespace, long newThreshold]
        /// </summary>
        /// <returns></returns>
        SetOffloadThreshold,

        /// <summary>
        /// Arguments[string tenant, string cluster, string namespace]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetPermissions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string role]
        /// </summary>
        /// <returns></returns>
        GrantPermissionOnNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, string role]
        /// </summary>
        /// <returns></returns>
        RevokePermissionsOnNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>PersistencePolicies</returns>
        GetPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, PersistencePolicies persistence]
        /// </summary>
        /// <returns></returns>
        SetPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, BookieAffinityGroupData bookieAffinity]
        /// </summary>
        /// <returns></returns>
        SetBookieAffinityGroup,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>IList<string></string></returns>
        GetNamespaceReplicationClusters,

        /// <summary>
        /// Arguments[string tenant, string namespace, List<string> clusterIds]
        /// </summary>
        /// <returns></returns>
        SetNamespaceReplicationClusters,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>DispatchRate</returns>
        GetReplicatorDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate dispatchRate]
        /// </summary>
        /// <returns></returns>
        SetReplicatorDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>RetentionPolicies</returns>
        GetRetention,

        /// <summary>
        /// Arguments[string tenant, string namespace, RetentionPolicies retention]
        /// </summary>
        /// <returns></returns>
        SetRetention,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>string</returns>
        GetSchemaAutoUpdateCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace, SchemaAutoUpdateCompatibilityStrategy strategy]
        /// </summary>
        /// <returns></returns>
        SetSchemaAutoUpdateCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>string</returns>
        GetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace, SchemaCompatibilityStrategy strategy]
        /// </summary>
        /// <returns></returns>
        SetSchemaCompatibilityStrategy,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>bool</returns>
        GetSchemaValidtionEnforced,

        /// <summary>
        /// Arguments[string tenant, string namespace, bool schemaValidationEnforced]
        /// </summary>
        /// <returns></returns>
        SetSchemaValidtionEnforced,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>SubscribeRate</returns>
        GetSubscribeRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, SubscribeRate rate]
        /// </summary>
        /// <returns></returns>
        SetSubscribeRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, SubscriptionAuthMode subscriptionAuthMode]
        /// </summary>
        /// <returns></returns>
        SetSubscriptionAuthMode,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>DispatchRate</returns>
        GetSubscriptionDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, DispatchRate rate]
        /// </summary>
        /// <returns></returns>
        SetSubscriptionDispatchRate,

        /// <summary>
        /// Arguments[string tenant, string namespace, string mode]
        /// mode possible values include: 'PERSISTENT', 'NON_PERSISTENT', 'ALL'
        /// </summary>
        /// <returns>IList<string></string></returns>
        GetTopics,

        /// <summary>
        /// Arguments[string tenant, string namespaceParameter]
        /// </summary>
        /// <returns></returns>
        UnloadNamespace,

        /// <summary>
        /// Arguments[string tenant, string cluster, string namespace, string subscription, bool authoritative]
        /// </summary>
        /// <returns></returns>
        UnsubscribeNamespace,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeleteNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ClearNamespaceBundleBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, string bundle, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ClearNamespaceBundleBacklogForSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative, bool unload, string splitAlgorithmName]
        /// </summary>
        /// <returns></returns>
        SplitNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle, bool authoritative]
        /// </summary>
        /// <returns></returns>
        UnloadNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string subscription, string bundle, bool authoritative]
        /// </summary>
        /// <returns></returns>
        UnsubscribeNamespaceBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>IList<string></string></returns>
        GetList,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// Persistent Topic
        /// </summary>
        /// <returns>IList<string></returns>
        GetListPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>IList<string></returns>
        GetPartitionedTopicList,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// Persistent Topic
        /// </summary>
        /// <returns>IList<string></string></returns>
        GetPartitionedTopicListPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        /// <returns>IList<string></string></returns>
        GetListFromBundle,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        CreateNonPartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        CreateNonPartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeleteTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeletePersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ExpireMessagesForAllSubscriptions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ExpireMessagesForAllSubscriptionsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>PersistentOfflineTopicStats</returns>
        GetBacklog,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>PersistentOfflineTopicStats</returns>
        GetBacklogPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>LongRunningProcessStatus</returns>
        CompactionStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>LongRunningProcessStatus</returns>
        CompactionStatusPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        Compact,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        CompactPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        /// <returns></returns>
        GetManagedLedgerInfo,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        /// <returns></returns>
        GetManagedLedgerInfoPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>PersistentTopicInternalStats</returns>
        GetInternalStats,
        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>PersistentTopicInternalStats</returns>
        GetInternalStatsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>object</returns>
        GetLastMessageId,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>object</returns>
        GetLastMessageIdPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>OffloadProcessStatus</returns>
        OffloadStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>OffloadProcessStatus</returns>
        OffloadStatusPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        TriggerOffload,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        TriggerOffloadPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool perPartition, bool authoritative]
        /// </summary>
        /// <returns></returns>
        GetPartitionedStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool perPartition, bool authoritative]
        /// </summary>
        /// <returns></returns>
        GetPartitionedStatsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative, bool checkAllowAutoCreation]
        /// </summary>
        /// <returns>PartitionedTopicMetadata</returns>
        GetPartitionedMetadata,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative, bool checkAllowAutoCreation]
        /// </summary>
        /// <returns>PartitionedTopicMetadata</returns>
        GetPartitionedMetadataPersistence,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body, bool updateLocalTopicOnly]
        /// </summary>
        /// <returns></returns>
        UpdatePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body, bool updateLocalTopicOnly]
        /// </summary>
        /// <returns></returns>
        UpdatePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body]
        /// </summary>
        /// <returns></returns>
        CreatePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, int body]
        /// </summary>
        /// <returns></returns>
        CreatePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeletePartitionedTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool force, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeletePartitionedPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetPermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic]
        /// </summary>
        /// <returns>IDictionary<string, object></returns>
        GetPermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role, IList<string> action]
        /// action = Actions to be granted (produce,functions,consume) 
        /// </summary>
        /// <returns></returns>
        GrantPermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role, IList<string> action]
        /// action = Actions to be granted (produce,functions,consume) 
        /// </summary>
        /// <returns></returns>
        GrantPermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role]
        /// </summary>
        /// <returns></returns>
        RevokePermissionsOnTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string role]
        /// </summary>
        /// <returns></returns>
        RevokePermissionsOnPersistentTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>TopicStats</returns>
        GetStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>TopicStats</returns>
        GetPersistentStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeleteSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        /// <returns></returns>
        DeleteSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ExpireTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int expireTimeInSeconds, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ExpirePersistentTopicMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// </summary>
        /// <returns></returns>
        PeekNthMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int messagePosition, bool authoritative]
        /// </summary>
        /// <returns></returns>
        PeekNthPersistentMessage,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// </summary>
        /// <returns></returns>
        ResetCursorOnPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative, MessageIdImpl messageId]
        /// </summary>
        /// <returns></returns>
        ResetCursorOnPersistentPosition,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ResetCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, long Timestamp, bool authoritative]
        /// </summary>
        /// <returns></returns>
        ResetPersistentCursor,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// </summary>
        /// <returns></returns>
        SkipMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, int numMessages, bool authoritative]
        /// </summary>
        /// <returns></returns>
        SkipPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        /// <returns></returns>
        SkipAllMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subName, bool authoritative]
        /// </summary>
        /// <returns></returns>
        SkipAllPersistentMessages,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// </summary>
        /// <returns></returns>
        CreateSubscription,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string subscriptionName, string messageId, bool replicated]
        /// messageId where to create the subscription. It can be 'latest', 'earliest' or (ledgerId:entryId)
        /// </summary>
        /// <returns></returns>
        CreateSubscriptionPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        GetSubscriptions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        GetSubscriptionsPersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>object</returns>
        Terminate,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>object</returns>
        TerminatePersistent,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        UnloadTopic,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns></returns>
        UnloadPersistentTopic,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<string></returns>
        GetDefaultResourceQuota,

        /// <summary>
        /// Arguments[ResourceQuota quota]
        /// </summary>
        /// <returns>IList<string></returns>
        SetDefaultResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        /// <returns>ResourceQuota</returns>
        GetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        /// <returns></returns>
        SetNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string bundle]
        /// </summary>
        /// <returns></returns>
        RemoveNamespaceBundleResourceQuota,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        /// <returns>IsCompatibilityResponse</returns>
        TestCompatibility,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        /// <returns>PostSchemaResponse</returns>
        PostSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, PostSchemaPayload body, bool authoritative]
        /// </summary>
        /// <returns>LongSchemaVersion</returns>
        GetVersionBySchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>DeleteSchemaResponse</returns>
        DeleteSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>GetSchemaResponse</returns>
        GetSchema,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, string version, bool authoritative]
        /// </summary>
        /// <returns>GetSchemaResponse</returns>
        GetSchemaVersion,

        /// <summary>
        /// Arguments[string tenant, string namespace, string topic, bool authoritative]
        /// </summary>
        /// <returns>GetAllVersionsSchemaResponse</returns>
        GetAllSchemas,

        /// <summary>
        /// Arguments[]
        /// </summary>
        /// <returns>IList<string></returns>
        GetTenants,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        /// <returns>TenantInfo</returns>
        GetTenantAdmin,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// </summary>
        /// <returns></returns>
        UpdateTenant,

        /// <summary>
        /// Arguments[string tenant, TenantInfo body]
        /// </summary>
        /// <returns></returns>
        CreateTenant,

        /// <summary>
        /// Arguments[string tenant]
        /// </summary>
        /// <returns></returns>
        DeleteTenant,

        /// <summary>
        /// Arguments[string tenant, string namespace, DelayedDeliveryPolicies deliveryPolicies]
        /// </summary>
        /// <returns></returns>
        SetDelayedDeliveryPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>DelayedDeliveryPolicies</returns>
        GetDelayedDeliveryPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace, OffloadPolicies offload]
        /// </summary>
        /// <returns></returns>
        SetOffloadPolicies,

        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>OffloadPolicies</returns>
        GetOffloadPolicies
    }
}
