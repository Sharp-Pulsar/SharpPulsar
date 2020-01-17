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
	/// A schema for `Integer`.
	/// </summary>
	public class IntSchema : AbstractSchema<int>
	{

		private static readonly IntSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static IntSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("INT32").setType(SchemaType.INT32).setSchema(new sbyte[0]);
			INSTANCE = new IntSchema();
		}

		public static IntSchema Of()
		{
			return INSTANCE;
		}

		public void Validate(sbyte[] message)
		{
			if (message.Length != 4)
			{
				throw new SchemaSerializationException("Size of data received by IntSchema is not 4");
			}
		}

		public void Validate(IByteBuffer message)
		{
			if (message.ReadableBytes != 4)
			{
				throw new SchemaSerializationException("Size of data received by IntSchema is not 4");
			}
		}

		public sbyte[] Encode(int? message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)message >> 24)), (sbyte)((int)((uint)message >> 16)), (sbyte)((int)((uint)message >> 8)), message.Value};
			}
		}

		public int? Decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			Validate(bytes);
			int value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return value;
		}

		public override int Decode(IByteBuffer byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			Validate(byteBuf);
			int value = 0;

			for (int i = 0; i < 4; i++)
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