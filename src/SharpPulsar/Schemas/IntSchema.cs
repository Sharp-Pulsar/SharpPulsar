﻿using SharpPulsar.Exceptions;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces.Schema;
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
namespace SharpPulsar.Schemas
{
    /// <summary>
    /// A schema for `Integer`.
    /// </summary>
    public class IntSchema : AbstractSchema<int>
	{

		private static readonly IntSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static IntSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT32",
				Type = SchemaType.INT32,
				Schema = new byte[0]
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

		public override byte[] Encode(int message)
		{
			return BitConverter.GetBytes(message.IntToBigEndian());
			//return new byte[] { (sbyte)((int)((uint)message >> 24)), (sbyte)((int)((uint)message >> 16)), (sbyte)((int)((uint)message >> 8)), (sbyte)message };
		}

		public override int Decode(byte[] bytes)
		{
			Validate(bytes);
			/*int value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			return value;*/
			return BitConverter.ToInt32(bytes, 0).IntFromBigEndian();
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