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
	/// Producer statistics recorded by client.
	/// 
	/// <para>All the stats are relative to the last recording period. The interval of the stats refreshes is configured with
	/// <seealso cref="IPulsarClientBuilder.statsInterval(long, java.util.concurrent.TimeUnit)"/> with a default of 1 minute.
	/// </para>
	/// </summary>
	public interface IProducerStats
	{
		/// <returns> the number of messages published in the last interval </returns>
		long NumMsgsSent {get;}

		/// <returns> the number of bytes sent in the last interval </returns>
		long NumBytesSent {get;}

		/// <returns> the number of failed send operations in the last interval </returns>
		long NumSendFailed {get;}

		/// <returns> the number of send acknowledges received by broker in the last interval </returns>
		long NumAcksReceived {get;}

		/// <returns> the messages send rate in the last interval </returns>
		double SendMsgsRate {get;}

		/// <returns> the bytes send rate in the last interval </returns>
		double SendBytesRate {get;}

		/// <returns> the 50th percentile of the send latency in milliseconds for the last interval </returns>
		double SendLatencyMillis50Pct {get;}

		/// <returns> the 75th percentile of the send latency in milliseconds for the last interval </returns>
		double SendLatencyMillis75Pct {get;}

		/// <returns> the 95th percentile of the send latency in milliseconds for the last interval </returns>
		double SendLatencyMillis95Pct {get;}

		/// <returns> the 99th percentile of the send latency in milliseconds for the last interval </returns>
		double SendLatencyMillis99Pct {get;}

		/// <returns> the 99.9th percentile of the send latency in milliseconds for the last interval </returns>
		double SendLatencyMillis999Pct {get;}

		/// <returns> the max send latency in milliseconds for the last interval </returns>
		double SendLatencyMillisMax {get;}

		/// <returns> the total number of messages published by this producer </returns>
		long TotalMsgsSent {get;}

		/// <returns> the total number of bytes sent by this producer </returns>
		long TotalBytesSent {get;}

		/// <returns> the total number of failed send operations </returns>
		long TotalSendFailed {get;}

		/// <returns> the total number of send acknowledges received by broker </returns>
		long TotalAcksReceived {get;}

	}

}