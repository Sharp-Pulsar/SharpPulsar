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
	public class LongSchema : AbstractSchema<long?>
	{

		private static readonly LongSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static LongSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT64",
				Type = SchemaType.INT64,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new LongSchema();
		}

		public static LongSchema Of()
		{
			return _instance;
		}

		public override void Validate(byte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override sbyte[] Encode(long? data)
		{
			if (null == data)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)data >> 56)), (sbyte)((int)((uint)data >> 48)), (sbyte)((int)((uint)data >> 40)), (sbyte)((int)((uint)data >> 32)), (sbyte)((int)((uint)data >> 24)), (sbyte)((int)((uint)data >> 16)), (sbyte)((int)((uint)data >> 8)), (sbyte)data.Value};
			}
		}

		public override long? Decode(byte[] bytes)
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

		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}

}