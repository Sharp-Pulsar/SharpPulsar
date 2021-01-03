using NodaTime;
using SharpPulsar.Common.Schema;
using SharpPulsar.Interfaces.Schema;
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
	/// A schema for `java.time.LocalTime`.
	/// </summary>
	public class LocalTimeSchema : AbstractSchema<LocalTime?>
	{

	   private static readonly LocalTimeSchema _instance;
	   private static readonly ISchemaInfo _schemaInfo;

	   static LocalTimeSchema()
	   {
			var info = new SchemaInfo
			{
				Name = "LocalTime",
				Type = SchemaType.LocalDateTime,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new LocalTimeSchema();
	   }

	   public static LocalTimeSchema of()
	   {
		  return _instance;
	   }

	   public override sbyte[] Encode(LocalTime? message)
	   {
		  if (null == message)
		  {
			 return null;
		  }

		  long? nanoOfDay = message.Value.NanosecondOfDay;
		  return LongSchema.Of().Encode(nanoOfDay);
	   }

	   public override LocalTime? Decode(byte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }

		  long? decode = LongSchema.Of().Decode(bytes);
		  return LocalTime.FromNanosecondsSinceMidnight(decode.Value);
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