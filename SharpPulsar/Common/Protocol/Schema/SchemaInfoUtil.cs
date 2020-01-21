﻿using SharpPulsar.Common.Schema;
using System;
using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Common.Protocol.Schema
{

	/// <summary>
	/// Class helping to initialize schemas.
	/// </summary>
	public class SchemaInfoUtil
	{

		public static SchemaInfo NewSchemaInfo(string name, SchemaData data)
		{
			SchemaInfo si = new SchemaInfo
			{
				Name = name,
				Schema = data.Data,
				Type = data.Type,
				Properties = data.Props
			};
			return si;
		}

		public static SchemaInfo NewSchemaInfo(Schema schema)
		{
			SchemaInfo si = new SchemaInfo
			{
				Name = schema.Name,
				Schema = (sbyte[])(Array)schema.SchemaData,
				Type = Commands.GetSchemaType(schema.type)
			};
			if (schema.Properties.Count == 0)
			{
				si.Properties = new Dictionary<string, string>();
			}
			else
			{
				si.Properties = new SortedDictionary<string,  string>();
				for (int i = 0; i < schema.Properties.Count; i++)
				{
					KeyValue kv = schema.Properties[i];
					si.Properties.Add(kv.Key, kv.Value);
				}
			}
			return si;
		}

		public static SchemaInfo NewSchemaInfo(string name, GetSchemaResponse schema)
		{
			SchemaInfo si = new SchemaInfo
			{
				Name = name,
				Schema = (sbyte[])(Array)Encoding.UTF8.GetBytes(schema.Data),
				Type = schema.Type,
				Properties = schema.Properties
			};
			return si;
		}
	}

}