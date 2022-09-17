using System;
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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	using ToString = lombok.ToString;
	using DispatchRateImpl = Org.Apache.Pulsar.Common.Policies.Data.Impl.DispatchRateImpl;

	/// <summary>
	/// Definition of Pulsar policies.
	/// </summary>
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @ToString public class Policies
	public class Policies
	{
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public final AuthPolicies auth_policies = AuthPolicies.builder().build();
		public readonly AuthPolicies AuthPolicies = AuthPolicies.builder().build();
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public java.util.Set<String> replication_clusters = new java.util.HashSet<>();
		public ISet<string> ReplicationClusters = new HashSet<string>();
		public BundlesData Bundles;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public java.util.Map<BacklogQuota_BacklogQuotaType, BacklogQuota> backlog_quota_map = new java.util.HashMap<>();
		public IDictionary<BacklogQuotaBacklogQuotaType, BacklogQuota> BacklogQuotaMap = new Dictionary<BacklogQuotaBacklogQuotaType, BacklogQuota>();
		[Obsolete]
		public IDictionary<string, DispatchRateImpl> ClusterDispatchRate = new Dictionary<string, DispatchRateImpl>();
		public IDictionary<string, DispatchRateImpl> TopicDispatchRate = new Dictionary<string, DispatchRateImpl>();
		public IDictionary<string, DispatchRateImpl> SubscriptionDispatchRate = new Dictionary<string, DispatchRateImpl>();
		public IDictionary<string, DispatchRateImpl> ReplicatorDispatchRate = new Dictionary<string, DispatchRateImpl>();
		public IDictionary<string, SubscribeRate> ClusterSubscribeRate = new Dictionary<string, SubscribeRate>();
		public PersistencePolicies Persistence = null;

		// If set, it will override the broker settings for enabling deduplication
		public bool? DeduplicationEnabled = null;
		// If set, it will override the broker settings for allowing auto topic creation
		public AutoTopicCreationOverride AutoTopicCreationOverride = null;
		// If set, it will override the broker settings for allowing auto subscription creation
		public AutoSubscriptionCreationOverride AutoSubscriptionCreationOverride = null;
		public IDictionary<string, PublishRate> PublishMaxMessageRate = new Dictionary<string, PublishRate>();

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public java.util.Map<String, int> latency_stats_sample_rate = new java.util.HashMap<>();
		public IDictionary<string, int> LatencyStatsSampleRate = new Dictionary<string, int>();
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> message_ttl_in_seconds = null;
		public int? MessageTtlInSeconds = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> subscription_expiration_time_minutes = null;
		public int? SubscriptionExpirationTimeMinutes = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public RetentionPolicies retention_policies = null;
		public RetentionPolicies RetentionPolicies = null;
		public bool Deleted = false;
		public const string FirstBoundary = "0x00000000";
		public const string LastBoundary = "0xffffffff";

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public boolean encryption_required = false;
		public bool EncryptionRequired = false;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public DelayedDeliveryPolicies delayed_delivery_policies = null;
		public DelayedDeliveryPolicies DelayedDeliveryPolicies = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public InactiveTopicPolicies inactive_topic_policies = null;
		public InactiveTopicPolicies InactiveTopicPolicies = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public SubscriptionAuthMode subscription_auth_mode = SubscriptionAuthMode.None;
		public SubscriptionAuthMode SubscriptionAuthMode = SubscriptionAuthMode.None;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_producers_per_topic = null;
		public int? MaxProducersPerTopic = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_consumers_per_topic = null;
		public int? MaxConsumersPerTopic = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_consumers_per_subscription = null;
		public int? MaxConsumersPerSubscription = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_unacked_messages_per_consumer = null;
		public int? MaxUnackedMessagesPerConsumer = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_unacked_messages_per_subscription = null;
		public int? MaxUnackedMessagesPerSubscription = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_subscriptions_per_topic = null;
		public int? MaxSubscriptionsPerTopic = null;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<long> compaction_threshold = null;
		public long? CompactionThreshold = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public long offload_threshold = -1;
		public long OffloadThreshold = -1;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<long> offload_deletion_lag_ms = null;
		public long? OffloadDeletionLagMs = null;
// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<int> max_topics_per_namespace = null;
		public int? MaxTopicsPerNamespace = null;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") @Deprecated public SchemaAutoUpdateCompatibilityStrategy schema_auto_update_compatibility_strategy = null;
		[Obsolete]
		public SchemaAutoUpdateCompatibilityStrategy SchemaAutoUpdateCompatibilityStrategy = null;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public SchemaCompatibilityStrategy schema_compatibility_strategy = SchemaCompatibilityStrategy.UNDEFINED;
		public SchemaCompatibilityStrategy SchemaCompatibilityStrategy = SchemaCompatibilityStrategy.UNDEFINED;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public System.Nullable<bool> is_allow_auto_update_schema = null;
		public bool? IsAllowAutoUpdateSchema = null;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public boolean schema_validation_enforced = false;
		public bool SchemaValidationEnforced = false;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public OffloadPolicies offload_policies = null;
		public OffloadPolicies OffloadPolicies = null;

		public int? DeduplicationSnapshotIntervalSeconds = null;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public java.util.Set<String> subscription_types_enabled = new java.util.HashSet<>();
		public ISet<string> SubscriptionTypesEnabled = new HashSet<string>();

		public IDictionary<string, string> Properties = new Dictionary<string, string>();

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public String resource_group_name = null;
		public string ResourceGroupName = null;

		public enum BundleType
		{
			LARGEST,
			HOT
		}

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @SuppressWarnings("checkstyle:MemberName") public EntryFilters entryFilters = null;
		public EntryFilters EntryFilters = null;

		public override int GetHashCode()
		{
			return Objects.hash(AuthPolicies, ReplicationClusters, BacklogQuotaMap, PublishMaxMessageRate, ClusterDispatchRate, TopicDispatchRate, SubscriptionDispatchRate, ReplicatorDispatchRate, ClusterSubscribeRate, DeduplicationEnabled, AutoTopicCreationOverride, AutoSubscriptionCreationOverride, Persistence, Bundles, LatencyStatsSampleRate, MessageTtlInSeconds, SubscriptionExpirationTimeMinutes, RetentionPolicies, EncryptionRequired, DelayedDeliveryPolicies, InactiveTopicPolicies, SubscriptionAuthMode, MaxProducersPerTopic, MaxConsumersPerTopic, MaxConsumersPerSubscription, MaxUnackedMessagesPerConsumer, MaxUnackedMessagesPerSubscription, CompactionThreshold, OffloadThreshold, OffloadDeletionLagMs, SchemaAutoUpdateCompatibilityStrategy, SchemaValidationEnforced, SchemaCompatibilityStrategy, IsAllowAutoUpdateSchema, OffloadPolicies, SubscriptionTypesEnabled, Properties, ResourceGroupName, EntryFilters);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is Policies)
			{
				Policies Other = (Policies) Obj;
				return Objects.equals(AuthPolicies, Other.AuthPolicies) && Objects.equals(ReplicationClusters, Other.ReplicationClusters) && Objects.equals(BacklogQuotaMap, Other.BacklogQuotaMap) && Objects.equals(ClusterDispatchRate, Other.ClusterDispatchRate) && Objects.equals(TopicDispatchRate, Other.TopicDispatchRate) && Objects.equals(SubscriptionDispatchRate, Other.SubscriptionDispatchRate) && Objects.equals(ReplicatorDispatchRate, Other.ReplicatorDispatchRate) && Objects.equals(ClusterSubscribeRate, Other.ClusterSubscribeRate) && Objects.equals(PublishMaxMessageRate, Other.PublishMaxMessageRate) && Objects.equals(DeduplicationEnabled, Other.DeduplicationEnabled) && Objects.equals(AutoTopicCreationOverride, Other.AutoTopicCreationOverride) && Objects.equals(AutoSubscriptionCreationOverride, Other.AutoSubscriptionCreationOverride) && Objects.equals(Persistence, Other.Persistence) && Objects.equals(Bundles, Other.Bundles) && Objects.equals(LatencyStatsSampleRate, Other.LatencyStatsSampleRate) && Objects.equals(MessageTtlInSeconds, Other.MessageTtlInSeconds) && Objects.equals(SubscriptionExpirationTimeMinutes, Other.SubscriptionExpirationTimeMinutes) && Objects.equals(RetentionPolicies, Other.RetentionPolicies) && Objects.equals(EncryptionRequired, Other.EncryptionRequired) && Objects.equals(DelayedDeliveryPolicies, Other.DelayedDeliveryPolicies) && Objects.equals(InactiveTopicPolicies, Other.InactiveTopicPolicies) && Objects.equals(SubscriptionAuthMode, Other.SubscriptionAuthMode) && Objects.equals(MaxProducersPerTopic, Other.MaxProducersPerTopic) && Objects.equals(MaxConsumersPerTopic, Other.MaxConsumersPerTopic) && Objects.equals(MaxUnackedMessagesPerConsumer, Other.MaxUnackedMessagesPerConsumer) && Objects.equals(MaxUnackedMessagesPerSubscription, Other.MaxUnackedMessagesPerSubscription) && Objects.equals(MaxConsumersPerSubscription, Other.MaxConsumersPerSubscription) && Objects.equals(CompactionThreshold, Other.CompactionThreshold) && OffloadThreshold == Other.OffloadThreshold && Objects.equals(OffloadDeletionLagMs, Other.OffloadDeletionLagMs) && SchemaAutoUpdateCompatibilityStrategy == Other.SchemaAutoUpdateCompatibilityStrategy && SchemaValidationEnforced == Other.SchemaValidationEnforced && SchemaCompatibilityStrategy == Other.SchemaCompatibilityStrategy && IsAllowAutoUpdateSchema == Other.IsAllowAutoUpdateSchema.Value && Objects.equals(OffloadPolicies, Other.OffloadPolicies) && Objects.equals(SubscriptionTypesEnabled, Other.SubscriptionTypesEnabled) && Objects.equals(Properties, Other.Properties) && Objects.equals(ResourceGroupName, Other.ResourceGroupName) && Objects.equals(EntryFilters, Other.EntryFilters);
			}

			return false;
		}


	}

}