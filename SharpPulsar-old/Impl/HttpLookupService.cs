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
namespace SharpPulsar.Impl
{
	using Lists = com.google.common.collect.Lists;

	using EventLoopGroup = io.netty.channel.EventLoopGroup;


	using Pair = org.apache.commons.lang3.tuple.Pair;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using NotFoundException = org.apache.pulsar.client.api.PulsarClientException.NotFoundException;
	using ClientConfigurationData = SharpPulsar.Impl.conf.ClientConfigurationData;
	using LookupData = org.apache.pulsar.common.lookup.data.LookupData;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using GetSchemaResponse = org.apache.pulsar.common.protocol.schema.GetSchemaResponse;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using SchemaInfoUtil = org.apache.pulsar.common.protocol.schema.SchemaInfoUtil;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.yahoo.sketches.Util.bytesToLong;

	internal class HttpLookupService : LookupService
	{

		private readonly HttpClient httpClient;
		private readonly bool useTls;

		private const string BasePathV1 = "lookup/v2/destination/";
		private const string BasePathV2 = "lookup/v2/topic/";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public HttpLookupService(SharpPulsar.Impl.conf.ClientConfigurationData conf, io.netty.channel.EventLoopGroup eventLoopGroup) throws org.apache.pulsar.client.api.PulsarClientException
		public HttpLookupService(ClientConfigurationData conf, EventLoopGroup eventLoopGroup)
		{
			this.httpClient = new HttpClient(conf.ServiceUrl, conf.Authentication, eventLoopGroup, conf.TlsAllowInsecureConnection, conf.TlsTrustCertsFilePath);
			this.useTls = conf.UseTls;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateServiceUrl(String serviceUrl) throws org.apache.pulsar.client.api.PulsarClientException
		public virtual void updateServiceUrl(string serviceUrl)
		{
			httpClient.ServiceUrl = serviceUrl;
		}

		/// <summary>
		/// Calls http-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName"> topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") public java.util.concurrent.CompletableFuture<org.apache.commons.lang3.tuple.Pair<java.net.InetSocketAddress, java.net.InetSocketAddress>> getBroker(org.apache.pulsar.common.naming.TopicName topicName)
		public virtual CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>> getBroker(TopicName topicName)
		{
			string basePath = topicName.V2 ? BasePathV2 : BasePathV1;

			return httpClient.get(basePath + topicName.LookupName, typeof(LookupData)).thenCompose(lookupData =>
			{
			URI uri = null;
			try
			{
				if (useTls)
				{
					uri = new URI(lookupData.BrokerUrlTls);
				}
				else
				{
					string serviceUrl = lookupData.BrokerUrl;
					if (string.ReferenceEquals(serviceUrl, null))
					{
						serviceUrl = lookupData.NativeUrl;
					}
					uri = new URI(serviceUrl);
				}
				InetSocketAddress brokerAddress = InetSocketAddress.createUnresolved(uri.Host, uri.Port);
				return CompletableFuture.completedFuture(Pair.of(brokerAddress, brokerAddress));
			}
			catch (Exception e)
			{
				log.warn("[{}] Lookup Failed due to invalid url {}, {}", topicName, uri, e.Message);
				return FutureUtil.failedFuture(e);
			}
			});
		}

		public virtual CompletableFuture<PartitionedTopicMetadata> getPartitionedTopicMetadata(TopicName topicName)
		{
			string format = topicName.V2 ? "admin/v2/%s/partitions" : "admin/%s/partitions";
			return httpClient.get(string.format(format, topicName.LookupName) + "?checkAllowAutoCreation=true", typeof(PartitionedTopicMetadata));
		}

		public virtual string ServiceUrl
		{
			get
			{
				return httpClient.ServiceUrl;
			}
		}

		public virtual CompletableFuture<IList<string>> getTopicsUnderNamespace(NamespaceName @namespace, Mode mode)
		{
			CompletableFuture<IList<string>> future = new CompletableFuture<IList<string>>();

			string format = @namespace.V2 ? "admin/v2/namespaces/%s/topics?mode=%s" : "admin/namespaces/%s/destinations?mode=%s";
			httpClient.get(string.format(format, @namespace, mode.ToString()), typeof(string[])).thenAccept(topics =>
			{
			IList<string> result = Lists.newArrayList();
			Arrays.asList(topics).forEach(topic =>
			{
				string filtered = TopicName.get(topic).PartitionedTopicName;
				if (!result.contains(filtered))
				{
					result.add(filtered);
				}
			});
			future.complete(result);
			}).exceptionally(ex =>
			{
			log.warn("Failed to getTopicsUnderNamespace namespace: {}.", @namespace, ex.Message);
			future.completeExceptionally(ex);
			return null;
		});
			return future;
		}

		public virtual CompletableFuture<Optional<SchemaInfo>> getSchema(TopicName topicName)
		{
			return getSchema(topicName, null);
		}

		public virtual CompletableFuture<Optional<SchemaInfo>> getSchema(TopicName topicName, sbyte[] version)
		{
			CompletableFuture<Optional<SchemaInfo>> future = new CompletableFuture<Optional<SchemaInfo>>();

			string schemaName = topicName.SchemaName;
			string path = string.Format("admin/v2/schemas/{0}/schema", schemaName);
			if (version != null)
			{
				path = string.Format("admin/v2/schemas/{0}/schema/{1}", schemaName, bytesToLong(version));
			}
			httpClient.get(path, typeof(GetSchemaResponse)).thenAccept(response =>
			{
			future.complete(SchemaInfoUtil.newSchemaInfo(schemaName, response));
			}).exceptionally(ex =>
			{
			if (ex.Cause is NotFoundException)
			{
				future.complete(null);
			}
			else
			{
				log.warn("Failed to get schema for topic {} version {}", topicName, version != null ? Base64.Encoder.encodeToString(version) : null, ex.Cause);
				future.completeExceptionally(ex);
			}
			return null;
		});
			return future;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws Exception
		public override void close()
		{
			httpClient.Dispose();
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(HttpLookupService));
	}

}