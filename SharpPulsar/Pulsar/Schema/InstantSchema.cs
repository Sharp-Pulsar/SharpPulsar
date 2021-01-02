using NodaTime;
using SharpPulsar.Common.Schema;
using SharpPulsar.Pulsar.Api.Schema;
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
namespace SharpPulsar.Pulsar.Schema
{
	/// <summary>
	/// A schema for `java.time.Instant`.
	/// </summary>
	public class InstantSchema : AbstractSchema<Instant?>
	{

	   private static readonly InstantSchema _instance;
	   private static readonly ISchemaInfo _schemaInfo;

	   static InstantSchema()
	   {
			var info = new SchemaInfo
			{
				Name = "Instant",
				Type = SchemaType.INSTANT,
				Schema = new sbyte[0]
			};
			_schemaInfo = info;
			_instance = new InstantSchema();
	   }

	   public static InstantSchema Of()
	   {
		  return _instance;
	   }

		public override sbyte[] Encode(Instant? message)
		{
			if (null == message)
			{
				return null;
			}
			// Instant is accurate to nanoseconds and requires two value storage.
			/*ByteBuffer buffer = ByteBuffer.allocate(Long.BYTES + Integer.BYTES);
			buffer.putLong(message.EpochSecond);
			buffer.putInt(message.Nano);
			  return LongSchema.Of().Encode(epochDay);
			  return buffer.array();*/
			long epochDay = message.Value.ToDateTimeOffset().ToUnixTimeMilliseconds();
			return LongSchema.Of().Encode(epochDay);
		}

	   public override Instant? Decode(byte[] bytes)
	   {
		  if (null == bytes)
		  {
			 return null;
		  }
			//ByteBuffer buffer = ByteBuffer.wrap(bytes);
			//long epochSecond = buffer.Long;
			//int nanos = buffer.Int;
			long? decode = LongSchema.Of().Decode(bytes);
			return Instant.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(decode.Value));
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