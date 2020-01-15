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
namespace SharpPulsar.Transaction
{
	

	/// <summary>
	/// A enum represents the status of a transaction.
	/// </summary>
	public sealed class TxnStatus
	{

		// A new transaction is open.
		public static readonly TxnStatus OPEN = new TxnStatus("OPEN", InnerEnum.OPEN);
		// A transaction is in the progress of committing.
		public static readonly TxnStatus COMMITTING = new TxnStatus("COMMITTING", InnerEnum.COMMITTING);
		// A transaction is already committed.
		public static readonly TxnStatus COMMITTED = new TxnStatus("COMMITTED", InnerEnum.COMMITTED);
		// A transaction is in the progress of aborting.
		public static readonly TxnStatus ABORTING = new TxnStatus("ABORTING", InnerEnum.ABORTING);
		// A transaction is already aborted.
		public static readonly TxnStatus ABORTED = new TxnStatus("ABORTED", InnerEnum.ABORTED);

		private static readonly IList<TxnStatus> valueList = new List<TxnStatus>();

		static TxnStatus()
		{
			valueList.Add(OPEN);
			valueList.Add(COMMITTING);
			valueList.Add(COMMITTED);
			valueList.Add(ABORTING);
			valueList.Add(ABORTED);
		}

		public enum InnerEnum
		{
			OPEN,
			COMMITTING,
			COMMITTED,
			ABORTING,
			ABORTED
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private TxnStatus(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Check if the a status can be transaction to a new status.
		/// </summary>
		/// <param name="newStatus"> the new status </param>
		/// <returns> true if the current status can be transitioning to. </returns>
		public bool canTransitionTo(TxnStatus newStatus)
		{
			TxnStatus currentStatus = this;

			switch (currentStatus.innerEnumValue)
			{
				case TxnStatus.InnerEnum.OPEN:
					return newStatus != COMMITTED && newStatus != ABORTED;
				case TxnStatus.InnerEnum.COMMITTING:
					return newStatus == COMMITTING || newStatus == COMMITTED;
				case TxnStatus.InnerEnum.COMMITTED:
					return newStatus == COMMITTED;
				case TxnStatus.InnerEnum.ABORTING:
					return newStatus == ABORTING || newStatus == ABORTED;
				case TxnStatus.InnerEnum.ABORTED:
					return newStatus == ABORTED;
				default:
					throw new System.ArgumentException("Unknown txn status : " + newStatus);
			}
		}


		public static IList<TxnStatus> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static TxnStatus valueOf(string name)
		{
			foreach (TxnStatus enumInstance in TxnStatus.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}