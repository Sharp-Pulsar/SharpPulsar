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
namespace SharpPulsar.TransactionImpl
{


    /// <summary>
    /// A enum represents the status of a transaction.
    /// </summary>
    public sealed class TxnStatus
    {

        // A new transaction is open.
        public static readonly TxnStatus Open = new TxnStatus("OPEN", InnerEnum.Open);
        // A transaction is in the progress of committing.
        public static readonly TxnStatus Committing = new TxnStatus("COMMITTING", InnerEnum.Committing);
        // A transaction is already committed.
        public static readonly TxnStatus Committed = new TxnStatus("COMMITTED", InnerEnum.Committed);
        // A transaction is in the progress of aborting.
        public static readonly TxnStatus Aborting = new TxnStatus("ABORTING", InnerEnum.Aborting);
        // A transaction is already aborted.
        public static readonly TxnStatus Aborted = new TxnStatus("ABORTED", InnerEnum.Aborted);

        private static readonly IList<TxnStatus> ValueList = new List<TxnStatus>();

        static TxnStatus()
        {
            ValueList.Add(Open);
            ValueList.Add(Committing);
            ValueList.Add(Committed);
            ValueList.Add(Aborting);
            ValueList.Add(Aborted);
        }

        public enum InnerEnum
        {
            Open,
            Committing,
            Committed,
            Aborting,
            Aborted
        }

        public readonly InnerEnum InnerEnumValue;
        private readonly string _nameValue;
        private readonly int _ordinalValue;
        private static int _nextOrdinal = 0;

        private TxnStatus(string name, InnerEnum innerEnum)
        {
            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        /// <summary>
        /// Check if the a status can be transaction to a new status.
        /// </summary>
        /// <param name="newStatus"> the new status </param>
        /// <returns> true if the current status can be transitioning to. </returns>
        public bool canTransitionTo(TxnStatus newStatus)
        {
            var currentStatus = this;

            switch (currentStatus.InnerEnumValue)
            {
                case InnerEnum.Open:
                    return newStatus != Committed && newStatus != Aborted;
                case InnerEnum.Committing:
                    return newStatus == Committing || newStatus == Committed;
                case InnerEnum.Committed:
                    return newStatus == Committed;
                case InnerEnum.Aborting:
                    return newStatus == Aborting || newStatus == Aborted;
                case InnerEnum.Aborted:
                    return newStatus == Aborted;
                default:
                    throw new System.ArgumentException("Unknown txn status : " + newStatus);
            }
        }


        public static IList<TxnStatus> values()
        {
            return ValueList;
        }

        public int ordinal()
        {
            return _ordinalValue;
        }

        public override string ToString()
        {
            return _nameValue;
        }

        public static TxnStatus valueOf(string name)
        {
            foreach (var enumInstance in ValueList)
            {
                if (enumInstance._nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }

}