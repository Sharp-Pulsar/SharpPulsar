using System;
using System.Buffers;
using System.IO;
using System.Net;
using ProtoBuf;
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Protocol.Extension
{
    public static class ProtoLizer
    {
        public static byte[] ToByteArray<T>(this T obj)
        {
            using var memStream = new MemoryStream();
            Serializer.Serialize(memStream, obj); 
            return memStream.ToArray();
        }
        public static int ByteLength<T>(this T obj)
        {
            using var memStream = new MemoryStream();
            Serializer.Serialize(memStream, obj);
            return memStream.ToArray().Length;
        }
        public static T FromByteArray<T>(this byte[] protoBytes)
        {
            using var ms = new MemoryStream();
            ms.Write(protoBytes, 0, protoBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(ms);
        }

        public static (MessageMetadata metadata, byte[] payload, BaseCommand command, long consumed) TryParse(ReadOnlySequence<byte> buffer)
        {
            var array = buffer.ToArray();
            if (array.Length >= 8)
            {
                using var stream = new MemoryStream(array);
                using var reader = new BinaryReader(stream);
                var totalength = Int32FromBigEndian(reader.ReadInt32());
                var frameLength = totalength + 4;
                if (array.Length >= frameLength)
                {
                    var readerIndex = reader.ReadInt16();

                     if(Int16FromBigEndian(reader.ReadInt16()) != MagicNumber)
                         throw  new ArgumentException("Invalid magicNumber");
                     var messageCheckSum = Int32FromBigEndian(reader.ReadInt32());
                     var metadataPointer = stream.Position;
                     var metadata = Serializer.DeserializeWithLengthPrefix<MessageMetadata>(stream, PrefixStyle.Fixed32BigEndian);
                     var payloadPointer = stream.Position;
                     var metadataLength = (int)(payloadPointer - metadataPointer);
                    var payloadLength = frameLength - ((int)payloadPointer);
                    var payload = reader.ReadBytes(payloadLength);
                    stream.Seek(metadataPointer, SeekOrigin.Begin);
                    var checkSum = (int)CRC32C.Get(0u, stream, metadataLength + payloadLength);
                    if(checkSum != messageCheckSum)
                        throw new ArgumentException("Invalid checksum");
                    var command =
                        Serializer.DeserializeWithLengthPrefix<BaseCommand>(stream, PrefixStyle.Fixed32BigEndian);
                    var consumed = buffer.GetPosition(frameLength);
                    return (metadata, payload, command, consumed.ByteLength());
                }
            }
            throw new InvalidDataException("Incomplete Command");
        }
        public static short MagicNumber => 0x0e01;
        public static int Int32ToBigEndian(int num) => IPAddress.HostToNetworkOrder(num);

        public static int Int32FromBigEndian(int num) => IPAddress.NetworkToHostOrder(num);

        private static int Int16FromBigEndian(short num) => IPAddress.NetworkToHostOrder(num);
    }
}
