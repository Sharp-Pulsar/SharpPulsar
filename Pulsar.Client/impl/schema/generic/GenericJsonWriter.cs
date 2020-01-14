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
namespace Pulsar.Client.Impl.Schema.Generic
{
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using SchemaSerializationException = Api.SchemaSerializationException;
	using GenericRecord = Api.Schema.GenericRecord;
    using Pulsar.Api.Schema;
    using System.IO;

    public class GenericJsonWriter : SchemaWriter<GenericRecord>
	{

		private readonly ObjectMapper objectMapper;

		public GenericJsonWriter()
		{
			this.objectMapper = new ObjectMapper();
		}

		public sbyte[] Write(GenericRecord message)
		{
			try
			{
				return objectMapper.writeValueAsBytes(((GenericJsonRecord)message).JsonNode.ToString());
			}
			catch (IOException ioe)
			{
				throw new SchemaSerializationException(ioe);
			}
		}
	}

}