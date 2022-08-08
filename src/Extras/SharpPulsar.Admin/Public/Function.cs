using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Function;

namespace SharpPulsar.Admin.Public
{
    public class Function
    {
        private readonly PulsarFunctionsRESTAPIClient _api;
        public Function(HttpClient httpClient)
        {
            _api = new PulsarFunctionsRESTAPIClient(httpClient);
        }
        /// <summary>Fetches a list of supported Pulsar IO connectors currently running in cluster mode</summary>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        [Obsolete]
        public ICollection<object> GetConnectorsList()
        {
            return GetConnectorsListAsync().GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Fetches a list of supported Pulsar IO connectors currently running in cluster mode</summary>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        [Obsolete]
        public async Task<ICollection<object>> GetConnectorsListAsync(CancellationToken cancellationToken = default)
        {
            return await _api.GetConnectorsListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Lists all Pulsar Functions currently deployed in a given namespace</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public string ListFunctions(string tenant, string @namespace)
        {
            return ListFunctionsAsync(tenant, @namespace).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Lists all Pulsar Functions currently deployed in a given namespace</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<string> ListFunctionsAsync(string tenant, string @namespace, CancellationToken cancellationToken = default)
        {
            return await _api.ListFunctionsAsync(tenant, @namespace, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Fetches information about a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionConfig GetFunctionInfo(string tenant, string @namespace, string functionName)
        {
            return GetFunctionInfoAsync(tenant, @namespace, functionName).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Fetches information about a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionConfig> GetFunctionInfoAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionInfoAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Creates a new Pulsar Function in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="function">A JSON value presenting configuration payload of a Pulsar Function. An example of the expected Pulsar Function can be found here.  
        /// - **autoAck**  
        ///   Whether or not the framework acknowledges messages automatically.  
        /// - **runtime**  
        ///   What is the runtime of the Pulsar Function. Possible Values: [JAVA, PYTHON, GO]  
        /// - **resources**  
        ///   The size of the system resources allowed by the Pulsar Function runtime. The resources include: cpu, ram, disk.  
        /// - **className**  
        ///   The class name of a Pulsar Function.  
        /// - **customSchemaInputs**  
        ///   The map of input topics to Schema class names (specified as a JSON object).  
        /// - **customSerdeInputs**  
        ///   The map of input topics to SerDe class names (specified as a JSON object).  
        /// - **deadLetterTopic**  
        ///   Messages that are not processed successfully are sent to `deadLetterTopic`.  
        /// - **runtimeFlags**  
        ///   Any flags that you want to pass to the runtime. Note that in thread mode, these flags have no impact.  
        /// - **fqfn**  
        ///   The Fully Qualified Function Name (FQFN) for the Pulsar Function.  
        /// - **inputSpecs**  
        ///    The map of input topics to its consumer configuration, each configuration has schema of    {"schemaType": "type-x", "serdeClassName": "name-x", "isRegexPattern": true, "receiverQueueSize": 5}  
        /// - **inputs**  
        ///   The input topic or topics (multiple topics can be specified as a comma-separated list) of a Pulsar Function.  
        /// - **jar**  
        ///   Path to the JAR file for the Pulsar Function (if the Pulsar Function is written in Java).   It also supports URL path [http/https/file (file protocol assumes that file   already exists on worker host)] from which worker can download the package.  
        /// - **py**  
        ///   Path to the main Python file or Python wheel file for the Pulsar Function (if the Pulsar Function is written in Python).  
        /// - **go**  
        ///   Path to the main Go executable binary for the Pulsar Function (if the Pulsar Function is written in Go).  
        /// - **logTopic**  
        ///   The topic to which the logs of a Pulsar Function are produced.  
        /// - **maxMessageRetries**  
        ///   How many times should we try to process a message before giving up.  
        /// - **output**  
        ///   The output topic of a Pulsar Function (If none is specified, no output is written).  
        /// - **outputSerdeClassName**  
        ///   The SerDe class to be used for messages output by the Pulsar Function.  
        /// - **parallelism**  
        ///   The parallelism factor of a Pulsar Function (i.e. the number of a Pulsar Function instances to run).  
        /// - **processingGuarantees**  
        ///   The processing guarantees (that is, delivery semantics) applied to the Pulsar Function.  Possible Values: [ATLEAST_ONCE, ATMOST_ONCE, EFFECTIVELY_ONCE]  
        /// - **retainOrdering**  
        ///   Function consumes and processes messages in order.  
        /// - **outputSchemaType**  
        ///    Represents either a builtin schema type (for example: 'avro', 'json', ect) or the class name for a Schema implementation.- **subName**  
        ///   Pulsar source subscription name. User can specify a subscription-name for the input-topic consumer.  
        /// - **windowConfig**  
        ///   The window configuration of a Pulsar Function.  
        /// - **timeoutMs**  
        ///   The message timeout in milliseconds.  
        /// - **topicsPattern**  
        ///   The topic pattern to consume from a list of topics under a namespace that match the pattern.  [input] and [topic-pattern] are mutually exclusive. Add SerDe class name for a   pattern in customSerdeInputs (supported for java fun only)  
        /// - **userConfig**  
        ///   A map of user-defined configurations (specified as a JSON object).  
        /// - **secrets**  
        ///   This is a map of secretName(that is how the secret is going to be accessed in the Pulsar Function via context) to an object that  encapsulates how the secret is fetched by the underlying secrets provider. The type of an value here can be found by the  SecretProviderConfigurator.getSecretObjectType() method. 
        /// - **cleanupSubscription**  
        ///   Whether the subscriptions of a Pulsar Function created or used should be deleted when the Pulsar Function is deleted.</param>
        /// <returns>Pulsar Function successfully created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void RegisterFunction(string tenant, string @namespace, string functionName, Stream function)
        {
            RegisterFunctionAsync(tenant, @namespace, functionName, function).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Creates a new Pulsar Function in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="function">A JSON value presenting configuration payload of a Pulsar Function. An example of the expected Pulsar Function can be found here.  
        /// - **autoAck**  
        ///   Whether or not the framework acknowledges messages automatically.  
        /// - **runtime**  
        ///   What is the runtime of the Pulsar Function. Possible Values: [JAVA, PYTHON, GO]  
        /// - **resources**  
        ///   The size of the system resources allowed by the Pulsar Function runtime. The resources include: cpu, ram, disk.  
        /// - **className**  
        ///   The class name of a Pulsar Function.  
        /// - **customSchemaInputs**  
        ///   The map of input topics to Schema class names (specified as a JSON object).  
        /// - **customSerdeInputs**  
        ///   The map of input topics to SerDe class names (specified as a JSON object).  
        /// - **deadLetterTopic**  
        ///   Messages that are not processed successfully are sent to `deadLetterTopic`.  
        /// - **runtimeFlags**  
        ///   Any flags that you want to pass to the runtime. Note that in thread mode, these flags have no impact.  
        /// - **fqfn**  
        ///   The Fully Qualified Function Name (FQFN) for the Pulsar Function.  
        /// - **inputSpecs**  
        ///    The map of input topics to its consumer configuration, each configuration has schema of    {"schemaType": "type-x", "serdeClassName": "name-x", "isRegexPattern": true, "receiverQueueSize": 5}  
        /// - **inputs**  
        ///   The input topic or topics (multiple topics can be specified as a comma-separated list) of a Pulsar Function.  
        /// - **jar**  
        ///   Path to the JAR file for the Pulsar Function (if the Pulsar Function is written in Java).   It also supports URL path [http/https/file (file protocol assumes that file   already exists on worker host)] from which worker can download the package.  
        /// - **py**  
        ///   Path to the main Python file or Python wheel file for the Pulsar Function (if the Pulsar Function is written in Python).  
        /// - **go**  
        ///   Path to the main Go executable binary for the Pulsar Function (if the Pulsar Function is written in Go).  
        /// - **logTopic**  
        ///   The topic to which the logs of a Pulsar Function are produced.  
        /// - **maxMessageRetries**  
        ///   How many times should we try to process a message before giving up.  
        /// - **output**  
        ///   The output topic of a Pulsar Function (If none is specified, no output is written).  
        /// - **outputSerdeClassName**  
        ///   The SerDe class to be used for messages output by the Pulsar Function.  
        /// - **parallelism**  
        ///   The parallelism factor of a Pulsar Function (i.e. the number of a Pulsar Function instances to run).  
        /// - **processingGuarantees**  
        ///   The processing guarantees (that is, delivery semantics) applied to the Pulsar Function.  Possible Values: [ATLEAST_ONCE, ATMOST_ONCE, EFFECTIVELY_ONCE]  
        /// - **retainOrdering**  
        ///   Function consumes and processes messages in order.  
        /// - **outputSchemaType**  
        ///    Represents either a builtin schema type (for example: 'avro', 'json', ect) or the class name for a Schema implementation.- **subName**  
        ///   Pulsar source subscription name. User can specify a subscription-name for the input-topic consumer.  
        /// - **windowConfig**  
        ///   The window configuration of a Pulsar Function.  
        /// - **timeoutMs**  
        ///   The message timeout in milliseconds.  
        /// - **topicsPattern**  
        ///   The topic pattern to consume from a list of topics under a namespace that match the pattern.  [input] and [topic-pattern] are mutually exclusive. Add SerDe class name for a   pattern in customSerdeInputs (supported for java fun only)  
        /// - **userConfig**  
        ///   A map of user-defined configurations (specified as a JSON object).  
        /// - **secrets**  
        ///   This is a map of secretName(that is how the secret is going to be accessed in the Pulsar Function via context) to an object that  encapsulates how the secret is fetched by the underlying secrets provider. The type of an value here can be found by the  SecretProviderConfigurator.getSecretObjectType() method. 
        /// - **cleanupSubscription**  
        ///   Whether the subscriptions of a Pulsar Function created or used should be deleted when the Pulsar Function is deleted.</param>
        /// <returns>Pulsar Function successfully created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task RegisterFunctionAsync(string tenant, string @namespace, string functionName, Stream function, CancellationToken cancellationToken = default)
        {
            await _api.RegisterFunctionAsync(tenant, @namespace, functionName, function, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Updates a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="bodyBody">A JSON value presenting configuration payload of a Pulsar Function. An example of the expected Pulsar Function can be found here.  
        /// - **autoAck**  
        ///   Whether or not the framework acknowledges messages automatically.  
        /// - **runtime**  
        ///   What is the runtime of the Pulsar Function. Possible Values: [JAVA, PYTHON, GO]  
        /// - **resources**  
        ///   The size of the system resources allowed by the Pulsar Function runtime. The resources include: cpu, ram, disk.  
        /// - **className**  
        ///   The class name of a Pulsar Function.  
        /// - **customSchemaInputs**  
        ///   The map of input topics to Schema class names (specified as a JSON object).  
        /// - **customSerdeInputs**  
        ///   The map of input topics to SerDe class names (specified as a JSON object).  
        /// - **deadLetterTopic**  
        ///   Messages that are not processed successfully are sent to `deadLetterTopic`.  
        /// - **runtimeFlags**  
        ///   Any flags that you want to pass to the runtime. Note that in thread mode, these flags have no impact.  
        /// - **fqfn**  
        ///   The Fully Qualified Function Name (FQFN) for the Pulsar Function.  
        /// - **inputSpecs**  
        ///    The map of input topics to its consumer configuration, each configuration has schema of    {"schemaType": "type-x", "serdeClassName": "name-x", "isRegexPattern": true, "receiverQueueSize": 5}  
        /// - **inputs**  
        ///   The input topic or topics (multiple topics can be specified as a comma-separated list) of a Pulsar Function.  
        /// - **jar**  
        ///   Path to the JAR file for the Pulsar Function (if the Pulsar Function is written in Java).   It also supports URL path [http/https/file (file protocol assumes that file   already exists on worker host)] from which worker can download the package.  
        /// - **py**  
        ///   Path to the main Python file or Python wheel file for the Pulsar Function (if the Pulsar Function is written in Python).  
        /// - **go**  
        ///   Path to the main Go executable binary for the Pulsar Function (if the Pulsar Function is written in Go).  
        /// - **logTopic**  
        ///   The topic to which the logs of a Pulsar Function are produced.  
        /// - **maxMessageRetries**  
        ///   How many times should we try to process a message before giving up.  
        /// - **output**  
        ///   The output topic of a Pulsar Function (If none is specified, no output is written).  
        /// - **outputSerdeClassName**  
        ///   The SerDe class to be used for messages output by the Pulsar Function.  
        /// - **parallelism**  
        ///   The parallelism factor of a Pulsar Function (i.e. the number of a Pulsar Function instances to run).  
        /// - **processingGuarantees**  
        ///   The processing guarantees (that is, delivery semantics) applied to the Pulsar Function.  Possible Values: [ATLEAST_ONCE, ATMOST_ONCE, EFFECTIVELY_ONCE]  
        /// - **retainOrdering**  
        ///   Function consumes and processes messages in order.  
        /// - **outputSchemaType**  
        ///    Represents either a builtin schema type (for example: 'avro', 'json', ect) or the class name for a Schema implementation.- **subName**  
        ///   Pulsar source subscription name. User can specify a subscription-name for the input-topic consumer.  
        /// - **windowConfig**  
        ///   The window configuration of a Pulsar Function.  
        /// - **timeoutMs**  
        ///   The message timeout in milliseconds.  
        /// - **topicsPattern**  
        ///   The topic pattern to consume from a list of topics under a namespace that match the pattern.  [input] and [topic-pattern] are mutually exclusive. Add SerDe class name for a   pattern in customSerdeInputs (supported for java fun only)  
        /// - **userConfig**  
        ///   A map of user-defined configurations (specified as a JSON object).  
        /// - **secrets**  
        ///   This is a map of secretName(that is how the secret is going to be accessed in the Pulsar Function via context) to an object that  encapsulates how the secret is fetched by the underlying secrets provider. The type of an value here can be found by the  SecretProviderConfigurator.getSecretObjectType() method. 
        /// - **cleanupSubscription**  
        ///   Whether the subscriptions of a Pulsar Function created or used should be deleted when the Pulsar Function is deleted.</param>
        /// <returns>Pulsar Function successfully updated</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void UpdateFunction(string tenant, string @namespace, string functionName, Stream functionStream)
        {
            UpdateFunctionAsync(tenant, @namespace, functionName, functionStream).Wait();
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Updates a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="bodyBody">A JSON value presenting configuration payload of a Pulsar Function. An example of the expected Pulsar Function can be found here.  
        /// - **autoAck**  
        ///   Whether or not the framework acknowledges messages automatically.  
        /// - **runtime**  
        ///   What is the runtime of the Pulsar Function. Possible Values: [JAVA, PYTHON, GO]  
        /// - **resources**  
        ///   The size of the system resources allowed by the Pulsar Function runtime. The resources include: cpu, ram, disk.  
        /// - **className**  
        ///   The class name of a Pulsar Function.  
        /// - **customSchemaInputs**  
        ///   The map of input topics to Schema class names (specified as a JSON object).  
        /// - **customSerdeInputs**  
        ///   The map of input topics to SerDe class names (specified as a JSON object).  
        /// - **deadLetterTopic**  
        ///   Messages that are not processed successfully are sent to `deadLetterTopic`.  
        /// - **runtimeFlags**  
        ///   Any flags that you want to pass to the runtime. Note that in thread mode, these flags have no impact.  
        /// - **fqfn**  
        ///   The Fully Qualified Function Name (FQFN) for the Pulsar Function.  
        /// - **inputSpecs**  
        ///    The map of input topics to its consumer configuration, each configuration has schema of    {"schemaType": "type-x", "serdeClassName": "name-x", "isRegexPattern": true, "receiverQueueSize": 5}  
        /// - **inputs**  
        ///   The input topic or topics (multiple topics can be specified as a comma-separated list) of a Pulsar Function.  
        /// - **jar**  
        ///   Path to the JAR file for the Pulsar Function (if the Pulsar Function is written in Java).   It also supports URL path [http/https/file (file protocol assumes that file   already exists on worker host)] from which worker can download the package.  
        /// - **py**  
        ///   Path to the main Python file or Python wheel file for the Pulsar Function (if the Pulsar Function is written in Python).  
        /// - **go**  
        ///   Path to the main Go executable binary for the Pulsar Function (if the Pulsar Function is written in Go).  
        /// - **logTopic**  
        ///   The topic to which the logs of a Pulsar Function are produced.  
        /// - **maxMessageRetries**  
        ///   How many times should we try to process a message before giving up.  
        /// - **output**  
        ///   The output topic of a Pulsar Function (If none is specified, no output is written).  
        /// - **outputSerdeClassName**  
        ///   The SerDe class to be used for messages output by the Pulsar Function.  
        /// - **parallelism**  
        ///   The parallelism factor of a Pulsar Function (i.e. the number of a Pulsar Function instances to run).  
        /// - **processingGuarantees**  
        ///   The processing guarantees (that is, delivery semantics) applied to the Pulsar Function.  Possible Values: [ATLEAST_ONCE, ATMOST_ONCE, EFFECTIVELY_ONCE]  
        /// - **retainOrdering**  
        ///   Function consumes and processes messages in order.  
        /// - **outputSchemaType**  
        ///    Represents either a builtin schema type (for example: 'avro', 'json', ect) or the class name for a Schema implementation.- **subName**  
        ///   Pulsar source subscription name. User can specify a subscription-name for the input-topic consumer.  
        /// - **windowConfig**  
        ///   The window configuration of a Pulsar Function.  
        /// - **timeoutMs**  
        ///   The message timeout in milliseconds.  
        /// - **topicsPattern**  
        ///   The topic pattern to consume from a list of topics under a namespace that match the pattern.  [input] and [topic-pattern] are mutually exclusive. Add SerDe class name for a   pattern in customSerdeInputs (supported for java fun only)  
        /// - **userConfig**  
        ///   A map of user-defined configurations (specified as a JSON object).  
        /// - **secrets**  
        ///   This is a map of secretName(that is how the secret is going to be accessed in the Pulsar Function via context) to an object that  encapsulates how the secret is fetched by the underlying secrets provider. The type of an value here can be found by the  SecretProviderConfigurator.getSecretObjectType() method. 
        /// - **cleanupSubscription**  
        ///   Whether the subscriptions of a Pulsar Function created or used should be deleted when the Pulsar Function is deleted.</param>
        /// <returns>Pulsar Function successfully updated</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task UpdateFunctionAsync(string tenant, string @namespace, string functionName, Stream functionStream, CancellationToken cancellationToken = default)
        {
            await _api.UpdateFunctionAsync(tenant, @namespace, functionName, functionStream, cancellationToken).ConfigureAwait(false);
        }

        
        /// <summary>Deletes a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>The Pulsar Function was successfully deleted</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void DeregisterFunction(string tenant, string @namespace, string functionName)
        {
            DeregisterFunctionAsync(tenant, @namespace, functionName).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Deletes a Pulsar Function currently running in cluster mode</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>The Pulsar Function was successfully deleted</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task DeregisterFunctionAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            await _api.DeregisterFunctionAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Restart all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void RestartFunction(string tenant, string @namespace, string functionName)
        {
            RestartFunctionAsync(tenant, @namespace, functionName).Wait();
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Restart all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task RestartFunctionAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            await _api.RestartFunctionAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Start all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void StartFunction(string tenant, string @namespace, string functionName)
        {
            StartFunctionAsync(tenant, @namespace, functionName).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Start all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task StartFunctionAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            await _api.StartFunctionAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>Fetch the current state associated with a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="key">The stats key</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionState GetFunctionState(string tenant, string @namespace, string functionName, string key)
        {
            return GetFunctionStateAsync(tenant, @namespace, functionName, key).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Fetch the current state associated with a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="key">The stats key</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionState> GetFunctionStateAsync(string tenant, string @namespace, string functionName, string key, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionStateAsync(tenant, @namespace, functionName, key, cancellationToken).ConfigureAwait(false);
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Put the state associated with a Pulsar Function</summary>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void PutFunctionState(string tenant, string @namespace, string functionName, string key)
        {
            PutFunctionStateAsync(tenant, @namespace, functionName, key).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Put the state associated with a Pulsar Function</summary>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task PutFunctionStateAsync(string tenant, string @namespace, string functionName, string key, CancellationToken cancellationToken = default)
        {
            await _api.PutFunctionStateAsync(tenant, @namespace, functionName, key, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Displays the stats of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionStatsImpl GetFunctionStats(string tenant, string @namespace, string functionName)
        {
            return GetFunctionStatsAsync(tenant, @namespace, functionName).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Displays the stats of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionStatsImpl> GetFunctionStatsAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionStatsAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Displays the status of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionStatus GetFunctionStatus(string tenant, string @namespace, string functionName)
        {
            return GetFunctionStatusAsync(tenant, @namespace, functionName).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Displays the status of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionStatus> GetFunctionStatusAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionStatusAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Stop all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void StopFunction(string tenant, string @namespace, string functionName)
        {
            StopFunctionAsync(tenant, @namespace, functionName).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Stop all instances of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task StopFunctionAsync(string tenant, string @namespace, string functionName, CancellationToken cancellationToken = default)
        {
            await _api.StopFunctionAsync(tenant, @namespace, functionName, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>Triggers a Pulsar Function with a user-specified value or file data</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="bodyBody">The value with which you want to trigger the Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public Message TriggerFunction(string tenant, string @namespace, string functionName, Stream functionStream)
        {
            return TriggerFunctionAsync(tenant, @namespace, functionName, functionStream).GetAwaiter().GetResult();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Triggers a Pulsar Function with a user-specified value or file data</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="bodyBody">The value with which you want to trigger the Pulsar Function</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<Message> TriggerFunctionAsync(string tenant, string @namespace, string functionName, Stream functionStream, CancellationToken cancellationToken = default)
        {
            return await _api.TriggerFunctionAsync(tenant, @namespace, functionName, functionStream, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Restart an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances are restarted</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void RestartFunction2(string tenant, string @namespace, string functionName, string instanceId)
        {
           RestartFunction2Async(tenant, @namespace, functionName, instanceId).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Restart an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances are restarted</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task RestartFunction2Async(string tenant, string @namespace, string functionName, string instanceId, CancellationToken cancellationToken = default)
        {
            await _api.RestartFunction2Async(tenant, @namespace, functionName, instanceId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Start an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances sre started.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void StartFunction2(string tenant, string @namespace, string functionName, string instanceId)
        {
            StartFunction2Async(tenant, @namespace, functionName, instanceId).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Start an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances sre started.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task StartFunction2Async(string tenant, string @namespace, string functionName, string instanceId, CancellationToken cancellationToken = default)
        {
            await _api.StartFunction2Async(tenant, @namespace, functionName, instanceId, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>Displays the stats of a Pulsar Function instance</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, the stats of all instances is returned</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionInstanceStatsDataImpl GetFunctionInstanceStats(string tenant, string @namespace, string functionName, string instanceId)
        {
            return GetFunctionInstanceStatsAsync(tenant, @namespace, functionName, instanceId).GetAwaiter().GetResult();
        }       
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Displays the stats of a Pulsar Function instance</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, the stats of all instances is returned</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionInstanceStatsDataImpl> GetFunctionInstanceStatsAsync(string tenant, string @namespace, string functionName, string instanceId, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionInstanceStatsAsync(tenant, @namespace, functionName, instanceId, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>Displays the status of a Pulsar Function instance</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, the stats of all instances is returned</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public FunctionInstanceStatusData GetFunctionInstanceStatus(string tenant, string @namespace, string functionName, string instanceId)
        {
            return GetFunctionInstanceStatusAsync(tenant, @namespace, functionName, instanceId).GetAwaiter().GetResult();
        }
        
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Displays the status of a Pulsar Function instance</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, the stats of all instances is returned</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<FunctionInstanceStatusData> GetFunctionInstanceStatusAsync(string tenant, string @namespace, string functionName, string instanceId, CancellationToken cancellationToken = default)
        {
            return await _api.GetFunctionInstanceStatusAsync(tenant, @namespace, functionName, instanceId, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>Stop an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances are stopped.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public void StopFunction2(string tenant, string @namespace, string functionName, string instanceId)
        {
            StopFunction2Async(tenant, @namespace, functionName, instanceId).Wait();
        }
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Stop an instance of a Pulsar Function</summary>
        /// <param name="tenant">The tenant of a Pulsar Function</param>
        /// <param name="@namespace">The namespace of a Pulsar Function</param>
        /// <param name="functionName">The name of a Pulsar Function</param>
        /// <param name="instanceId">The instanceId of a Pulsar Function (if instance-id is not provided, all instances are stopped.</param>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task StopFunction2Async(string tenant, string @namespace, string functionName, string instanceId, CancellationToken cancellationToken = default)
        {
            await _api.StopFunction2Async(tenant, @namespace, functionName, instanceId, cancellationToken).ConfigureAwait(false);
        }

    }
}
