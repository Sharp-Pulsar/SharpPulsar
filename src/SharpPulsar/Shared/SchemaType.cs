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
		public static readonly SchemaType NONE = new SchemaType("NONE", InnerEnum.NONE, 0);

		/// <summary>
		/// Simple String encoding with UTF-8.
		/// </summary>
		public static readonly SchemaType STRING = new SchemaType("STRING", InnerEnum.STRING, 1);

		/// <summary>
		/// JSON object encoding and validation.
		/// </summary>
		public static readonly SchemaType JSON = new SchemaType("JSON", InnerEnum.JSON, 2);

		/// <summary>
		/// Protobuf message encoding and decoding.
		/// </summary>
		public static readonly SchemaType PROTOBUF = new SchemaType("PROTOBUF", InnerEnum.PROTOBUF, 3);

		/// <summary>
		/// Serialize and deserialize via avro.
		/// </summary>
		public static readonly SchemaType AVRO = new SchemaType("AVRO", InnerEnum.AVRO, 4);

		/// <summary>
		/// boolean schema defined.
		/// @since 2.3.0
		/// </summary>
		public static readonly SchemaType BOOLEAN = new SchemaType("BOOLEAN", InnerEnum.BOOLEAN, 5);

		/// <summary>
		/// A 8-byte integer.
		/// </summary>
		public static readonly SchemaType INT8 = new SchemaType("INT8", InnerEnum.INT8, 6);

		/// <summary>
		/// A 16-byte integer.
		/// </summary>
		public static readonly SchemaType INT16 = new SchemaType("INT16", InnerEnum.INT16, 7);

		/// <summary>
		/// A 32-byte integer.
		/// </summary>
		public static readonly SchemaType INT32 = new SchemaType("INT32", InnerEnum.INT32, 8);

		/// <summary>
		/// A 64-byte integer.
		/// </summary>
		public static readonly SchemaType INT64 = new SchemaType("INT64", InnerEnum.INT64, 9);

		/// <summary>
		/// A float number.
		/// </summary>
		public static readonly SchemaType FLOAT = new SchemaType("FLOAT", InnerEnum.FLOAT, 10);

		/// <summary>
		/// A double number.
		/// </summary>
		public static readonly SchemaType DOUBLE = new SchemaType("DOUBLE", InnerEnum.DOUBLE, 11);

		/// <summary>
		/// Date.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType DATE = new SchemaType("DATE", InnerEnum.DATE, 12);

		/// <summary>
		/// Time.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType TIME = new SchemaType("TIME", InnerEnum.TIME, 13);

		/// <summary>
		/// Timestamp.
		/// @since 2.4.0
		/// </summary>
		public static readonly SchemaType TIMESTAMP = new SchemaType("TIMESTAMP", InnerEnum.TIMESTAMP, 14);

		/// <summary>
		/// A Schema that contains Key Schema and Value Schema.
		/// </summary>
		public static readonly SchemaType KeyValue = new SchemaType("KeyValue", InnerEnum.KeyValue, 15);

		/// <summary>
		/// Instant.
		/// </summary>
		public static readonly SchemaType INSTANT = new SchemaType("INSTANT", InnerEnum.INSTANT, 16);

		/// <summary>
		/// LocalDate.
		/// </summary>
		public static readonly SchemaType LocalDate = new SchemaType("LocalDate", InnerEnum.LocalDate, 17);

		/// <summary>
		/// LocalTime.
		/// </summary>
		public static readonly SchemaType LocalTime = new SchemaType("LocalTime", InnerEnum.LocalTime, 18);

		/// <summary>
		/// LocalDateTime.
		/// </summary>
		public static readonly SchemaType LocalDateTime = new SchemaType("LocalDateTime", InnerEnum.LocalDateTime, 19);

		/// <summary>
		/// Protobuf native schema base on Descriptor.
		/// </summary>
		public static readonly SchemaType ProtobufNative = new SchemaType("ProtobufNative", InnerEnum.ProtobufNative, 20);

		//
		// Schemas that don't have schema info. the value should be negative.
		//

		/// <summary>
		/// A bytes array.
		/// </summary>
		public static readonly SchemaType BYTES = new SchemaType("BYTES", InnerEnum.BYTES, -1);

		/// <summary>
		/// Auto Detect Schema Type.
		/// </summary>
		[System.Obsolete]
		public static readonly SchemaType AUTO = new SchemaType("AUTO", InnerEnum.AUTO, -2);

		/// <summary>
		/// Auto Consume Type.
		/// </summary>
		public static readonly SchemaType AutoConsume = new SchemaType("AutoConsume", InnerEnum.AutoConsume, -3);

		/// <summary>
		/// Auto Publish Type.
		/// </summary>
		public static readonly SchemaType AutoPublish = new SchemaType("AutoPublish", InnerEnum.AutoPublish, -4);

		private static readonly List<SchemaType> _valueList = new List<SchemaType>();

        [System.Obsolete]
        static SchemaType()
		{
			_valueList.Add(NONE);
			_valueList.Add(STRING);
			_valueList.Add(JSON);
			_valueList.Add(PROTOBUF);
			_valueList.Add(AVRO);
			_valueList.Add(BOOLEAN);
			_valueList.Add(INT8);
			_valueList.Add(INT16);
			_valueList.Add(INT32);
			_valueList.Add(INT64);
			_valueList.Add(FLOAT);
			_valueList.Add(DOUBLE);
			_valueList.Add(DATE);
			_valueList.Add(TIME);
			_valueList.Add(TIMESTAMP);
			_valueList.Add(KeyValue);
			_valueList.Add(INSTANT);
			_valueList.Add(LocalDate);
			_valueList.Add(LocalTime);
			_valueList.Add(LocalDateTime);
			_valueList.Add(ProtobufNative);
			_valueList.Add(BYTES);
			_valueList.Add(AUTO);
			_valueList.Add(AutoConsume);
			_valueList.Add(AutoPublish);
		}

		public enum InnerEnum
		{
			NONE,
			STRING,
			JSON,
			PROTOBUF,
			AVRO,
			BOOLEAN,
			INT8,
			INT16,
			INT32,
			INT64,
			FLOAT,
			DOUBLE,
			DATE,
			TIME,
			TIMESTAMP,
			KeyValue,
			INSTANT,
			LocalDate,
			LocalTime,
			LocalDateTime,
			ProtobufNative,
			BYTES,
			AUTO,
			AutoConsume,
			AutoPublish
		}

		public readonly InnerEnum InnerEnumValue;
		public readonly string Name;
		private readonly int _ordinal;
		private static int nextOrdinal = 0;

		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
		internal int _value;

		internal SchemaType(string name, InnerEnum innerEnum, int Value)
		{
			this._value = Value;

			Name = name;
			_ordinal = nextOrdinal++;
			InnerEnumValue = innerEnum;
		}

		public int Value
		{
			get
			{
				return _value;
			}
		}

        
        public static SchemaType ValueOf(int Value)
		{
			switch (Value)
			{
				case 0:
					return NONE;
				case 1:
					return STRING;
				case 2:
					return JSON;
				case 3:
					return PROTOBUF;
				case 4:
					return AVRO;
				case 5:
					return BOOLEAN;
				case 6:
					return INT8;
				case 7:
					return INT16;
				case 8:
					return INT32;
				case 9:
					return INT64;
				case 10:
					return FLOAT;
				case 11:
					return DOUBLE;
				case 12:
					return DATE;
				case 13:
					return TIME;
				case 14:
					return TIMESTAMP;
				case 15:
					return KeyValue;
				case 16:
					return INSTANT;
				case 17:
					return LocalDate;
				case 18:
					return LocalTime;
				case 19:
					return LocalDateTime;
				case 20:
					return ProtobufNative;
				case -1:
					return BYTES;
				case -2:
					return AUTO;
				case -3:
					return AutoConsume;
				case -4:
					return AutoPublish;
				default:
					return NONE;
			}
		}


		public bool Primitive
		{
			get
			{
				return IsPrimitiveType(this);
			}
		}

		public bool Struct
		{
			get
			{
				return IsStructType(this);
			}
		}

		public static bool IsPrimitiveType(SchemaType Type)
		{
			switch (Type.InnerEnumValue)
			{
				case InnerEnum.STRING:
				case InnerEnum.BOOLEAN:
				case InnerEnum.INT8:
				case InnerEnum.INT16:
				case InnerEnum.INT32:
				case InnerEnum.INT64:
				case InnerEnum.FLOAT:
				case InnerEnum.DOUBLE:
				case InnerEnum.DATE:
				case InnerEnum.TIME:
				case InnerEnum.TIMESTAMP:
				case InnerEnum.BYTES:
				case InnerEnum.INSTANT:
				case InnerEnum.LocalDate:
				case InnerEnum.LocalTime:
				case InnerEnum.LocalDateTime:
				case InnerEnum.NONE:
					return true;
				default:
					return false;
			}

		}

		public static bool IsStructType(SchemaType type)
		{
			switch (type.InnerEnumValue)
			{
				case InnerEnum.AVRO:
				case InnerEnum.JSON:
				case InnerEnum.PROTOBUF:
				case InnerEnum.ProtobufNative:
					return true;
				default:
					return false;
			}
		}

		public static SchemaType[] Values()
		{
			return _valueList.ToArray();
		}

		public int Ordinal()
		{
			return _ordinal;
		}

		public override string ToString()
		{
			return Name;
		}

		public static SchemaType ValueOf(string name)
		{
			foreach (SchemaType enumInstance in _valueList)
			{
				if (enumInstance.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}