using DotNetty.Buffers;
using SharpPulsar.Common.Schema;
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
namespace SharpPulsar.Impl.Schema
{

	/// <summary>
	/// A schema for `java.util.Date` or `java.sql.Date`.
	/// </summary>
	public class DateSchema : AbstractSchema<DateTime>
	{

	   private static readonly DateSchema INSTANCE;
	   private static readonly SchemaInfo SCHEMA_INFO;

	   static DateSchema()
	   {
		   SCHEMA_INFO = (new SchemaInfo()).setName("Date").setType(SchemaType.DATE).setSchema(new sbyte[0]);
		   INSTANCE = new DateSchema();
	   }

	   public static DateSchema Of()
	   {
		  return INSTANCE;
	   }

	   public sbyte[] Encode(DateTime message)
	   {
		  if (null == message)
		  {
			 return null;
		  }

		  long? date = message.Ticks;
		  return LongSchema.Of().Encode(date);
	   }

	   public DateTime? Decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }

		  long? decode = LongSchema.Of().Decode(bytes);
		  return new DateTime(decode);
	   }

	   public override DateTime Decode(IByteBuffer byteBuf)
	   {
		  long? decode = LongSchema.Of().Decode(byteBuf);
		  return new DateTime(decode);
	   }

	   public override SchemaInfo SchemaInfo
	   {
		   get
		   {
			  return SCHEMA_INFO;
		   }
	   }
	}

}