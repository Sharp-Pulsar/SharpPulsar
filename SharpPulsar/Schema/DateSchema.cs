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
	/// A schema for `java.util.Date` or `java.sql.Date`.
	/// </summary>
	public class DateSchema : AbstractSchema<DateTime>
	{

	   private static readonly DateSchema _instance;
	   private static readonly ISchemaInfo _schemaInfo;

	   static DateSchema()
	   {
			var info = new SchemaInfo
			{
				Name = "Date",
				Type = SchemaType.DATE,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new DateSchema();
		}

	   public static DateSchema Of()
	   {
		  return _instance;
	   }

	   public override sbyte[] Encode(DateTime message)
	   {
		  if (null == message)
		  {
			 return null;
		  }

		  long date = message.Ticks;
		  return LongSchema.Of().Encode(date);
	   }

	   public override DateTime Decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return default;
		  }

		  long decode = (long)LongSchema.Of().Decode(bytes);
		  return new DateTime(decode);
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