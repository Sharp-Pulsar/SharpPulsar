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
    /// A schema for `Short`.
    /// </summary>
    public class ShortSchema : AbstractSchema<short>
	{

		private static readonly ShortSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static ShortSchema()
		{
			var info = new SchemaInfo
			{
				Name = "INT16",
				Type = SchemaType.INT16,
				Schema = new byte[0]
			};
			_schemaInfo = info;
			_instance = new ShortSchema();
		}

		public static ShortSchema Of()
		{
			return _instance;
		}

		public override void Validate(byte[] message)
		{
			if (message.Length != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public override byte[] Encode(short message)
		{
			return BitConverter.GetBytes(message.Int16ToBigEndian());
			//return new byte[] { (sbyte)((int)((uint)message >> 8)), (sbyte)message };
		}

		public override short Decode(byte[] bytes)
		{
			Validate(bytes);

			return BitConverter.ToInt16(bytes, 0).Int16FromBigEndian();
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