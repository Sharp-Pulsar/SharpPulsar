using System;
using System.Collections.Generic;
using System.Text.Json;
using DotNetty.Codecs.Base64;
using Google.Protobuf;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Shared;
using SharpPulsar.Util;

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
	/*using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using GsonBuilder = com.google.gson.GsonBuilder;
	using JsonDeserializationContext = com.google.gson.JsonDeserializationContext;
	using JsonDeserializer = com.google.gson.JsonDeserializer;
	using JsonElement = com.google.gson.JsonElement;
	using JsonObject = com.google.gson.JsonObject;
	using JsonParseException = com.google.gson.JsonParseException;
	using JsonParser = com.google.gson.JsonParser;
	using JsonPrimitive = com.google.gson.JsonPrimitive;
	using JsonSerializationContext = com.google.gson.JsonSerializationContext;
	using JsonSerializer = com.google.gson.JsonSerializer;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ByteBufUtil = io.netty.buffer.ByteBufUtil;


	using DefaultImplementation = SharpPulsar.@internal.DefaultImplementation;
	using Org.Apache.Pulsar.Common.Schema;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaInfoWithVersion = Org.Apache.Pulsar.Common.Schema.SchemaInfoWithVersion;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;
	using ObjectMapperFactory = Org.Apache.Pulsar.Common.Util.ObjectMapperFactory;*/


	/// <summary>
	/// Utils for schemas.
	/// </summary>
	public sealed class SchemaUtils
	{

		private static readonly sbyte[] KEY_VALUE_SCHEMA_IS_PRIMITIVE = new sbyte[0];

		private const string KeyValueSchemaNullString = @"""""";

		private SchemaUtils()
		{
		}

		/// <summary>
		/// Keeps a map between <seealso cref="SchemaType"/> to a list of java classes that can be used to represent them.
		/// </summary>
		private static readonly IDictionary<SchemaType, IList<Type>> SchemaTypeClasses = new Dictionary<SchemaType, IList<Type>>();

		/// <summary>
		/// Maps the java classes to the corresponding <seealso cref="SchemaType"/>.
		/// </summary>
		private static readonly IDictionary<Type, SchemaType> JavaClassSchemaTypes = new Dictionary<Type, SchemaType>();

		static SchemaUtils()
		{
			// int8
			SchemaTypeClasses[SchemaType.INT8] = Arrays.asList(typeof(Byte));
			// int16
			SchemaTypeClasses[SchemaType.INT16] = Arrays.asList(typeof(Short));
			// int32
			SchemaTypeClasses[SchemaType.INT32] = Arrays.asList(typeof(Integer));
			// int64
			SchemaTypeClasses[SchemaType.INT64] = Arrays.asList(typeof(Long));
			// float
			SchemaTypeClasses[SchemaType.FLOAT] = Arrays.asList(typeof(Float));
			// double
			SchemaTypeClasses[SchemaType.DOUBLE] = Arrays.asList(typeof(Double));
			// boolean
			SchemaTypeClasses[SchemaType.BOOLEAN] = Arrays.asList(typeof(Boolean));
			// string
			SchemaTypeClasses[SchemaType.STRING] = Arrays.asList(typeof(string));
			// bytes
			SchemaTypeClasses[SchemaType.BYTES] = Arrays.asList(typeof(sbyte[]), typeof(ByteBuffer), typeof(ByteBuf));
			// build the reverse mapping
			SchemaTypeClasses.forEach((type, classes) => classes.forEach(clz => JavaClassSchemaTypes.put(clz, type)));
		}

		public static void ValidateFieldSchema(string Name, SchemaType Type, object Val)
		{
			if (null == Val)
			{
				return;
			}

			var ExpectedClasses = SchemaTypeClasses[Type];

			if (null == ExpectedClasses)
			{
				throw new System.Exception("Invalid Java object for schema type " + Type + " : " + Val.GetType() + @" for field : """ + Name + @"""");
			}

			var FoundMatch = false;
			foreach (var ExpectedCls in ExpectedClasses)
			{
				if (ExpectedCls.IsInstanceOfType(Val))
				{
					FoundMatch = true;
					break;
				}
			}

			if (!FoundMatch)
			{
				throw new System.Exception("Invalid Java object for schema type " + Type + " : " + Val.GetType() + @" for field : """ + Name + @"""");
			}

			switch (Type.InnerEnumValue)
			{
				case SchemaType.InnerEnum.INT8:
				case SchemaType.InnerEnum.INT16:
				case SchemaType.InnerEnum.PROTOBUF:
				case SchemaType.InnerEnum.AVRO:
				case SchemaType.InnerEnum.AUTO_CONSUME:
				case SchemaType.InnerEnum.AUTO_PUBLISH:
				case SchemaType.InnerEnum.AUTO:
				case SchemaType.InnerEnum.KEY_VALUE:
				case SchemaType.InnerEnum.JSON:
				case SchemaType.InnerEnum.NONE:
					throw new System.Exception("Currently " + Type.GetType().Name + " is not supported");
				default:
					break;
			}
		}

		public static string GetStringSchemaVersion(sbyte[] SchemaVersionBytes)
		{
			if (null == SchemaVersionBytes)
			{
				return "NULL";
			}
			else if (SchemaVersionBytes.Length == Long.BYTES || SchemaVersionBytes.Length == (sizeof(long) * 8))
			{
				ByteBuffer Bb = ByteBuffer.wrap(SchemaVersionBytes);
				return Bb.Long.ToString();
			}
			else if (SchemaVersionBytes.Length == 0)
			{
				return "EMPTY";
			}
			else
			{
				return Convert.ToBase64String((byte[])(Array)SchemaVersionBytes);
			}

		}

		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifySchemaInfo(SchemaInfo schemaInfo)
        {
            return JsonSerializer.Serialize(schemaInfo, new JsonSerializerOptions{WriteIndented = true});
        }

		/// <summary>
		/// Jsonify the schema info with verison.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string JsonifySchemaInfoWithVersion(SchemaInfoWithVersion SchemaInfoWithVersion)
		{
            return JsonSerializer.Serialize(SchemaInfoWithVersion, new JsonSerializerOptions { WriteIndented = true });
		}

		public class SchemaPropertiesSerializer : JsonSerializer<IDictionary<string, string>>
		{

			public override JsonElement Serialize(IDictionary<string, string> properties, Type Type, JsonSerializationContext JsonSerializationContext)
			{
				var SortedProperties = new SortedDictionary<string, string>(properties);
				JsonObject Object = new JsonObject();
				SortedProperties.forEach((key, value) =>
				{
				    Object.add(key, new JsonPrimitive(value));
				});
				return Object;
			}

		}

		public class SchemaPropertiesDeserializer : JsonDeserializer<IDictionary<string, string>>
		{

            public override IDictionary<string, string> Deserialize(JsonElement JsonElement, Type Type, JsonDeserializationContext JsonDeserializationContext)
			{

				var SortedProperties = new SortedDictionary<string, string>();
				JsonElement.AsJsonObject.entrySet().forEach(entry => SortedProperties.put(entry.Key, entry.Value.AsString));
				return SortedProperties;
			}

		}

		private static readonly SchemaPropertiesSerializer SCHEMA_PROPERTIES_SERIALIZER = new SchemaPropertiesSerializer();

		private static readonly SchemaPropertiesDeserializer SCHEMA_PROPERTIES_DESERIALIZER = new SchemaPropertiesDeserializer();

		public class ByteArrayToStringAdapter : JsonSerializer<sbyte[]>
		{

			internal readonly SchemaInfo SchemaInfo;

			public ByteArrayToStringAdapter(SchemaInfo SchemaInfo)
			{
				this.SchemaInfo = SchemaInfo;
			}

			public virtual JsonElement Serialize(sbyte[] Src, Type TypeOfSrc, JsonSerializationContext Context)
			{
				string SchemaDef = SchemaInfo.SchemaDefinition;
				SchemaType Type = SchemaInfo.Type;
				switch (Type.innerEnumValue)
				{
					case SchemaType.InnerEnum.AVRO:
					case SchemaType.InnerEnum.JSON:
					case SchemaType.InnerEnum.PROTOBUF:
						return ToJsonObject(SchemaInfo.SchemaDefinition);
					case SchemaType.InnerEnum.KEY_VALUE:
						KeyValue<SchemaInfo, SchemaInfo> SchemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(SchemaInfo);
						JsonObject Obj = new JsonObject();
						var KeyJson = JsonifySchemaInfo(SchemaInfoKeyValue.Key);
						var ValueJson = JsonifySchemaInfo(SchemaInfoKeyValue.Value);
						Obj.add("key", ToJsonObject(KeyJson));
						Obj.add("value", ToJsonObject(ValueJson));
						return Obj;
					default:
						return new JsonPrimitive(SchemaDef);
				}
			}
		}

		public static JsonObject ToJsonObject(string Json)
		{
			JsonParser Parser = new JsonParser();
			return Parser.parse(Json).AsJsonObject;
		}

		public class SchemaInfoToStringAdapter : JsonSerializer<SchemaInfo>
		{

			public override JsonElement Serialize(SchemaInfo SchemaInfo, Type Type, JsonSerializationContext JsonSerializationContext)
			{
				return ToJsonObject(JsonifySchemaInfo(SchemaInfo));
			}
		}

		private static readonly SchemaInfoToStringAdapter SCHEMAINFO_ADAPTER = new SchemaInfoToStringAdapter();

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> KvSchemaInfo)
		{
			GsonBuilder GsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(SchemaInfo), SCHEMAINFO_ADAPTER).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);
			return GsonBuilder.create().toJson(KvSchemaInfo);
			return JsonSerializer.Serialize(KvSchemaInfo)
		}

		/// <summary>
		/// convert the key/value schema info data to string
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert schema info data string </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String convertKeyValueSchemaInfoDataToString(org.apache.pulsar.common.schema.KeyValue<org.apache.pulsar.common.schema.SchemaInfo, org.apache.pulsar.common.schema.SchemaInfo> kvSchemaInfo) throws java.io.IOException
		public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> KvSchemaInfo)
		{
			ObjectMapper ObjectMapper = ObjectMapperFactory.create();
			KeyValue<object, object> KeyValue = new KeyValue<object, object>(SchemaType.isPrimitiveType(KvSchemaInfo.Key.Type) ? "" : ObjectMapper.readTree(KvSchemaInfo.Key.Schema), SchemaType.isPrimitiveType(KvSchemaInfo.Value.Type) ? "" : ObjectMapper.readTree(KvSchemaInfo.Value.Schema));
			return ObjectMapper.writeValueAsString(KeyValue);
		}

		private static sbyte[] GetKeyOrValueSchemaBytes(JsonElement JsonElement)
		{
			return KeyValueSchemaNullString.Equals(JsonElement.ToString()) ? KEY_VALUE_SCHEMA_IS_PRIMITIVE : JsonElement.ToString().GetBytes(UTF_8);
		}

		/// <summary>
		/// convert the key/value schema info data json bytes to key/value schema info data bytes
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] convertKeyValueDataStringToSchemaInfoSchema(byte[] keyValueSchemaInfoDataJsonBytes) throws java.io.IOException
		public static sbyte[] ConvertKeyValueDataStringToSchemaInfoSchema(sbyte[] KeyValueSchemaInfoDataJsonBytes)
		{
			JsonObject JsonObject = ToJsonObject(StringHelper.NewString(KeyValueSchemaInfoDataJsonBytes, UTF_8));
			var KeyBytes = GetKeyOrValueSchemaBytes(JsonObject.get("key"));
			var ValueBytes = GetKeyOrValueSchemaBytes(JsonObject.get("value"));
			var DataLength = 4 + KeyBytes.Length + 4 + ValueBytes.Length;
			var Schema = new sbyte[DataLength];
			//record the key value schema respective length
			ByteBuf ByteBuf = ByteBufAllocator.DEFAULT.heapBuffer(DataLength);
			ByteBuf.writeInt(KeyBytes.Length).writeBytes(KeyBytes).writeInt(ValueBytes.Length).writeBytes(ValueBytes);
			ByteBuf.readBytes(Schema);
			return Schema;
		}

		/// <summary>
		/// Serialize schema properties
		/// </summary>
		/// <param name="properties"> schema properties </param>
		/// <returns> the serialized schema properties </returns>
		public static string SerializeSchemaProperties(IDictionary<string, string> Properties)
		{
			GsonBuilder GsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);
			return GsonBuilder.create().toJson(Properties);
		}

		/// <summary>
		/// Deserialize schema properties from a serialized schema properties.
		/// </summary>
		/// <param name="serializedProperties"> serialized properties </param>
		/// <returns> the deserialized properties </returns>
		public static IDictionary<string, string> DeserializeSchemaProperties(string SerializedProperties)
		{
			GsonBuilder GsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_DESERIALIZER);
			return GsonBuilder.create().fromJson(SerializedProperties, typeof(System.Collections.IDictionary));
		}

	}

}