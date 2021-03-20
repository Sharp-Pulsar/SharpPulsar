using System;
using System.Collections.Generic;
using Akka.Actor;
using SharpPulsar.Common.Compression;
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
	/// Batch message container framework.
	/// </summary>
	internal abstract class AbstractBatchMessageContainer<T> : IBatchMessageContainerBase<T>
	{
		public abstract bool MultiBatches {get;}
		public abstract void Discard(Exception ex);
		public abstract bool Empty {get;}
		public abstract void Clear();
		public abstract bool HasSameSchema(Message<T> msg);
		public abstract bool Add(Message<T> msg, Action<object, Exception> callback);

		private string _topicName;
		private string _producerName;

        private int _maxNumMessagesInBatch;
		private int _maxBytesInBatch;
		private int _numMessagesInBatch = 0;
		private long _currentBatchSizeBytes = 0;

		protected internal long CurrentTxnidMostBits = -1L;
		protected internal long CurrentTxnidLeastBits = -1L;

		private const int InitialBatchBufferSize = 1024;

		// This will be the largest Size for a batch sent from this particular producer. This is used as a baseline to
		// allocate a new buffer that can hold the entire batch without needing costly reallocations
		private int _maxBatchSize = InitialBatchBufferSize;

		public virtual bool HaveEnoughSpace(Message<T> msg)
		{
			var messageSize = msg.Data.Length;
			return ((_maxBytesInBatch <= 0 && (messageSize + _currentBatchSizeBytes) <= _producerContainer.MaxMessageSize) || (_maxBytesInBatch > 0 && (messageSize + _currentBatchSizeBytes) <= _maxBytesInBatch)) && (_maxNumMessagesInBatch <= 0 || _numMessagesInBatch < _maxNumMessagesInBatch);
		}

		public virtual bool BatchFull => (_maxBytesInBatch > 0 && _currentBatchSizeBytes >= _maxBytesInBatch) || (_maxBytesInBatch <= 0 && _currentBatchSizeBytes >= _producerContainer.MaxMessageSize) || (_maxNumMessagesInBatch > 0 && _numMessagesInBatch >= _maxNumMessagesInBatch);

        public virtual int NumMessagesInBatch
        {
            get => _numMessagesInBatch;
            set => _numMessagesInBatch = value;
        }

        public CompressionCodec Compressor { get; private set; }
        public CompressionType CompressionType { get; private set; }
        public virtual long CurrentBatchSize
        {
            get => _currentBatchSizeBytes;
            set => _currentBatchSizeBytes = value;

        }
        public virtual string ProducerName
        {
            set => _producerName = value;
            get => _producerName;
        }

        public virtual string TopicName => _topicName;

        public virtual IList<ProducerActor<T>.OpSendMsg<T>> CreateOpSendMsgs()
		{
			throw new NotSupportedException();
		}

		public virtual ProducerActor<T>.OpSendMsg<T> CreateOpSendMsg()
		{
			throw new NotSupportedException();
		}

        private ProducerContainer _producerContainer;
		public virtual ProducerContainer Container
        {
            get => _producerContainer;
			set
            {
                _producerContainer = value;
                Producer = value.Producer;
				_topicName = value.Configuration.TopicName;
				_producerName = value.Configuration.ProducerName;
				CompressionType = CompressionCodecProvider.ConvertToWireProtocol((int)value.Configuration.CompressionType);
				Compressor = CompressionCodecProvider.GetCompressionCodec((int)CompressionType);
				_maxNumMessagesInBatch = value.Configuration.BatchingMaxMessages;
				_maxBytesInBatch = value.Configuration.BatchingMaxBytes;
            }
		}
		public virtual bool HasSameTxn(Message<T> msg)
		{
			if (!msg.Metadata.ShouldSerializeTxnidMostBits() || !msg.Metadata.ShouldSerializeTxnidLeastBits())
			{
				return true;
			}
			if (CurrentTxnidMostBits == -1 || CurrentTxnidLeastBits == -1)
			{
				CurrentTxnidMostBits = (long)msg.Metadata.TxnidMostBits;
				CurrentTxnidLeastBits = (long)msg.Metadata.TxnidLeastBits;
				return true;
			}
			return CurrentTxnidMostBits == (long)msg.Metadata.TxnidMostBits && CurrentTxnidLeastBits == (long)msg.Metadata.TxnidLeastBits;
		}
		public int MaxBatchSize
        {
            get => _maxBatchSize;
            set => _maxBytesInBatch = value;

        }
        public virtual IActorRef Producer { get; set; }
    }

}