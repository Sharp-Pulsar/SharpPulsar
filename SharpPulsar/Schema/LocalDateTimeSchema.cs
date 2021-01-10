using NodaTime;
using SharpPulsar.Common.Schema;
using SharpPulsar.Interfaces.ISchema;
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
namespace SharpPulsar.Schema
{

	/// <summary>
	/// A schema for `java.time.LocalDateTime`.
	/// </summary>
	public class LocalDateTimeSchema : AbstractSchema<LocalDateTime>
	{

	   private static readonly LocalDateTimeSchema _instance;
	   private static readonly ISchemaInfo _schemaInfo;
	   public const string DELIMITER = ":";

	   static LocalDateTimeSchema()
	   {
			var info = new SchemaInfo
			{
				Name = "LocalDateTime",
				Type = SchemaType.LocalDateTime,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new LocalDateTimeSchema();
	   }

	   public static LocalDateTimeSchema Of()
	   {
		  return _instance;
	   }

	   public override sbyte[] Encode(LocalDateTime message)
	   {
			//LocalDateTime is accurate to nanoseconds and requires two value storage.
			//ByteBuffer buffer = ByteBuffer.allocate(Long.BYTES * 2);
			//buffer.putLong(message.toLocalDate().toEpochDay());
			//buffer.putLong(message.toLocalTime().toNanoOfDay());
			var epochDay = new DateTimeOffset(message.ToDateTimeUnspecified()).ToUnixTimeMilliseconds();
			return LongSchema.Of().Encode(epochDay);
		}

		public override LocalDateTime Decode(sbyte[] bytes)
		{
			//ByteBuffer buffer = ByteBuffer.wrap(bytes);
			//long epochDay = buffer.Long;
			//long nanoOfDay = buffer.Long;
			//return new DateTime(LocalDate.ofEpochDay(epochDay), LocalTime.ofNanoOfDay(nanoOfDay));
			var decode = LongSchema.Of().Decode(bytes);
			return LocalDateTime.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(decode).DateTime);
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