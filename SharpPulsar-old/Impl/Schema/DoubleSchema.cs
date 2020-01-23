﻿using DotNetty.Buffers;
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

		public static DoubleSchema Of()
		{
			return INSTANCE;
		}

		public void Validate(sbyte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}

		public void Validate(IByteBuffer message)
		{
			if (message.ReadableBytes != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}


		public sbyte[] Encode(double? message)
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

		public double? Decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			Validate(bytes);
			long value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return Double.longBitsToDouble(value);
		}

		public override double Decode(IByteBuffer byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			Validate(byteBuf);
			long value = 0;

			for (int i = 0; i < 8; i++)
			{
				value <<= 8;
				value |= byteBuf.GetByte(i) & 0xFF;
			}
			return Double.longBitsToDouble(value);
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