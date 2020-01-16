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
namespace org.apache.pulsar.common.protocol.schema
{
	using UtilityClass = lombok.experimental.UtilityClass;
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using Schema = org.apache.pulsar.common.api.proto.PulsarApi.Schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;

	/// <summary>
	/// Class helping to initialize schemas.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UtilityClass public class SchemaInfoUtil
	public class SchemaInfoUtil
	{

		public static SchemaInfo newSchemaInfo(string name, SchemaData data)
		{
			SchemaInfo si = new SchemaInfo();
			si.Name = name;
			si.Schema = data.Data;
			si.Type = data.Type;
			si.Properties = data.Props;
			return si;
		}

		public static SchemaInfo newSchemaInfo(PulsarApi.Schema schema)
		{
			SchemaInfo si = new SchemaInfo();
			si.Name = schema.Name;
			si.Schema = schema.SchemaData.toByteArray();
			si.Type = Commands.getSchemaType(schema.getType());
			if (schema.PropertiesCount == 0)
			{
				si.Properties = Collections.emptyMap();
			}
			else
			{
				si.Properties = new SortedDictionary<>();
				for (int i = 0; i < schema.PropertiesCount; i++)
				{
					PulsarApi.KeyValue kv = schema.getProperties(i);
					si.Properties.put(kv.Key, kv.Value);
				}
			}
			return si;
		}

		public static SchemaInfo newSchemaInfo(string name, GetSchemaResponse schema)
		{
			SchemaInfo si = new SchemaInfo();
			si.Name = name;
			si.Schema = schema.Data.Bytes;
			si.Type = schema.Type;
			si.Properties = schema.Properties;
			return si;
		}
	}

}