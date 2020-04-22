
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
        /// Arguments[string tenant, string namespace, string functionName, Dictionary<string, object> body]
        /// body A JSON value presenting configuration payload of a Pulsar Function.
        /// An example of the expected Pulsar Function can be found here.
        /// https://pulsar.apache.org/functions-rest-api/?version=2.5.0#operation/registerFunction
        /// </summary>
        RegisterFunction,
        /// <summary>
        /// Arguments[string tenant, string namespace, string functionName, Dictionary<string, object> body]
        /// body A JSON value presenting configuration payload of a Pulsar Function.
        /// An example of the expected Pulsar Function can be found here.
        /// https://pulsar.apache.org/functions-rest-api/?version=2.5.0#operation/updateFunction
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
        /// Arguments[string tenant, string namespace, string functionName, string instanceId]
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
