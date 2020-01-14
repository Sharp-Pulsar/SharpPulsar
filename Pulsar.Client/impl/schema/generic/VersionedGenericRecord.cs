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
namespace org.apache.pulsar.client.impl.schema.generic
{
	using Field = org.apache.pulsar.client.api.schema.Field;
	using GenericRecord = org.apache.pulsar.client.api.schema.GenericRecord;

	/// <summary>
	/// A generic record carrying schema version.
	/// </summary>
	internal abstract class VersionedGenericRecord : GenericRecord
	{

		protected internal readonly sbyte[] schemaVersion;
		protected internal readonly IList<Field> fields;

		protected internal VersionedGenericRecord(sbyte[] schemaVersion, IList<Field> fields)
		{
			this.schemaVersion = schemaVersion;
			this.fields = fields;
		}

		public override sbyte[] SchemaVersion
		{
			get
			{
				return schemaVersion;
			}
		}

		public override IList<Field> Fields
		{
			get
			{
				return fields;
			}
		}

	}

}