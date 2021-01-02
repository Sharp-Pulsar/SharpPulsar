using Avro.Generic;
using SharpPulsar.Pulsar.Api.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpPulsar.Pulsar.Schema.Reader
{
    public class AvroReader : ISchemaReader
    {
        private GenericReader _reader;
        public object Read(sbyte[] bytes, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public object Read(Stream inputStream)
        {
            throw new NotImplementedException();
        }
    }
}
