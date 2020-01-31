using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;

namespace SharpPulsar.Protocol.Proto
{
	public partial class CommandSubscribe :  ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandSubscribe.newBuilder() to construct.
		internal static ThreadLocalPool<CommandSubscribe> _pool = new ThreadLocalPool<CommandSubscribe>(handle => new CommandSubscribe(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandSubscribe(ThreadLocalPool.Handle handle)
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

		public CommandSubscribe(bool NoInit)
		{
		}

		
		internal static readonly CommandSubscribe _defaultInstance;
		public static CommandSubscribe DefaultInstance
		{
			get
			{
				return _defaultInstance;
			}
		}

		public CommandSubscribe DefaultInstanceForType
		{
			get
			{
				return _defaultInstance;
			}
		}
		public static Types.InitialPosition ValueOf(int Value)
		{
			switch (Value)
			{
				case 0:
					return Types.InitialPosition.Latest;
				case 1:
					return Types.InitialPosition.Earliest;
				default:
					return Types.InitialPosition.Latest;
			}
		}
		public void InitFields()
		{
			Topic = "";
			Subscription = "";
			SubType = Types.SubType.Exclusive;
			ConsumerId = 0L;
			RequestId = 0L;
			ConsumerName = "";
			PriorityLevel = 0;
			Durable = true;
			StartMessageId = MessageIdData.DefaultInstance;
			Metadata.Clear();
			ReadCompacted = false;
			Schema = Schema.DefaultInstance;
			InitialPosition = Types.InitialPosition.Latest;
			ReplicateSubscriptionState = false;
			ForceTopicCreation = true;
			StartMessageRollbackDurationSec = 0L;
			KeySharedMeta = KeySharedMeta.DefaultInstance;
		}
		public KeyValue GetMetadata(int Index)
		{
			return Metadata[Index];
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
				if (!HasSubscription)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasSubType)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasConsumerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasRequestId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (HasStartMessageId)
				{
					if (!StartMessageId.Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
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
				if (HasKeySharedMeta)
				{
					if (!KeySharedMeta.IsInitialized())
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
				Output.WriteBytes(2, ByteString.CopyFromUtf8(Subscription));
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(3, (int)SubType);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(4, (long)ConsumerId);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteUInt64(5, (long)RequestId);
			}
			if (((_hasBits0 & 0x00000020) == 0x00000020))
			{
				Output.WriteBytes(6, ByteString.CopyFromUtf8(ConsumerName));
			}
			if (((_hasBits0 & 0x00000040) == 0x00000040))
			{
				Output.WriteInt32(7, PriorityLevel);
			}
			if (((_hasBits0 & 0x00000080) == 0x00000080))
			{
				Output.WriteBool(8, Durable);
			}
			if (((_hasBits0 & 0x00000100) == 0x00000100))
			{
				Output.WriteMessage(9, StartMessageId);
			}
			for (int I = 0; I < Metadata.Count; I++)
			{
				Output.WriteMessage(10, Metadata[I]);
			}
			if (((_hasBits0 & 0x00000200) == 0x00000200))
			{
				Output.WriteBool(11, ReadCompacted);
			}
			if (((_hasBits0 & 0x00000400) == 0x00000400))
			{
				Output.WriteMessage(12, Schema);
			}
			if (((_hasBits0 & 0x00000800) == 0x00000800))
			{
				Output.WriteEnum(13, (int)InitialPosition);
			}
			if (((_hasBits0 & 0x00001000) == 0x00001000))
			{
				Output.WriteBool(14, ReplicateSubscriptionState);
			}
			if (((_hasBits0 & 0x00002000) == 0x00002000))
			{
				Output.WriteBool(15, ForceTopicCreation);
			}
			if (((_hasBits0 & 0x00004000) == 0x00004000))
			{
				Output.WriteUInt64(16, (long)StartMessageRollbackDurationSec);
			}
			if (((_hasBits0 & 0x00008000) == 0x00008000))
			{
				Output.WriteMessage(17, KeySharedMeta);
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
		public static Builder NewBuilder(CommandSubscribe prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder : ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandSubscribe.newBuilder()
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
				_hasBits0 = (_hasBits0 & ~0x00000001);
				Subscription_ = "";
				_hasBits0 = (_hasBits0 & ~0x00000002);
				SubType_ = Types.SubType.Exclusive;
				_hasBits0 = (_hasBits0 & ~0x00000004);
				ConsumerId_ = 0L;
				_hasBits0 = (_hasBits0 & ~0x00000008);
				RequestId_ = 0L;
				_hasBits0 = (_hasBits0 & ~0x00000010);
				ConsumerName_ = "";
				_hasBits0 = (_hasBits0 & ~0x00000020);
				PriorityLevel_ = 0;
				_hasBits0 = (_hasBits0 & ~0x00000040);
				Durable_ = true;
				_hasBits0 = (_hasBits0 & ~0x00000080);
				StartMessageId_ = MessageIdData.DefaultInstance;
				_hasBits0 = (_hasBits0 & ~0x00000100);
				Metadata_.Clear();
				_hasBits0 = (_hasBits0 & ~0x00000200);
				ReadCompacted_ = false;
				_hasBits0 = (_hasBits0 & ~0x00000400);
				Schema_ = Schema.DefaultInstance;
				_hasBits0 = (_hasBits0 & ~0x00000800);
				InitialPosition_ = Types.InitialPosition.Latest;
				_hasBits0 = (_hasBits0 & ~0x00001000);
				ReplicateSubscriptionState_ = false;
				_hasBits0 = (_hasBits0 & ~0x00002000);
				ForceTopicCreation_ = true;
				_hasBits0 = (_hasBits0 & ~0x00004000);
				StartMessageRollbackDurationSec_ = 0L;
				_hasBits0 = (_hasBits0 & ~0x00008000);
				KeySharedMeta_ = KeySharedMeta.DefaultInstance;
				_hasBits0 = (_hasBits0 & ~0x00010000);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandSubscribe DefaultInstanceForType
			{
				get
				{
					return DefaultInstance;
				}
			}

			public CommandSubscribe Build()
			{
				CommandSubscribe Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException("CommandSubscribe not initialized");
				}
				return Result;
			}

			public CommandSubscribe BuildParsed()
			{
				CommandSubscribe Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException("CommandSubscribe not initialized");
				}
				return Result;
			}

			public CommandSubscribe BuildPartial()
			{
				CommandSubscribe Result = CommandSubscribe._pool.Take();
				int From_hasBits0 = _hasBits0;
				int To_hasBits0 = 0;
				if (((From_hasBits0 & 0x00000001) == 0x00000001))
				{
					To_hasBits0 |= 0x00000001;
				}
				Result.Topic = Topic_.ToString();
				if (((From_hasBits0 & 0x00000002) == 0x00000002))
				{
					To_hasBits0 |= 0x00000002;
				}
				Result.Subscription = Subscription_.ToString();
				if (((From_hasBits0 & 0x00000004) == 0x00000004))
				{
					To_hasBits0 |= 0x00000004;
				}
				Result.SubType = SubType_;
				if (((From_hasBits0 & 0x00000008) == 0x00000008))
				{
					To_hasBits0 |= 0x00000008;
				}
				Result.ConsumerId = (ulong)ConsumerId_;
				if (((From_hasBits0 & 0x00000010) == 0x00000010))
				{
					To_hasBits0 |= 0x00000010;
				}
				Result.RequestId = (ulong)RequestId_;
				if (((From_hasBits0 & 0x00000020) == 0x00000020))
				{
					To_hasBits0 |= 0x00000020;
				}
				Result.ConsumerName = ConsumerName_.ToString();
				if (((From_hasBits0 & 0x00000040) == 0x00000040))
				{
					To_hasBits0 |= 0x00000040;
				}
				Result.PriorityLevel = PriorityLevel_;
				if (((From_hasBits0 & 0x00000080) == 0x00000080))
				{
					To_hasBits0 |= 0x00000080;
				}
				Result.Durable = Durable_;
				if (((From_hasBits0 & 0x00000100) == 0x00000100))
				{
					To_hasBits0 |= 0x00000100;
				}
				Result.StartMessageId = StartMessageId_;
				if (((_hasBits0 & 0x00000200) == 0x00000200))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					_hasBits0 = (_hasBits0 & ~0x00000200);
				}
				Metadata_.ToList().ForEach(Result.Metadata.Add);
				if (((From_hasBits0 & 0x00000400) == 0x00000400))
				{
					To_hasBits0 |= 0x00000200;
				}
				Result.ReadCompacted = ReadCompacted_;
				if (((From_hasBits0 & 0x00000800) == 0x00000800))
				{
					To_hasBits0 |= 0x00000400;
				}
				Result.Schema = Schema_;
				if (((From_hasBits0 & 0x00001000) == 0x00001000))
				{
					To_hasBits0 |= 0x00000800;
				}
				Result.InitialPosition = InitialPosition_;
				if (((From_hasBits0 & 0x00002000) == 0x00002000))
				{
					To_hasBits0 |= 0x00001000;
				}
				Result.ReplicateSubscriptionState = ReplicateSubscriptionState_;
				if (((From_hasBits0 & 0x00004000) == 0x00004000))
				{
					To_hasBits0 |= 0x00002000;
				}
				Result.ForceTopicCreation = ForceTopicCreation_;
				if (((From_hasBits0 & 0x00008000) == 0x00008000))
				{
					To_hasBits0 |= 0x00004000;
				}
				Result.StartMessageRollbackDurationSec = (ulong)StartMessageRollbackDurationSec_;
				if (((From_hasBits0 & 0x00010000) == 0x00010000))
				{
					To_hasBits0 |= 0x00008000;
				}
				Result.KeySharedMeta = KeySharedMeta_;
				Result._hasBits0 = To_hasBits0;
				return Result;
			}

			public Builder MergeFrom(CommandSubscribe Other)
			{
				if (Other == CommandSubscribe.DefaultInstance)
				{
					return this;
				}
				if (Other.HasTopic)
				{
					SetTopic(Other.Topic);
				}
				if (Other.HasSubscription)
				{
					SetSubscription(Other.Subscription);
				}
				if (Other.HasSubType)
				{
					SetSubType(Other.SubType);
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.HasRequestId)
				{
					SetRequestId((long)Other.RequestId);
				}
				if (Other.HasConsumerName)
				{
					SetConsumerName(Other.ConsumerName);
				}
				if (Other.HasPriorityLevel)
				{
					SetPriorityLevel(Other.PriorityLevel);
				}
				if (Other.HasDurable)
				{
					SetDurable(Other.Durable);
				}
				if (Other.HasStartMessageId)
				{
					MergeStartMessageId(Other.StartMessageId);
				}
				if (Other.Metadata.Count > 0)
				{
					if (Metadata_.Count == 0)
					{
						Metadata_ = Other.Metadata;
						_hasBits0 = (_hasBits0 & ~0x00000200);
					}
					else
					{
						EnsureMetadataIsMutable();
						((List<KeyValue>)Metadata_).AddRange(Other.Metadata);
					}

				}
				if (Other.HasReadCompacted)
				{
					SetReadCompacted(Other.ReadCompacted);
				}
				if (Other.HasSchema)
				{
					MergeSchema(Other.Schema);
				}
				if (Other.HasInitialPosition)
				{
					SetInitialPosition(Other.InitialPosition);
				}
				if (Other.HasReplicateSubscriptionState)
				{
					SetReplicateSubscriptionState(Other.ReplicateSubscriptionState);
				}
				if (Other.HasForceTopicCreation)
				{
					SetForceTopicCreation(Other.ForceTopicCreation);
				}
				if (Other.HasStartMessageRollbackDurationSec)
				{
					SetStartMessageRollbackDurationSec((long)Other.StartMessageRollbackDurationSec);
				}
				if (Other.HasKeySharedMeta)
				{
					MergeKeySharedMeta(Other.KeySharedMeta);
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
					if (!HasSubType())
					{

						return false;
					}
					if (!HasConsumerId())
					{

						return false;
					}
					if (!HasRequestId())
					{

						return false;
					}
					if (HasStartMessageId())
					{
						if (!GetStartMessageId().IsInitialized())
						{

							return false;
						}
					}
					for (int I = 0; I < MetadataCount; I++)
					{
						if (!GetMetadata(I).IsInitialized())
						{

							return false;
						}
					}
					if (HasSchema())
					{
						if (!GetSchema().IsInitialized())
						{

							return false;
						}
					}
					if (HasKeySharedMeta())
					{
						if (!GetKeySharedMeta().IsInitialized())
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
					int tag = input.ReadTag();
					switch (tag)
					{
						case 0:

							return this;
						default:
							{
								if (!input.SkipField(tag))
								{

									return this;
								}
								break;
							}
						case 10:
							{
								_hasBits0 |= 0x00000001;
								Topic_ = input.ReadBytes();
								break;
							}
						case 18:
							{
								_hasBits0 |= 0x00000002;
								Subscription_ = input.ReadBytes();
								break;
							}
						case 24:
							{
								int RawValue = input.ReadEnum();
								Types.SubType Value =Enum.GetValues(typeof(Types.SubType)).Cast<Types.SubType>().ToList()[RawValue];
								if (Value != null)
								{
									_hasBits0 |= 0x00000004;
									SubType_ = Value;
								}
								break;
							}
						case 32:
							{
								_hasBits0 |= 0x00000008;
								ConsumerId_ = input.ReadUInt64();
								break;
							}
						case 40:
							{
								_hasBits0 |= 0x00000010;
								RequestId_ = input.ReadUInt64();
								break;
							}
						case 50:
							{
								_hasBits0 |= 0x00000020;
								ConsumerName_ = input.ReadBytes();
								break;
							}
						case 56:
							{
								_hasBits0 |= 0x00000040;
								PriorityLevel_ = input.ReadInt32();
								break;
							}
						case 64:
							{
								_hasBits0 |= 0x00000080;
								Durable_ = input.ReadBool();
								break;
							}
						case 74:
							{
								MessageIdData.Builder subBuilder = MessageIdData.NewBuilder();
								if (HasStartMessageId())
								{
									subBuilder.MergeFrom(GetStartMessageId());
								}
								input.ReadMessage(subBuilder, extensionRegistry);
								SetStartMessageId(subBuilder.BuildPartial());
								subBuilder.Recycle();
								break;
							}
						case 82:
							{
								KeyValue.Builder SubBuilder = KeyValue.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddMetadata(SubBuilder.BuildPartial());
								break;
							}
						case 88:
							{
								_hasBits0 |= 0x00000400;
								ReadCompacted_ = input.ReadBool();
								break;
							}
						case 98:
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
						case 104:
							{
								int RawValue = input.ReadEnum();
								Types.InitialPosition Value = Enum.GetValues(typeof(Types.InitialPosition)).Cast<Types.InitialPosition>().ToList()[RawValue];
								if (Value != null)
								{
									_hasBits0 |= 0x00001000;
									InitialPosition_ = Value;
								}
								break;
							}
						case 112:
							{
								_hasBits0 |= 0x00002000;
								ReplicateSubscriptionState_ = input.ReadBool();
								break;
							}
						case 120:
							{
								_hasBits0 |= 0x00004000;
								ForceTopicCreation_ = input.ReadBool();
								break;
							}
						case 128:
							{
								_hasBits0 |= 0x00008000;
								StartMessageRollbackDurationSec_ = input.ReadUInt64();
								break;
							}
						case 138:
							{
								KeySharedMeta.Builder SubBuilder = KeySharedMeta.NewBuilder();
								if (HasKeySharedMeta())
								{
									SubBuilder.MergeFrom(GetKeySharedMeta());
								}
								input.ReadMessage(SubBuilder, extensionRegistry);
								SetKeySharedMeta(SubBuilder.BuildPartial());
								SubBuilder.Recycle();
								break;
							}
					}
				}
			}

			internal int _hasBits0;

			// required string topic = 1;
			internal object Topic_ = "";
			public bool HasTopic()
			{
				return ((_hasBits0 & 0x00000001) == 0x00000001);
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
					throw new System.NullReferenceException();
				}
				_hasBits0 |= 0x00000001;
				Topic_ = Value;

				return this;
			}
			public Builder ClearTopic()
			{
				_hasBits0 = (_hasBits0 & ~0x00000001);
				Topic_ = DefaultInstance.Topic;

				return this;
			}
			public void SetTopic(ByteString Value)
			{
				_hasBits0 |= 0x00000001;
				Topic_ = Value;

			}

			// required string subscription = 2;
			internal object Subscription_ = "";
			public bool HasSubscription()
			{
				return ((_hasBits0 & 0x00000002) == 0x00000002);
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
			public Builder SetSubscription(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new System.NullReferenceException();
				}
				_hasBits0 |= 0x00000002;
				Subscription_ = Value;

				return this;
			}
			public Builder ClearSubscription()
			{
				_hasBits0 = (_hasBits0 & ~0x00000002);
				Subscription_ = DefaultInstance.Subscription;

				return this;
			}
			public void SetSubscription(ByteString Value)
			{
				_hasBits0 |= 0x00000002;
				Subscription_ = Value;

			}

			// required .pulsar.proto.CommandSubscribe.SubType subType = 3;
			internal CommandSubscribe.Types.SubType SubType_ = CommandSubscribe.Types.SubType.Exclusive;
			public bool HasSubType()
			{
				return ((_hasBits0 & 0x00000004) == 0x00000004);
			}
			public CommandSubscribe.Types.SubType SubType
			{
				get
				{
					return SubType_;
				}
			}
			public Builder SetSubType(Types.SubType Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_hasBits0 |= 0x00000004;
				SubType_ = Value;

				return this;
			}
			public Builder ClearSubType()
			{
				_hasBits0 = (_hasBits0 & ~0x00000004);
				SubType_ = CommandSubscribe.Types.SubType.Exclusive;

				return this;
			}

			// required uint64 consumer_id = 4;
			internal long ConsumerId_;
			public bool HasConsumerId()
			{
				return ((_hasBits0 & 0x00000008) == 0x00000008);
			}
			public long ConsumerId
			{
				get
				{
					return ConsumerId_;
				}
			}
			public Builder SetConsumerId(long Value)
			{
				_hasBits0 |= 0x00000008;
				ConsumerId_ = Value;

				return this;
			}
			public Builder ClearConsumerId()
			{
				_hasBits0 = (_hasBits0 & ~0x00000008);
				ConsumerId_ = 0L;

				return this;
			}

			// required uint64 request_id = 5;
			internal long RequestId_;
			public bool HasRequestId()
			{
				return ((_hasBits0 & 0x00000010) == 0x00000010);
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
				_hasBits0 |= 0x00000010;
				RequestId_ = Value;

				return this;
			}
			public Builder ClearRequestId()
			{
				_hasBits0 = (_hasBits0 & ~0x00000010);
				RequestId_ = 0L;

				return this;
			}

			// optional string consumer_name = 6;
			internal object ConsumerName_ = "";
			public bool HasConsumerName()
			{
				return ((_hasBits0 & 0x00000020) == 0x00000020);
			}
			public string GetConsumerName()
			{
				object Ref = ConsumerName_;
				if (!(Ref is string))
				{
					string S = ((ByteString)Ref).ToStringUtf8();
					ConsumerName_ = S;
					return S;
				}
				else
				{
					return (string)Ref;
				}
			}
			public Builder SetConsumerName(string Value)
			{
				if (string.ReferenceEquals(Value, null))
				{
					throw new System.NullReferenceException();
				}
				_hasBits0 |= 0x00000020;
				ConsumerName_ = Value;

				return this;
			}
			public Builder ClearConsumerName()
			{
				_hasBits0 = (_hasBits0 & ~0x00000020);
				ConsumerName_ = DefaultInstance.ConsumerName;

				return this;
			}
			public void SetConsumerName(ByteString Value)
			{
				_hasBits0 |= 0x00000020;
				ConsumerName_ = Value;

			}

			// optional int32 priority_level = 7;
			internal int PriorityLevel_;
			public bool HasPriorityLevel()
			{
				return ((_hasBits0 & 0x00000040) == 0x00000040);
			}
			public int PriorityLevel
			{
				get
				{
					return PriorityLevel_;
				}
			}
			public Builder SetPriorityLevel(int Value)
			{
				_hasBits0 |= 0x00000040;
				PriorityLevel_ = Value;

				return this;
			}
			public Builder ClearPriorityLevel()
			{
				_hasBits0 = (_hasBits0 & ~0x00000040);
				PriorityLevel_ = 0;

				return this;
			}

			// optional bool durable = 8 [default = true];
			internal bool Durable_ = true;
			public bool HasDurable()
			{
				return ((_hasBits0 & 0x00000080) == 0x00000080);
			}
			public bool Durable
			{
				get
				{
					return Durable_;
				}
			}
			public Builder SetDurable(bool Value)
			{
				_hasBits0 |= 0x00000080;
				Durable_ = Value;

				return this;
			}
			public Builder ClearDurable()
			{
				_hasBits0 = (_hasBits0 & ~0x00000080);
				Durable_ = true;

				return this;
			}

			// optional .pulsar.proto.MessageIdData start_message_id = 9;
			internal MessageIdData StartMessageId_ = MessageIdData.DefaultInstance;
			public bool HasStartMessageId()
			{
				return ((_hasBits0 & 0x00000100) == 0x00000100);
			}
			public MessageIdData GetStartMessageId()
			{
				return StartMessageId_;
			}
			public Builder SetStartMessageId(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				StartMessageId_ = Value;

				_hasBits0 |= 0x00000100;
				return this;
			}
			public Builder SetStartMessageId(MessageIdData.Builder BuilderForValue)
			{
				StartMessageId_ = BuilderForValue.Build();

				_hasBits0 |= 0x00000100;
				return this;
			}
			public Builder MergeStartMessageId(MessageIdData Value)
			{
				if (((_hasBits0 & 0x00000100) == 0x00000100) && StartMessageId_ != MessageIdData.DefaultInstance)
				{
					StartMessageId_ = MessageIdData.NewBuilder(StartMessageId_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					StartMessageId_ = Value;
				}

				_hasBits0 |= 0x00000100;
				return this;
			}
			public Builder ClearStartMessageId()
			{
				StartMessageId_ = MessageIdData.DefaultInstance;

				_hasBits0 = (_hasBits0 & ~0x00000100);
				return this;
			}
			public static Types.SubType ValueOf(int Value)
			{
				switch (Value)
				{
					case 0:
						return Types.SubType.Exclusive;
					case 1:
						return Types.SubType.Shared;
					case 2:
						return Types.SubType.Failover;
					case 3:
						return Types.SubType.KeyShared;
					default:
						return Types.SubType.Exclusive;
				}
			}
			// repeated .pulsar.proto.KeyValue metadata = 10;
			internal IList<KeyValue> Metadata_ = new List<KeyValue>();
			public void EnsureMetadataIsMutable()
			{
				if (!((_hasBits0 & 0x00000200) == 0x00000200))
				{
					Metadata_ = new List<KeyValue>(Metadata_);
					_hasBits0 |= 0x00000200;
				}
			}

			public IList<KeyValue> MetadataList
			{
				get
				{
					return Metadata_;
				}
			}
			public int MetadataCount
			{
				get
				{
					return Metadata_.Count;
				}
			}
			public KeyValue GetMetadata(int Index)
			{
				return Metadata_[Index];
			}
			public Builder SetMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
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
					throw new System.NullReferenceException();
				}
				EnsureMetadataIsMutable();
				Metadata_.Add(Value);

				return this;
			}
			public Builder AddMetadata(int Index, KeyValue Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
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
				Metadata_.Clear();
				_hasBits0 = (_hasBits0 & ~0x00000200);

				return this;
			}
			public Builder RemoveMetadata(int Index)
			{
				EnsureMetadataIsMutable();
				Metadata_.RemoveAt(Index);

				return this;
			}

			// optional bool read_compacted = 11;
			internal bool ReadCompacted_;
			public bool HasReadCompacted()
			{
				return ((_hasBits0 & 0x00000400) == 0x00000400);
			}
			public bool ReadCompacted
			{
				get
				{
					return ReadCompacted_;
				}
			}
			public Builder SetReadCompacted(bool Value)
			{
				_hasBits0 |= 0x00000400;
				ReadCompacted_ = Value;

				return this;
			}
			public Builder ClearReadCompacted()
			{
				_hasBits0 = (_hasBits0 & ~0x00000400);
				ReadCompacted_ = false;

				return this;
			}

			// optional .pulsar.proto.Schema schema = 12;
			internal Schema Schema_ = Schema.DefaultInstance;
			public bool HasSchema()
			{
				return ((_hasBits0 & 0x00000800) == 0x00000800);
			}
			public Schema GetSchema()
			{
				return Schema_;
			}
			public Builder SetSchema(Schema Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				Schema_ = Value;

				_hasBits0 |= 0x00000800;
				return this;
			}
			public Builder SetSchema(Schema.Builder BuilderForValue)
			{
				Schema_ = BuilderForValue.Build();

				_hasBits0 |= 0x00000800;
				return this;
			}
			public Builder MergeSchema(Schema Value)
			{
				if (((_hasBits0 & 0x00000800) == 0x00000800) && Schema_ != Schema.DefaultInstance)
				{
					Schema_ = Schema.NewBuilder(Schema_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					Schema_ = Value;
				}

				_hasBits0 |= 0x00000800;
				return this;
			}
			public Builder ClearSchema()
			{
				Schema_ = Schema.DefaultInstance;

				_hasBits0 = (_hasBits0 & ~0x00000800);
				return this;
			}

			// optional .pulsar.proto.CommandSubscribe.InitialPosition initialPosition = 13 [default = Latest];
			internal CommandSubscribe.Types.InitialPosition InitialPosition_ = CommandSubscribe.Types.InitialPosition.Latest;
			public bool HasInitialPosition()
			{
				return ((_hasBits0 & 0x00001000) == 0x00001000);
			}
			public CommandSubscribe.Types.InitialPosition InitialPosition
			{
				get
				{
					return InitialPosition_;
				}
			}
			public Builder SetInitialPosition(CommandSubscribe.Types.InitialPosition Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				_hasBits0 |= 0x00001000;
				InitialPosition_ = Value;

				return this;
			}
			public Builder ClearInitialPosition()
			{
				_hasBits0 = (_hasBits0 & ~0x00001000);
				InitialPosition_ = CommandSubscribe.Types.InitialPosition.Latest;

				return this;
			}

			// optional bool replicate_subscription_state = 14;
			internal bool ReplicateSubscriptionState_;
			public bool HasReplicateSubscriptionState()
			{
				return ((_hasBits0 & 0x00002000) == 0x00002000);
			}
			public bool ReplicateSubscriptionState
			{
				get
				{
					return ReplicateSubscriptionState_;
				}
			}
			public Builder SetReplicateSubscriptionState(bool Value)
			{
				_hasBits0 |= 0x00002000;
				ReplicateSubscriptionState_ = Value;

				return this;
			}
			public Builder ClearReplicateSubscriptionState()
			{
				_hasBits0 = (_hasBits0 & ~0x00002000);
				ReplicateSubscriptionState_ = false;

				return this;
			}

			// optional bool force_topic_creation = 15 [default = true];
			internal bool ForceTopicCreation_ = true;
			public bool HasForceTopicCreation()
			{
				return ((_hasBits0 & 0x00004000) == 0x00004000);
			}
			public bool ForceTopicCreation
			{
				get
				{
					return ForceTopicCreation_;
				}
			}
			public Builder SetForceTopicCreation(bool Value)
			{
				_hasBits0 |= 0x00004000;
				ForceTopicCreation_ = Value;

				return this;
			}
			public Builder ClearForceTopicCreation()
			{
				_hasBits0 = (_hasBits0 & ~0x00004000);
				ForceTopicCreation_ = true;

				return this;
			}

			// optional uint64 start_message_rollback_duration_sec = 16 [default = 0];
			internal long StartMessageRollbackDurationSec_;
			public bool HasStartMessageRollbackDurationSec()
			{
				return ((_hasBits0 & 0x00008000) == 0x00008000);
			}
			public long StartMessageRollbackDurationSec
			{
				get
				{
					return StartMessageRollbackDurationSec_;
				}
			}
			public Builder SetStartMessageRollbackDurationSec(long Value)
			{
				_hasBits0 |= 0x00008000;
				StartMessageRollbackDurationSec_ = Value;

				return this;
			}
			public Builder ClearStartMessageRollbackDurationSec()
			{
				_hasBits0 = (_hasBits0 & ~0x00008000);
				StartMessageRollbackDurationSec_ = 0L;

				return this;
			}

			// optional .pulsar.proto.KeySharedMeta keySharedMeta = 17;
			internal KeySharedMeta KeySharedMeta_ = KeySharedMeta.DefaultInstance;
			public bool HasKeySharedMeta()
			{
				return ((_hasBits0 & 0x00010000) == 0x00010000);
			}
			public KeySharedMeta GetKeySharedMeta()
			{
				return KeySharedMeta_;
			}
			public Builder SetKeySharedMeta(KeySharedMeta Value)
			{
				if (Value == null)
				{
					throw new System.NullReferenceException();
				}
				KeySharedMeta_ = Value;

				_hasBits0 |= 0x00010000;
				return this;
			}
			public Builder SetKeySharedMeta(KeySharedMeta.Builder BuilderForValue)
			{
				KeySharedMeta_ = BuilderForValue.Build();

				_hasBits0 |= 0x00010000;
				return this;
			}
			public Builder MergeKeySharedMeta(KeySharedMeta Value)
			{
				if (((_hasBits0 & 0x00010000) == 0x00010000) && KeySharedMeta_ != KeySharedMeta.DefaultInstance)
				{
					KeySharedMeta_ = KeySharedMeta.NewBuilder(KeySharedMeta_).MergeFrom(Value).BuildPartial();
				}
				else
				{
					KeySharedMeta_ = Value;
				}

				_hasBits0 |= 0x00010000;
				return this;
			}
			public Builder ClearKeySharedMeta()
			{
				KeySharedMeta_ = KeySharedMeta.DefaultInstance;

				_hasBits0 = (_hasBits0 & ~0x00010000);
				return this;
			}

		}

		static CommandSubscribe()
		{
			_defaultInstance = new CommandSubscribe(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandSubscribe)
	}

}
