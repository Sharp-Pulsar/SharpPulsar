using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using Akka.Event;
using ProtoBuf;
using SharpPulsar.Common;
using SharpPulsar.Exceptions;
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
	internal class BatchMessageContainer<T> : AbstractBatchMessageContainer<T>
	{

		private MessageMetadata _messageMetadata = new MessageMetadata();
		// sequence id for this batch which will be persisted as a single entry by broker
		private long _lowestSequenceId = -1L;
		private long _highestSequenceId = -1L;
		private List<byte> _batchedMessageMetadataAndPayload;
		private IList<Message<T>> _messages = new List<Message<T>>();
		private Action<object, Exception> _previousCallback = null;
		// keep track of callbacks for individual messages being published in a batch
		private Action<object, Exception> _firstCallback;
        private ILoggingAdapter _log;

        public BatchMessageContainer(ActorSystem system)
        {
            _log = system.Log;
        }
		public override bool Add(Message<T> msg, Action<object, Exception> callback)
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
				_batchedMessageMetadataAndPayload = new List<byte>(Math.Min(MaxBatchSize, Container.MaxMessageSize));
				if(msg.Metadata.OriginalMetadata.ShouldSerializeTxnidMostBits() && CurrentTxnidMostBits == -1)
				{
					CurrentTxnidMostBits = (long)msg.Metadata.OriginalMetadata.TxnidMostBits;
				}
				if (msg.Metadata.OriginalMetadata.ShouldSerializeTxnidLeastBits() && CurrentTxnidLeastBits == -1)
				{
					CurrentTxnidLeastBits = (long)msg.Metadata.OriginalMetadata.TxnidLeastBits;
				}
			}

            _previousCallback = callback;
			CurrentBatchSize += msg.Data.Length;
			_messages.Add(msg);

			if (_lowestSequenceId == -1L)
			{
				_lowestSequenceId = msg.SequenceId;
				_messageMetadata.SequenceId = (ulong)_lowestSequenceId;
			}
			_highestSequenceId = msg.SequenceId;
            callback(msg.SequenceId, null);

			return BatchFull;
		}

		private byte[] CompressedBatchMetadataAndPayload
		{
            get
            {
                var stream = Helpers.Serializer.MemoryManager.GetStream();
                var messageWriter = new BinaryWriter(stream);

                for (int i = 0, n = _messages.Count; i < n; i++)
                {
                    try
                    {
                        var msg = _messages[i];
                        var msgMetadata = msg.Metadata.OriginalMetadata;
                        Serializer.SerializeWithLengthPrefix(stream, Commands.SingleMessageMetadat(msgMetadata, (int)msg.Data.Length, msg.SequenceId), PrefixStyle.Fixed32BigEndian);
                        messageWriter.Write(msg.Data.ToArray());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                var batchedMessageMetadataAndPayload = stream.ToArray();

                var uncompressedSize = batchedMessageMetadataAndPayload.Length;
                var compressedPayload = Compressor.Encode(batchedMessageMetadataAndPayload);
                if (CompressionType != CompressionType.None)
                {
                    _messageMetadata.Compression = CompressionType;
                    _messageMetadata.UncompressedSize = (uint)uncompressedSize;
                }

                // Update the current max batch Size using the uncompressed Size, which is what we need in any case to
                // accumulate the batch content
                MaxBatchSize = (int)Math.Max(MaxBatchSize, uncompressedSize);
                return compressedPayload;
            }
        }

		public override void Clear()
		{
			_messages = new List<Message<T>>();
			_firstCallback = null;
			_previousCallback = null;
			_messageMetadata = new MessageMetadata();
			NumMessagesInBatch = 0;
			CurrentBatchSize = 0;
			_lowestSequenceId = -1L;
			_highestSequenceId = -1L;
			_batchedMessageMetadataAndPayload = null;
			CurrentTxnidMostBits = -1L;
			CurrentTxnidLeastBits = -1L;
		}

		public override bool Empty => _messages.Count == 0;

        public override void Discard(Exception ex)
		{
			try
            {
                // Need to protect ourselves from any exception being thrown in the future handler from the application
                _firstCallback(null, ex);
            }
			catch (Exception t)
			{
				_log.Warning($"[{TopicName}] [{ProducerName}] Got exception while completing the callback for msg {_lowestSequenceId}: {t}");
			}
			Clear();
		}

		public override bool MultiBatches => false;

		public override ProducerActor<T>.OpSendMsg<T> CreateOpSendMsg()
		{
			var encryptedPayload = CompressedBatchMetadataAndPayload;
            if (Container.Configuration.EncryptionEnabled && Container.Crypto != null)
            {
                try
                {
                    encryptedPayload = Container.Crypto.Encrypt(Container.Configuration.EncryptionKeys, Container.Configuration.CryptoKeyReader, _messageMetadata, encryptedPayload);
                }
                catch (PulsarClientException e)
                {
                    // Unless config is set to explicitly publish un-encrypted message upon failure, fail the request
                    if (Container.Configuration.CryptoFailureAction != ProducerCryptoFailureAction.Send) 
                        throw;
                    _log.Warning($"[{TopicName}] [{ProducerName}] Failed to encrypt message '{e.Message}'. Proceeding with publishing unencrypted message");
                    encryptedPayload = CompressedBatchMetadataAndPayload;
                }
            }
			if (encryptedPayload.Length > Container.MaxMessageSize)
			{
				Discard(new PulsarClientException.InvalidMessageException("Message Size is bigger than " + Container.MaxMessageSize + " bytes"));
				return null;
			}
			_messageMetadata.NumMessagesInBatch = NumMessagesInBatch;
			_messageMetadata.SequenceId = (ulong)_messages[0].SequenceId;
			_messageMetadata.HighestSequenceId = (ulong)_highestSequenceId;
			_messageMetadata.PartitionKey = _messages[0].Key;
			_messageMetadata.OrderingKey = _messages[0].OrderingKey;
			_messageMetadata.ProducerName = Container.ProducerName;
			_messageMetadata.PublishTime = (ulong)DateTimeHelper.CurrentUnixTimeMillis();
			if (CurrentTxnidMostBits != -1)
			{
				_messageMetadata.TxnidMostBits = (ulong)CurrentTxnidMostBits;
			}
			if (CurrentTxnidLeastBits != -1)
			{
				_messageMetadata.TxnidLeastBits = (ulong)CurrentTxnidLeastBits;
			}
			var cmd = Commands.NewSend(Container.ProducerId, _messages[0].SequenceId, _highestSequenceId, NumMessagesInBatch, _messageMetadata, new ReadOnlySequence<byte>(encryptedPayload));

			var op = ProducerActor<T>.OpSendMsg<T>.Create(_messages, cmd, _messages[0].SequenceId, _highestSequenceId);

			op.NumMessagesInBatch = NumMessagesInBatch;
			op.BatchSizeByte = CurrentBatchSize;
			_lowestSequenceId = -1L;
			return op;
		}

		public override bool HasSameSchema(Message<T> msg)
		{
			if (NumMessagesInBatch == 0)
			{
				return true;
			}
			if (!_messageMetadata.ShouldSerializeSchemaVersion())
			{
				return msg.SchemaVersion == null;
			}
			return Equals(msg.SchemaVersion, _messageMetadata.SchemaVersion);
		}

	}

}