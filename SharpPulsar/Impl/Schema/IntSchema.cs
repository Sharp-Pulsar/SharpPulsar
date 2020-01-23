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
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;

	/// <summary>
	/// A schema for `Integer`.
	/// </summary>
	public class IntSchema : AbstractSchema<int>
	{

		private static readonly IntSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static IntSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("INT32").setType(SchemaType.INT32).setSchema(new sbyte[0]);
			INSTANCE = new IntSchema();
		}

		public static IntSchema Of()
		{
			return INSTANCE;
		}

		public override void Validate(sbyte[] Message)
		{
			if (Message.Length != 4)
			{
				throw new SchemaSerializationException("Size of data received by IntSchema is not 4");
			}
		}

		public override void Validate(ByteBuf Message)
		{
			if (Message.readableBytes() != 4)
			{
				throw new SchemaSerializationException("Size of data received by IntSchema is not 4");
			}
		}

		public override sbyte[] Encode(int? Message)
		{
			if (null == Message)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)Message >> 24)), (sbyte)((int)((uint)Message >> 16)), (sbyte)((int)((uint)Message >> 8)), Message.Value};
			}
		}

		public override int? Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			Validate(Bytes);
			int Value = 0;
			foreach (sbyte B in Bytes)
			{
				Value <<= 8;
				Value |= B & 0xFF;
			}
			return Value;
		}

		public override int? Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			Validate(ByteBuf);
			int Value = 0;

			for (int I = 0; I < 4; I++)
			{
				Value <<= 8;
				Value |= ByteBuf.getByte(I) & 0xFF;
			}

			return Value;
		}

	}

}