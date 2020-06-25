using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities;
using SharpPulsar.Api;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
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
namespace SharpPulsar.Batch
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

		private MessageMetadata _messageMetadata = new MessageMetadata();
		// sequence id for this batch which will be persisted as a single entry by broker
		private long _lowestSequenceId = -1L;
		private long _highestSequenceId = -1L;
		private byte[] _batchedMessageMetadataAndPayload;
		private IList<Message> _messages = new List<Message>();
		private ISendCallback _previousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		private ISendCallback _firstCallback;
        private ILoggingAdapter _log;

        public BatchMessageContainer(ActorSystem system)
        {
            _log = system.Log;
        }
		public override bool Add(Message msg, ISendCallback callback)
		{

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"[{TopicName}] [{ProducerName}] add message to batch, num messages in batch so far {NumMessagesInBatch}");
			}

			if (++NumMessagesInBatch == 1)
			{
				// some properties are common amongst the different messages in the batch, hence we just pick it up from
				// the first message
				_messageMetadata.SequenceId = (ulong)msg.SequenceId;
				_lowestSequenceId = Commands.InitBatchMessageMetadata(_messageMetadata);
				_firstCallback = callback;
				_batchedMessageMetadataAndPayload = new byte[Math.Min(MaxBatchSize, ProducerContainer.MaxMessageSize)]; ;
			}

            _previousCallback?.AddCallback(msg, callback);
            _previousCallback = callback;
			CurrentBatchSize += msg.Payload.Length;
			_messages.Add(msg);

			if (_lowestSequenceId == -1L)
			{
				_lowestSequenceId = msg.SequenceId;
				_messageMetadata.SequenceId = (ulong)_lowestSequenceId;
			}
			_highestSequenceId = msg.SequenceId;
            callback.LastSequencePushed(msg.SequenceId);

			return BatchFull;
		}

		private byte[] CompressedBatchMetadataAndPayload
		{
			get
			{
    
				for (int i = 0, n = _messages.Count; i < n; i++)
				{
					Message msg = _messages[i];
					var msgMetadata = new MessageMetadata();
					try
					{
						_batchedMessageMetadataAndPayload = Commands.SerializeSingleMessageInBatchWithPayload(msgMetadata, msg.Payload, _batchedMessageMetadataAndPayload);
					}
					catch (Exception)
					{
						// serializing batch message can corrupt the index of message and batch-message. Reset the index so,
						// next iteration doesn't send corrupt message to broker.
						/*for (int j = 0; j <= i; j++)
						{
							var previousMsg = _messages[j];
						}*/
						throw;
					}
				}
				// Recycle messages only once they serialized successfully in batch
				/*foreach (var msg in _messages)
				{
					msg;
				}*/
				var uncompressedSize = _batchedMessageMetadataAndPayload.Length;
				var compressedPayload = Compressor.Encode(_batchedMessageMetadataAndPayload);
				_batchedMessageMetadataAndPayload = new byte[0];
				if (CompressionType != CompressionType.None)
				{
					_messageMetadata.Compression = CompressionType;
					_messageMetadata.UncompressedSize = (uint)uncompressedSize;
				}
    
				// Update the current max batch size using the uncompressed size, which is what we need in any case to
				// accumulate the batch content
				MaxBatchSize = (int)Math.Max(CurrentBatchSize, uncompressedSize);
				return compressedPayload;
			}
		}

		public override void Clear()
		{
			_messages = new List<Message>();
			_firstCallback = null;
			_previousCallback = null;
			_messageMetadata = new MessageMetadata();
			NumMessagesInBatch = 0;
			CurrentBatchSize = 0;
			_lowestSequenceId = -1L;
			_highestSequenceId = -1L;
			_batchedMessageMetadataAndPayload = null;
		}

		public override bool Empty => _messages.Count == 0;

        public override void Discard(Exception ex)
		{
			try
            {
                // Need to protect ourselves from any exception being thrown in the future handler from the application
                _firstCallback?.SendComplete(ex);
            }
			catch (Exception t)
			{
				_log.Warning($"[{TopicName}] [{ProducerName}] Got exception while completing the callback for msg {_lowestSequenceId}: {t}");
			}
			Clear();
		}

		public override bool MultiBatches => false;

		public override OpSendMsg CreateOpSendMsg()
		{
			var encryptedPayload = CompressedBatchMetadataAndPayload;
            if (ProducerContainer.Configuration.EncryptionEnabled && ProducerContainer.Crypto != null)
            {
                try
                {
                    encryptedPayload = ProducerContainer.Crypto.Encrypt(ProducerContainer.Configuration.EncryptionKeys, ProducerContainer.Configuration.CryptoKeyReader, _messageMetadata, encryptedPayload);
                }
                catch (PulsarClientException e)
                {
                    // Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
                    if (ProducerContainer.Configuration.CryptoFailureAction != ProducerCryptoFailureAction.Send) 
                        throw;
                    _log.Warning($"[{TopicName}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
                    encryptedPayload = CompressedBatchMetadataAndPayload;
                }
            }
			if (encryptedPayload.Length > ProducerContainer.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message size is bigger than " + ProducerContainer.MaxMessageSize + " bytes"));
				return null;
			}
			_messageMetadata.NumMessagesInBatch = NumMessagesInBatch;
			_messageMetadata.HighestSequenceId = (ulong)_highestSequenceId;
			var cmd = Commands.NewSend(ProducerContainer.ProducerId, (long)_messageMetadata.SequenceId, (long)_messageMetadata.HighestSequenceId, NumMessagesInBatch, _messageMetadata, encryptedPayload);

			var op = OpSendMsg.Create(_messages, cmd, (long)_messageMetadata.SequenceId, (long)_messageMetadata.HighestSequenceId, _firstCallback);

			op.NumMessagesInBatch = NumMessagesInBatch;
			op.BatchSizeByte = CurrentBatchSize;
			_lowestSequenceId = -1L;
			return op;
		}

		public override bool HasSameSchema(Message msg)
		{
			if (NumMessagesInBatch == 0)
			{
				return true;
			}
			if (_messageMetadata.SchemaVersion.Length !> 0)
			{
				return msg.SchemaVersion == null;
			}
			return Arrays.Equals(msg.SchemaVersion, _messageMetadata.SchemaVersion);
		}

	}

}