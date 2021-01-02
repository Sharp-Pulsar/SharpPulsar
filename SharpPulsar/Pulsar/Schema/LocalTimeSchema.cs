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
	/// A schema for `java.time.LocalTime`.
	/// </summary>
	public class LocalTimeSchema : AbstractSchema<LocalTime>
	{

	   private static readonly LocalTimeSchema INSTANCE;
	   private static readonly SchemaInfo SCHEMA_INFO;

	   static LocalTimeSchema()
	   {
		   SCHEMA_INFO = (new SchemaInfo()).setName("LocalTime").setType(SchemaType.LOCAL_TIME).setSchema(new sbyte[0]);
		   INSTANCE = new LocalTimeSchema();
	   }

	   public static LocalTimeSchema of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] encode(LocalTime message)
	   {
		  if (null == message)
		  {
			 return null;
		  }

		  long? nanoOfDay = message.toNanoOfDay();
		  return LongSchema.of().encode(nanoOfDay);
	   }

	   public override LocalTime decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }

		  long? decode = LongSchema.of().decode(bytes);
		  return LocalTime.ofNanoOfDay(decode);
	   }

	   public override LocalTime decode(ByteBuf byteBuf)
	   {
		  if (null == byteBuf)
		  {
			 return null;
		  }

		  long? decode = LongSchema.of().decode(byteBuf);
		  return LocalTime.ofNanoOfDay(decode);
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