using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandEndTxnOnSubscription : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandEndTxnOnSubscription.newBuilder() to construct.
		internal static ThreadLocalPool<CommandEndTxnOnSubscription> _pool = new ThreadLocalPool<CommandEndTxnOnSubscription>(handle => new CommandEndTxnOnSubscription(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandEndTxnOnSubscription(ThreadLocalPool.Handle handle)
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

		public CommandEndTxnOnSubscription(bool NoInit)
		{
		}

		
		internal static readonly CommandEndTxnOnSubscription _defaultInstance;
		public static CommandEndTxnOnSubscription DefaultInstance => _defaultInstance;

        public CommandEndTxnOnSubscription DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			Subscription = Subscription.DefaultInstance;
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
				if (HasSubscription)
				{
					if (!Subscription.Initialized)
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
				Output.WriteMessage(4, Subscription);
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
		public static Builder NewBuilder(CommandEndTxnOnSubscription Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandEndTxnOnSubscription.newBuilder()
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
				Subscription_ = Subscription.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000008);
				TxnAction_ = TxnAction.Commit;
				BitField0_ = (BitField0_ & ~0x00000010);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandEndTxnOnSubscription DefaultInstanceForType => DefaultInstance;

            public CommandEndTxnOnSubscription Build()
			{
				CommandEndTxnOnSubscription Result = BuildPartial();
				
				return Result;
			}
			public CommandEndTxnOnSubscription BuildParsed()
			{
				CommandEndTxnOnSubscription Result = BuildPartial();
				
				return Result;
			}

			public CommandEndTxnOnSubscription BuildPartial()
			{
				CommandEndTxnOnSubscription Result = CommandEndTxnOnSubscription._pool.Take();
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
				Result.Subscription = Subscription_;
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.TxnAction = TxnAction_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandEndTxnOnSubscription Other)
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
				if (Other.HasSubscription)
				{
					MergeSubscription(Other.Subscription);
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
					if (HasSubscription())
					{
						if (!GetSubscription().Initialized)
						{

							return false;
						}
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
								Subscription.Builder SubBuilder = Subscription.NewBuilder();
								if (HasSubscription())
								{
									SubBuilder.MergeFrom(GetSubscription());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSubscription(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
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

			// optional .pulsar.proto.Subscription subscription = 4;
			internal Subscription Subscription_ = Subscription.DefaultInstance;
			public bool HasSubscription()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public Subscription GetSubscription()
			{
				return Subscription_;
			}
			public Builder SetSubscription(Subscription Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Subscription_ = Value;

				BitField0_ |= 0x00000008;
				return this;
			}
			public Builder SetSubscription(Subscription.Builder BuilderForValue)
			{
				Subscription_ = BuilderForValue.Build();

				BitField0_ |= 0x00000008;
				return this;
			}
			public Builder MergeSubscription(Subscription Value)
			{
				if (((BitField0_ & 0x00000008) == 0x00000008) && Subscription_ != Subscription.DefaultInstance)
				{
					Subscription_ = Subscription.NewBuilder(Subscription_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Subscription_ = Value;
				}

				BitField0_ |= 0x00000008;
				return this;
			}
			public Builder ClearSubscription()
			{
				Subscription_ = Subscription.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
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

		static CommandEndTxnOnSubscription()
		{
			_defaultInstance = new CommandEndTxnOnSubscription(true);
			_defaultInstance.InitFields();
		}

	}

}
