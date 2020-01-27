using SharpPulsar.Api;
using SharpPulsar.Api.Schema;
using SharpPulsar.Common.Naming;
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
namespace SharpPulsar.Impl.Schema.Generic
{


	/// <summary>
	/// Multi version generic schema provider by guava cache.
	/// </summary>
	public class MultiVersionSchemaInfoProvider : SchemaInfoProvider
	{

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(MultiVersionSchemaInfoProvider));

		private readonly TopicName topicName;
		public virtual IPulsarClient PulsarClient { get;}

		private readonly LoadingCache<BytesSchemaVersion, CompletableFuture<SchemaInfo>> cache = CacheBuilder.newBuilder().maximumSize(100000).expireAfterAccess(30, BAMCIS.Util.Concurrent.TimeUnit.MINUTES).build(new CacheLoaderAnonymousInnerClass());

		public class CacheLoaderAnonymousInnerClass : CacheLoader<BytesSchemaVersion, CompletableFuture<SchemaInfo>>
		{
			public override CompletableFuture<SchemaInfo> load(BytesSchemaVersion SchemaVersion)
			{
				CompletableFuture<SchemaInfo> SiFuture = outerInstance.loadSchema(SchemaVersion.get());
				SiFuture.whenComplete((si, cause) =>
				{
				if (null != cause)
				{
					cache.asMap().remove(SchemaVersion, SiFuture);
				}
				});
				return SiFuture;
			}
		}

		public MultiVersionSchemaInfoProvider(TopicName topicName, PulsarClientImpl PulsarClient)
		{
			this.topicName = topicName;
			this.PulsarClient = PulsarClient;
		}

		public override CompletableFuture<SchemaInfo> GetSchemaByVersion(sbyte[] SchemaVersion)
		{
			try
			{
				if (null == SchemaVersion)
				{
					return CompletableFuture.completedFuture(null);
				}
				return cache.get(BytesSchemaVersion.of(SchemaVersion));
			}
			catch (ExecutionException E)
			{
				LOG.error("Can't get schema for topic {} schema version {}", topicName.ToString(), StringHelper.NewString(SchemaVersion, StandardCharsets.UTF_8), E);
				return FutureUtil.failedFuture(E.InnerException);
			}
		}

		public virtual CompletableFuture<SchemaInfo> LatestSchema
		{
			get
			{
				return PulsarClient.Lookup.getSchema(topicName).thenApply(o => o.orElse(null));
			}
		}

		public virtual string TopicName
		{
			get
			{
				return topicName.LocalName;
			}
		}

		private CompletableFuture<SchemaInfo> LoadSchema(sbyte[] SchemaVersion)
		{
			 return PulsarClient.Lookup.getSchema(topicName, SchemaVersion).thenApply(o => o.orElse(null));
		}

	}

}