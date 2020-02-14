using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DotNetty.Buffers;
using DotNetty.Codecs.Base64;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
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
	

	/// <summary>
	/// Utils for schemas.
	/// </summary>
	public sealed class SchemaUtils
	{

		private static readonly sbyte[] KeyValueSchemaIsPrimitive = new sbyte[0];

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
			SchemaTypeClasses[SchemaType.String] = new List<Type>{ typeof(string) };
			// bytes
			SchemaTypeClasses[SchemaType.BYTES] = new List<Type>{typeof(sbyte[]), typeof(ByteBuffer), typeof(IByteBuffer)};
			// build the reverse mapping
			SchemaTypeClasses.ToList().ForEach((x => x.Value.ToList().ForEach(clz => JavaClassSchemaTypes.Add(clz, x.Key))));
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
				case SchemaType.InnerEnum.AUTO_CONSUME:
				case SchemaType.InnerEnum.AUTO_PUBLISH:
				case SchemaType.InnerEnum.AUTO:
				case SchemaType.InnerEnum.KEY_VALUE:
				case SchemaType.InnerEnum.JSON:
				case SchemaType.InnerEnum.NONE:
					throw new System.Exception("Currently " + type.GetType().Name + " is not supported");
				default:
					break;
			}
		}

		public static string GetStringSchemaVersion(sbyte[] schemaVersionBytes)
		{
			if (null == schemaVersionBytes)
			{
				return "NULL";
			}
			else if (schemaVersionBytes.Length == sizeof(long) || schemaVersionBytes.Length == (sizeof(long) * 8))
			{
				var bb = new ByteBuffer(schemaVersionBytes);
				return bb.Long.ToString();
			}
			else if (schemaVersionBytes.Length == 0)
			{
				return "EMPTY";
			}
			else
			{
				return Convert.ToBase64String((byte[])(Array)schemaVersionBytes);
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
		public static string JsonifySchemaInfoWithVersion(SchemaInfoWithVersion schemaInfoWithVersion)
		{
            return JsonSerializer.Serialize(schemaInfoWithVersion, new JsonSerializerOptions { WriteIndented = true });
		}

		/// <summary>
		/// Jsonify the key/value schema info.
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the jsonified schema info </returns>
		public static string JsonifyKeyValueSchemaInfo(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
            return JsonSerializer.Serialize(kvSchemaInfo);
        }

		/// <summary>
		/// convert the key/value schema info data to string
		/// </summary>
		/// <param name="kvSchemaInfo"> the key/value schema info </param>
		/// <returns> the convert schema info data string </returns>
        public static string ConvertKeyValueSchemaInfoDataToString(KeyValue<SchemaInfo, SchemaInfo> kvSchemaInfo)
		{
			var objectMapper = new ObjectMapper();
			var keyValue = new KeyValue<object, object>(SchemaType.IsPrimitiveType(kvSchemaInfo.Key.Type) ? "" : objectMapper.WriteValueAsString(kvSchemaInfo.Key.Schema), SchemaType.IsPrimitiveType(kvSchemaInfo.Value.Type) ? "" : objectMapper.WriteValueAsString(kvSchemaInfo.Value.Schema));
			return objectMapper.WriteValueAsString(keyValue);
		}

		private static sbyte[] GetKeyOrValueSchemaBytes(JsonElement jsonElement)
		{
			return KeyValueSchemaNullString.Equals(jsonElement.ToString()) ? KeyValueSchemaIsPrimitive : (sbyte[])(object)Encoding.UTF8.GetBytes(jsonElement.ToString());
		}

		/// <summary>
		/// convert the key/value schema info data json bytes to key/value schema info data bytes
		/// </summary>
		/// <param name="keyValueSchemaInfoDataJsonBytes"> the key/value schema info data json bytes </param>
		/// <returns> the key/value schema info data bytes </returns>
		public static sbyte[] ConvertKeyValueDataStringToSchemaInfoSchema(sbyte[] keyValueSchemaInfoDataJsonBytes)
		{
			var jsonObject = JsonDocument.Parse(StringHelper.NewString(keyValueSchemaInfoDataJsonBytes, CharSet.Ansi.ToString()));
			var keyBytes = (byte[])(object)GetKeyOrValueSchemaBytes(jsonObject.RootElement.GetProperty("key"));
			var valueBytes = (byte[])(object)GetKeyOrValueSchemaBytes(jsonObject.RootElement.GetProperty("value"));
			var dataLength = 4 + keyBytes.Length + 4 + valueBytes.Length;
			var schema = new sbyte[dataLength];
			
			//record the key value schema respective length
			var byteBuf = PooledByteBufferAllocator.Default.HeapBuffer(dataLength);
			byteBuf.WriteInt(keyBytes.Length).WriteBytes(keyBytes).WriteInt(valueBytes.Length).WriteBytes(valueBytes);
			byteBuf.ReadBytes((byte[])(object)schema);
			return schema;
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

	}

}