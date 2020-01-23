using System.Collections.Generic;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	using ByteBuf = io.netty.buffer.ByteBuf;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using SchemaType = Org.Apache.Pulsar.Common.Schema.SchemaType;


	/// <summary>
	/// Schema definition for Strings encoded in UTF-8 format.
	/// </summary>
	public class StringSchema : AbstractSchema<string>
	{

		internal static readonly string CharsetKey;

		private static readonly SchemaInfo DEFAULT_SCHEMA_INFO;
		private static readonly Charset DEFAULT_CHARSET;
		private static readonly StringSchema UTF8;

		static StringSchema()
		{
			// Ensure the ordering of the static initialization
			CharsetKey = "__charset";
			DEFAULT_CHARSET = StandardCharsets.UTF_8;
			DEFAULT_SCHEMA_INFO = (new SchemaInfo()).setName("String").setType(SchemaType.STRING).setSchema(new sbyte[0]);

			UTF8 = new StringSchema(StandardCharsets.UTF_8);
		}

		private static readonly FastThreadLocal<sbyte[]> tmpBuffer = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<sbyte[]>
		{
			public override sbyte[] initialValue()
			{
				return new sbyte[1024];
			}
		}

		public static StringSchema FromSchemaInfo(SchemaInfo SchemaInfo)
		{
			checkArgument(SchemaType.STRING == SchemaInfo.Type, "Not a string schema");
			string CharsetName = SchemaInfo.Properties.get(CharsetKey);
			if (null == CharsetName)
			{
				return UTF8;
			}
			else
			{
				return new StringSchema(Charset.forName(CharsetName));
			}
		}

		public static StringSchema Utf8()
		{
			return UTF8;
		}

		private readonly Charset charset;
		public virtual SchemaInfo {get;}

		public StringSchema()
		{
			this.charset = DEFAULT_CHARSET;
			this.SchemaInfo = DEFAULT_SCHEMA_INFO;
		}

		public StringSchema(Charset Charset)
		{
			this.charset = Charset;
			IDictionary<string, string> Properties = new Dictionary<string, string>();
			Properties[CharsetKey] = Charset.name();
			this.SchemaInfo = (new SchemaInfo()).setName(DEFAULT_SCHEMA_INFO.Name).setType(SchemaType.STRING).setSchema(DEFAULT_SCHEMA_INFO.Schema).setProperties(Properties);
		}

		public virtual sbyte[] Encode(string Message)
		{
			if (null == Message)
			{
				return null;
			}
			else
			{
				return Message.GetBytes(charset);
			}
		}

		public virtual string Decode(sbyte[] Bytes)
		{
			if (null == Bytes)
			{
				return null;
			}
			else
			{
				return StringHelper.NewString(Bytes, charset);
			}
		}

		public virtual string Decode(ByteBuf ByteBuf)
		{
			if (null == ByteBuf)
			{
				return null;
			}
			else
			{
				int Size = ByteBuf.readableBytes();
				sbyte[] Bytes = tmpBuffer.get();
				if (Size > Bytes.Length)
				{
					Bytes = new sbyte[Size * 2];
					tmpBuffer.set(Bytes);
				}
				ByteBuf.readBytes(Bytes, 0, Size);

				return StringHelper.NewString(Bytes, 0, Size, charset);
			}
		}

	}

}