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
namespace SharpPulsar.Impl.Schema.Generic
{
	using JsonNode = com.fasterxml.jackson.databind.JsonNode;
	using Lists = com.google.common.collect.Lists;
	using Field = Api.Schema.Field;

	/// <summary>
	/// Generic json record.
	/// </summary>
	internal class GenericJsonRecord : VersionedGenericRecord
	{

		private readonly JsonNode jn;

		internal GenericJsonRecord(sbyte[] schemaVersion, IList<Field> fields, JsonNode jn) : base(schemaVersion, fields)
		{
			this.jn = jn;
		}

		internal virtual JsonNode JsonNode
		{
			get
			{
				return jn;
			}
		}

		public object GetField(string fieldName)
		{
			JsonNode fn = jn.get(fieldName);
			if (fn.ContainerNode)
			{
				AtomicInteger idx = new AtomicInteger(0);
				IList<Field> fields = Lists.newArrayList(fn.fieldNames()).Select(f => new Field(f, idx.AndIncrement)).ToList();
				return new GenericJsonRecord(schemaVersion, fields, fn);
			}
			else if (fn.Boolean)
			{
				return fn.asBoolean();
			}
			else if (fn.Int)
			{
				return fn.asInt();
			}
			else if (fn.FloatingPointNumber)
			{
				return fn.asDouble();
			}
			else if (fn.Double)
			{
				return fn.asDouble();
			}
			else
			{
				return fn.asText();
			}
		}
	}

}