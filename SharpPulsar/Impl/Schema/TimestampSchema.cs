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
	/// A schema for `java.sql.Timestamp`.
	/// </summary>
	public class TimestampSchema : AbstractSchema<Timestamp>
	{

	   private static readonly TimestampSchema INSTANCE;
	   public virtual SchemaInfo {get;}

	   static TimestampSchema()
	   {
		   SchemaInfo = (new SchemaInfo()).setName("Timestamp").setType(SchemaType.TIMESTAMP).setSchema(new sbyte[0]);
		   INSTANCE = new TimestampSchema();
	   }

	   public static TimestampSchema Of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] Encode(Timestamp Message)
	   {
		  if (null == Message)
		  {
			 return null;
		  }

		  long? Timestamp = Message.Time;
		  return LongSchema.Of().encode(Timestamp);
	   }

	   public override Timestamp Decode(sbyte[] Bytes)
	   {
		  if (null == Bytes)
		  {
			 return null;
		  }

		  long? Decode = LongSchema.Of().decode(Bytes);
		  return new Timestamp(Decode);
	   }

	   public override Timestamp Decode(ByteBuf ByteBuf)
	   {
		  if (null == ByteBuf)
		  {
			 return null;
		  }

		  long? Decode = LongSchema.Of().decode(ByteBuf);
		  return new Timestamp(Decode);
	   }

	}

}