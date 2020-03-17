﻿using System.Buffers;
using SharpPulsar.Akka;
using SharpPulsar.Akka.Network;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Extension
{
    public static class MessageExtension
    {
        public static uint GetMetadataSize(this ReadOnlySequence<byte> data)
            => data.ReadUInt32(Constants.MetadataSizeOffset, true);

        public static MessageMetadata ExtractMetadata(this ReadOnlySequence<byte> data, uint metadataSize)
            => Serializer.Deserialize<MessageMetadata>(data.Slice(Constants.MetadataOffset, metadataSize));

        public static ReadOnlySequence<byte> ExtractData(this ReadOnlySequence<byte> data, uint metadataSize)
            => data.Slice(Constants.MetadataOffset + metadataSize);

        public static bool IsValid(this ReadOnlySequence<byte> data)
            => StartsWithMagicNumber(data) && HasValidCheckSum(data);

        private static bool StartsWithMagicNumber(ReadOnlySequence<byte> input)
            => input.StartsWith(Constants.MagicNumber);

        private static bool HasValidCheckSum(ReadOnlySequence<byte> input)
            => input.ReadUInt32(Constants.MagicNumber.Length, true) == DotCrc32C.Calculate(input.Slice(Constants.MetadataSizeOffset));
    }
}