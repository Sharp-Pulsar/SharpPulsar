using SharpPulsar.Extension;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Text;

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
namespace SharpPulsar.Schemas
{

	/// <summary>
	/// Schema definition for Strings encoded in UTF-8 format.
	/// </summary>
	public class StringSchema : AbstractSchema<string>
	{

		public static readonly string CHARSET_KEY;

		private static readonly ISchemaInfo DefaultSchemaInfo;
		private static readonly Encoding DefaultEncoding;
		private static readonly StringSchema UTF8;

		static StringSchema()
		{
			// Ensure the ordering of the static initialization
			CHARSET_KEY = "__charset";
			DefaultEncoding = Encoding.UTF8;
			var info = new SchemaInfo
			{
				Name = "String",
				Type = SchemaType.STRING,
				Schema = new sbyte[0]
			};
			DefaultSchemaInfo = info;

			UTF8 = new StringSchema(Encoding.UTF8);
		}

		public static StringSchema FromSchemaInfo(ISchemaInfo schemaInfo)
		{
			if(SchemaType.STRING != schemaInfo.Type)
				throw new ArgumentException("Not a string schema");

			string charsetName = schemaInfo.Properties[CHARSET_KEY];
			if (null == charsetName)
			{
				return UTF8;
			}
			else
			{
				return new StringSchema(Encoding.GetEncoding(charsetName));
			}
		}

		public static StringSchema Utf8()
		{
			return UTF8;
		}

		private readonly Encoding _encoding;
		private readonly ISchemaInfo _schemaInfo;

		public StringSchema()
		{
			_encoding = DefaultEncoding;
			_schemaInfo =  DefaultSchemaInfo;
		}

		public StringSchema(Encoding encoding)
		{
			_encoding = encoding;
            IDictionary<string, string> properties = new Dictionary<string, string>
            {
                [CHARSET_KEY] = encoding.WebName
            };
			var info = new SchemaInfo
			{
				Name = DefaultSchemaInfo.Name,
				Type = SchemaType.STRING,
				Schema = DefaultSchemaInfo.Schema,
				Properties = properties
			};
			_schemaInfo = info;
		}

		public override sbyte[] Encode(string message)
		{
			if (null == message)
			{
				return null;
			}
			else
			{
				return _encoding.GetBytes(message).ToSBytes();
			}
		}

		public override string Decode(sbyte[] bytes)
		{
			if (null == bytes)
			{
				return string.Empty;
			}
			else
			{
				return _encoding.GetString(bytes.ToBytes());
			}
		}

		public override ISchemaInfo SchemaInfo
		{
			get
			{
				return _schemaInfo;
			}
		}
	}

}