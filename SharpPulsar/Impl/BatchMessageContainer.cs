using Microsoft.Extensions.Logging;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Api;

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
	/// Default batch message container
	/// 
	/// incoming single messages:
	/// (k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)
	/// 
	/// batched into single batch message:
	/// [(k1, v1), (k2, v1), (k3, v1), (k1, v2), (k2, v2), (k3, v2), (k1, v3), (k2, v3), (k3, v3)]
	/// </summary>
	public class BatchMessageContainer : AbstractBatchMessageContainer
	{
		// sequence id for this batch which will be persisted as a single entry by broker
        private  MessageMetadata _messageMetadata = new MessageMetadata();
		private long _lowestSequenceId = -1L;
		private long _highestSequenceId = -1L;
		private byte[] _batchedMessageMetadataAndPayload;
		private IList<Message> _messages = new List<Message>();

        public override bool HasSameSchema(Message msg)
        {
			if (NumMessagesInBatch == 0)
            {
                return true;
            }
            if (_messageMetadata.SchemaVersion == null)
            {
                return msg.SchemaVersion == null;
            }
            return Equals(msg.SchemaVersion, _messageMetadata.SchemaVersion);
		}

        public override (long LastSequenceIdPushed, bool BatchFul) Add(Message msg)
		{

			if (Log.IsEnabled(LogLevel.Debug))
			{
				Log.LogDebug("[{}] [{}] add message to batch, num messages in batch so far {}", TopicName, ProducerName, NumMessagesInBatch);
			}

			if (++NumMessagesInBatch == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				_lowestSequenceId = Commands.InitBatchMessageMetadata(new MessageMetadata());
				_batchedMessageMetadataAndPayload = new byte[Math.Min(MaxBatchSize, Commands.DefaultMaxMessageSize)];
			}

			CurrentBatchSizeBytes += msg.Payload.Length;
			_messages.Add(msg);

			if (_lowestSequenceId == -1L)
			{
				_lowestSequenceId = msg.SequenceId;
				_messageMetadata.SequenceId = (ulong)_lowestSequenceId;
			}
			_highestSequenceId = msg.SequenceId;

			return (msg.SequenceId, BatchFull);
		}

		public byte[] CompressedBatchMetadataAndPayload
		{
			get
			{
                for (int i = 0, n = _messages.Count; i < n; i++)
				{
					var msg = _messages[i];
					var msgMetadata = msg.Metadata;
					try
					{
						_batchedMessageMetadataAndPayload = Commands.SerializeSingleMessageInBatchWithPayload(msgMetadata, msg.Payload, _batchedMessageMetadataAndPayload);
					}
					catch (Exception th)
					{
						
						throw new Exception(th.Message);
					}
				}
				
				var uncompressedSize = _batchedMessageMetadataAndPayload.Length;
				var compressedPayload = Compressor.Encode(_batchedMessageMetadataAndPayload);
				if (CompressionType != ICompressionType.None)
                {
                    var compression = Enum.GetValues(typeof(CompressionType)).Cast<CompressionType>()
                        .ToList()[(int)CompressionType];
					_messageMetadata.Compression = compression;
					_messageMetadata.UncompressedSize = (uint)uncompressedSize;
				}
    
				// Update the current max batch size using the uncompressed size, which is what we need in any case to
				// accumulate the batch content
				MaxBatchSize = Math.Max(MaxBatchSize, uncompressedSize);
				return compressedPayload;
			}
		}

        public List<Message> Messages => _messages.ToList();
        public MessageMetadata Metadata => _messageMetadata;
        public int GetNumMessagesInBatch => NumMessagesInBatch;
        public long HighestSequenceId => _highestSequenceId;
		public override void Clear()
		{
			_messages = new List<Message>();
			_messageMetadata = new MessageMetadata();
			NumMessagesInBatch = 0;
			CurrentBatchSizeBytes = 0;
			_lowestSequenceId = -1L;
			_highestSequenceId = -1L;
			_batchedMessageMetadataAndPayload = null;
		}

		public override bool Empty => _messages.Count == 0;

        public override void Discard(Exception ex)
		{
			Clear();
		}

        public long LowestSequenceId {
            get => _lowestSequenceId;
            set => _lowestSequenceId = value;
        }
        public override bool MultiBatches => false;

        
		private static readonly ILogger Log = Utility.Log.Logger.CreateLogger<BatchMessageContainer>();
	}

}