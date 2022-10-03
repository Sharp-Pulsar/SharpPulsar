using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Schemas;

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
	/// Admin interface on interacting with schemas.
	/// </summary>
	public interface ISchemas
	{

		/// <summary>
		/// Retrieve the latest schema of a topic.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema </returns>
		/// <exception cref="PulsarAdminException"> </exception>
		ISchemaInfo GetSchemaInfo(string topic);

		/// <summary>
		/// Retrieve the latest schema of a topic asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema </returns>
		ValueTask<ISchemaInfo> GetSchemaInfoAsync(string topic);

		/// <summary>
		/// Retrieve the latest schema with verison of a topic.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema with version </returns>
		/// <exception cref="PulsarAdminException"> </exception>

		SchemaInfoWithVersion GetSchemaInfoWithVersion(string topic);

		/// <summary>
		/// Retrieve the latest schema with verison of a topic asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema with version </returns>
		ValueTask<SchemaInfoWithVersion> GetSchemaInfoWithVersionAsync(string topic);

		/// <summary>
		/// Retrieve the schema of a topic at a given <tt>version</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="version"> schema version </param>
		/// <returns> the schema info at a given <tt>version</tt> </returns>
		/// <exception cref="PulsarAdminException"> </exception>
		ISchemaInfo GetSchemaInfo(string topic, long version);

		/// <summary>
		/// Retrieve the schema of a topic at a given <tt>version</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="version"> schema version </param>
		/// <returns> the schema info at a given <tt>version</tt> </returns>
		ValueTask<ISchemaInfo> GetSchemaInfoAsync(string topic, long version);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void DeleteSchema(string topic);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		ValueTask DeleteSchemaAsync(string topic);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="force"> whether to delete schema completely.
		///              If true, delete all resources (including metastore and ledger),
		///              otherwise only do a mark deletion and not remove any resources indeed </param>
		/// <exception cref="PulsarAdminException"> </exception>
		void DeleteSchema(string topic, bool force);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		ValueTask DeleteSchemaAsync(string topic, bool force);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt> with the provided schema info.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified fomrat </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 
		void CreateSchema(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt> with the provided schema info asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified fomrat </param>
		/// <param name="schemaInfo"> schema info </param>
		ValueTask CreateSchemaAsync(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 
		void CreateSchema(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		ValueTask CreateSchemaAsync(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IsCompatibilityResponse TestCompatibility(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		ValueTask<IsCompatibilityResponse> TestCompatibilityAsync(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
		long? GetVersionBySchema(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Find schema version <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		ValueTask<long> GetVersionBySchemaAsync(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IsCompatibilityResponse TestCompatibility(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		ValueTask<IsCompatibilityResponse> TestCompatibilityAsync(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
        /// 
		long? GetVersionBySchema(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Find schema version <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		ValueTask<long> GetVersionBySchemaAsync(string topic, ISchemaInfo schemaInfo);

		/// <summary>
		/// Get all version schemas <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IList<SchemaInfo> GetAllSchemas(string topic);

		/// <summary>
		/// Get all version schemas <tt>topic</tt> asynchronously.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		ValueTask<IList<ISchemaInfo>> GetAllSchemasAsync(string topic);
	}

}