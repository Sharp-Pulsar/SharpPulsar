using DotNetty.Buffers;
using SharpPulsar.Common.Schema;
using SharpPulsar.Exception;
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

		public static LongSchema Of()
		{
			return INSTANCE;
		}

		public void Validate(sbyte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public void Validate(IByteBuffer message)
		{
			if (message.ReadableBytes != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public sbyte[] Encode(long? data)
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

		public long? Decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			Validate(bytes);
			long value = 0L;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return value;
		}

		public override long? Decode(IByteBuffer byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			Validate(byteBuf);
			long value = 0L;
			for (int i = 0; i < 8; i++)
			{
				value <<= 8;
				value |= byteBuf.GetByte(i) & 0xFF;
			}

			return value;
		}

		public SchemaInfo SchemaInfo
		{
			get
			{
				return SCHEMA_INFO;
			}
		}
	}

}