using SharpPulsar.Api;
using SharpPulsar.Common.Compression;
using System;
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
namespace SharpPulsar.Impl
{
	
	/// <summary>
	/// Batch message container framework.
	/// </summary>
	public abstract class AbstractBatchMessageContainer : BatchMessageContainerBase
	{
		public abstract bool MultiBatches {get;}
		public abstract void Discard(System.Exception Ex);
		public abstract bool Empty {get;}
		public abstract void Clear();
		public abstract bool HasSameSchema(MessageImpl<object> Msg);
		public abstract bool Add(MessageImpl<object> Msg, SendCallback Callback);

		protected internal ICompressionType CompressionType;
		protected internal CompressionCodec Compressor;
		protected internal string TopicName;
		protected internal string ProducerName;

		protected internal ProducerImpl<object> producer;

		protected internal int MaxNumMessagesInBatch;
		protected internal int MaxBytesInBatch;

		protected internal int NumMessagesInBatchConflict = 0;
		protected internal long CurrentBatchSizeBytes = 0;

		protected internal const int InitialBatchBufferSize = 1024;

		// This will be the largest size for a batch sent from this particular producer. This is used as a baseline to
		// allocate a new buffer that can hold the entire batch without needing costly reallocations
		protected internal int MaxBatchSize = InitialBatchBufferSize;

		public bool HaveEnoughSpace(MessageImpl<object> Msg)
		{
			int MessageSize = Msg.DataBuffer.ReadableBytes;
			return ((MaxBytesInBatch <= 0 && (MessageSize + CurrentBatchSizeBytes) <= ClientCnx.MaxMessageSize) || (MaxBytesInBatch > 0 && (MessageSize + CurrentBatchSizeBytes) <= MaxBytesInBatch)) && (MaxNumMessagesInBatch <= 0 || NumMessagesInBatchConflict < MaxNumMessagesInBatch);
		}

		public virtual bool BatchFull
		{
			get
			{
				return (MaxBytesInBatch > 0 && CurrentBatchSizeBytes >= MaxBytesInBatch) || (MaxBytesInBatch <= 0 && CurrentBatchSizeBytes >= ClientCnx.MaxMessageSize) || (MaxNumMessagesInBatch > 0 && NumMessagesInBatchConflict >= MaxNumMessagesInBatch);
			}
		}

		public virtual int NumMessagesInBatch
		{
			get
			{
				return NumMessagesInBatchConflict;
			}
		}

		public virtual long CurrentBatchSize
		{
			get
			{
				return CurrentBatchSizeBytes;
			}
		}
		public IList<ProducerImpl<object>.OpSendMsg> CreateOpSendMsgs()
		{
			throw new NotSupportedException();
		}

		public ProducerImpl<object>.OpSendMsg CreateOpSendMsg()
		{
			throw new NotSupportedException();
		}

		public virtual ProducerImpl<object> Producer
		{
			get
			{
				return producer;
			}
			set
			{
				producer = value;
				TopicName = value.Topic;
				ProducerName = value.ProducerName;
				CompressionType = (ICompressionType)CompressionCodecProvider.ConvertToWireProtocol(value.Configuration.CompressionType);
				Compressor = CompressionCodecProvider.GetCompressionCodec(CompressionType);
				MaxNumMessagesInBatch = value.Configuration.BatchingMaxMessages;
				MaxBytesInBatch = value.Configuration.BatchingMaxBytes;
			}
		}
	}

}