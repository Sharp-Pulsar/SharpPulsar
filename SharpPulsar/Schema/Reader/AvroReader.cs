using Avro.Generic;
using SharpPulsar.Interfaces.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpPulsar.Schema.Reader
{
    public class AvroReader<T> : ISchemaReader<T>
    {
        private GenericReader<T> _reader;
        public T Read(byte[] bytes, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public T Read(Stream inputStream)
        {
            throw new NotImplementedException();
        }

    }
}
