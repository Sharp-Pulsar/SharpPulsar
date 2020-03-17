using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Compression;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Impl
{

	/// <summary>
	/// Key based batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into multiple batch messages:
	/// [(k1, v1), (k1, v2), (k1, v3)], [(k2, v1), (k2, v2), (k2, v3)], [(k3, v1), (k3, v2), (k3, v3)]
	/// </summary>
	public class BatchMessageKeyBasedContainer : AbstractBatchMessageContainer
    {
        private static int _maxMessageSize;
        public BatchMessageKeyBasedContainer(int maxMessageSize)
        {
            _maxMessageSize = maxMessageSize;
        }
		public override (long LastSequenceIdPushed, bool BatchFul) Add(Message msg)
		{
			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug($"[{TopicName}] [{ProducerName}] add message to batch, num messages in batch so far is {Batches.Count}");
			}
			NumMessagesInBatch++;
			CurrentBatchSizeBytes += msg.Payload.Length;
			var key = GetKey(msg);
			Batches.TryGetValue(key, out var part);
			if (part == null)
			{
				part = new KeyedBatch();
				part.AddMsg(msg);
				part.CompressionType = Protocol.Proto.CompressionType.None;
				part.Compressor = Compressor;
				part.MaxBatchSize = MaxBatchSize;
				part.TopicName = TopicName;
				part.ProducerName = ProducerName;
				if (!Batches.ContainsKey(key)) 
                    Batches.Add(key, part);
			}
			else
			{
				part.AddMsg(msg);
			}
			return (msg.SequenceId, BatchFull);
		}

		public override void Clear()
		{
			NumMessagesInBatch = 0;
			CurrentBatchSizeBytes = 0;
			Batches = new Dictionary<string, KeyedBatch>();
		}

        public IDictionary<string, KeyedBatch> Batches { get; private set; } = new Dictionary<string, KeyedBatch>();
		public override bool Empty => Batches.Count == 0;

        public override void Discard(Exception ex)
		{
			try
			{
				// Need to protect ourselves from any exception being thrown in the future handler from the application
				Batches.ToList().ForEach(x => x.Value.Discard(ex));
			}
			catch (System.Exception T)
			{
				Log.LogWarning("[{}] [{}] Got exception while completing the callback", TopicName, ProducerName, T);
			}
            //_batches.ToList().ForEach((x => x.Value.BatchedMessageMetadataAndPayload.SafeRelease()));
			Clear();
		}

		public override bool MultiBatches => true;

		public override bool HasSameSchema(Message msg)
		{
			var key = GetKey(msg);
			Batches.TryGetValue(key, out var part);
			if (part == null || part.Messages.Count == 0)
			{
				return true;
			}
			if (part.MessageMetadata.SchemaVersion.Length < 1)
			{
				return msg.SchemaVersion == null;
			}
			return Equals(msg.SchemaVersion, part.MessageMetadata.SchemaVersion);
		}

		private string GetKey(Message msg)
		{
			if (msg.HasOrderingKey())
			{
				return Convert.ToBase64String((byte[])(object)msg.OrderingKey);
			}
			return msg.Key;
		}

		public class KeyedBatch
		{
			internal MessageMetadata MessageMetadata = new MessageMetadata();
			// sequence id for this batch which will be persisted as a single entry by broker
			internal long SequenceId = -1;
			internal byte[] BatchedMessageMetadataAndPayload;

			internal IList<Message> Messages = new List<Message>();
			internal SendCallback PreviousCallback = null;
			internal CompressionType CompressionType;
			internal CompressionCodec Compressor;
			internal int MaxBatchSize;
			internal string TopicName;
			internal string ProducerName;

			// keep track of callbacks for individual messages being published in a batch
			
			public byte[] CompressedBatchMetadataAndPayload
			{
				get
				{

					foreach (var msg in Messages)
					{
						MessageMetadata msgBuilder = msg.Metadata;
						BatchedMessageMetadataAndPayload = Commands.SerializeSingleMessageInBatchWithPayload(msgBuilder, msg.Payload, BatchedMessageMetadataAndPayload);
						
					}
					int uncompressedSize = BatchedMessageMetadataAndPayload.Length;
					var compressedPayload = Compressor.Encode(BatchedMessageMetadataAndPayload);
					if (CompressionType != CompressionType.None)
					{
						MessageMetadata.Compression = CompressionType;
						MessageMetadata.UncompressedSize = (uint)uncompressedSize;
					}
    
					// Update the current max batch size using the uncompressed size, which is what we need in any case to
					// accumulate the batch content
					MaxBatchSize = Math.Max(MaxBatchSize, uncompressedSize);
					return compressedPayload;
				}
			}

			public virtual void AddMsg(Message msg)
			{
				if (Messages.Count == 0)
				{
					SequenceId = Commands.InitBatchMessageMetadata(msg.Metadata);
					if (msg.HasKey())
					{
						MessageMetadata.PartitionKey = msg.Key;
						if (msg.HasBase64EncodedKey())
						{
							MessageMetadata.PartitionKeyB64Encoded = true;
						}
					}
					if (msg.HasOrderingKey())
					{
						MessageMetadata.OrderingKey = (byte[])(object)msg.OrderingKey;
					}
					BatchedMessageMetadataAndPayload = new byte[Math.Min(MaxBatchSize, _maxMessageSize)];
					
				}

				Messages.Add(msg);
			}

			public virtual void Discard(Exception ex)
			{
				Clear();
			}

			public virtual void Clear()
			{
				Messages = new List<Message>();
				PreviousCallback = null;
				MessageMetadata = new MessageMetadata();
				SequenceId = -1;
				BatchedMessageMetadataAndPayload = null;
			}
		}

		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger(typeof(BatchMessageKeyBasedContainer));

	}

}