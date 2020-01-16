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
	using NotAuthorizedException = org.apache.pulsar.client.admin.PulsarAdminException.NotAuthorizedException;
	using NotFoundException = org.apache.pulsar.client.admin.PulsarAdminException.NotFoundException;
	using PreconditionFailedException = org.apache.pulsar.client.admin.PulsarAdminException.PreconditionFailedException;
	using UpdateOptions = org.apache.pulsar.common.functions.UpdateOptions;
	using ConnectorDefinition = org.apache.pulsar.common.io.ConnectorDefinition;
	using SinkStatus = org.apache.pulsar.common.policies.data.SinkStatus;
	using SinkConfig = org.apache.pulsar.common.io.SinkConfig;

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
		IList<string> listSinks(string tenant, string @namespace);

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
		SinkConfig getSink(string tenant, string @namespace, string sink);

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
		void createSink(SinkConfig sinkConfig, string fileName);

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
		void createSinkWithUrl(SinkConfig sinkConfig, string pkgUrl);

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
		void updateSink(SinkConfig sinkConfig, string fileName);

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
		void updateSink(SinkConfig sinkConfig, string fileName, UpdateOptions updateOptions);

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
		void updateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl);

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
		void updateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl, UpdateOptions updateOptions);

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
		void deleteSink(string tenant, string @namespace, string sink);

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
		SinkStatus getSinkStatus(string tenant, string @namespace, string sink);

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
		SinkStatus.SinkInstanceStatus.SinkInstanceStatusData getSinkStatus(string tenant, string @namespace, string sink, int id);

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
		void restartSink(string tenant, string @namespace, string sink, int instanceId);

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
		void restartSink(string tenant, string @namespace, string sink);


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
		void stopSink(string tenant, string @namespace, string sink, int instanceId);

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
		void stopSink(string tenant, string @namespace, string sink);

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
		void startSink(string tenant, string @namespace, string sink, int instanceId);

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
		void startSink(string tenant, string @namespace, string sink);


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
		void reloadBuiltInSinks();
	}

}