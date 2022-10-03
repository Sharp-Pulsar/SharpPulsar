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
	/// Admin interface for Source management.
	/// </summary>
	public interface ISources
	{
		/// <summary>
		/// Get the list of sources.
		/// <p/>
		/// Get the list of all the Pulsar Sources.
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
		IList<string> ListSources(string tenant, string @namespace);

		/// <summary>
		/// Get the list of sources asynchronously.
		/// <p/>
		/// Get the list of all the Pulsar Sources.
		/// <p/>
		/// Response Example:
		/// 
		/// <pre>
		/// <code>["f1", "f2", "f3"]</code>
		/// </pre>
		/// </summary>
		ValueTask<IList<string>> ListSourcesAsync(string tenant, string @namespace);

		/// <summary>
		/// Get the configuration for the specified source.
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
		/// <param name="source">
		///            Source name
		/// </param>
		/// <returns> the source configuration
		/// </returns>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to get the configuration of the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		SourceConfig GetSource(string tenant, string @namespace, string source);

		/// <summary>
		/// Get the configuration for the specified source asynchronously.
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
		/// <param name="source">
		///            Source name
		/// </param>
		/// <returns> the source configuration </returns>
		ValueTask<SourceConfig> GetSourceAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Create a new source.
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void CreateSource(SourceConfig sourceConfig, string fileName);

		/// <summary>
		/// Create a new source asynchronously.
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		ValueTask CreateSourceAsync(SourceConfig sourceConfig, string fileName);

		/// <summary>
		/// Create a new source with package url.
		/// <p/>
		/// Create a new source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void CreateSourceWithUrl(SourceConfig sourceConfig, string pkgUrl);

		/// <summary>
		/// Create a new source with package url asynchronously.
		/// <p/>
		/// Create a new source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask CreateSourceWithUrlAsync(SourceConfig sourceConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a source.
		/// <p/>
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateSource(SourceConfig sourceConfig, string fileName);

		/// <summary>
		/// Update the configuration for a source asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		ValueTask UpdateSourceAsync(SourceConfig sourceConfig, string fileName);

		/// <summary>
		/// Update the configuration for a source.
		/// <p/>
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateSource(SourceConfig sourceConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a source asynchronously.
		/// <p/>
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateSourceAsync(SourceConfig sourceConfig, string fileName, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a source.
		/// <p/>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission to create the cluster </exception>
		/// <exception cref="NotFoundException">
		///             Cluster doesn't exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void UpdateSourceWithUrl(SourceConfig sourceConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a source asynchronously.
		/// <p/>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		ValueTask UpdateSourceWithUrlAsync(SourceConfig sourceConfig, string pkgUrl);

		/// <summary>
		/// Update the configuration for a source.
		/// <p/>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
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
		void UpdateSourceWithUrl(SourceConfig sourceConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Update the configuration for a source asynchronously.
		/// <p/>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <param name="updateOptions">
		///            options for the update operations </param>
		ValueTask UpdateSourceWithUrlAsync(SourceConfig sourceConfig, string pkgUrl, UpdateOptions updateOptions);

		/// <summary>
		/// Delete an existing source.
		/// <p/>
		/// Delete a source
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             You don't have admin permission </exception>
		/// <exception cref="NotFoundException">
		///             Cluster does not exist </exception>
		/// <exception cref="PreconditionFailedException">
		///             Cluster is not empty </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void DeleteSource(string tenant, string @namespace, string source);

		/// <summary>
		/// Delete an existing source asynchronously.
		/// <p/>
		/// Delete a source
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		ValueTask DeleteSourceAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Gets the current status of a source.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		SourceStatus GetSourceStatus(string tenant, string @namespace, string source);

		/// <summary>
		/// Gets the current status of a source asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		ValueTask<SourceStatus> GetSourceStatusAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Gets the current status of a source instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="id">
		///            Source instance-id
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		SourceStatus.SourceInstanceStatus.SourceInstanceStatusData GetSourceStatus(string tenant, string @namespace, string source, int id);

		/// <summary>
		/// Gets the current status of a source instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="id">
		///            Source instance-id
		/// @return </param>
		ValueTask<SourceStatus.SourceInstanceStatus.SourceInstanceStatusData> GetSourceStatusAsync(string tenant, string @namespace, string source, int id);

		/// <summary>
		/// Restart source instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void RestartSource(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Restart source instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId </param>
		ValueTask RestartSourceAsync(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Restart all source instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void RestartSource(string tenant, string @namespace, string source);

		/// <summary>
		/// Restart all source instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		ValueTask RestartSourceAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Stop source instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StopSource(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Stop source instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId </param>
		ValueTask StopSourceAsync(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Stop all source instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StopSource(string tenant, string @namespace, string source);

		/// <summary>
		/// Stop all source instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		ValueTask StopSourceAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Start source instance.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StartSource(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Start source instance asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		/// <param name="instanceId">
		///            Source instanceId </param>
		ValueTask StartSourceAsync(string tenant, string @namespace, string source, int instanceId);

		/// <summary>
		/// Start all source instances.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void StartSource(string tenant, string @namespace, string source);

		/// <summary>
		/// Start all source instances asynchronously.
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name </param>
		ValueTask StartSourceAsync(string tenant, string @namespace, string source);

		/// <summary>
		/// Fetches a list of supported Pulsar IO sources currently running in cluster mode.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		IList<ConnectorDefinition> BuiltInSources {get;}

		/// <summary>
		/// Fetches a list of supported Pulsar IO sources currently running in cluster mode asynchronously.
		/// </summary>
		ValueTask<IList<ConnectorDefinition>> BuiltInSourcesAsync {get;}

		/// <summary>
		/// Reload the available built-in connectors, include Source and Source.
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
		void ReloadBuiltInSources();

		/// <summary>
		/// Reload the available built-in connectors, include Source and Source asynchronously.
		/// </summary>
		ValueTask ReloadBuiltInSourcesAsync();
	}

}