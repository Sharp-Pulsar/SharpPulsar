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
	using UpdateOptions = Org.Apache.Pulsar.Common.Functions.UpdateOptions;
	using ConnectorDefinition = Org.Apache.Pulsar.Common.Io.ConnectorDefinition;
	using SinkStatus = Org.Apache.Pulsar.Common.Policies.Data.SinkStatus;
	using SinkConfig = Org.Apache.Pulsar.Common.Io.SinkConfig;

	/// <summary>
	/// Admin interface for Sink management.
	/// </summary>
	public interface Sinks
	{
		/// <summary>
		/// Get the list of sinks.
		/// <para>
		/// Get the list of all the Pulsar Sinks.
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
//ORIGINAL LINE: java.util.List<String> listSinks(String tenant, String namespace) throws PulsarAdminException;
		IList<string> ListSinks(string Tenant, string Namespace);

		/// <summary>
		/// Get the configuration for the specified sink.
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
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <returns> the sink configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.io.SinkConfig getSink(String tenant, String namespace, String sink) throws PulsarAdminException;
		SinkConfig GetSink(string Tenant, string Namespace, string Sink);

		/// <summary>
		/// Create a new sink.
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws PulsarAdminException;
		void CreateSink(SinkConfig SinkConfig, string FileName);

		/// <summary>
		/// <pre>
		/// Create a new sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws PulsarAdminException;
		void CreateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a sink.
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName) throws PulsarAdminException;
		void UpdateSink(SinkConfig SinkConfig, string FileName);

		/// <summary>
		/// Update the configuration for a sink.
		/// <para>
		/// 
		/// </para>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSink(org.apache.pulsar.common.io.SinkConfig sinkConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateSink(SinkConfig SinkConfig, string FileName, UpdateOptions UpdateOptions);

		/// <summary>
		/// Update the configuration for a sink.
		/// <pre>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl) throws PulsarAdminException;
		void UpdateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a sink.
		/// <pre>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
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
//ORIGINAL LINE: void updateSinkWithUrl(org.apache.pulsar.common.io.SinkConfig sinkConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateSinkWithUrl(SinkConfig SinkConfig, string PkgUrl, UpdateOptions UpdateOptions);

		/// <summary>
		/// Delete an existing sink
		/// <para>
		/// Delete a sink
		/// 
		/// </para>
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
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
//ORIGINAL LINE: void deleteSink(String tenant, String namespace, String sink) throws PulsarAdminException;
		void DeleteSink(string Tenant, string Namespace, string Sink);

		/// <summary>
		/// Gets the current status of a sink.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SinkStatus getSinkStatus(String tenant, String namespace, String sink) throws PulsarAdminException;
		SinkStatus GetSinkStatus(string Tenant, string Namespace, string Sink);

		/// <summary>
		/// Gets the current status of a sink instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="id">
		///            Sink instance-id
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SinkStatus.SinkInstanceStatus.SinkInstanceStatusData getSinkStatus(String tenant, String namespace, String sink, int id) throws PulsarAdminException;
		SinkStatus.SinkInstanceStatus.SinkInstanceStatusData GetSinkStatus(string Tenant, string Namespace, string Sink, int Id);

		/// <summary>
		/// Restart sink instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartSink(String tenant, String namespace, String sink, int instanceId) throws PulsarAdminException;
		void RestartSink(string Tenant, string Namespace, string Sink, int InstanceId);

		/// <summary>
		/// Restart all sink instances
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartSink(String tenant, String namespace, String sink) throws PulsarAdminException;
		void RestartSink(string Tenant, string Namespace, string Sink);


		/// <summary>
		/// Stop sink instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopSink(String tenant, String namespace, String sink, int instanceId) throws PulsarAdminException;
		void StopSink(string Tenant, string Namespace, string Sink, int InstanceId);

		/// <summary>
		/// Stop all sink instances
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopSink(String tenant, String namespace, String sink) throws PulsarAdminException;
		void StopSink(string Tenant, string Namespace, string Sink);

		/// <summary>
		/// Start sink instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startSink(String tenant, String namespace, String sink, int instanceId) throws PulsarAdminException;
		void StartSink(string Tenant, string Namespace, string Sink, int InstanceId);

		/// <summary>
		/// Start all sink instances
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startSink(String tenant, String namespace, String sink) throws PulsarAdminException;
		void StartSink(string Tenant, string Namespace, string Sink);


		/// <summary>
		/// Fetches a list of supported Pulsar IO sinks currently running in cluster mode
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.io.ConnectorDefinition> getBuiltInSinks() throws PulsarAdminException;
		IList<ConnectorDefinition> BuiltInSinks {get;}

		/// <summary>
		/// Reload the available built-in connectors, include Source and Sink
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void reloadBuiltInSinks() throws PulsarAdminException;
		void ReloadBuiltInSinks();
	}

}