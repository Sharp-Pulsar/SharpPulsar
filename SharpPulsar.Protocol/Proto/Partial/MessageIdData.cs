﻿using DotNetty.Common;
using Google.Protobuf;
using SharpPulsar.Util.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Util.Protobuf.ByteBufCodedInputStream;
using static SharpPulsar.Util.Protobuf.ByteBufCodedOutputStream;

namespace SharpPulsar.Protocol.Proto
{
    public partial class MessageIdData: ByteBufGeneratedMessage
    {
        // Use MessageIdData.newBuilder() to construct.
        internal static ThreadLocalPool<MessageIdData> _pool = new ThreadLocalPool<MessageIdData>(handle => new MessageIdData(handle), 1, true);
        internal ThreadLocalPool.Handle _handle;
        private MessageIdData(ThreadLocalPool.Handle handle)
        {
            _handle = handle;
        }


        public void Recycle()
        {
            this.InitFields();
            this.MemoizedIsInitialized = -1;
            _hasBits0 = 0;
            this.MemoizedSerializedSize = -1;
            _handle?.Release(this);
        }

        public MessageIdData(bool NoInit)
        {
        }

        internal static readonly MessageIdData _defaultInstance;
        public static MessageIdData DefaultInstance => _defaultInstance;

        public MessageIdData DefaultInstanceForType => _defaultInstance;


        public void InitFields()
        {
            LedgerId = 0L;
            EntryId = 0L;
            Partition = -1;
            BatchIndex = -1;
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

                if (!HasLedgerId)
                {
                    MemoizedIsInitialized = 0;
                    return false;
                }
                if (!HasEntryId)
                {
                    MemoizedIsInitialized = 0;
                    return false;
                }
                MemoizedIsInitialized = 1;
                return true;
            }
        }

        public int SerializedSize => CalculateSize();

        internal int MemoizedSerializedSize = -1;


        internal const long SerialVersionUID = 0L;

        public static Builder NewBuilder()
        {
            return Builder.Create();
        }
        public Builder NewBuilderForType()
        {
            return NewBuilder();
        }
        public static Builder NewBuilder(MessageIdData Prototype)
        {
            return NewBuilder().MergeFrom(Prototype);
        }
        public Builder ToBuilder()
        {
            return NewBuilder(this);
        }
        public void WriteTo(ByteBufCodedOutputStream output)
        {
            throw new NotImplementedException();
        }

        public sealed class Builder: ByteBufMessageBuilder
        {
            internal static ThreadLocalPool<Builder> _pool = new ThreadLocalPool<Builder>(handle => new Builder(handle), 1, true);
            internal ThreadLocalPool.Handle _handle;
            public Builder(ThreadLocalPool.Handle handle)
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
                LedgerId = 0L;
                _bitField = (_bitField & ~0x00000001);
                EntryId = 0L;
                _bitField = (_bitField & ~0x00000002);
                Partition = -1;
                _bitField = (_bitField & ~0x00000004);
                BatchIndex = -1;
                _bitField = (_bitField & ~0x00000008);
                return this;
            }

            public MessageIdData DefaultInstanceForType => DefaultInstance;

            public MessageIdData Build()
            {
                var result = BuildPartial();
                if (!result.Initialized)
                {
                    throw new NullReferenceException("MessageIdData not initialized");
                }
                return result;
            }


            public MessageIdData BuildPartial()
            {
                var result = MessageIdData._pool.Take();
                int From_bitField = _bitField;
                int To_bitField = 0;
                if (((From_bitField & 0x00000001) == 0x00000001))
                {
                    To_bitField |= 0x00000001;
                }
                result.LedgerId = (ulong)LedgerId;
                if (((From_bitField & 0x00000002) == 0x00000002))
                {
                    To_bitField |= 0x00000002;
                }
                result.EntryId = (ulong)EntryId;
                if (((From_bitField & 0x00000004) == 0x00000004))
                {
                    To_bitField |= 0x00000004;
                }
                result.Partition = Partition;
                if (((From_bitField & 0x00000008) == 0x00000008))
                {
                    To_bitField |= 0x00000008;
                }
                result.BatchIndex = BatchIndex;
                result._hasBits0 = To_bitField;
                return result;
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
                                _bitField |= 0x00000001;
                                LedgerId = input.ReadUInt64();
                                break;
                            }
                        case 16:
                            {
                                _bitField |= 0x00000002;
                                EntryId_ = input.ReadUInt64();
                                break;
                            }
                        case 24:
                            {
                                _bitField |= 0x00000004;
                                Partition = input.ReadInt32();
                                break;
                            }
                        case 32:
                            {
                                _bitField |= 0x00000008;
                                BatchIndex = input.ReadInt32();
                                break;
                            }
                    }
                }
            }
            public Builder MergeFrom(MessageIdData Other)
            {
                if (Other == DefaultInstance)
                {
                    return this;
                }
                if (Other.HasLedgerId)
                {
                    LedgerId = (long)Other.LedgerId;
                }
                if (Other.HasEntryId)
                {
                    EntryId = (long)Other.EntryId;
                }
                if (Other.HasPartition)
                {
                    Partition = Other.Partition;
                }
                if (Other.HasBatchIndex)
                {
                    BatchIndex = Other.BatchIndex;
                }
                return this;
            }

            public bool Initialized
            {
                get
                {
                    if (!HasLedgerId())
                    {

                        return false;
                    }
                    if (!HasEntryId())
                    {

                        return false;
                    }
                    return true;
                }
            }


            private int _bitField;

            // required uint64 ledgerId = 1;
            internal long LedgerId;

            // required uint64 ledgerId = 2;
            internal long EntryId;
            public bool HasLedgerId()
            {
                return ((_bitField & 0x00000001) == 0x00000001);
            }

            public Builder SetLedgerId(long value)
            {
                _bitField |= 0x00000001;
                LedgerId = value;

                return this;
            }
            public Builder ClearLedgerId()
            {
                _bitField = (_bitField & ~0x00000001);
                LedgerId = 0L;

                return this;
            }

            // required uint64 entryId = 2;
            internal long EntryId_;
            public bool HasEntryId()
            {
                return ((_bitField & 0x00000002) == 0x00000002);
            }

            public Builder SetEntryId(long Value)
            {
                _bitField |= 0x00000002;
                EntryId = Value;

                return this;
            }
            public Builder ClearEntryId()
            {
                _bitField = (_bitField & ~0x00000002);
                EntryId = 0L;

                return this;
            }

            // optional int32 partition = 3 [default = -1];
            internal int Partition = -1;
            public bool HasPartition()
            {
                return ((_bitField & 0x00000004) == 0x00000004);
            }

            public Builder SetPartition(int value)
            {
                _bitField |= 0x00000004;
                Partition = value;

                return this;
            }
            public Builder ClearPartition()
            {
                _bitField = (_bitField & ~0x00000004);
                Partition = -1;

                return this;
            }

            // optional int32 batch_index = 4 [default = -1];
            internal int BatchIndex = -1;
            public bool HasBatchIndex()
            {
                return ((_bitField & 0x00000008) == 0x00000008);
            }

            public Builder SetBatchIndex(int value)
            {
                _bitField |= 0x00000008;
                BatchIndex = value;

                return this;
            }
            public Builder ClearBatchIndex()
            {
                _bitField = (_bitField & ~0x00000008);
                BatchIndex = -1;

                return this;
            }

            // @@protoc_insertion_point(builder_scope:pulsar.proto.MessageIdData)
        }

        static MessageIdData()
        {
            _defaultInstance = new MessageIdData(true);
            _defaultInstance.InitFields();
        }

        // @@protoc_insertion_point(class_scope:pulsar.proto.MessageIdData)
    }


}
