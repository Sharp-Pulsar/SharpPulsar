﻿using NodaTime;
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
namespace SharpPulsar.Schemas
{

	/// <summary>
	/// A schema for `java.time.LocalDateTime`.
	/// </summary>
	public class LocalDateTimeSchema : AbstractSchema<LocalDateTime>
	{

	   private static readonly LocalDateTimeSchema _instance;
	   private static readonly ISchemaInfo _schemaInfo;
	   public const string DELIMITER = ":";
		private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static LocalDateTimeSchema()
	   {
			var info = new SchemaInfo
			{
				Name = "LocalDateTime",
				Type = SchemaType.LocalDateTime,
				Schema = new byte[0]
			};
			_schemaInfo = info;
			_instance = new LocalDateTimeSchema();
	   }

	   public static LocalDateTimeSchema Of()
	   {
		  return _instance;
	   }

	   public override byte[] Encode(LocalDateTime message)
	   {
			long epochDay = (long)(message.ToDateTimeUnspecified() - _epoch).TotalMilliseconds;
			return LongSchema.Of().Encode(epochDay);
		}

		public override LocalDateTime Decode(byte[] bytes)
		{
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