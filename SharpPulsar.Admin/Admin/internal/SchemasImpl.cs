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
namespace org.apache.pulsar.client.admin.@internal
{


	using Authentication = org.apache.pulsar.client.api.Authentication;
	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using ErrorData = org.apache.pulsar.common.policies.data.ErrorData;
	using DeleteSchemaResponse = org.apache.pulsar.common.protocol.schema.DeleteSchemaResponse;
	using GetAllVersionsSchemaResponse = org.apache.pulsar.common.protocol.schema.GetAllVersionsSchemaResponse;
	using GetSchemaResponse = org.apache.pulsar.common.protocol.schema.GetSchemaResponse;
	using IsCompatibilityResponse = org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse;
	using LongSchemaVersionResponse = org.apache.pulsar.common.protocol.schema.LongSchemaVersionResponse;
	using PostSchemaPayload = org.apache.pulsar.common.protocol.schema.PostSchemaPayload;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaInfoWithVersion = org.apache.pulsar.common.schema.SchemaInfoWithVersion;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;


	public class SchemasImpl : BaseResource, Schemas
	{

		private readonly WebTarget target;

		public SchemasImpl(WebTarget web, Authentication auth, long readTimeoutMs) : base(auth, readTimeoutMs)
		{
			this.target = web.path("/admin/v2/schemas");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SchemaInfo getSchemaInfo(string topic)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				GetSchemaResponse response = request(schemaPath(tn)).get(typeof(GetSchemaResponse));
				return convertGetSchemaResponseToSchemaInfo(tn, response);
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfoWithVersion getSchemaInfoWithVersion(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SchemaInfoWithVersion getSchemaInfoWithVersion(string topic)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				GetSchemaResponse response = request(schemaPath(tn)).get(typeof(GetSchemaResponse));
				return convertGetSchemaResponseToSchemaInfoWithVersion(tn, response);
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic, long version) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual SchemaInfo getSchemaInfo(string topic, long version)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				GetSchemaResponse response = request(schemaPath(tn).path(Convert.ToString(version))).get(typeof(GetSchemaResponse));
				return convertGetSchemaResponseToSchemaInfo(tn, response);
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSchema(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void deleteSchema(string topic)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				request(schemaPath(tn)).delete(typeof(DeleteSchemaResponse));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSchema(string topic, SchemaInfo schemaInfo)
		{

			createSchema(topic, convertSchemaInfoToPostSchemaPayload(schemaInfo));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual void createSchema(string topic, PostSchemaPayload payload)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				request(schemaPath(tn)).post(Entity.json(payload), typeof(ErrorData));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IsCompatibilityResponse testCompatibility(string topic, PostSchemaPayload payload)
		{
			try
			{
				TopicName tn = TopicName.get(topic);
				return request(compatibilityPath(tn)).post(Entity.json(payload), typeof(IsCompatibilityResponse));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual long? getVersionBySchema(string topic, PostSchemaPayload payload)
		{
			try
			{
				return request(versionPath(TopicName.get(topic))).post(Entity.json(payload), typeof(LongSchemaVersionResponse)).Version;
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IsCompatibilityResponse testCompatibility(string topic, SchemaInfo schemaInfo)
		{
			try
			{
				return request(compatibilityPath(TopicName.get(topic))).post(Entity.json(convertSchemaInfoToPostSchemaPayload(schemaInfo)), typeof(IsCompatibilityResponse));
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual long? getVersionBySchema(string topic, SchemaInfo schemaInfo)
		{
			try
			{
				return request(versionPath(TopicName.get(topic))).post(Entity.json(convertSchemaInfoToPostSchemaPayload(schemaInfo)), typeof(LongSchemaVersionResponse)).Version;
			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.schema.SchemaInfo> getAllSchemas(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public virtual IList<SchemaInfo> getAllSchemas(string topic)
		{
			try
			{
				TopicName topicName = TopicName.get(topic);
				return request(schemasPath(TopicName.get(topic))).get(typeof(GetAllVersionsSchemaResponse)).GetSchemaResponses.Select(getSchemaResponse => convertGetSchemaResponseToSchemaInfo(topicName, getSchemaResponse)).ToList();

			}
			catch (Exception e)
			{
				throw getApiException(e);
			}
		}

		private WebTarget schemaPath(TopicName topicName)
		{
			return target.path(topicName.Tenant).path(topicName.NamespacePortion).path(topicName.EncodedLocalName).path("schema");
		}

		private WebTarget versionPath(TopicName topicName)
		{
			return target.path(topicName.Tenant).path(topicName.NamespacePortion).path(topicName.EncodedLocalName).path("version");
		}

		private WebTarget schemasPath(TopicName topicName)
		{
			return target.path(topicName.Tenant).path(topicName.NamespacePortion).path(topicName.EncodedLocalName).path("schemas");
		}

		private WebTarget compatibilityPath(TopicName topicName)
		{
			return target.path(topicName.Tenant).path(topicName.NamespacePortion).path(topicName.EncodedLocalName).path("compatibility");
		}

		// the util function converts `GetSchemaResponse` to `SchemaInfo`
		internal static SchemaInfo convertGetSchemaResponseToSchemaInfo(TopicName tn, GetSchemaResponse response)
		{
			SchemaInfo info = new SchemaInfo();
			sbyte[] schema;
			if (response.Type == SchemaType.KEY_VALUE)
			{
				schema = DefaultImplementation.convertKeyValueDataStringToSchemaInfoSchema(response.Data.getBytes(UTF_8));
			}
			else
			{
				schema = response.Data.getBytes(UTF_8);
			}
			info.Schema = schema;
			info.Type = response.Type;
			info.Properties = response.Properties;
			info.Name = tn.LocalName;
			return info;
		}

		internal static SchemaInfoWithVersion convertGetSchemaResponseToSchemaInfoWithVersion(TopicName tn, GetSchemaResponse response)
		{

			return SchemaInfoWithVersion.builder().schemaInfo(convertGetSchemaResponseToSchemaInfo(tn, response)).version(response.Version).build();
		}




		// the util function exists for backward compatibility concern
		internal static string convertSchemaDataToStringLegacy(SchemaInfo schemaInfo)
		{
			sbyte[] schemaData = schemaInfo.Schema;
			if (null == schemaInfo.Schema)
			{
				return "";
			}

			if (schemaInfo.Type == SchemaType.KEY_VALUE)
			{
			   return DefaultImplementation.convertKeyValueSchemaInfoDataToString(DefaultImplementation.decodeKeyValueSchemaInfo(schemaInfo));
			}

			return StringHelper.NewString(schemaData, UTF_8);
		}

		internal static PostSchemaPayload convertSchemaInfoToPostSchemaPayload(SchemaInfo schemaInfo)
		{

			PostSchemaPayload payload = new PostSchemaPayload();
			payload.Type = schemaInfo.Type.name();
			payload.Properties = schemaInfo.Properties;
			// for backward compatibility concern, we convert `bytes` to `string`
			// we can consider fixing it in a new version of rest endpoint
			payload.Schema = convertSchemaDataToStringLegacy(schemaInfo);
			return payload;
		}
	}

}