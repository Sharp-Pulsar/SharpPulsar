using DotNetty.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPulsar.Protocol.Proto
{
	public partial class SingleMessageMetadata 
	{
		// Use SingleMessageMetadata.newBuilder() to construct.
		internal static ThreadLocalPool<SingleMessageMetadata> _pool = new ThreadLocalPool<SingleMessageMetadata>(handle => new SingleMessageMetadata(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private SingleMessageMetadata(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}


		public void Recycle()
		{
			this.InitFields();
			this.MemoizedIsInitialized = -1;
			this._bitField = 0;
			this.MemoizedSerializedSize = -1;
			if (_handle != null)
			{
				_handle.Release(this);
			}
		}

		public SingleMessageMetadata(bool NoInit)
		{
		}

		//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal static readonly SingleMessageMetadata _defaultInstance;
		public static SingleMessageMetadata DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public SingleMessageMetadata DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

		internal int _bitField;
		// repeated .pulsar.proto.KeyValue properties = 1;
		public const int PropertiesFieldNumber = 1;
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
		
		// optional string partition_key = 2;
		public const int PartitionKeyFieldNumber = 2;
		public bool HasPartitionKey()
		{
			return ((_bitField & 0x00000001) == 0x00000001);
		}
		
		// required int32 payload_size = 3;
		public const int PayloadSizeFieldNumber = 3;
		public bool HasPayloadSize()
		{
			return ((_bitField & 0x00000002) == 0x00000002);
		}
	

		// optional bool compacted_out = 4 [default = false];
		public const int CompactedOutFieldNumber = 4;
		public bool HasCompactedOut()
		{
			return ((_bitField & 0x00000004) == 0x00000004);
		}
		
		// optional uint64 event_time = 5 [default = 0];
		public const int EventTimeFieldNumber = 5;
		public bool HasEventTime()
		{
			return ((_bitField & 0x00000008) == 0x00000008);
		}
		
		// optional bool partition_key_b64_encoded = 6 [default = false];
		public const int PartitionKeyB64EncodedFieldNumber = 6;
		public bool HasPartitionKeyB64Encoded()
		{
			return ((_bitField & 0x00000010) == 0x00000010);
		}
		
		// optional bytes ordering_key = 7;
		public const int OrderingKeyFieldNumber = 7;
		public bool HasOrderingKey()
		{
			return ((_bitField & 0x00000020) == 0x00000020);
		}
		

		// optional uint64 sequence_id = 8;
		public const int SequenceIdFieldNumber = 8;
		public bool HasSequenceId()
		{
			return ((_bitField & 0x00000040) == 0x00000040);
		}
		

		public void InitFields()
		{
			Properties.Clear();
			PartitionKey = "";
			PayloadSize = 0;
			CompactedOut = false;
			EventTime = 0L;
			PartitionKeyB64Encoded = false;
			OrderingKey = Encoding.UTF8.GetBytes(string.Empty);
			SequenceId = 0L;
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

				if (!HasPayloadSize())
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				for (int I = 0; I < PropertiesCount; I++)
				{
					if (GetProperties(I) != null)
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

			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
			}
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
				_properties.Clear();
				_bitField = (_bitField & ~0x00000001);
				_partitionKey = "";
				_bitField = (_bitField & ~0x00000002);
				_payloadSize = 0;
				_bitField = (_bitField & ~0x00000004);
				_compactedOut = false;
				_bitField = (_bitField & ~0x00000008);
				_eventTime = 0L;
				_bitField = (_bitField & ~0x00000010);
				_partitionKeyB64Encoded = false;
				_bitField = (_bitField & ~0x00000020);
				_orderingKey = Encoding.UTF8.GetBytes(string.Empty);
				_bitField = (_bitField & ~0x00000040);
				_sequenceId = 0L;
				_bitField = (_bitField & ~0x00000080);
				return this;
			}


			public SingleMessageMetadata DefaultInstanceForType
			{
				get
				{
					return DefaultInstance;
				}
			}

			public SingleMessageMetadata Build()
			{
				SingleMessageMetadata Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException("SingleMessageMetadata not initialized");
				}
				return Result;
			}

			
			public SingleMessageMetadata BuildPartial()
			{
				var result = SingleMessageMetadata._pool.Take();
				int frombitField = _bitField;
				int tobitField = 0;
				if (((_bitField & 0x00000001) == 0x00000001))
				{
					_properties = new List<KeyValue>(_properties);
					_bitField = (_bitField & ~0x00000001);
				}
				result.Properties.Clear();
				_properties.ToList().ForEach(result.Properties.Add);
				if (((frombitField & 0x00000002) == 0x00000002))
				{
					tobitField |= 0x00000001;
				}
				result.PartitionKey = _partitionKey;
				if (((frombitField & 0x00000004) == 0x00000004))
				{
					tobitField |= 0x00000002;
				}
				result.PayloadSize = _payloadSize;
				if (((frombitField & 0x00000008) == 0x00000008))
				{
					tobitField |= 0x00000004;
				}
				result.CompactedOut = _compactedOut;
				if (((frombitField & 0x00000010) == 0x00000010))
				{
					tobitField |= 0x00000008;
				}
				result.EventTime = (ulong)_eventTime;
				if (((frombitField & 0x00000020) == 0x00000020))
				{
					tobitField |= 0x00000010;
				}
				result.PartitionKeyB64Encoded = _partitionKeyB64Encoded;
				if (((frombitField & 0x00000040) == 0x00000040))
				{
					tobitField |= 0x00000020;
				}
				result.OrderingKey = _orderingKey;
				if (((frombitField & 0x00000080) == 0x00000080))
				{
					tobitField |= 0x00000040;
				}
				result.SequenceId = (ulong)_sequenceId;
				result._bitField = tobitField;
				return result;
			}

			public Builder MergeFrom(SingleMessageMetadata Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.Properties.Count > 0)
				{
					if (_properties.Count == 0)
					{
						_properties = Other.Properties;
						_bitField = (_bitField & ~0x00000001);
					}
					else
					{
						EnsurePropertiesIsMutable();
						((List<KeyValue>)_properties).AddRange(Other.Properties);
					}

				}
				if (Other.HasPartitionKey())
				{
					SetPartitionKey(Other.PartitionKey);
				}
				if (Other.HasPayloadSize())
				{
					SetPayloadSize(Other.PayloadSize);
				}
				if (Other.HasCompactedOut())
				{
					SetCompactedOut(Other.CompactedOut);
				}
				if (Other.HasEventTime())
				{
					SetEventTime((long)Other.EventTime);
				}
				if (Other.HasPartitionKeyB64Encoded())
				{
					SetPartitionKeyB64Encoded(Other.PartitionKeyB64Encoded);
				}
				if (Other.HasOrderingKey())
				{
					SetOrderingKey(Other.OrderingKey);
				}
				if (Other.HasSequenceId())
				{
					SetSequenceId((long)Other.SequenceId);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasPayloadSize())
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

			// repeated .pulsar.proto.KeyValue properties = 1;
			internal IList<KeyValue> _properties = new List<KeyValue>();
			public void EnsurePropertiesIsMutable()
			{
				if (!((_bitField & 0x00000001) == 0x00000001))
				{
					_properties = new List<KeyValue>(_properties);
					_bitField |= 0x00000001;
				}
			}

			public IList<KeyValue> PropertiesList
			{
				get
				{
					return _properties;
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
			
			
			public Builder AddAllProperties<T1>(IEnumerable<T1> Values) where T1 : KeyValue
			{
				EnsurePropertiesIsMutable();
				Values.ToList().ForEach(_properties.Add);

				return this;
			}
			public Builder ClearProperties()
			{
				_properties.Clear();
				_bitField = (_bitField & ~0x00000001);

				return this;
			}
			public Builder RemoveProperties(int Index)
			{
				EnsurePropertiesIsMutable();
				_properties.RemoveAt(Index);

				return this;
			}

			// optional string partition_key = 2;
			private string _partitionKey = "";
			public bool HasPartitionKey()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			public string GetPartitionKey()
			{
				return _partitionKey;
			}
			public Builder SetPartitionKey(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new System.NullReferenceException();
				}
				_bitField |= 0x00000002;
				_partitionKey = Value;

				return this;
			}
			public Builder ClearPartitionKey()
			{
				_bitField = (_bitField & ~0x00000002);
				_partitionKey = DefaultInstance.PartitionKey;

				return this;
			}
			
			// required int32 payload_size = 3;
			internal int _payloadSize;
			public bool HasPayloadSize()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			public int PayloadSize
			{
				get
				{
					return _payloadSize;
				}
			}
			public Builder SetPayloadSize(int Value)
			{
				_bitField |= 0x00000004;
				_payloadSize = Value;

				return this;
			}
			public Builder ClearPayloadSize()
			{
				_bitField = (_bitField & ~0x00000004);
				_payloadSize = 0;

				return this;
			}

			// optional bool compacted_out = 4 [default = false];
			internal bool _compactedOut;
			public bool HasCompactedOut()
			{
				return ((_bitField & 0x00000008) == 0x00000008);
			}
			public bool CompactedOut
			{
				get
				{
					return _compactedOut;
				}
			}
			public Builder SetCompactedOut(bool Value)
			{
				_bitField |= 0x00000008;
				_compactedOut = Value;

				return this;
			}
			public Builder ClearCompactedOut()
			{
				_bitField = (_bitField & ~0x00000008);
				_compactedOut = false;

				return this;
			}

			// optional uint64 event_time = 5 [default = 0];
			internal long _eventTime;
			public bool HasEventTime()
			{
				return ((_bitField & 0x00000010) == 0x00000010);
			}
			public long EventTime
			{
				get
				{
					return _eventTime;
				}
			}
			public Builder SetEventTime(long Value)
			{
				_bitField |= 0x00000010;
				_eventTime = Value;

				return this;
			}
			public Builder ClearEventTime()
			{
				_bitField = (_bitField & ~0x00000010);
				_eventTime = 0L;

				return this;
			}

			// optional bool partition_key_b64_encoded = 6 [default = false];
			internal bool _partitionKeyB64Encoded;
			public bool HasPartitionKeyB64Encoded()
			{
				return ((_bitField & 0x00000020) == 0x00000020);
			}
			public bool PartitionKeyB64Encoded
			{
				get
				{
					return _partitionKeyB64Encoded;
				}
			}
			public Builder SetPartitionKeyB64Encoded(bool Value)
			{
				_bitField |= 0x00000020;
				_partitionKeyB64Encoded = Value;

				return this;
			}
			public Builder ClearPartitionKeyB64Encoded()
			{
				_bitField = (_bitField & ~0x00000020);
				_partitionKeyB64Encoded = false;

				return this;
			}

			// optional bytes ordering_key = 7;
			internal byte[] _orderingKey = Encoding.UTF8.GetBytes(string.Empty);
			public bool HasOrderingKey()
			{
				return ((_bitField & 0x00000040) == 0x00000040);
			}
			
			public Builder SetOrderingKey(byte[] Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_bitField |= 0x00000040;
				_orderingKey = Value;

				return this;
			}
			public Builder ClearOrderingKey()
			{
				_bitField = (_bitField & ~0x00000040);
				_orderingKey = DefaultInstance.OrderingKey;

				return this;
			}

			// optional uint64 sequence_id = 8;
			internal long _sequenceId;
			public bool HasSequenceId()
			{
				return ((_bitField & 0x00000080) == 0x00000080);
			}
			public long SequenceId
			{
				get
				{
					return _sequenceId;
				}
			}
			public Builder SetSequenceId(long Value)
			{
				_bitField |= 0x00000080;
				_sequenceId = Value;

				return this;
			}
			public Builder ClearSequenceId()
			{
				_bitField = (_bitField & ~0x00000080);
				_sequenceId = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.SingleMessageMetadata)
		}

		static SingleMessageMetadata()
		{
			_defaultInstance = new SingleMessageMetadata(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.SingleMessageMetadata)
	}


}
