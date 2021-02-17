using System;
using System.Buffers;
using System.IO;
using Microsoft.IO;
using ProtoBuf;
using SharpPulsar.Common;
using SharpPulsar.Extension;
using SharpPulsar.Helpers;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Helpers
{
    public static class Serializer
    {
        public static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager();
        
        public static ReadOnlySequence<byte> Serialize(BaseCommand command)
        {
            var stream = MemoryManager.GetStream();
            var writer = new BinaryWriter(stream);
            // write fake totalLength
            for (var i = 0; i < 4; i++)
                stream.WriteByte(0);

            // write commandPayload
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, command, PrefixStyle.Fixed32BigEndian);
            var frameSize = (int)stream.Length;

            var totalSize = frameSize - 4;

            //write total size and command size
            stream.Seek(0L, SeekOrigin.Begin);
            writer.Write(totalSize.IntToBigEndian());
            stream.Seek(0L, SeekOrigin.Begin);
            return new ReadOnlySequence<byte>(stream.ToArray());
        }
        public static byte[] Serialize(BaseCommand command, MessageMetadata metadata, ReadOnlySequence<byte> payload) 
        {
            var stream = MemoryManager.GetStream();
            var writer = new BinaryWriter(stream);
            // write fake totalLength
            for (var i = 0; i < 4; i++)
                stream.WriteByte(0);

            // write commandPayload
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, command, PrefixStyle.Fixed32BigEndian);

            var stream1Size = (int)stream.Length;

            // write magic number 0x0e01 0x0e, 0x01
            stream.WriteByte(14);
            stream.WriteByte(1);

            for (var i = 0; i < 4; i++)
                stream.WriteByte(0);
            // write metadata
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, metadata, PrefixStyle.Fixed32BigEndian);

            var stream2Size = (int)stream.Length;
            var totalMetadataSize = stream2Size - stream1Size - 6;

            // write payload
            stream.Write(payload.ToArray(), 0, (int)payload.Length);

            var frameSize = (int)stream.Length;
            var totalSize = frameSize - 4;
            var payloadSize = frameSize - stream2Size;

            var crcStart = stream1Size + 2;
            var crcPayloadStart = crcStart + 4;

            //write CRC
            stream.Seek(crcPayloadStart, SeekOrigin.Begin);
            var crc = (int)CRC32C.Get(0u, stream, totalMetadataSize + payloadSize);
            stream.Seek(crcStart, SeekOrigin.Begin);
            writer.Write(crc.IntToBigEndian());

            //write total size and command size
            stream.Seek(0L, SeekOrigin.Begin);
            writer.Write(totalSize.IntToBigEndian());

            stream.Seek(0L, SeekOrigin.Begin);
            return stream.ToArray();
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
    }
}
