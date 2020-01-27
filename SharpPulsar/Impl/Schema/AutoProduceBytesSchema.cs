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
//	import static com.google.common.@base.Preconditions.checkState;

	using SharpPulsar.Api;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// Auto detect schema.
	/// </summary>
	public class AutoProduceBytesSchema<T> : ISchema<sbyte[]>
	{

		private bool requireSchemaValidation = true;
		private ISchema<T> schema;

		public AutoProduceBytesSchema()
		{
		}

		public AutoProduceBytesSchema(ISchema<T> Schema)
		{
			this.schema = Schema;
			SchemaInfo SchemaInfo = Schema.SchemaInfo;
			this.requireSchemaValidation = SchemaInfo != null && SchemaInfo.Type != SchemaType.BYTES && SchemaInfo.Type != SchemaType.NONE;
		}

		public virtual ISchema<T> Schema
		{
			set
			{
				this.schema = value;
				this.requireSchemaValidation = value.SchemaInfo != null && SchemaType.BYTES != value.SchemaInfo.Type && SchemaType.NONE != value.SchemaInfo.Type;
			}
		}

		private void EnsureSchemaInitialized()
		{
			checkState(SchemaInitialized(), "Schema is not initialized before used");
		}

		public virtual bool SchemaInitialized()
		{
			return schema != null;
		}

		public override void Validate(sbyte[] Message)
		{
			EnsureSchemaInitialized();

			schema.Validate(Message);
		}

		public override sbyte[] Encode(sbyte[] Message)
		{
			EnsureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify if the message can be decoded by the underlying schema
				schema.Validate(Message);
			}

			return Message;
		}

		public override sbyte[] Decode(sbyte[] Bytes, sbyte[] SchemaVersion)
		{
			EnsureSchemaInitialized();

			if (requireSchemaValidation)
			{
				// verify the message can be detected by the underlying schema
				schema.Decode(Bytes, SchemaVersion);
			}

			return Bytes;
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				EnsureSchemaInitialized();
    
				return schema.SchemaInfo;
			}
		}
	}

}