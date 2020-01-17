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
namespace SharpPulsar.Impl.Schema
{

	using FieldSchemaBuilder = org.apache.pulsar.client.api.schema.FieldSchemaBuilder;
	using GenericSchema = org.apache.pulsar.client.api.schema.GenericSchema;
	using RecordSchemaBuilder = org.apache.pulsar.client.api.schema.RecordSchemaBuilder;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// The default implementation of <seealso cref="RecordSchemaBuilder"/>.
	/// </summary>
	public class RecordSchemaBuilderImpl : RecordSchemaBuilder
	{

		public const string NAMESPACE = "org.apache.pulsar.schema.record";
		public const string DEFAULT_SCHEMA_NAME = "PulsarDefault";

		private readonly string name;
		private readonly IDictionary<string, string> properties;
		private readonly IList<FieldSchemaBuilderImpl> fields = new List<FieldSchemaBuilderImpl>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private string doc_Conflict;

		public RecordSchemaBuilderImpl(string name)
		{
			this.name = name;
			this.properties = new Dictionary<string, string>();
		}

		public override RecordSchemaBuilder property(string name, string val)
		{
			this.properties[name] = val;
			return this;
		}

		public override FieldSchemaBuilder field(string fieldName)
		{
			FieldSchemaBuilderImpl field = new FieldSchemaBuilderImpl(fieldName);
			fields.Add(field);
			return field;
		}

		public override FieldSchemaBuilder field(string fieldName, GenericSchema genericSchema)
		{
			FieldSchemaBuilderImpl field = new FieldSchemaBuilderImpl(fieldName, genericSchema);
			fields.Add(field);
			return field;
		}

		public override RecordSchemaBuilder doc(string doc)
		{
			this.doc_Conflict = doc;
			return this;
		}

		public override SchemaInfo build(SchemaType schemaType)
		{
			switch (schemaType)
			{
				case JSON:
				case AVRO:
					break;
				default:
					throw new Exception("Currently only AVRO and JSON record schema is supported");
			}

			string schemaNs = NAMESPACE;
			string schemaName = DEFAULT_SCHEMA_NAME;
			if (!string.ReferenceEquals(name, null))
			{
				string[] split = splitName(name);
				schemaNs = split[0];
				schemaName = split[1];
			}

			org.apache.avro.Schema baseSchema = org.apache.avro.Schema.createRecord(!string.ReferenceEquals(schemaName, null) ? schemaName : DEFAULT_SCHEMA_NAME, doc_Conflict, schemaNs, false);

			IList<org.apache.avro.Schema.Field> avroFields = new List<org.apache.avro.Schema.Field>();
			foreach (FieldSchemaBuilderImpl field in fields)
			{
				avroFields.Add(field.build());
			}

			baseSchema.Fields = avroFields;
			return new SchemaInfo(name, baseSchema.ToString().GetBytes(UTF_8), schemaType, properties);
		}

		/// <summary>
		/// Split a full dotted-syntax name into a namespace and a single-component name.
		/// </summary>
		private static string[] splitName(string fullName)
		{
			string[] result = new string[2];
			int indexLastDot = fullName.LastIndexOf('.');
			if (indexLastDot >= 0)
			{
				result[0] = fullName.Substring(0, indexLastDot);
				result[1] = fullName.Substring(indexLastDot + 1);
			}
			else
			{
				result[0] = null;
				result[1] = fullName;
			}
			return result;
		}

	}

}