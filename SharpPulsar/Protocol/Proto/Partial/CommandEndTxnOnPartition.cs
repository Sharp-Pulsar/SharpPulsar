using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandEndTxnOnPartition : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandEndTxnOnPartition.newBuilder() to construct.
		internal static ThreadLocalPool<CommandEndTxnOnPartition> _pool = new ThreadLocalPool<CommandEndTxnOnPartition>(handle => new CommandEndTxnOnPartition(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandEndTxnOnPartition(ThreadLocalPool.Handle handle)
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

		public CommandEndTxnOnPartition(bool NoInit)
		{
		}

		
		internal static readonly CommandEndTxnOnPartition DefaultInstanceConflict;
		public static CommandEndTxnOnPartition DefaultInstance => DefaultInstanceConflict;

        public CommandEndTxnOnPartition DefaultInstanceForType => DefaultInstanceConflict;


        public void InitFields()
		{
			RequestId = 0L;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			Topic = "";
			TxnAction = TxnAction.Commit;
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
			var _ = SerializedSize;
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
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(Topic));
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteEnum(5, (int)TxnAction);
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
		public static Builder NewBuilder(CommandEndTxnOnPartition Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandEndTxnOnPartition.newBuilder()
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				TxnidLeastBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnidMostBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000004);
				Topic_ = "";
				BitField0_ = (BitField0_ & ~0x00000008);
				TxnAction_ = TxnAction.Commit;
				BitField0_ = (BitField0_ & ~0x00000010);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandEndTxnOnPartition DefaultInstanceForType => DefaultInstance;

            public CommandEndTxnOnPartition Build()
			{
				CommandEndTxnOnPartition Result = BuildPartial();
				
				return Result;
			}

			
			public CommandEndTxnOnPartition BuildParsed()
			{
				CommandEndTxnOnPartition Result = BuildPartial();
				
				return Result;
			}

			public CommandEndTxnOnPartition BuildPartial()
			{
				CommandEndTxnOnPartition Result = CommandEndTxnOnPartition._pool.Take();
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
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.Topic = Topic_.ToString();
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.TxnAction = TxnAction_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandEndTxnOnPartition Other)
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
				if (Other.HasTopic)
				{
					SetTopic(Other.Topic);
				}
				if (Other.HasTxnAction)
				{
					SetTxnAction(Other.TxnAction);
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
								BitField0_ |= 0x00000008;
								Topic_ = input.ReadBytes();
								break;
							}
						case 40:
							{
								int RawValue = input.ReadEnum();
								TxnAction Value = Enum.GetValues(typeof(TxnAction)).Cast<TxnAction>().ToList()[RawValue]; 
								if (Value != null)
								{
									BitField0_ |= 0x00000010;
									TxnAction_ = Value;
								}
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

			// optional string topic = 4;
			internal object Topic_ = "";
			public bool HasTopic()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public string GetTopic()
			{
				object Ref = Topic_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Topic_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetTopic(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				Topic_ = Value;

				return this;
			}
			public Builder ClearTopic()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				Topic_ = DefaultInstance.Topic;

				return this;
			}
			public void SetTopic(ByteString Value)
			{
				BitField0_ |= 0x00000008;
				Topic_ = Value;

			}

			// optional .pulsar.proto.TxnAction txn_action = 5;
			internal TxnAction TxnAction_ = TxnAction.Commit;
			public bool HasTxnAction()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public TxnAction TxnAction => TxnAction_;

            public Builder SetTxnAction(TxnAction Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000010;
				TxnAction_ = Value;

				return this;
			}
			public Builder ClearTxnAction()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				TxnAction_ = TxnAction.Commit;

				return this;
			}

		}

		static CommandEndTxnOnPartition()
		{
			DefaultInstanceConflict = new CommandEndTxnOnPartition(true);
			DefaultInstanceConflict.InitFields();
		}

	}

}
