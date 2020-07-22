using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Auth;
using SharpPulsar.Impl.Schema;
using SharpPulsar.Impl.Schema.Generic;
using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Batch;
using SharpPulsar.Batch.Api;

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
namespace SharpPulsar.Impl
{
	/// <summary>
	/// Helper class for class instantiations and it also contains methods to work with schemas.
	/// </summary>
	public class DefaultImplementation
	{

		//private static readonly Type CLIENT_BUILDER_IMPL = new ClientBuilderImpl();

		public static ISchemaDefinitionBuilder NewSchemaDefinitionBuilder()
		{
			return new SchemaDefinitionBuilderImpl();
		}


		public static IMessageId NewMessageId(long ledgerId, long entryId, int partitionIndex, int batch)
		{
			return new BatchMessageId(ledgerId, entryId, partitionIndex, batch);
		}
        
		public static IMessageId NewMessageIdFromByteArray(sbyte[] data)
		{
			return MessageId.FromByteArray(data);
		}

		public static IMessageId NewMessageIdFromByteArrayWithTopic(sbyte[] data, string topicName)
		{
			return MessageId.FromByteArrayWithTopic(data, topicName);
		}

		public static IAuthentication NewAuthenticationToken(string token)
		{
			return new AuthenticationToken(token);
		}
        public static IAuthentication NewAuthenticationSts(string client, string secret, string authority)
        {
            return new AuthenticationOAuth2(client, secret, authority);
        }
		public static IAuthentication NewAuthenticationToken(Func<string> supplier)
		{
			return new AuthenticationToken(supplier);
		}

		public static IAuthentication NewAuthenticationTls(string certFilePath, string keyFilePath)
		{
			return new AuthenticationTls(certFilePath, keyFilePath);
		}

		public static IAuthentication CreateAuthentication(string authPluginClassName, string authParamsString)
		{
			return AuthenticationUtil.Create(authPluginClassName, authParamsString);
		}

		public static IAuthentication CreateAuthentication(string authPluginClassName, IDictionary<string, string> authParams)
		{
			return AuthenticationUtil.Create(authPluginClassName, authParams);
		}

		public static ISchema NewBytesSchema()
		{
			return new BytesSchema();
		}
		
		public static ISchema NewAvroSchema(ISchemaDefinition schemaDefinition)
		{
			return AvroSchema.Of(schemaDefinition);
		}

		public static ISchema NewAutoConsumeSchema()
		{
			return new AutoConsumeSchema();
		}

		public static ISchema NewAutoProduceSchema()
		{
			return new AutoProduceBytesSchema();
		}

		public static ISchema NewAutoProduceSchema(ISchema schema)
		{
			return new AutoProduceBytesSchema(schema);
			//return catchExceptions(() => (Schema<sbyte[]>) getConstructor("SharpPulsar.Impl.Schema.AutoProduceBytesSchema", typeof(Schema)).newInstance(schema));
		}

		public static ISchema GetSchema(ISchemaInfo schemaInfo)
		{
			return AutoConsumeSchema.GetSchema((SchemaInfo)schemaInfo);
		}

		public static IGenericSchema GetGenericSchema(ISchemaInfo schemaInfo)
		{
			return GenericSchemaImpl.Of((SchemaInfo)schemaInfo);
		}



		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifySchemaInfo(SchemaInfo schemaInfo)
		{
			return SchemaUtils.JsonifySchemaInfo(schemaInfo);
		}

		/// <summary>
		/// Jsonify the schema info with version.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info with version </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string JsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
			return SchemaUtils.JsonifySchemaInfoWithVersion(schemaInfoWithVersion);
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.JsonifyKeyValueSchemaInfo(kvSchemaInfo);
		}

		/// <summary>
		/// Convert the key/value schema data.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert key/value schema data string </returns>
		public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			return SchemaUtils.ConvertKeyValueSchemaInfoDataToString(kvSchemaInfo);
		}

		public static IBatcherBuilder NewDefaultBatcherBuilder(ActorSystem system)
		{
			return new DefaultBatcherBuilder(system);
		}

		public static IBatcherBuilder NewKeyBasedBatcherBuilder(ActorSystem system)
        {
            return new KeyBasedBatcherBuilder(system);
        }
	}

}