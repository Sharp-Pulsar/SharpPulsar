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
namespace org.apache.pulsar.client.impl.schema.writer
{
	using JsonProcessingException = com.fasterxml.jackson.core.JsonProcessingException;
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaWriter = org.apache.pulsar.client.api.schema.SchemaWriter;

	public class JsonWriter<T> : SchemaWriter<T>
	{

		private readonly ObjectMapper objectMapper;

		public JsonWriter(ObjectMapper objectMapper)
		{
			this.objectMapper = objectMapper;
		}

		public override sbyte[] write(T message)
		{
			try
			{
				return objectMapper.writeValueAsBytes(message);
			}
			catch (JsonProcessingException e)
			{
				throw new SchemaSerializationException(e);
			}
		}
	}

}