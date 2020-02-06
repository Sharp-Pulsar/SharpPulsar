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

	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;
	using IRecordSchemaBuilder = Api.Schema.IRecordSchemaBuilder;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// The default implementation of <seealso cref="IRecordSchemaBuilder"/>.
	/// </summary>
	public class RecordSchemaBuilderImpl : IRecordSchemaBuilder
	{

		public const string NAMESPACE = "org.apache.pulsar.schema.record";
		public const string DefaultSchemaName = "PulsarDefault";

		private readonly string name;
		private readonly IDictionary<string, string> properties;
		private readonly IList<FieldSchemaBuilderImpl> fields = new List<FieldSchemaBuilderImpl>();
		private string doc;

		public RecordSchemaBuilderImpl(string Name)
		{
			this.name = Name;
			this.properties = new Dictionary<string, string>();
		}

		public override IRecordSchemaBuilder Property(string Name, string Val)
		{
			this.properties[Name] = Val;
			return this;
		}

		public override FieldSchemaBuilder Field(string FieldName)
		{
			FieldSchemaBuilderImpl Field = new FieldSchemaBuilderImpl(FieldName);
			fields.Add(Field);
			return Field;
		}

		public override FieldSchemaBuilder Field(string FieldName, IGenericSchema GenericSchema)
		{
			FieldSchemaBuilderImpl Field = new FieldSchemaBuilderImpl(FieldName, GenericSchema);
			fields.Add(Field);
			return Field;
		}

		public override IRecordSchemaBuilder Doc(string Doc)
		{
			this.doc = Doc;
			return this;
		}

		public override SchemaInfo Build(SchemaType SchemaType)
		{
			switch (SchemaType.innerEnumValue)
			{
				case SchemaType.InnerEnum.JSON:
				case SchemaType.InnerEnum.AVRO:
					break;
				default:
					throw new Exception("Currently only AVRO and JSON record schema is supported");
			}

			string SchemaNs = NAMESPACE;
			string SchemaName = DefaultSchemaName;
			if (!string.ReferenceEquals(name, null))
			{
				string[] Split = SplitName(name);
				SchemaNs = Split[0];
				SchemaName = Split[1];
			}

			org.apache.avro.Schema BaseSchema = org.apache.avro.Schema.createRecord(!string.ReferenceEquals(SchemaName, null) ? SchemaName : DefaultSchemaName, doc, SchemaNs, false);

			IList<org.apache.avro.Schema.Field> AvroFields = new List<org.apache.avro.Schema.Field>();
			foreach (FieldSchemaBuilderImpl Field in fields)
			{
				AvroFields.Add(Field.build());
			}

			BaseSchema.Fields = AvroFields;
			return new SchemaInfo(name, BaseSchema.ToString().GetBytes(UTF_8), SchemaType, properties);
		}

		/// <summary>
		/// Split a full dotted-syntax name into a namespace and a single-component name.
		/// </summary>
		private static string[] SplitName(string FullName)
		{
			string[] Result = new string[2];
			int IndexLastDot = FullName.LastIndexOf('.');
			if (IndexLastDot >= 0)
			{
				Result[0] = FullName.Substring(0, IndexLastDot);
				Result[1] = FullName.Substring(IndexLastDot + 1);
			}
			else
			{
				Result[0] = null;
				Result[1] = FullName;
			}
			return Result;
		}

	}

}