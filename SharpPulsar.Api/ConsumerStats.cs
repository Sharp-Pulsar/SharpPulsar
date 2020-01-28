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
	/// Consumer statistics recorded by client.
	/// 
	/// <para>All the stats are relative to the last recording period. The interval of the stats refreshes is configured with
	/// <seealso cref="IPulsarClientBuilder.statsInterval(long, java.util.concurrent.TimeUnit)"/> with a default of 1 minute.
	/// </para>
	/// </summary>
	public interface ConsumerStats
	{

		/// <returns> Number of messages received in the last interval </returns>
		long NumMsgsReceived {get;}

		/// <returns> Number of bytes received in the last interval </returns>
		long NumBytesReceived {get;}

		/// <returns> Rate of bytes per second received in the last interval </returns>
		double RateMsgsReceived {get;}

		/// <returns> Rate of bytes per second received in the last interval </returns>
		double RateBytesReceived {get;}

		/// <returns> Number of message acknowledgments sent in the last interval </returns>
		long NumAcksSent {get;}

		/// <returns> Number of message acknowledgments failed in the last interval </returns>
		long NumAcksFailed {get;}

		/// <returns> Number of message receive failed in the last interval </returns>
		long NumReceiveFailed {get;}

		/// <returns> Number of message batch receive failed in the last interval </returns>
		long NumBatchReceiveFailed {get;}

		/// <returns> Total number of messages received by this consumer </returns>
		long TotalMsgsReceived {get;}

		/// <returns> Total number of bytes received by this consumer </returns>
		long TotalBytesReceived {get;}

		/// <returns> Total number of messages receive failures </returns>
		long TotalReceivedFailed {get;}

		/// <returns> Total number of messages batch receive failures </returns>
		long TotaBatchReceivedFailed {get;}

		/// <returns> Total number of message acknowledgments sent by this consumer </returns>
		long TotalAcksSent {get;}

		/// <returns> Total number of message acknowledgments failures on this consumer </returns>
		long TotalAcksFailed {get;}
	}

}