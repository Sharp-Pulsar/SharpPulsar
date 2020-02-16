﻿using System.Collections.Generic;

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
	using SourceConfig = Org.Apache.Pulsar.Common.Io.SourceConfig;
	using SourceStatus = Org.Apache.Pulsar.Common.Policies.Data.SourceStatus;

	/// <summary>
	/// Admin interface for Source management.
	/// </summary>
	public interface Sources
	{
		/// <summary>
		/// Get the list of sources.
		/// <para>
		/// Get the list of all the Pulsar Sources.
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
//ORIGINAL LINE: java.util.List<String> listSources(String tenant, String namespace) throws PulsarAdminException;
		IList<string> ListSources(string Tenant, string Namespace);

		/// <summary>
		/// Get the configuration for the specified source.
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.io.SourceConfig getSource(String tenant, String namespace, String source) throws PulsarAdminException;
		SourceConfig GetSource(string Tenant, string Namespace, string Source);

		/// <summary>
		/// Create a new source.
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws PulsarAdminException;
		void CreateSource(SourceConfig SourceConfig, string FileName);

		/// <summary>
		/// <pre>
		/// Create a new source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
		/// </summary>
		/// <param name="sourceConfig">
		///            the source configuration object </param>
		/// <param name="pkgUrl">
		///            url from which pkg can be downloaded </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws PulsarAdminException;
		void CreateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a source.
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName) throws PulsarAdminException;
		void UpdateSource(SourceConfig SourceConfig, string FileName);

		/// <summary>
		/// Update the configuration for a source.
		/// <para>
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSource(org.apache.pulsar.common.io.SourceConfig sourceConfig, String fileName, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateSource(SourceConfig SourceConfig, string FileName, UpdateOptions UpdateOptions);

		/// <summary>
		/// Update the configuration for a source.
		/// <pre>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl) throws PulsarAdminException;
		void UpdateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl);

		/// <summary>
		/// Update the configuration for a source.
		/// <pre>
		/// Update a source by providing url from which fun-pkg can be downloaded. supported url: http/file
		/// eg:
		/// File: file:/dir/fileName.jar
		/// Http: http://www.repo.com/fileName.jar
		/// </pre>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateSourceWithUrl(org.apache.pulsar.common.io.SourceConfig sourceConfig, String pkgUrl, org.apache.pulsar.common.functions.UpdateOptions updateOptions) throws PulsarAdminException;
		void UpdateSourceWithUrl(SourceConfig SourceConfig, string PkgUrl, UpdateOptions UpdateOptions);

		/// <summary>
		/// Delete an existing source
		/// <para>
		/// Delete a source
		/// 
		/// </para>
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteSource(String tenant, String namespace, String source) throws PulsarAdminException;
		void DeleteSource(string Tenant, string Namespace, string Source);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SourceStatus getSourceStatus(String tenant, String namespace, String source) throws PulsarAdminException;
		SourceStatus GetSourceStatus(string Tenant, string Namespace, string Source);

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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.policies.data.SourceStatus.SourceInstanceStatus.SourceInstanceStatusData getSourceStatus(String tenant, String namespace, String source, int id) throws PulsarAdminException;
		SourceStatus.SourceInstanceStatus.SourceInstanceStatusData GetSourceStatus(string Tenant, string Namespace, string Source, int Id);

		/// <summary>
		/// Restart source instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartSource(String tenant, String namespace, String source, int instanceId) throws PulsarAdminException;
		void RestartSource(string Tenant, string Namespace, string Source, int InstanceId);

		/// <summary>
		/// Restart all source instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void restartSource(String tenant, String namespace, String source) throws PulsarAdminException;
		void RestartSource(string Tenant, string Namespace, string Source);


		/// <summary>
		/// Stop source instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopSource(String tenant, String namespace, String source, int instanceId) throws PulsarAdminException;
		void StopSource(string Tenant, string Namespace, string Source, int InstanceId);

		/// <summary>
		/// Stop all source instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stopSource(String tenant, String namespace, String source) throws PulsarAdminException;
		void StopSource(string Tenant, string Namespace, string Source);

		/// <summary>
		/// Start source instance
		/// </summary>
		/// <param name="tenant">
		///            Tenant name </param>
		/// <param name="namespace">
		///            Namespace name </param>
		/// <param name="source">
		///            Source name
		/// </param>
		/// <param name="instanceId">
		///            Source instanceId
		/// </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startSource(String tenant, String namespace, String source, int instanceId) throws PulsarAdminException;
		void StartSource(string Tenant, string Namespace, string Source, int InstanceId);

		/// <summary>
		/// Start all source instances
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void startSource(String tenant, String namespace, String source) throws PulsarAdminException;
		void StartSource(string Tenant, string Namespace, string Source);


		/// <summary>
		/// Fetches a list of supported Pulsar IO sources currently running in cluster mode
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error
		///  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.io.ConnectorDefinition> getBuiltInSources() throws PulsarAdminException;
		IList<ConnectorDefinition> BuiltInSources {get;}


		/// <summary>
		/// Reload the available built-in connectors, include Source and Sink
		/// </summary>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void reloadBuiltInSources() throws PulsarAdminException;
		void ReloadBuiltInSources();
	}

}