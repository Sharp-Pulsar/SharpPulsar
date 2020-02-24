using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedOutputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class MessageMetadata : ByteBufGeneratedMessage
	{
		internal static ThreadLocalPool<MessageMetadata> _pool = new ThreadLocalPool<MessageMetadata>(handle => new MessageMetadata(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private MessageMetadata(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		
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

		public MessageMetadata(bool NoInit)
		{
		}

		
		internal static readonly MessageMetadata _defaultInstance;
		public static MessageMetadata DefaultInstance => _defaultInstance;

        public MessageMetadata DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			ProducerName = "";
			SequenceId = 0L;
			PublishTime = 0L;
			ReplicatedFrom = "";
			PartitionKey = "";
			Compression = CompressionType.None;
			UncompressedSize = 0;
			NumMessagesInBatch = 1;
			EventTime = 0L;
			EncryptionAlgo = "";
			EncryptionParam= ByteString.Empty;
			SchemaVersion = ByteString.Empty;
			PartitionKeyB64Encoded = false;
			OrderingKey = ByteString.Empty;
			DeliverAtTime = 0L;
			MarkerType = 0;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			HighestSequenceId = 0L;
		}
		internal sbyte MemoizedIsInitialized = -1;
		public int SerializedSize => CalculateSize();
		public bool Initialized
		{
			get
			{
				sbyte IsInitialized = MemoizedIsInitialized;
				if (IsInitialized != -1)
				{
					return IsInitialized == 1;
				}

				if (!HasProducerName)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSequenceId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasPublishTime)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				for (int I = 0; I < Properties.Count; I++)
				{
					if (Properties[I] != null)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				for (int I = 0; I < EncryptionKeys.Count; I++)
				{
					if (EncryptionKeys[I] != null)
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
			var _= SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(ProducerName));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)SequenceId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt64(3, (long)PublishTime);
			}
			for (int I = 0; I < Properties.Count; I++)
			{
				Output.WriteMessage(4, Properties[I]);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(5, ByteString.CopyFromUtf8(ReplicatedFrom));
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBytes(6, ByteString.CopyFromUtf8(PartitionKey));
			}
			for (int I = 0; I < ReplicateTo.Count; I++)
			{
				Output.WriteBytes(7, ByteString.CopyFromUtf8(ReplicateTo[I]));
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteEnum(8, (int)Compression);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				Output.WriteUInt32(9, (int)UncompressedSize);
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				Output.WriteInt32(11, NumMessagesInBatch);
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				Output.WriteUInt64(12, (long)EventTime);
			}
			for (int I = 0; I < EncryptionKeys.Count; I++)
			{
				Output.WriteMessage(13, EncryptionKeys[I]);
			}
			if (((_hasBits0 & 0x00000200) == 0x00000200))
			{
				Output.WriteBytes(14, ByteString.CopyFromUtf8(EncryptionAlgo));
			}
			if (((_hasBits0 & 0x00000400) == 0x00000400))
			{
				Output.WriteBytes(15, EncryptionParam);
			}
			if (((_hasBits0 & 0x00000800) == 0x00000800))
			{
				Output.WriteBytes(16, SchemaVersion);
			}
			if (((_hasBits0 & 0x00001000) == 0x00001000))
			{
				Output.WriteBool(17, PartitionKeyB64Encoded);
			}
			if (((_hasBits0 & 0x00002000) == 0x00002000))
			{
				Output.WriteBytes(18, OrderingKey);
			}
			if (((_hasBits0 & 0x00004000) == 0x00004000))
			{
				Output.WriteInt64(19, DeliverAtTime);
			}
			if (((_hasBits0 & 0x00008000) == 0x00008000))
			{
				Output.WriteInt32(20, MarkerType);
			}
			if (((_hasBits0 & 0x00010000) == 0x00010000))
			{
				Output.WriteUInt64(22, (long)TxnidLeastBits);
			}
			if (((_hasBits0 & 0x00020000) == 0x00020000))
			{
				Output.WriteUInt64(23, (long)TxnidMostBits);
			}
			if (((_hasBits0 & 0x00040000) == 0x00040000))
			{
				Output.WriteUInt64(24, (long)HighestSequenceId);
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
		public static Builder NewBuilder(MessageMetadata prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}
		public sealed class Builder : ByteBufMessageBuilder
		{

			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

			internal readonly ThreadLocalPool.Handle _handle;
			public Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
				MaybeForceBuilderInitialization();
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
				_producerName = "";
				_bitField = (_bitField & ~0x00000001);
				_sequenceId = 0L;
				_bitField = (_bitField & ~0x00000002);
				_publishTime = 0L;
				_bitField = (_bitField & ~0x00000004);
				_properties = new List<KeyValue>();
				_bitField = (_bitField & ~0x00000008);
				_replicatedFrom = "";
				_bitField = (_bitField & ~0x00000010);
				_partitionKey = "";
				_bitField = (_bitField & ~0x00000020);
				_replicateToes = new List<string>();
				_bitField = (_bitField & ~0x00000040);
				_compression = CompressionType.None;
				_bitField = (_bitField & ~0x00000080);
				_uncompressedSize = 0;
				_bitField = (_bitField & ~0x00000100);
				_numMessagesInBatch = 1;
				_bitField = (_bitField & ~0x00000200);
				_eventTime = 0L;
				_bitField = (_bitField & ~0x00000400);
				_encryptionKeys = new List<EncryptionKeys>();
				_bitField = (_bitField & ~0x00000800);
				_encryptionAlgo = "";
				_bitField = (_bitField & ~0x00001000);
				_encryptionParam = ByteString.Empty;
				_bitField = (_bitField & ~0x00002000);
				_schemaVersion = ByteString.Empty;
				_bitField = (_bitField & ~0x00004000);
				_partitionKeyB64Encoded = false;
				_bitField = (_bitField & ~0x00008000);
				_orderingKey = ByteString.Empty;
				_bitField = (_bitField & ~0x00010000);
				_deliverAtTime = 0L;
				_bitField = (_bitField & ~0x00020000);
				_markerType = 0;
				_bitField = (_bitField & ~0x00040000);
				_txnidLeastBits = 0L;
				_bitField = (_bitField & ~0x00080000);
				_txnidMostBits = 0L;
				_bitField = (_bitField & ~0x00100000);
				HighestSequenceId = 0L;
				_bitField = (_bitField & ~0x00200000);
				return this;
			}

			public MessageMetadata DefaultInstanceForType => MessageMetadata.DefaultInstance;

            public MessageMetadata Build()
			{
				var result = BuildPartial();
				
				return result;
			}


			private MessageMetadata BuildPartial()
			{
				var result = MessageMetadata._pool.Take();
				int _frombitField = _bitField;
				int To_bitField = 0;
				if (((_frombitField & 0x00000001) == 0x00000001))
				{
					To_bitField |= 0x00000001;
				}
				result.ProducerName = _producerName;
				if (((_frombitField & 0x00000002) == 0x00000002))
				{
					To_bitField |= 0x00000002;
				}
				result.SequenceId = (ulong)_sequenceId;
				if (((_frombitField & 0x00000004) == 0x00000004))
				{
					To_bitField |= 0x00000004;
				}
				result.PublishTime = (ulong)_publishTime;
				if (((_bitField & 0x00000008) == 0x00000008))
				{
					_properties = new List<KeyValue>(_properties);
					_bitField = (_bitField & ~0x00000008);
				}
				result.Properties.Clear();
				_properties.ToList().ForEach(result.Properties.Add);
				if (((_frombitField & 0x00000010) == 0x00000010))
				{
					To_bitField |= 0x00000008;
				}
				result.ReplicatedFrom = _replicatedFrom;
				if (((_frombitField & 0x00000020) == 0x00000020))
				{
					To_bitField |= 0x00000010;
				}
				result.PartitionKey = _partitionKey;
				if (((_bitField & 0x00000040) == 0x00000040))
				{
					_replicateToes = new List<string>(_replicateToes);
					_bitField = (_bitField & ~0x00000040);
				}
				result.ReplicateTo.Clear();
				_replicateToes.ToList().ForEach(result.ReplicateTo.Add);
				if (((_frombitField & 0x00000080) == 0x00000080))
				{
					To_bitField |= 0x00000020;
				}
				result.Compression = _compression;
				if (((_frombitField & 0x00000100) == 0x00000100))
				{
					To_bitField |= 0x00000040;
				}
				result.UncompressedSize = (uint)_uncompressedSize;
				if (((_frombitField & 0x00000200) == 0x00000200))
				{
					To_bitField |= 0x00000080;
				}
				result.NumMessagesInBatch = _numMessagesInBatch;
				if (((_frombitField & 0x00000400) == 0x00000400))
				{
					To_bitField |= 0x00000100;
				}
				result.EventTime = (ulong)_eventTime;
				if (((_bitField & 0x00000800) == 0x00000800))
				{
					_encryptionKeys = new List<EncryptionKeys>(_encryptionKeys);
					_bitField = (_bitField & ~0x00000800);
				}
				result.EncryptionKeys.Clear();
				_encryptionKeys.ToList().ForEach(result.EncryptionKeys.Add);
				if (((_frombitField & 0x00001000) == 0x00001000))
				{
					To_bitField |= 0x00000200;
				}
				result.EncryptionAlgo = _encryptionAlgo;
				if (((_frombitField & 0x00002000) == 0x00002000))
				{
					To_bitField |= 0x00000400;
				}
				result.EncryptionParam = _encryptionParam;
				if (((_frombitField & 0x00004000) == 0x00004000))
				{
					To_bitField |= 0x00000800;
				}
				result.SchemaVersion = _schemaVersion;
				if (((_frombitField & 0x00008000) == 0x00008000))
				{
					To_bitField |= 0x00001000;
				}
				result.PartitionKeyB64Encoded = _partitionKeyB64Encoded;
				if (((_frombitField & 0x00010000) == 0x00010000))
				{
					To_bitField |= 0x00002000;
				}
				result.OrderingKey = _orderingKey;
				if (((_frombitField & 0x00020000) == 0x00020000))
				{
					To_bitField |= 0x00004000;
				}
				result.DeliverAtTime = _deliverAtTime;
				if (((_frombitField & 0x00040000) == 0x00040000))
				{
					To_bitField |= 0x00008000;
				}
				result.MarkerType = _markerType;
				if (((_frombitField & 0x00080000) == 0x00080000))
				{
					To_bitField |= 0x00010000;
				}
				result.TxnidLeastBits = (ulong)_txnidLeastBits;
				if (((_frombitField & 0x00100000) == 0x00100000))
				{
					To_bitField |= 0x00020000;
				}
				result.TxnidMostBits = (ulong)_txnidMostBits;
				if (((_frombitField & 0x00200000) == 0x00200000))
				{
					To_bitField |= 0x00040000;
				}
				result.HighestSequenceId = (ulong)HighestSequenceId;
				result._hasBits0 = To_bitField;
				return result;
			}

			public Builder MergeFrom(MessageMetadata Other)
			{
				if (Other == MessageMetadata.DefaultInstance)
				{
					return this;
				}
				if (Other.HasProducerName)
				{
					SetProducerName(Other.ProducerName);
				}
				if (Other.HasSequenceId)
				{
					_sequenceId = (long)Other.SequenceId;
				}
				if (Other.HasPublishTime)
				{
					SetPublishTime((long)Other.PublishTime);
				}
				if (Other.Properties.Count > 0)
				{
					if (_properties.Count == 0)
					{
						_properties = Other.Properties;
						_bitField = (_bitField & ~0x00000008);
					}
					else
					{
						EnsurePropertiesIsMutable();
						((List<KeyValue>)_properties).AddRange(Other.Properties);
					}

				}
				if (Other.HasReplicatedFrom)
				{
					SetReplicatedFrom(Other.ReplicatedFrom);
				}
				if (Other.HasPartitionKey)
				{
					SetPartitionKey(Other.PartitionKey);
				}
				if (!Other.ReplicateTo.Any())
				{
					if (!_replicateToes.Any())
					{
						_replicateToes = Other.ReplicateTo;
						_bitField = (_bitField & ~0x00000040);
					}
					else
					{
						EnsureReplicateToIsMutable();
						Other.ReplicateTo.ToList().ForEach(_replicateToes.Add);
					}

				}
				if (Other.HasCompression)
				{
					SetCompression(Other.Compression);
				}
				if (Other.HasUncompressedSize)
				{
					SetUncompressedSize((int)Other.UncompressedSize);
				}
				if (Other.HasNumMessagesInBatch)
				{
					SetNumMessagesInBatch(Other.NumMessagesInBatch);
				}
				if (Other.HasEventTime)
				{
					SetEventTime((long)Other.EventTime);
				}
				if (Other.EncryptionKeys.Count > 0)
				{
					if (_encryptionKeys.Count == 0)
					{
						_encryptionKeys = Other.EncryptionKeys;
						_bitField = (_bitField & ~0x00000800);
					}
					else
					{
						EnsureEncryptionKeysIsMutable();
						((List<EncryptionKeys>)_encryptionKeys).AddRange(Other.EncryptionKeys);
					}

				}
				if (Other.HasEncryptionAlgo)
				{
					SetEncryptionAlgo(Other.EncryptionAlgo);
				}
				if (Other.HasEncryptionParam)
				{
					SetEncryptionParam(Other.EncryptionParam);
				}
				if (Other.HasSchemaVersion)
				{
					SetSchemaVersion(Other.SchemaVersion);
				}
				if (Other.HasPartitionKeyB64Encoded)
				{
					SetPartitionKeyB64Encoded(Other.PartitionKeyB64Encoded);
				}
				if (Other.HasOrderingKey)
				{
					SetOrderingKey(Other.OrderingKey);
				}
				if (Other.HasDeliverAtTime)
				{
					_deliverAtTime = Other.DeliverAtTime;
				}
				if (Other.HasMarkerType)
				{
					_markerType = Other.MarkerType;
				}
				if (Other.HasTxnidLeastBits)
				{
					_txnidLeastBits = (long)Other.TxnidLeastBits;
				}
				if (Other.HasTxnidMostBits)
				{
					_txnidMostBits = (long)Other.TxnidMostBits;
				}
				if (Other.HasHighestSequenceId)
				{
					HighestSequenceId = (long)Other.HighestSequenceId;
				}
				return this;
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistry ExtensionRegistry)
			{
				while (true)
				{
					int Tag = Input.ReadTag();
					switch (Tag)
					{
						case 0:

							return this;
						default:
							{
								if (!Input.SkipField(Tag))
								{

									return this;
								}
								break;
							}
						case 10:
							{
								_bitField |= 0x00000001;
								_producerName = Input.ReadBytes().ToStringUtf8();
								break;
							}
						case 16:
							{
								_bitField |= 0x00000002;
								_sequenceId = Input.ReadUInt64();
								break;
							}
						case 24:
							{
								_bitField |= 0x00000004;
								_publishTime = Input.ReadUInt64();
								break;
							}
						case 34:
							{
								KeyValue.Builder SubBuilder = KeyValue.NewBuilder();
								Input.ReadMessage(SubBuilder, ExtensionRegistry);
								AddProperties(SubBuilder.BuildPartial());
								break;
							}
						case 42:
							{
								_bitField |= 0x00000010;
								_replicatedFrom = Input.ReadBytes().ToStringUtf8();
								break;
							}
						case 50:
							{
								_bitField |= 0x00000020;
								_partitionKey = Input.ReadBytes().ToStringUtf8();
								break;
							}
						case 58:
							{
								EnsureReplicateToIsMutable();
								_replicateToes.Add(Input.ReadBytes().ToStringUtf8());
								break;
							}
						case 64:
							{
								int RawValue = Input.ReadEnum();
								CompressionType Value = Enum.GetValues(typeof(CompressionType)).Cast<CompressionType>().ToList()[RawValue];
								if (Value != null)
								{
									_bitField |= 0x00000080;
									_compression = Value;
								}
								break;
							}
						case 72:
							{
								_bitField |= 0x00000100;
								_uncompressedSize = Input.ReadUInt32();
								break;
							}
						case 88:
							{
								_bitField |= 0x00000200;
								_numMessagesInBatch = Input.ReadInt32();
								break;
							}
						case 96:
							{
								_bitField |= 0x00000400;
								_eventTime = Input.ReadUInt64();
								break;
							}
						case 106:
							{
								EncryptionKeys.Builder SubBuilder = Proto.EncryptionKeys.NewBuilder();
								Input.ReadMessage(SubBuilder, ExtensionRegistry);
								AddEncryptionKeys(SubBuilder.BuildPartial());
								break;
							}
						case 114:
							{
								_bitField |= 0x00001000;
								_encryptionAlgo = Input.ReadBytes().ToStringUtf8();
								break;
							}
						case 122:
							{
								_bitField |= 0x00002000;
								_encryptionParam = Input.ReadBytes();
								break;
							}
						case 130:
							{
								_bitField |= 0x00004000;
								_schemaVersion = Input.ReadBytes();
								break;
							}
						case 136:
							{
								_bitField |= 0x00008000;
								_partitionKeyB64Encoded = Input.ReadBool();
								break;
							}
						case 146:
							{
								_bitField |= 0x00010000;
								_orderingKey = Input.ReadBytes();
								break;
							}
						case 152:
							{
								_bitField |= 0x00020000;
								_deliverAtTime = Input.ReadInt64();
								break;
							}
						case 160:
							{
								_bitField |= 0x00040000;
								_markerType = Input.ReadInt32();
								break;
							}
						case 176:
							{
								_bitField |= 0x00080000;
								_txnidLeastBits = Input.ReadUInt64();
								break;
							}
						case 184:
							{
								_bitField |= 0x00100000;
								_txnidMostBits = Input.ReadUInt64();
								break;
							}
						case 192:
							{
								_bitField |= 0x00200000;
								HighestSequenceId = Input.ReadUInt64();
								break;
							}
					}
				}
			}
			public bool Initialized
			{
				get
				{
					if (!HasProducerName())
					{

						return false;
					}
					if (!HasSequenceId())
					{

						return false;
					}
					if (!HasPublishTime())
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
					for (int I = 0; I < EncryptionKeysCount; I++)
					{
						if (GetEncryptionKeys(I) != null)
						{

							return false;
						}
					}
					return true;
				}
			}


			internal int _bitField;

			// required string producer_name = 1;
			private string _producerName = "";
			public bool HasProducerName()
			{
				return ((_bitField & 0x00000001) == 0x00000001);
			}
			public string GetProducerName()
			{
				return _producerName;

			}
			public Builder SetProducerName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000001;
				_producerName = Value;

				return this;
			}
			public Builder ClearProducerName()
			{
				_bitField = (_bitField & ~0x00000001);
				_producerName = DefaultInstance.ProducerName;

				return this;
			}
			
			// required uint64 sequence_id = 2;
			internal long _sequenceId;
			public bool HasSequenceId()
			{
				return ((_bitField & 0x00000002) == 0x00000002);
			}
			
			public Builder SetSequenceId(long Value)
			{
				_bitField |= 0x00000002;
				_sequenceId = Value;

				return this;
			}
			public Builder ClearSequenceId()
			{
				_bitField = (_bitField & ~0x00000002);
				_sequenceId = 0L;

				return this;
			}

			// required uint64 publish_time = 3;
			internal long _publishTime;
			public bool HasPublishTime()
			{
				return ((_bitField & 0x00000004) == 0x00000004);
			}
			
			public Builder SetPublishTime(long Value)
			{
				_bitField |= 0x00000004;
				_publishTime = Value;

				return this;
			}
			public Builder ClearPublishTime()
			{
				_bitField = (_bitField & ~0x00000004);
				_publishTime = 0L;

				return this;
			}

			// repeated .pulsar.proto.KeyValue properties = 4;
			internal IList<KeyValue> _properties = new List<KeyValue>();
			public void EnsurePropertiesIsMutable()
			{
				if (!((_bitField & 0x00000008) == 0x00000008))
				{
					_properties = new List<KeyValue>(_properties);
					_bitField |= 0x00000008;
				}
			}

			public IList<KeyValue> PropertiesList => _properties;

            public int PropertiesCount => _properties.Count;

            public KeyValue GetProperties(int Index)
			{
				return _properties[Index];
			}
			public Builder SetProperties(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				_properties[Index] = Value;

				return this;
			}

			public Builder AddProperties(KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				_properties.Add(Value);

				return this;
			}
			public Builder AddProperties(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				_properties.Insert(Index, Value);

				return this;
			}
			public Builder AddAllProperties(IEnumerable<KeyValue> Values) 
			{
				EnsurePropertiesIsMutable();
				Values.ToList().ForEach(_properties.Add);

				return this;
			}
			public Builder ClearProperties()
			{
				_properties.Clear();
				_bitField = (_bitField & ~0x00000008);

				return this;
			}
			public Builder RemoveProperties(int Index)
			{
				EnsurePropertiesIsMutable();
				_properties.RemoveAt(Index);

				return this;
			}

			// optional string replicated_from = 5;
			private string _replicatedFrom = "";
			public bool HasReplicatedFrom()
			{
				return ((_bitField & 0x00000010) == 0x00000010);
			}
			public string GetReplicatedFrom()
			{
				return _replicatedFrom;
			}
			public Builder SetReplicatedFrom(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000010;
				_replicatedFrom = Value;

				return this;
			}
			public Builder ClearReplicatedFrom()
			{
				_bitField = (_bitField & ~0x00000010);
				_replicatedFrom = DefaultInstance.ReplicatedFrom;

				return this;
			}
			
			// optional string partition_key = 6;
			private string _partitionKey = "";
			public bool HasPartitionKey()
			{
				return ((_bitField & 0x00000020) == 0x00000020);
			}
			public string GetPartitionKey()
			{
				return _partitionKey;
			}
			public long GetPublishTime()
			{
				return _publishTime;
			}
			public long SequenceId()
			{
				return _sequenceId;
			}
			public long GetSequenceId()
			{
				return _sequenceId;
			}
			public ByteString GetOrderingKey()
			{
				return _orderingKey;
			}
			public ByteString GetSchemaVersion()
			{
				return _schemaVersion;
			}
			public Builder SetPartitionKey(string Value)
			{
				if (ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000020;
				_partitionKey = Value;

				return this;
			}
			public Builder ClearPartitionKey()
			{
				_bitField = (_bitField & ~0x00000020);
				_partitionKey = DefaultInstance.PartitionKey;

				return this;
			}
			
			// repeated string replicate_to = 7;
			internal IList<string> _replicateToes = new List<string>();
			public void EnsureReplicateToIsMutable()
			{
				if (!((_bitField & 0x00000040) == 0x00000040))
				{
					_replicateToes = new List<string>(_replicateToes);
					_bitField |= 0x00000040;
				}
			}
			public IList<string> ReplicateToList => _replicateToes;

            public int ReplicateToCount => _replicateToes.Count;

            public string GetReplicateTo(int Index)
			{
				return _replicateToes[Index];
			}
			public Builder SetReplicateTo(int Index, string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsureReplicateToIsMutable();
				_replicateToes.Insert(Index, Value);

				return this;
			}
			public Builder AddReplicateTo(string Value)
			{
				if (ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsureReplicateToIsMutable();
				_replicateToes.Add(Value);

				return this;
			}
			public Builder AddAllReplicateTo(IEnumerable<string> values)
			{
				EnsureReplicateToIsMutable();
				values.ToList().ForEach(_replicateToes.Add);

				return this;
			}
			public Builder ClearReplicateTo()
			{
				_replicateToes.Clear();
				_bitField = (_bitField & ~0x00000040);

				return this;
			}
			
			// optional .pulsar.proto.CompressionType compression = 8 [default = NONE];
			private CompressionType _compression = CompressionType.None;
			public bool HasCompression()
			{
				return ((_bitField & 0x00000080) == 0x00000080);
			}
			
			public Builder SetCompression(Common.Enum.CompressionType Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000080;
				_compression = GetCompressionType(Value);

				return this;
			}
			public Builder SetCompression(CompressionType Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00000080;
				_compression = Value;

				return this;
			}
			private CompressionType GetCompressionType(Common.Enum.CompressionType value)
			{
				switch(value)
				{
					case Common.Enum.CompressionType.LZ4:
						return CompressionType.Lz4;
					case Common.Enum.CompressionType.SNAPPY:
						return CompressionType.Snappy;
					case Common.Enum.CompressionType.ZLIB:
						return CompressionType.Zlib;
					case Common.Enum.CompressionType.ZSTD:
						return CompressionType.Zstd;
					default:
						return CompressionType.None;
				}
			}
			public Builder ClearCompression()
			{
				_bitField = (_bitField & ~0x00000080);
				_compression = CompressionType.None;

				return this;
			}

			// optional uint32 uncompressed_size = 9 [default = 0];
			internal int _uncompressedSize;
			public bool HasUncompressedSize()
			{
				return ((_bitField & 0x00000100) == 0x00000100);
			}
			public int UncompressedSize => _uncompressedSize;

            public Builder SetUncompressedSize(int Value)
			{
				_bitField |= 0x00000100;
				_uncompressedSize = Value;

				return this;
			}
			public Builder ClearUncompressedSize()
			{
				_bitField = (_bitField & ~0x00000100);
				_uncompressedSize = 0;

				return this;
			}

			// optional int32 num_messages_in_batch = 11 [default = 1];
			internal int _numMessagesInBatch = 1;
			public bool HasNumMessagesInBatch()
			{
				return ((_bitField & 0x00000200) == 0x00000200);
			}
			public int NumMessagesInBatch => _numMessagesInBatch;

            public Builder SetNumMessagesInBatch(int Value)
			{
				_bitField |= 0x00000200;
				_numMessagesInBatch = Value;

				return this;
			}
			public Builder ClearNumMessagesInBatch()
			{
				_bitField = (_bitField & ~0x00000200);
				_numMessagesInBatch = 1;

				return this;
			}

			// optional uint64 event_time = 12 [default = 0];
			internal long _eventTime;
			public bool HasEventTime()
			{
				return ((_bitField & 0x00000400) == 0x00000400);
			}
			public long EventTime => _eventTime;

            public Builder SetEventTime(long Value)
			{
				_bitField |= 0x00000400;
				_eventTime = Value;

				return this;
			}
			public Builder ClearEventTime()
			{
				_bitField = (_bitField & ~0x00000400);
				_eventTime = 0L;

				return this;
			}

			// repeated .pulsar.proto.EncryptionKeys encryption_keys = 13;
			internal IList<EncryptionKeys> _encryptionKeys = new List<EncryptionKeys>();
			public void EnsureEncryptionKeysIsMutable()
			{
				if (!((_bitField & 0x00000800) == 0x00000800))
				{
					_encryptionKeys = new List<EncryptionKeys>(_encryptionKeys);
					_bitField |= 0x00000800;
				}
			}

			public IList<EncryptionKeys> EncryptionKeysList => _encryptionKeys;

            public int EncryptionKeysCount => _encryptionKeys.Count;

            public EncryptionKeys GetEncryptionKeys(int Index)
			{
				return _encryptionKeys[Index];
			}
			public Builder SetEncryptionKeys(int Index, EncryptionKeys Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureEncryptionKeysIsMutable();
				_encryptionKeys[Index] = Value;

				return this;
			}
			public Builder AddEncryptionKeys(EncryptionKeys Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureEncryptionKeysIsMutable();
				_encryptionKeys.Add(Value);

				return this;
			}
			public Builder AddEncryptionKeys(int Index, EncryptionKeys Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureEncryptionKeysIsMutable();
				_encryptionKeys.Insert(Index, Value);

				return this;
			}
						
			public Builder AddAllEncryptionKeys(IEnumerable<EncryptionKeys> Values) 
			{
				EnsureEncryptionKeysIsMutable();
				Values.ToList().ForEach(_encryptionKeys.Add);
				return this;
			}
			public Builder ClearEncryptionKeys()
			{
				_encryptionKeys.Clear();
				_bitField = (_bitField & ~0x00000800);

				return this;
			}
			public Builder RemoveEncryptionKeys(int Index)
			{
				EnsureEncryptionKeysIsMutable();
				_encryptionKeys.RemoveAt(Index);

				return this;
			}

			// optional string encryption_algo = 14;
			private string _encryptionAlgo = "";
			public bool HasEncryptionAlgo()
			{
				return ((_bitField & 0x00001000) == 0x00001000);
			}
			public string GetEncryptionAlgo()
			{
				return _encryptionAlgo;
			}
			public Builder SetEncryptionAlgo(string Value)
			{
				if (Value is null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00001000;
				_encryptionAlgo = Value;

				return this;
			}
			public Builder ClearEncryptionAlgo()
			{
				_bitField = (_bitField & ~0x00001000);
				_encryptionAlgo = DefaultInstance.EncryptionAlgo;

				return this;
			}
			
			// optional bytes encryption_param = 15;
			private ByteString _encryptionParam = ByteString.Empty;
			public bool HasEncryptionParam()
			{
				return ((_bitField & 0x00002000) == 0x00002000);
			}
			
			public Builder SetEncryptionParam(ByteString value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00002000;
				_encryptionParam = value;

				return this;
			}
			public Builder ClearEncryptionParam()
			{
				_bitField = (_bitField & ~0x00002000);
				_encryptionParam = DefaultInstance.EncryptionParam;

				return this;
			}

			// optional bytes schema_version = 16;
			internal ByteString _schemaVersion = ByteString.Empty;
			public bool HasSchemaVersion()
			{
				return ((_bitField & 0x00004000) == 0x00004000);
			}
			
			public Builder SetSchemaVersion(ByteString value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00004000;
				_schemaVersion = value;

				return this;
			}
			public Builder ClearSchemaVersion()
			{
				_bitField = (_bitField & ~0x00004000);
				_schemaVersion = DefaultInstance.SchemaVersion;

				return this;
			}

			// optional bool partition_key_b64_encoded = 17 [default = false];
			internal bool _partitionKeyB64Encoded;
			public bool HasPartitionKeyB64Encoded()
			{
				return ((_bitField & 0x00008000) == 0x00008000);
			}
			public bool PartitionKeyB64Encoded => _partitionKeyB64Encoded;

            public Builder SetPartitionKeyB64Encoded(bool Value)
			{
				_bitField |= 0x00008000;
				_partitionKeyB64Encoded = Value;

				return this;
			}
			public Builder ClearPartitionKeyB64Encoded()
			{
				_bitField = (_bitField & ~0x00008000);
				_partitionKeyB64Encoded = false;

				return this;
			}

			// optional bytes ordering_key = 18;
			private ByteString _orderingKey = ByteString.Empty;
			public bool HasOrderingKey()
			{
				return ((_bitField & 0x00010000) == 0x00010000);
			}
			
			public Builder SetOrderingKey(ByteString value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_bitField |= 0x00010000;
				_orderingKey = value;

				return this;
			}
			public Builder ClearOrderingKey()
			{
				_bitField = (_bitField & ~0x00010000);
				_orderingKey = DefaultInstance.OrderingKey;

				return this;
			}

			// optional int64 deliver_at_time = 19;
			internal long _deliverAtTime;
			public bool HasDeliverAtTime()
			{
				return ((_bitField & 0x00020000) == 0x00020000);
			}
			
			public Builder SetDeliverAtTime(long Value)
			{
				_bitField |= 0x00020000;
				_deliverAtTime = Value;

				return this;
			}
			public Builder ClearDeliverAtTime()
			{
				_bitField = (_bitField & ~0x00020000);
				_deliverAtTime = 0L;

				return this;
			}

			// optional int32 marker_type = 20;
			internal int _markerType;
			public bool HasMarkerType()
			{
				return ((_bitField & 0x00040000) == 0x00040000);
			}
			
			public Builder SetMarkerType(int Value)
			{
				_bitField |= 0x00040000;
				_markerType = Value;

				return this;
			}
			public Builder ClearMarkerType()
			{
				_bitField = (_bitField & ~0x00040000);
				_markerType = 0;

				return this;
			}

			// optional uint64 txnid_least_bits = 22 [default = 0];
			internal long _txnidLeastBits;
			public bool HasTxnidLeastBits()
			{
				return ((_bitField & 0x00080000) == 0x00080000);
			}
			
			public Builder SetTxnidLeastBits(long Value)
			{
				_bitField |= 0x00080000;
				_txnidLeastBits = Value;

				return this;
			}
			public Builder ClearTxnidLeastBits()
			{
				_bitField = (_bitField & ~0x00080000);
				_txnidLeastBits = 0L;

				return this;
			}

			// optional uint64 txnid_most_bits = 23 [default = 0];
			internal long _txnidMostBits;
			public bool HasTxnidMostBits()
			{
				return ((_bitField & 0x00100000) == 0x00100000);
			}
			
			public Builder SetTxnidMostBits(long Value)
			{
				_bitField |= 0x00100000;
				_txnidMostBits = Value;

				return this;
			}
			public Builder ClearTxnidMostBits()
			{
				_bitField = (_bitField & ~0x00100000);
				_txnidMostBits = 0L;

				return this;
			}

			// optional uint64 highest_sequence_id = 24 [default = 0];
			public long HighestSequenceId;
			public bool HasHighestSequenceId()
			{
				return ((_bitField & 0x00200000) == 0x00200000);
			}
			
			public Builder SetHighestSequenceId(long Value)
			{
				_bitField |= 0x00200000;
				HighestSequenceId = Value;

				return this;
			}
			public Builder ClearHighestSequenceId()
			{
				_bitField = (_bitField & ~0x00200000);
				HighestSequenceId = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.MessageMetadata)
		}

		static MessageMetadata()
		{
			_defaultInstance = new MessageMetadata(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.MessageMetadata)
	}

}
