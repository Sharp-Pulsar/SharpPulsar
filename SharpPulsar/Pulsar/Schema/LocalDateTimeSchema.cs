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
namespace org.apache.pulsar.client.impl.schema
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;


	/// <summary>
	/// A schema for `java.time.LocalDateTime`.
	/// </summary>
	public class LocalDateTimeSchema : AbstractSchema<DateTime>
	{

	   private static readonly LocalDateTimeSchema INSTANCE;
	   private static readonly SchemaInfo SCHEMA_INFO;
	   public const string DELIMITER = ":";

	   static LocalDateTimeSchema()
	   {
		   SCHEMA_INFO = (new SchemaInfo()).setName("LocalDateTime").setType(SchemaType.LOCAL_DATE_TIME).setSchema(new sbyte[0]);
		   INSTANCE = new LocalDateTimeSchema();
	   }

	   public static LocalDateTimeSchema of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] encode(DateTime message)
	   {
		  if (null == message)
		  {
			 return null;
		  }
		  //LocalDateTime is accurate to nanoseconds and requires two value storage.
		  ByteBuffer buffer = ByteBuffer.allocate(Long.BYTES * 2);
		  buffer.putLong(message.toLocalDate().toEpochDay());
		  buffer.putLong(message.toLocalTime().toNanoOfDay());
		  return buffer.array();
	   }

	   public override DateTime decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }
		  ByteBuffer buffer = ByteBuffer.wrap(bytes);
		  long epochDay = buffer.Long;
		  long nanoOfDay = buffer.Long;
		  return new DateTime(LocalDate.ofEpochDay(epochDay), LocalTime.ofNanoOfDay(nanoOfDay));
	   }

	   public override DateTime decode(ByteBuf byteBuf)
	   {
		  if (null == byteBuf)
		  {
			 return null;
		  }
		  long epochDay = byteBuf.readLong();
		  long nanoOfDay = byteBuf.readLong();
		  return new DateTime(LocalDate.ofEpochDay(epochDay), LocalTime.ofNanoOfDay(nanoOfDay));
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