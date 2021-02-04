using System;
using SharpPulsar.Stats.Producer.Api;

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
namespace SharpPulsar.Stats.Producer
{
	[Serializable]
	public class ProducerStatsDisabled : IProducerStatsRecorder
	{
		private const long SerialVersionUid = 1L;

		internal static readonly ProducerStatsDisabled Instance = new ProducerStatsDisabled();

		public virtual void IncrementSendFailed()
		{
			// Do nothing
		}

		public virtual void IncrementSendFailed(long numMsgs)
		{
			// Do nothing
		}

		public virtual void IncrementNumAcksReceived(long latencyNs)
		{
			// Do nothing
		}

		public virtual void UpdateNumMsgsSent(long numMsgs, long totalMsgsSize)
		{
			// Do nothing
		}

		public virtual void CancelStatsTimeout()
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

        public virtual double SendLatencyMillis50pct => 0;

        public virtual double SendLatencyMillis75pct => 0;

        public virtual double SendLatencyMillis95pct => 0;

        public virtual double SendLatencyMillis99pct => 0;

        public virtual double SendLatencyMillis999pct => 0;

        public virtual double SendLatencyMillisMax => 0;
    }

}