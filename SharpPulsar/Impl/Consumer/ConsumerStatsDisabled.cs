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

	using ConsumerStats = org.apache.pulsar.client.api.ConsumerStats;
	using Message = org.apache.pulsar.client.api.Message;

	using Timeout = io.netty.util.Timeout;

	public class ConsumerStatsDisabled : ConsumerStatsRecorder
	{
		private const long serialVersionUID = 1L;

		internal static readonly ConsumerStatsRecorder INSTANCE = new ConsumerStatsDisabled();

		public virtual void updateNumMsgsReceived<T1>(Message<T1> message)
		{
			// Do nothing
		}

		public virtual void incrementNumReceiveFailed()
		{
			// Do nothing
		}

		public virtual void incrementNumBatchReceiveFailed()
		{
			// Do nothing
		}

		public virtual void incrementNumAcksSent(long numAcks)
		{
			// Do nothing
		}

		public virtual void incrementNumAcksFailed()
		{
			// Do nothing
		}

		public override long NumMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public override long NumBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public override long NumAcksSent
		{
			get
			{
				return 0;
			}
		}

		public override long NumAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public override long NumReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public override long NumBatchReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public override long TotalMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public override long TotalBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public override long TotalReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public override long TotaBatchReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public override long TotalAcksSent
		{
			get
			{
				return 0;
			}
		}

		public override long TotalAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public override double RateMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public override double RateBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual Optional<Timeout> StatTimeout
		{
			get
			{
				return null;
			}
		}

		public virtual void reset()
		{
			// do nothing
		}

		public virtual void updateCumulativeStats(ConsumerStats stats)
		{
			// do nothing
		}
	}

}