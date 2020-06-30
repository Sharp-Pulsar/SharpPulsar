using System.Collections.Generic;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Impl;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Tracker.Api;
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
namespace SharpPulsar.Tracker
{
    /// <summary>
    /// A no-op acknowledgment grouping tracker.
    /// </summary>
    public class NonPersistentAcknowledgmentGroupingTracker : IAcknowledgmentsGroupingTracker
    {

        public static NonPersistentAcknowledgmentGroupingTracker Of()
        {
            return Instance;
        }

        private static readonly NonPersistentAcknowledgmentGroupingTracker Instance = new NonPersistentAcknowledgmentGroupingTracker();

        private NonPersistentAcknowledgmentGroupingTracker()
        {
        }

        public virtual bool IsDuplicate(IMessageId messageId)
        {
            return false;
        }

        public virtual void AddAcknowledgment(IMessageId msgId, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
            // no-op
        }

        public virtual void AddBatchIndexAcknowledgment(BatchMessageId msgId, int batchIndex, int BatchSize, CommandAck.AckType ackType, IDictionary<string, long> properties)
        {
            // no-op
        }

        public virtual void Flush()
        {
            // no-op
        }

        public virtual void Close()
        {
            // no-op
        }

        public virtual void FlushAndClean()
        {
            // no-op
        }
	}
}
