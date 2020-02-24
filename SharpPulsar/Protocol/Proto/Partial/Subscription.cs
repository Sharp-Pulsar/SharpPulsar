using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class Subscription : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use Subscription.newBuilder() to construct.
		internal static ThreadLocalPool<Subscription> _pool = new ThreadLocalPool<Subscription>(handle => new Subscription(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private Subscription(ThreadLocalPool.Handle handle)
		{
			_handle = handle;
		}
		public void Recycle()
		{
			InitFields();
			MemoizedIsInitialized = -1;
			BitField0_ = 0;
			MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

		public Subscription(bool NoInit)
		{
		}

		internal static readonly Subscription _defaultInstance;
		public static Subscription DefaultInstance => _defaultInstance;

        public Subscription DefaultInstanceForType => _defaultInstance;

        internal int BitField0_;
		
		public void InitFields()
		{
			Topic = "";
			Subscription_ = "";
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

				if (!HasTopic)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSubscription_)
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
			if (((BitField0_ & 0x00000001) == 0x00000001))
			{
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Topic));
			}
			if (((BitField0_ & 0x00000002) == 0x00000002))
			{
				Output.WriteBytes(2, ByteString.CopyFromUtf8(Subscription_));
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
		public static Builder NewBuilder(Subscription Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.Subscription.newBuilder()
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
				Topic_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				Subscription_ = "";
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public Subscription DefaultInstanceForType => DefaultInstance;

            public Subscription Build()
			{
				Subscription Result = BuildPartial();
				
				return Result;
			}
			public Subscription BuildParsed()
			{
				Subscription Result = BuildPartial();
				
				return Result;
			}

			public Subscription BuildPartial()
			{
				Subscription Result = Subscription._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.Topic = Topic_.ToString();
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.Subscription_ = ((ByteString)Subscription_).ToStringUtf8();
				Result.BitField0_ = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(Subscription Other)
			{
				if (Other == Subscription.DefaultInstance)
				{
					return this;
				}
				if (Other.HasTopic)
				{
					SetTopic(Other.Topic);
				}
				if (Other.HasSubscription_)
				{
					SetSubscription(ByteString.CopyFromUtf8(Other.subscription_));
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasTopic())
					{

						return false;
					}
					if (!HasSubscription())
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
						case 10:
							{
								BitField0_ |= 0x00000001;
								Topic_ = input.ReadBytes();
								break;
							}
						case 18:
							{
								BitField0_ |= 0x00000002;
								Subscription_ = input.ReadBytes();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required string topic = 1;
			internal object Topic_ = "";
			public bool HasTopic()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public string getTopic()
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
				BitField0_ |= 0x00000001;
				Topic_ = Value;

				return this;
			}
			public Builder ClearTopic()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				Topic_ = DefaultInstance.Topic;

				return this;
			}
			public void SetTopic(ByteString Value)
			{
				BitField0_ |= 0x00000001;
				Topic_ = Value;

			}

			// required string subscription = 2;
			internal object Subscription_ = "";
			public bool HasSubscription()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public string GetSubscription()
			{
				object Ref = Subscription_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					Subscription_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder GetSubscription(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				Subscription_ = Value;

				return this;
			}
			public Builder ClearSubscription()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				Subscription_ = DefaultInstance.subscription_;

				return this;
			}
			public void SetSubscription(ByteString Value)
			{
				BitField0_ |= 0x00000002;
				Subscription_ = Value;

			}

			
		}

		static Subscription()
		{
			_defaultInstance = new Subscription(true);
			_defaultInstance.InitFields();
		}

	}

}
