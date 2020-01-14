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
namespace org.apache.pulsar.client.impl.schema.generic
{
	using CacheBuilder = com.google.common.cache.CacheBuilder;
	using CacheLoader = com.google.common.cache.CacheLoader;
	using LoadingCache = com.google.common.cache.LoadingCache;
	using SchemaInfoProvider = org.apache.pulsar.client.api.schema.SchemaInfoProvider;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using FutureUtil = org.apache.pulsar.common.util.FutureUtil;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;


	/// <summary>
	/// Multi version generic schema provider by guava cache.
	/// </summary>
	public class MultiVersionSchemaInfoProvider : SchemaInfoProvider
	{

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(MultiVersionSchemaInfoProvider));

		private readonly TopicName topicName;
		private readonly PulsarClientImpl pulsarClient;

		private readonly LoadingCache<BytesSchemaVersion, CompletableFuture<SchemaInfo>> cache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		private class CacheLoaderAnonymousInnerClass : CacheLoader<BytesSchemaVersion, CompletableFuture<SchemaInfo>>
		{
			public override CompletableFuture<SchemaInfo> load(BytesSchemaVersion schemaVersion)
			{
				CompletableFuture<SchemaInfo> siFuture = outerInstance.loadSchema(schemaVersion.get());
				siFuture.whenComplete((si, cause) =>
				{
				if (null != cause)
				{
					cache.asMap().remove(schemaVersion, siFuture);
				}
				});
				return siFuture;
			}
		}

		public MultiVersionSchemaInfoProvider(TopicName topicName, PulsarClientImpl pulsarClient)
		{
			this.topicName = topicName;
			this.pulsarClient = pulsarClient;
		}

		public override CompletableFuture<SchemaInfo> getSchemaByVersion(sbyte[] schemaVersion)
		{
			try
			{
				if (null == schemaVersion)
				{
					return CompletableFuture.completedFuture(null);
				}
				return cache.get(BytesSchemaVersion.of(schemaVersion));
			}
			catch (ExecutionException e)
			{
				LOG.error("Can't get schema for topic {} schema version {}", topicName.ToString(), StringHelper.NewString(schemaVersion, StandardCharsets.UTF_8), e);
				return FutureUtil.failedFuture(e.InnerException);
			}
		}

		public override CompletableFuture<SchemaInfo> LatestSchema
		{
			get
			{
				return pulsarClient.Lookup.getSchema(topicName).thenApply(o => o.orElse(null));
			}
		}

		public override string TopicName
		{
			get
			{
				return topicName.LocalName;
			}
		}

		private CompletableFuture<SchemaInfo> loadSchema(sbyte[] schemaVersion)
		{
			 return pulsarClient.Lookup.getSchema(topicName, schemaVersion).thenApply(o => o.orElse(null));
		}

		public virtual PulsarClientImpl PulsarClient
		{
			get
			{
				return pulsarClient;
			}
		}
	}

}