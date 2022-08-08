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
    using SharpPulsar.Interfaces.ISchema;
    using SharpPulsar.Protocol.Schema;

    /// <summary>
    /// A multi version generic avro reader.
    /// </summary>
    public class MultiVersionGenericAvroReader : AbstractMultiVersionGenericReader
    {

        public MultiVersionGenericAvroReader(bool useProvidedSchemaAsReaderSchema, Avro.Schema readerSchema) : base(useProvidedSchemaAsReaderSchema, new GenericAvroReader(readerSchema), readerSchema)
        {
        }

        protected internal override ISchemaReader<IGenericRecord> LoadReader(BytesSchemaVersion schemaVersion)
        {
            var schemaInfo = GetSchemaInfoByVersion(schemaVersion.Get());
            if (schemaInfo != null)
            {
                //LOG.info("Load schema reader for version({}), schema is : {}", SchemaUtils.GetStringSchemaVersion(schemaVersion.Get()), schemaInfo);
                var writerSchema = SchemaUtils.ParseAvroSchema(schemaInfo.SchemaDefinition);
                var readerSchema = useProvidedSchemaAsReaderSchema ? ReaderSchema : writerSchema;
                //readerSchema.GetProperty.addProp(GenericAvroSchema.OFFSET_PROP, schemaInfo.Properties.GetOrDefault(GenericAvroSchema.OFFSET_PROP, "0"));

                return new GenericAvroReader(writerSchema, readerSchema, schemaVersion.Get());
            }
            else
            {
                //LOG.warn("No schema found for version({}), use latest schema : {}", SchemaUtils.getStringSchemaVersion(schemaVersion.get()), this.readerSchema);
                return providerSchemaReader;
            }
        }
    }
}
