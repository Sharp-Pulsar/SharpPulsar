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

	using ConsumerStats = SharpPulsar.Api.ConsumerStats;
	using SharpPulsar.Api;

	using Timeout = io.netty.util.Timeout;

	[Serializable]
	public class ConsumerStatsDisabled : ConsumerStatsRecorder
	{
		private const long SerialVersionUID = 1L;

		internal static readonly ConsumerStatsRecorder INSTANCE = new ConsumerStatsDisabled();

		public override void UpdateNumMsgsReceived<T1>(Message<T1> Message)
		{
			// Do nothing
		}

		public override void IncrementNumReceiveFailed()
		{
			// Do nothing
		}

		public override void IncrementNumBatchReceiveFailed()
		{
			// Do nothing
		}

		public override void IncrementNumAcksSent(long NumAcks)
		{
			// Do nothing
		}

		public override void IncrementNumAcksFailed()
		{
			// Do nothing
		}

		public virtual long NumMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumAcksSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long NumBatchReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotaBatchReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalAcksSent
		{
			get
			{
				return 0;
			}
		}

		public virtual long TotalAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public virtual double RateMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public virtual double RateBytesReceived
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

		public override void Reset()
		{
			// do nothing
		}

		public override void UpdateCumulativeStats(ConsumerStats Stats)
		{
			// do nothing
		}
	}

}