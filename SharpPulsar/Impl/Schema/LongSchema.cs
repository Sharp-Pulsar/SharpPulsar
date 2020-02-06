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
	using SchemaSerializationException = Api.SchemaSerializationException;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// A schema for `Long`.
	/// </summary>
	public class LongSchema : AbstractSchema<long>
	{

		private static readonly LongSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static LongSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("INT64").setType(SchemaType.INT64).setSchema(new sbyte[0]);
			INSTANCE = new LongSchema();
		}

		public static LongSchema Of()
		{
			return INSTANCE;
		}

		public override void Validate(sbyte[] Message)
		{
			if (Message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override void Validate(ByteBuf Message)
		{
			if (Message.readableBytes() != 8)
			{
				throw new SchemaSerializationException("Size of data received by LongSchema is not 8");
			}
		}

		public override sbyte[] Encode(long? Data)
		{
			if (null == Data)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)Data >> 56)), (sbyte)((int)((uint)Data >> 48)), (sbyte)((int)((uint)Data >> 40)), (sbyte)((int)((uint)Data >> 32)), (sbyte)((int)((uint)Data >> 24)), (sbyte)((int)((uint)Data >> 16)), (sbyte)((int)((uint)Data >> 8)), Data.Value};
			}
		}

		public override long? Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			Validate(Bytes);
			long Value = 0L;
			foreach (sbyte B in Bytes)
			{
				Value <<= 8;
				Value |= B & 0xFF;
			}
			return Value;
		}

		public override long? Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			Validate(ByteBuf);
			long Value = 0L;
			for (int I = 0; I < 8; I++)
			{
				Value <<= 8;
				Value |= ByteBuf.getByte(I) & 0xFF;
			}

			return Value;
		}

	}

}