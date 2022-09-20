using System;
using System.Collections.Generic;
using SharpPulsar.Admin.Admin.Models;
using System.Text.Json.Serialization;
using System.Linq;

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
	/// <summary>
	/// Definition of Pulsar policies.
	/// </summary>
	public class Policies
	{
		
        [JsonPropertyName("entry_filters")]
        public EntryFilters EntryFilters { get; set; }


        /// <summary>
        /// </summary>
        [JsonPropertyName("auth_policies")]
        public AuthPolicies AuthPolicies { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("replication_clusters")]
        public IList<string> ReplicationClusters { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("bundles")]
        public BundlesData Bundles { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("backlog_quota_map")]
        public IDictionary<string, BacklogQuota> BacklogQuotaMap { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("clusterDispatchRate")]
        public IDictionary<string, DispatchRateImpl> ClusterDispatchRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("topicDispatchRate")]
        public IDictionary<string, DispatchRateImpl> TopicDispatchRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("subscriptionDispatchRate")]
        public IDictionary<string, DispatchRateImpl> SubscriptionDispatchRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("replicatorDispatchRate")]
        public IDictionary<string, DispatchRateImpl> ReplicatorDispatchRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("clusterSubscribeRate")]
        public IDictionary<string, SubscribeRate> ClusterSubscribeRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("persistence")]
        public PersistencePolicies Persistence { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("deduplicationEnabled")]
        public bool? DeduplicationEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("autoTopicCreationOverride")]
        public AutoTopicCreationOverride AutoTopicCreationOverride { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("autoSubscriptionCreationOverride")]
        public AutoSubscriptionCreationOverride AutoSubscriptionCreationOverride { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("publishMaxMessageRate")]
        public IDictionary<string, PublishRate> PublishMaxMessageRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("latency_stats_sample_rate")]
        public IDictionary<string, int?> LatencyStatsSampleRate { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("message_ttl_in_seconds")]
        public int? MessageTtlInSeconds { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("subscription_expiration_time_minutes")]
        public int? SubscriptionExpirationTimeMinutes { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("retention_policies")]
        public RetentionPolicies RetentionPolicies { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("deleted")]
        public bool? Deleted { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("encryption_required")]
        public bool? EncryptionRequired { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("delayed_delivery_policies")]
        public DelayedDeliveryPolicies DelayedDeliveryPolicies { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("inactive_topic_policies")]
        public InactiveTopicPolicies InactiveTopicPolicies { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'None', 'Prefix'
        /// </summary>
        [JsonPropertyName("subscription_auth_mode")]
        public string SubscriptionAuthMode { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_producers_per_topic")]
        public int? MaxProducersPerTopic { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_consumers_per_topic")]
        public int? MaxConsumersPerTopic { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_consumers_per_subscription")]
        public int? MaxConsumersPerSubscription { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_unacked_messages_per_consumer")]
        public int? MaxUnackedMessagesPerConsumer { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_unacked_messages_per_subscription")]
        public int? MaxUnackedMessagesPerSubscription { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_subscriptions_per_topic")]
        public int? MaxSubscriptionsPerTopic { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("compaction_threshold")]
        public long? CompactionThreshold { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("offload_threshold")]
        public long? OffloadThreshold { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("offload_deletion_lag_ms")]
        public long? OffloadDeletionLagMs { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("max_topics_per_namespace")]
        public int? MaxTopicsPerNamespace { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'AutoUpdateDisabled',
        /// 'Backward', 'Forward', 'Full', 'AlwaysCompatible',
        /// 'BackwardTransitive', 'ForwardTransitive', 'FullTransitive'
        /// </summary>
        [JsonPropertyName("schema_auto_update_compatibility_strategy")]
        public SchemaAutoUpdateCompatibilityStrategy SchemaAutoUpdateCompatibilityStrategy { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'UNDEFINED',
        /// 'ALWAYS_INCOMPATIBLE', 'ALWAYS_COMPATIBLE', 'BACKWARD', 'FORWARD',
        /// 'FULL', 'BACKWARD_TRANSITIVE', 'FORWARD_TRANSITIVE',
        /// 'FULL_TRANSITIVE'
        /// </summary>
        [JsonPropertyName("schema_compatibility_strategy")]
        public SchemaCompatibilityStrategy SchemaCompatibilityStrategy { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("is_allow_auto_update_schema")]
        public bool? IsAllowAutoUpdateSchema { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("schema_validation_enforced")]
        public bool? SchemaValidationEnforced { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("offload_policies")]
        public OffloadPolicies OffloadPolicies { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("deduplicationSnapshotIntervalSeconds")]
        public int? DeduplicationSnapshotIntervalSeconds { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("subscription_types_enabled")]
        public IList<string> SubscriptionTypesEnabled { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("properties")]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("resource_group_name")]
        public string ResourceGroupName { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (ReplicationClusters != null)
            {
                if (ReplicationClusters.Count != Enumerable.Count(Enumerable.Distinct(ReplicationClusters)))
                {
                    throw new InvalidOperationException("ReplicationClusters");
                }
            }
            if (SubscriptionTypesEnabled != null)
            {
                if (SubscriptionTypesEnabled.Count != Enumerable.Count(Enumerable.Distinct(SubscriptionTypesEnabled)))
                {
                    throw new InvalidOperationException("SubscriptionTypesEnabled");
                }
            }
        }
    }

}