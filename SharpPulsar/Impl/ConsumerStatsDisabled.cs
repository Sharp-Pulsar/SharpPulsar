using System;
using System.Threading;
using DotNetty.Common.Utilities;
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

	[Serializable]
	public class ConsumerStatsDisabled : ConsumerStatsRecorder
	{
		private const long SerialVersionUid = 1L;

		internal static readonly ConsumerStatsRecorder Instance = new ConsumerStatsDisabled();

		public void UpdateNumMsgsReceived<T1>(Message<T1> message)
		{
			// Do nothing
		}

		public void IncrementNumReceiveFailed()
		{
			// Do nothing
		}

		public void IncrementNumBatchReceiveFailed()
		{
			// Do nothing
		}

		public void IncrementNumAcksSent(long numAcks)
		{
			// Do nothing
		}

		public void IncrementNumAcksFailed()
		{
			// Do nothing
		}

		public virtual long NumMsgsReceived => 0;

        public virtual long NumBytesReceived => 0;

        public virtual long NumAcksSent => 0;

        public virtual long NumAcksFailed => 0;

        public virtual long NumReceiveFailed => 0;

        public virtual long NumBatchReceiveFailed => 0;

        public virtual long TotalMsgsReceived => 0;

        public virtual long TotalBytesReceived => 0;

        public virtual long TotalReceivedFailed => 0;

        public virtual long TotaBatchReceivedFailed => 0;

        public virtual long TotalAcksSent => 0;

        public virtual long TotalAcksFailed => 0;

        public virtual double RateMsgsReceived => 0;

        public virtual double RateBytesReceived => 0;

        public virtual ITimeout StatTimeout => null;

        public void Reset()
		{
			// do nothing
		}

		public void UpdateCumulativeStats(IConsumerStats stats)
		{
			// do nothing
		}
	}

}