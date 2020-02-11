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
	public sealed partial class CommandAck : ByteBufCodedOutputStream.ByteBufGeneratedMessage
	{
		// Use CommandAck.newBuilder() to construct.
		internal static ThreadLocalPool<CommandAck> _pool = new ThreadLocalPool<CommandAck>(handle => new CommandAck(handle), 1, true);

		internal ThreadLocalPool.Handle _handle;
		private CommandAck(ThreadLocalPool.Handle handle)
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

		public CommandAck(bool NoInit)
		{
		}

		internal static readonly CommandAck _defaultInstance;
		public static CommandAck DefaultInstance => _defaultInstance;

        public CommandAck DefaultInstanceForType => _defaultInstance;

        public Types.AckType GetAckType()
		{
			return AckType;
		}

		public MessageIdData GetMessageId(int Index)
		{
			return MessageId[Index];
		}
		
		public KeyLongValue GetProperties(int Index)
		{
			return Properties[Index];
		}
		
		public void InitFields()
		{
			ConsumerId = 0L;
			AckType = CommandAck.Types.AckType.Individual;
			MessageId.Clear();
			ValidationError = CommandAck.Types.ValidationError.UncompressedSizeCorruption;
			Properties.Clear();
			TxnidLeastBits = 0L;
			TxnidMostBits = 0L;
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

				if (!HasConsumerId)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				if (!HasAckType)
				{
					MemoizedIsInitialized = 0;
					return false;
				}
				for (int I = 0; I < MessageId.Count; I++)
				{
					if (!GetMessageId(I).Initialized)
					{
						MemoizedIsInitialized = 0;
						return false;
					}
				}
				for (int I = 0; I < Properties.Count; I++)
				{
					if (!GetProperties(I).HasKey)
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
			var _= SerializedSize;
			if (((_hasBits0 & 0x00000001) == 0x00000001))
			{
				Output.WriteUInt64(1, (long)ConsumerId);
			}
			if (((_hasBits0 & 0x00000002) == 0x00000002))
			{
				Output.WriteEnum(2, (int)AckType);
			}
			for (int I = 0; I < MessageId.Count; I++)
			{
				Output.WriteMessage(3, MessageId[I]);
			}
			if (((_hasBits0 & 0x00000004) == 0x00000004))
			{
				Output.WriteEnum(4, (int)ValidationError);
			}
			for (int I = 0; I < Properties.Count; I++)
			{
				Output.WriteMessage(5, Properties[I]);
			}
			if (((_hasBits0 & 0x00000008) == 0x00000008))
			{
				Output.WriteUInt64(6, (long)TxnidLeastBits);
			}
			if (((_hasBits0 & 0x00000010) == 0x00000010))
			{
				Output.WriteUInt64(7, (long)TxnidMostBits);
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
		public static Builder NewBuilder(CommandAck Prototype)
		{
			return NewBuilder().MergeFrom(Prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}

		public sealed class Builder :  ByteBufMessageBuilder
		{
			// Construct using org.apache.pulsar.common.api.proto.CommandAck.newBuilder()
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
				ConsumerId_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000001);
				AckType_ = CommandAck.Types.AckType.Individual;
				BitField0_ = (BitField0_ & ~0x00000002);
				MessageId_ = new List<MessageIdData>();
				BitField0_ = (BitField0_ & ~0x00000004);
				ValidationError_ = CommandAck.Types.ValidationError.UncompressedSizeCorruption;
				BitField0_ = (BitField0_ & ~0x00000008);
				Properties_ = new List<KeyLongValue>();
				BitField0_ = (BitField0_ & ~0x00000010);
				TxnidLeastBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000020);
				TxnidMostBits_ = 0L;
				BitField0_ = (BitField0_ & ~0x00000040);
				return this;
			}

			public Builder Clone()
			{
				return Create().MergeFrom(BuildPartial());
			}

			public CommandAck DefaultInstanceForType => CommandAck.DefaultInstance;

            public CommandAck Build()
			{
				CommandAck Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			
			public CommandAck BuildParsed()
			{
				CommandAck Result = BuildPartial();
				if (!Result.Initialized)
				{
					throw new NullReferenceException($"{Result.GetType().Name} not initialized");
				}
				return Result;
			}

			public CommandAck BuildPartial()
			{
				CommandAck Result = CommandAck._pool.Take();
				int FromBitField0_ = BitField0_;
				int ToBitField0_ = 0;
				if (((FromBitField0_ & 0x00000001) == 0x00000001))
				{
					ToBitField0_ |= 0x00000001;
				}
				Result.ConsumerId = (ulong)ConsumerId_;
				if (((FromBitField0_ & 0x00000002) == 0x00000002))
				{
					ToBitField0_ |= 0x00000002;
				}
				Result.AckType = AckType_;
				if (((BitField0_ & 0x00000004) == 0x00000004))
				{
					MessageId_ =  new List<MessageIdData>(MessageId_);
					BitField0_ = (BitField0_ & ~0x00000004);
				}
				MessageId_.ToList().ForEach(Result.MessageId.Add);
				if (((FromBitField0_ & 0x00000008) == 0x00000008))
				{
					ToBitField0_ |= 0x00000004;
				}
				Result.ValidationError = ValidationError_;
				if (((BitField0_ & 0x00000010) == 0x00000010))
				{
					Properties_ = new List<KeyLongValue>(Properties_);
					BitField0_ = (BitField0_ & ~0x00000010);
				}
				Result.Properties.AddRange(Properties_);
				if (((FromBitField0_ & 0x00000020) == 0x00000020))
				{
					ToBitField0_ |= 0x00000008;
				}
				Result.TxnidLeastBits = (ulong)TxnidLeastBits_;
				if (((FromBitField0_ & 0x00000040) == 0x00000040))
				{
					ToBitField0_ |= 0x00000010;
				}
				Result.TxnidMostBits = (ulong)TxnidMostBits_;
				Result._hasBits0 = ToBitField0_;
				return Result;
			}

			public Builder MergeFrom(CommandAck Other)
			{
				if (Other == CommandAck.DefaultInstance)
				{
					return this;
				}
				if (Other.HasConsumerId)
				{
					SetConsumerId((long)Other.ConsumerId);
				}
				if (Other.HasAckType)
				{
					SetAckType(Other.GetAckType());
				}
				if (Other.MessageId.Count > 0)
				{
					if (MessageId_.Count == 0)
					{
						MessageId_ = Other.MessageId;
						BitField0_ = (BitField0_ & ~0x00000004);
					}
					else
					{
						EnsureMessageIdIsMutable();
						((List<MessageIdData>)MessageId_).AddRange(Other.MessageId);
					}

				}
				if (Other.HasValidationError)
				{
					SetValidationError(Other.ValidationError);
				}
				if (Other.Properties.Count > 0)
				{
					if (Properties_.Count == 0)
					{
						Properties_ = Other.Properties;
						BitField0_ = (BitField0_ & ~0x00000010);
					}
					else
					{
						EnsurePropertiesIsMutable();
						((List<KeyLongValue>)Properties_).AddRange(Other.Properties);
					}

				}
				if (Other.HasTxnidLeastBits)
				{
					SetTxnidLeastBits((long)Other.TxnidLeastBits);
				}
				if (Other.HasTxnidMostBits)
				{
					SetTxnidMostBits((long)Other.TxnidMostBits);
				}
				return this;
			}

			public bool Initialized
			{
				get
				{
					if (!HasConsumerId())
					{

						return false;
					}
					if (!HasAckType())
					{

						return false;
					}
					for (int I = 0; I < MessageIdCount; I++)
					{
						if (!GetMessageId(I).Initialized)
						{

							return false;
						}
					}
					for (int I = 0; I < PropertiesCount; I++)
					{
						if (!GetProperties(I).HasKey)
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
								ConsumerId_ = input.ReadUInt64();
								break;
							}
						case 16:
							{
								int RawValue = input.ReadEnum();
                                Types.AckType Value = Enum.GetValues(typeof(Types.AckType)).Cast<Types.AckType>().ToList()[RawValue];
								if (Value != null)
								{
									BitField0_ |= 0x00000002;
									AckType_ = Value;
								}
								break;
							}
						case 26:
							{
								MessageIdData.Builder SubBuilder = MessageIdData.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddMessageId(SubBuilder.BuildPartial());
								break;
							}
						case 32:
							{
								int RawValue = input.ReadEnum();
                                Types.ValidationError Value = Enum.GetValues(typeof(Types.ValidationError)).Cast<Types.ValidationError>().ToList()[RawValue]; ;
								if (Value != null)
								{
									BitField0_ |= 0x00000008;
									ValidationError_ = Value;
								}
								break;
							}
						case 42:
							{
								KeyLongValue.Builder SubBuilder = KeyLongValue.NewBuilder();
								input.ReadMessage(SubBuilder, extensionRegistry);
								AddProperties(SubBuilder.BuildPartial());
								break;
							}
						case 48:
							{
								BitField0_ |= 0x00000020;
								TxnidLeastBits_ = input.ReadUInt64();
								break;
							}
						case 56:
							{
								BitField0_ |= 0x00000040;
								TxnidMostBits_ = input.ReadUInt64();
								break;
							}
					}
				}
			}

			internal int BitField0_;

			// required uint64 consumer_id = 1;
			internal long ConsumerId_;
			public bool HasConsumerId()
			{
				return ((BitField0_ & 0x00000001) == 0x00000001);
			}
			public long ConsumerId => ConsumerId_;

            public Builder SetConsumerId(long Value)
			{
				BitField0_ |= 0x00000001;
				ConsumerId_ = Value;

				return this;
			}
			public Builder ClearConsumerId()
			{
				BitField0_ = (BitField0_ & ~0x00000001);
				ConsumerId_ = 0L;

				return this;
			}

			// required .pulsar.proto.CommandAck.AckType ack_type = 2;
			internal Types.AckType AckType_ = CommandAck.Types.AckType.Individual;
			public bool HasAckType()
			{
				return ((BitField0_ & 0x00000002) == 0x00000002);
			}
			public Types.AckType AckType => AckType_;

            public Builder SetAckType(Types.AckType Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				BitField0_ |= 0x00000002;
				AckType_ = Value;

				return this;
			}
			public Builder ClearAckType()
			{
				BitField0_ = (BitField0_ & ~0x00000002);
				AckType_ = CommandAck.Types.AckType.Individual;

				return this;
			}

			// repeated .pulsar.proto.MessageIdData message_id = 3;
			internal IList<MessageIdData> MessageId_ = new List<MessageIdData>();
			public void EnsureMessageIdIsMutable()
			{
				if (!((BitField0_ & 0x00000004) == 0x00000004))
				{
					MessageId_ = new List<MessageIdData>(MessageId_);
					BitField0_ |= 0x00000004;
				}
			}

			public IList<MessageIdData> MessageIdList => new List<MessageIdData>(MessageId_);

            public int MessageIdCount => MessageId_.Count;

            public MessageIdData GetMessageId(int Index)
			{
				return MessageId_[Index];
			}
			public Builder SetMessageId(int Index, MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdIsMutable();
				MessageId_[Index] = Value;

				return this;
			}
			public Builder SetMessageId(int Index, MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdIsMutable();
				MessageId_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddMessageId(MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdIsMutable();
				MessageId_.Add(Value);

				return this;
			}
			public Builder AddMessageId(int Index, MessageIdData Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsureMessageIdIsMutable();
				MessageId_.Insert(Index, Value);

				return this;
			}
			public Builder AddMessageId(MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdIsMutable();
				MessageId_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddMessageId(int Index, MessageIdData.Builder BuilderForValue)
			{
				EnsureMessageIdIsMutable();
				MessageId_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllMessageId(IEnumerable<MessageIdData> Values)
			{
				EnsureMessageIdIsMutable();
				Values.ToList().ForEach(MessageId_.Add);

				return this;
			}
			public Builder ClearMessageId()
			{
				MessageId_ = new List<MessageIdData>();
				BitField0_ = (BitField0_ & ~0x00000004);

				return this;
			}
			public Builder RemoveMessageId(int Index)
			{
				EnsureMessageIdIsMutable();
				MessageId_.RemoveAt(Index);

				return this;
			}

			// optional .pulsar.proto.CommandAck.ValidationError validation_error = 4;
			internal Types.ValidationError ValidationError_ = CommandAck.Types.ValidationError.UncompressedSizeCorruption;
			public bool HasValidationError()
			{
				return ((BitField0_ & 0x00000008) == 0x00000008);
			}
			public Types.ValidationError ValidationError => ValidationError_;

            public Builder SetValidationError(Types.ValidationError? value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                else
                {
                    BitField0_ |= 0x00000008;
                    ValidationError_ = (Types.ValidationError) value;

                    return this;
				}
			}
			public Builder ClearValidationError()
			{
				BitField0_ = (BitField0_ & ~0x00000008);
				ValidationError_ = CommandAck.Types.ValidationError.UncompressedSizeCorruption;

				return this;
			}

			// repeated .pulsar.proto.KeyLongValue properties = 5;
			internal IList<KeyLongValue> Properties_ = new List<KeyLongValue>();
			public void EnsurePropertiesIsMutable()
			{
				if (!((BitField0_ & 0x00000010) == 0x00000010))
				{
					Properties_ = new List<KeyLongValue>(Properties_);
					BitField0_ |= 0x00000010;
				}
			}

			public IList<KeyLongValue> PropertiesList => new List<KeyLongValue>(Properties_);

            public int PropertiesCount => Properties_.Count;

            public KeyLongValue GetProperties(int Index)
			{
				return Properties_[Index];
			}
			public Builder SetProperties(int Index, KeyLongValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				Properties_[Index] = Value;

				return this;
			}
			public Builder SetProperties(int Index, KeyLongValue.Builder BuilderForValue)
			{
				EnsurePropertiesIsMutable();
				Properties_[Index] = BuilderForValue.Build();

				return this;
			}
			public Builder AddProperties(KeyLongValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				Properties_.Add(Value);

				return this;
			}
			public Builder AddProperties(int Index, KeyLongValue Value)
			{
				if (Value == null)
				{
					throw new NullReferenceException();
				}
				EnsurePropertiesIsMutable();
				Properties_.Insert(Index, Value);

				return this;
			}
			public Builder AddProperties(KeyLongValue.Builder BuilderForValue)
			{
				EnsurePropertiesIsMutable();
				Properties_.Add(BuilderForValue.Build());

				return this;
			}
			public Builder AddProperties(int Index, KeyLongValue.Builder BuilderForValue)
			{
				EnsurePropertiesIsMutable();
				Properties_.Insert(Index, BuilderForValue.Build());

				return this;
			}
			public Builder AddAllProperties(IEnumerable<KeyLongValue> Values)
			{
				EnsurePropertiesIsMutable();
				Values.ToList().ForEach(Properties_.Add);

				return this;
			}
			public Builder ClearProperties()
			{
				Properties_ = new List<KeyLongValue>();
				BitField0_ = (BitField0_ & ~0x00000010);

				return this;
			}
			public Builder RemoveProperties(int Index)
			{
				EnsurePropertiesIsMutable();
				Properties_.RemoveAt(Index);

				return this;
			}

			// optional uint64 txnid_least_bits = 6 [default = 0];
			internal long TxnidLeastBits_;
			public bool HasTxnidLeastBits()
			{
				return ((BitField0_ & 0x00000020) == 0x00000020);
			}
			public long TxnidLeastBits => TxnidLeastBits_;

            public Builder SetTxnidLeastBits(long Value)
			{
				BitField0_ |= 0x00000020;
				TxnidLeastBits_ = Value;

				return this;
			}
			public Builder ClearTxnidLeastBits()
			{
				BitField0_ = (BitField0_ & ~0x00000020);
				TxnidLeastBits_ = 0L;

				return this;
			}

			// optional uint64 txnid_most_bits = 7 [default = 0];
			internal long TxnidMostBits_;
			public bool HasTxnidMostBits()
			{
				return ((BitField0_ & 0x00000040) == 0x00000040);
			}
			public long TxnidMostBits => TxnidMostBits_;

            public Builder SetTxnidMostBits(long Value)
			{
				BitField0_ |= 0x00000040;
				TxnidMostBits_ = Value;

				return this;
			}
			public Builder ClearTxnidMostBits()
			{
				BitField0_ = (BitField0_ & ~0x00000040);
				TxnidMostBits_ = 0L;

				return this;
			}

			// @@protoc_insertion_point(builder_scope:pulsar.proto.CommandAck)
		}

		static CommandAck()
		{
			_defaultInstance = new CommandAck(true);
			_defaultInstance.InitFields();
		}

		// @@protoc_insertion_point(class_scope:pulsar.proto.CommandAck)
	}

}
