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

	/// <summary>
	/// Statistics for a non-persistent topic.
	/// </summary>
	public interface NonPersistentTopicStats : TopicStats
	{

		/// <summary>
		/// for non-persistent topic: broker drops msg if publisher publishes messages more than configured max inflight
		/// messages per connection.
		/// 
		/// </summary>
		double MsgDropRate {get;}

		/// <summary>
		/// List of connected publishers on this topic w/ their stats. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.List<? extends NonPersistentPublisherStats> getPublishers();
		IList<NonPersistentPublisherStats> Publishers {get;}

		/// <summary>
		/// Map of subscriptions with their individual statistics. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.Map<String, ? extends NonPersistentSubscriptionStats> getSubscriptions();
		IDictionary<string, NonPersistentSubscriptionStats> Subscriptions {get;}

		/// <summary>
		/// Map of replication statistics by remote cluster context. </summary>
// JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
// ORIGINAL LINE: java.util.Map<String, ? extends NonPersistentReplicatorStats> getReplication();
		IDictionary<string, NonPersistentReplicatorStats> Replication {get;}
	}

}