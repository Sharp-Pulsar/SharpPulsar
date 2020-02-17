using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Common;
using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Schema;
using SharpPulsar.Shared;

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


	/// <summary>
	/// Schema definition for Strings encoded in UTF-8 format.
	/// </summary>
	public class StringSchema : AbstractSchema<string>
	{

		internal static readonly string CharsetKey;

		private static readonly SchemaInfo DefaultSchemaInfo;
		private static readonly CharSet DefaultCharset;
		private static readonly StringSchema _utf8;

		static StringSchema()
		{
			// Ensure the ordering of the static initialization
			CharsetKey = "__charset";
			DefaultCharset = CharSet.Ansi;
			DefaultSchemaInfo = new SchemaInfo()
            {
				Name = "String",
				Type = SchemaType.String,
				Schema = Array.Empty<sbyte>()
			};

			_utf8 = new StringSchema(CharSet.Ansi);
		}

		private static readonly FastThreadLocal<sbyte[]> TmpBuffer = new FastThreadLocalAnonymousInnerClass();

		public class FastThreadLocalAnonymousInnerClass : FastThreadLocal<sbyte[]>
		{
			public  sbyte[] InitialValue()
			{
				return new sbyte[1024];
			}
		}

		public static StringSchema FromSchemaInfo(SchemaInfo schemaInfo)
		{
			if(SchemaType.String != schemaInfo.Type)
                throw new ArgumentException("Not a string schema");
			var charsetName = schemaInfo.Properties[CharsetKey];
			if (null == charsetName)
			{
				return _utf8;
			}
			else
            {
                var charSet = Enum.GetValues(typeof(CharSet)).Cast<CharSet>().FirstOrDefault(x => x.ToString().Equals(charsetName, StringComparison.OrdinalIgnoreCase));
				return new StringSchema(charSet);
			}
		}

		public static StringSchema Utf8()
		{
			return _utf8;
		}

		private readonly CharSet _charset;

		public StringSchema()
		{
			this._charset = DefaultCharset;
			this.SchemaInfo = DefaultSchemaInfo;
		}

		public StringSchema(CharSet charset)
		{
			this._charset = charset;
			IDictionary<string, string> properties = new Dictionary<string, string>
			{
				[CharsetKey] = charset.ToString()
			};
			this.SchemaInfo = new SchemaInfo()
            {
                Name = DefaultSchemaInfo.Name,
                Type = SchemaType.String,
                Schema = DefaultSchemaInfo.Schema,
                Properties = properties
            };
        }

        public override ISchema<IGenericRecord> Auto()
        {
            throw new NotImplementedException();
        }

        public override ISchema<string> Json(ISchemaDefinition<string> schemaDefinition)
        {
            throw new NotImplementedException();
        }

        public override ISchema<string> Json(string pojo)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureSchemaInfo(string topic, string componentName, SchemaInfo schemaInfo)
        {
            throw new NotImplementedException();
        }

        public override bool RequireFetchingSchemaInfo()
        {
            throw new NotImplementedException();
        }

        public override string Decode(sbyte[] bytes, sbyte[] schemaVersion)
        {
            throw new NotImplementedException();
        }

        public override ISchemaInfo SchemaInfo { get; }
        public override bool SupportSchemaVersioning()
        {
            throw new NotImplementedException();
        }

        public override ISchemaInfoProvider SchemaInfoProvider
        {
            set => throw new NotImplementedException();
        }

        public override sbyte[] Encode(string message)
        {
            return message?.GetBytes(Encoding.GetEncoding(_charset.ToString()));
        }

        public override void Validate(sbyte[] message)
        {
            throw new NotImplementedException();
        }

        public override string Decode(sbyte[] bytes)
        {
            return null == bytes ? null : StringHelper.NewString(bytes, _charset.ToString());
        }

		public override string Decode(IByteBuffer byteBuf)
		{
			if (null == byteBuf)
			{
				return null;
			}
			else
			{
				var size = byteBuf.ReadableBytes;
				var bytes = TmpBuffer.Value;
				if (size > bytes.Length)
				{
					bytes = new sbyte[size * 2];
					TmpBuffer.Value = bytes;
				}
				byteBuf.WriteBytes((byte[])(Array)bytes, 0, size);

				return StringHelper.NewString(bytes, 0, size, _charset.ToString());
			}
		}

	}

}