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
	/// A schema for bytes array.
	/// </summary>
	public class BytesSchema : AbstractSchema<sbyte[]>
	{

		private static readonly BytesSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static BytesSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("Bytes").setType(SchemaType.BYTES).setSchema(new sbyte[0]);
			INSTANCE = new BytesSchema();
		}

		public static BytesSchema Of()
		{
			return INSTANCE;
		}

		public override sbyte[] Encode(sbyte[] Message)
		{
			return Message;
		}

		public override sbyte[] Decode(sbyte[] Bytes)
		{
			return Bytes;
		}

		public override sbyte[] Decode(ByteBuf ByteBuf)
		{
			if (ByteBuf == null)
			{
				return null;
			}
			int Size = ByteBuf.readableBytes();
			var Bytes = new sbyte[Size];

			ByteBuf.readBytes(Bytes, 0, Size);
			return Bytes;
		}

	}

}