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

using BAMCIS.Util.Concurrent;

namespace SharpPulsar.Api
{

	/// <summary>
	/// Configuration for message batch receive <seealso cref="IConsumer.batchReceive()"/> <seealso cref="IConsumer.batchReceiveAsync()"/>.
	/// 
	/// <para>Batch receive policy can limit the number and bytes of messages in a single batch, and can specify a timeout
	/// for waiting for enough messages for this batch.
	/// 
	/// </para>
	/// <para>This batch receive will be completed as long as any one of the
	/// conditions(has enough number of messages, has enough of size of messages, wait timeout) is met.
	/// 
	/// </para>
	/// <para>Examples:
	/// 1.If set maxNumMessages = 10, maxSizeOfMessages = 1MB and without timeout, it
	/// means <seealso cref="IConsumer.batchReceive()"/> will always wait until there is enough messages.
	/// 2.If set maxNumberOfMessages = 0, maxNumBytes = 0 and timeout = 100ms, it
	/// means <seealso cref="IConsumer.batchReceive()"/> will waiting for 100ms whether or not there is enough messages.
	/// 
	/// </para>
	/// <para>Note:
	/// Must specify messages limitation(maxNumMessages, maxNumBytes) or wait timeout.
	/// Otherwise, <seealso cref="Messages"/> ingest <seealso cref="Message"/> will never end.
	/// 
	/// @since 2.4.1
	/// </para>
	/// </summary>
	public class BatchReceivePolicy
	{

		/// <summary>
		/// Default batch receive policy.
		/// 
		/// <para>Max number of messages: 100
		/// Max number of bytes: 10MB
		/// Timeout: 100ms<p/>
		/// </para>
		/// </summary>
		public static readonly BatchReceivePolicy DefaultPolicy = new BatchReceivePolicy(-1, 10 * 1024 * 1024, 100, TimeUnit.MILLISECONDS);

		private BatchReceivePolicy(int maxNumMessages, int maxNumBytes, int timeout, TimeUnit timeoutUnit)
		{
			this.MaxNumMessages = maxNumMessages;
			this._maxNumBytes = maxNumBytes;
			this._timeout = timeout;
			this._timeoutUnit = timeoutUnit;
		}

		/// <summary>
		/// Max number of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		public virtual long MaxNumMessages {get;}

		/// <summary>
		/// Max bytes of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		private readonly int _maxNumBytes;

		/// <summary>
		/// timeout for waiting for enough messages(enough number or enough bytes).
		/// </summary>
		private readonly int _timeout;
		private readonly TimeUnit _timeoutUnit;

		public virtual void Verify()
		{
			if (MaxNumMessages <= 0 && _maxNumBytes <= 0 && _timeout <= 0)
			{
				throw new System.ArgumentException("At least " + "one of maxNumMessages, maxNumBytes, timeout must be specified.");
			}
			if (_timeout > 0 && _timeoutUnit == null)
			{
				throw new System.ArgumentException("Must set timeout unit for timeout.");
			}
		}

		public virtual long TimeoutMs => (_timeout > 0 && _timeoutUnit != null) ? _timeoutUnit.ToMilliseconds(_timeout) : 0L;


        public virtual long MaxNumBytes => _maxNumBytes;

        /// <summary>
		/// Builder of BatchReceivePolicy.
		/// </summary>
		public class Builder
		{
			internal int _MaxNumMessages;
			internal int _MaxNumBytes;
			internal int _Timeout;
			internal TimeUnit TimeoutUnit;

			public virtual Builder MaxNumMessages(int maxNumMessages)
			{
				this._MaxNumMessages = maxNumMessages;
				return this;
			}

			public virtual Builder MaxNumBytes(int maxNumBytes)
			{
				this._MaxNumBytes = maxNumBytes;
				return this;
			}

			public virtual Builder Timeout(int timeout, TimeUnit timeoutUnit)
			{
				this._Timeout = timeout;
				this.TimeoutUnit = timeoutUnit;
				return this;
			}

			public virtual BatchReceivePolicy Build()
			{
				return new BatchReceivePolicy(_MaxNumMessages, _MaxNumBytes, _Timeout, TimeoutUnit);
			}
		}

		public static Builder Build()
		{
			return new Builder();
		}

		public override string ToString()
		{
			return "BatchReceivePolicy{" + "maxNumMessages=" + MaxNumMessages + ", maxNumBytes=" + _maxNumBytes + ", timeout=" + _timeout + ", timeoutUnit=" + _timeoutUnit + '}';
		}
	}

}