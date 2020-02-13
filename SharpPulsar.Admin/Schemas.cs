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
	using GetAllVersionsSchemaResponse = Org.Apache.Pulsar.Common.Protocol.Schema.GetAllVersionsSchemaResponse;
	using IsCompatibilityResponse = Org.Apache.Pulsar.Common.Protocol.Schema.IsCompatibilityResponse;
	using PostSchemaPayload = Org.Apache.Pulsar.Common.Protocol.Schema.PostSchemaPayload;
	using PostSchemaResponse = Org.Apache.Pulsar.Common.Protocol.Schema.PostSchemaResponse;
	using SchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.SchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaInfoWithVersion = Org.Apache.Pulsar.Common.Schema.SchemaInfoWithVersion;

	/// <summary>
	/// Admin interface on interacting with schemas.
	/// </summary>
	public interface Schemas
	{

		/// <summary>
		/// Retrieve the latest schema of a topic.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic) throws PulsarAdminException;
		SchemaInfo GetSchemaInfo(string Topic);

		/// <summary>
		/// Retrieve the latest schema with verison of a topic.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema with version </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.schema.SchemaInfoWithVersion getSchemaInfoWithVersion(String topic) throws PulsarAdminException;
		SchemaInfoWithVersion GetSchemaInfoWithVersion(string Topic);

		/// <summary>
		/// Retrieve the schema of a topic at a given <tt>version</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="version"> schema version </param>
		/// <returns> the schema info at a given <tt>version</tt> </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic, long version) throws PulsarAdminException;
		SchemaInfo GetSchemaInfo(string Topic, long Version);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteSchema(String topic) throws PulsarAdminException;
		void DeleteSchema(string Topic);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt> with the provided schema info.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified fomrat </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		void CreateSchema(string Topic, SchemaInfo SchemaInfo);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		void CreateSchema(string Topic, PostSchemaPayload SchemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		IsCompatibilityResponse TestCompatibility(string Topic, PostSchemaPayload SchemaPayload);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		long? GetVersionBySchema(string Topic, PostSchemaPayload SchemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		IsCompatibilityResponse TestCompatibility(string Topic, SchemaInfo SchemaInfo);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		long? GetVersionBySchema(string Topic, SchemaInfo SchemaInfo);

		/// <summary>
		/// Get all version schemas <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.schema.SchemaInfo> getAllSchemas(String topic) throws PulsarAdminException;
		IList<SchemaInfo> GetAllSchemas(string Topic);

	}

}