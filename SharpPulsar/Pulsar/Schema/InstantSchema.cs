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
	/// A schema for `java.time.Instant`.
	/// </summary>
	public class InstantSchema : AbstractSchema<Instant>
	{

	   private static readonly InstantSchema INSTANCE;
	   private static readonly SchemaInfo SCHEMA_INFO;

	   static InstantSchema()
	   {
		   SCHEMA_INFO = (new SchemaInfo()).setName("Instant").setType(SchemaType.INSTANT).setSchema(new sbyte[0]);
		   INSTANCE = new InstantSchema();
	   }

	   public static InstantSchema of()
	   {
		  return INSTANCE;
	   }

	   public override sbyte[] encode(Instant message)
	   {
		  if (null == message)
		  {
			 return null;
		  }
		  // Instant is accurate to nanoseconds and requires two value storage.
		  ByteBuffer buffer = ByteBuffer.allocate(Long.BYTES + Integer.BYTES);
		  buffer.putLong(message.EpochSecond);
		  buffer.putInt(message.Nano);
		  return buffer.array();
	   }

	   public override Instant decode(sbyte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }
		  ByteBuffer buffer = ByteBuffer.wrap(bytes);
		  long epochSecond = buffer.Long;
		  int nanos = buffer.Int;
		  return Instant.ofEpochSecond(epochSecond, nanos);
	   }

	   public override Instant decode(ByteBuf byteBuf)
	   {
		  if (null == byteBuf)
		  {
			 return null;
		  }
		  long epochSecond = byteBuf.readLong();
		  int nanos = byteBuf.readInt();
		  return Instant.ofEpochSecond(epochSecond, nanos);
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