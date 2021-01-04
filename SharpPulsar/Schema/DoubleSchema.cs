using SharpPulsar.Common.Schema;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Schema;
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
	/// A schema for `Double`.
	/// </summary>
	public class DoubleSchema : AbstractSchema<double>
	{

		private static readonly DoubleSchema _instance;
		private static readonly ISchemaInfo _schemaInfo;

		static DoubleSchema()
		{
			var info = new SchemaInfo
			{
				Name = "Double",
				Type = SchemaType.DOUBLE,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new DoubleSchema();
		}

		public static DoubleSchema Of()
		{
			return _instance;
		}

		public override void Validate(sbyte[] message)
		{
			if (message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}


		public override sbyte[] Encode(double message)
		{
			long bits = System.BitConverter.DoubleToInt64Bits(message);
			return new sbyte[] { (sbyte)((long)((ulong)bits >> 56)), (sbyte)((long)((ulong)bits >> 48)), (sbyte)((long)((ulong)bits >> 40)), (sbyte)((long)((ulong)bits >> 32)), (sbyte)((long)((ulong)bits >> 24)), (sbyte)((long)((ulong)bits >> 16)), (sbyte)((long)((ulong)bits >> 8)), (sbyte)bits };
		}

		public override double Decode(sbyte[] bytes)
		{
			Validate(bytes);
			long value = 0;
			foreach (sbyte b in bytes)
			{
				value <<= 8;
				value |= b & 0xFF;
			}
			
			return System.BitConverter.Int64BitsToDouble(value);
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