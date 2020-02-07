using System.Collections.Generic;
using System.Text.Json;
using App.Metrics.Concurrency;
using Google.Protobuf.Collections;
using System.Linq;

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
	using Field = Api.Schema.Field;

	/// <summary>
	/// Generic json record.
	/// </summary>
	public class GenericJsonRecord : VersionedGenericRecord
    {
        private readonly JsonDocument _jsonDocument;
		public GenericJsonRecord(sbyte[] schemaVersion, IList<Field> fields, JsonDocument jd) : base(schemaVersion, fields)
		{
			_jsonDocument = jd;
		}


		public override object GetField(string fieldName)
		{
			var fn = _jsonDocument.RootElement.EnumerateArray().Where(x => !string.IsNullOrWhiteSpace(x.GetProperty(fieldName).GetString()));
			if (fn.Any())
			{
				var idx = new AtomicInteger(0);
				IList<Field> fields = fn.ToList().Select(f => new Field(){Name = f.GetString(), Index = idx.GetAndIncrement() }).ToList();
				return new GenericJsonRecord(SchemaVersion, fields, _jsonDocument);
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

		public override object GetField(Field field)
		{
			throw new System.NotImplementedException();
		}
	}

}