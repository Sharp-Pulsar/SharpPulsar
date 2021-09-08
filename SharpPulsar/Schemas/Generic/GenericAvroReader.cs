
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avro.Generic;
using Avro.IO;
using SharpPulsar.Exceptions;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Interfaces.Schema;
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
    using Schema = Avro.Schema;

    public class GenericAvroReader : ISchemaReader<IGenericRecord>
    {

        private readonly GenericDatumReader<GenericRecord> _reader;
        //private readonly MemoryStream _byteArrayOutputStream;
        private readonly IList<Field> _fields;
        private readonly Schema _schema;
        private readonly byte[] _schemaVersion;
        private readonly int _offset;

        public GenericAvroReader(Schema schema) : this(null, schema, null)
        {
        }

        public GenericAvroReader(Schema writerSchema, Schema readerSchema, byte[] schemaVersion)
        {
            _schema = readerSchema;
            _fields = ((Avro.RecordSchema)_schema).Fields.Select(f => new Field(f.Name, f.Pos)).ToList();
            _schemaVersion = schemaVersion;
            if (writerSchema == null)
            {
                _reader = new GenericDatumReader<GenericRecord>(readerSchema, readerSchema);
            }
            else
            {
                _reader = new GenericDatumReader<GenericRecord>(writerSchema, readerSchema);
            }
            //_byteArrayOutputStream = new MemoryStream();
            //_encoder =new BinaryEncoder(_byteArrayOutputStream);

            if (_schema.GetProperty(GenericAvroSchema.OFFSET_PROP) != null)
            {
                _offset = int.Parse(_schema.GetProperty(GenericAvroSchema.OFFSET_PROP).ToString());
            }
            else
            {
                _offset = 0;
            }

        }

        public IGenericRecord Read(Stream inputStream)
        {
            try
            {
                var decoder = new BinaryDecoder(inputStream);
                var avroRecord = _reader.Read(default, decoder);
                return new GenericAvroRecord(_schemaVersion, _schema, _fields, avroRecord);
            }
            catch (Exception e) when (e is IOException || e is System.IndexOutOfRangeException)
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
                    //log.error("GenericAvroReader close inputStream close error", e);
                }
            }
        }

        public IGenericRecord Read(byte[] bytes, int offset, int length)
        {
            using var stream = new MemoryStream(bytes);
            stream.Seek(offset, SeekOrigin.Begin);
            try
            {
                if (offset == 0 && _offset > 0)
                {
                    offset = _offset;
                }
                Decoder decoder = new BinaryDecoder(stream);
                var avroRecord = _reader.Read(default, decoder);
                return new GenericAvroRecord(_schemaVersion, _schema, _fields, avroRecord);
            }
            catch (Exception e) when (e is IOException || e is System.IndexOutOfRangeException)
            {
                throw new SchemaSerializationException(e);
            }
            throw new NotImplementedException();
        }

        public virtual int Offset
        {
            get
            {
                return _offset;
            }
        }

        public object NativeSchema
        {
            get
            {
                return _schema;
            }
        }

    }
}
