﻿using System.Collections.Generic;

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
namespace SharpPulsar.Impl
{

	using Pair = org.apache.commons.lang3.tuple.Pair;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;

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
	public interface LookupService : AutoCloseable
	{

		/// <summary>
		/// Instruct the LookupService to switch to a new service URL for all subsequent requests
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateServiceUrl(String serviceUrl) throws org.apache.pulsar.client.api.PulsarClientException;
		void updateServiceUrl(string serviceUrl);

		/// <summary>
		/// Calls broker lookup-api to get broker <seealso cref="InetSocketAddress"/> which serves namespace bundle that contains given
		/// topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> a pair of addresses, representing the logical and physical address of the broker that serves given topic </returns>
		CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>> getBroker(TopicName topicName);

		/// <summary>
		/// Returns <seealso cref="PartitionedTopicMetadata"/> for a given topic.
		/// </summary>
		/// <param name="topicName"> topic-name
		/// @return </param>
		CompletableFuture<PartitionedTopicMetadata> getPartitionedTopicMetadata(TopicName topicName);

		/// <summary>
		/// Returns current SchemaInfo <seealso cref="SchemaInfo"/> for a given topic.
		/// </summary>
		/// <param name="topicName"> topic-name </param>
		/// <returns> SchemaInfo </returns>
		CompletableFuture<Optional<SchemaInfo>> getSchema(TopicName topicName);

		/// <summary>
		/// Returns specific version SchemaInfo <seealso cref="SchemaInfo"/> for a given topic.
		/// </summary>
		/// <param name="topicName"> topic-name </param>
		/// <param name="version"> schema info version </param>
		/// <returns> SchemaInfo </returns>
		CompletableFuture<Optional<SchemaInfo>> getSchema(TopicName topicName, sbyte[] version);

		/// <summary>
		/// Returns broker-service lookup api url.
		/// 
		/// @return
		/// </summary>
		string ServiceUrl {get;}

		/// <summary>
		/// Returns all the topics name for a given namespace.
		/// </summary>
		/// <param name="namespace"> : namespace-name
		/// @return </param>
		CompletableFuture<IList<string>> getTopicsUnderNamespace(NamespaceName @namespace, Mode mode);

	}

}