using System.Collections.Generic;
using System.Threading.Tasks;
using SharpPulsar.Admin.Model;
using SharpPulsar.Common.Enum;

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
namespace SharpPulsar.Admin.Interfaces
{
	/// <summary>
	/// Admin interface for topic policies management.
	/// </summary>
	public interface ITopicPolicies
	{

		/// <summary>
		/// Get backlog quota map for a topic. </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException.NotFoundException"> Topic does not exist </exception>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> GetBacklogQuotaMap(string topic);

		/// <summary>
		/// Get applied backlog quota map for a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>
		IDictionary<BacklogQuota.BacklogQuotaType, BacklogQuota> GetBacklogQuotaMap(string topic, bool applied);

		/// <summary>
		/// Set a backlog quota for a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="backlogQuota">
		///            the new BacklogQuota </param>
		/// <param name="backlogQuotaType">
		/// </param>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setBacklogQuota(String topic, org.apache.pulsar.common.policies.data.BacklogQuota backlogQuota, org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType backlogQuotaType) throws PulsarAdminException;
		void SetBacklogQuota(string topic, BacklogQuota backlogQuota, BacklogQuota.BacklogQuotaType backlogQuotaType);


// ORIGINAL LINE: default void setBacklogQuota(String topic, org.apache.pulsar.common.policies.data.BacklogQuota backlogQuota) throws PulsarAdminException
		void SetBacklogQuota(string topic, BacklogQuota backlogQuota)
		{
			SetBacklogQuota(topic, backlogQuota, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}

		/// <summary>
		/// Remove a backlog quota policy from a topic.
		/// The namespace backlog policy falls back to the default.
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="backlogQuotaType">
		/// </param>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void removeBacklogQuota(String topic, org.apache.pulsar.common.policies.data.BacklogQuota.BacklogQuotaType backlogQuotaType) throws PulsarAdminException;
		void RemoveBacklogQuota(string topic, BacklogQuota.BacklogQuotaType backlogQuotaType);


// ORIGINAL LINE: default void removeBacklogQuota(String topic) throws PulsarAdminException
		void RemoveBacklogQuota(string topic)
		{
			RemoveBacklogQuota(topic, BacklogQuota.BacklogQuotaType.DestinationStorage);
		}

		/// <summary>
		/// Get the delayed delivery policy applied for a specified topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DelayedDeliveryPolicies getDelayedDeliveryPolicy(String topic, boolean applied) throws PulsarAdminException;
		DelayedDeliveryPolicies GetDelayedDeliveryPolicy(string topic, bool applied);

		/// <summary>
		/// Get the delayed delivery policy applied for a specified topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<DelayedDeliveryPolicies> GetDelayedDeliveryPolicyAsync(string topic, bool applied);
		/// <summary>
		/// Get the delayed delivery policy for a specified topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DelayedDeliveryPolicies getDelayedDeliveryPolicy(String topic) throws PulsarAdminException;
		DelayedDeliveryPolicies GetDelayedDeliveryPolicy(string topic);

		/// <summary>
		/// Get the delayed delivery policy for a specified topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<DelayedDeliveryPolicies> GetDelayedDeliveryPolicyAsync(string topic);

		/// <summary>
		/// Set the delayed delivery policy for a specified topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="delayedDeliveryPolicies"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setDelayedDeliveryPolicy(String topic, org.apache.pulsar.common.policies.data.DelayedDeliveryPolicies delayedDeliveryPolicies) throws PulsarAdminException;
		void SetDelayedDeliveryPolicy(string topic, DelayedDeliveryPolicies delayedDeliveryPolicies);

		/// <summary>
		/// Set the delayed delivery policy for a specified topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="delayedDeliveryPolicies">
		/// @return </param>
		ValueTask SetDelayedDeliveryPolicyAsync(string topic, DelayedDeliveryPolicies delayedDeliveryPolicies);

		/// <summary>
		/// Remove the delayed delivery policy for a specified topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveDelayedDeliveryPolicyAsync(string topic);

		/// <summary>
		/// Remove the delayed delivery policy for a specified topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeDelayedDeliveryPolicy(String topic) throws PulsarAdminException;
		void RemoveDelayedDeliveryPolicy(string topic);

		/// <summary>
		/// Set message TTL for a topic.
		/// </summary>
		/// <param name="topic">
		///          Topic name </param>
		/// <param name="messageTTLInSecond">
		///          Message TTL in second. </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setMessageTTL(String topic, int messageTTLInSecond) throws PulsarAdminException;
		void SetMessageTTL(string topic, int messageTTLInSecond);

		/// <summary>
		/// Get message TTL for a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> Message TTL in second. </returns>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMessageTTL(String topic) throws PulsarAdminException;
		int? GetMessageTTL(string topic);

		/// <summary>
		/// Get message TTL applied for a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMessageTTL(String topic, boolean applied) throws PulsarAdminException;
		int? GetMessageTTL(string topic, bool applied);

		/// <summary>
		/// Remove message TTL for a topic.
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void removeMessageTTL(String topic) throws PulsarAdminException;
		void RemoveMessageTTL(string topic);

		/// <summary>
		/// Set the retention configuration on a topic.
		/// <p/>
		/// Set the retention configuration on a topic. This operation requires Pulsar super-user access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setRetention(String topic, org.apache.pulsar.common.policies.data.RetentionPolicies retention) throws PulsarAdminException;
		void SetRetention(string topic, RetentionPolicies retention);

		/// <summary>
		/// Set the retention configuration for all the topics on a topic asynchronously.
		/// <p/>
		/// Set the retention configuration on a topic. This operation requires Pulsar super-user access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask SetRetentionAsync(string topic, RetentionPolicies retention);

		/// <summary>
		/// Get the retention configuration for a topic.
		/// <p/>
		/// Get the retention configuration for a topic.
		/// <p/>
		/// Response example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.RetentionPolicies getRetention(String topic) throws PulsarAdminException;
		RetentionPolicies GetRetention(string topic);

		/// <summary>
		/// Get the retention configuration for a topic asynchronously.
		/// <p/>
		/// Get the retention configuration for a topic.
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask<RetentionPolicies> GetRetentionAsync(string topic);

		/// <summary>
		/// Get the applied retention configuration for a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.RetentionPolicies getRetention(String topic, boolean applied) throws PulsarAdminException;
		RetentionPolicies GetRetention(string topic, bool applied);

		/// <summary>
		/// Get the applied retention configuration for a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<RetentionPolicies> GetRetentionAsync(string topic, bool applied);

		/// <summary>
		/// Remove the retention configuration for all the topics on a topic.
		/// <p/>
		/// Remove the retention configuration on a topic. This operation requires Pulsar super-user access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Topic does not exist </exception>
		/// <exception cref="ConflictException">
		///             Concurrent modification </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void removeRetention(String topic) throws PulsarAdminException;
		void RemoveRetention(string topic);

		/// <summary>
		/// Remove the retention configuration for all the topics on a topic asynchronously.
		/// <p/>
		/// Remove the retention configuration on a topic. This operation requires Pulsar super-user access.
		/// <p/>
		/// Request parameter example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// {
		///     "retentionTimeInMinutes" : 60,            // how long to retain messages
		///     "retentionSizeInMB" : 1024,              // retention backlog limit
		/// }
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask RemoveRetentionAsync(string topic);

		/// <summary>
		/// Get max unacked messages on a consumer of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxUnackedMessagesOnConsumer(String topic) throws PulsarAdminException;
		int? GetMaxUnackedMessagesOnConsumer(string topic);

		/// <summary>
		/// get max unacked messages on consumer of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<int> GetMaxUnackedMessagesOnConsumerAsync(string topic);

		/// <summary>
		/// get applied max unacked messages on consumer of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxUnackedMessagesOnConsumer(String topic, boolean applied) throws PulsarAdminException;
		int? GetMaxUnackedMessagesOnConsumer(string topic, bool applied);

		/// <summary>
		/// get applied max unacked messages on consumer of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<int> GetMaxUnackedMessagesOnConsumerAsync(string topic, bool applied);

		/// <summary>
		/// set max unacked messages on consumer of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="maxNum"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setMaxUnackedMessagesOnConsumer(String topic, int maxNum) throws PulsarAdminException;
		void SetMaxUnackedMessagesOnConsumer(string topic, int maxNum);

		/// <summary>
		/// set max unacked messages on consumer of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="maxNum">
		/// @return </param>
		ValueTask SetMaxUnackedMessagesOnConsumerAsync(string topic, int maxNum);

		/// <summary>
		/// remove max unacked messages on consumer of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeMaxUnackedMessagesOnConsumer(String topic) throws PulsarAdminException;
		void RemoveMaxUnackedMessagesOnConsumer(string topic);

		/// <summary>
		/// remove max unacked messages on consumer of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveMaxUnackedMessagesOnConsumerAsync(string topic);

		/// <summary>
		/// Get inactive topic policies applied for a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.InactiveTopicPolicies getInactiveTopicPolicies(String topic, boolean applied) throws PulsarAdminException;
		InactiveTopicPolicies GetInactiveTopicPolicies(string topic, bool applied);

		/// <summary>
		/// Get inactive topic policies applied for a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<InactiveTopicPolicies> GetInactiveTopicPoliciesAsync(string topic, bool applied);
		/// <summary>
		/// get inactive topic policies of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.InactiveTopicPolicies getInactiveTopicPolicies(String topic) throws PulsarAdminException;
		InactiveTopicPolicies GetInactiveTopicPolicies(string topic);

		/// <summary>
		/// get inactive topic policies of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<InactiveTopicPolicies> GetInactiveTopicPoliciesAsync(string topic);

		/// <summary>
		/// set inactive topic policies of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="inactiveTopicPolicies"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setInactiveTopicPolicies(String topic, org.apache.pulsar.common.policies.data.InactiveTopicPolicies inactiveTopicPolicies) throws PulsarAdminException;
		void SetInactiveTopicPolicies(string topic, InactiveTopicPolicies inactiveTopicPolicies);

		/// <summary>
		/// set inactive topic policies of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="inactiveTopicPolicies">
		/// @return </param>
		ValueTask SetInactiveTopicPoliciesAsync(string topic, InactiveTopicPolicies inactiveTopicPolicies);

		/// <summary>
		/// remove inactive topic policies of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeInactiveTopicPolicies(String topic) throws PulsarAdminException;
		void RemoveInactiveTopicPolicies(string topic);

		/// <summary>
		/// remove inactive topic policies of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveInactiveTopicPoliciesAsync(string topic);

		/// <summary>
		/// get offload policies of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.OffloadPolicies getOffloadPolicies(String topic) throws PulsarAdminException;
		OffloadPolicies GetOffloadPolicies(string topic);

		/// <summary>
		/// get offload policies of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<OffloadPolicies> GetOffloadPoliciesAsync(string topic);

		/// <summary>
		/// get applied offload policies of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.OffloadPolicies getOffloadPolicies(String topic, boolean applied) throws PulsarAdminException;
		OffloadPolicies GetOffloadPolicies(string topic, bool applied);

		/// <summary>
		/// get applied offload policies of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<OffloadPolicies> GetOffloadPoliciesAsync(string topic, bool applied);

		/// <summary>
		/// set offload policies of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="offloadPolicies"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setOffloadPolicies(String topic, org.apache.pulsar.common.policies.data.OffloadPolicies offloadPolicies) throws PulsarAdminException;
		void SetOffloadPolicies(string topic, OffloadPolicies offloadPolicies);

		/// <summary>
		/// set offload policies of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="offloadPolicies">
		/// @return </param>
		ValueTask SetOffloadPoliciesAsync(string topic, OffloadPolicies offloadPolicies);

		/// <summary>
		/// remove offload policies of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeOffloadPolicies(String topic) throws PulsarAdminException;
		void RemoveOffloadPolicies(string topic);

		/// <summary>
		/// remove offload policies of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveOffloadPoliciesAsync(string topic);

		/// <summary>
		/// get max unacked messages on subscription of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxUnackedMessagesOnSubscription(String topic) throws PulsarAdminException;
		int? GetMaxUnackedMessagesOnSubscription(string topic);

		/// <summary>
		/// get max unacked messages on subscription of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<int> GetMaxUnackedMessagesOnSubscriptionAsync(string topic);

		/// <summary>
		/// get max unacked messages on subscription of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxUnackedMessagesOnSubscription(String topic, boolean applied) throws PulsarAdminException;
		int? GetMaxUnackedMessagesOnSubscription(string topic, bool applied);

		/// <summary>
		/// get max unacked messages on subscription of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<int> GetMaxUnackedMessagesOnSubscriptionAsync(string topic, bool applied);

		/// <summary>
		/// set max unacked messages on subscription of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="maxNum"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setMaxUnackedMessagesOnSubscription(String topic, int maxNum) throws PulsarAdminException;
		void SetMaxUnackedMessagesOnSubscription(string topic, int maxNum);

		/// <summary>
		/// set max unacked messages on subscription of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="maxNum">
		/// @return </param>
		ValueTask SetMaxUnackedMessagesOnSubscriptionAsync(string topic, int maxNum);

		/// <summary>
		/// remove max unacked messages on subscription of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeMaxUnackedMessagesOnSubscription(String topic) throws PulsarAdminException;
		void RemoveMaxUnackedMessagesOnSubscription(string topic);

		/// <summary>
		/// remove max unacked messages on subscription of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveMaxUnackedMessagesOnSubscriptionAsync(string topic);

		/// <summary>
		/// Set the configuration of persistence policies for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="persistencePolicies"> Configuration of bookkeeper persistence policies </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void setPersistence(String topic, org.apache.pulsar.common.policies.data.PersistencePolicies persistencePolicies) throws PulsarAdminException;
		void SetPersistence(string topic, PersistencePolicies persistencePolicies);

		/// <summary>
		/// Set the configuration of persistence policies for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="persistencePolicies"> Configuration of bookkeeper persistence policies </param>
		ValueTask SetPersistenceAsync(string topic, PersistencePolicies persistencePolicies);

		/// <summary>
		/// Get the configuration of persistence policies for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistencePolicies getPersistence(String topic) throws PulsarAdminException;
		PersistencePolicies GetPersistence(string topic);

		/// <summary>
		/// Get the configuration of persistence policies for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask<PersistencePolicies> GetPersistenceAsync(string topic);

		/// <summary>
		/// Get the applied configuration of persistence policies for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PersistencePolicies getPersistence(String topic, boolean applied) throws PulsarAdminException;
		PersistencePolicies GetPersistence(string topic, bool applied);

		/// <summary>
		/// Get the applied configuration of persistence policies for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask<PersistencePolicies> GetPersistenceAsync(string topic, bool applied);

		/// <summary>
		/// Remove the configuration of persistence policies for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void removePersistence(String topic) throws PulsarAdminException;
		void RemovePersistence(string topic);

		/// <summary>
		/// Remove the configuration of persistence policies for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask RemovePersistenceAsync(string topic);

		/// <summary>
		/// get deduplication enabled of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<bool> getDeduplicationStatus(String topic) throws PulsarAdminException;
		bool? GetDeduplicationStatus(string topic);

		/// <summary>
		/// get deduplication enabled of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<bool> GetDeduplicationStatusAsync(string topic);
		/// <summary>
		/// get applied deduplication enabled of a topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<bool> getDeduplicationStatus(String topic, boolean applied) throws PulsarAdminException;
		bool? GetDeduplicationStatus(string topic, bool applied);

		/// <summary>
		/// get applied deduplication enabled of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<bool> GetDeduplicationStatusAsync(string topic, bool applied);

		/// <summary>
		/// set deduplication enabled of a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="enabled"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setDeduplicationStatus(String topic, boolean enabled) throws PulsarAdminException;
		void SetDeduplicationStatus(string topic, bool enabled);

		/// <summary>
		/// set deduplication enabled of a topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="enabled">
		/// @return </param>
		ValueTask SetDeduplicationStatusAsync(string topic, bool enabled);

		/// <summary>
		/// remove deduplication enabled of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeDeduplicationStatus(String topic) throws PulsarAdminException;
		void RemoveDeduplicationStatus(string topic);

		/// <summary>
		/// remove deduplication enabled of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveDeduplicationStatusAsync(string topic);

		/// <summary>
		/// Set message-dispatch-rate (topic can dispatch this many messages per second).
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setDispatchRate(String topic, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void SetDispatchRate(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Set message-dispatch-rate asynchronously.
		/// <p/>
		/// topic can dispatch this many messages per second
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetDispatchRateAsync(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Get message-dispatch-rate (topic can dispatch this many messages per second).
		/// </summary>
		/// <param name="topic">
		/// @returns messageRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getDispatchRate(String topic) throws PulsarAdminException;
		DispatchRate GetDispatchRate(string topic);

		/// <summary>
		/// Get message-dispatch-rate asynchronously.
		/// <p/>
		/// Topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns messageRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetDispatchRateAsync(string topic);

		/// <summary>
		/// Get applied message-dispatch-rate (topic can dispatch this many messages per second).
		/// </summary>
		/// <param name="topic">
		/// @returns messageRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getDispatchRate(String topic, boolean applied) throws PulsarAdminException;
		DispatchRate GetDispatchRate(string topic, bool applied);

		/// <summary>
		/// Get applied message-dispatch-rate asynchronously.
		/// <p/>
		/// Topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns messageRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetDispatchRateAsync(string topic, bool applied);

		/// <summary>
		/// Remove message-dispatch-rate.
		/// <p/>
		/// Remove topic message dispatch rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: void removeDispatchRate(String topic) throws PulsarAdminException;
		void RemoveDispatchRate(string topic);

		/// <summary>
		/// Remove message-dispatch-rate asynchronously.
		/// <p/>
		/// Remove topic message dispatch rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: java.util.concurrent.ValueTask removeDispatchRateAsync(String topic) throws PulsarAdminException;
		ValueTask RemoveDispatchRateAsync(string topic);

		/// <summary>
		/// Set subscription-message-dispatch-rate for the topic.
		/// <p/>
		/// Subscriptions of this topic can dispatch this many messages per second
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setSubscriptionDispatchRate(String topic, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void SetSubscriptionDispatchRate(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Set subscription-message-dispatch-rate for the topic asynchronously.
		/// <p/>
		/// Subscriptions of this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetSubscriptionDispatchRateAsync(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Get applied subscription-message-dispatch-rate.
		/// <p/>
		/// Subscriptions of this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String topic, boolean applied) throws PulsarAdminException;
		DispatchRate GetSubscriptionDispatchRate(string topic, bool applied);

		/// <summary>
		/// Get applied subscription-message-dispatch-rate asynchronously.
		/// <p/>
		/// Subscriptions in this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string topic, bool applied);

		/// <summary>
		/// Get subscription-message-dispatch-rate for the topic.
		/// <p/>
		/// Subscriptions of this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String topic) throws PulsarAdminException;
		DispatchRate GetSubscriptionDispatchRate(string topic);

		/// <summary>
		/// Get subscription-message-dispatch-rate asynchronously.
		/// <p/>
		/// Subscriptions of this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string topic);

		/// <summary>
		/// Remove subscription-message-dispatch-rate for a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: void removeSubscriptionDispatchRate(String topic) throws PulsarAdminException;
		void RemoveSubscriptionDispatchRate(string topic);

		/// <summary>
		/// Remove subscription-message-dispatch-rate for a topic asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask RemoveSubscriptionDispatchRateAsync(string topic);

		/// <summary>
		/// Set dispatch rate limiter for a specific subscription.
		/// </summary>

// ORIGINAL LINE: void setSubscriptionDispatchRate(String topic, String subscriptionName, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void SetSubscriptionDispatchRate(string topic, string subscriptionName, DispatchRate dispatchRate);

		/// <summary>
		/// Async version of <seealso cref="setSubscriptionDispatchRate(String, String, DispatchRate)"/>.
		/// </summary>
		ValueTask SetSubscriptionDispatchRateAsync(string topic, string subscriptionName, DispatchRate dispatchRate);

		/// <summary>
		/// If applied is true, get dispatch rate limiter for a specific subscription.
		/// Or else, return subscription level setting.
		/// </summary>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String topic, String subscriptionName, boolean applied) throws PulsarAdminException;
		DispatchRate GetSubscriptionDispatchRate(string topic, string subscriptionName, bool applied);

		/// <summary>
		/// Async version of <seealso cref="getSubscriptionDispatchRate(String, String, bool)"/>.
		/// </summary>
		ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string topic, string subscriptionName, bool applied);

		/// <summary>
		/// Get subscription level dispatch rate limiter setting for a specific subscription.
		/// </summary>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getSubscriptionDispatchRate(String topic, String subscriptionName) throws PulsarAdminException;
		DispatchRate GetSubscriptionDispatchRate(string topic, string subscriptionName);

		/// <summary>
		/// Async version of <seealso cref="getSubscriptionDispatchRate(String, String)"/>.
		/// </summary>
		ValueTask<DispatchRate> GetSubscriptionDispatchRateAsync(string topic, string subscriptionName);

		/// <summary>
		/// Remove subscription level dispatch rate limiter setting for a specific subscription.
		/// </summary>

// ORIGINAL LINE: void removeSubscriptionDispatchRate(String topic, String subscriptionName) throws PulsarAdminException;
		void RemoveSubscriptionDispatchRate(string topic, string subscriptionName);

		/// <summary>
		/// Async version of <seealso cref="removeSubscriptionDispatchRate(String, String)"/>.
		/// </summary>
		ValueTask RemoveSubscriptionDispatchRateAsync(string topic, string subscriptionName);

		/// <summary>
		/// Set replicatorDispatchRate for the topic.
		/// <p/>
		/// Replicator dispatch rate under this topic can dispatch this many messages per second
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setReplicatorDispatchRate(String topic, org.apache.pulsar.common.policies.data.DispatchRate dispatchRate) throws PulsarAdminException;
		void SetReplicatorDispatchRate(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Set replicatorDispatchRate for the topic asynchronously.
		/// <p/>
		/// Replicator dispatch rate under this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="dispatchRate">
		///            number of messages per second </param>
		ValueTask SetReplicatorDispatchRateAsync(string topic, DispatchRate dispatchRate);

		/// <summary>
		/// Get replicatorDispatchRate for the topic.
		/// <p/>
		/// Replicator dispatch rate under this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getReplicatorDispatchRate(String topic) throws PulsarAdminException;
		DispatchRate GetReplicatorDispatchRate(string topic);

		/// <summary>
		/// Get replicatorDispatchRate asynchronously.
		/// <p/>
		/// Replicator dispatch rate under this topic can dispatch this many messages per second.
		/// </summary>
		/// <param name="topic">
		/// @returns DispatchRate
		///            number of messages per second </param>
		ValueTask<DispatchRate> GetReplicatorDispatchRateAsync(string topic);

		/// <summary>
		/// Get applied replicatorDispatchRate for the topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.DispatchRate getReplicatorDispatchRate(String topic, boolean applied) throws PulsarAdminException;
		DispatchRate GetReplicatorDispatchRate(string topic, bool applied);

		/// <summary>
		/// Get applied replicatorDispatchRate asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<DispatchRate> GetReplicatorDispatchRateAsync(string topic, bool applied);

		/// <summary>
		/// Remove replicatorDispatchRate for a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: void removeReplicatorDispatchRate(String topic) throws PulsarAdminException;
		void RemoveReplicatorDispatchRate(string topic);

		/// <summary>
		/// Remove replicatorDispatchRate for a topic asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask RemoveReplicatorDispatchRateAsync(string topic);

		/// <summary>
		/// Get the compactionThreshold for a topic. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<long> getCompactionThreshold(String topic) throws PulsarAdminException;
		long? GetCompactionThreshold(string topic);

		/// <summary>
		/// Get the compactionThreshold for a topic asynchronously. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask<long> GetCompactionThresholdAsync(string topic);

		/// <summary>
		/// Get the compactionThreshold for a topic. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables. </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="NotAuthorizedException"> Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException"> Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<long> getCompactionThreshold(String topic, boolean applied) throws PulsarAdminException;
		long? GetCompactionThreshold(string topic, bool applied);

		/// <summary>
		/// Get the compactionThreshold for a topic asynchronously. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables. </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask<long> GetCompactionThresholdAsync(string topic, bool applied);

		/// <summary>
		/// Set the compactionThreshold for a topic. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="compactionThreshold">
		///            maximum number of backlog bytes before compaction is triggered
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setCompactionThreshold(String topic, long compactionThreshold) throws PulsarAdminException;
		void SetCompactionThreshold(string topic, long compactionThreshold);

		/// <summary>
		/// Set the compactionThreshold for a topic asynchronously. The maximum number of bytes
		/// can have before compaction is triggered. 0 disables.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10000000</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="compactionThreshold">
		///            maximum number of backlog bytes before compaction is triggered </param>
		ValueTask SetCompactionThresholdAsync(string topic, long compactionThreshold);

		/// <summary>
		/// Remove the compactionThreshold for a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: void removeCompactionThreshold(String topic) throws PulsarAdminException;
		void RemoveCompactionThreshold(string topic);

		/// <summary>
		/// Remove the compactionThreshold for a topic asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask RemoveCompactionThresholdAsync(string topic);

		/// <summary>
		/// Set message-publish-rate (topics can publish this many messages per second).
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="publishMsgRate">
		///            number of messages per second </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setPublishRate(String topic, org.apache.pulsar.common.policies.data.PublishRate publishMsgRate) throws PulsarAdminException;
		void SetPublishRate(string topic, PublishRate publishMsgRate);

		/// <summary>
		/// Set message-publish-rate (topics can publish this many messages per second) asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="publishMsgRate">
		///            number of messages per second </param>
		ValueTask SetPublishRateAsync(string topic, PublishRate publishMsgRate);

		/// <summary>
		/// Get message-publish-rate (topics can publish this many messages per second).
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> number of messages per second </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.PublishRate getPublishRate(String topic) throws PulsarAdminException;
		PublishRate GetPublishRate(string topic);

		/// <summary>
		/// Get message-publish-rate (topics can publish this many messages per second) asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <returns> number of messages per second </returns>
		ValueTask<PublishRate> GetPublishRateAsync(string topic);

		/// <summary>
		/// Remove message-publish-rate.
		/// <p/>
		/// Remove topic message publish rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: void removePublishRate(String topic) throws PulsarAdminException;
		void RemovePublishRate(string topic);

		/// <summary>
		/// Remove message-publish-rate asynchronously.
		/// <p/>
		/// Remove topic message publish rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: java.util.concurrent.ValueTask removePublishRateAsync(String topic) throws PulsarAdminException;
		ValueTask RemovePublishRateAsync(string topic);

		/// <summary>
		/// Get the maxConsumersPerSubscription for a topic.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxConsumersPerSubscription(String topic) throws PulsarAdminException;
		int? GetMaxConsumersPerSubscription(string topic);

		/// <summary>
		/// Get the maxConsumersPerSubscription for a topic asynchronously.
		/// <p/>
		/// Response example:
		/// 
		/// <pre>
		/// <code>0</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask<int> GetMaxConsumersPerSubscriptionAsync(string topic);

		/// <summary>
		/// Set maxConsumersPerSubscription for a topic.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="maxConsumersPerSubscription">
		///            maxConsumersPerSubscription value for a namespace
		/// </param>
		/// <exception cref="NotAuthorizedException">
		///             Don't have admin permission </exception>
		/// <exception cref="PulsarAdminException.NotFoundException">
		///             Namespace does not exist </exception>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setMaxConsumersPerSubscription(String topic, int maxConsumersPerSubscription) throws PulsarAdminException;
		void SetMaxConsumersPerSubscription(string topic, int maxConsumersPerSubscription);

		/// <summary>
		/// Set maxConsumersPerSubscription for a topic asynchronously.
		/// <p/>
		/// Request example:
		/// 
		/// <pre>
		/// <code>10</code>
		/// </pre>
		/// </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <param name="maxConsumersPerSubscription">
		///            maxConsumersPerSubscription value for a namespace </param>
		ValueTask SetMaxConsumersPerSubscriptionAsync(string topic, int maxConsumersPerSubscription);

		/// <summary>
		/// Remove the maxConsumersPerSubscription for a topic. </summary>
		/// <param name="topic">
		///            Topic name </param>
		/// <exception cref="PulsarAdminException">
		///            Unexpected error </exception>

// ORIGINAL LINE: void removeMaxConsumersPerSubscription(String topic) throws PulsarAdminException;
		void RemoveMaxConsumersPerSubscription(string topic);

		/// <summary>
		/// Remove the maxConsumersPerSubscription for a topic asynchronously. </summary>
		/// <param name="topic">
		///            Topic name </param>
		ValueTask RemoveMaxConsumersPerSubscriptionAsync(string topic);

		/// <summary>
		/// Get the max number of producer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxProducers(String topic) throws PulsarAdminException;
		int? GetMaxProducers(string topic);

		/// <summary>
		/// Get the max number of producer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask<int> GetMaxProducersAsync(string topic);

		/// <summary>
		/// Get the max number of producer applied for specified topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxProducers(String topic, boolean applied) throws PulsarAdminException;
		int? GetMaxProducers(string topic, bool applied);

		/// <summary>
		/// Get the max number of producer applied for specified topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<int> GetMaxProducersAsync(string topic, bool applied);


		/// <summary>
		/// Set the max number of producer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxProducers"> Max number of producer </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void setMaxProducers(String topic, int maxProducers) throws PulsarAdminException;
		void SetMaxProducers(string topic, int maxProducers);

		/// <summary>
		/// Set the max number of producer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxProducers"> Max number of producer </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask SetMaxProducersAsync(string topic, int maxProducers);

		/// <summary>
		/// Remove the max number of producer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void removeMaxProducers(String topic) throws PulsarAdminException;
		void RemoveMaxProducers(string topic);

		/// <summary>
		/// Remove the max number of producer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask RemoveMaxProducersAsync(string topic);

		/// <summary>
		/// Get the max number of subscriptions for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxSubscriptionsPerTopic(String topic) throws PulsarAdminException;
		int? GetMaxSubscriptionsPerTopic(string topic);

		/// <summary>
		/// Get the max number of subscriptions for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask<int> GetMaxSubscriptionsPerTopicAsync(string topic);


		/// <summary>
		/// Set the max number of subscriptions for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxSubscriptionsPerTopic"> Max number of subscriptions </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void setMaxSubscriptionsPerTopic(String topic, int maxSubscriptionsPerTopic) throws PulsarAdminException;
		void SetMaxSubscriptionsPerTopic(string topic, int maxSubscriptionsPerTopic);

		/// <summary>
		/// Set the max number of subscriptions for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxSubscriptionsPerTopic"> Max number of subscriptions </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask SetMaxSubscriptionsPerTopicAsync(string topic, int maxSubscriptionsPerTopic);

		/// <summary>
		/// Remove the max number of subscriptions for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void removeMaxSubscriptionsPerTopic(String topic) throws PulsarAdminException;
		void RemoveMaxSubscriptionsPerTopic(string topic);

		/// <summary>
		/// Remove the max number of subscriptions for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask RemoveMaxSubscriptionsPerTopicAsync(string topic);

		/// <summary>
		/// Get the max message size for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxMessageSize(String topic) throws PulsarAdminException;
		int? GetMaxMessageSize(string topic);

		/// <summary>
		/// Get the max message size for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask<int> GetMaxMessageSizeAsync(string topic);


		/// <summary>
		/// Set the max message size for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxMessageSize"> Max message size of producer </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void setMaxMessageSize(String topic, int maxMessageSize) throws PulsarAdminException;
		void SetMaxMessageSize(string topic, int maxMessageSize);

		/// <summary>
		/// Set the max message size for specified topic asynchronously.0 disables.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxMessageSize"> Max message size of topic </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask SetMaxMessageSizeAsync(string topic, int maxMessageSize);

		/// <summary>
		/// Remove the max message size for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void removeMaxMessageSize(String topic) throws PulsarAdminException;
		void RemoveMaxMessageSize(string topic);

		/// <summary>
		/// Remove the max message size for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask RemoveMaxMessageSizeAsync(string topic);

		/// <summary>
		/// Get the max number of consumer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxConsumers(String topic) throws PulsarAdminException;
		int? GetMaxConsumers(string topic);

		/// <summary>
		/// Get the max number of consumer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <returns> Configuration of bookkeeper persistence policies </returns>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask<int> GetMaxConsumersAsync(string topic);

		/// <summary>
		/// Get the max number of consumer applied for specified topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getMaxConsumers(String topic, boolean applied) throws PulsarAdminException;
		int? GetMaxConsumers(string topic, bool applied);

		/// <summary>
		/// Get the max number of consumer applied for specified topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<int> GetMaxConsumersAsync(string topic, bool applied);

		/// <summary>
		/// Set the max number of consumer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxConsumers"> Max number of consumer </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void setMaxConsumers(String topic, int maxConsumers) throws PulsarAdminException;
		void SetMaxConsumers(string topic, int maxConsumers);

		/// <summary>
		/// Set the max number of consumer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <param name="maxConsumers"> Max number of consumer </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>
		ValueTask SetMaxConsumersAsync(string topic, int maxConsumers);

		/// <summary>
		/// Remove the max number of consumer for specified topic.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		/// <exception cref="PulsarAdminException"> Unexpected error </exception>

// ORIGINAL LINE: void removeMaxConsumers(String topic) throws PulsarAdminException;
		void RemoveMaxConsumers(string topic);

		/// <summary>
		/// Remove the max number of consumer for specified topic asynchronously.
		/// </summary>
		/// <param name="topic"> Topic name </param>
		ValueTask RemoveMaxConsumersAsync(string topic);

		/// <summary>
		/// Get the deduplication snapshot interval for specified topic. </summary>
		/// <param name="topic">
		/// @return </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: System.Nullable<int> getDeduplicationSnapshotInterval(String topic) throws PulsarAdminException;
		int? GetDeduplicationSnapshotInterval(string topic);

		/// <summary>
		/// Get the deduplication snapshot interval for specified topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask<int> GetDeduplicationSnapshotIntervalAsync(string topic);

		/// <summary>
		/// Set the deduplication snapshot interval for specified topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="interval"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void setDeduplicationSnapshotInterval(String topic, int interval) throws PulsarAdminException;
		void SetDeduplicationSnapshotInterval(string topic, int interval);

		/// <summary>
		/// Set the deduplication snapshot interval for specified topic asynchronously. </summary>
		/// <param name="topic"> </param>
		/// <param name="interval">
		/// @return </param>
		ValueTask SetDeduplicationSnapshotIntervalAsync(string topic, int interval);

		/// <summary>
		/// Remove the deduplication snapshot interval for specified topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeDeduplicationSnapshotInterval(String topic) throws PulsarAdminException;
		void RemoveDeduplicationSnapshotInterval(string topic);

		/// <summary>
		/// Remove the deduplication snapshot interval for specified topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveDeduplicationSnapshotIntervalAsync(string topic);

		/// <summary>
		/// Set is enable sub types.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subscriptionTypesEnabled">
		///            is enable subTypes </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setSubscriptionTypesEnabled(String topic, java.util.Set<org.apache.pulsar.client.api.SubscriptionType> subscriptionTypesEnabled) throws PulsarAdminException;
		void SetSubscriptionTypesEnabled(string topic, ISet<SubscriptionType> subscriptionTypesEnabled);

		/// <summary>
		/// Set is enable sub types asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subscriptionTypesEnabled">
		///            is enable subTypes </param>
		ValueTask SetSubscriptionTypesEnabledAsync(string topic, ISet<SubscriptionType> subscriptionTypesEnabled);

		/// <summary>
		/// Get is enable sub types.
		/// </summary>
		/// <param name="topic">
		///            is topic for get is enable sub types </param>
		/// <returns> set of enable sub types <seealso cref="System.Collections.Generic.ISet <SubscriptionType>"/> </returns>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: java.util.Set<org.apache.pulsar.client.api.SubscriptionType> getSubscriptionTypesEnabled(String topic) throws PulsarAdminException;
		ISet<SubscriptionType> GetSubscriptionTypesEnabled(string topic);

		/// <summary>
		/// Get is enable sub types asynchronously.
		/// </summary>
		/// <param name="topic">
		///            is topic for get is enable sub types </param>
		ValueTask<ISet<SubscriptionType>> GetSubscriptionTypesEnabledAsync(string topic);

		/// <summary>
		/// Remove subscription types enabled for a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeSubscriptionTypesEnabled(String topic) throws PulsarAdminException;
		void RemoveSubscriptionTypesEnabled(string topic);

		/// <summary>
		/// Remove subscription types enabled for a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveSubscriptionTypesEnabledAsync(string topic);

		/// <summary>
		/// Set topic-subscribe-rate (topic will limit by subscribeRate).
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subscribeRate">
		///            consumer subscribe limit by this subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: void setSubscribeRate(String topic, org.apache.pulsar.common.policies.data.SubscribeRate subscribeRate) throws PulsarAdminException;
		void SetSubscribeRate(string topic, SubscribeRate subscribeRate);

		/// <summary>
		/// Set topic-subscribe-rate (topics will limit by subscribeRate) asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="subscribeRate">
		///            consumer subscribe limit by this subscribeRate </param>
		ValueTask SetSubscribeRateAsync(string topic, SubscribeRate subscribeRate);

		/// <summary>
		/// Get topic-subscribe-rate (topics allow subscribe times per consumer in a period).
		/// </summary>
		/// <param name="topic">
		/// @returns subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.SubscribeRate getSubscribeRate(String topic) throws PulsarAdminException;
		SubscribeRate GetSubscribeRate(string topic);

		/// <summary>
		/// Get topic-subscribe-rate asynchronously.
		/// <p/>
		/// Topic allow subscribe times per consumer in a period.
		/// </summary>
		/// <param name="topic">
		/// @returns subscribeRate </param>
		ValueTask<SubscribeRate> GetSubscribeRateAsync(string topic);

		/// <summary>
		/// Get applied topic-subscribe-rate (topics allow subscribe times per consumer in a period).
		/// </summary>
		/// <param name="topic">
		/// @returns subscribeRate </param>
		/// <exception cref="PulsarAdminException">
		///             Unexpected error </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.SubscribeRate getSubscribeRate(String topic, boolean applied) throws PulsarAdminException;
		SubscribeRate GetSubscribeRate(string topic, bool applied);

		/// <summary>
		/// Get applied topic-subscribe-rate asynchronously.
		/// </summary>
		/// <param name="topic">
		/// @returns subscribeRate </param>
		ValueTask<SubscribeRate> GetSubscribeRateAsync(string topic, bool applied);

		/// <summary>
		/// Remove topic-subscribe-rate.
		/// <p/>
		/// Remove topic subscribe rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: void removeSubscribeRate(String topic) throws PulsarAdminException;
		void RemoveSubscribeRate(string topic);

		/// <summary>
		/// Remove topic-subscribe-rate asynchronously.
		/// <p/>
		/// Remove topic subscribe rate
		/// </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException">
		///              unexpected error </exception>

// ORIGINAL LINE: java.util.concurrent.ValueTask removeSubscribeRateAsync(String topic) throws PulsarAdminException;
		ValueTask RemoveSubscribeRateAsync(string topic);

		/// <summary>
		/// Get schema compatibility strategy on a topic.
		/// </summary>
		/// <param name="topic">   The topic in whose policy we are interested </param>
		/// <param name="applied"> Get the current applied schema compatibility strategy </param>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy getSchemaCompatibilityStrategy(String topic, boolean applied) throws PulsarAdminException;
		SchemaCompatibilityStrategy GetSchemaCompatibilityStrategy(string topic, bool applied);

		/// <summary>
		/// Get schema compatibility strategy on a topic asynchronously.
		/// </summary>
		/// <param name="topic">   The topic in whose policy we are interested </param>
		/// <param name="applied"> Get the current applied schema compatibility strategy </param>
		ValueTask<SchemaCompatibilityStrategy> GetSchemaCompatibilityStrategyAsync(string topic, bool applied);

		/// <summary>
		/// Set schema compatibility strategy on a topic.
		/// </summary>
		/// <param name="topic">    The topic in whose policy should be set </param>
		/// <param name="strategy"> The schema compatibility strategy </param>

// ORIGINAL LINE: void setSchemaCompatibilityStrategy(String topic, org.apache.pulsar.common.policies.data.SchemaCompatibilityStrategy strategy) throws PulsarAdminException;
		void SetSchemaCompatibilityStrategy(string topic, SchemaCompatibilityStrategy strategy);

		/// <summary>
		/// Set schema compatibility strategy on a topic asynchronously.
		/// </summary>
		/// <param name="topic">    The topic in whose policy should be set </param>
		/// <param name="strategy"> The schema compatibility strategy </param>
		ValueTask SetSchemaCompatibilityStrategyAsync(string topic, SchemaCompatibilityStrategy strategy);

		/// <summary>
		/// Remove schema compatibility strategy on a topic.
		/// </summary>
		/// <param name="topic"> The topic in whose policy should be removed </param>

// ORIGINAL LINE: void removeSchemaCompatibilityStrategy(String topic) throws PulsarAdminException;
		void RemoveSchemaCompatibilityStrategy(string topic);

		/// <summary>
		/// Remove schema compatibility strategy on a topic asynchronously.
		/// </summary>
		/// <param name="topic"> The topic in whose policy should be removed </param>
		ValueTask RemoveSchemaCompatibilityStrategyAsync(string topic);

		/// <summary>
		/// Get applied entry filters for a topic. </summary>
		/// <param name="topic"> </param>
		/// <param name="applied"> </param>
		/// <returns> entry filters classes info. </returns>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: org.apache.pulsar.common.policies.data.EntryFilters getEntryFiltersPerTopic(String topic, boolean applied) throws PulsarAdminException;
		EntryFilters GetEntryFiltersPerTopic(string topic, bool applied);

		/// <summary>
		/// Get applied entry filters for a topic asynchronously.
		/// </summary>
		/// <param name="topic"> </param>
		/// <param name="applied">
		/// @return </param>
		ValueTask<EntryFilters> GetEntryFiltersPerTopicAsync(string topic, bool applied);

		/// <summary>
		/// Set entry filters on a topic.
		/// </summary>
		/// <param name="topic">    The topic in whose policy should be set </param>
		/// <param name="entryFilters"> The entry filters </param>

// ORIGINAL LINE: void setEntryFiltersPerTopic(String topic, org.apache.pulsar.common.policies.data.EntryFilters entryFilters) throws PulsarAdminException;
		void SetEntryFiltersPerTopic(string topic, EntryFilters entryFilters);

		/// <summary>
		/// Set entry filters on a topic asynchronously.
		/// </summary>
		/// <param name="topic">    The topic in whose policy should be set </param>
		/// <param name="entryFilters"> The entry filters </param>
		ValueTask SetEntryFiltersPerTopicAsync(string topic, EntryFilters entryFilters);

		/// <summary>
		/// remove entry filters of a topic. </summary>
		/// <param name="topic"> </param>
		/// <exception cref="PulsarAdminException"> </exception>

// ORIGINAL LINE: void removeEntryFiltersPerTopic(String topic) throws PulsarAdminException;
		void RemoveEntryFiltersPerTopic(string topic);

		/// <summary>
		/// remove entry filters of a topic asynchronously. </summary>
		/// <param name="topic">
		/// @return </param>
		ValueTask RemoveEntryFiltersPerTopicAsync(string topic);


	}

}