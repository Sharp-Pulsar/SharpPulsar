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
	/// A schema for `Long`.
	/// </summary>
	public class LongSchema : AbstractSchema<long>
	{

		private static readonly LongSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static LongSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("INT64").setType(SchemaType.INT64).setSchema(new sbyte[0]);
			INSTANCE = new LongSchema();
		}

		public static LongSchema of()
		{
			return INSTANCE;
		}

		public override void validate(sbyte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override void validate(ByteBuf message)
		{
			if (message.readableBytes() != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override sbyte[] encode(long? data)
		{
			if (null == data)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)data >> 56)), (sbyte)((int)((uint)data >> 48)), (sbyte)((int)((uint)data >> 40)), (sbyte)((int)((uint)data >> 32)), (sbyte)((int)((uint)data >> 24)), (sbyte)((int)((uint)data >> 16)), (sbyte)((int)((uint)data >> 8)), data.Value};
			}
		}

		public override long? decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			validate(bytes);
			long value = 0L;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return value;
		}

		public override long? decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			validate(byteBuf);
			long value = 0L;
			for (int i = 0; i < 8; i++)
			{
				value <<= 8;
				value |= byteBuf.getByte(i) & 0xFF;
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