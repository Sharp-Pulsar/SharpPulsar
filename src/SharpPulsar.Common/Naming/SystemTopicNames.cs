using System;
using System.Collections.Generic;

namespace SharpPulsar.Common.Naming
{
    internal class SystemTopicNames
    {
        /// <summary>
		/// Local topic name for the namespace events.
		/// </summary>
		public const string NamespaceEventsLocalName = "__change_events";

        /// <summary>
        /// Local topic name for the transaction buffer snapshot.
        /// </summary>
        public const string TransactionBufferSnapshot = "__transaction_buffer_snapshot";


        public const string PendingAckStoreSuffix = "__transaction_pending_ack";

        public const string PendingAckStoreCursorName = "__pending_ack_state";

        /// <summary>
        /// The set of all local topic names declared above.
        /// </summary>
        public static readonly ISet<string> EventsTopicNames = new HashSet<string> { NamespaceEventsLocalName, TransactionBufferSnapshot };


        public static readonly TopicName TransactionCoordinatorAssign = TopicName.Get(TopicDomain.Persistent.Value(), NamespaceName.SystemNamespace, "transaction_coordinator_assign");

        public static readonly TopicName TransactionCoordinatorLog = TopicName.Get(TopicDomain.Persistent.Value(), NamespaceName.SystemNamespace, "__transaction_log_");

        public static readonly TopicName ResourceUsageTopic = TopicName.Get(TopicDomain.NonPersistent.Value(), NamespaceName.SystemNamespace, "resource-usage");

        public static bool IsEventSystemTopic(TopicName topicName)
        {
            return EventsTopicNames.Contains(TopicName.Get(topicName.PartitionedTopicName).LocalName);
        }

        public static bool IsTransactionCoordinatorAssign(TopicName TopicName)
        {
            return TopicName != null && TopicName.ToString().StartsWith(TransactionCoordinatorAssign.ToString(), StringComparison.Ordinal);
        }

        public static bool IsTopicPoliciesSystemTopic(string topic)
        {
            if (ReferenceEquals(topic, null))
            {
                return false;
            }
            return TopicName.Get(topic).LocalName.Equals(NamespaceEventsLocalName);
        }

        public static bool IsTransactionInternalName(TopicName topicName)
        {
            string Topic = topicName.ToString();
            return Topic.StartsWith(TransactionCoordinatorAssign.ToString(), StringComparison.Ordinal) || Topic.StartsWith(TransactionCoordinatorLog.ToString(), StringComparison.Ordinal) || Topic.EndsWith(PendingAckStoreSuffix, StringComparison.Ordinal);
        }

        public static bool IsSystemTopic(TopicName topicName)
        {
            TopicName NonePartitionedTopicName = TopicName.Get(topicName.PartitionedTopicName);
            return IsEventSystemTopic(NonePartitionedTopicName) || IsTransactionInternalName(NonePartitionedTopicName);
        }

    }
}
