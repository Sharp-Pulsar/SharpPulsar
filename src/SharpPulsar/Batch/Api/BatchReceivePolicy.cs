﻿using System;
using DotNetty.Common.Utilities;
using SharpPulsar.TimeUnit;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
namespace SharpPulsar.Batch.Api
{

	/// <summary>
	/// Configuration for message batch receive <seealso cref="ConsumerActor.batchReceive()"/> <seealso cref="ConsumerActor.batchReceiveAsync()"/>.
	/// 
	/// <para>Batch receive policy can limit the number and bytes of messages in a single batch, and can specify a timeout
	/// for waiting for enough messages for this batch.
	/// 
	/// </para>
	/// <para>This batch receive will be completed as long as any one of the
	/// conditions(has enough number of messages, has enough of Size of messages, wait timeout) is met.
	/// 
	/// </para>
	/// <para>Examples:
	/// 1.If set maxNumMessages = 10, maxSizeOfMessages = 1MB and without timeout, it
	/// means <seealso cref="ConsumerActor.batchReceive()"/> will always wait until there is enough messages.
	/// 2.If set maxNumberOfMessages = 0, maxNumBytes = 0 and timeout = 100ms, it
	/// means <seealso cref="ConsumerActor.batchReceive()"/> will waiting for 100ms whether or not there is enough messages.
	/// 
	/// </para>
	/// <para>Note:
	/// Must specify messages limitation(maxNumMessages, maxNumBytes) or wait timeout.
	/// Otherwise, <seealso cref="Messages"/> ingest <seealso cref="Message"/> will never end.
	/// 
	/// @since 2.4.1
	/// </para>
	/// </summary>
	[Serializable]
	public class BatchReceivePolicy
	{

        /// <summary>
        /// Default batch receive policy.
        /// 
        /// <para>Max number of messages: no limit
        /// Max number of bytes: 10MB
        /// Timeout: 100ms<p/>
        /// </para>
        /// </summary>
        public static readonly BatchReceivePolicy DefaultPolicy = new BatchReceivePolicy(-1, 10 * 1024 * 1024, 100, TimeUnit.TimeUnit.MILLISECONDS, true);
        public static readonly BatchReceivePolicy DefaultMultiTopicsDisablePolicy = new BatchReceivePolicy(
            -1, 10 * 1024 * 1024, 100, TimeUnit.TimeUnit.MILLISECONDS, false);

        private BatchReceivePolicy(int maxNumMessages, int maxNumBytes, int timeoutMs, TimeUnit.TimeUnit timeoutUnit,
                               bool messagesFromMultiTopicsEnabled)
		{
			MaxNumMessages = maxNumMessages;
			MaxNumBytes = maxNumBytes;
			_timeout = timeoutMs;
            _timeoutUnit = timeoutUnit;
            _messagesFromMultiTopicsEnabled = messagesFromMultiTopicsEnabled;
        }

		/// <summary>
		/// Max number of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		public virtual int MaxNumMessages {get;}

		/// <summary>
		/// Max bytes of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		public virtual int MaxNumBytes {get;}

		/// <summary>
		/// timeout for waiting for enough messages(enough number or enough bytes).
		/// </summary>
		private readonly int _timeout;
        private readonly TimeUnit.TimeUnit _timeoutUnit;

        
       /// If it is false, one time `batchReceive()` only can receive the single topic messages,
       /// the max messages and max size will not be strictly followed. (default: true).
     
        private readonly bool _messagesFromMultiTopicsEnabled;
		public virtual void Verify()
		{
			if (MaxNumMessages <= 0 && MaxNumBytes <= 0 && _timeout <= 0)
			{
				throw new ArgumentException("At least one of maxNumMessages, maxNumBytes, timeout must be specified.");
			}
			if (_timeout > 0 /*&& _timeoutUnit == null*/)
			{
				throw new ArgumentException("Must set timeout unit for timeout.");
			}
		}

		public virtual long TimeoutMs => _timeout;
        public bool IsMessagesFromMultiTopicsEnabled()
        {
            return _messagesFromMultiTopicsEnabled;
        }

        /// <summary>
		/// Builder of BatchReceivePolicy.
		/// </summary>
		public class Builder
		{
			private int _maxNumMessages;
			private int _maxNumBytes;
			private int _timeout;
            private TimeUnit.TimeUnit _timeoutUnit;
            private bool _messagesFromMultiTopicsEnabled = true;

            public virtual Builder MaxNumMessages(int maxNumMessages)
			{
				_maxNumMessages = maxNumMessages;
				return this;
			}

			public virtual Builder MaxNumBytes(int maxNumBytes)
			{
				_maxNumBytes = maxNumBytes;
				return this;
			}

			public virtual Builder Timeout(int timeout, TimeUnit.TimeUnit timeUnit)
			{
				_timeout = timeout;
                _timeoutUnit = timeUnit;
				return this;
			}
            public Builder MessagesFromMultiTopicsEnabled(bool messagesFromMultiTopicsEnabled)
            {
                _messagesFromMultiTopicsEnabled = messagesFromMultiTopicsEnabled;
                return this;
            }
            public virtual BatchReceivePolicy Build()
			{
				return new BatchReceivePolicy(_maxNumMessages, _maxNumBytes, _timeout, _timeoutUnit,
                    _messagesFromMultiTopicsEnabled);
			}
		}

		public static Builder GetBuilder()
		{
			return new Builder();
		}

		public override string ToString()
		{
			return "BatchReceivePolicy{" + "maxNumMessages=" + MaxNumMessages + ", maxNumBytes=" + MaxNumBytes + ", timeout=" + _timeout + ", timeoutUnit=" + _timeoutUnit + ", messagesFromMultiTopicsEnabled=" + _messagesFromMultiTopicsEnabled + '}';
		}
	}

}