using System.Collections.Generic;

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
namespace org.apache.pulsar.client.impl
{
	using PulsarApi = org.apache.pulsar.common.api.proto.PulsarApi;
	using CompressionCodec = org.apache.pulsar.common.compression.CompressionCodec;
	using CompressionCodecProvider = org.apache.pulsar.common.compression.CompressionCodecProvider;


	/// <summary>
	/// Batch message container framework.
	/// </summary>
	public abstract class AbstractBatchMessageContainer : BatchMessageContainerBase
	{
		public abstract bool hasSameSchema<T1>(MessageImpl<T1> msg);
		public abstract bool add<T1>(MessageImpl<T1> msg, SendCallback callback);

		protected internal PulsarApi.CompressionType compressionType;
		protected internal CompressionCodec compressor;
		protected internal string topicName;
		protected internal string producerName;
		protected internal ProducerImpl producer;

		protected internal int maxNumMessagesInBatch;
		protected internal int maxBytesInBatch;
		protected internal int numMessagesInBatch = 0;
		protected internal long currentBatchSizeBytes = 0;

		protected internal const int INITIAL_BATCH_BUFFER_SIZE = 1024;

		// This will be the largest size for a batch sent from this particular producer. This is used as a baseline to
		// allocate a new buffer that can hold the entire batch without needing costly reallocations
		protected internal int maxBatchSize = INITIAL_BATCH_BUFFER_SIZE;

		public virtual bool haveEnoughSpace<T1>(MessageImpl<T1> msg)
		{
			int messageSize = msg.DataBuffer.readableBytes();
			return ((maxBytesInBatch <= 0 && (messageSize + currentBatchSizeBytes) <= ClientCnx.MaxMessageSize) || (maxBytesInBatch > 0 && (messageSize + currentBatchSizeBytes) <= maxBytesInBatch)) && (maxNumMessagesInBatch <= 0 || numMessagesInBatch < maxNumMessagesInBatch);
		}

		protected internal virtual bool BatchFull
		{
			get
			{
				return (maxBytesInBatch > 0 && currentBatchSizeBytes >= maxBytesInBatch) || (maxBytesInBatch <= 0 && currentBatchSizeBytes >= ClientCnx.MaxMessageSize) || (maxNumMessagesInBatch > 0 && numMessagesInBatch >= maxNumMessagesInBatch);
			}
		}

		public override int NumMessagesInBatch
		{
			get
			{
				return numMessagesInBatch;
			}
		}

		public override long CurrentBatchSize
		{
			get
			{
				return currentBatchSizeBytes;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<ProducerImpl.OpSendMsg> createOpSendMsgs() throws java.io.IOException
		public virtual IList<ProducerImpl.OpSendMsg> createOpSendMsgs()
		{
			throw new System.NotSupportedException();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public ProducerImpl.OpSendMsg createOpSendMsg() throws java.io.IOException
		public virtual ProducerImpl.OpSendMsg createOpSendMsg()
		{
			throw new System.NotSupportedException();
		}

		public virtual ProducerImpl<T1> Producer<T1>
		{
			set
			{
				this.producer = value;
				this.topicName = value.Topic;
				this.producerName = value.ProducerName;
				this.compressionType = CompressionCodecProvider.convertToWireProtocol(value.Configuration.CompressionType);
				this.compressor = CompressionCodecProvider.getCompressionCodec(compressionType);
				this.maxNumMessagesInBatch = value.Configuration.BatchingMaxMessages;
				this.maxBytesInBatch = value.Configuration.BatchingMaxBytes;
			}
		}
	}

}