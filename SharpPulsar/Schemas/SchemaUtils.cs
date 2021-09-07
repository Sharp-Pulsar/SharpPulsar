using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using AvroSchemaGenerator;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Builder;
using SharpPulsar.Shared;
using SharpPulsar.Interfaces.ISchema;

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
namespace SharpPulsar.Schemas
{


    /// <summary>
    /// Utils for schemas.
    /// </summary>
    public sealed class SchemaUtils
	{

		private static readonly byte[] KeyValueSchemaIsPrimitive = new byte[0];

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
			// string
			SchemaTypeClasses[SchemaType.STRING] = new List<Type>{ typeof(string) };
			// bytes
			SchemaTypeClasses[SchemaType.BYTES] = new List<Type>{typeof(byte[]), typeof(byte[])};
			// build the reverse mapping
			SchemaTypeClasses.ToList().ForEach((x => x.Value.ToList().ForEach(clz => JavaClassSchemaTypes.TryAdd(clz, x.Key))));
		}

		public static void ValidateFieldSchema(string name, SchemaType type, object val)
		{
			if (null == val)
			{
				return;
			}

			var expectedClasses = SchemaTypeClasses[type];

			if (null == expectedClasses)
			{
				throw new System.Exception("Invalid Java object for schema type " + type + " : " + val.GetType() + @" for field : """ + name + @"""");
			}

			var foundMatch = false;
			foreach (var expectedCls in expectedClasses)
			{
				if (expectedCls.IsInstanceOfType(val))
				{
					foundMatch = true;
					break;
				}
			}

			if (!foundMatch)
			{
				throw new System.Exception("Invalid Java object for schema type " + type + " : " + val.GetType() + @" for field : """ + name + @"""");
			}

			switch (type.InnerEnumValue)
			{
				case SchemaType.InnerEnum.INT8:
				case SchemaType.InnerEnum.INT16:
				case SchemaType.InnerEnum.PROTOBUF:
				case SchemaType.InnerEnum.AVRO:
				case SchemaType.InnerEnum.AutoConsume:
				case SchemaType.InnerEnum.AutoPublish:
				case SchemaType.InnerEnum.AUTO:
				case SchemaType.InnerEnum.KeyValue:
				case SchemaType.InnerEnum.JSON:
				case SchemaType.InnerEnum.NONE:
					throw new System.Exception("Currently " + type.GetType().Name + " is not supported");
				default:
					break;
			}
		}

		public static string GetStringSchemaVersion(byte[] schemaVersionBytes)
		{
			if (null == schemaVersionBytes)
			{
				return "NULL";
			}

            if (schemaVersionBytes.Length == sizeof(long) || schemaVersionBytes.Length == (sizeof(long) * 8))
            {
                var bb = ByteBuffer.Allocate(schemaVersionBytes.Length).Wrap(schemaVersionBytes);
                return bb.GetLong().ToString();
            }
            if (schemaVersionBytes.Length == 0)
            {
                return "EMPTY";
            }
            return Convert.ToBase64String(schemaVersionBytes);
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
		public static string JsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
            return JsonSerializer.Serialize(schemaInfoWithVersion, new JsonSerializerOptions { WriteIndented = true });
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifyKeyValueSchemaInfo(KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo)
		{
            return JsonSerializer.Serialize(kvSchemaInfo);
        }

		/// <summary>
		/// convert the key/value schema info data to string
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert schema info data string </returns>
        public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<ISchemaInfo, ISchemaInfo> kvSchemaInfo)
		{
			var keyValue = new KeyValue<object, object>(SchemaType.IsPrimitiveType(kvSchemaInfo.Key.Type) ? "" : JsonSerializer.Serialize(kvSchemaInfo.Key.Schema), SchemaType.IsPrimitiveType(kvSchemaInfo.Value.Type) ? "" : JsonSerializer.Serialize(kvSchemaInfo.Value.Schema));
			return JsonSerializer.Serialize(keyValue);
		}

		private static byte[] GetKeyOrValueSchemaBytes(JsonElement jsonElement)
		{
			return KeyValueSchemaNullString.Equals(jsonElement.ToString()) ? KeyValueSchemaIsPrimitive : Encoding.UTF8.GetBytes(jsonElement.ToString());
		}
		
		/// <summary>
		/// Serialize schema properties
		/// </summary>
		/// <param name="properties"> schema properties </param>
		/// <returns> the serialized schema properties </returns>
		public static string SerializeSchemaProperties(IDictionary<string, string> properties)
		{
            return JsonSerializer.Serialize(properties);
		}

		/// <summary>
		/// Deserialize schema properties from a serialized schema properties.
		/// </summary>
		/// <param name="serializedProperties"> serialized properties </param>
		/// <returns> the deserialized properties </returns>
		public static IDictionary<string, string> DeserializeSchemaProperties(string serializedProperties)
        {
            return JsonSerializer.Deserialize<IDictionary<string, string>>(serializedProperties);
        }
		public static bool GetJsr310ConversionEnabledFromSchemaInfo<T>(SchemaInfo schemaInfo)
		{
			if (schemaInfo != null)
			{
				if(schemaInfo.Properties.TryGetValue(SchemaDefinitionBuilderImpl<T>.Jsr310ConversionEnabled, out var st))
				  return bool.Parse(st);
			}
			return false;
		}
		public static Avro.Schema CreateAvroSchema<T>(ISchemaDefinition<T> schemaDefinition)
		{
			var pojo = schemaDefinition.Pojo;

			if (!string.IsNullOrWhiteSpace(schemaDefinition.JsonDef))
			{
				return ParseAvroSchema(schemaDefinition.JsonDef);
			}

			var schema = pojo.GetSchema();
			return ParseAvroSchema(schema);

		}

		public static Avro.Schema ParseAvroSchema(string schemaJson)
		{
			return Avro.Schema.Parse(schemaJson);
		}

        public static ISchemaInfo ParseSchemaInfo<T>(ISchemaDefinition<T> schemaDefinition, SchemaType schemaType)
		{
			return new SchemaInfoBuilder().SetSchema(CreateAvroSchema(schemaDefinition).ToString().GetBytes()).SetProperties(schemaDefinition.Properties).SetName("").SetType(schemaType).Build();
		}
		public static Avro.Schema ExtractAvroSchema<T>(ISchemaDefinition<T> schemaDefinition, Type pojo)
		{
			try
			{
				return ParseAvroSchema(pojo.GetField("SCHEMA$").GetValue(null).ToString());
			}
			catch (Exception ignored) when (ignored is ArgumentException)
			{
				//return schemaDefinition.AlwaysAllowNull ? ReflectData.AllowNull.get().getSchema(pojo) : ReflectData.get().getSchema(pojo);
				return Avro.Schema.Parse(pojo.GetSchema());
			}
		}
	}

}