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
	using Field = SharpPulsar.Api.Schema.Field;

	/// <summary>
	/// Generic json record.
	/// </summary>
	public class GenericJsonRecord : VersionedGenericRecord
	{

		internal virtual JsonNode {get;}

		public GenericJsonRecord(sbyte[] SchemaVersion, IList<Field> Fields, JsonNode Jn) : base(SchemaVersion, Fields)
		{
			this.JsonNode = Jn;
		}


		public override object GetField(string FieldName)
		{
			JsonNode Fn = JsonNode.get(FieldName);
			if (Fn.ContainerNode)
			{
				AtomicInteger Idx = new AtomicInteger(0);
				IList<Field> Fields = Lists.newArrayList(Fn.fieldNames()).Select(f => new Field(f, Idx.AndIncrement)).ToList();
				return new GenericJsonRecord(SchemaVersionConflict, Fields, Fn);
			}
			else if (Fn.Boolean)
			{
				return Fn.asBoolean();
			}
			else if (Fn.Int)
			{
				return Fn.asInt();
			}
			else if (Fn.FloatingPointNumber)
			{
				return Fn.asDouble();
			}
			else if (Fn.Double)
			{
				return Fn.asDouble();
			}
			else
			{
				return Fn.asText();
			}
		}
	}

}