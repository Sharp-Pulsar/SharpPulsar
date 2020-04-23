
namespace SharpPulsar.Akka.Function
{
    public enum FunctionCommand
    {
        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        ListFunctions,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        GetFunctionInfo,
        /// <summary>
        /// Arguments[FunctionConfig config, string pkgUrl, string file]
        /// </summary>
        RegisterFunction,
        /// <summary>
        /// Arguments[FunctionConfig config, UpdateOptions options, string pkgUrl, string file]
        /// </summary>
        UpdateFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        DeregisterFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        RestartFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        StartFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string key]
        /// </summary>
        GetFunctionState,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string key]
        /// </summary>
        PutFunctionState,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        GetFunctionStats,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        GetFunctionStatus,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        StopFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, object value]
        /// https://pulsar.apache.org/functions-rest-api/?version=2.5.0#operation/triggerFunction
        /// </summary>
        TriggerFunction,
        /// <summary>
        /// Arguments[string tenant, string @namespace, string functionName, string topic, string triggerValue, string triggerFile]
        /// </summary>
        RestartInstanceFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        StartInstanceFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        GetFunctionInstanceStats,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        GetFunctionInstanceStatus,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        StopInstanceFunction

    }
}
