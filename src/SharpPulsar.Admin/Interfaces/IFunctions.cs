using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Admin.Interfaces
{

    /// <summary>
    /// Admin interface for function management.
    /// </summary>
    public interface IFunctions
	{
		/// <summary>
		/// Get the list of functions.
		/// <p/>
		/// Get the list of all the Pulsar functions.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["f1", "f2", "f3"]</code>
		/// </pre>
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<string> GetFunctions(string tenant, string @namespace);

		/// <summary>
		/// Get the list of functions asynchronously.
		/// <p/>
		/// Get the list of all the Pulsar functions.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["f1", "f2", "f3"]</code>
		/// </pre>
		/// 
		/// </summary>
		ValueTask<IList<string>> GetFunctionsAsync(string tenant, string @namespace);

		/// <summary>
		/// Get the configuration for the specified function.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <returns> the function configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///             
		FunctionConfig GetFunction(string tenant, string @namespace, string function);

		/// <summary>
		/// Get the configuration for the specified function asynchronously.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <returns> the function configuration
		///  </returns>
		ValueTask<FunctionConfig> GetFunctionAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Create a new function.
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void CreateFunction(FunctionConfig functionConfig, string fileName);

		/// <summary>
		/// Create a new function asynchronously.
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		ValueTask CreateFunctionAsync(FunctionConfig functionConfig, string fileName);

		/// <summary>
		/// Create a new function with package url.
		/// <p/>
		/// Create a new function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void CreateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl);

		/// <summary>
		/// Create a new function with package url asynchronously.
		/// <p/>
		/// Create a new function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask CreateFunctionWithUrlAsync(FunctionConfig functionConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a function.
		/// <p/>
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateFunction(FunctionConfig functionConfig, string fileName);

		/// <summary>
		/// Update the configuration for a function asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		ValueTask UpdateFunctionAsync(FunctionConfig functionConfig, string fileName);

		/// <summary>
		/// Update the configuration for a function.
		/// <p/>
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateFunction(FunctionConfig functionConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a function asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateFunctionAsync(FunctionConfig functionConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a function.
		/// <p/>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void UpdateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a function asynchronously.
		/// <p/>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask UpdateFunctionWithUrlAsync(FunctionConfig functionConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a function.
		/// <p/>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void UpdateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a function asynchronously.
		/// <p/>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateFunctionWithUrlAsync(FunctionConfig functionConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Delete an existing function.
		/// <p/>
		/// Delete a function
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Cluster does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///   

        void DeleteFunction(string tenant, string @namespace, string function);

		/// <summary>
		/// Delete an existing function asynchronously.
		/// <p/>
		/// Delete a function
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask DeleteFunctionAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Gets the current status of a function.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///   

        FunctionStatus GetFunctionStatus(string tenant, string @namespace, string function);

		/// <summary>
		/// Gets the current status of a function asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask<FunctionStatus> GetFunctionStatusAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Gets the current status of a function instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="id">
		///            Function instance-id
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 

        FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData GetFunctionStatus(string tenant, string @namespace, string function, int id);

		/// <summary>
		/// Gets the current status of a function instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="id">
		///            Function instance-id
		/// @return </param>
		ValueTask<FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData> GetFunctionStatusAsync(string tenant, string @namespace, string function, int id);

		/// <summary>
		/// Gets the current stats of a function instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="id">
		///            Function instance-id
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 

        FunctionInstanceStatsData GetFunctionStats(string tenant, string @namespace, string function, int id);

		/// <summary>
		/// Gets the current stats of a function instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="id">
		///            Function instance-id
		/// @return </param>
		ValueTask<FunctionInstanceStatsData> GetFunctionStatsAsync(string tenant, string @namespace, string function, int id);

		/// <summary>
		/// Gets the current stats of a function.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>


        FunctionStats GetFunctionStats(string tenant, string @namespace, string function);

		/// <summary>
		/// Gets the current stats of a function asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// @return </param>

		ValueTask<FunctionStats> GetFunctionStatsAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Restart function instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///  

        void RestartFunction(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Restart function instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId </param>
		ValueTask RestartFunctionAsync(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Restart all function instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void RestartFunction(string tenant, string @namespace, string function);

		/// <summary>
		/// Restart all function instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask RestartFunctionAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Stop function instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void StopFunction(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Stop function instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId </param>
		ValueTask StopFunctionAsync(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Start all function instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///   

        void StartFunction(string tenant, string @namespace, string function);

		/// <summary>
		/// Start all function instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask StartFunctionAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Start function instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void StartFunction(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Start function instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <param name="instanceId">
		///            Function instanceId </param>
		ValueTask StartFunctionAsync(string tenant, string @namespace, string function, int instanceId);

		/// <summary>
		/// Stop all function instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void StopFunction(string tenant, string @namespace, string function);

		/// <summary>
		/// Stop all function instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask StopFunctionAsync(string tenant, string @namespace, string function);

		/// <summary>
		/// Triggers the function by writing to the input topic.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="triggerValue">
		///            The input that will be written to input topic </param>
		/// <param name="triggerFile">
		///            The file which contains the input that will be written to input topic
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///

        string TriggerFunction(string tenant, string @namespace, string function, string topic, string triggerValue, string triggerFile);

		/// <summary>
		/// Triggers the function by writing to the input topic asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="triggerValue">
		///            The input that will be written to input topic </param>
		/// <param name="triggerFile">
		///            The file which contains the input that will be written to input topic </param>
		ValueTask<string> TriggerFunctionAsync(string tenant, string @namespace, string function, string topic, string triggerValue, string triggerFile);

		/// <summary>
		/// Upload Data.
		/// </summary>
		/// <param name="sourceFile">
		///            dataFile that needs to be uploaded </param>
		/// <param name="path">
		///            Path where data should be stored
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///  

        void UploadFunction(string sourceFile, string path);

		/// <summary>
		/// Upload Data asynchronously.
		/// </summary>
		/// <param name="sourceFile">
		///            dataFile that needs to be uploaded </param>
		/// <param name="path">
		///            Path where data should be stored </param>
		ValueTask UploadFunctionAsync(string sourceFile, string path);

		/// <summary>
		/// Download Function Code.
		/// </summary>
		/// <param name="destinationFile">
		///            file where data should be downloaded to </param>
		/// <param name="path">
		///            Path where data is located
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///      

        void DownloadFunction(string destinationFile, string path);

		/// <summary>
		/// Download Function Code.
		/// </summary>
		/// <param name="destinationFile">
		///            file where data should be downloaded to </param>
		/// <param name="path">
		///            Path where data is located </param>
		ValueTask DownloadFunctionAsync(string destinationFile, string path);

		/// <summary>
		/// Download Function Code.
		/// </summary>
		/// <param name="destinationFile">
		///           file where data should be downloaded to </param>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 

        void DownloadFunction(string destinationFile, string tenant, string @namespace, string function);

		/// <summary>
		/// Download Function Code asynchronously.
		/// </summary>
		/// <param name="destinationFile">
		///           file where data should be downloaded to </param>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		ValueTask DownloadFunctionAsync(string destinationFile, string tenant, string @namespace, string function);

		/// <summary>
		/// Download Function Code.
		/// </summary>
		/// <param name="destinationFile">
		///           file where data should be downloaded to </param>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="transformFunction">
		///            Whether to download the transform function (for sources and sinks) </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 

        void DownloadFunction(string destinationFile, string tenant, string @namespace, string function, bool transformFunction);

		/// <summary>
		/// Download Function Code asynchronously.
		/// </summary>
		/// <param name="destinationFile">
		///           file where data should be downloaded to </param>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="transformFunction">
		///            Whether to download the transform function (for sources and sinks) </param>
		ValueTask DownloadFunctionAsync(string destinationFile, string tenant, string @namespace, string function, bool transformFunction);




		/// <summary>
		/// Fetches a list of supported Pulsar Functions currently running in cluster mode.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///

        IList<FunctionDefinition> BuiltInFunctions {get;}

		/// <summary>
		/// Fetches a list of supported Pulsar Functions currently running in cluster mode asynchronously.
		/// </summary>
		ValueTask<IList<FunctionDefinition>> BuiltInFunctionsAsync {get;}

		/// <summary>
		/// Fetch the current state associated with a Pulsar Function.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="key">
		///            Key name of State
		/// </param>
		/// <returns> the function configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        FunctionState GetFunctionState(string tenant, string @namespace, string function, string key);

		/// <summary>
		/// Fetch the current state associated with a Pulsar Function asynchronously.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="key">
		///            Key name of State
		/// </param>
		/// <returns> the function configuration </returns>
		ValueTask<FunctionState> GetFunctionStateAsync(string tenant, string @namespace, string function, string key);

		/// <summary>
		/// Puts the given state associated with a Pulsar Function.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="state">
		///            FunctionState
		/// ** </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        ///  

        void PutFunctionState(string tenant, string @namespace, string function, FunctionState state);

		/// <summary>
		/// Puts the given state associated with a Pulsar Function asynchronously.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="function">
		///            Function name </param>
		/// <param name="state">
		///            FunctionState </param>
		ValueTask PutFunctionStateAsync(string tenant, string @namespace, string function, FunctionState state);

		/// <summary>
		/// Reload the available built-in functions.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
        /// 

        void ReloadBuiltInFunctions();

		/// <summary>
		/// Reload the available built-in functions.
		/// </summary>
		ValueTask ReloadBuiltInFunctionsAsync();
	}

}