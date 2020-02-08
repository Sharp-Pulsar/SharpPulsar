using System;
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
	public class ProducerStatsDisabled<T> : IProducerStatsRecorder
	{
		private const long SerialVersionUid = 1L;

		internal static readonly IProducerStatsRecorder Instance = new ProducerStatsDisabled<T>();

		public void IncrementSendFailed()
		{
			// Do nothing
		}

		public void IncrementSendFailed(long numMsgs)
		{
			// Do nothing
		}

		public void IncrementNumAcksReceived(long latencyNs)
		{
			// Do nothing
		}

		public void UpdateNumMsgsSent(long numMsgs, long totalMsgsSize)
		{
			// Do nothing
		}

		public void CancelStatsTimeout()
		{
			// Do nothing
		}

		public virtual long NumMsgsSent => 0;

        public virtual long NumBytesSent => 0;

        public virtual long NumSendFailed => 0;

        public virtual long NumAcksReceived => 0;

        public virtual long TotalMsgsSent => 0;

        public virtual long TotalBytesSent => 0;

        public virtual long TotalSendFailed => 0;

        public virtual long TotalAcksReceived => 0;

        public virtual double SendMsgsRate => 0;

        public virtual double SendBytesRate => 0;

        public virtual double SendLatencyMillis50Pct => 0;

        public virtual double SendLatencyMillis75Pct => 0;

        public virtual double SendLatencyMillis95Pct => 0;

        public virtual double SendLatencyMillis99Pct => 0;

        public virtual double SendLatencyMillis999Pct => 0;

        public virtual double SendLatencyMillisMax => 0;
    }

}