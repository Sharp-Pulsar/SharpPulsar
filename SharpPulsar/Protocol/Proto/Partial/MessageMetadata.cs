using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPulsar.Protocol.Proto
{
	public partial class MessageMetadata 
    { 
		public static Builder NewBuilder()
		{
			return Builder.Create();
		}
		public static Builder NewBuilder(MessageMetadata prototype)
		{
			return NewBuilder().MergeFrom(prototype);
		}
		public Builder ToBuilder()
		{
			return NewBuilder(this);
		}
		public sealed class Builder
        {
            private MessageMetadata _metadata = new MessageMetadata();
			internal static Builder Create()
			{
				return new Builder();
			}

            public MessageMetadata Build()
			{
                return _metadata;
			}
            public Builder MergeFrom(MessageMetadata message)
            {
                _metadata = message;
                return this;
            }

            public int NumMessagesInBatch {
                get => _metadata.NumMessagesInBatch;
                set => _metadata.NumMessagesInBatch = value;

            }
            public bool HasNumMessagesInBatch()
            {
                return _metadata.NumMessagesInBatch > 0;
            }
			public Builder SetProducerName(string value)
			{
                _metadata.ProducerName = value ?? throw new NullReferenceException();

				return this;
			}

            public bool HasPublishTime()
            {
                return _metadata.PublishTime > 0;
            }

			public ulong GetSequenceId()
            {
                return _metadata.SequenceId;
            }

			public bool HasSequenceId()
            {
                return _metadata.SequenceId > 0;
            }

            public IDictionary<string, string> Properties =>
                _metadata.Properties.ToDictionary(x => x.Key, x => x.Value);
            public bool HasOrderingKey()
            {
                return _metadata.OrderingKey?.Length > 0;
            }

            public string GetReplicatedFrom()
            {
                return _metadata.ReplicatedFrom;
            }

			public bool HasReplicatedFrom()
            {
                return !string.IsNullOrWhiteSpace(_metadata.ReplicatedFrom);
            }

			public long GetPublishTime()
            {
                return (long)_metadata.PublishTime;
            }

			public long EventTime => (long)_metadata.EventTime;

			public bool HasEventTime()
            {
                return _metadata.EventTime > 0;
            }

			public bool HasSchemaVersion()
            {
                return _metadata.SchemaVersion?.Length > 0;
            }

			public byte[] GetSchemaVersion()
            {
                return _metadata.SchemaVersion;
            }

			public string GetProducerName()
            {
                return _metadata.ProducerName;
            }

			public bool HasPartitionKey()
            {
                return !string.IsNullOrWhiteSpace(_metadata.PartitionKey);
            }

			public string GetPartitionKey()
            {
                return _metadata.PartitionKey;
            }

			public bool PartitionKeyB64Encoded => _metadata.PartitionKeyB64Encoded;
			public byte[] GetOrderingKey()
            {
                return _metadata.OrderingKey;
            }

			public bool HasDeliverAtTime()
            {
                return _metadata.DeliverAtTime > 0;
            }

			public bool HasProducerName()
            {
                return !string.IsNullOrWhiteSpace(_metadata.ProducerName);
            }
			public Builder SetSequenceId(long value)
			{
				_metadata.SequenceId = (ulong)value;

				return this;
			}
			
			public Builder SetPublishTime(long value)
			{
				_metadata.PublishTime = (ulong)value;

				return this;
			}
			
			public Builder SetProperties(int index, KeyValue value)
			{
				_metadata.Properties[index] = value;

				return this;
			}

			public Builder AddProperties(KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				
				_metadata.Properties.Add(value);

				return this;
			}
			public Builder AddProperties(int index, KeyValue value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				
				_metadata.Properties.Insert(index, value);

				return this;
			}
			public Builder AddAllProperties(IEnumerable<KeyValue> values) 
			{
				_metadata.Properties.AddRange(values);

				return this;
			}
			
			public Builder SetReplicatedFrom(string value)
			{
                _metadata.ReplicatedFrom = value ?? throw new NullReferenceException();

				return this;
			}
			
			public long SequenceId()
			{
				return (long)_metadata.SequenceId;
			}
			
			public Builder SetPartitionKey(string value)
			{
				_metadata.PartitionKey = value ?? throw new NullReferenceException();

				return this;
			}
			
            public string GetReplicateTo(int index)
			{
				return _metadata.ReplicateToes[index];
			}
			public Builder SetReplicateTo(int index, string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
                _metadata.ReplicateToes.Insert(index, value);

				return this;
			}
			public Builder AddReplicateTo(string value)
			{
				if (ReferenceEquals(value, null))
				{
					throw new NullReferenceException();
				}
				_metadata.ReplicateToes.Add(value);

				return this;
			}
			public Builder AddAllReplicateTo(IEnumerable<string> values)
			{
				_metadata.ReplicateToes.AddRange(values);
				return this;
			}
			
			public Builder SetCompression(Common.Enum.CompressionType value)
			{
				_metadata.Compression = GetCompressionType(value);

				return this;
			}
			public Builder SetCompression(CompressionType value)
			{
                _metadata.Compression = value;
				return this;
			}
			private CompressionType GetCompressionType(Common.Enum.CompressionType value)
			{
				switch(value)
				{
					case Common.Enum.CompressionType.LZ4:
						return CompressionType.Lz4;
					case Common.Enum.CompressionType.SNAPPY:
						return CompressionType.Snappy;
					case Common.Enum.CompressionType.ZLIB:
						return CompressionType.Zlib;
					case Common.Enum.CompressionType.ZSTD:
						return CompressionType.Zstd;
					default:
						return CompressionType.None;
				}
			}
			
            public Builder SetUncompressedSize(int value)
            {
                _metadata.UncompressedSize = (uint)value;
				return this;
			}
			
            public Builder SetNumMessagesInBatch(int value)
            {
                _metadata.NumMessagesInBatch = value;
				return this;
			}
			
            public Builder SetEventTime(long value)
            {
                _metadata.EventTime = (ulong)value;
				return this;
			}
			
            public EncryptionKeys GetEncryptionKeys(int index)
			{
				return _metadata.EncryptionKeys[index];
			}
			public Builder SetEncryptionKeys(int index, EncryptionKeys value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _metadata.EncryptionKeys[index] = value;

				return this;
			}
			public Builder AddEncryptionKeys(EncryptionKeys value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _metadata.EncryptionKeys.Add(value);

				return this;
			}
			public Builder AddEncryptionKeys(int index, EncryptionKeys value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
                _metadata.EncryptionKeys.Insert(index, value);

				return this;
			}
						
			public Builder AddAllEncryptionKeys(IEnumerable<EncryptionKeys> values) 
			{
                _metadata.EncryptionKeys.AddRange(values);
				return this;
			}
			
			public Builder RemoveEncryptionKeys(int index)
			{
                _metadata.EncryptionKeys.RemoveAt(index);

				return this;
			}

			public string GetEncryptionAlgo()
			{
				return _metadata.EncryptionAlgo;
			}
			public Builder SetEncryptionAlgo(string Value)
			{
                _metadata.EncryptionAlgo = Value ?? throw new NullReferenceException();

				return this;
			}
			
			public Builder SetEncryptionParam(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}

                _metadata.EncryptionParam = value;

				return this;
			}
			
			public Builder SetSchemaVersion(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				_metadata.SchemaVersion = value;

				return this;
			}
			
            public Builder SetPartitionKeyB64Encoded(bool value)
			{
				_metadata.PartitionKeyB64Encoded = value;

				return this;
			}
			
			public Builder SetOrderingKey(byte[] value)
			{
				if (value == null)
				{
					throw new NullReferenceException();
				}
				
				_metadata.OrderingKey = value;

				return this;
			}
			
			public Builder SetDeliverAtTime(long value)
			{
				_metadata.DeliverAtTime = value;

				return this;
			}
			
			public Builder SetMarkerType(int value)
			{
				_metadata.MarkerType = value;

				return this;
			}
			
			public Builder SetTxnidLeastBits(long value)
			{
				_metadata.TxnidLeastBits = (ulong)value;

				return this;
			}
			
			public Builder SetTxnidMostBits(long value)
			{
				_metadata.TxnidMostBits = (ulong)value;

				return this;
			}
			
			public Builder SetHighestSequenceId(long value)
			{
				_metadata.HighestSequenceId = (ulong)value;

				return this;
			}

            public long HighestSequenceId => (long)_metadata.HighestSequenceId;

        }

	}

}
