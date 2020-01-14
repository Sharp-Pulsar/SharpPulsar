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
//	import static com.google.common.@base.Preconditions.checkState;

	using Schema = org.apache.pulsar.client.api.Schema;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// Auto detect schema.
	/// </summary>
	public class AutoProduceBytesSchema<T> : Schema<sbyte[]>
	{

		private bool requireSchemaValidation = true;
		private Schema<T> schema;

		public AutoProduceBytesSchema()
		{
		}

		public AutoProduceBytesSchema(Schema<T> schema)
		{
			this.schema = schema;
			SchemaInfo schemaInfo = schema.SchemaInfo;
			this.requireSchemaValidation = schemaInfo != null && schemaInfo.Type != SchemaType.BYTES && schemaInfo.Type != SchemaType.NONE;
		}

		public virtual Schema<T> Schema
		{
			set
			{
				this.schema = value;
				this.requireSchemaValidation = value.SchemaInfo != null && SchemaType.BYTES != value.SchemaInfo.Type && SchemaType.NONE != value.SchemaInfo.Type;
			}
		}

		private void ensureSchemaInitialized()
		{
			checkState(schemaInitialized(), "Schema is not initialized before used");
		}

		public virtual bool schemaInitialized()
		{
			return schema != null;
		}

		public override void validate(sbyte[] message)
		{
			ensureSchemaInitialized();

			schema.validate(message);
		}

		public override sbyte[] encode(sbyte[] message)
		{
			ensureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify if the message can be decoded by the underlying schema
				schema.validate(message);
			}

			return message;
		}

		public override sbyte[] decode(sbyte[] bytes, sbyte[] schemaVersion)
		{
			ensureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify the message can be detected by the underlying schema
				schema.decode(bytes, schemaVersion);
			}

			return bytes;
		}

		public override SchemaInfo SchemaInfo
		{
			get
			{
				ensureSchemaInitialized();
    
				return schema.SchemaInfo;
			}
		}
	}

}