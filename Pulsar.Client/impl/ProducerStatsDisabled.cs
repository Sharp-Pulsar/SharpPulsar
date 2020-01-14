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
	public class ProducerStatsDisabled : ProducerStatsRecorder
	{
		private const long serialVersionUID = 1L;

		internal static readonly ProducerStatsRecorder INSTANCE = new ProducerStatsDisabled();

		public virtual void incrementSendFailed()
		{
			// Do nothing
		}

		public virtual void incrementSendFailed(long numMsgs)
		{
			// Do nothing
		}

		public virtual void incrementNumAcksReceived(long latencyNs)
		{
			// Do nothing
		}

		public virtual void updateNumMsgsSent(long numMsgs, long totalMsgsSize)
		{
			// Do nothing
		}

		public virtual void cancelStatsTimeout()
		{
			// Do nothing
		}

		public override long NumMsgsSent
		{
			get
			{
				return 0;
			}
		}

		public override long NumBytesSent
		{
			get
			{
				return 0;
			}
		}

		public override long NumSendFailed
		{
			get
			{
				return 0;
			}
		}

		public override long NumAcksReceived
		{
			get
			{
				return 0;
			}
		}

		public override long TotalMsgsSent
		{
			get
			{
				return 0;
			}
		}

		public override long TotalBytesSent
		{
			get
			{
				return 0;
			}
		}

		public override long TotalSendFailed
		{
			get
			{
				return 0;
			}
		}

		public override long TotalAcksReceived
		{
			get
			{
				return 0;
			}
		}

		public override double SendMsgsRate
		{
			get
			{
				return 0;
			}
		}

		public override double SendBytesRate
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillis50pct
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillis75pct
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillis95pct
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillis99pct
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillis999pct
		{
			get
			{
				return 0;
			}
		}

		public override double SendLatencyMillisMax
		{
			get
			{
				return 0;
			}
		}
	}

}