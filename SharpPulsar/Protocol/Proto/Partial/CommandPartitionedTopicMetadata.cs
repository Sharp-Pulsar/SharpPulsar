using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandPartitionedTopicMetadata : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandPartitionedTopicMetadata.newBuilder() to construct.
		internal static ThreadLocalPool<CommandPartitionedTopicMetadata> _pool = new ThreadLocalPool<CommandPartitionedTopicMetadata>(handle => new CommandPartitionedTopicMetadata(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandPartitionedTopicMetadata(ThreadLocalPool.Handle handle)
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

		public CommandPartitionedTopicMetadata(bool NoInit)
		{
		}

		
		internal static readonly CommandPartitionedTopicMetadata _defaultInstance;
		public static CommandPartitionedTopicMetadata DefaultInstance => _defaultInstance;

        public CommandPartitionedTopicMetadata DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			Topic = "";
			RequestId = 0L;
			OriginalPrincipal = "";
			OriginalAuthData = "";
			OriginalAuthMethod = "";
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
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Topic));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteBytes(3, ByteString.CopyFromUtf8(OriginalPrincipal));
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(OriginalAuthData));
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBytes(5, ByteString.CopyFromUtf8(OriginalAuthMethod));
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
		public static Builder NewBuilder(CommandPartitionedTopicMetadata Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandPartitionedTopicMetadata.newBuilder()
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

				Topic_ = "";
				BitField0_ = (BitField0_ & ~0x00000001);
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				OriginalPrincipal_ = "";
				BitField0_ = (BitField0_ & ~0x00000004);
				OriginalAuthData_ = "";
				BitField0_ = (BitField0_ & ~0x00000008);
				OriginalAuthMethod_ = "";
				BitField0_ = (BitField0_ & ~0x00000010);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandPartitionedTopicMetadata DefaultInstanceForType => CommandPartitionedTopicMetadata.DefaultInstance;

            public CommandPartitionedTopicMetadata Build()
			{
				CommandPartitionedTopicMetadata Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}
						
			public CommandPartitionedTopicMetadata BuildParsed()
			{
				CommandPartitionedTopicMetadata Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandPartitionedTopicMetadata BuildPartial()
			{
				CommandPartitionedTopicMetadata Result = CommandPartitionedTopicMetadata._pool.Take();
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
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.OriginalPrincipal = OriginalPrincipal_.ToString();
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.OriginalAuthData = OriginalAuthData_.ToString();
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.OriginalAuthMethod = OriginalAuthMethod_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandPartitionedTopicMetadata Other)
			{
				if (Other == CommandPartitionedTopicMetadata.DefaultInstance)
				{
					return this;
				}
				if (Other.HasTopic)
				{
					SetTopic(Other.Topic);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasOriginalPrincipal)
				{
					SetOriginalPrincipal(Other.OriginalPrincipal);
				}
				if (Other.HasOriginalAuthData)
				{
					SetOriginalAuthData(Other.OriginalAuthData);
				}
				if (Other.HasOriginalAuthMethod)
				{
					SetOriginalAuthMethod(Other.OriginalAuthMethod);
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
						case 10:
							{
								BitField0_ |= 0x00000001;
								Topic_ = input.ReadBytes();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 26:
							{
								BitField0_ |= 0x00000004;
								OriginalPrincipal_ = input.ReadBytes();
								break;
							}
						case 34:
							{
								BitField0_ |= 0x00000008;
								OriginalAuthData_ = input.ReadBytes();
								break;
							}
						case 42:
							{
								BitField0_ |= 0x00000010;
								OriginalAuthMethod_ = input.ReadBytes();
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

			// required uint64 request_id = 2;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000002;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				RequestId_ = 0L;

				return this;
			}

			// optional string original_principal = 3;
			internal object OriginalPrincipal_ = "";
			public bool HasOriginalPrincipal()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public string GetOriginalPrincipal()
			{
				object Ref = OriginalPrincipal_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					OriginalPrincipal_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetOriginalPrincipal(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000004;
				OriginalPrincipal_ = Value;

				return this;
			}
			public Builder ClearOriginalPrincipal()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				OriginalPrincipal_ = DefaultInstance.OriginalPrincipal;

				return this;
			}
			public void SetOriginalPrincipal(ByteString Value)
			{
				BitField0_ |= 0x00000004;
				OriginalPrincipal_ = Value;

			}

			// optional string original_auth_data = 4;
			internal object OriginalAuthData_ = "";
			public bool HasOriginalAuthData()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public string GetOriginalAuthData()
			{
				object Ref = OriginalAuthData_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					OriginalAuthData_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetOriginalAuthData(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				OriginalAuthData_ = Value;

				return this;
			}
			public Builder ClearOriginalAuthData()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				OriginalAuthData_ = DefaultInstance.OriginalAuthData;

				return this;
			}
			public void SetOriginalAuthData(ByteString Value)
			{
				BitField0_ |= 0x00000008;
				OriginalAuthData_ = Value;

			}

			// optional string original_auth_method = 5;
			internal object OriginalAuthMethod_ = "";
			public bool HasOriginalAuthMethod()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public string GetOriginalAuthMethod()
			{
				object Ref = OriginalAuthMethod_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					OriginalAuthMethod_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetOriginalAuthMethod(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000010;
				OriginalAuthMethod_ = Value;

				return this;
			}
			public Builder ClearOriginalAuthMethod()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				OriginalAuthMethod_ = DefaultInstance.OriginalAuthMethod;

				return this;
			}
			public void SetOriginalAuthMethod(ByteString Value)
			{
				BitField0_ |= 0x00000010;
				OriginalAuthMethod_ = Value;

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandPartitionedTopicMetadata)
		}

		static CommandPartitionedTopicMetadata()
		{
			_defaultInstance = new CommandPartitionedTopicMetadata(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandPartitionedTopicMetadata)
	}

}
