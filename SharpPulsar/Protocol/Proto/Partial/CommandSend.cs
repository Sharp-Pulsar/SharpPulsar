using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandSend : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSend.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSend> _pool = new ThreadLocalPool<CommandSend>(handle => new CommandSend(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSend(ThreadLocalPool.Handle handle)
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

		public CommandSend(bool NoInit)
		{
		}

		
		internal static readonly CommandSend _defaultInstance;
		public static CommandSend DefaultInstance => _defaultInstance;

        public CommandSend DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			ProducerId = 0L;
			SequenceId = 0L;
			NumMessages = 1;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			HighestSequenceId = 0L;
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

				if (!HasProducerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSequenceId)
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
			var _ = SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)ProducerId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)SequenceId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteInt32(3, NumMessages);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(4, (long)TxnidLeastBits);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteUInt64(5, (long)TxnidMostBits);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteUInt64(6, (long)HighestSequenceId);
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
		public static Builder NewBuilder(CommandSend Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSend.newBuilder()
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
				ProducerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				SequenceId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				NumMessages_ = 1;
				BitField0_ = (BitField0_ & ~0x00000004);
				TxnidLeastBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000008);
				TxnidMostBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000010);
				HighestSequenceId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000020);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSend DefaultInstanceForType => CommandSend.DefaultInstance;

            public CommandSend Build()
			{
				CommandSend Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSend BuildParsed()
			{
				CommandSend Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandSend BuildPartial()
			{
				CommandSend Result = CommandSend._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ProducerId = (ulong)ProducerId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.SequenceId = (ulong)SequenceId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.NumMessages = NumMessages_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.TxnidLeastBits = (ulong)TxnidLeastBits_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.TxnidMostBits = (ulong)TxnidMostBits_;
				if (((FromBitField0_ & 0x00000020) == 0x00000020))
				{
					ToBitField0_ |= 0x00000020;
				}
				Result.HighestSequenceId = (ulong)HighestSequenceId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandSend Other)
			{
				if (Other == CommandSend.DefaultInstance)
				{
					return this;
				}
				if (Other.HasProducerId)
				{
					SetProducerId((long)Other.ProducerId);
				}
				if (Other.HasSequenceId)
				{
					SetSequenceId((long)Other.SequenceId);
				}
				if (Other.HasNumMessages)
				{
					SetNumMessages(Other.NumMessages);
				}
				if (Other.HasTxnidLeastBits)
				{
					SetTxnidLeastBits((long)Other.TxnidLeastBits);
				}
				if (Other.HasTxnidMostBits)
				{
					SetTxnidMostBits((long)Other.TxnidMostBits);
				}
				if (Other.HasHighestSequenceId)
				{
					SetHighestSequenceId((long)Other.HighestSequenceId);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasProducerId())
					{

						return false;
					}
					if (!HasSequenceId())
					{

						return false;
					}
					return true;
				}
			}
			public ByteBufMessageBuilder MergeFrom(ByteBufCodedInputStream Input, ExtensionRegistry extensionRegistry)
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
						case 8:
							{
								BitField0_ |= 0x00000001;
								ProducerId_ = Input.ReadUInt64();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								SequenceId_ = Input.ReadUInt64();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								NumMessages_ = Input.ReadInt32();
								break;
							}
						case 32:
							{
								BitField0_ |= 0x00000008;
								TxnidLeastBits_ = Input.ReadUInt64();
								break;
							}
						case 40:
							{
								BitField0_ |= 0x00000010;
								TxnidMostBits_ = Input.ReadUInt64();
								break;
							}
						case 48:
							{
								BitField0_ |= 0x00000020;
								HighestSequenceId_ = Input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 producer_id = 1;
			internal long ProducerId_;
			public bool HasProducerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ProducerId => ProducerId_;

            public Builder SetProducerId(long Value)
			{
				BitField0_ |= 0x00000001;
				ProducerId_ = Value;

				return this;
			}
			public Builder ClearProducerId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ProducerId_ = 0L;

				return this;
			}

			// required uint64 sequence_id = 2;
			internal long SequenceId_;
			public bool HasSequenceId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long SequenceId => SequenceId_;

            public Builder SetSequenceId(long Value)
			{
				BitField0_ |= 0x00000002;
				SequenceId_ = Value;

				return this;
			}
			public Builder ClearSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				SequenceId_ = 0L;

				return this;
			}

			// optional int32 num_messages = 3 [default = 1];
			internal int NumMessages_ = 1;
			public bool HasNumMessages()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public int NumMessages => NumMessages_;

            public Builder SetNumMessages(int Value)
			{
				BitField0_ |= 0x00000004;
				NumMessages_ = Value;

				return this;
			}
			public Builder ClearNumMessages()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				NumMessages_ = 1;

				return this;
			}

			// optional uint64 txnid_least_bits = 4 [default = 0];
			internal long TxnidLeastBits_;
			public bool HasTxnidLeastBits()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public long TxnidLeastBits => TxnidLeastBits_;

            public Builder SetTxnidLeastBits(long Value)
			{
				BitField0_ |= 0x00000008;
				TxnidLeastBits_ = Value;

				return this;
			}
			public Builder ClearTxnidLeastBits()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				TxnidLeastBits_ = 0L;

				return this;
			}

			// optional uint64 txnid_most_bits = 5 [default = 0];
			internal long TxnidMostBits_;
			public bool HasTxnidMostBits()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public long TxnidMostBits => TxnidMostBits_;

            public Builder SetTxnidMostBits(long Value)
			{
				BitField0_ |= 0x00000010;
				TxnidMostBits_ = Value;

				return this;
			}
			public Builder ClearTxnidMostBits()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				TxnidMostBits_ = 0L;

				return this;
			}

			// optional uint64 highest_sequence_id = 6 [default = 0];
			internal long HighestSequenceId_;
			public bool HasHighestSequenceId()
			{
				return ((BitField0_ & 0x00000020) == 0x00000020);
			}
			public long HighestSequenceId => HighestSequenceId_;

            public Builder SetHighestSequenceId(long Value)
			{
				BitField0_ |= 0x00000020;
				HighestSequenceId_ = Value;

				return this;
			}
			public Builder ClearHighestSequenceId()
			{
				BitField0_ = (BitField0_ & ~0x00000020);
				HighestSequenceId_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandSend)
		}

		static CommandSend()
		{
			_defaultInstance = new CommandSend(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSend)
	}

}
