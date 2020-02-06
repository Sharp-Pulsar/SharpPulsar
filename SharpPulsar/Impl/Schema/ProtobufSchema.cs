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
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Impl.Schema.Reader;
	using SharpPulsar.Impl.Schema.Writer;
	using BytesSchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.BytesSchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;


	/// <summary>
	/// A schema implementation to deal with protobuf generated messages.
	/// </summary>
	public class ProtobufSchema<T> : StructSchema<T> where T : com.google.protobuf.GeneratedMessageV3
	{

		public const string ParsingInfoProperty = "__PARSING_INFO__";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter @AllArgsConstructor public static class ProtoBufParsingInfo
		public class ProtoBufParsingInfo
		{
			internal readonly int Number;
			internal readonly string Name;
			internal readonly string Type;
			internal readonly string Label;
			// For future nested fields
			internal readonly IDictionary<string, object> Definition;
		}

		private static org.apache.avro.Schema CreateProtobufAvroSchema<T>(Type Pojo)
		{
			return ProtobufData.get().getSchema(Pojo);
		}

		private ProtobufSchema(SchemaInfo SchemaInfo, T ProtoMessageInstance) : base(SchemaInfo)
		{
			Reader = new ProtobufReader<>(ProtoMessageInstance);
			Writer = new ProtobufWriter<>();
			// update properties with protobuf related properties
			IDictionary<string, string> AllProperties = new Dictionary<string, string>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			AllProperties.putAll(SchemaInfo.Properties);
			// set protobuf parsing info
			AllProperties[ParsingInfoProperty] = GetParsingInfo(ProtoMessageInstance);
			SchemaInfo.Properties = AllProperties;
		}

		private string GetParsingInfo(T ProtoMessageInstance)
		{
			IList<ProtoBufParsingInfo> ProtoBufParsingInfos = new LinkedList<ProtoBufParsingInfo>();
			ProtoMessageInstance.DescriptorForType.Fields.forEach((Descriptors.FieldDescriptor FieldDescriptor) =>
			{
			ProtoBufParsingInfos.Add(new ProtoBufParsingInfo(FieldDescriptor.Number, FieldDescriptor.Name, FieldDescriptor.Type.name(), FieldDescriptor.toProto().Label.name(), null));
			});

			try
			{
				return (new ObjectMapper()).writeValueAsString(ProtoBufParsingInfos);
			}
			catch (JsonProcessingException E)
			{
				throw new Exception(E);
			}
		}

		public override ISchemaReader<T> LoadReader(BytesSchemaVersion SchemaVersion)
		{
			throw new Exception("ProtobufSchema don't support schema versioning");
		}

		public static ProtobufSchema<T> Of<T>(Type Pojo) where T : com.google.protobuf.GeneratedMessageV3
		{
			return Of(Pojo, new Dictionary<string, string>());
		}

		public static ProtobufSchema OfGenericClass<T>(Type Pojo, IDictionary<string, string> Properties)
		{
			ISchemaDefinition<T> SchemaDefinition = SchemaDefinition.builder<T>().withPojo(Pojo).withProperties(Properties).build();
			return ProtobufSchema.Of(SchemaDefinition);
		}

		public static ProtobufSchema Of<T>(ISchemaDefinition<T> SchemaDefinition)
		{
			Type Pojo = SchemaDefinition.Pojo;

			if (!Pojo.IsAssignableFrom(typeof(GeneratedMessageV3)))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException(typeof(GeneratedMessageV3).FullName + " is not assignable from " + Pojo.FullName);
			}

				SchemaInfo SchemaInfo = SchemaInfo.builder().schema(CreateProtobufAvroSchema(SchemaDefinition.Pojo).ToString().GetBytes(UTF_8)).type(SchemaType.PROTOBUF).name("").properties(SchemaDefinition.Properties).build();

			try
			{
				return new ProtobufSchema(SchemaInfo, (GeneratedMessageV3) Pojo.GetMethod("getDefaultInstance").invoke(null));
			}
			catch (Exception e) when (e is IllegalAccessException || e is InvocationTargetException || e is NoSuchMethodException)
			{
				throw new System.ArgumentException(e);
			}
		}

		public static ProtobufSchema<T> Of<T>(Type Pojo, IDictionary<string, string> Properties) where T : com.google.protobuf.GeneratedMessageV3
		{
			return OfGenericClass(Pojo, Properties);
		}
	}

}