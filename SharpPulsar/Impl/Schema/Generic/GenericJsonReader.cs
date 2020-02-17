using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using SharpPulsar.Exception;
using SharpPulsar.Impl.Conf;
using SchemaSerializationException = SharpPulsar.Exceptions.SchemaSerializationException;

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
namespace SharpPulsar.Impl.Schema.Generic
{
	using Field = Api.Schema.Field;
	using IGenericRecord = Api.Schema.IGenericRecord;
	using SharpPulsar.Api.Schema;


	public class GenericJsonReader : ISchemaReader<IGenericRecord>
	{

		private readonly ObjectMapper _objectMapper;
		private readonly sbyte[] _schemaVersion;
		private readonly IList<Field> _fields;
		public GenericJsonReader(IList<Field> fields)
		{
			_fields = fields;
			_schemaVersion = null;
			_objectMapper = new ObjectMapper();
		}

		public GenericJsonReader(sbyte[] schemaVersion, IList<Field> fields)
		{
			_objectMapper = new ObjectMapper();
			_fields = fields;
			_schemaVersion = schemaVersion;
		}
		public GenericJsonRecord Read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				var jn = _objectMapper.ReadValue(StringHelper.NewString(bytes, offset, length, Encoding.UTF8.EncodingName));
				return new GenericJsonRecord(_schemaVersion, _fields, jn);
			}
			catch (IOException ioe)
			{
				throw new SchemaSerializationException(ioe);
			}
		}

		public IGenericRecord Read(Stream inputStream)
		{
			try
			{
				var jn = _objectMapper.ReadValue(inputStream);
				return new GenericJsonRecord(_schemaVersion, _fields, jn);
			}
			catch (IOException ioe)
			{
				throw new SchemaSerializationException(ioe);
			}
			finally
			{
				try
				{
					inputStream.Close();
				}
				catch (IOException e)
				{
					Log.LogError("GenericJsonReader close inputStream close error", e.Message);
				}
			}
		}

		IGenericRecord ISchemaReader<IGenericRecord>.Read(sbyte[] Bytes, int Offset, int Length)
		{
			throw new System.NotImplementedException();
		}

		private static readonly ILogger Log = new LoggerFactory().CreateLogger(typeof(GenericJsonReader));
	}

}