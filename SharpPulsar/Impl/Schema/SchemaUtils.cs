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
namespace org.apache.pulsar.client.impl.schema
{
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
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


	using DefaultImplementation = org.apache.pulsar.client.@internal.DefaultImplementation;
	using KeyValue = org.apache.pulsar.common.schema.KeyValue;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaInfoWithVersion = org.apache.pulsar.common.schema.SchemaInfoWithVersion;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;
	using ObjectMapperFactory = org.apache.pulsar.common.util.ObjectMapperFactory;


	/// <summary>
	/// Utils for schemas.
	/// </summary>
	public sealed class SchemaUtils
	{

		private static readonly sbyte[] KEY_VALUE_SCHEMA_IS_PRIMITIVE = new sbyte[0];

		private const string KEY_VALUE_SCHEMA_NULL_STRING = "\"\"";

		private SchemaUtils()
		{
		}

		/// <summary>
		/// Keeps a map between <seealso cref="SchemaType"/> to a list of java classes that can be used to represent them.
		/// </summary>
		private static readonly IDictionary<SchemaType, IList<Type>> SCHEMA_TYPE_CLASSES = new Dictionary<SchemaType, IList<Type>>();

		/// <summary>
		/// Maps the java classes to the corresponding <seealso cref="SchemaType"/>.
		/// </summary>
		private static readonly IDictionary<Type, SchemaType> JAVA_CLASS_SCHEMA_TYPES = new Dictionary<Type, SchemaType>();

		static SchemaUtils()
		{
			// int8
			SCHEMA_TYPE_CLASSES[SchemaType.INT8] = Arrays.asList(typeof(Byte));
			// int16
			SCHEMA_TYPE_CLASSES[SchemaType.INT16] = Arrays.asList(typeof(Short));
			// int32
			SCHEMA_TYPE_CLASSES[SchemaType.INT32] = Arrays.asList(typeof(Integer));
			// int64
			SCHEMA_TYPE_CLASSES[SchemaType.INT64] = Arrays.asList(typeof(Long));
			// float
			SCHEMA_TYPE_CLASSES[SchemaType.FLOAT] = Arrays.asList(typeof(Float));
			// double
			SCHEMA_TYPE_CLASSES[SchemaType.DOUBLE] = Arrays.asList(typeof(Double));
			// boolean
			SCHEMA_TYPE_CLASSES[SchemaType.BOOLEAN] = Arrays.asList(typeof(Boolean));
			// string
			SCHEMA_TYPE_CLASSES[SchemaType.STRING] = Arrays.asList(typeof(string));
			// bytes
			SCHEMA_TYPE_CLASSES[SchemaType.BYTES] = Arrays.asList(typeof(sbyte[]), typeof(ByteBuffer), typeof(ByteBuf));
			// build the reverse mapping
			SCHEMA_TYPE_CLASSES.forEach((type, classes) => classes.forEach(clz => JAVA_CLASS_SCHEMA_TYPES.put(clz, type)));
		}

		public static void validateFieldSchema(string name, SchemaType type, object val)
		{
			if (null == val)
			{
				return;
			}

			IList<Type> expectedClasses = SCHEMA_TYPE_CLASSES[type];

			if (null == expectedClasses)
			{
				throw new Exception("Invalid Java object for schema type " + type + " : " + val.GetType() + " for field : \"" + name + "\"");
			}

			bool foundMatch = false;
			foreach (Type expectedCls in expectedClasses)
			{
				if (expectedCls.IsInstanceOfType(val))
				{
					foundMatch = true;
					break;
				}
			}

			if (!foundMatch)
			{
				throw new Exception("Invalid Java object for schema type " + type + " : " + val.GetType() + " for field : \"" + name + "\"");
			}

			switch (type)
			{
				case INT8:
				case INT16:
				case PROTOBUF:
				case AVRO:
				case AUTO_CONSUME:
				case AUTO_PUBLISH:
				case AUTO:
				case KEY_VALUE:
				case JSON:
				case NONE:
					throw new Exception("Currently " + type.name() + " is not supported");
				default:
					break;
			}
		}

		public static object toAvroObject(object value)
		{
			if (value != null)
			{
				if (value is ByteBuffer)
				{
					ByteBuffer bb = (ByteBuffer) value;
					sbyte[] bytes = new sbyte[bb.remaining()];
					bb.duplicate().get(bytes);
					return bytes;
				}
				else if (value is ByteBuf)
				{
					return ByteBufUtil.getBytes((ByteBuf) value);
				}
				else
				{
					return value;
				}
			}
			else
			{
				return null;
			}
		}

		public static string getStringSchemaVersion(sbyte[] schemaVersionBytes)
		{
			if (null == schemaVersionBytes)
			{
				return "NULL";
			}
			else if (schemaVersionBytes.Length == Long.BYTES || schemaVersionBytes.Length == (sizeof(long) * 8))
			{
				ByteBuffer bb = ByteBuffer.wrap(schemaVersionBytes);
				return bb.Long.ToString();
			}
			else if (schemaVersionBytes.Length == 0)
			{
				return "EMPTY";
			}
			else
			{
				return Base64.Encoder.encodeToString(schemaVersionBytes);
			}

		}

		/// <summary>
		/// Jsonify the schema info.
		/// </summary>
		/// <param name="schemaInfo"> the schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifySchemaInfo(SchemaInfo schemaInfo)
		{
			GsonBuilder gsonBuilder = (new GsonBuilder()).setPrettyPrinting().registerTypeHierarchyAdapter(typeof(sbyte[]), new ByteArrayToStringAdapter(schemaInfo)).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);

			return gsonBuilder.create().toJson(schemaInfo);
		}

		/// <summary>
		/// Jsonify the schema info with verison.
		/// </summary>
		/// <param name="schemaInfoWithVersion"> the schema info </param>
		/// <returns> the jsonified schema info with version </returns>
		public static string jsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
			GsonBuilder gsonBuilder = (new GsonBuilder()).setPrettyPrinting().registerTypeHierarchyAdapter(typeof(SchemaInfo), SCHEMAINFO_ADAPTER).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);

			return gsonBuilder.create().toJson(schemaInfoWithVersion);
		}

		private class SchemaPropertiesSerializer : JsonSerializer<IDictionary<string, string>>
		{

			public override JsonElement serialize(IDictionary<string, string> properties, Type type, JsonSerializationContext jsonSerializationContext)
			{
				SortedDictionary<string, string> sortedProperties = new SortedDictionary<string, string>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				sortedProperties.putAll(properties);
				JsonObject @object = new JsonObject();
				sortedProperties.forEach((key, value) =>
				{
				@object.add(key, new JsonPrimitive(value));
				});
				return @object;
			}

		}

		private class SchemaPropertiesDeserializer : JsonDeserializer<IDictionary<string, string>>
		{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.Map<String, String> deserialize(com.google.gson.JsonElement jsonElement, Type type, com.google.gson.JsonDeserializationContext jsonDeserializationContext) throws com.google.gson.JsonParseException
			public override IDictionary<string, string> deserialize(JsonElement jsonElement, Type type, JsonDeserializationContext jsonDeserializationContext)
			{

				SortedDictionary<string, string> sortedProperties = new SortedDictionary<string, string>();
				jsonElement.AsJsonObject.entrySet().forEach(entry => sortedProperties.put(entry.Key, entry.Value.AsString));
				return sortedProperties;
			}

		}

		private static readonly SchemaPropertiesSerializer SCHEMA_PROPERTIES_SERIALIZER = new SchemaPropertiesSerializer();

		private static readonly SchemaPropertiesDeserializer SCHEMA_PROPERTIES_DESERIALIZER = new SchemaPropertiesDeserializer();

		private class ByteArrayToStringAdapter : JsonSerializer<sbyte[]>
		{

			internal readonly SchemaInfo schemaInfo;

			public ByteArrayToStringAdapter(SchemaInfo schemaInfo)
			{
				this.schemaInfo = schemaInfo;
			}

			public virtual JsonElement serialize(sbyte[] src, Type typeOfSrc, JsonSerializationContext context)
			{
				string schemaDef = schemaInfo.SchemaDefinition;
				SchemaType type = schemaInfo.Type;
				switch (type)
				{
					case AVRO:
					case JSON:
					case PROTOBUF:
						return toJsonObject(schemaInfo.SchemaDefinition);
					case KEY_VALUE:
						KeyValue<SchemaInfo, SchemaInfo> schemaInfoKeyValue = DefaultImplementation.decodeKeyValueSchemaInfo(schemaInfo);
						JsonObject obj = new JsonObject();
						string keyJson = jsonifySchemaInfo(schemaInfoKeyValue.Key);
						string valueJson = jsonifySchemaInfo(schemaInfoKeyValue.Value);
						obj.add("key", toJsonObject(keyJson));
						obj.add("value", toJsonObject(valueJson));
						return obj;
					default:
						return new JsonPrimitive(schemaDef);
				}
			}
		}

		public static JsonObject toJsonObject(string json)
		{
			JsonParser parser = new JsonParser();
			return parser.parse(json).AsJsonObject;
		}

		private class SchemaInfoToStringAdapter : JsonSerializer<SchemaInfo>
		{

			public override JsonElement serialize(SchemaInfo schemaInfo, Type type, JsonSerializationContext jsonSerializationContext)
			{
				return toJsonObject(jsonifySchemaInfo(schemaInfo));
			}
		}

		private static readonly SchemaInfoToStringAdapter SCHEMAINFO_ADAPTER = new SchemaInfoToStringAdapter();

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string jsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			GsonBuilder gsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(SchemaInfo), SCHEMAINFO_ADAPTER).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);
			return gsonBuilder.create().toJson(kvSchemaInfo);
		}

		/// <summary>
		/// convert the key/value schema info data to string
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert schema info data string </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String convertKeyValueSchemaInfoDataToString(org.apache.pulsar.common.schema.KeyValue<org.apache.pulsar.common.schema.SchemaInfo, org.apache.pulsar.common.schema.SchemaInfo> kvSchemaInfo) throws java.io.IOException
		public static string convertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			ObjectMapper objectMapper = ObjectMapperFactory.create();
			KeyValue<object, object> keyValue = new KeyValue<object, object>(SchemaType.isPrimitiveType(kvSchemaInfo.Key.Type) ? "" : objectMapper.readTree(kvSchemaInfo.Key.Schema), SchemaType.isPrimitiveType(kvSchemaInfo.Value.Type) ? "" : objectMapper.readTree(kvSchemaInfo.Value.Schema));
			return objectMapper.writeValueAsString(keyValue);
		}

		private static sbyte[] getKeyOrValueSchemaBytes(JsonElement jsonElement)
		{
			return KEY_VALUE_SCHEMA_NULL_STRING.Equals(jsonElement.ToString()) ? KEY_VALUE_SCHEMA_IS_PRIMITIVE : jsonElement.ToString().GetBytes(UTF_8);
		}

		/// <summary>
		/// convert the key/value schema info data json bytes to key/value schema info data bytes
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static byte[] convertKeyValueDataStringToSchemaInfoSchema(byte[] keyValueSchemaInfoDataJsonBytes) throws java.io.IOException
		public static sbyte[] convertKeyValueDataStringToSchemaInfoSchema(sbyte[] keyValueSchemaInfoDataJsonBytes)
		{
			JsonObject jsonObject = toJsonObject(StringHelper.NewString(keyValueSchemaInfoDataJsonBytes, UTF_8));
			sbyte[] keyBytes = getKeyOrValueSchemaBytes(jsonObject.get("key"));
			sbyte[] valueBytes = getKeyOrValueSchemaBytes(jsonObject.get("value"));
			int dataLength = 4 + keyBytes.Length + 4 + valueBytes.Length;
			sbyte[] schema = new sbyte[dataLength];
			//record the key value schema respective length
			ByteBuf byteBuf = ByteBufAllocator.DEFAULT.heapBuffer(dataLength);
			byteBuf.writeInt(keyBytes.Length).writeBytes(keyBytes).writeInt(valueBytes.Length).writeBytes(valueBytes);
			byteBuf.readBytes(schema);
			return schema;
		}

		/// <summary>
		/// Serialize schema properties
		/// </summary>
		/// <param name="properties"> schema properties </param>
		/// <returns> the serialized schema properties </returns>
		public static string serializeSchemaProperties(IDictionary<string, string> properties)
		{
			GsonBuilder gsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_SERIALIZER);
			return gsonBuilder.create().toJson(properties);
		}

		/// <summary>
		/// Deserialize schema properties from a serialized schema properties.
		/// </summary>
		/// <param name="serializedProperties"> serialized properties </param>
		/// <returns> the deserialized properties </returns>
		public static IDictionary<string, string> deserializeSchemaProperties(string serializedProperties)
		{
			GsonBuilder gsonBuilder = (new GsonBuilder()).registerTypeHierarchyAdapter(typeof(System.Collections.IDictionary), SCHEMA_PROPERTIES_DESERIALIZER);
			return gsonBuilder.create().fromJson(serializedProperties, typeof(System.Collections.IDictionary));
		}

	}

}