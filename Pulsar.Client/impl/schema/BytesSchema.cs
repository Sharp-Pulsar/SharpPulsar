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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// A schema for bytes array.
	/// </summary>
	public class BytesSchema : AbstractSchema<sbyte[]>
	{

		private static readonly BytesSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static BytesSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("Bytes").setType(SchemaType.BYTES).setSchema(new sbyte[0]);
			INSTANCE = new BytesSchema();
		}

		public static BytesSchema of()
		{
			return INSTANCE;
		}

		public override sbyte[] encode(sbyte[] message)
		{
			return message;
		}

		public override sbyte[] decode(sbyte[] bytes)
		{
			return bytes;
		}

		public override sbyte[] decode(ByteBuf byteBuf)
		{
			if (byteBuf == null)
			{
				return null;
			}
			int size = byteBuf.readableBytes();
			sbyte[] bytes = new sbyte[size];

			byteBuf.readBytes(bytes, 0, size);
			return bytes;
		}

		public override SchemaInfo SchemaInfo
		{
			get
			{
				return SCHEMA_INFO;
			}
		}
	}

}