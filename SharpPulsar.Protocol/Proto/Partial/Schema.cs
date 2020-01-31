
using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
    public partial class Schema : ByteBufCodedOutputStream.ByteBufGeneratedMessage
    {
        // Use Schema.newBuilder() to construct.
        internal ThreadLocalPool.Handle _handle;
        private Schema(ThreadLocalPool.Handle handle)
        {
            _handle = handle;
        }

        internal static ThreadLocalPool<Schema> _pool = new ThreadLocalPool<Schema>(handle => new Schema(handle), 1, true);

        public void Recycle()
        {
            InitFields();
            MemoizedIsInitialized = -1;
            _hasBits0 = 0;
            MemoizedSerializedSize = -1;
            if (_handle != null)
            {
                _handle.Release(this);
            }
        }

        public Schema(bool NoInit)
        {
        }
        internal static readonly Schema _defaultInstance;
        public static Schema DefaultInstance
        {
            get
            {
                return _defaultInstance;
            }
        }

        public Schema DefaultInstanceForType
        {
            get
            {
                return _defaultInstance;
            }
        }

        
        public IList<KeyValue> PropertiesList
        {
            get
            {
                return Properties;
            }
        }

        public int PropertiesCount
        {
            get
            {
                return Properties.Count;
            }
        }
        public KeyValue GetProperties(int Index)
        {
            return Properties[Index];
        }
        public static Types.Type ValueOf(int Value)
        {
            return Value switch
            {
                0 => Types.Type.None,
                1 => Types.Type.String,
                2 => Types.Type.Json,
                3 => Types.Type.Protobuf,
                4 => Types.Type.Avro,
                5 => Types.Type.Bool,
                6 => Types.Type.Int8,
                7 => Types.Type.Int16,
                8 => Types.Type.Int32,
                9 => Types.Type.Int64,
                10 => Types.Type.Float,
                11 => Types.Type.Double,
                12 => Types.Type.Date,
                13 => Types.Type.Time,
                14 => Types.Type.Timestamp,
                15 => Types.Type.KeyValue,
                _ => Types.Type.None,
            };
        }

        public void InitFields()
        {
            Name = "";
            SchemaData = ByteString.Empty;
            type_ = Types.Type.None;
        }
        internal sbyte MemoizedIsInitialized = -1;
        public bool Initialized
        {
            get
            {
                sbyte IsInitialized = MemoizedIsInitialized;
                if (IsInitialized != -1)
                {
                    return IsInitialized == 1;
                }

                if (!HasName)
                {
                    MemoizedIsInitialized = 0;
                    return false;
                }
                if (!HasSchemaData)
                {
                    MemoizedIsInitialized = 0;
                    return false;
                }
                if (!HasType)
                {
                    MemoizedIsInitialized = 0;
                    return false;
                }
                for (int i = 0; i < PropertiesCount; i++)
                {
                    if (GetProperties(i) != null)
                    {
                        MemoizedIsInitialized = 0;
                        return false;
                    }
                }
                MemoizedIsInitialized = 1;
                return true;
            }
        }

        public void WriteTo(ByteBufCodedOutputStream Output)
        {
            var _ = SerializedSize;
            if (((_hasBits0 & 0x00000001) == 0x00000001))
            {
                Output.WriteBytes(1, ByteString.CopyFromUtf8(Name));
            }
            if (((_hasBits0 & 0x00000002) == 0x00000002))
            {
                Output.WriteBytes(3, SchemaData);
            }
            if (((_hasBits0 & 0x00000004) == 0x00000004))
            {
                Output.WriteEnum(4, (int)type_);
            }
            for (int I = 0; I < PropertiesCount; I++)
            {
                Output.WriteMessage(5, Properties[I]);
            }
        }

        public int SerializedSize => CalculateSize();
        internal int MemoizedSerializedSize = -1;

        internal const long SerialVersionUID = 0L;

        public static Builder NewBuilder()
        {
            return Builder.Create();
        }
        public Builder NewBuilderForType()
        {
            return NewBuilder();
        }
        public static Builder NewBuilder(Schema Prototype)
        {
            return NewBuilder().MergeFrom(Prototype);
        }
        public Builder ToBuilder()
        {
            return NewBuilder(this);
        }

        public sealed class Builder: ByteBufMessageBuilder
        {
            internal ThreadLocalPool.Handle _handle;
            public Builder(ThreadLocalPool.Handle handle)
            {
                _handle = handle;
                MaybeForceBuilderInitialization();
            }

            internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);


            public void Recycle()
            {
                Clear();
                if (_handle != null)
                {
                    _handle.Release(this);
                }
            }

            public void MaybeForceBuilderInitialization()
            {
            }
            internal static Builder Create()
            {
                return _pool.Take();
            }

            public Builder Clear()
            {
                _name = "";
                _bitField = (_bitField & ~0x00000001);
                _schemaData = ByteString.Empty;
                _bitField = (_bitField & ~0x00000002);
                _type = Types.Type.None;
                _bitField = (_bitField & ~0x00000004);
                _properties = new List<KeyValue>();
                _bitField = (_bitField & ~0x00000008);
                return this;
            }

            public Schema DefaultInstanceForType
            {
                get
                {
                    return DefaultInstance;
                }
            }

            public Schema Build()
            {
                Schema result = BuildPartial();
                if (!result.Initialized)
                {
                    throw new NullReferenceException("Schema not initialized");
                }
                return result;
            }

            internal IList<KeyValue> Properties_;
            public Schema BuildPartial()
            {
                var result = Schema._pool.Take();
                int from_bitField = _bitField;
                int to_bitField = 0;
                if (((from_bitField & 0x00000001) == 0x00000001))
                {
                    to_bitField |= 0x00000001;
                }
                result.Name = _name.ToString();
                if (((from_bitField & 0x00000002) == 0x00000002))
                {
                    to_bitField |= 0x00000002;
                }
                result.SchemaData = _schemaData;
                if (((from_bitField & 0x00000004) == 0x00000004))
                {
                    to_bitField |= 0x00000004;
                }
                result.type_ = _type;
                if (((_bitField & 0x00000008) == 0x00000008))
                {
                    _properties = new List<KeyValue>(_properties);
                    _bitField = (_bitField & ~0x00000008);
                }
                result.Properties.Clear();
                _properties.ToList().ForEach(result.Properties.Add);
                result._hasBits0 = to_bitField;
                return result;
            }
            public Builder MergeFrom(Schema Other)
            {
                if (Other == DefaultInstance)
                {
                    return this;
                }
                if (Other.HasName)
                {
                    SetName(Other.Name);
                }
                if (Other.HasSchemaData)
                {
                    SetSchemaData(Other.SchemaData);
                }
                if (Other.HasType)
                {
                    SetType(Other.type_);
                }
                if (Other.Properties.Count > 0)
                {
                    if (Properties_.Count == 0)
                    {
                        Properties_ = Other.Properties;
                        _bitField = (_bitField & ~0x00000008);
                    }
                    else
                    {
                        EnsurePropertiesIsMutable();
                        ((List<KeyValue>)Properties_).AddRange(Other.Properties);
                    }

                }
                return this;
            }
            public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream input, ExtensionRegistry extensionRegistry)
            {
                while (true)
                {
                    int Tag = input.ReadTag();
                    switch (Tag)
                    {
                        case 0:

                            return this;
                        default:
                            {
                                if (!input.SkipField(Tag))
                                {

                                    return this;
                                }
                                break;
                            }
                        case 10:
                            {
                                _bitField |= 0x00000001;
                                _name = input.ReadBytes();
                                break;
                            }
                        case 26:
                            {
                                _bitField |= 0x00000002;
                                _schemaData = input.ReadBytes();
                                break;
                            }
                        case 32:
                            {
                                int RawValue = input.ReadEnum();
                                Schema.Types.Type Value = Enum.GetValues(typeof(Types.Type)).Cast<Types.Type>().ToList()[RawValue];
                                if (Value != null)
                                {
                                    _bitField |= 0x00000004;
                                    _type = Value;
                                }
                                break;
                            }
                        case 42:
                            {
                                KeyValue.Builder SubBuilder =KeyValue.NewBuilder();
                                input.ReadMessage(SubBuilder, extensionRegistry);
                                AddProperties(SubBuilder.BuildPartial());
                                break;
                            }
                    }
                }
            }
            public bool Initialized
            {
                get
                {
                    if (!HasName())
                    {

                        return false;
                    }
                    if (!HasSchemaData())
                    {

                        return false;
                    }
                    if (!HasType())
                    {

                        return false;
                    }
                    for (int I = 0; I < PropertiesCount; I++)
                    {
                        if (GetProperties(I) != null)
                        {

                            return false;
                        }
                    }
                    return true;
                }
            }

            internal int _bitField;

            // required string name = 1;
            private object _name = "";
            public bool HasName()
            {
                return ((_bitField & 0x00000001) == 0x00000001);
            }
            public string GetName()
            {
                return _name.ToString();
            }
            public Builder SetName(string Value)
            {
                if (Value is null)
                {
                    throw new NullReferenceException();
                }
                _bitField |= 0x00000001;
                _name = Value;

                return this;
            }
            public Builder ClearName()
            {
                _bitField = (_bitField & ~0x00000001);
                _name = DefaultInstance.Name;

                return this;
            }

            // required bytes schema_data = 3;
            private ByteString _schemaData = ByteString.Empty;
            public bool HasSchemaData()
            {
                return ((_bitField & 0x00000002) == 0x00000002);
            }

            public Builder SetSchemaData(ByteString value)
            {
                if (value == null)
                {
                    throw new NullReferenceException();
                }
                _bitField |= 0x00000002;
                _schemaData = value;

                return this;
            }

            // required .pulsar.proto.Schema.Type type = 4;
            internal Types.Type _type = Types.Type.None;
            public bool HasType()
            {
                return ((_bitField & 0x00000004) == 0x00000004);
            }
            public Types.Type Type
            {
                get
                {
                    return _type;
                }
            }
            public Builder SetType(Types.Type value)
            {
                if (value == null)
                {
                    throw new System.NullReferenceException();
                }
                _bitField |= 0x00000004;
                _type = value;

                return this;
            }
            public Builder ClearType()
            {
                _bitField = (_bitField & ~0x00000004);
                _type = Schema.Types.Type.None;

                return this;
            }

            // repeated .pulsar.proto.KeyValue properties = 5;
            internal IList<KeyValue> _properties = new List<KeyValue>();
            public void EnsurePropertiesIsMutable()
            {
                if (!((_bitField & 0x00000008) == 0x00000008))
                {
                    _properties = new List<KeyValue>(_properties);
                    _bitField |= 0x00000008;
                }
            }

            public IList<KeyValue> PropertiesList
            {
                get
                {
                    return new List<KeyValue>(_properties);
                }
            }
            public int PropertiesCount
            {
                get
                {
                    return _properties.Count;
                }
            }
            public KeyValue GetProperties(int Index)
            {
                return _properties[Index];
            }
            public Builder SetProperties(int Index, KeyValue Value)
            {
                if (Value == null)
                {
                    throw new System.NullReferenceException();
                }
                EnsurePropertiesIsMutable();
                _properties[Index] = Value;

                return this;
            }

            public Builder AddProperties(KeyValue Value)
            {
                if (Value == null)
                {
                    throw new System.NullReferenceException();
                }
                EnsurePropertiesIsMutable();
                _properties.Add(Value);

                return this;
            }
            public Builder AddProperties(int Index, KeyValue Value)
            {
                if (Value == null)
                {
                    throw new System.NullReferenceException();
                }
                EnsurePropertiesIsMutable();
                _properties.Insert(Index, Value);

                return this;
            }


            public Builder AddAllProperties(IEnumerable<KeyValue> values)
            {
                EnsurePropertiesIsMutable();
                values.ToList().ForEach(x => Properties_.Add(x));

                return this;
            }
            public Builder ClearProperties()
            {
                _properties = new List<KeyValue>();
                _bitField = (_bitField & ~0x00000008);

                return this;
            }
            public Builder RemoveProperties(int Index)
            {
                EnsurePropertiesIsMutable();
                _properties.RemoveAt(Index);

                return this;
            }

            // @@protoc_insertion_point(builder_scope:pulsar.proto.Schema)
        }

        static Schema()
        {
            _defaultInstance = new Schema(true);
            _defaultInstance.InitFields();
        }

        // @@protoc_insertion_point(class_scope:pulsar.proto.Schema)
    }


}
