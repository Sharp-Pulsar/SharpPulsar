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
	/// A schema for `Double`.
	/// </summary>
	public class DoubleSchema : AbstractSchema<double>
	{

		private static readonly DoubleSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static DoubleSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("Double").setType(SchemaType.DOUBLE).setSchema(new sbyte[0]);
			INSTANCE = new DoubleSchema();
		}

		public static DoubleSchema Of()
		{
			return INSTANCE;
		}

		public override void Validate(sbyte[] Message)
		{
			if (Message.Length != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}

		public override void Validate(ByteBuf Message)
		{
			if (Message.readableBytes() != 8)
			{
				throw new SchemaSerializationException("Size of data received by DoubleSchema is not 8");
			}
		}


		public override sbyte[] Encode(double? Message)
		{
			if (null == Message)
			{
				return null;
			}
			else
			{
				long Bits = System.BitConverter.DoubleToInt64Bits(Message);
				return new sbyte[] {(sbyte)((long)((ulong)Bits >> 56)), (sbyte)((long)((ulong)Bits >> 48)), (sbyte)((long)((ulong)Bits >> 40)), (sbyte)((long)((ulong)Bits >> 32)), (sbyte)((long)((ulong)Bits >> 24)), (sbyte)((long)((ulong)Bits >> 16)), (sbyte)((long)((ulong)Bits >> 8)), (sbyte) Bits};
			}
		}

		public override double? Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			Validate(Bytes);
			long Value = 0;
			foreach (sbyte B in Bytes)
			{
				Value <<= 8;
				Value |= B & 0xFF;
			}
			return Double.longBitsToDouble(Value);
		}

		public override double? Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			Validate(ByteBuf);
			long Value = 0;

			for (int I = 0; I < 8; I++)
			{
				Value <<= 8;
				Value |= ByteBuf.getByte(I) & 0xFF;
			}
			return Double.longBitsToDouble(Value);
		}

	}

}