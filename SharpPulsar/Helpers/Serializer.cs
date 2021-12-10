using System;
using System.Buffers;
using System.IO;
using Microsoft.IO;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Helpers
{
    public static class Serializer
    {
        public static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager();
        
        public static T Deserialize<T>(ReadOnlySequence<byte> sequence) => ProtoBuf.Serializer.Deserialize<T>(sequence);

        public static ReadOnlySequence<byte> Serialize(BaseCommand command)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD]
            var commandBytes = Serialize<BaseCommand>(command);
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
            // Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            var commandBytes = Serialize<BaseCommand>(command);
            var commandSizeBytes = ToBigEndianBytes((uint)commandBytes.Length);

            var metadataBytes = Serialize(metadata);
            var metadataSizeBytes = ToBigEndianBytes((uint)metadataBytes.Length);

            var sb = new SequenceBuilder<byte>().Append(metadataSizeBytes).Append(metadataBytes).Append(payload);
            var checksum = CRC32C.Calculate(sb.Build());

            return sb.Prepend(ToBigEndianBytes(checksum))
                .Prepend(Constants.MagicNumber)
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
        public static byte[] GetBytes<T>(T item)
        {
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, item);
            return ms.ToArray();
        }
        private static byte[] Serialize<T>(T item)
        {
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, item);
            return ms.ToArray();
        }
    }
}
