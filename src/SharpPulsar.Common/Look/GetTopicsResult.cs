
using System.Collections.Generic;
using System.Linq;
using SharpPulsar.Common.Naming;

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */
namespace SharpPulsar.Common.Look
{
    //using CommandGetTopicsOfNamespace = org.apache.pulsar.common.api.proto.CommandGetTopicsOfNamespace;
    //using CommandGetTopicsOfNamespaceResponse = org.apache.pulsar.common.api.proto.CommandGetTopicsOfNamespaceResponse;
    //using TopicName = org.apache.pulsar.common.naming.TopicName;
    //using TopicList = org.apache.pulsar.common.topics.TopicList;
    //using TopicsPattern = org.apache.pulsar.common.topics.TopicsPattern;

    /// <summary>
    ///*
    /// A value object.
    /// - The response of HTTP API "admin/v2/namespaces/{domain}/topics" is a topic(non-partitioned topic or partitions)
    ///   array. It will be wrapped to "topics: {topic array}, topicsHash: null, filtered: false, changed: true".
    /// - The response of binary API <seealso cref="CommandGetTopicsOfNamespace"/> is a <seealso cref="CommandGetTopicsOfNamespaceResponse"/>,
    ///   it will be transferred to a <seealso cref="GetTopicsResult"/>.
    /// See more details https://github.com/apache/pulsar/pull/14804.
    /// </summary>
    public class GetTopicsResult
    {
        private readonly IList<string> NonPartitionedOrPartitionTopics;

        /// <summary>
        /// The topics have been filtered by Broker using a regexp. Otherwise, the client should do a client-side filter.
        /// There are three cases that brokers will not filter the topics:
        /// 1. the lookup service is typed HTTP lookup service, the HTTP API has not implemented this feature yet.
        /// 2. the broker does not support this feature(in other words, its version is lower than "2.11.0").
        /// 3. the input param "topicPattern" is too long than the broker config "subscriptionPatternMaxLength".
        /// </summary>
        private readonly bool filtered;

        /// <summary>
        /// The topics hash that was calculated by <seealso cref="TopicList.calculateHash(List)"/>. The param topics that will be used
        /// to calculate the hash code is only contains the topics that has been filtered.
        /// Note: It is always "null" if broker did not filter the topics when calling the API
        /// "LookupService.getTopicsUnderNamespace"(in other words, <seealso cref="filtered"/> is false).
        /// </summary>
        private readonly string topicsHash;

        /// <summary>
        /// The topics hash has changed after compare with the input param "topicsHash" when calling
        /// "LookupService.getTopicsUnderNamespace".
        /// Note: It is always set "true" if the input param "topicsHash" that used to call
        /// "LookupService.getTopicsUnderNamespace" is null or the "LookupService" is "HttpLookupService".
        /// </summary>
        private readonly bool changed;

        /// <summary>
        /// Partitioned topics and non-partitioned topics.
        /// In other words, there is no topic partitions of partitioned topics in this list.
        /// Note: it is not a field of the response of "LookupService.getTopicsUnderNamespace", it is generated in
        /// client-side memory.
        /// </summary>
        private volatile IList<string> topics;

        /// <summary>
        /// This constructor is used for binary API.
        /// </summary>
        public GetTopicsResult(IList<string> nonPartitionedOrPartitionTopics, string topicsHash, bool filtered, bool changed)
        {
            this.NonPartitionedOrPartitionTopics = nonPartitionedOrPartitionTopics;
            this.topicsHash = topicsHash;
            this.filtered = filtered;
            this.changed = changed;
        }

        /// <summary>
        /// This constructor is used for HTTP API.
        /// </summary>
        public GetTopicsResult(string[] nonPartitionedOrPartitionTopics) : this(nonPartitionedOrPartitionTopics, null, false, true)
        {
        }

        public virtual IList<string> Topics
        {
            get
            {
                if (topics != null)
                {
                    return topics;
                }
                lock (this)
                {
                    if (topics != null)
                    {
                        return topics;
                    }
                    // Group partitioned topics.
                    IList<string> grouped = new List<string>();
                    foreach (string topic in NonPartitionedOrPartitionTopics)
                    {
                        string partitionedTopic = TopicName.Get(topic).GetPartitionedTopicName();
                        if (!grouped.Contains(partitionedTopic))
                        {
                            grouped.Add(partitionedTopic);
                        }
                    }
                    topics = grouped;
                    return topics;
                }
            }
        }

        public virtual GetTopicsResult FilterTopics(TopicsPattern topicsPattern)
        {
            IList<string> topicsFiltered = TopicList.FilterTopics(Topics, topicsPattern);
            // If nothing changed.
            if (topicsFiltered.SequenceEqual(Topics))
            {
                GetTopicsResult newObj = new GetTopicsResult(NonPartitionedOrPartitionTopics, null, true, true);
                newObj.topics = topics;
                return newObj;
            }
            // Filtered some topics.
            ISet<string> topicsFilteredSet = new HashSet<string>(topicsFiltered);
            IList<string> newTps = new List<string>();
            foreach (string tp in NonPartitionedOrPartitionTopics)
            {
                if (topicsFilteredSet.Contains(TopicName.get(tp).getPartitionedTopicName()))
                {
                    newTps.Add(tp);
                }
            }
            GetTopicsResult newObj = new GetTopicsResult(newTps, null, true, true);
            newObj.topics = topicsFiltered;
            return newObj;
        }
    }
}
