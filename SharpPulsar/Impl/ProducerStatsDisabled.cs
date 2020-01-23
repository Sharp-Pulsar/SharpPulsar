using System;

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
	public class ProducerStatsDisabled : ProducerStatsRecorder
	{
		private const long SerialVersionUID = 1L;

		internal static readonly ProducerStatsRecorder INSTANCE = new ProducerStatsDisabled();

		public override void IncrementSendFailed()
		{
			// Do nothing
		}

		public override void IncrementSendFailed(long NumMsgs)
		{
			// Do nothing
		}

		public override void IncrementNumAcksReceived(long LatencyNs)
		{
			// Do nothing
		}

		public override void UpdateNumMsgsSent(long NumMsgs, long TotalMsgsSize)
		{
			// Do nothing
		}

		public override void CancelStatsTimeout()
		{
			// Do nothing
		}

		public virtual long NumMsgsSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumBytesSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumSendFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumAcksReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalMsgsSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalBytesSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalSendFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalAcksReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendMsgsRate
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendBytesRate
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillis50pct
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillis75pct
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillis95pct
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillis99pct
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillis999pct
		{
			get
			{
				return 0;
			}
		}

		public virtual double SendLatencyMillisMax
		{
			get
			{
				return 0;
			}
		}
	}

}