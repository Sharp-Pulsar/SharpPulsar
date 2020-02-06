using System;
using System.IO;

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
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = SharpPulsar.Api.SchemaSerializationException;
	using SharpPulsar.Api.Schema;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class JsonReader<T> : ISchemaReader<T>
	{
		private readonly Type pojo = typeof(T);
		private readonly ObjectMapper objectMapper;

		public JsonReader(ObjectMapper ObjectMapper, Type Pojo)
		{
			this.pojo = Pojo;
			this.objectMapper = ObjectMapper;
		}

		public override T Read(sbyte[] Bytes, int Offset, int Length)
		{
			try
			{
				return objectMapper.readValue(Bytes, Offset, Length, this.pojo);
			}
			catch (IOException E)
			{
				throw new SchemaSerializationException(E);
			}
		}

		public override T Read(Stream InputStream)
		{
			try
			{
				return objectMapper.readValue(InputStream, pojo);
			}
			catch (IOException E)
			{
				throw new SchemaSerializationException(E);
			}
			finally
			{
				try
				{
					InputStream.Close();
				}
				catch (IOException E)
				{
					log.error("JsonReader close inputStream close error", E.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(JsonReader));
	}

}