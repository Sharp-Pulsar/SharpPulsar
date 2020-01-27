using DotNetty.Transport.Libuv;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl.Conf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.yahoo.sketches.Util.bytesToLong;

	public class HttpLookupService : ILookupService
	{

		private readonly HttpClient httpClient;
		private readonly bool useTls;

		private const string BasePathV1 = "lookup/v2/destination/";
		private const string BasePathV2 = "lookup/v2/topic/";

		public HttpLookupService(ClientConfigurationData Conf)
		{
			this.httpClient = new HttpClient(Conf.ServiceUrl, Conf.Authentication, Conf.TlsAllowInsecureConnection, Conf.TlsTrustCertsFilePath);
			this.useTls = Conf.UseTls;
		}

		public void UpdateServiceUrl(string ServiceUrl)
		{
			httpClient.ServiceUrl = ServiceUrl;
		}

		/// <summary>
		/// Calls http-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName"> topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		public virtual ValueTask<KeyValuePair<EndPoint, EndPoint>> GetBroker(TopicName TopicName)
		{
			string BasePath = TopicName.V2 ? BasePathV2 : BasePathV1;

			return httpClient.Get(BasePath + TopicName.LookupName, typeof(LookupData)).thenCompose(lookupData =>
			{
			URI Uri = null;
			try
			{
				if (useTls)
				{
					Uri = new URI(lookupData.BrokerUrlTls);
				}
				else
				{
					string ServiceUrl = lookupData.BrokerUrl;
					if (string.ReferenceEquals(ServiceUrl, null))
					{
						ServiceUrl = lookupData.NativeUrl;
					}
					Uri = new URI(ServiceUrl);
				}
				InetSocketAddress BrokerAddress = InetSocketAddress.createUnresolved(Uri.Host, Uri.Port);
				return CompletableFuture.completedFuture(Pair.of(BrokerAddress, BrokerAddress));
			}
			catch (Exception E)
			{
				log.warn("[{}] Lookup Failed due to invalid url {}, {}", TopicName, Uri, E.Message);
				return FutureUtil.failedFuture(E);
			}
			});
		}

		public virtual CompletableFuture<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName TopicName)
		{
			string Format = TopicName.V2 ? "admin/v2/%s/partitions" : "admin/%s/partitions";
			return httpClient.Get(string.format(Format, TopicName.LookupName) + "?checkAllowAutoCreation=true", typeof(PartitionedTopicMetadata));
		}

		public virtual string ServiceUrl
		{
			get
			{
				return httpClient.ServiceUrl;
			}
		}

		public override CompletableFuture<IList<string>> GetTopicsUnderNamespace(NamespaceName Namespace, Mode Mode)
		{
			CompletableFuture<IList<string>> Future = new CompletableFuture<IList<string>>();

			string Format = Namespace.V2 ? "admin/v2/namespaces/%s/topics?mode=%s" : "admin/namespaces/%s/destinations?mode=%s";
			httpClient.Get(string.format(Format, Namespace, Mode.ToString()), typeof(string[])).thenAccept(topics =>
			{
			IList<string> Result = Lists.newArrayList();
			Arrays.asList(topics).forEach(topic =>
			{
				string Filtered = TopicName.get(topic).PartitionedTopicName;
				if (!Result.Contains(Filtered))
				{
					Result.Add(Filtered);
				}
			});
			Future.complete(Result);
			}).exceptionally(ex =>
			{
			log.warn("Failed to getTopicsUnderNamespace namespace: {}.", Namespace, ex.Message);
			Future.completeExceptionally(ex);
			return null;
		});
			return Future;
		}

		public override CompletableFuture<Optional<SchemaInfo>> GetSchema(TopicName TopicName)
		{
			return GetSchema(TopicName, null);
		}

		public override CompletableFuture<Optional<SchemaInfo>> GetSchema(TopicName TopicName, sbyte[] Version)
		{
			CompletableFuture<Optional<SchemaInfo>> Future = new CompletableFuture<Optional<SchemaInfo>>();

			string SchemaName = TopicName.SchemaName;
			string Path = string.Format("admin/v2/schemas/{0}/schema", SchemaName);
			if (Version != null)
			{
				Path = string.Format("admin/v2/schemas/{0}/schema/{1}", SchemaName, bytesToLong(Version));
			}
			httpClient.Get(Path, typeof(GetSchemaResponse)).thenAccept(response =>
			{
			Future.complete(SchemaInfoUtil.newSchemaInfo(SchemaName, response));
			}).exceptionally(ex =>
			{
			if (ex.Cause is NotFoundException)
			{
				Future.complete(null);
			}
			else
			{
				log.warn("Failed to get schema for topic {} version {}", TopicName, Version != null ? Base64.Encoder.encodeToString(Version) : null, ex.Cause);
				Future.completeExceptionally(ex);
			}
			return null;
		});
			return Future;
		}

		public override void Close()
		{
			httpClient.Dispose();
		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(HttpLookupService));
	}

}