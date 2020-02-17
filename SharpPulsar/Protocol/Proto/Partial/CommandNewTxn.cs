using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandNewTxn : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandNewTxn.newBuilder() to construct.
		internal static ThreadLocalPool<CommandNewTxn> _pool = new ThreadLocalPool<CommandNewTxn>(handle => new CommandNewTxn(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandNewTxn(ThreadLocalPool.Handle handle)
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

		public CommandNewTxn(bool NoInit)
		{
		}

		
		internal static readonly CommandNewTxn _defaultInstance;
		public static CommandNewTxn DefaultInstance => _defaultInstance;

        public CommandNewTxn DefaultInstanceForType => _defaultInstance;


        public void InitFields()
		{
			RequestId = 0L;
			TxnTtlSeconds = 0L;
			TcId = 0L;
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
				Output.WriteUInt64(2, (long)TxnTtlSeconds);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt64(3, (long)TcId);
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
		public static Builder NewBuilder(CommandNewTxn Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandNewTxn.newBuilder()
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
				TxnTtlSeconds_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				TcId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000004);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandNewTxn DefaultInstanceForType => CommandNewTxn.DefaultInstance;

            public CommandNewTxn Build()
			{
				CommandNewTxn Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandNewTxn BuildParsed()
			{
				CommandNewTxn Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandNewTxn BuildPartial()
			{
				CommandNewTxn Result = CommandNewTxn._pool.Take();
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
				Result.TxnTtlSeconds = (ulong)TxnTtlSeconds_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.TcId = (ulong)TcId_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandNewTxn Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasTxnTtlSeconds)
				{
					SetTxnTtlSeconds((long)Other.TxnTtlSeconds);
				}
				if (Other.HasTcId)
				{
					SetTcId((long)Other.TcId);
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
								TxnTtlSeconds_ = input.ReadUInt64();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								TcId_ = input.ReadUInt64();
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

			// optional uint64 txn_ttl_seconds = 2 [default = 0];
			internal long TxnTtlSeconds_;
			public bool HasTxnTtlSeconds()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long TxnTtlSeconds => TxnTtlSeconds_;

            public Builder SetTxnTtlSeconds(long Value)
			{
				BitField0_ |= 0x00000002;
				TxnTtlSeconds_ = Value;

				return this;
			}
			public Builder ClearTxnTtlSeconds()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				TxnTtlSeconds_ = 0L;

				return this;
			}

			// optional uint64 tc_id = 3 [default = 0];
			internal long TcId_;
			public bool HasTcId()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public long TcId => TcId_;

            public Builder SetTcId(long Value)
			{
				BitField0_ |= 0x00000004;
				TcId_ = Value;

				return this;
			}
			public Builder ClearTcId()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				TcId_ = 0L;

				return this;
			}

			
		}

		static CommandNewTxn()
		{
			_defaultInstance = new CommandNewTxn(true);
			_defaultInstance.InitFields();
		}

		
	}

}
