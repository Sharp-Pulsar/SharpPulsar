using SharpPulsar.Protocol.Proto;
using System.Collections.Generic;

namespace SharpPulsar.Protocol.Builder
{
    public class MessageMetadataBuilder
    {
        private MessageMetadata _data;
        public MessageMetadataBuilder()
        {
            _data = new MessageMetadata();
        }
        
        public MessageMetadataBuilder SetCompressionType(CompressionType compression)
        {
            _data.Compression = compression;
            return this;
        }
        public MessageMetadataBuilder SetDeliveryTime(long deliverAtTime)
        {
            _data.DeliverAtTime = deliverAtTime;
            return this;
        }
        public MessageMetadataBuilder SetEncryptionAlgo(string encryptionAlgo)
        {
            _data.EncryptionAlgo = encryptionAlgo;
            return this;
        }
        public MessageMetadataBuilder SetEncryptionKeys(IList<EncryptionKeys> keys)
        {
            _data.EncryptionKeys.AddRange(keys);
            return this;
        }
        public MessageMetadataBuilder SetEncryptionParam(byte[] para)
        {
            _data.EncryptionParam = para;
            return this;
        }
        public MessageMetadataBuilder SetEventTime(long eventTime)
        {
            _data.EventTime = (ulong)eventTime;
            return this;
        }
        public MessageMetadataBuilder SetHighestSequenceId(long highestSequenceId)
        {
            _data.HighestSequenceId = (ulong)highestSequenceId;
            return this;
        }
        public MessageMetadataBuilder SetMarkerType(int markerType)
        {
            _data.MarkerType = markerType;
            return this;
        }
        public MessageMetadataBuilder SetNumMessagesInBatch(int numMessagesInBatch)
        {
            _data.NumMessagesInBatch = numMessagesInBatch;
            return this;
        }
        public MessageMetadataBuilder SetOrderingKey(byte[] orderingKey)
        {
            _data.OrderingKey = orderingKey;
            return this;
        }
        public MessageMetadataBuilder SetPartitionKey(string partitionKey)
        {
            _data.PartitionKey = partitionKey;
            return this;
        }
        public MessageMetadataBuilder SetPartitionKeyB64EncodedTo(bool partitionKeyB64Encoded)
        {
            _data.PartitionKeyB64Encoded = partitionKeyB64Encoded;
            return this;
        }
        public MessageMetadataBuilder SetProducerName(string producerName)
        {
            _data.ProducerName = producerName;
            return this;
        }
        public MessageMetadataBuilder AddProperties(IDictionary<string, string> properties)
        {
            _data.Properties.AddRange(CommandUtils.ToKeyValueList(properties));
            return this;
        }
        public MessageMetadataBuilder SetPublishTime(long publishTime)
        {
            _data.PublishTime = (ulong)publishTime;
            return this;
        }
        public MessageMetadataBuilder SetReplicatedFrom(string replicatedFrom)
        {
            _data.ReplicatedFrom = replicatedFrom;
            return this;
        }
        public MessageMetadataBuilder SetReplicateToes(IList<string> toes)
        {
            _data.ReplicateToes.AddRange(toes);
            return this;
        }
        public MessageMetadataBuilder SetSchemaVersion(byte[] schemaVersion)
        {
            _data.SchemaVersion = schemaVersion;
            return this;
        }
        public MessageMetadataBuilder SetSequenceId(long sequenceId)
        {
            _data.SequenceId = (ulong)sequenceId;
            return this;
        }
        public MessageMetadataBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _data.TxnidLeastBits = (ulong)txnidLeastBits;
            return this;
        }
        public MessageMetadataBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _data.TxnidMostBits = (ulong)txnidMostBits;
            return this;
        }
        public MessageMetadataBuilder SetUncompressedSize(int uncompressedSize)
        {
            _data.UncompressedSize = (uint)uncompressedSize;
            return this;
        }
        public MessageMetadata Build()
        {
            return _data;
        }
    }
}
