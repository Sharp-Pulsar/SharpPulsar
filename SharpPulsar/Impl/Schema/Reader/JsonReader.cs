using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharpPulsar.Api.Schema;
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
namespace SharpPulsar.Impl.Schema.Reader
{

	public class JsonReader : ISchemaReader
	{
		private readonly ObjectMapper _objectMapper;

		public JsonReader(ObjectMapper objectMapper)
		{
			this._objectMapper = objectMapper;
		}

		public  object Read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				return _objectMapper.ReadValue((byte[])(Array)bytes, offset, length);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public object Read(Stream inputStream)
		{
			try
            {
                var t = _objectMapper.ReadValue(inputStream).Children<JObject>();
				
				return t;
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
			finally
			{
				try
				{
					inputStream.Close();
				}
				catch (IOException e)
				{
					Log.LogError("JsonReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(JsonReader));
	}

}