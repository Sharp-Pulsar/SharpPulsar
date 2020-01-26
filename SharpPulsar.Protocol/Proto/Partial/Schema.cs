
using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public partial class Schema
	{
		// Use Schema.newBuilder() to construct.
		internal ThreadLocalPool.Handle _handle;
		public Schema(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}

		internal static ThreadLocalPool<Schema> _pool = new ThreadLocalPool<Schema>(handle => new Schema(handle), 1, true);
		
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			_bitField = 0;
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
		
		private int _bitField;
		// required string name = 1;
		public const int NameFieldNumber = 1;
		public bool HasName()
		{
			return ((_bitField & 0x00000001) == 0x00000001);
		}
		public string NameBytes
		{
			get
			{
				return Name;
			}
		}

		// required bytes schema_data = 3;
		public const int SchemaDataFieldNumber = 3;
		public bool HasSchemaData()
		{
			return ((_bitField & 0x00000002) == 0x00000002);
		}
		
		// required .pulsar.proto.Schema.Type type = 4;
		public const int TypeFieldNumber = 4;
		public bool HasType()
		{
			return ((_bitField & 0x00000004) == 0x00000004);
		}
		public Type GetSchemaType()
		{
			return type;
		}

		// repeated .pulsar.proto.KeyValue properties = 5;
		public const int PropertiesFieldNumber = 5;
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
		
		public void InitFields()
		{
			Name = "";
			SchemaData = null;
			type = Type.None;
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

				if (!HasName())
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSchemaData())
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasType())
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
		
		public sealed class Builder
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
				_schemaData = new byte[0];
				_bitField = (_bitField & ~0x00000002);
				_type = Type.None;
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


			public Schema BuildPartial()
			{
				var result = Schema._pool.Take();
				int from_bitField = _bitField;
				int to_bitField = 0;
				if (((from_bitField & 0x00000001) == 0x00000001))
				{
					to_bitField |= 0x00000001;
				}
				result.Name = _name;
				if (((from_bitField & 0x00000002) == 0x00000002))
				{
					to_bitField |= 0x00000002;
				}
				result.SchemaData = _schemaData;
				if (((from_bitField & 0x00000004) == 0x00000004))
				{
					to_bitField |= 0x00000004;
				}
				result.type = _type;
				if (((_bitField & 0x00000008) == 0x00000008))
				{
					_properties = new List<KeyValue>(_properties);
					_bitField = (_bitField & ~0x00000008);
				}
				result.Properties.Clear();
				_properties.ToList().ForEach(result.Properties.Add);
				result._bitField = to_bitField;
				return result;
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
			private string _name = "";
			public bool HasName()
			{
				return ((_bitField & 0x00000001) == 0x00000001);
			}
			public string GetName()
			{
				return _name;
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
			private byte[] _schemaData = new byte[0];
			public bool HasSchemaData()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			
			public Builder SetSchemaData(byte[] value)
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
			internal Schema.Type _type = Schema.Type.None;
			public bool HasType()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public Schema.Type Type
			{
				get
				{
					return _type;
				}
			}
			public Builder SetType(Type value)
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
				_type = Schema.Type.None;

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
			
			
			public Builder AddAllProperties<T1>(IEnumerable<T1> values) where T1 : KeyValue
			{
				EnsurePropertiesIsMutable();
				values.ToList().ForEach(_properties.Add);

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
