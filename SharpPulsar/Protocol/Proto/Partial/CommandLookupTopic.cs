using DotNetty.Common;
using Google.Protobuf;
using System;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public sealed partial class CommandLookupTopic : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandLookupTopic.newBuilder() to construct.
		internal static ThreadLocalPool<CommandLookupTopic> _pool = new ThreadLocalPool<CommandLookupTopic>(handle => new CommandLookupTopic(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandLookupTopic(ThreadLocalPool.Handle handle)
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

		public CommandLookupTopic(bool NoInit)
		{
		}

		
		internal static readonly CommandLookupTopic _defaultInstance;
		public static CommandLookupTopic DefaultInstance => _defaultInstance;

        public CommandLookupTopic DefaultInstanceForType => _defaultInstance;

        public void InitFields()
		{
			Topic = "";
			RequestId = 0L;
			Authoritative = false;
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
				Output.WriteBool(3, Authoritative);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(OriginalPrincipal));
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBytes(5, ByteString.CopyFromUtf8(OriginalAuthData));
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteBytes(6, ByteString.CopyFromUtf8(OriginalAuthMethod));
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
		public static Builder NewBuilder(CommandLookupTopic Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandLookupTopic.newBuilder()
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
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				Authoritative_ = false;
				BitField0_ = (BitField0_ & ~0x00000004);
				OriginalPrincipal_ = "";
				BitField0_ = (BitField0_ & ~0x00000008);
				OriginalAuthData_ = "";
				BitField0_ = (BitField0_ & ~0x00000010);
				OriginalAuthMethod_ = "";
				BitField0_ = (BitField0_ & ~0x00000020);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandLookupTopic DefaultInstanceForType => CommandLookupTopic.DefaultInstance;

            public CommandLookupTopic Build()
			{
				CommandLookupTopic Result = BuildPartial();
				
				return Result;
			}
						
			public CommandLookupTopic BuildParsed()
			{
				CommandLookupTopic Result = BuildPartial();
				
				return Result;
			}

			public CommandLookupTopic BuildPartial()
			{
				CommandLookupTopic Result = CommandLookupTopic._pool.Take();
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
				Result.Authoritative = Authoritative_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.OriginalPrincipal = OriginalPrincipal_.ToString();
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.OriginalAuthData = OriginalAuthData_.ToString();
				if (((FromBitField0_ & 0x00000020) == 0x00000020))
				{
					ToBitField0_ |= 0x00000020;
				}
				Result.OriginalAuthMethod = OriginalAuthMethod_.ToString();
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandLookupTopic Other)
			{
				if (Other == CommandLookupTopic.DefaultInstance)
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
				if (Other.HasAuthoritative)
				{
					SetAuthoritative(Other.Authoritative);
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
						case 24:
							{
								BitField0_ |= 0x00000004;
								Authoritative_ = input.ReadBool();
								break;
							}
						case 34:
							{
								BitField0_ |= 0x00000008;
								OriginalPrincipal_ = input.ReadBytes();
								break;
							}
						case 42:
							{
								BitField0_ |= 0x00000010;
								OriginalAuthData_ = input.ReadBytes();
								break;
							}
						case 50:
							{
								BitField0_ |= 0x00000020;
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

			// optional bool authoritative = 3 [default = false];
			internal bool Authoritative_;
			public bool HasAuthoritative()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public bool Authoritative => Authoritative_;

            public Builder SetAuthoritative(bool Value)
			{
				BitField0_ |= 0x00000004;
				Authoritative_ = Value;

				return this;
			}
			public Builder ClearAuthoritative()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				Authoritative_ = false;

				return this;
			}

			// optional string original_principal = 4;
			internal object OriginalPrincipal_ = "";
			public bool HasOriginalPrincipal()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
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
				BitField0_ |= 0x00000008;
				OriginalPrincipal_ = Value;

				return this;
			}
			public Builder ClearOriginalPrincipal()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				OriginalPrincipal_ = DefaultInstance.OriginalPrincipal;

				return this;
			}
			public void SetOriginalPrincipal(ByteString Value)
			{
				BitField0_ |= 0x00000008;
				OriginalPrincipal_ = Value;

			}

			// optional string original_auth_data = 5;
			internal object OriginalAuthData_ = "";
			public bool HasOriginalAuthData()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
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
				BitField0_ |= 0x00000010;
				OriginalAuthData_ = Value;

				return this;
			}
			public Builder ClearOriginalAuthData()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				OriginalAuthData_ = DefaultInstance.OriginalAuthData;

				return this;
			}
			public void SetOriginalAuthData(ByteString Value)
			{
				BitField0_ |= 0x00000010;
				OriginalAuthData_ = Value;

			}

			// optional string original_auth_method = 6;
			internal object OriginalAuthMethod_ = "";
			public bool HasOriginalAuthMethod()
			{
				return ((BitField0_ & 0x00000020) == 0x00000020);
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
				BitField0_ |= 0x00000020;
				OriginalAuthMethod_ = Value;

				return this;
			}
			public Builder ClearOriginalAuthMethod()
			{
				BitField0_ = (BitField0_ & ~0x00000020);
				OriginalAuthMethod_ = DefaultInstance.OriginalAuthMethod;

				return this;
			}
			public void SetOriginalAuthMethod(ByteString Value)
			{
				BitField0_ |= 0x00000020;
				OriginalAuthMethod_ = Value;

			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandLookupTopic)
		}

		static CommandLookupTopic()
		{
			_defaultInstance = new CommandLookupTopic(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandLookupTopic)
	}

}
