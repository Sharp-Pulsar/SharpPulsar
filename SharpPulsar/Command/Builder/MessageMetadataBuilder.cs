using SharpPulsar.Common.Protocol;
using SharpPulsar.Common.PulsarApi;
using System.Collections.Generic;

namespace SharpPulsar.Command.Builder
{
    public class MessageMetadataBuilder
    {
        private MessageMetadata _data;
        public MessageMetadataBuilder()
        {
            _data = new MessageMetadata();
        }
        private MessageMetadataBuilder(MessageMetadata metadata)
        {
            _data = metadata;
        }
        public MessageMetadataBuilder SetCompressionType(CompressionType compression)
        {
            _data.Compression = compression;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetDeliveryTime(long deliverAtTime)
        {
            _data.DeliverAtTime = deliverAtTime;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetEncryptionAlgo(string encryptionAlgo)
        {
            _data.EncryptionAlgo = encryptionAlgo;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetEncryptionKeys(IList<EncryptionKeys> keys)
        {
            _data.EncryptionKeys.AddRange(keys);
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetEncryptionParam(byte[] para)
        {
            _data.EncryptionParam = para;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetEventTime(long eventTime)
        {
            _data.EventTime = (ulong)eventTime;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetHighestSequenceId(long highestSequenceId)
        {
            _data.HighestSequenceId = (ulong)highestSequenceId;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetMarkerType(int markerType)
        {
            _data.MarkerType = markerType;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetNumMessagesInBatch(int numMessagesInBatch)
        {
            _data.NumMessagesInBatch = numMessagesInBatch;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetOrderingKey(byte[] orderingKey)
        {
            _data.OrderingKey = orderingKey;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetPartitionKey(string partitionKey)
        {
            _data.PartitionKey = partitionKey;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetPartitionKeyB64EncodedTo(bool partitionKeyB64Encoded)
        {
            _data.PartitionKeyB64Encoded = partitionKeyB64Encoded;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetProducerName(string producerName)
        {
            _data.ProducerName = producerName;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder AddProperties(IDictionary<string, string> properties)
        {
            _data.Properties.AddRange(CommandUtils.ToKeyValueList(properties));
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetPublishTime(long publishTime)
        {
            _data.PublishTime = (ulong)publishTime;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetReplicatedFrom(string replicatedFrom)
        {
            _data.ReplicatedFrom = replicatedFrom;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetReplicateToes(IList<string> toes)
        {
            _data.ReplicateToes.AddRange(toes);
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetSchemaVersion(byte[] schemaVersion)
        {
            _data.SchemaVersion = schemaVersion;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetSequenceId(long sequenceId)
        {
            _data.SequenceId = (ulong)sequenceId;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetTxnidLeastBits(long txnidLeastBits)
        {
            _data.TxnidLeastBits = (ulong)txnidLeastBits;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetTxnidMostBits(long txnidMostBits)
        {
            _data.TxnidMostBits = (ulong)txnidMostBits;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadataBuilder SetUncompressedSize(int uncompressedSize)
        {
            _data.UncompressedSize = (uint)uncompressedSize;
            return new MessageMetadataBuilder(_data);
        }
        public MessageMetadata Build()
        {
            return _data;
        }
    }
}
