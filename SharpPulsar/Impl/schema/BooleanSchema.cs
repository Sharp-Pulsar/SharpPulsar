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
	/// A schema for `Boolean`.
	/// </summary>
	public class BooleanSchema : AbstractSchema<bool>
	{

		private static readonly BooleanSchema INSTANCE;
		public virtual SchemaInfo {get;}

		static BooleanSchema()
		{
			SchemaInfo = (new SchemaInfo()).setName("Boolean").setType(SchemaType.BOOLEAN).setSchema(new sbyte[0]);
			INSTANCE = new BooleanSchema();
		}

		public static BooleanSchema Of()
		{
			return INSTANCE;
		}

		public override void Validate(sbyte[] Message)
		{
			if (Message.Length != 1)
			{
				throw new SchemaSerializationException("Size of data received by BooleanSchema is not 1");
			}
		}

		public override sbyte[] Encode(bool? Message)
		{
			if (null == Message)
			{
				return null;
			}
			else
			{
				return new sbyte[]{(sbyte)(Message ? 1 : 0)};
			}
		}

		public override bool? Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			Validate(Bytes);
			return Bytes[0] != 0;
		}

		public override bool? Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			return ByteBuf.getBoolean(0);
		}

	}

}