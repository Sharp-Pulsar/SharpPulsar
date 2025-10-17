using System.Buffers;
using Microsoft.IO;

using ProtoBuf;
using SharpPulsar.Shared;
using static SharpPulsar.Protocol.Schema.Commands;
using SharpPulsar.Shared.Buf;
using SharpPulsar.Protocol.Extension;
using System.Buffers.Text;
using System.Text;
using Google.Protobuf;
using System.IO;
using Pulsar.Proto;

namespace SharpPulsar.Common.Helpers
{
    internal static class Serializer
    {
        public static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager();
        
        public static T Deserialize<T>(ByteBuf sequence) => ProtoBuf.Serializer.Deserialize<T>(sequence.AsSpan());

        public static ByteBuf Serialize(BaseCommand command)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD]
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
            return new ByteBuf(stream.ToArray());
        }

        public static ByteBuf Serialize(BaseCommand command, ChecksumType checksumType, MessageMetadata metadata, byte[] payload)
        {
            var payld = new ByteBuf(payload);
            // Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
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
            stream.Write(payld.Data.ToArray(), 0, (int)payload.Length);

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
            return new ByteBuf(stream.ToArray());
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
        public static ByteBuf SerializeMetadataAndPayload(ChecksumType checksumType, MessageMetadata msgMetadata, ByteBuf payload)
        {
            // / Wire format
            // [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            int msgMetadataSize = msgMetadata.CalculateSize();
            int payloadSize = payload.ReadableBytes;
            int magicAndChecksumLength = ChecksumType.Crc32c.Equals(checksumType) ? (2 + 4) : 0;
            bool includeChecksum = magicAndChecksumLength > 0;
            int headerContentSize = magicAndChecksumLength + 4 + msgMetadataSize; // magicLength +
                                                                                  // checksumSize + msgMetadataLength +
                                                                                  // msgMetadataSize
            int checksumReaderIndex = -1;
            int totalSize = headerContentSize + payloadSize;

            ByteBuf metadataAndPayload = PulsarByteBufAllocator.DEFAULT.buffer(totalSize, totalSize);

            // Create checksum placeholder
            if (includeChecksum)
            {
                metadataAndPayload.WriteShort(MagicCrc32c);
                checksumReaderIndex = metadataAndPayload.WriterIndex;
                metadataAndPayload.WriterIndex = metadataAndPayload.WriterIndex + ChecksumSize; // skip 4 bytes of checksum
            }

            // Write metadata
            metadataAndPayload.WriteInt(msgMetadataSize);
            msgMetadata.WriteTo(metadataAndPayload);

            // write checksum at created checksum-placeholder
            if (includeChecksum)
            {
                metadataAndPayload.MarkReaderIndex();
                metadataAndPayload.ReaderIndex = checksumReaderIndex + _checksumSize;
                int metadataChecksum = ComputeChecksum(metadataAndPayload);
                int computedChecksum = ResumeChecksum(metadataChecksum, payload);
                // set computed checksum
                metadataAndPayload.SetInt(checksumReaderIndex, computedChecksum);
                metadataAndPayload.ResetReaderIndex();
            }
            metadataAndPayload.WriteBytes(payload);

            return metadataAndPayload;
        }

        public static long InitBatchMessageMetadata(MessageMetadata messageMetadata, MessageMetadata builder)
        {
            messageMetadata.PublishTime = builder.PublishTime;
            messageMetadata.ProducerName = builder.ProducerName;
            messageMetadata.SequenceId = builder.SequenceId;

            // Attach the key to the message metadata.
            if (builder.HasPartitionKey)
            {
                messageMetadata.PartitionKey = builder.PartitionKey;
                messageMetadata.PartitionKeyB64Encoded = builder.PartitionKeyB64Encoded;
            }
            if (builder.HasOrderingKey)
            {
                messageMetadata.OrderingKey = builder.OrderingKey;
            }
            if (builder.HasReplicatedFrom)
            {
                messageMetadata.ReplicatedFrom = builder.ReplicatedFrom;
            }
            if (builder.ReplicateTo.Count > 0)
            {
                for (int i = 0; i < builder.ReplicateTo.Count; i++)
                {
                    messageMetadata.ReplicateTo.AddRange(builder.ReplicateTo.ReplicateToAt(i));
                }
            }
            if (builder.HasSchemaVersion)
            {
                messageMetadata.SchemaVersion = builder.SchemaVersion;
            }
            if (builder.HasSchemaId)
            {
                messageMetadata.SchemaId = builder.SchemaId;
            }

            return (long)builder.SequenceId;
        }

        public static ByteBuf SerializeSingleMessageInBatchWithPayload(SingleMessageMetadata singleMessageMetadata, ByteBuf payload, ByteBuf batchBuffer)
        {
            singleMessageMetadata.PayloadSize = payload.ReadableBytes;

            // serialize meta-data size, meta-data and payload for single message in batch
            batchBuffer.WriteInt(GetBytes(singleMessageMetadata).Length);
            ToByteBuff(singleMessageMetadata, batchBuffer);
            return batchBuffer;
        }
        private static void ToByteBuff(IMessage message, ByteBuf byteBuf)
        {
            byte[] messageBytes;
            using(var stream = new MemoryStream())
            {
                message.WriteTo(stream);
                messageBytes = stream.ToArray();
            }
            byteBuf.WriteBytes(messageBytes);   
        }
        public static ByteBuf SerializeSingleMessageInBatchWithPayload(MessageMetadata msg, ByteBuf payload, ByteBuf batchBuffer)
        {
            // build single message meta-data
            SingleMessageMetadata smm = new SingleMessageMetadata();
            smm.cle.Clear();

            if (msg.hasPartitionKey())
            {
                smm.setPartitionKey(msg.getPartitionKey());
                smm.setPartitionKeyB64Encoded(msg.isPartitionKeyB64Encoded());
            }
            if (msg.hasOrderingKey())
            {
                smm.setOrderingKey(msg.getOrderingKey());
            }
            for (int i = 0; i < msg.getPropertiesCount(); i++)
            {
                smm.addProperty().setKey(msg.getPropertyAt(i).getKey()).setValue(msg.getPropertyAt(i).getValue());
            }

            if (msg.hasEventTime())
            {
                smm.setEventTime(msg.getEventTime());
            }

            if (msg.hasSequenceId())
            {
                smm.setSequenceId(msg.getSequenceId());
            }

            if (msg.hasNullValue())
            {
                smm.setNullValue(msg.isNullValue());
            }

            if (msg.hasNullPartitionKey())
            {
                smm.setNullPartitionKey(msg.isNullPartitionKey());
            }

            return SerializeSingleMessageInBatchWithPayload(smm, payload, batchBuffer);
        }

        public static ByteBuf DeSerializeSingleMessageInBatch(ByteBuf uncompressedPayload, SingleMessageMetadata singleMessageMetadata, int index, int batchSize)
        {
            int singleMetaSize = (int)uncompressedPayload.readUnsignedInt();
            singleMessageMetadata.ParseFrom(uncompressedPayload, singleMetaSize);

            int singleMessagePayloadSize = singleMessageMetadata.PayloadSize;

            int readerIndex = uncompressedPayload.ReaderIndex;
            ByteBuf singleMessagePayload = uncompressedPayload.retainedSlice(readerIndex, singleMessagePayloadSize);

            // reader now points to beginning of payload read; so move it past message payload just read
            if (index < batchSize)
            {
                uncompressedPayload.readerIndex(readerIndex + singleMessagePayloadSize);
            }

            return singleMessagePayload;
        }

        public static ByteBufPair SerializeCommandMessageWithSize(BaseCommand cmd, ByteBuf metadataAndPayload)
        {
            // / Wire format
            // [TOTAL_SIZE] [CMD_SIZE][CMD] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
            //
            // metadataAndPayload contains from magic-number to the payload included

            int cmdSize = cmd.getSerializedSize();
            int totalSize = 4 + cmdSize + metadataAndPayload.readableBytes();
            int headersSize = 4 + 4 + cmdSize;

            ByteBuf headers = PulsarByteBufAllocator.DEFAULT.buffer(headersSize);
            headers.writeInt(totalSize); // External frame

            // Write cmd
            headers.writeInt(cmdSize);
            cmd.writeTo(headers);
            return ByteBufPair.get(headers, metadataAndPayload);
        }

        public static MessageMetadata peekMessageMetadata(ByteBuf metadataAndPayload, string subscription, long consumerId)
        {
            // save the reader index and restore after parsing
            int readerIdx = metadataAndPayload.readerIndex();
            try
            {
                MessageMetadata metadata = parseMessageMetadata(metadataAndPayload);
                return metadata;
            }
            catch (Exception t)
            {
                log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
                return null;
            }
            finally
            {
                metadataAndPayload.readerIndex(readerIdx);
            }
        }

        public static void peekMessageMetadata(ByteBuf metadataAndPayload, MessageMetadata msgMetadata)
        {
            // save the reader index and restore after parsing
            int readerIdx = metadataAndPayload.readerIndex();
            try
            {
                parseMessageMetadata(metadataAndPayload, msgMetadata);
            }
            finally
            {
                metadataAndPayload.readerIndex(readerIdx);
            }
        }

        /// <summary>
        /// Peek the message metadata from the buffer and return a deep copy of the metadata.
        /// 
        /// If you want to hold multiple <seealso cref="MessageMetadata"/> instances from multiple buffers, you must call this method
        /// rather than <seealso cref="Commands.peekMessageMetadata(ByteBuf, String, long)"/>, which returns a thread local reference,
        /// see <seealso cref="Commands.LOCAL_MESSAGE_METADATA"/>.
        /// </summary>
        public static MessageMetadata peekAndCopyMessageMetadata(ByteBuf metadataAndPayload, string subscription, long consumerId)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final org.apache.pulsar.common.api.proto.MessageMetadata metadata = new org.apache.pulsar.common.api.proto.MessageMetadata();
            MessageMetadata metadata = new MessageMetadata();
            try
            {
                peekMessageMetadata(metadataAndPayload, metadata);
            }
            catch (Exception t)
            {
                log.error("[{}] [{}] Failed to parse message metadata", subscription, consumerId, t);
                return null;
            }
            return metadata;
        }

        public static readonly sbyte[] NONE_KEY = "NONE_KEY".getBytes(StandardCharsets.UTF_8);

        public static sbyte[] peekStickyKey(ByteBuf metadataAndPayload, string topic, string subscription)
        {
            int readerIdx = metadataAndPayload.readerIndex();
            try
            {
                MessageMetadata metadata = parseMessageMetadata(metadataAndPayload);
                return resolveStickyKey(metadata);
            }
            catch (Exception t)
            {
                log.error("[{}] [{}] Failed to peek sticky key from the message metadata", topic, subscription, t);
                return NONE_KEY;
            }
            finally
            {
                metadataAndPayload.readerIndex(readerIdx);
            }
        }

        public static sbyte[] resolveStickyKey(MessageMetadata metadata)
        {
            sbyte[] stickyKey;
            if (metadata.hasOrderingKey())
            {
                stickyKey = metadata.getOrderingKey();
            }
            else if (metadata.hasPartitionKey())
            {
                if (metadata.isPartitionKeyB64Encoded())
                {
                    stickyKey = Base64.getDecoder().decode(metadata.getPartitionKey());
                }
                else
                {
                    stickyKey = metadata.getPartitionKey().getBytes(StandardCharsets.UTF_8);
                }
            }
            else if (metadata.hasProducerName() && metadata.hasSequenceId())
            {
                string fallbackKey = metadata.getProducerName() + "-" + metadata.getSequenceId();
                stickyKey = fallbackKey.GetBytes(Encoding.UTF8);
            }
            else
            {
                stickyKey = NONE_KEY;
            }
            return stickyKey;
        }

        public static int CurrentProtocolVersion
        {
            get
            {
                return CURRENT_PROTOCOL_VERSION;
            }
        }

        /// <summary>
        /// Definition of possible checksum types.
        /// </summary>
        public enum ChecksumType
        {
            Crc32c,
            None
        }

        public static bool peerSupportsGetLastMessageId(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v12.getValue();
        }

        public static bool peerSupportsActiveConsumerListener(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v12.getValue();
        }

        public static bool PeerSupportsMultiMessageAcknowledgment(int peerVersion)
        {
            return peerVersion >= (int)ProtocolVersion.V12;
        }

        public static bool PeerSupportJsonSchemaAvroFormat(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.V13.getValue();
        }

        public static bool peerSupportsGetOrCreateSchema(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v15.getValue();
        }

        public static bool peerSupportsAckReceipt(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v17.getValue();
        }

        public static bool peerSupportsCarryAutoConsumeSchemaToBroker(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v21.getValue();
        }

        private static ProducerAccessMode ConvertProducerAccessMode(ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case Exclusive:
                    return org.apache.pulsar.common.api.proto.ProducerAccessMode.Exclusive;
                case Shared:
                    return org.apache.pulsar.common.api.proto.ProducerAccessMode.Shared;
                case WaitForExclusive:
                    return org.apache.pulsar.common.api.proto.ProducerAccessMode.WaitForExclusive;
                case ExclusiveWithFencing:
                    return org.apache.pulsar.common.api.proto.ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new System.ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static ProducerAccessMode convertProducerAccessMode(org.apache.pulsar.common.api.proto.ProducerAccessMode accessMode)
        {
            switch (accessMode)
            {
                case Exclusive:
                    return ProducerAccessMode.Exclusive;
                case Shared:
                    return ProducerAccessMode.Shared;
                case WaitForExclusive:
                    return ProducerAccessMode.WaitForExclusive;
                case ExclusiveWithFencing:
                    return ProducerAccessMode.ExclusiveWithFencing;
                default:
                    throw new System.ArgumentException("Unknown access mode: " + accessMode);
            }
        }

        public static bool peerSupportsBrokerMetadata(int peerVersion)
        {
            return peerVersion >= ProtocolVersion.v16.getValue();
        }

    }
}
