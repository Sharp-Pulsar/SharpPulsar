using Pulsar.Api.Schema;
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
namespace Pulsar.Client.Impl.Schema.Reader
{
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = Api.SchemaSerializationException;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class JsonReader<T> : SchemaReader<T>
	{
		private readonly Type pojo = typeof(T);
		private readonly ObjectMapper objectMapper;

		public JsonReader(ObjectMapper objectMapper, Type pojo)
		{
			this.pojo = pojo;
			this.objectMapper = objectMapper;
		}

		public T Read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				return objectMapper.readValue(bytes, offset, length, this.pojo);
			}
			catch (IOException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public T Read(Stream inputStream)
		{
			try
			{
				return objectMapper.readValue(inputStream, pojo);
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
					log.error("JsonReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(JsonReader));
	}

}