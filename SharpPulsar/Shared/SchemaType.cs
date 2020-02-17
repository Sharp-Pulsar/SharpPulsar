using System.Collections.Generic;

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
namespace SharpPulsar.Shared
{
	/// <summary>
	/// Types of supported schema for Pulsar messages.
	/// 
	/// <para>Ideally we should have just one single set of enum definitions
	/// for schema type. but we have 3 locations of defining schema types.
	/// 
	/// </para>
	/// <para>when you are adding a new schema type that whose
	/// schema info is required to be recorded in schema registry,
	/// add corresponding schema type into `pulsar-common/src/main/proto/PulsarApi.proto`
	/// and `pulsar-broker/src/main/proto/SchemaRegistryFormat.proto`.
	/// </para>
	/// </summary>
	public sealed class SchemaType
	{
		/// <summary>
		/// No schema defined.
		/// </summary>
		public static readonly SchemaType None = new SchemaType("NONE", InnerEnum.None, 0);

		/// <summary>
		/// Simple String encoding with UTF-8.
		/// </summary>
		public static readonly SchemaType String = new SchemaType("STRING", InnerEnum.String, 1);

		/// <summary>
		/// JSON object encoding and validation.
		/// </summary>
		public static readonly SchemaType Json = new SchemaType("JSON", InnerEnum.Json, 2);

		/// <summary>
		/// Protobuf message encoding and decoding.
		/// </summary>
		public static readonly SchemaType Protobuf = new SchemaType("PROTOBUF", InnerEnum.Protobuf, 3);

		/// <summary>
		/// Serialize and deserialize via avro.
		/// </summary>
		public static readonly SchemaType Avro = new SchemaType("AVRO", InnerEnum.Avro, 4);

		/// <summary>
		/// boolean schema defined.
		/// @since 2.3.0
		/// </summary>
		public static readonly SchemaType Boolean = new SchemaType("BOOLEAN", InnerEnum.Boolean, 5);

		/// <summary>
		/// A 8-byte integer.
		/// </summary>
		public static readonly SchemaType Int8 = new SchemaType("INT8", InnerEnum.Int8, 6);

		/// <summary>
		/// A 16-byte integer.
		/// </summary>
		public static readonly SchemaType Int16 = new SchemaType("INT16", InnerEnum.Int16, 7);

		/// <summary>
		/// A 32-byte integer.
		/// </summary>
		public static readonly SchemaType Int32 = new SchemaType("INT32", InnerEnum.Int32, 8);

		/// <summary>
		/// A 64-byte integer.
		/// </summary>
		public static readonly SchemaType Int64 = new SchemaType("INT64", InnerEnum.Int64, 9);

		/// <summary>
		/// A float number.
		/// </summary>
		public static readonly SchemaType Float = new SchemaType("FLOAT", InnerEnum.Float, 10);

		/// <summary>
		/// A double number.
		/// </summary>
		public static readonly SchemaType Double = new SchemaType("DOUBLE", InnerEnum.Double, 11);

		/// <summary>
		/// Date.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType Date = new SchemaType("DATE", InnerEnum.Date, 12);

		/// <summary>
		/// Time.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType Time = new SchemaType("TIME", InnerEnum.Time, 13);

		/// <summary>
		/// Timestamp.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType Timestamp = new SchemaType("TIMESTAMP", InnerEnum.Timestamp, 14);

		/// <summary>
		/// A Schema that contains Key Schema and Value Schema.
		/// </summary>
		public static readonly SchemaType KeyValue = new SchemaType("KEY_VALUE", InnerEnum.KeyValue, 15);

		//
		// Schemas that don't have schema info. the value should be negative.
		//

		/// <summary>
		/// A bytes array.
		/// </summary>
		public static readonly SchemaType Bytes = new SchemaType("BYTES", InnerEnum.Bytes, -1);

		/// <summary>
		/// Auto Detect Schema Type.
		/// </summary>
		[System.Obsolete]
		public static readonly SchemaType Auto = new SchemaType("AUTO", InnerEnum.Auto, -2);

		/// <summary>
		/// Auto Consume Type.
		/// </summary>
		public static readonly SchemaType AutoConsume = new SchemaType("AUTO_CONSUME", InnerEnum.AutoConsume, -3);

		/// <summary>
		/// Auto Publish Type.
		/// </summary>
		public static readonly SchemaType AutoPublish = new SchemaType("AUTO_PUBLISH", InnerEnum.AutoPublish, -4);

		private static readonly IList<SchemaType> ValueList = new List<SchemaType>();

		
		static SchemaType()
		{
			ValueList.Add(None);
			ValueList.Add(String);
			ValueList.Add(Json);
			ValueList.Add(Protobuf);
			ValueList.Add(Avro);
			ValueList.Add(Boolean);
			ValueList.Add(Int8);
			ValueList.Add(Int16);
			ValueList.Add(Int32);
			ValueList.Add(Int64);
			ValueList.Add(Float);
			ValueList.Add(Double);
			ValueList.Add(Date);
			ValueList.Add(Time);
			ValueList.Add(Timestamp);
			ValueList.Add(KeyValue);
			ValueList.Add(Bytes);
			ValueList.Add(Auto);
			ValueList.Add(AutoConsume);
			ValueList.Add(AutoPublish);
		}

		public enum InnerEnum
		{
			None,
			String,
			Json,
			Protobuf,
			Avro,
			Boolean,
			Int8,
			Int16,
			Int32,
			Int64,
			Float,
			Double,
			Date,
			Time,
			Timestamp,
			KeyValue,
			Bytes,
			Auto,
			AutoConsume,
			AutoPublish
		}

		public readonly InnerEnum InnerEnumValue;
		private readonly string _nameValue;
		private readonly int _ordinalValue;
		private static int _nextOrdinal = 0;

		private int _value;

		internal SchemaType(string name, InnerEnum innerEnum, int value)
		{
			_value = value;

			_nameValue = name;
			_ordinalValue = _nextOrdinal++;
			InnerEnumValue = innerEnum;
		}

		public int Value => _value;


        public static SchemaType ValueOf(int value)
		{
			return value switch
			{
				0 => None,
				1 => String,
				2 => Json,
				3 => Protobuf,
				4 => Avro,
				5 => Boolean,
				6 => Int8,
				7 => Int16,
				8 => Int32,
				9 => Int64,
				10 => Float,
				11 => Double,
				12 => Date,
				13 => Time,
				14 => Timestamp,
				15 => KeyValue,
				-1 => Bytes,
				-2 => Auto,
				-3 => AutoConsume,
				-4 => AutoPublish,
				_ => None,
			};
		}


		public bool Primitive => IsPrimitiveType(this);

        public bool Struct => IsStructType(this);

        public static bool IsPrimitiveType(SchemaType type)
		{
			switch (type.InnerEnumValue)
			{
				case InnerEnum.String:
				case InnerEnum.Boolean:
				case InnerEnum.Int8:
				case InnerEnum.Int16:
				case InnerEnum.Int32:
				case InnerEnum.Int64:
				case InnerEnum.Float:
				case InnerEnum.Double:
				case InnerEnum.Date:
				case InnerEnum.Time:
				case InnerEnum.Timestamp:
				case InnerEnum.Bytes:
				case InnerEnum.None:
					return true;
				default:
					return false;
			}

		}

		public static bool IsStructType(SchemaType type)
		{
			switch (type.InnerEnumValue)
			{
				case InnerEnum.Avro:
				case InnerEnum.Json:
				case InnerEnum.Protobuf:
					return true;
				default:
					return false;
			}
		}

		public static IList<SchemaType> Values()
		{
			return ValueList;
		}

		public int Ordinal()
		{
			return _ordinalValue;
		}

		public override string ToString()
		{
			return _nameValue;
		}

	}

}