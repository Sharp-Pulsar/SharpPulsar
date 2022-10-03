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
	/// Admin interface for Sink management.
	/// </summary>
	public interface ISinks
	{
		/// <summary>
		/// Get the list of sinks.
		/// <p/>
		/// Get the list of all the Pulsar Sinks.
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

		IList<string> ListSinks(string tenant, string @namespace);

		/// <summary>
		/// Get the list of sinks asynchronously.
		/// <p/>
		/// Get the list of all the Pulsar Sinks.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["f1", "f2", "f3"]</code>
		/// </pre>
		/// </summary>
		ValueTask<IList<string>> ListSinksAsync(string tenant, string @namespace);

		/// <summary>
		/// Get the configuration for the specified sink.
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


		SinkConfig GetSink(string tenant, string @namespace, string sink);

		/// <summary>
		/// Get the configuration for the specified sink asynchronously.
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
		/// <param name="sink">
		///            Sink name
		/// </param>
		/// <returns> the sink configuration </returns>
		ValueTask<SinkConfig> GetSinkAsync(string tenant, string @namespace, string sink);

		/// <summary>
		/// Create a new sink.
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
    	void CreateSink(SinkConfig sinkConfig, string fileName);

		/// <summary>
		/// Create a new sink asynchronously.
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		ValueTask CreateSinkAsync(SinkConfig sinkConfig, string fileName);

		/// <summary>
		/// Create a new sink with package url.
		/// <p/>
		/// Create a new sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
    	void CreateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl);

		/// <summary>
		/// Create a new sink with package url asynchronously.
		/// <p/>
		/// Create a new sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask CreateSinkWithUrlAsync(SinkConfig sinkConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a sink.
		/// <p/>
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

	    void UpdateSink(SinkConfig sinkConfig, string fileName);

		/// <summary>
		/// Update the configuration for a sink asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		ValueTask UpdateSinkAsync(SinkConfig sinkConfig, string fileName);

		/// <summary>
		/// Update the configuration for a sink.
		/// <p/>
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

        void UpdateSink(SinkConfig sinkConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a sink asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateSinkAsync(SinkConfig sinkConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a sink.
		/// <p/>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
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
    	void UpdateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a sink asynchronously.
		/// <p/>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask UpdateSinkWithUrlAsync(SinkConfig sinkConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a sink.
		/// <p/>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
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

    	void UpdateSinkWithUrl(SinkConfig sinkConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a sink asynchronously.
		/// <p/>
		/// Update a sink by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sinkConfig">
		///            the sink configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateSinkWithUrlAsync(SinkConfig sinkConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Delete an existing sink.
		/// <p/>
		/// Delete a sink
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
		void DeleteSink(string tenant, string @namespace, string sink);

		/// <summary>
		/// Delete an existing sink asynchronously.
		/// <p/>
		/// Delete a sink
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		ValueTask DeleteSinkAsync(string tenant, string @namespace, string sink);

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
		SinkStatus GetSinkStatus(string tenant, string @namespace, string sink);

		/// <summary>
		/// Gets the current status of a sink asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		ValueTask<SinkStatus> GetSinkStatusAsync(string tenant, string @namespace, string sink);

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
		SinkStatus.SinkInstanceStatus.SinkInstanceStatusData GetSinkStatus(string tenant, string @namespace, string sink, int id);

		/// <summary>
		/// Gets the current status of a sink instance asynchronously.
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
		ValueTask<SinkStatus.SinkInstanceStatus.SinkInstanceStatusData> GetSinkStatusAsync(string tenant, string @namespace, string sink, int id);

		/// <summary>
		/// Restart sink instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void RestartSink(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Restart sink instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId </param>
		ValueTask RestartSinkAsync(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Restart all sink instances.
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
		void RestartSink(string tenant, string @namespace, string sink);

		/// <summary>
		/// Restart all sink instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		ValueTask RestartSinkAsync(string tenant, string @namespace, string sink);

		/// <summary>
		/// Stop sink instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StopSink(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Stop sink instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId </param>
		ValueTask StopSinkAsync(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Stop all sink instances.
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
		void StopSink(string tenant, string @namespace, string sink);

		/// <summary>
		/// Stop all sink instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		ValueTask StopSinkAsync(string tenant, string @namespace, string sink);

		/// <summary>
		/// Start sink instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StartSink(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Start sink instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		/// <param name="instanceId">
		///            Sink instanceId </param>
		ValueTask StartSinkAsync(string tenant, string @namespace, string sink, int instanceId);

		/// <summary>
		/// Start all sink instances.
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
		void StartSink(string tenant, string @namespace, string sink);

		/// <summary>
		/// Start all sink instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="sink">
		///            Sink name </param>
		ValueTask StartSinkAsync(string tenant, string @namespace, string sink);

		/// <summary>
		/// Fetches a list of supported Pulsar IO sinks currently running in cluster mode.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<ConnectorDefinition> BuiltInSinks {get;}

		/// <summary>
		/// Fetches a list of supported Pulsar IO sinks currently running in cluster mode asynchronously.
		/// </summary>
		ValueTask<IList<ConnectorDefinition>> BuiltInSinksAsync {get;}

		/// <summary>
		/// Reload the available built-in connectors, include Source and Sink.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void ReloadBuiltInSinks();

		/// <summary>
		/// Reload the available built-in connectors, include Source and Sink asynchronously.
		/// </summary>
		ValueTask ReloadBuiltInSinksAsync();
	}

}