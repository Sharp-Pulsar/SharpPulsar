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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	using JsonProperties = org.apache.avro.JsonProperties;
	using LogicalTypes = org.apache.avro.LogicalTypes;
	using Schema = org.apache.avro.Schema;
	using Field = org.apache.avro.Schema.Field;
	using SchemaBuilder = org.apache.avro.SchemaBuilder;
	using FieldSchemaBuilder = org.apache.pulsar.client.api.schema.FieldSchemaBuilder;
	using GenericSchema = org.apache.pulsar.client.api.schema.GenericSchema;
	using GenericAvroSchema = org.apache.pulsar.client.impl.schema.generic.GenericAvroSchema;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// The default implementation of <seealso cref="FieldSchemaBuilder"/>.
	/// </summary>
	internal class FieldSchemaBuilderImpl : FieldSchemaBuilder<FieldSchemaBuilderImpl>
	{

		private readonly string fieldName;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		private SchemaType type_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		private bool optional_Conflict = false;
		private object defaultVal = null;
		private readonly IDictionary<string, string> properties = new Dictionary<string, string>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		private string doc_Conflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		private string[] aliases_Conflict;

		private GenericSchema genericSchema;

		internal FieldSchemaBuilderImpl(string fieldName) : this(fieldName, null)
		{
		}

		internal FieldSchemaBuilderImpl(string fieldName, GenericSchema genericSchema)
		{
			this.fieldName = fieldName;
			this.genericSchema = genericSchema;
		}

		public override FieldSchemaBuilderImpl property(string name, string val)
		{
			properties[name] = val;
			return this;
		}

		public override FieldSchemaBuilderImpl doc(string doc)
		{
			this.doc_Conflict = doc;
			return this;
		}

		public override FieldSchemaBuilderImpl aliases(params string[] aliases)
		{
			this.aliases_Conflict = aliases;
			return this;
		}

		public override FieldSchemaBuilderImpl type(SchemaType type)
		{
			this.type_Conflict = type;
			return this;
		}

		public override FieldSchemaBuilderImpl optional()
		{
			optional_Conflict = true;
			return this;
		}

		public override FieldSchemaBuilderImpl required()
		{
			optional_Conflict = false;
			return this;
		}

		public override FieldSchemaBuilderImpl defaultValue(object value)
		{
			defaultVal = value;
			return this;
		}

		internal virtual Schema.Field build()
		{
			requireNonNull(type_Conflict, "Schema type is not provided");
			// verify the default value and object
			SchemaUtils.validateFieldSchema(fieldName, type_Conflict, defaultVal);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.avro.Schema baseSchema;
			Schema baseSchema;
			switch (type_Conflict)
			{
				case INT32:
					baseSchema = SchemaBuilder.builder().intType();
					break;
				case INT64:
					baseSchema = SchemaBuilder.builder().longType();
					break;
				case STRING:
					baseSchema = SchemaBuilder.builder().stringType();
					break;
				case FLOAT:
					baseSchema = SchemaBuilder.builder().floatType();
					break;
				case DOUBLE:
					baseSchema = SchemaBuilder.builder().doubleType();
					break;
				case BOOLEAN:
					baseSchema = SchemaBuilder.builder().booleanType();
					break;
				case BYTES:
					baseSchema = SchemaBuilder.builder().bytesType();
					break;
				// DATE, TIME, TIMESTAMP support from generic record
				case DATE:
					baseSchema = LogicalTypes.date().addToSchema(Schema.create(Schema.Type.INT));
					break;
				case TIME:
					baseSchema = LogicalTypes.timeMillis().addToSchema(Schema.create(Schema.Type.INT));
					break;
				case TIMESTAMP:
					baseSchema = LogicalTypes.timestampMillis().addToSchema(Schema.create(Schema.Type.LONG));
					break;
				case AVRO:
					checkArgument(genericSchema.SchemaInfo.Type == SchemaType.AVRO, "The field is expected to be using AVRO schema but " + genericSchema.SchemaInfo.Type + " schema is found");
					GenericAvroSchema genericAvroSchema = (GenericAvroSchema) genericSchema;
					baseSchema = genericAvroSchema.AvroSchema;
					break;
				default:
					throw new Exception("Schema `" + type_Conflict + "` is not supported to be used as a field for now");
			}

			foreach (KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs())
			{
				baseSchema.addProp(entry.Key, entry.Value);
			}

			if (null != aliases_Conflict)
			{
				foreach (string alias in aliases_Conflict)
				{
					baseSchema.addAlias(alias);
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.avro.Schema finalSchema;
			Schema finalSchema;
			if (optional_Conflict)
			{
				if (defaultVal != null)
				{
					finalSchema = SchemaBuilder.builder().unionOf().type(baseSchema).and().nullType().endUnion();
				}
				else
				{
					finalSchema = SchemaBuilder.builder().unionOf().nullType().and().type(baseSchema).endUnion();
				}
			}
			else
			{
				finalSchema = baseSchema;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object finalDefaultValue;
			object finalDefaultValue;
			if (defaultVal != null)
			{
				finalDefaultValue = SchemaUtils.toAvroObject(defaultVal);
			}
			else
			{
				if (optional_Conflict)
				{
					finalDefaultValue = JsonProperties.NULL_VALUE;
				}
				else
				{
					finalDefaultValue = null;
				}
			}

			return new Schema.Field(fieldName, finalSchema, doc_Conflict, finalDefaultValue);
		}

	}

}