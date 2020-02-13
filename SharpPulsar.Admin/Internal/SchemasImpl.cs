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
namespace Org.Apache.Pulsar.Client.Admin.@internal
{


	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using DefaultImplementation = Org.Apache.Pulsar.Client.@internal.DefaultImplementation;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using ErrorData = Org.Apache.Pulsar.Common.Policies.Data.ErrorData;
	using DeleteSchemaResponse = Org.Apache.Pulsar.Common.Protocol.Schema.DeleteSchemaResponse;
	using GetAllVersionsSchemaResponse = Org.Apache.Pulsar.Common.Protocol.Schema.GetAllVersionsSchemaResponse;
	using GetSchemaResponse = Org.Apache.Pulsar.Common.Protocol.Schema.GetSchemaResponse;
	using IsCompatibilityResponse = Org.Apache.Pulsar.Common.Protocol.Schema.IsCompatibilityResponse;
	using LongSchemaVersionResponse = Org.Apache.Pulsar.Common.Protocol.Schema.LongSchemaVersionResponse;
	using PostSchemaPayload = Org.Apache.Pulsar.Common.Protocol.Schema.PostSchemaPayload;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaInfoWithVersion = Org.Apache.Pulsar.Common.Schema.SchemaInfoWithVersion;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;


	public class SchemasImpl : BaseResource, Schemas
	{

		private readonly WebTarget target;

		public SchemasImpl(WebTarget Web, Authentication Auth, long ReadTimeoutMs) : base(Auth, ReadTimeoutMs)
		{
			this.target = Web.path("/admin/v2/schemas");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SchemaInfo GetSchemaInfo(string Topic)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				GetSchemaResponse Response = Request(SchemaPath(Tn)).get(typeof(GetSchemaResponse));
				return ConvertGetSchemaResponseToSchemaInfo(Tn, Response);
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfoWithVersion getSchemaInfoWithVersion(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SchemaInfoWithVersion GetSchemaInfoWithVersion(string Topic)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				GetSchemaResponse Response = Request(SchemaPath(Tn)).get(typeof(GetSchemaResponse));
				return ConvertGetSchemaResponseToSchemaInfoWithVersion(Tn, Response);
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.schema.SchemaInfo getSchemaInfo(String topic, long version) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override SchemaInfo GetSchemaInfo(string Topic, long Version)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				GetSchemaResponse Response = Request(SchemaPath(Tn).path(Convert.ToString(Version))).get(typeof(GetSchemaResponse));
				return ConvertGetSchemaResponseToSchemaInfo(Tn, Response);
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void deleteSchema(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void DeleteSchema(string Topic)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				Request(SchemaPath(Tn)).delete(typeof(DeleteSchemaResponse));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSchema(string Topic, SchemaInfo SchemaInfo)
		{

			CreateSchema(Topic, ConvertSchemaInfoToPostSchemaPayload(SchemaInfo));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void createSchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override void CreateSchema(string Topic, PostSchemaPayload Payload)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				Request(SchemaPath(Tn)).post(Entity.json(Payload), typeof(ErrorData));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IsCompatibilityResponse TestCompatibility(string Topic, PostSchemaPayload Payload)
		{
			try
			{
				TopicName Tn = TopicName.get(Topic);
				return Request(CompatibilityPath(Tn)).post(Entity.json(Payload), typeof(IsCompatibilityResponse));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.protocol.schema.PostSchemaPayload payload) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override long? GetVersionBySchema(string Topic, PostSchemaPayload Payload)
		{
			try
			{
				return Request(VersionPath(TopicName.get(Topic))).post(Entity.json(Payload), typeof(LongSchemaVersionResponse)).Version;
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.apache.pulsar.common.protocol.schema.IsCompatibilityResponse testCompatibility(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IsCompatibilityResponse TestCompatibility(string Topic, SchemaInfo SchemaInfo)
		{
			try
			{
				return Request(CompatibilityPath(TopicName.get(Topic))).post(Entity.json(ConvertSchemaInfoToPostSchemaPayload(SchemaInfo)), typeof(IsCompatibilityResponse));
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public System.Nullable<long> getVersionBySchema(String topic, org.apache.pulsar.common.schema.SchemaInfo schemaInfo) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override long? GetVersionBySchema(string Topic, SchemaInfo SchemaInfo)
		{
			try
			{
				return Request(VersionPath(TopicName.get(Topic))).post(Entity.json(ConvertSchemaInfoToPostSchemaPayload(SchemaInfo)), typeof(LongSchemaVersionResponse)).Version;
			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<org.apache.pulsar.common.schema.SchemaInfo> getAllSchemas(String topic) throws org.apache.pulsar.client.admin.PulsarAdminException
		public override IList<SchemaInfo> GetAllSchemas(string Topic)
		{
			try
			{
				TopicName TopicName = TopicName.get(Topic);
				return Request(SchemasPath(TopicName.get(Topic))).get(typeof(GetAllVersionsSchemaResponse)).GetSchemaResponses.Select(getSchemaResponse => ConvertGetSchemaResponseToSchemaInfo(TopicName, getSchemaResponse)).ToList();

			}
			catch (Exception E)
			{
				throw GetApiException(E);
			}
		}

		private WebTarget SchemaPath(TopicName TopicName)
		{
			return target.path(TopicName.Tenant).path(TopicName.NamespacePortion).path(TopicName.EncodedLocalName).path("schema");
		}

		private WebTarget VersionPath(TopicName TopicName)
		{
			return target.path(TopicName.Tenant).path(TopicName.NamespacePortion).path(TopicName.EncodedLocalName).path("version");
		}

		private WebTarget SchemasPath(TopicName TopicName)
		{
			return target.path(TopicName.Tenant).path(TopicName.NamespacePortion).path(TopicName.EncodedLocalName).path("schemas");
		}

		private WebTarget CompatibilityPath(TopicName TopicName)
		{
			return target.path(TopicName.Tenant).path(TopicName.NamespacePortion).path(TopicName.EncodedLocalName).path("compatibility");
		}

		// the util function converts `GetSchemaResponse` to `SchemaInfo`
		internal static SchemaInfo ConvertGetSchemaResponseToSchemaInfo(TopicName Tn, GetSchemaResponse Response)
		{
			SchemaInfo Info = new SchemaInfo();
			sbyte[] Schema;
			if (Response.Type == SchemaType.KEY_VALUE)
			{
				Schema = DefaultImplementation.convertKeyValueDataStringToSchemaInfoSchema(Response.Data.getBytes(UTF_8));
			}
			else
			{
				Schema = Response.Data.getBytes(UTF_8);
			}
			Info.Schema = Schema;
			Info.Type = Response.Type;
			Info.Properties = Response.Properties;
			Info.Name = Tn.LocalName;
			return Info;
		}

		internal static SchemaInfoWithVersion ConvertGetSchemaResponseToSchemaInfoWithVersion(TopicName Tn, GetSchemaResponse Response)
		{

			return SchemaInfoWithVersion.builder().schemaInfo(ConvertGetSchemaResponseToSchemaInfo(Tn, Response)).version(Response.Version).build();
		}




		// the util function exists for backward compatibility concern
		internal static string ConvertSchemaDataToStringLegacy(SchemaInfo SchemaInfo)
		{
			sbyte[] SchemaData = SchemaInfo.Schema;
			if (null == SchemaInfo.Schema)
			{
				return "";
			}

			if (SchemaInfo.Type == SchemaType.KEY_VALUE)
			{
			   return DefaultImplementation.convertKeyValueSchemaInfoDataToString(DefaultImplementation.decodeKeyValueSchemaInfo(SchemaInfo));
			}

			return StringHelper.NewString(SchemaData, UTF_8);
		}

		internal static PostSchemaPayload ConvertSchemaInfoToPostSchemaPayload(SchemaInfo SchemaInfo)
		{

			PostSchemaPayload Payload = new PostSchemaPayload();
			Payload.Type = SchemaInfo.Type.name();
			Payload.Properties = SchemaInfo.Properties;
			// for backward compatibility concern, we convert `bytes` to `string`
			// we can consider fixing it in a new version of rest endpoint
			Payload.Schema = ConvertSchemaDataToStringLegacy(SchemaInfo);
			return Payload;
		}
	}

}