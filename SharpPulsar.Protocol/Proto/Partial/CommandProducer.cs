using DotNetty.Common;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpPulsar.Utility.Protobuf;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Utility.Protobuf.ByteBufCodedOutputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandProducer : ByteBufGeneratedMessage
	{
		// Use CommandProducer.newBuilder() to construct.
		internal static ThreadLocalPool<CommandProducer> _pool = new ThreadLocalPool<CommandProducer>(handle => new CommandProducer(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandProducer(ThreadLocalPool.Handle handle)
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

		public CommandProducer(bool NoInit)
		{
		}

		internal static readonly CommandProducer _defaultInstance;
		public static CommandProducer DefaultInstance => _defaultInstance;

        public CommandProducer DefaultInstanceForType => _defaultInstance;

        public KeyValue GetMetadata(int Index)
		{
			return Metadata[Index];
		}
		
		public void InitFields()
		{
			Topic = "";
			ProducerId = 0L;
			RequestId = 0L;
			ProducerName = "";
			Encrypted = false;
			Metadata.Clear();
			Schema = Schema.DefaultInstance;
			Epoch = 0L;
			UserProvidedProducerName = true;
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
				if (!HasProducerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				for (int I = 0; I < Metadata.Count; I++)
				{
					if (!GetMetadata(I).Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				if (HasSchema)
				{
					if (!Schema.Initialized)
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
				Output.WriteBytes(1, ByteString.CopyFromUtf8(Topic));
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteUInt64(2, (long)ProducerId);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteUInt64(3, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteBytes(4, ByteString.CopyFromUtf8(ProducerName));
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteBool(5, Encrypted);
			}
			for (int I = 0; I < Metadata.Count; I++)
			{
				Output.WriteMessage(6, Metadata[I]);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteMessage(7, Schema);
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				Output.WriteUInt64(8, (long)Epoch);
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				Output.WriteBool(9, UserProvidedProducerName);
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
		public static Builder NewBuilder(CommandProducer Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandProducer.newBuilder()
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
				ProducerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000002);
				RequestId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000004);
				ProducerName_ = "";
				BitField0_ = (BitField0_ & ~0x00000008);
				Encrypted_ = false;
				BitField0_ = (BitField0_ & ~0x00000010);
				Metadata_.Clear();
				BitField0_ = (BitField0_ & ~0x00000020);
				Schema_ = Schema.DefaultInstance;
				BitField0_ = (BitField0_ & ~0x00000040);
				Epoch_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000080);
				UserProvidedProducerName_ = true;
				BitField0_ = (BitField0_ & ~0x00000100);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandProducer DefaultInstanceForType => CommandProducer.DefaultInstance;

            public CommandProducer Build()
			{
				CommandProducer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandProducer BuildParsed()
			{
				CommandProducer Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandProducer BuildPartial()
			{
				CommandProducer Result = CommandProducer._pool.Take();
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
				Result.ProducerId = (ulong)ProducerId_;
				if (((FromBitField0_ & 0x00000004) == 0x00000004))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.ProducerName = ProducerName_.ToString();
				if (((FromBitField0_ & 0x00000010) == 0x00000010))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.Encrypted = Encrypted_;
				if (((BitField0_ & 0x00000020) == 0x00000020))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					BitField0_ = (BitField0_ & ~0x00000020);
				}
				Metadata_.ToList().ForEach(Result.Metadata.Add);
				if (((FromBitField0_ & 0x00000040) == 0x00000040))
				{
					ToBitField0_ |= 0x00000020;
				}
				Result.Schema = Schema_;
				if (((FromBitField0_ & 0x00000080) == 0x00000080))
				{
					ToBitField0_ |= 0x00000040;
				}
				Result.Epoch = (ulong)Epoch_;
				if (((FromBitField0_ & 0x00000100) == 0x00000100))
				{
					ToBitField0_ |= 0x00000080;
				}
				Result.UserProvidedProducerName = UserProvidedProducerName_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandProducer Other)
			{
				if (Other == CommandProducer.DefaultInstance)
				{
					return this;
				}
				if (Other.HasTopic)
				{
					SetTopic(Other.Topic);
				}
				if (Other.HasProducerId)
				{
					SetProducerId((long)Other.ProducerId);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasProducerName)
				{
					SetProducerName(Other.ProducerName);
				}
				if (Other.HasEncrypted)
				{
					SetEncrypted(Other.Encrypted);
				}
				if (Other.Metadata.Count > 0)
				{
					if (Metadata_.Count == 0)
					{
						Metadata_ = Other.Metadata;
						BitField0_ = (BitField0_ & ~0x00000020);
					}
					else
					{
						EnsureMetadataIsMutable();
						((List<KeyValue>)Metadata_).AddRange(Other.Metadata);
					}

				}
				if (Other.HasSchema)
				{
					MergeSchema(Other.Schema);
				}
				if (Other.HasEpoch)
				{
					SetEpoch((long)Other.Epoch);
				}
				if (Other.HasUserProvidedProducerName)
				{
					SetUserProvidedProducerName(Other.UserProvidedProducerName);
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
					if (!HasProducerId())
					{

						return false;
					}
					if (!HasRequestId())
					{

						return false;
					}
					for (int I = 0; I < MetadataCount; I++)
					{
						if (!GetMetadata(I).Initialized)
						{

							return false;
						}
					}
					if (HasSchema())
					{
						if (!GetSchema().Initialized)
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
						case 10:
							{
								BitField0_ |= 0x00000001;
								Topic_ = input.ReadBytes();
								break;
							}
						case 16:
							{
								BitField0_ |= 0x00000002;
								ProducerId_ = input.ReadUInt64();
								break;
							}
						case 24:
							{
								BitField0_ |= 0x00000004;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 34:
							{
								BitField0_ |= 0x00000008;
								ProducerName_ = input.ReadBytes();
								break;
							}
						case 40:
							{
								BitField0_ |= 0x00000010;
								Encrypted_ =input.ReadBool();
								break;
							}
						case 50:
							{
								KeyValue.Builder SubBuilder = KeyValue.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddMetadata(SubBuilder.BuildPartial());
								break;
							}
						case 58:
							{
								Schema.Builder SubBuilder = Schema.NewBuilder();
								if (HasSchema())
								{
									SubBuilder.MergeFrom(GetSchema());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetSchema(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
						case 64:
							{
								BitField0_ |= 0x00000080;
								Epoch_ = input.ReadUInt64();
								break;
							}
						case 72:
							{
								BitField0_ |= 0x00000100;
								UserProvidedProducerName_ = input.ReadBool();
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

			// required uint64 producer_id = 2;
			internal long ProducerId_;
			public bool HasProducerId()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public long ProducerId => ProducerId_;

            public Builder SetProducerId(long Value)
			{
				BitField0_ |= 0x00000002;
				ProducerId_ = Value;

				return this;
			}
			public Builder ClearProducerId()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				ProducerId_ = 0L;

				return this;
			}

			// required uint64 request_id = 3;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((BitField0_ & 0x00000004) == 0x00000004);
			}
			public long RequestId => RequestId_;

            public Builder SetRequestId(long Value)
			{
				BitField0_ |= 0x00000004;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				BitField0_ = (BitField0_ & ~0x00000004);
				RequestId_ = 0L;

				return this;
			}

			// optional string producer_name = 4;
			internal object ProducerName_ = "";
			public bool HasProducerName()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public string GetProducerName()
			{
				object Ref = ProducerName_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ProducerName_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetProducerName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000008;
				ProducerName_ = Value;

				return this;
			}
			public Builder ClearProducerName()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				ProducerName_ = DefaultInstance.ProducerName;

				return this;
			}
			public void SetProducerName(ByteString Value)
			{
				BitField0_ |= 0x00000008;
				ProducerName_ = Value;

			}

			// optional bool encrypted = 5 [default = false];
			internal bool Encrypted_;
			public bool HasEncrypted()
			{
				return ((BitField0_ & 0x00000010) == 0x00000010);
			}
			public bool Encrypted => Encrypted_;

            public Builder SetEncrypted(bool Value)
			{
				BitField0_ |= 0x00000010;
				Encrypted_ = Value;

				return this;
			}
			public Builder ClearEncrypted()
			{
				BitField0_ = (BitField0_ & ~0x00000010);
				Encrypted_ = false;

				return this;
			}

			// repeated .pulsar.proto.KeyValue metadata = 6;
			internal IList<KeyValue> Metadata_ = new  List<KeyValue>();
			public void EnsureMetadataIsMutable()
			{
				if (!((BitField0_ & 0x00000020) == 0x00000020))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					BitField0_ |= 0x00000020;
				}
			}

			public IList<KeyValue> MetadataList => new List<KeyValue>(Metadata_);

            public int MetadataCount => Metadata_.Count;

            public KeyValue GetMetadata(int Index)
			{
				return Metadata_[Index];
			}
			public Builder SetMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_[Index] = Value;

				return this;
			}
			public Builder SetMetadata(int Index, KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddMetadata(KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_.Add(Value);

				return this;
			}
			public Builder AddMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_.Insert(Index, Value);

				return this;
			}
			public Builder AddMetadata(KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddMetadata(int Index, KeyValue.Builder BuilderForValue)
			{
				EnsureMetadataIsMutable();
				Metadata_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllMetadata(IEnumerable<KeyValue> Values)
			{
				EnsureMetadataIsMutable();
				Values.ToList().ForEach(Metadata_.Add);

				return this;
			}
			public Builder ClearMetadata()
			{
				Metadata_ = new List<KeyValue>();
				BitField0_ = (BitField0_ & ~0x00000020);

				return this;
			}
			public Builder RemoveMetadata(int Index)
			{
				EnsureMetadataIsMutable();
				Metadata_.RemoveAt(Index);

				return this;
			}

			// optional .pulsar.proto.Schema schema = 7;
			internal Schema Schema_ = Schema.DefaultInstance;
			public bool HasSchema()
			{
				return ((BitField0_ & 0x00000040) == 0x00000040);
			}
			public Schema GetSchema()
			{
				return Schema_;
			}
			public Builder SetSchema(Schema Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				Schema_ = Value;

				BitField0_ |= 0x00000040;
				return this;
			}
			public Builder SetSchema(Schema.Builder BuilderForValue)
			{
				Schema_ = BuilderForValue.Build();

				BitField0_ |= 0x00000040;
				return this;
			}
			public Builder MergeSchema(Schema Value)
			{
				if (((BitField0_ & 0x00000040) == 0x00000040) && Schema_ != Schema.DefaultInstance)
				{
					Schema_ = Schema.NewBuilder(Schema_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Schema_ = Value;
				}

				BitField0_ |= 0x00000040;
				return this;
			}
			public Builder ClearSchema()
			{
				Schema_ = Schema.DefaultInstance;

				BitField0_ = (BitField0_ & ~0x00000040);
				return this;
			}

			// optional uint64 epoch = 8 [default = 0];
			internal long Epoch_;
			public bool HasEpoch()
			{
				return ((BitField0_ & 0x00000080) == 0x00000080);
			}
			public long Epoch => Epoch_;

            public Builder SetEpoch(long Value)
			{
				BitField0_ |= 0x00000080;
				Epoch_ = Value;

				return this;
			}
			public Builder ClearEpoch()
			{
				BitField0_ = (BitField0_ & ~0x00000080);
				Epoch_ = 0L;

				return this;
			}

			// optional bool user_provided_producer_name = 9 [default = true];
			internal bool UserProvidedProducerName_ = true;
			public bool HasUserProvidedProducerName()
			{
				return ((BitField0_ & 0x00000100) == 0x00000100);
			}
			public bool UserProvidedProducerName => UserProvidedProducerName_;

            public Builder SetUserProvidedProducerName(bool Value)
			{
				BitField0_ |= 0x00000100;
				UserProvidedProducerName_ = Value;

				return this;
			}
			public Builder ClearUserProvidedProducerName()
			{
				BitField0_ = (BitField0_ & ~0x00000100);
				UserProvidedProducerName_ = true;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandProducer)
		}

		static CommandProducer()
		{
			_defaultInstance = new CommandProducer(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandProducer)
	}

}
