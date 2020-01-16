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
	using GetAllVersionsSchemaResponse = org.apache.pulsar.common.protocol.schema.GetAllVersionsSchemaResponse;
	using IsCompatibilityResponse = org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse;
	using PostSchemaPayload = org.apache.pulsar.common.protocol.schema.PostSchemaPayload;
	using PostSchemaResponse = org.apache.pulsar.common.protocol.schema.PostSchemaResponse;
	using SchemaVersion = org.apache.pulsar.common.protocol.schema.SchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaInfoWithVersion = org.apache.pulsar.common.schema.SchemaInfoWithVersion;

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
		SchemaInfo getSchemaInfo(string topic);

		/// <summary>
		/// Retrieve the latest schema with verison of a topic.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <returns> latest schema with version </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.schema.SchemaInfoWithVersion getSchemaInfoWithVersion(String topic) throws PulsarAdminException;
		SchemaInfoWithVersion getSchemaInfoWithVersion(string topic);

		/// <summary>
		/// Retrieve the schema of a topic at a given <tt>version</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="version"> schema version </param>
		/// <returns> the schema info at a given <tt>version</tt> </returns>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic, long version) throws PulsarAdminException;
		SchemaInfo getSchemaInfo(string topic, long version);

		/// <summary>
		/// Delete the schema associated with a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteSchema(String topic) throws PulsarAdminException;
		void deleteSchema(string topic);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt> with the provided schema info.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified fomrat </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		void createSchema(string topic, SchemaInfo schemaInfo);

		/// <summary>
		/// Create a schema for a given <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createSchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		void createSchema(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		IsCompatibilityResponse testCompatibility(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaPayload"> schema payload </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload schemaPayload) throws PulsarAdminException;
		long? getVersionBySchema(string topic, PostSchemaPayload schemaPayload);

		/// <summary>
		/// Judge schema compatibility <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		IsCompatibilityResponse testCompatibility(string topic, SchemaInfo schemaInfo);

		/// <summary>
		/// Find schema version <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <param name="schemaInfo"> schema info </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws PulsarAdminException;
		long? getVersionBySchema(string topic, SchemaInfo schemaInfo);

		/// <summary>
		/// Get all version schemas <tt>topic</tt>.
		/// </summary>
		/// <param name="topic"> topic name, in fully qualified format </param>
		/// <exception cref="PulsarAdminException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.List<org.apache.pulsar.common.schema.SchemaInfo> getAllSchemas(String topic) throws PulsarAdminException;
		IList<SchemaInfo> getAllSchemas(string topic);

	}

}