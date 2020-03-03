
using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
    public partial class Schema
    {
        
        public KeyValue GetProperties(int index)
        {
            return Properties[index];
        }
        public static Type ValueOf(int value)
        {
            return value switch
            {
                0 => Type.None,
                1 => Type.String,
                2 => Type.Json,
                3 => Type.Protobuf,
                4 => Type.Avro,
                5 => Type.Bool,
                6 => Type.Int8,
                7 => Type.Int16,
                8 => Type.Int32,
                9 => Type.Int64,
                10 => Type.Float,
                11 => Type.Double,
                12 => Type.Date,
                13 => Type.Time,
                14 => Type.Timestamp,
                15 => Type.KeyValue,
                _ => Type.None,
            };
        }

        public static Builder NewBuilder()
        {
            return Builder.Create();
        }
        
        public sealed class Builder
        {
            private Schema _schema;

            public Builder()
            {
                _schema = new Schema();
            }
            internal static Builder Create()
            {
                return new Builder();
            }

            public Schema Build()
            {
                return _schema;
            }
            public string GetName()
            {
                return _schema.Name;
            }
            public Builder SetName(string value)
            {
                if (value is null)
                {
                    throw new NullReferenceException();
                }

                _schema.Name = value;
                return this;
            }
           
            public Builder SetSchemaData(byte[] value)
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }

                _schema.SchemaData = value;
                return this;
            }

            public Builder SetType(Type value)
            {
                _schema.type = value;
                return this;
            }
            
            public KeyValue GetProperties(int index)
            {
                return _schema.Properties[index];
            }
            public Builder SetProperties(int index, KeyValue value)
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }
                _schema.Properties[index] = value;

                return this;
            }

            public Builder AddProperties(KeyValue value)
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }
                _schema.Properties.Add(value);

                return this;
            }
            public Builder AddProperties(int index, KeyValue value)
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }
                _schema.Properties.Insert(index, value);

                return this;
            }


            public Builder AddAllProperties(IEnumerable<KeyValue> values)
            {
                _schema.Properties.AddRange(values);

                return this;
            }
            
        }

    }


}
