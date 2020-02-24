using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAddPartitionToTxn : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandAddPartitionToTxn.newBuilder() to construct.
		internal static ThreadLocalPool<CommandAddPartitionToTxn> _pool = new ThreadLocalPool<CommandAddPartitionToTxn>(handle => new CommandAddPartitionToTxn(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandAddPartitionToTxn(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			_hasBits0 = 0;
			MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

		public CommandAddPartitionToTxn(bool NoInit)
		{
		}

		internal static readonly CommandAddPartitionToTxn _defaultInstance;
		public static CommandAddPartitionToTxn DefaultInstance => _defaultInstance;

        public CommandAddPartitionToTxn DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			RequestId = 0L;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			Partitions.Clear();
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

				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
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
				Output.WriteUInt64(1, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)TxnidLeastBits);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt64(3, (long)TxnidMostBits);
			}
			for (int I = 0; I < Partitions.Count; I++)
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(Partitions[I]));
			}
		}

		internal int MemoizedSerializedSize = -1;
		public int SerializedSize => CalculateSize();

		internal const long SerialVersionUID = 0L;
		
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public Builder NewBuilderForType()
		{
			return NewBuilder();
		}
		public static Builder NewBuilder(CommandAddPartitionToTxn Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandAddPartitionToTxn.newBuilder()
			internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);

			internal ThreadLocalPool.Handle _handle;
			private Builder(ThreadLocalPool.Handle handle)
			{
				_handle = handle;
				MaybeForceBuilderInitialization();
			}

			public void Recycle()
			{
				Clear();
                _handle?.Release(this);
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				TxnidLeastBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnidMostBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000004);
				Partitions_ = new List<string>();
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandAddPartitionToTxn DefaultInstanceForType => CommandAddPartitionToTxn.DefaultInstance;

            public CommandAddPartitionToTxn Build()
			{
				CommandAddPartitionToTxn Result = BuildPartial();
				
				return Result;
			}

			public CommandAddPartitionToTxn BuildParsed()
			{
				CommandAddPartitionToTxn Result = BuildPartial();
				
				return Result;
			}

			public CommandAddPartitionToTxn BuildPartial()
			{
				CommandAddPartitionToTxn Result = CommandAddPartitionToTxn._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.TxnidLeastBits = (ulong)TxnidLeastBits_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.TxnidMostBits = (ulong)TxnidMostBits_;
				if (((BitField0_ & 0x00000008) == 0x00000008))
				{
					Partitions_ = new List<string>(Partitions_);
					BitField0_ = (BitField0_ & ~0x00000008);
				}
				Result.Partitions.AddRange(Partitions_);
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandAddPartitionToTxn Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasTxnidLeastBits)
				{
					SetTxnidLeastBits((long)Other.TxnidLeastBits);
				}
				if (Other.HasTxnidMostBits)
				{
					SetTxnidMostBits((long)Other.TxnidMostBits);
				}
				if (!Other.Partitions.Any())
				{
					if (!Partitions_.Any())
					{
						Partitions_ = Other.Partitions.ToList();
						BitField0_ = (BitField0_ & ~0x00000008);
					}
					else
					{
						EnsurePartitionsIsMutable();
						Partitions_.AddRange(Other.Partitions);
					}

				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasRequestId())
					{

						return false;
					}
					return true;
				}
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
						case 8:
							{
								BitField0_ |= 0x00000001;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								TxnidLeastBits_ = input.ReadUInt64();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								TxnidMostBits_ = input.ReadUInt64();
								break;
							}
						case 34:
							{
								EnsurePartitionsIsMutable();
								Partitions_.Add(input.ReadBytes().ToStringUtf8());
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 request_id = 1;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000001;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;

				return this;
			}

			// optional uint64 txnid_least_bits = 2 [default = 0];
			internal long TxnidLeastBits_;
			public bool HasTxnidLeastBits()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long TxnidLeastBits => TxnidLeastBits_;

            public Builder SetTxnidLeastBits(long Value)
			{
				BitField0_ |= 0x00000002;
				TxnidLeastBits_ = Value;

				return this;
			}
			public Builder ClearTxnidLeastBits()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnidLeastBits_ = 0L;

				return this;
			}

			// optional uint64 txnid_most_bits = 3 [default = 0];
			internal long TxnidMostBits_;
			public bool HasTxnidMostBits()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public long TxnidMostBits => TxnidMostBits_;

            public Builder SetTxnidMostBits(long Value)
			{
				BitField0_ |= 0x00000004;
				TxnidMostBits_ = Value;

				return this;
			}
			public Builder ClearTxnidMostBits()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				TxnidMostBits_ = 0L;

				return this;
			}

			// repeated string partitions = 4;
			internal List<string> Partitions_ = new List<string>();
			public void EnsurePartitionsIsMutable()
			{
				if (!((BitField0_ & 0x00000008) == 0x00000008))
				{
					Partitions_ = new List<string>(Partitions_);
					BitField0_ |= 0x00000008;
				}
			}
			public IList<string> PartitionsList => new List<string>(Partitions_);

            public int PartitionsCount => Partitions_.Count;

            public string GetPartitions(int Index)
			{
				return Partitions_[Index];
			}
			public Builder SetPartitions(int Index, string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsurePartitionsIsMutable();
				Partitions_.Insert(Index, Value);

				return this;
			}
			public Builder AddPartitions(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsurePartitionsIsMutable();
				Partitions_.Add(Value);

				return this;
			}
			public Builder AddAllPartitions(IEnumerable<string> Values)
			{
				EnsurePartitionsIsMutable();
				Partitions_.AddRange(Values);

				return this;
			}
			public Builder ClearPartitions()
			{
				Partitions_ = new List<string>();
				BitField0_ = (BitField0_ & ~0x00000008);

				return this;
			}
			public void AddPartitions(ByteString Value)
			{
				EnsurePartitionsIsMutable();
				Partitions_.Add(Value.ToStringUtf8());

			}

			
		}

		static CommandAddPartitionToTxn()
		{
			_defaultInstance = new CommandAddPartitionToTxn(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandAddPartitionToTxn)
	}

}
