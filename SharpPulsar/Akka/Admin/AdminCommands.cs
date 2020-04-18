
namespace SharpPulsar.Akka.Admin
{
    public enum AdminCommands
    {
        Help,
        GetBookiesRackInfo,
        GetBookieRackInfo,
        UpdateBookieRackInfo,
        DeleteBookieRackInfo,
        GetAllocatorStats,
        GetPendingBookieOpsStats,
        GetBrokerResourceAvailability,
        GetLoadReport,
        GetMBeans,
        GetMetrics,
        GetTopics2,
        GetDynamicConfigurationName,
        GetRuntimeConfiguration,
        GetAllDynamicConfigurations,
        DeleteDynamicConfiguration,
        UpdateDynamicConfiguration,
        Healthcheck,
        GetInternalConfigurationData,
        GetOwnedNamespaces,
        GetActiveBrokers,
        GetClusters,
        GetCluster,
        UpdateCluster,
        CreateCluster,
        DeleteCluster,
        GetFailureDomains,
        GetDomain
    }
}
