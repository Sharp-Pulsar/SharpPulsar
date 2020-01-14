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
	/// A schema for `Double`.
	/// </summary>
	public class DoubleSchema : AbstractSchema<double>
	{

		private static readonly DoubleSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static DoubleSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("Double").setType(SchemaType.DOUBLE).setSchema(new sbyte[0]);
			INSTANCE = new DoubleSchema();
		}

		public static DoubleSchema of()
		{
			return INSTANCE;
		}

		public override void validate(sbyte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}

		public override void validate(ByteBuf message)
		{
			if (message.readableBytes() != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}


		public override sbyte[] encode(double? message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				long bits = System.BitConverter.DoubleToInt64Bits(message);
				return new sbyte[] {(sbyte)((long)((ulong)bits >> 56)), (sbyte)((long)((ulong)bits >> 48)), (sbyte)((long)((ulong)bits >> 40)), (sbyte)((long)((ulong)bits >> 32)), (sbyte)((long)((ulong)bits >> 24)), (sbyte)((long)((ulong)bits >> 16)), (sbyte)((long)((ulong)bits >> 8)), (sbyte) bits};
			}
		}

		public override double? decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			validate(bytes);
			long value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return Double.longBitsToDouble(value);
		}

		public override double? decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			validate(byteBuf);
			long value = 0;

			for (int i = 0; i < 8; i++)
			{
				value <<= 8;
				value |= byteBuf.getByte(i) & 0xFF;
			}
			return Double.longBitsToDouble(value);
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