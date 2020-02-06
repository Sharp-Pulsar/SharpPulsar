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
namespace org.apache.pulsar.client.admin
{

	using NotAuthorizedException = PulsarAdminException.NotAuthorizedException;
	using NotFoundException = PulsarAdminException.NotFoundException;
	using PreconditionFailedException = PulsarAdminException.PreconditionFailedException;
	using FunctionState = pulsar.common.functions.FunctionState;
	using UpdateOptions = pulsar.common.functions.UpdateOptions;
	using ConnectorDefinition = pulsar.common.io.ConnectorDefinition;
	using FunctionStats = pulsar.common.policies.data.FunctionStats;
	using FunctionConfig = pulsar.common.functions.FunctionConfig;
	using FunctionStatus = pulsar.common.policies.data.FunctionStatus;

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
		IList<string> getFunctions(string tenant, string @namespace);

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
		FunctionConfig getFunction(string tenant, string @namespace, string function);

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
		void createFunction(FunctionConfig functionConfig, string fileName);

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
		void createFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl);

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
		void updateFunction(FunctionConfig functionConfig, string fileName);

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
		void updateFunction(FunctionConfig functionConfig, string fileName, UpdateOptions updateOptions);

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
		void updateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl);

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
		void updateFunctionWithUrl(FunctionConfig functionConfig, string pkgUrl, UpdateOptions updateOptions);


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
		void deleteFunction(string tenant, string @namespace, string function);

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
		FunctionStatus getFunctionStatus(string tenant, string @namespace, string function);

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
		FunctionStatus.FunctionInstanceStatus.FunctionInstanceStatusData getFunctionStatus(string tenant, string @namespace, string function, int id);

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
		FunctionStats.FunctionInstanceStats.FunctionInstanceStatsData getFunctionStats(string tenant, string @namespace, string function, int id);

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
		FunctionStats getFunctionStats(string tenant, string @namespace, string function);

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
		void restartFunction(string tenant, string @namespace, string function, int instanceId);

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
		void restartFunction(string tenant, string @namespace, string function);


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
		void stopFunction(string tenant, string @namespace, string function, int instanceId);

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
		void startFunction(string tenant, string @namespace, string function);

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
		void startFunction(string tenant, string @namespace, string function, int instanceId);

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
		void stopFunction(string tenant, string @namespace, string function);


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
		string triggerFunction(string tenant, string @namespace, string function, string topic, string triggerValue, string triggerFile);

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
		void uploadFunction(string sourceFile, string path);

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
		void downloadFunction(string destinationFile, string path);

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
		void downloadFunction(string destinationFile, string tenant, string @namespace, string function);

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
		FunctionState getFunctionState(string tenant, string @namespace, string function, string key);

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
		void putFunctionState(string tenant, string @namespace, string function, FunctionState state);
	}

}