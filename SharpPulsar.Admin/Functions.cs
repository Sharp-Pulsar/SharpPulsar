using System;
using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Admin
{

	using NotAuthorizedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.NotFoundException;
	using PreconditionFailedException = Org.Apache.Pulsar.Client.Admin.PulsarAdminException.PreconditionFailedException;
	using FunctionState = Org.Apache.Pulsar.Common.Functions.FunctionState;
	using UpdateOptions = Org.Apache.Pulsar.Common.Functions.UpdateOptions;
	using ConnectorDefinition = Org.Apache.Pulsar.Common.Io.ConnectorDefinition;
	using FunctionStats = Org.Apache.Pulsar.Common.Policies.Data.FunctionStats;
	using FunctionConfig = Org.Apache.Pulsar.Common.Functions.FunctionConfig;
	using FunctionStatus = Org.Apache.Pulsar.Common.Policies.Data.FunctionStatus;

	/// <summary>
	/// Admin interface for function management.
	/// </summary>
	public interface Functions
	{
		/// <summary>
		/// Get the list of functions.
		/// <para>
		/// Get the list of all the Pulsar functions.
		/// </para>
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["f1", "f2", "f3"]</code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<String> getFunctions(String tenant, String namespace) throws PulsarAdminException;
		IList<string> GetFunctions(string Tenant, string Namespace);

		/// <summary>
		/// Get the configuration for the specified function.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ serviceUrl : "http://my-broker.example.com:8080/" }</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.functions.FunctionConfig getFunction(String tenant, String namespace, String function) throws PulsarAdminException;
		FunctionConfig GetFunction(string Tenant, string Namespace, string Function);

		/// <summary>
		/// Create a new function.
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws PulsarAdminException;
		void CreateFunction(FunctionConfig FunctionConfig, string FileName);

		/// <summary>
		/// <pre>
		/// Create a new function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
		/// </summary>
		/// <param name="functionConfig">
		///            the function configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws PulsarAdminException;
		void CreateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a function.
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName) throws PulsarAdminException;
		void UpdateFunction(FunctionConfig FunctionConfig, string FileName);

		/// <summary>
		/// Update the configuration for a function.
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFunction(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateFunction(FunctionConfig FunctionConfig, string FileName, UpdateOptions UpdateOptions);

		/// <summary>
		/// Update the configuration for a function.
		/// <pre>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl) throws PulsarAdminException;
		void UpdateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a function.
		/// <pre>
		/// Update a function by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateFunctionWithUrl(org.apache.pulsar.common.functions.FunctionConfig functionConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateFunctionWithUrl(FunctionConfig FunctionConfig, string PkgUrl, UpdateOptions UpdateOptions);


		/// <summary>
		/// Delete an existing function
		/// <para>
		/// Delete a function
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteFunction(String tenant, String namespace, String function) throws PulsarAdminException;
		void DeleteFunction(string Tenant, string Namespace, string Function);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FunctionStatus getFunctionStatus(String tenant, String namespace, String function) throws PulsarAdminException;
		FunctionStatus GetFunctionStatus(string Tenant, string Namespace, string Function);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData getFunctionStatus(String tenant, String namespace, String function, int id) throws PulsarAdminException;
		FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData GetFunctionStatus(string Tenant, string Namespace, string Function, int Id);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData getFunctionStats(String tenant, String namespace, String function, int id) throws PulsarAdminException;
		FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData GetFunctionStats(string Tenant, string Namespace, string Function, int Id);

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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.FunctionStats getFunctionStats(String tenant, String namespace, String function) throws PulsarAdminException;
		FunctionStats GetFunctionStats(string Tenant, string Namespace, string Function);

		/// <summary>
		/// Restart function instance
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartFunction(String tenant, String namespace, String function, int instanceId) throws PulsarAdminException;
		void RestartFunction(string Tenant, string Namespace, string Function, int InstanceId);

		/// <summary>
		/// Restart all function instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartFunction(String tenant, String namespace, String function) throws PulsarAdminException;
		void RestartFunction(string Tenant, string Namespace, string Function);


		/// <summary>
		/// Stop function instance
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopFunction(String tenant, String namespace, String function, int instanceId) throws PulsarAdminException;
		void StopFunction(string Tenant, string Namespace, string Function, int InstanceId);

		/// <summary>
		/// Start all function instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startFunction(String tenant, String namespace, String function) throws PulsarAdminException;
		void StartFunction(string Tenant, string Namespace, string Function);

		/// <summary>
		/// Start function instance
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startFunction(String tenant, String namespace, String function, int instanceId) throws PulsarAdminException;
		void StartFunction(string Tenant, string Namespace, string Function, int InstanceId);

		/// <summary>
		/// Stop all function instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopFunction(String tenant, String namespace, String function) throws PulsarAdminException;
		void StopFunction(string Tenant, string Namespace, string Function);


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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String triggerFunction(String tenant, String namespace, String function, String topic, String triggerValue, String triggerFile) throws PulsarAdminException;
		string TriggerFunction(string Tenant, string Namespace, string Function, string Topic, string TriggerValue, string TriggerFile);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void uploadFunction(String sourceFile, String path) throws PulsarAdminException;
		void UploadFunction(string SourceFile, string Path);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void downloadFunction(String destinationFile, String path) throws PulsarAdminException;
		void DownloadFunction(string DestinationFile, string Path);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void downloadFunction(String destinationFile, String tenant, String namespace, String function) throws PulsarAdminException;
		void DownloadFunction(string DestinationFile, string Tenant, string Namespace, string Function);

		/// <summary>
		/// Deprecated in favor of getting sources and sinks for their own APIs
		/// 
		/// Fetches a list of supported Pulsar IO connectors currently running in cluster mode
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated List<org.apache.pulsar.common.io.ConnectorDefinition> getConnectorsList() throws PulsarAdminException;
		[Obsolete]
		IList<ConnectorDefinition> ConnectorsList {get;}

		/// <summary>
		/// Deprecated in favor of getting sources and sinks for their own APIs
		/// 
		/// Fetches a list of supported Pulsar IO sources currently running in cluster mode
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated Set<String> getSources() throws PulsarAdminException;
		[Obsolete]
		ISet<string> Sources {get;}

		/// <summary>
		/// Deprecated in favor of getting sources and sinks for their own APIs
		/// 
		/// Fetches a list of supported Pulsar IO sinks currently running in cluster mode
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Deprecated Set<String> getSinks() throws PulsarAdminException;
		[Obsolete]
		ISet<string> Sinks {get;}

		/// <summary>
		/// Fetch the current state associated with a Pulsar Function.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.functions.FunctionState getFunctionState(String tenant, String namespace, String function, String key) throws PulsarAdminException;
		FunctionState GetFunctionState(string Tenant, string Namespace, string Function, string Key);

		/// <summary>
		/// Puts the given state associated with a Pulsar Function.
		/// <para>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>{ "value : 12, version : 2"}</code>
		/// </pre>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void putFunctionState(String tenant, String namespace, String function, org.apache.pulsar.common.functions.FunctionState state) throws PulsarAdminException;
		void PutFunctionState(string Tenant, string Namespace, string Function, FunctionState State);
	}

}