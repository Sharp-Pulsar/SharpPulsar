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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// A schema for `java.util.Date` or `java.sql.Date`.
	/// </summary>
	public class DateSchema : AbstractSchema<DateTime>
	{

	   private static readonly DateSchema INSTANCE;
	   public virtual SchemaInfo {get;}

	   static DateSchema()
	   {
		   SchemaInfo = (new SchemaInfo()).setName("Date").setType(SchemaType.DATE).setSchema(new sbyte[0]);
		   INSTANCE = new DateSchema();
	   }

	   public static DateSchema Of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] Encode(DateTime Message)
	   {
		  if (null == Message)
		  {
			 return null;
		  }

		  long? Date = Message.Ticks;
		  return LongSchema.Of().encode(Date);
	   }

	   public override DateTime Decode(sbyte[] Bytes)
	   {
		  if (null == Bytes)
		  {
			 return null;
		  }

		  long? Decode = LongSchema.Of().decode(Bytes);
		  return new DateTime(Decode);
	   }

	   public override DateTime Decode(ByteBuf ByteBuf)
	   {
		  if (null == ByteBuf)
		  {
			 return null;
		  }

		  long? Decode = LongSchema.Of().decode(ByteBuf);
		  return new DateTime(Decode);
	   }

	}

}