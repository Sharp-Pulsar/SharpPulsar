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
	/// A schema for `java.time.LocalDate`.
	/// </summary>
	public class LocalDateSchema : AbstractSchema<LocalDate>
	{

	   private static readonly LocalDateSchema INSTANCE;
	   private static readonly SchemaInfo SCHEMA_INFO;

	   static LocalDateSchema()
	   {
		   SCHEMA_INFO = (new SchemaInfo()).setName("LocalDate").setType(SchemaType.LOCAL_DATE).setSchema(new sbyte[0]);
		   INSTANCE = new LocalDateSchema();
	   }

	   public static LocalDateSchema of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] encode(LocalDate message)
	   {
		  if (null == message)
		  {
			 return null;
		  }

		  long? epochDay = message.toEpochDay();
		  return LongSchema.of().encode(epochDay);
	   }

	   public override LocalDate decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }

		  long? decode = LongSchema.of().decode(bytes);
		  return LocalDate.ofEpochDay(decode);
	   }

	   public override LocalDate decode(ByteBuf byteBuf)
	   {
		  if (null == byteBuf)
		  {
			 return null;
		  }

		  long? decode = LongSchema.of().decode(byteBuf);
		  return LocalDate.ofEpochDay(decode);
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