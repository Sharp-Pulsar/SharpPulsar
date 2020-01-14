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
	using ByteBufUtil = io.netty.buffer.ByteBufUtil;
	using Unpooled = io.netty.buffer.Unpooled;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;

	/// <summary>
	/// A variant `Bytes` schema that takes <seealso cref="io.netty.buffer.ByteBuf"/>.
	/// </summary>
	public class ByteBufSchema : AbstractSchema<ByteBuf>
	{

		private static readonly ByteBufSchema INSTANCE;
		private static readonly SchemaInfo SCHEMA_INFO;

		static ByteBufSchema()
		{
			SCHEMA_INFO = (new SchemaInfo()).setName("ByteBuf").setType(SchemaType.BYTES).setSchema(new sbyte[0]);
			INSTANCE = new ByteBufSchema();
		}

		public static ByteBufSchema of()
		{
			return INSTANCE;
		}

		public override sbyte[] encode(ByteBuf message)
		{
			if (message == null)
			{
				return null;
			}

			return ByteBufUtil.getBytes(message);
		}

		public override ByteBuf decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			else
			{
				return Unpooled.wrappedBuffer(bytes);
			}
		}

		public override ByteBuf decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			else
			{
				return byteBuf;
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