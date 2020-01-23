using Optional;
using SharpPulsar.Interface.Consumer;
using SharpPulsar.Interface.Message;
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
	public class ConsumerStatsDisabled : ConsumerStatsRecorder
	{
		private const long serialVersionUID = 1L;

		internal static readonly ConsumerStatsRecorder INSTANCE = new ConsumerStatsDisabled();

		public virtual void UpdateNumMsgsReceived<T1>(IMessage<T1> message)
		{
			// Do nothing
		}

		public virtual void IncrementNumReceiveFailed()
		{
			// Do nothing
		}

		public virtual void IncrementNumBatchReceiveFailed()
		{
			// Do nothing
		}

		public virtual void IncrementNumAcksSent(long numAcks)
		{
			// Do nothing
		}

		public virtual void IncrementNumAcksFailed()
		{
			// Do nothing
		}

		public long NumMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public long NumBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public long NumAcksSent
		{
			get
			{
				return 0;
			}
		}

		public long NumAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public long NumReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public long NumBatchReceiveFailed
		{
			get
			{
				return 0;
			}
		}

		public long TotalMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public long TotalBytesReceived
		{
			get
			{
				return 0;
			}
		}

		public long TotalReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public long TotaBatchReceivedFailed
		{
			get
			{
				return 0;
			}
		}

		public long TotalAcksSent
		{
			get
			{
				return 0;
			}
		}

		public long TotalAcksFailed
		{
			get
			{
				return 0;
			}
		}

		public double RateMsgsReceived
		{
			get
			{
				return 0;
			}
		}

		public double RateBytesReceived
		{
			get
			{
				return 0;
			}
		}


		Option<Timeout> ConsumerStatsRecorder.StatTimeout => throw new System.NotImplementedException();

		public virtual void Reset()
		{
			// do nothing
		}

		public virtual void UpdateCumulativeStats(IConsumerStats stats)
		{
			// do nothing
		}
	}

}