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
	using PulsarApi = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi;
	using CompressionCodec = Org.Apache.Pulsar.Common.Compression.CompressionCodec;
	using CompressionCodecProvider = Org.Apache.Pulsar.Common.Compression.CompressionCodecProvider;


	/// <summary>
	/// Batch message container framework.
	/// </summary>
	public abstract class AbstractBatchMessageContainer : BatchMessageContainerBase
	{
		public abstract bool MultiBatches {get;}
		public abstract void Discard(Exception Ex);
		public abstract bool Empty {get;}
		public abstract void Clear();
		public abstract bool hasSameSchema<T1>(MessageImpl<T1> Msg);
		public abstract bool add<T1>(MessageImpl<T1> Msg, SendCallback Callback);

		protected internal PulsarApi.CompressionType CompressionType;
		protected internal CompressionCodec Compressor;
		protected internal string TopicName;
		protected internal string ProducerName;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal ProducerImpl ProducerConflict;

		protected internal int MaxNumMessagesInBatch;
		protected internal int MaxBytesInBatch;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int NumMessagesInBatchConflict = 0;
		protected internal long CurrentBatchSizeBytes = 0;

		protected internal const int InitialBatchBufferSize = 1024;

		// This will be the largest size for a batch sent from this particular producer. This is used as a baseline to
		// allocate a new buffer that can hold the entire batch without needing costly reallocations
		protected internal int MaxBatchSize = InitialBatchBufferSize;

		public override bool HaveEnoughSpace<T1>(MessageImpl<T1> Msg)
		{
			int MessageSize = Msg.DataBuffer.readableBytes();
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public java.util.List<ProducerImpl.OpSendMsg> createOpSendMsgs() throws java.io.IOException
		public override IList<ProducerImpl.OpSendMsg> CreateOpSendMsgs()
		{
			throw new System.NotSupportedException();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public ProducerImpl.OpSendMsg createOpSendMsg() throws java.io.IOException
		public override ProducerImpl.OpSendMsg CreateOpSendMsg()
		{
			throw new System.NotSupportedException();
		}

		public virtual ProducerImpl<T1> Producer<T1>
		{
			set
			{
				this.ProducerConflict = value;
				this.TopicName = value.Topic;
				this.ProducerName = value.ProducerName;
				this.CompressionType = CompressionCodecProvider.convertToWireProtocol(value.Configuration.CompressionType);
				this.Compressor = CompressionCodecProvider.getCompressionCodec(CompressionType);
				this.MaxNumMessagesInBatch = value.Configuration.BatchingMaxMessages;
				this.MaxBytesInBatch = value.Configuration.BatchingMaxBytes;
			}
		}
	}

}