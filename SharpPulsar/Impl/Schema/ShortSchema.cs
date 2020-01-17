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

		public void Validate(sbyte[] message)
		{
			if (message.Length != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public void Validate(IByteBuffer message)
		{
			if (message.ReadableBytes != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public sbyte[] Encode(short? message)
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

		public short? Decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			Validate(bytes);
			short value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= (short)(b & 0xFF);
			}
			return value;
		}

		public short? Decode(IByteBuffer byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			Validate(byteBuf);
			short value = 0;

			for (int i = 0; i < 2; i++)
			{
				value <<= 8;
				value |= (short)(byteBuf.GetByte(i) & 0xFF);
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