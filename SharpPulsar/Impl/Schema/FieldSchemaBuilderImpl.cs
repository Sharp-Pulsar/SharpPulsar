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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using JsonProperties = org.apache.avro.JsonProperties;
	using LogicalTypes = org.apache.avro.LogicalTypes;
	using Schema = org.apache.avro.Schema;
	using Field = org.apache.avro.Schema.Field;
	using SchemaBuilder = org.apache.avro.SchemaBuilder;
	using SharpPulsar.Api.Schema;
	using SharpPulsar.Api.Schema;
	using GenericAvroSchema = Generic.GenericAvroSchema;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// The default implementation of <seealso cref="FieldSchemaBuilder"/>.
	/// </summary>
	public class FieldSchemaBuilderImpl : FieldSchemaBuilder<FieldSchemaBuilderImpl>
	{

		private readonly string fieldName;

		private SchemaType type;
		private bool optional = false;
		private object defaultVal = null;
		private readonly IDictionary<string, string> properties = new Dictionary<string, string>();
		private string doc;
		private string[] aliases;

		private IGenericSchema genericSchema;

		public FieldSchemaBuilderImpl(string FieldName) : this(FieldName, null)
		{
		}

		public FieldSchemaBuilderImpl(string FieldName, IGenericSchema GenericSchema)
		{
			this.fieldName = FieldName;
			this.genericSchema = GenericSchema;
		}

		public override FieldSchemaBuilderImpl Property(string Name, string Val)
		{
			properties[Name] = Val;
			return this;
		}

		public override FieldSchemaBuilderImpl Doc(string Doc)
		{
			this.doc = Doc;
			return this;
		}

		public override FieldSchemaBuilderImpl Aliases(params string[] Aliases)
		{
			this.aliases = Aliases;
			return this;
		}

		public override FieldSchemaBuilderImpl Type(SchemaType Type)
		{
			this.type = Type;
			return this;
		}

		public override FieldSchemaBuilderImpl Optional()
		{
			optional = true;
			return this;
		}

		public override FieldSchemaBuilderImpl Required()
		{
			optional = false;
			return this;
		}

		public override FieldSchemaBuilderImpl DefaultValue(object Value)
		{
			defaultVal = Value;
			return this;
		}

		public virtual Schema.Field Build()
		{
			requireNonNull(type, "Schema type is not provided");
			// verify the default value and object
			SchemaUtils.ValidateFieldSchema(fieldName, type, defaultVal);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.avro.Schema baseSchema;
			Schema BaseSchema;
			switch (type.innerEnumValue)
			{
				case SchemaType.InnerEnum.INT32:
					BaseSchema = SchemaBuilder.builder().intType();
					break;
				case SchemaType.InnerEnum.INT64:
					BaseSchema = SchemaBuilder.builder().longType();
					break;
				case SchemaType.InnerEnum.STRING:
					BaseSchema = SchemaBuilder.builder().stringType();
					break;
				case SchemaType.InnerEnum.FLOAT:
					BaseSchema = SchemaBuilder.builder().floatType();
					break;
				case SchemaType.InnerEnum.DOUBLE:
					BaseSchema = SchemaBuilder.builder().doubleType();
					break;
				case SchemaType.InnerEnum.BOOLEAN:
					BaseSchema = SchemaBuilder.builder().booleanType();
					break;
				case SchemaType.InnerEnum.BYTES:
					BaseSchema = SchemaBuilder.builder().bytesType();
					break;
				// DATE, TIME, TIMESTAMP support from generic record
				case SchemaType.InnerEnum.DATE:
					BaseSchema = LogicalTypes.date().addToSchema(Schema.create(Schema.Type.INT));
					break;
				case SchemaType.InnerEnum.TIME:
					BaseSchema = LogicalTypes.timeMillis().addToSchema(Schema.create(Schema.Type.INT));
					break;
				case SchemaType.InnerEnum.TIMESTAMP:
					BaseSchema = LogicalTypes.timestampMillis().addToSchema(Schema.create(Schema.Type.LONG));
					break;
				case SchemaType.InnerEnum.AVRO:
					checkArgument(genericSchema.SchemaInfo.Type == SchemaType.AVRO, "The field is expected to be using AVRO schema but " + genericSchema.SchemaInfo.Type + " schema is found");
					GenericAvroSchema GenericAvroSchema = (GenericAvroSchema) genericSchema;
					BaseSchema = GenericAvroSchema.AvroSchema;
					break;
				default:
					throw new Exception("Schema `" + type + "` is not supported to be used as a field for now");
			}

			foreach (KeyValuePair<string, string> Entry in properties.SetOfKeyValuePairs())
			{
				BaseSchema.addProp(Entry.Key, Entry.Value);
			}

			if (null != aliases)
			{
				foreach (string Alias in aliases)
				{
					BaseSchema.addAlias(Alias);
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.avro.Schema finalSchema;
			Schema FinalSchema;
			if (optional)
			{
				if (defaultVal != null)
				{
					FinalSchema = SchemaBuilder.builder().unionOf().type(BaseSchema).and().nullType().endUnion();
				}
				else
				{
					FinalSchema = SchemaBuilder.builder().unionOf().nullType().and().type(BaseSchema).endUnion();
				}
			}
			else
			{
				FinalSchema = BaseSchema;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object finalDefaultValue;
			object FinalDefaultValue;
			if (defaultVal != null)
			{
				FinalDefaultValue = SchemaUtils.ToAvroObject(defaultVal);
			}
			else
			{
				if (optional)
				{
					FinalDefaultValue = JsonProperties.NULL_VALUE;
				}
				else
				{
					FinalDefaultValue = null;
				}
			}

			return new Schema.Field(fieldName, FinalSchema, doc, FinalDefaultValue);
		}

	}

}