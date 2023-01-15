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
namespace SharpPulsar.Schemas.Generic
{
    using Avro.Generic;
    using System.IO;
    using Avro.IO;
    using System;
    using SharpPulsar.Exceptions;
    using SharpPulsar.Interfaces.Schema;

    public class GenericAvroWriter : ISchemaWriter<IGenericRecord>
    {

        private readonly GenericDatumWriter<GenericRecord> _writer;
        private readonly BinaryEncoder _encoder;
        private readonly MemoryStream _byteArrayOutputStream;

        public GenericAvroWriter(Avro.RecordSchema schema)
        {
            _writer = new GenericDatumWriter<GenericRecord>(schema);
            _byteArrayOutputStream = new MemoryStream();
            _encoder = new BinaryEncoder(_byteArrayOutputStream);
        }

        public byte[] Write(IGenericRecord message)
        {
            lock (this)
            {
                try
                {
                    _writer.Write(((GenericAvroRecord)message).AvroRecord, _encoder);
                    _encoder.Flush();
                    var bytes = _byteArrayOutputStream.ToArray();
                    return bytes;
                }
                catch (Exception e)
                {
                    throw new SchemaSerializationException(e);
                }
                finally
                {
                    _byteArrayOutputStream.SetLength(0);
                }
            }
        }

    }
}
