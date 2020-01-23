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
	/// A schema for `Short`.
	/// </summary>
	public class ShortSchema : AbstractSchema<short>
	{

		private static readonly ShortSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static ShortSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("INT16").setType(SchemaType.INT16).setSchema(new sbyte[0]);
			INSTANCE = new ShortSchema();
		}

		public static ShortSchema Of()
		{
			return INSTANCE;
		}

		public override void Validate(sbyte[] Message)
		{
			if (Message.Length != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public override void Validate(ByteBuf Message)
		{
			if (Message.readableBytes() != 2)
			{
				throw new SchemaSerializationException("Size of data received by ShortSchema is not 2");
			}
		}

		public override sbyte[] Encode(short? Message)
		{
			if (null == Message)
			{
				return null;
			}
			else
			{
				return new sbyte[] {(sbyte)((int)((uint)Message >> 8)), Message.Value};
			}
		}

		public override short? Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			Validate(Bytes);
			short Value = 0;
			foreach (sbyte B in Bytes)
			{
				Value <<= 8;
				Value |= (short)(B & 0xFF);
			}
			return Value;
		}

		public override short? Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			Validate(ByteBuf);
			short Value = 0;

			for (int I = 0; I < 2; I++)
			{
				Value <<= 8;
				Value |= (short)(ByteBuf.getByte(I) & 0xFF);
			}
			return Value;
		}

	}

}