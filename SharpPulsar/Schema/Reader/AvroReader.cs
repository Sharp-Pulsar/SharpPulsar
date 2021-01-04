using Avro.Generic;
using SharpPulsar.Interfaces.ISchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpPulsar.Schema.Reader
{
    public class AvroReader<T> : ISchemaReader<T>
    {
        private GenericReader<T> _reader;
        
        public T Read(Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public T Read(sbyte[] bytes, int offset, int length)
        {
            throw new NotImplementedException();
        }
    }
}
