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
	/// A bytebuffer schema is effectively a `BYTES` schema.
	/// </summary>
	public class ByteBufferSchema : AbstractSchema<ByteBuffer>
	{

		private static readonly ByteBufferSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static ByteBufferSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("ByteBuffer").setType(SchemaType.BYTES).setSchema(new sbyte[0]);
			INSTANCE = new ByteBufferSchema();
		}

		public static ByteBufferSchema of()
		{
			return INSTANCE;
		}

		public override sbyte[] encode(ByteBuffer data)
		{
			if (data == null)
			{
				return null;
			}

			data.rewind();

			if (data.hasArray())
			{
				sbyte[] arr = data.array();
				if (data.arrayOffset() == 0 && arr.Length == data.remaining())
				{
					return arr;
				}
			}

			sbyte[] ret = new sbyte[data.remaining()];
			data.get(ret, 0, ret.Length);
			data.rewind();
			return ret;
		}

		public override ByteBuffer decode(sbyte[] data)
		{
			if (null == data)
			{
				return null;
			}
			else
			{
				return ByteBuffer.wrap(data);
			}
		}

		public override ByteBuffer decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			else
			{
				int size = byteBuf.readableBytes();
				sbyte[] bytes = new sbyte[size];
				byteBuf.readBytes(bytes);
				return ByteBuffer.wrap(bytes);
			}
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