using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.Interceptor.Schema;
using SharpPulsar.Shared;
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
namespace SharpPulsar.Schema
{
	/// <summary>
	/// A schema for `Integer`.
	/// </summary>
	public class IntSchema : AbstractSchema<int?>
	{

		private static readonly IntSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static IntSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT32",
				Type = SchemaType.INT32,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new IntSchema();
		}

		public static IntSchema Of()
		{
			return _instance;
		}

		public override void Validate(byte[] message)
		{
			if (message.Length != 4)
			{
				throw new SchemaSerializationException("Size of data received by IntSchema is not 4");
			}
		}

		public override sbyte[] Encode(int? message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)message >> 24)), (sbyte)((int)((uint)message >> 16)), (sbyte)((int)((uint)message >> 8)), (sbyte)message.Value};
			}
		}

		public override int? Decode(byte[] bytes)
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

		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}

}