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

using DotNetty.Buffers;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Shared;

namespace SharpPulsar.Impl.Schema
{
    /// <summary>
	/// A schema for bytes array.
	/// </summary>
	public class BytesSchema : AbstractSchema<sbyte[]>
	{

		private static readonly BytesSchema Instance = new BytesSchema();
        public override IGenericSchema<IGenericRecord> Generic(SchemaInfo schemaInfo)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<sbyte[]> GetSchema(SchemaInfo schemaInfo)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<sbyte[]> AUTO_PRODUCE_BYTES<T1>(ISchema<T1> schema)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<sbyte[]> AutoProduceBytes()
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<IGenericRecord> AutoConsume()
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<IGenericRecord> Auto()
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<sbyte[]> Json(ISchemaDefinition<sbyte[]> schemaDefinition)
        {
            throw new System.NotImplementedException();
        }

        public override ISchema<sbyte[]> Json(sbyte[] pojo)
        {
            throw new System.NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            throw new System.NotImplementedException();
        }

        public override bool RequireFetchingSchemaInfo()
        {
            throw new System.NotImplementedException();
        }

        public override sbyte[] Decode(sbyte[] bytes, sbyte[] schemaVersion)
        {
            throw new System.NotImplementedException();
        }

        public override ISchemaInfo SchemaInfo {get;}
        public override bool SupportSchemaVersioning()
        {
            throw new System.NotImplementedException();
        }

        public override ISchemaInfoProvider SchemaInfoProvider
        {
            set => throw new System.NotImplementedException();
        }

        public BytesSchema()
		{
            var s = new SchemaInfo {Name = "Bytes", Type = SchemaType.BYTES, Schema = new sbyte[0]};
            SchemaInfo = s;
		}

		public static BytesSchema Of()
		{
			return Instance;
		}

		public override sbyte[] Encode(sbyte[] message)
		{
			return message;
		}

        public override void Validate(sbyte[] message)
        {
            throw new System.NotImplementedException();
        }

        public override sbyte[] Decode(sbyte[] bytes)
		{
			return bytes;
		}

		public override sbyte[] Decode(IByteBuffer byteBuf)
		{
			if (byteBuf == null)
			{
				return null;
			}
			int size = byteBuf.ReadableBytes;
			var bytes = new sbyte[size];

			byteBuf.ReadBytes((byte[])(object)bytes, 0, size);
			return bytes;
		}

	}

}