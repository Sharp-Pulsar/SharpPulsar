using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Extension
{
    public static class MessageMetadataEx
    {
        public static void Clear(this MessageMetadata metadata)
        {
            metadata.ResetChunkId();
            metadata.ResetCompression();
            metadata.ResetDeliverAtTime();
            metadata.ResetEncryptionAlgo();
            metadata.ResetEncryptionParam();
            metadata.ResetEventTime();
            metadata.ResetHighestSequenceId();
            metadata.ResetMarkerType();
            metadata.ResetNullPartitionKey();
            metadata.ResetNullValue();
            metadata.ResetNumChunksFromMsg();
            metadata.ResetNumMessagesInBatch();
            metadata.ResetOrderingKey();
            metadata.ResetPartitionKey();
            metadata.ResetPartitionKeyB64Encoded();
            metadata.ResetReplicatedFrom();
            metadata.ResetSchemaVersion();
            metadata.ResetTotalChunkMsgSize();
            metadata.ResetTxnidLeastBits();
            metadata.ResetTxnidMostBits();
            metadata.ResetUncompressedSize();
            metadata.ResetUuid();
        }
    }
}
