using Optional;
using SharpPulsar.Common.Schema;
using System;
using System.Collections.Generic;
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

	using Lists = com.google.common.collect.Lists;

	using ByteBuf = io.netty.buffer.ByteBuf;


	using Pair = org.apache.commons.lang3.tuple.Pair;
	using PulsarClientException = org.apache.pulsar.client.api.PulsarClientException;
	using Commands = org.apache.pulsar.common.protocol.Commands;
	using Mode = org.apache.pulsar.common.api.proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using CommandLookupTopicResponse = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopicResponse;
	using LookupType = org.apache.pulsar.common.api.proto.PulsarApi.CommandLookupTopicResponse.LookupType;
	using NamespaceName = org.apache.pulsar.common.naming.NamespaceName;
	using TopicName = org.apache.pulsar.common.naming.TopicName;
	using PartitionedTopicMetadata = org.apache.pulsar.common.partition.PartitionedTopicMetadata;
	using BytesSchemaVersion = org.apache.pulsar.common.protocol.schema.BytesSchemaVersion;
	using SchemaInfo = org.apache.pulsar.common.schema.SchemaInfo;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class BinaryProtoLookupService : LookupService
	{

		private readonly PulsarClientImpl client;
		private readonly ServiceNameResolver serviceNameResolver;
		private readonly bool useTls;
		private readonly ExecutorService executor;

		public BinaryProtoLookupService(PulsarClientImpl client, string serviceUrl, bool useTls, ExecutorService executor)
		{
			this.client = client;
			this.useTls = useTls;
			this.executor = executor;
			this.serviceNameResolver = new PulsarServiceNameResolver();
			UdateServiceUrl(serviceUrl);
		}

		public virtual void UpdateServiceUrl(string serviceUrl)
		{
			serviceNameResolver.updateServiceUrl(serviceUrl);
		}

		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		public  ValueTask<KeyValuePair<InetSocketAddress, InetSocketAddress>> GetBroker(TopicName topicName)
		{
			return findBroker(serviceNameResolver.resolveHost(), false, topicName);
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		public virtual ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName topicName)
		{
			return getPartitionedTopicMetadata(serviceNameResolver.resolveHost(), topicName);
		}

		private ValueTask<KeyValuePair<InetSocketAddress, InetSocketAddress>> FindBroker(InetSocketAddress socketAddress, bool authoritative, TopicName topicName)
		{
			ValueTask<KeyValuePair<InetSocketAddress, InetSocketAddress>> addressFuture = new ValueTask<KeyValuePair<InetSocketAddress, InetSocketAddress>>();

			client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = client.newRequestId();
			ByteBuf request = Commands.newLookup(topicName.ToString(), authoritative, requestId);
			clientCnx.newLookup(request, requestId).thenAccept(lookupDataResult =>
			{
				URI uri = null;
				try
				{
					if (useTls)
					{
						uri = new URI(lookupDataResult.brokerUrlTls);
					}
					else
					{
						string serviceUrl = lookupDataResult.brokerUrl;
						uri = new URI(serviceUrl);
					}
					InetSocketAddress responseBrokerAddress = InetSocketAddress.createUnresolved(uri.Host, uri.Port);
					if (lookupDataResult.redirect)
					{
						findBroker(responseBrokerAddress, lookupDataResult.authoritative, topicName).thenAccept(addressPair =>
						{
							addressFuture.complete(addressPair);
						}).exceptionally((lookupException) =>
						{
							log.warn("[{}] lookup failed : {}", topicName.ToString(), lookupException.Message, lookupException);
							addressFuture.completeExceptionally(lookupException);
							return null;
						});
					}
					else
					{
						if (lookupDataResult.proxyThroughServiceUrl)
						{
							addressFuture.complete(Pair.of(responseBrokerAddress, socketAddress));
						}
						else
						{
							addressFuture.complete(Pair.of(responseBrokerAddress, responseBrokerAddress));
						}
					}
				}
				catch (Exception parseUrlException)
				{
					log.warn("[{}] invalid url {} : {}", topicName.ToString(), uri, parseUrlException.Message, parseUrlException);
					addressFuture.completeExceptionally(parseUrlException);
				}
			}).exceptionally((sendException) =>
			{
				log.warn("[{}] failed to send lookup request : {}", topicName.ToString(), sendException.Message);
				if (log.DebugEnabled)
				{
					log.warn("[{}] Lookup response exception: {}", topicName.ToString(), sendException);
				}
				addressFuture.completeExceptionally(sendException);
				return null;
			});
			}).exceptionally(connectionException =>
			{
			addressFuture.completeExceptionally(connectionException);
			return null;
		});
			return addressFuture;
		}

		private ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(InetSocketAddress socketAddress, TopicName topicName)
		{

			ValueTask<PartitionedTopicMetadata> partitionFuture = new ValueTask<PartitionedTopicMetadata>();

			client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = client.newRequestId();
			ByteBuf request = Commands.newPartitionMetadataRequest(topicName.ToString(), requestId);
			clientCnx.newLookup(request, requestId).thenAccept(lookupDataResult =>
			{
				try
				{
					partitionFuture.complete(new PartitionedTopicMetadata(lookupDataResult.partitions));
				}
				catch (Exception e)
				{
					partitionFuture.completeExceptionally(new PulsarClientException.LookupException(format("Failed to parse partition-response redirect=%s, topic=%s, partitions with %s", lookupDataResult.redirect, topicName.ToString(), lookupDataResult.partitions, e.Message)));
				}
			}).exceptionally((e) =>
			{
				log.warn("[{}] failed to get Partitioned metadata : {}", topicName.ToString(), e.Cause.Message, e);
				partitionFuture.completeExceptionally(e);
				return null;
			});
			}).exceptionally(connectionException =>
			{
			partitionFuture.completeExceptionally(connectionException);
			return null;
		});

			return partitionFuture;
		}

		public virtual ValueTask<Option<SchemaInfo>> GetSchema(TopicName topicName)
		{
			return GetSchema(topicName, null);
		}


		public virtual ValueTask<Option<SchemaInfo>> GetSchema(TopicName topicName, sbyte[] version)
		{
			return client.CnxPool.getConnection(serviceNameResolver.resolveHost()).thenCompose(clientCnx =>
			{
			long requestId = client.newRequestId();
			ByteBuf request = Commands.newGetSchema(requestId, topicName.ToString(), Optional.ofNullable(BytesSchemaVersion.of(version)));
			return clientCnx.sendGetSchema(request, requestId);
			});
		}

		public virtual string ServiceUrl
		{
			get
			{
				return serviceNameResolver.ServiceUrl;
			}
		}

		public virtual ValueTask<IList<string>> GetTopicsUnderNamespace(NamespaceName @namespace, Mode mode)
		{
			ValueTask<IList<string>> topicsFuture = new ValueTask<IList<string>>();

			AtomicLong opTimeoutMs = new AtomicLong(client.Configuration.OperationTimeoutMs);
			Backoff backoff = (new BackoffBuilder()).setInitialTime(100, TimeUnit.MILLISECONDS).setMandatoryStop(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).setMax(0, TimeUnit.MILLISECONDS).create();
			getTopicsUnderNamespace(serviceNameResolver.resolveHost(), @namespace, backoff, opTimeoutMs, topicsFuture, mode);
			return topicsFuture;
		}

		private void GetTopicsUnderNamespace(InetSocketAddress socketAddress, NamespaceName @namespace, Backoff backoff, AtomicLong remainingTime, CompletableFuture<IList<string>> topicsFuture, Mode mode)
		{
			client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = client.newRequestId();
			ByteBuf request = Commands.newGetTopicsOfNamespaceRequest(@namespace.ToString(), requestId, mode);
			clientCnx.newGetTopicsOfNamespace(request, requestId).thenAccept(topicsList =>
			{
				if (log.DebugEnabled)
				{
					log.debug("[namespace: {}] Success get topics list in request: {}", @namespace.ToString(), requestId);
				}
				IList<string> result = Lists.newArrayList();
				topicsList.forEach(topic =>
				{
					string filtered = TopicName.get(topic).PartitionedTopicName;
					if (!result.contains(filtered))
					{
						result.add(filtered);
					}
				});
				topicsFuture.complete(result);
			}).exceptionally((e) =>
			{
				topicsFuture.completeExceptionally(e);
				return null;
			});
			}).exceptionally((e) =>
			{
			long nextDelay = Math.Min(backoff.next(), remainingTime.get());
			if (nextDelay <= 0)
			{
				topicsFuture.completeExceptionally(new PulsarClientException.TimeoutException(format("Could not get topics of namespace %s within configured timeout", @namespace.ToString())));
				return null;
			}
			((ScheduledExecutorService) executor).schedule(() =>
			{
				log.warn("[namespace: {}] Could not get connection while getTopicsUnderNamespace -- Will try again in {} ms", @namespace, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				getTopicsUnderNamespace(socketAddress, @namespace, backoff, remainingTime, topicsFuture, mode);
			}, nextDelay, TimeUnit.MILLISECONDS);
			return null;
		});
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws Exception
		public void Close()
		{
			// no-op
		}

		public class LookupDataResult
		{

			public readonly string brokerUrl;
			public readonly string brokerUrlTls;
			public readonly int partitions;
			public readonly bool authoritative;
			public readonly bool proxyThroughServiceUrl;
			public readonly bool redirect;

			public LookupDataResult(CommandLookupTopicResponse result)
			{
				this.brokerUrl = result.BrokerServiceUrl;
				this.brokerUrlTls = result.BrokerServiceUrlTls;
				this.authoritative = result.Authoritative;
				this.redirect = result.Response == CommandLookupTopicResponse.LookupType.Redirect;
				this.proxyThroughServiceUrl = result.ProxyThroughServiceUrl;
				this.partitions = -1;
			}

			public LookupDataResult(int partitions) : base()
			{
				this.partitions = partitions;
				this.brokerUrl = null;
				this.brokerUrlTls = null;
				this.authoritative = false;
				this.proxyThroughServiceUrl = false;
				this.redirect = false;
			}

		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BinaryProtoLookupService));
	}

}