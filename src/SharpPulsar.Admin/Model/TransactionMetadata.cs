using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
	using Data = lombok.Data;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Data public class TransactionMetadata
	public class TransactionMetadata
	{

		/// <summary>
		/// The txnId of this transaction. </summary>
		public string TxnId;

		/// <summary>
		/// The status of this transaction. </summary>
		public string Status;

		/// <summary>
		/// The open time of this transaction. </summary>
		public long OpenTimestamp;

		/// <summary>
		/// The timeout of this transaction. </summary>
		public long TimeoutAt;

		/// <summary>
		/// The producedPartitions of this transaction. </summary>
		public IDictionary<string, TransactionInBufferStats> ProducedPartitions;

		/// <summary>
		/// The ackedPartitions of this transaction. </summary>
		public IDictionary<string, IDictionary<string, TransactionInPendingAckStats>> AckedPartitions;
	}

}