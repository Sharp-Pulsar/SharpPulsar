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
	/// A schema for `Short`.
	/// </summary>
	public class ShortSchema : AbstractSchema<short>
	{

		private static readonly ShortSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static ShortSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("INT16").setType(SchemaType.INT16).setSchema(new sbyte[0]);
			INSTANCE = new ShortSchema();
		}

		public static ShortSchema of()
		{
			return INSTANCE;
		}

		public override void validate(sbyte[] message)
		{
			if (message.Length != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public override void validate(ByteBuf message)
		{
			if (message.readableBytes() != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public override sbyte[] encode(short? message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)message >> 8)), message.Value};
			}
		}

		public override short? decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			validate(bytes);
			short value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= (short)(b & 0xFF);
			}
			return value;
		}

		public override short? decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			validate(byteBuf);
			short value = 0;

			for (int i = 0; i < 2; i++)
			{
				value <<= 8;
				value |= (short)(byteBuf.getByte(i) & 0xFF);
			}
			return value;
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