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
	/// A bytebuffer schema is effectively a `BYTES` schema.
	/// </summary>
	public class ByteBufferSchema : AbstractSchema<ByteBuffer>
	{

		private static readonly ByteBufferSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static ByteBufferSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("ByteBuffer").setType(SchemaType.BYTES).setSchema(new sbyte[0]);
			INSTANCE = new ByteBufferSchema();
		}

		public static ByteBufferSchema Of()
		{
			return INSTANCE;
		}

		public override sbyte[] Encode(ByteBuffer Data)
		{
			if (Data == null)
			{
				return null;
			}

			Data.rewind();

			if (Data.hasArray())
			{
				sbyte[] Arr = Data.array();
				if (Data.arrayOffset() == 0 && Arr.Length == Data.remaining())
				{
					return Arr;
				}
			}

			sbyte[] Ret = new sbyte[Data.remaining()];
			Data.get(Ret, 0, Ret.Length);
			Data.rewind();
			return Ret;
		}

		public override ByteBuffer Decode(sbyte[] Data)
		{
			if (null == Data)
			{
				return null;
			}
			else
			{
				return ByteBuffer.wrap(Data);
			}
		}

		public override ByteBuffer Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			else
			{
				int Size = ByteBuf.readableBytes();
				sbyte[] Bytes = new sbyte[Size];
				ByteBuf.readBytes(Bytes);
				return ByteBuffer.wrap(Bytes);
			}
		}

	}

}