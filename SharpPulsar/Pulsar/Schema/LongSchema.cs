using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Pulsar.Api.Schema;
using SharpPulsar.Shared;
using System;
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
namespace SharpPulsar.Pulsar.Schema
{
	/// <summary>
	/// A schema for `Long`.
	/// </summary>
	public class LongSchema : AbstractSchema
	{

		private static readonly LongSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static LongSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT64",
				Type = SchemaType.Int64,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new LongSchema();
		}

		public static LongSchema Of()
		{
			return _instance;
		}

		public override void Validate(sbyte[] message, Type returnType)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override void Validate(byte[] message, Type returnType)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override sbyte[] Encode(object data)
		{
			if (null == data)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)data >> 56)), (sbyte)((int)((uint)data >> 48)), (sbyte)((int)((uint)data >> 40)), (sbyte)((int)((uint)data >> 32)), (sbyte)((int)((uint)data >> 24)), (sbyte)((int)((uint)data >> 16)), (sbyte)((int)((uint)data >> 8)), (sbyte)data};
			}
		}

		public override object Decode(sbyte[] bytes, Type returnType)
		{
			if (null == bytes)
			{
				return null;
			}
			Validate(bytes, returnType);
			long value = 0L;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
                value |= b & 0xFF;
			}
			return value;
		}

		public override object Decode(byte[] byteBuf, Type returnType)
		{
			if (null == byteBuf)
			{
				return null;
			}
			Validate(byteBuf, returnType);
			long value = 0L;
			for (int i = 0; i < 8; i++)
			{
				value <<= 8;
				value |= byteBuf[i] & 0xFF;
			}

			return value;
		}

		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}

}