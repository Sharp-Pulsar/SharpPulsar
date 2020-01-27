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

		private BatchReceivePolicy(int MaxNumMessages, int MaxNumBytes, int Timeout, TimeUnit TimeoutUnit)
		{
			this.MaxNumMessages = MaxNumMessages;
			this.maxNumBytes = MaxNumBytes;
			this.timeout = Timeout;
			this.timeoutUnit = TimeoutUnit;
		}

		/// <summary>
		/// Max number of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		public virtual MaxNumMessages {get;}

		/// <summary>
		/// Max bytes of messages for a single batch receive, 0 or negative means no limit.
		/// </summary>
		private readonly int maxNumBytes;

		/// <summary>
		/// timeout for waiting for enough messages(enough number or enough bytes).
		/// </summary>
		private readonly int timeout;
		private readonly TimeUnit timeoutUnit;

		public virtual void Verify()
		{
			if (MaxNumMessages <= 0 && maxNumBytes <= 0 && timeout <= 0)
			{
				throw new System.ArgumentException("At least " + "one of maxNumMessages, maxNumBytes, timeout must be specified.");
			}
			if (timeout > 0 && timeoutUnit == null)
			{
				throw new System.ArgumentException("Must set timeout unit for timeout.");
			}
		}

		public virtual long TimeoutMs
		{
			get
			{
				return (timeout > 0 && timeoutUnit != null) ? timeoutUnit.toMillis(timeout) : 0L;
			}
		}


		public virtual long MaxNumBytes
		{
			get
			{
				return maxNumBytes;
			}
		}

		/// <summary>
		/// Builder of BatchReceivePolicy.
		/// </summary>
		public class Builder
		{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int MaxNumMessagesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int MaxNumBytesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal int TimeoutConflict;
			internal TimeUnit TimeoutUnit;

			public virtual Builder MaxNumMessages(int MaxNumMessages)
			{
				this.MaxNumMessagesConflict = MaxNumMessages;
				return this;
			}

			public virtual Builder MaxNumBytes(int MaxNumBytes)
			{
				this.MaxNumBytesConflict = MaxNumBytes;
				return this;
			}

			public virtual Builder Timeout(int Timeout, TimeUnit TimeoutUnit)
			{
				this.TimeoutConflict = Timeout;
				this.TimeoutUnit = TimeoutUnit;
				return this;
			}

			public virtual BatchReceivePolicy Build()
			{
				return new BatchReceivePolicy(MaxNumMessagesConflict, MaxNumBytesConflict, TimeoutConflict, TimeoutUnit);
			}
		}

		public static Builder Builder()
		{
			return new Builder();
		}

		public override string ToString()
		{
			return "BatchReceivePolicy{" + "maxNumMessages=" + MaxNumMessages + ", maxNumBytes=" + maxNumBytes + ", timeout=" + timeout + ", timeoutUnit=" + timeoutUnit + '}';
		}
	}

}