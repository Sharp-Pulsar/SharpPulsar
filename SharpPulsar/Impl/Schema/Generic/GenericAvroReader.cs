using System;
using System.IO;
using Avro.IO;
using Avro.Reflect;
using Microsoft.Extensions.Logging;

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

    public class GenericAvroReader
	{
        private readonly Avro.Schema _schema;
		private readonly sbyte[] _schemaVersion;
		
		public GenericAvroReader(Avro.Schema schema, sbyte[] schemaVersion)
        {
            _schema = schema;
            _schemaVersion = schemaVersion;
        }

		public object Read(byte[] message, Type returnType)
        {
            var r = new ReflectDefaultReader(returnType, _schema, _schema, new ClassCache());
            using var stream = new MemoryStream(message);
            return r.Read(default(object), new BinaryDecoder(stream));
        }
		
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(GenericAvroReader));
	}

}