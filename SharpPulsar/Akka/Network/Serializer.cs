using System;
using System.Buffers;
using System.IO;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.Network
{
    public static class Serializer
    {
        public static BaseCommand Deserialize(ReadOnlySequence<byte> sequence)
        {
            using var ms = new MemoryStream(sequence.ToArray());
            var o = ProtoBuf.Serializer.Deserialize<BaseCommand>(ms);
            return o;
        }

        public static ReadOnlySequence<byte> Serialize(BaseCommand command)
        {
            var commandBytes = GetBytes(command);
            var commandSizeBytes = ToBigEndianBytes((uint)commandBytes.Length);
            var totalSizeBytes = ToBigEndianBytes((uint)commandBytes.Length + 4);

            return new SequenceBuilder<byte>()
                .Append(totalSizeBytes)
                .Append(commandSizeBytes)
                .Append(commandBytes)
                .Build();
        }
        public static ReadOnlySequence<byte> Serialize(CommandConnect command)
        {
            var commandBytes = GetBytes(command);
            var commandSizeBytes = ToBigEndianBytes((uint)commandBytes.Length);
            var totalSizeBytes = ToBigEndianBytes((uint)commandBytes.Length + 4);

            return new SequenceBuilder<byte>()
                .Append(totalSizeBytes)
                .Append(commandSizeBytes)
                .Append(commandBytes)
                .Build();
        }
        public static ReadOnlySequence<byte> Serialize(BaseCommand command, MessageMetadata metadata, ReadOnlySequence<byte> payload)
        {
            var commandBytes = GetBytes(command);
            var commandSizeBytes = ToBigEndianBytes((uint)commandBytes.Length);

            var metadataBytes = GetBytes(metadata);
            var metadataSizeBytes = ToBigEndianBytes((uint)metadataBytes.Length);

            var sb = new SequenceBuilder<byte>().Append(metadataSizeBytes).Append(metadataBytes).Append(payload);
            var checksum = DotCrc32C.Calculate(sb.Build());

            return sb.Prepend(ToBigEndianBytes(checksum))
                .Prepend(new byte[] { 0x0e, 0x01 })
                .Prepend(commandBytes)
                .Prepend(commandSizeBytes)
                .Prepend(ToBigEndianBytes((uint)sb.Length))
                .Build();
        }

        public static byte[] ToBigEndianBytes(uint integer)
        {
            var union = new UIntUnion(integer);
            if (BitConverter.IsLittleEndian)
                return new[] { union.B3, union.B2, union.B1, union.B0 };
            else
                return new[] { union.B0, union.B1, union.B2, union.B3 };
        }
        private static byte[] GetBytes<T>(T item)
        {
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, item);
            return ms.ToArray();
        }
    }
}
