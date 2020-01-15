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
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// A schema for 'Byte'.
	/// </summary>
	public class ByteSchema : AbstractSchema<sbyte>
	{

		private static readonly ByteSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static ByteSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("INT8").setType(SchemaType.INT8).setSchema(new sbyte[0]);
			INSTANCE = new ByteSchema();
		}

		public static ByteSchema of()
		{
			return INSTANCE;
		}

		public override void validate(sbyte[] message)
		{
			if (message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by ByteSchema is not 1");
			}
		}

		public override void validate(ByteBuf message)
		{
			if (message.readableBytes() != 1)
			{
				throw new SchemaSerializationException("Size of data received by ByteSchema is not 1");
			}
		}

		public override sbyte[] encode(sbyte? message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return new sbyte[]{message};
			}
		}

		public override sbyte? decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			validate(bytes);
			return bytes[0];
		}

		public override sbyte? decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			validate(byteBuf);
			return byteBuf.getByte(0);
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