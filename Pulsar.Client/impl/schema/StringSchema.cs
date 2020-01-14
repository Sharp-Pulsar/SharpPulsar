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
namespace org.apache.pulsar.client.impl.schema
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;


	using ByteBuf = io.netty.buffer.ByteBuf;
	using FastThreadLocal = io.netty.util.concurrent.FastThreadLocal;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaType = org.apache.pulsar.common.schema.SchemaType;


	/// <summary>
	/// Schema definition for Strings encoded in UTF-8 format.
	/// </summary>
	public class StringSchema : AbstractSchema<string>
	{

		internal static readonly string CHARSET_KEY;

		private static readonly SchemaInfo DEFAULT_SCHEMA_INFO;
		private static readonly Charset DEFAULT_CHARSET;
		private static readonly StringSchema UTF8;

		static StringSchema()
		{
			// Ensure the ordering of the static initialization
			CHARSET_KEY = "__charset";
			DEFAULT_CHARSET = StandardCharsets.UTF_8;
			DEFAULT_SCHEMA_INFO = (new SchemaInfo()).setName("String").setType(SchemaType.STRING).setSchema(new sbyte[0]);

			UTF8 = new StringSchema(StandardCharsets.UTF_8);
		}

		private static readonly FastThreadLocal<sbyte[]> tmpBuffer = new FastThreadLocalAnonymousInnerClass();

		private class FastThreadLocalAnonymousInnerClass : FastThreadLocal<sbyte[]>
		{
			protected internal override sbyte[] initialValue()
			{
				return new sbyte[1024];
			}
		}

		public static StringSchema fromSchemaInfo(SchemaInfo schemaInfo)
		{
			checkArgument(SchemaType.STRING == schemaInfo.Type, "Not a string schema");
			string charsetName = schemaInfo.Properties.get(CHARSET_KEY);
			if (null == charsetName)
			{
				return UTF8;
			}
			else
			{
				return new StringSchema(Charset.forName(charsetName));
			}
		}

		public static StringSchema utf8()
		{
			return UTF8;
		}

		private readonly Charset charset;
		private readonly SchemaInfo schemaInfo;

		public StringSchema()
		{
			this.charset = DEFAULT_CHARSET;
			this.schemaInfo = DEFAULT_SCHEMA_INFO;
		}

		public StringSchema(Charset charset)
		{
			this.charset = charset;
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties[CHARSET_KEY] = charset.name();
			this.schemaInfo = (new SchemaInfo()).setName(DEFAULT_SCHEMA_INFO.Name).setType(SchemaType.STRING).setSchema(DEFAULT_SCHEMA_INFO.Schema).setProperties(properties);
		}

		public virtual sbyte[] encode(string message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return message.GetBytes(charset);
			}
		}

		public virtual string decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return null;
			}
			else
			{
				return StringHelper.NewString(bytes, charset);
			}
		}

		public override string decode(ByteBuf byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			else
			{
				int size = byteBuf.readableBytes();
				sbyte[] bytes = tmpBuffer.get();
				if (size > bytes.Length)
				{
					bytes = new sbyte[size * 2];
					tmpBuffer.set(bytes);
				}
				byteBuf.readBytes(bytes, 0, size);

				return StringHelper.NewString(bytes, 0, size, charset);
			}
		}

		public virtual SchemaInfo SchemaInfo
		{
			get
			{
				return schemaInfo;
			}
		}
	}

}