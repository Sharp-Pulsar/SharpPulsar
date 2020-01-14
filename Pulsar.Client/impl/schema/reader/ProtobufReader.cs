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
namespace org.apache.pulsar.client.impl.schema.reader
{
	using InvalidProtocolBufferException = com.google.protobuf.InvalidProtocolBufferException;
	using Parser = com.google.protobuf.Parser;
	using SchemaSerializationException = org.apache.pulsar.client.api.SchemaSerializationException;
	using SchemaReader = org.apache.pulsar.client.api.schema.SchemaReader;

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	public class ProtobufReader<T> : SchemaReader<T> where T : com.google.protobuf.GeneratedMessageV3
	{
		private Parser<T> tParser;

		public ProtobufReader(T protoMessageInstance)
		{
			tParser = (Parser<T>)(protoMessageInstance).ParserForType;
		}

		public override T read(sbyte[] bytes, int offset, int length)
		{
			try
			{
				return this.tParser.parseFrom(bytes, offset, length);
			}
			catch (InvalidProtocolBufferException e)
			{
				throw new SchemaSerializationException(e);
			}
		}

		public override T read(Stream inputStream)
		{
			try
			{
				return this.tParser.parseFrom(inputStream);
			}
			catch (InvalidProtocolBufferException e)
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
					log.error("ProtobufReader close inputStream close error", e.Message);
				}
			}
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ProtobufReader));
	}

}