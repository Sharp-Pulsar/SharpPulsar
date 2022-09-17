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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	using Data = lombok.Data;

	/// <summary>
	/// Transaction coordinator stats.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public class TransactionCoordinatorStats
	public class TransactionCoordinatorStats
	{

		/// <summary>
		/// The state of this transaction metadataStore. </summary>
		public string State;

		/// <summary>
		/// The sequenceId of transaction metadataStore. </summary>
		public long LeastSigBits;

		/// <summary>
		/// The low water mark of transaction metadataStore. </summary>
		public long LowWaterMark;

		/// <summary>
		///  The total number of ongoing transactions in this transaction coordinator.
		/// </summary>
		public long OngoingTxnSize;
		// Start timestamp of  transaction coordinator recovery. 0L means no startup.
		public long RecoverStartTime;
		// End timestamp of transaction coordinator recovery. 0L means no startup.
		public long RecoverEndTime;
	}

}