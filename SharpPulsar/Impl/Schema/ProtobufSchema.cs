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
namespace SharpPulsar.Impl.Schema
{
	using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using Descriptors = com.google.protobuf.Descriptors;
	using GeneratedMessageV3 = com.google.protobuf.GeneratedMessageV3;
	using AllArgsConstructor = lombok.AllArgsConstructor;
	using Getter = lombok.Getter;
	using ProtobufData = org.apache.avro.protobuf.ProtobufData;
	using SchemaDefinition = org.apache.pulsar.client.api.schema.SchemaDefinition;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;
	using SharpPulsar.Impl.Schema.reader;
	using SharpPulsar.Impl.Schema.writer;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using ISchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
    using Pulsar.Client.Impl.Schema.Reader;
    using Pulsar.Client.Impl.Schema.Writer;
    using SharpPulsar.Common.Schema;
    using SharpPulsar.Interface.Schema;
    using SharpPulsar.Common.Protocol.Schema;


    /// <summary>
    /// A schema implementation to deal with protobuf generated messages.
    /// </summary>
    public class ProtobufSchema<T> : StructSchema<T> //where T :  com.google.protobuf.GeneratedMessageV3
	{

		public const string PARSING_INFO_PROPERTY = "__PARSING_INFO__";
		public class ProtoBufParsingInfo
		{
			internal readonly int number;
			internal readonly string name;
			internal readonly string type;
			internal readonly string label;
			// For future nested fields
			internal readonly IDictionary<string, object> definition;
		}

		private static Avro.Schema CreateProtobufAvroSchema<T>(Type pojo)
		{
			return ProtobufData.get().getSchema(pojo);
		}

		private ProtobufSchema(SchemaInfo schemaInfo, T protoMessageInstance) : base(schemaInfo)
		{
			Reader = new ProtobufReader<T>(protoMessageInstance);
			Writer = new ProtobufWriter<T>();
			// update properties with protobuf related properties
			IDictionary<string, string> allProperties = new Dictionary<string, string>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			allProperties.putAll(schemaInfo.Properties);
			// set protobuf parsing info
			allProperties[PARSING_INFO_PROPERTY] = GetParsingInfo(protoMessageInstance);
			schemaInfo.Properties = allProperties;
		}

		private string GetParsingInfo(T protoMessageInstance)
		{
			IList<ProtoBufParsingInfo> protoBufParsingInfos = new LinkedList<ProtoBufParsingInfo>();
			protoMessageInstance.DescriptorForType.Fields.forEach((Descriptors.FieldDescriptor fieldDescriptor) =>
			{
			protoBufParsingInfos.Add(new ProtoBufParsingInfo(fieldDescriptor.Number, fieldDescriptor.Name, fieldDescriptor.Type.name(), fieldDescriptor.toProto().Label.name(), null));
			});

			try
			{
				return (new ObjectMapper()).writeValueAsString(protoBufParsingInfos);
			}
			catch (JsonProcessingException e)
			{
				throw new Exception(e);
			}
		}

		protected internal override ISchemaReader<T> LooadReader(BytesSchemaVersion schemaVersion)
		{
			throw new Exception("ProtobufSchema don't support schema versioning");
		}

		public static ProtobufSchema<T> Of<T>(Type pojo) //where T : com.google.protobuf.GeneratedMessageV3
		{
			return Of(pojo, new Dictionary<string, string>());
		}

		public static ProtobufSchema<T> OfGenericClass<T>(Type pojo, IDictionary<string, string> properties)
		{
			ISchemaDefinition<T> schemaDefinition = SchemaDefinition.builder<T>().withPojo(pojo).withProperties(properties).build();
			return ProtobufSchema<T>.Of<T>(schemaDefinition);
		}

		public static ProtobufSchema of<T>(SchemaDefinition<T> schemaDefinition)
		{
			Type pojo = schemaDefinition.Pojo;

			if (!pojo.IsAssignableFrom(typeof(GeneratedMessageV3)))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException(typeof(GeneratedMessageV3).FullName + " is not assignable from " + pojo.FullName);
			}

				SchemaInfo schemaInfo = ISchemaInfo.builder().schema(createProtobufAvroSchema(schemaDefinition.Pojo).ToString().GetBytes(UTF_8)).type(SchemaType.PROTOBUF).name("").properties(schemaDefinition.Properties).build();

			try
			{
				return new ProtobufSchema(schemaInfo, (GeneratedMessageV3) pojo.GetMethod("getDefaultInstance").invoke(null));
			}
			catch (Exception e) when (e is IllegalAccessException || e is InvocationTargetException || e is NoSuchMethodException)
			{
				throw new System.ArgumentException(e);
			}
		}

		public static ProtobufSchema<T> of<T>(Type pojo, IDictionary<string, string> properties) where T : com.google.protobuf.GeneratedMessageV3
		{
			return ofGenericClass(pojo, properties);
		}
	}

}