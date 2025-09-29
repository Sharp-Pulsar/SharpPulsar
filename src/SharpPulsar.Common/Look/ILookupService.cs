using System.Net;
using SharpPulsar.API.Schema;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Common.Protocol.Proto;

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
namespace SharpPulsar.Common.Look
{

    /// <summary>
    /// Provides lookup service to find broker which serves given topic. It helps to
    /// lookup
    /// <ul>
    /// <li><b>topic-lookup:</b> lookup to find broker-address which serves given
    /// topic</li>
    /// <li><b>Partitioned-topic-Metadata-lookup:</b> lookup to find
    /// PartitionedMetadata for a given topic</li>
    /// </ul>
    /// 
    /// </summary>
    public interface ILookupService
    {

        /// <summary>
        /// Instruct the LookupService to switch to a new service URL for all subsequent requests
        /// </summary>
        void UpdateServiceUrl(string serviceUrl);

        /// <summary>
        /// Calls broker lookup-api to get broker <seealso cref="InetSocketAddress"/> which serves namespace bundle that contains given
        /// topic.
        /// </summary>
        /// <param name="topicName">
        ///            topic-name </param>
        /// <returns> a pair of addresses, representing the logical and physical address of the broker that serves given topic </returns>
        ValueTask<LookupTopicResult> GetBroker(TopicName topicName);

        /// <summary>
        /// Returns <seealso cref="PartitionedTopicMetadata"/> for a given topic.
        /// </summary>
        /// <param name="topicName"> topic-name
        /// @return </param>
        virtual ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName topicName)
        {
            return GetPartitionedTopicMetadata(topicName, true, true);
        }

        /// <summary>
        /// See the doc <seealso cref="getPartitionedTopicMetadata(TopicName, bool, bool)"/>.
        /// </summary>
        virtual ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName topicName, bool metadataAutoCreationEnabled)
        {
            return GetPartitionedTopicMetadata(topicName, metadataAutoCreationEnabled, false);
        }

        /// <summary>
        /// 1.Get the partitions if the topic exists. Return "{partition: n}" if a partitioned topic exists;
        ///  return "{partition: 0}" if a non-partitioned topic exists. </summary>
        /// 2. When {<param name="metadataAutoCreationEnabled">} is "false," neither partitioned topic nor non-partitioned topic
        ///   does not exist. You will get a <seealso cref="PulsarClientException.NotFoundException"/> or
        ///   a <seealso cref="PulsarClientException.TopicDoesNotExistException"/>.
        ///  2-1. You will get a <seealso cref="PulsarClientException.NotSupportedException"/> with metadataAutoCreationEnabled=false
        ///       on an old broker version which does not support getting partitions without partitioned metadata
        ///       auto-creation. </param>
        /// 3.When {<param name="metadataAutoCreationEnabled">} is "true," it will trigger an auto-creation for this topic(using
        ///  the default topic auto-creation strategy you set for the broker), and the corresponding result is returned.
        ///  For the result, see case 1.
        /// </para>
        /// </param>
        /// <param name="useFallbackForNonPIP344Brokers"> <para>If true, fallback to the prior behavior of the method
        ///   <seealso cref="getPartitionedTopicMetadata(TopicName)"/> if the broker does not support the PIP-344 feature
        ///   'supports_get_partitioned_metadata_without_auto_creation'. This parameter only affects the behavior when </param>
        ///   {<param name="metadataAutoCreationEnabled">} is false.</p>
        /// @version 3.3.0. </param>
        ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName topicName, bool metadataAutoCreationEnabled, bool useFallbackForNonPIP344Brokers);


        /// <summary>
        /// Returns current SchemaInfo <seealso cref="SchemaInfo"/> for a given topic.
        /// </summary>
        /// <param name="topicName"> topic-name </param>
        /// <returns> SchemaInfo </returns>
        ValueTask<ISchemaInfo> GetSchema(TopicName topicName);

        /// <summary>
        /// Returns specific version SchemaInfo <seealso cref="SchemaInfo"/> for a given topic.
        /// </summary>
        /// <param name="topicName"> topic-name </param>
        /// <param name="version"> schema info version </param>
        /// <returns> SchemaInfo </returns>
        ValueTask<ISchemaInfo> GetSchema(TopicName topicName, byte[] version);

        /// <summary>
        /// Returns broker-service lookup api url.
        /// 
        /// @return
        /// </summary>
        string ServiceUrl { get; }

        /// <summary>
        /// Resolves pulsar service url.
        /// </summary>
        /// <returns> the service url resolved to a socket address </returns>
        IPEndPoint ResolveHost();

        /// Returns all the topics that matches {<param name="topicPattern">} for a given namespace.
        /// </param>
        /// Note: {<param name="topicPattern">} it relate to the topic name(without the partition suffix). For example:
        ///  - There is a partitioned topic "tp-a" with two partitions.
        ///    - tp-a-partition-0
        ///    - tp-a-partition-1 </param>
        ///  - If {<param name="topicPattern">} is "tp-a", the consumer will subscribe to the two partitions. </param>
        ///  - if {<param name="topicPattern">} is "tp-a-partition-0", the consumer will subscribe nothing.
        /// </param>
        /// <param name="namespace"> : namespace-name
        /// @return </param>

        ValueTask<GetTopicsResult> GetTopicsUnderNamespace(NamespaceName @namespace, CommandGetTopicsOfNamespace.Mode mode, string topicPattern, string topicsHash);
        void Close();
        IList<IPEndPoint> AddressList();

    }

}