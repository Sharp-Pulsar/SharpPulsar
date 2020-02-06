using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandAddSubscriptionToTxn : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandAddSubscriptionToTxn.newBuilder() to construct.
		internal static ThreadLocalPool<CommandAddSubscriptionToTxn> _pool = new ThreadLocalPool<CommandAddSubscriptionToTxn>(handle => new CommandAddSubscriptionToTxn(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandAddSubscriptionToTxn(ThreadLocalPool.Handle handle)
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

		public CommandAddSubscriptionToTxn(bool NoInit)
		{
		}

		internal static readonly CommandAddSubscriptionToTxn _defaultInstance;
		public static CommandAddSubscriptionToTxn DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandAddSubscriptionToTxn DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}

		
		public void InitFields()
		{
			RequestId = 0L;
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
			Subscription.Clear();
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
				for (int I = 0; I < Subscription.Count; I++)
				{
					if (!Subscription[I].Initialized)
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
			for (int I = 0; I < Subscription.Count; I++)
			{
				Output.WriteMessage(4, Subscription[I]);
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
		public static Builder NewBuilder(CommandAddSubscriptionToTxn Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandAddSubscriptionToTxn.newBuilder()
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
				Subscription_ = new List<Subscription>();
				BitField0_ = (BitField0_ & ~0x00000008);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandAddSubscriptionToTxn DefaultInstanceForType
			{
				get
				{
					return DefaultInstance;
				}
			}

			public CommandAddSubscriptionToTxn Build()
			{
				CommandAddSubscriptionToTxn Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}
			public CommandAddSubscriptionToTxn BuildParsed()
			{
				CommandAddSubscriptionToTxn Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandAddSubscriptionToTxn BuildPartial()
			{
				CommandAddSubscriptionToTxn Result = CommandAddSubscriptionToTxn._pool.Take();
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
					Subscription_ = new List<Subscription>(Subscription_);
					BitField0_ = (BitField0_ & ~0x00000008);
				}
				Result.Subscription.AddRange(Subscription_);
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandAddSubscriptionToTxn Other)
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
				if (Other.Subscription.Count > 0)
				{
					if (Subscription_.Count == 0)
					{
						Subscription_ = Other.Subscription;
						BitField0_ = (BitField0_ & ~0x00000008);
					}
					else
					{
						EnsureSubscriptionIsMutable();
						((List<Subscription>)Subscription_).AddRange(Other.Subscription);
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
					for (int I = 0; I < SubscriptionCount; I++)
					{
						if (!GetSubscription(I).Initialized)
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
								Subscription.Builder SubBuilder = Proto.Subscription.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddSubscription(SubBuilder.BuildPartial());
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
			public long RequestId
			{
				get
				{
					return RequestId_;
				}
			}
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
			public long TxnidLeastBits
			{
				get
				{
					return TxnidLeastBits_;
				}
			}
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
			public long TxnidMostBits
			{
				get
				{
					return TxnidMostBits_;
				}
			}
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

			// repeated .pulsar.proto.Subscription subscription = 4;
			internal IList<Subscription> Subscription_ = new List<Subscription>();
			public void EnsureSubscriptionIsMutable()
			{
				if (!((BitField0_ & 0x00000008) == 0x00000008))
				{
					Subscription_ = new List<Subscription>(Subscription_);
					BitField0_ |= 0x00000008;
				}
			}

			public IList<Subscription> SubscriptionList
			{
				get
				{
					return new List<Subscription>(Subscription_);
				}
			}
			public int SubscriptionCount
			{
				get
				{
					return Subscription_.Count;
				}
			}
			public Subscription GetSubscription(int Index)
			{
				return Subscription_[Index];
			}
			public Builder SetSubscription(int Index, Subscription Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureSubscriptionIsMutable();
				Subscription_[Index] = Value;

				return this;
			}
			public Builder SetSubscription(int Index, Subscription.Builder BuilderForValue)
			{
				EnsureSubscriptionIsMutable();
				Subscription_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddSubscription(Subscription Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureSubscriptionIsMutable();
				Subscription_.Add(Value);

				return this;
			}
			public Builder AddSubscription(int Index, Subscription Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureSubscriptionIsMutable();
				Subscription_.Insert(Index, Value);

				return this;
			}
			public Builder AddSubscription(Subscription.Builder BuilderForValue)
			{
				EnsureSubscriptionIsMutable();
				Subscription_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddSubscription(int Index, Subscription.Builder BuilderForValue)
			{
				EnsureSubscriptionIsMutable();
				Subscription_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllSubscription(IEnumerable<Subscription> Values)
			{
				EnsureSubscriptionIsMutable();
				Values.ToList().ForEach(Subscription_.Add);

				return this;
			}
			public Builder ClearSubscription()
			{
				Subscription_ = new List<Subscription>();
				BitField0_ = (BitField0_ & ~0x00000008);

				return this;
			}
			public Builder RemoveSubscription(int Index)
			{
				EnsureSubscriptionIsMutable();
				Subscription_.RemoveAt(Index);

				return this;
			}

			
		}

		static CommandAddSubscriptionToTxn()
		{
			_defaultInstance = new CommandAddSubscriptionToTxn(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandAddSubscriptionToTxn)
	}

}
