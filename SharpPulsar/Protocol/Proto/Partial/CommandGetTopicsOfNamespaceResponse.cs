using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandGetTopicsOfNamespaceResponse : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandGetTopicsOfNamespaceResponse.newBuilder() to construct.
		internal static ThreadLocalPool<CommandGetTopicsOfNamespaceResponse> _pool = new ThreadLocalPool<CommandGetTopicsOfNamespaceResponse>(handle => new CommandGetTopicsOfNamespaceResponse(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandGetTopicsOfNamespaceResponse(ThreadLocalPool.Handle handle)
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

		public CommandGetTopicsOfNamespaceResponse(bool NoInit)
		{
		}

		
		internal static readonly CommandGetTopicsOfNamespaceResponse _defaultInstance;
		public static CommandGetTopicsOfNamespaceResponse DefaultInstance => _defaultInstance;

        public CommandGetTopicsOfNamespaceResponse DefaultInstanceForType => _defaultInstance;

        public string GetTopics(int Index)
		{
			return Topics[Index];
		}

		public void InitFields()
		{
			RequestId = 0L;
			Topics.Clear();
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
			for (int I = 0; I < Topics.Count; I++)
			{
				Output.WriteBytes(2, ByteString.CopyFromUtf8(Topics[I]));
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
		public static Builder NewBuilder(CommandGetTopicsOfNamespaceResponse Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandGetTopicsOfNamespaceResponse.newBuilder()
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
				Topics_ = new List<string>();
				BitField0_ = (BitField0_ & ~0x00000002);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandGetTopicsOfNamespaceResponse DefaultInstanceForType => DefaultInstance;

            public CommandGetTopicsOfNamespaceResponse Build()
			{
				CommandGetTopicsOfNamespaceResponse Result = BuildPartial();
				
				return Result;
			}

			
			public CommandGetTopicsOfNamespaceResponse BuildParsed()
			{
				CommandGetTopicsOfNamespaceResponse Result = BuildPartial();
				
				return Result;
			}

			public CommandGetTopicsOfNamespaceResponse BuildPartial()
			{
				CommandGetTopicsOfNamespaceResponse Result = CommandGetTopicsOfNamespaceResponse._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((BitField0_ & 0x00000002) == 0x00000002))
				{
					Topics_ = new List<string>(Topics_);
					BitField0_ = (BitField0_ & ~0x00000002);
				}
				Result.Topics.AddRange(Topics_);
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandGetTopicsOfNamespaceResponse Other)
			{
				if (Other == DefaultInstance)
				{
					return this;
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (!Other.Topics.Any())
				{
					if (!Topics_.Any())
					{
						Topics_ = Other.Topics.ToList();
						BitField0_ = (BitField0_ & ~0x00000002);
					}
					else
					{
						EnsureTopicsIsMutable();
						Topics_.AddRange(Other.Topics);
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
						case 18:
							{
								EnsureTopicsIsMutable();
								Topics_.Add(input.ReadBytes().ToStringUtf8());
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

			// repeated string topics = 2;
			internal List<string> Topics_ = new List<string>();
			public void EnsureTopicsIsMutable()
			{
				if (!((BitField0_ & 0x00000002) == 0x00000002))
				{
					Topics_ = new List<string>(Topics_);
					BitField0_ |= 0x00000002;
				}
			}
			public IList<string> TopicsList => new List<string>(Topics_);

            public int TopicsCount => Topics_.Count;

            public string GetTopics(int Index)
			{
				return Topics_[Index];
			}
			public Builder SetTopics(int Index, string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsureTopicsIsMutable();
				Topics_.Insert(Index, Value);

				return this;
			}
			public Builder AddTopics(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				EnsureTopicsIsMutable();
				Topics_.Add(Value);

				return this;
			}
			public Builder AddAllTopics(IEnumerable<string> Values)
			{
				EnsureTopicsIsMutable();
				Topics_.AddRange(Values);

				return this;
			}
			public Builder ClearTopics()
			{
				Topics_.Clear();
				BitField0_ = (BitField0_ & ~0x00000002);

				return this;
			}
			public void AddTopics(ByteString Value)
			{
				EnsureTopicsIsMutable();
				Topics_.Add(Value.ToStringUtf8());

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandGetTopicsOfNamespaceResponse)
		}

		static CommandGetTopicsOfNamespaceResponse()
		{
			_defaultInstance = new CommandGetTopicsOfNamespaceResponse(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandGetTopicsOfNamespaceResponse)
	}

}
