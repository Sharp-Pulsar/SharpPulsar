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

	using ConsumerStats = Api.ConsumerStats;
	using SharpPulsar.Api;

	using Timeout = io.netty.util.Timeout;

	public interface ConsumerStatsRecorder : ConsumerStats
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void updateNumMsgsReceived(SharpPulsar.api.Message<?> message);
		void updateNumMsgsReceived<T1>(Message<T1> Message);

		void IncrementNumAcksSent(long NumAcks);

		void IncrementNumAcksFailed();

		void IncrementNumReceiveFailed();

		void IncrementNumBatchReceiveFailed();

		Optional<Timeout> StatTimeout {get;}

		void Reset();

		void UpdateCumulativeStats(ConsumerStats Stats);
	}

}