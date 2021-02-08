namespace SharpPulsar.Function
{
    public enum FunctionCommand
    {
        /// <summary>
        /// Arguments[string tenant, string namespace]
        /// </summary>
        /// <returns>string</returns>
        ListFunctions,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns>FunctionConfig</returns>
        GetFunctionInfo,

        /// <summary>
        /// Arguments[FunctionConfig config, string pkgUrl, string file]
        /// </summary>
        /// <returns></returns>
        RegisterFunction,

        /// <summary>
        /// Arguments[FunctionConfig config, UpdateOptions options, string pkgUrl, string file]
        /// </summary>
        /// <returns></returns>
        UpdateFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns></returns>
        DeregisterFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns></returns>
        RestartFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns></returns>
        StartFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string key]
        /// </summary>
        /// <returns>FunctionState</returns>
        GetFunctionState,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string key]
        /// </summary>
        /// <returns></returns>
        PutFunctionState,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns>FunctionStats</returns>
        GetFunctionStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns>FunctionStatus</returns>
        GetFunctionStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName]
        /// </summary>
        /// <returns></returns>
        StopFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, object value]
        /// https://pulsar.apache.org/functions-rest-api/?version=2.5.0#operation/triggerFunction
        /// </summary>
        /// <returns>Message</returns>
        TriggerFunction,

        /// <summary>
        /// Arguments[string tenant, string @namespace, string functionName, string topic, string triggerValue, string triggerFile]
        /// </summary>
        /// <returns></returns>
        RestartInstanceFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        /// <returns></returns>
        StartInstanceFunction,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        /// <returns>FunctionInstanceStatsData</returns>
        GetFunctionInstanceStats,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        /// <returns>FunctionInstanceStatusData</returns>
        GetFunctionInstanceStatus,

        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
        /// </summary>
        /// <returns></returns>
        StopInstanceFunction

    }
}
