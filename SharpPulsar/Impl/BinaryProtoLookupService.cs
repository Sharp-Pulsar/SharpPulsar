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

	using ByteBuf = io.netty.buffer.ByteBuf;


	using Pair = org.apache.commons.lang3.tuple.Pair;
	using PulsarClientException = SharpPulsar.Api.PulsarClientException;
	using Commands = Org.Apache.Pulsar.Common.Protocol.Commands;
	using Mode = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandGetTopicsOfNamespace.Mode;
	using CommandLookupTopicResponse = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandLookupTopicResponse;
	using LookupType = Org.Apache.Pulsar.Common.Api.Proto.PulsarApi.CommandLookupTopicResponse.LookupType;
	using NamespaceName = Org.Apache.Pulsar.Common.Naming.NamespaceName;
	using TopicName = Org.Apache.Pulsar.Common.Naming.TopicName;
	using PartitionedTopicMetadata = Org.Apache.Pulsar.Common.Partition.PartitionedTopicMetadata;
	using BytesSchemaVersion = Org.Apache.Pulsar.Common.Protocol.Schema.BytesSchemaVersion;
	using SchemaInfo = Org.Apache.Pulsar.Common.Schema.SchemaInfo;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

	public class BinaryProtoLookupService : LookupService
	{

		private readonly PulsarClientImpl client;
		private readonly ServiceNameResolver serviceNameResolver;
		private readonly bool useTls;
		private readonly ExecutorService executor;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BinaryProtoLookupService(PulsarClientImpl client, String serviceUrl, boolean useTls, java.util.concurrent.ExecutorService executor) throws SharpPulsar.api.PulsarClientException
		public BinaryProtoLookupService(PulsarClientImpl Client, string ServiceUrl, bool UseTls, ExecutorService Executor)
		{
			this.client = Client;
			this.useTls = UseTls;
			this.executor = Executor;
			this.serviceNameResolver = new PulsarServiceNameResolver();
			UpdateServiceUrl(ServiceUrl);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void updateServiceUrl(String serviceUrl) throws SharpPulsar.api.PulsarClientException
		public override void UpdateServiceUrl(string ServiceUrl)
		{
			serviceNameResolver.UpdateServiceUrl(ServiceUrl);
		}

		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		public virtual CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>> GetBroker(TopicName TopicName)
		{
			return FindBroker(serviceNameResolver.ResolveHost(), false, TopicName);
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		public virtual CompletableFuture<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName TopicName)
		{
			return GetPartitionedTopicMetadata(serviceNameResolver.ResolveHost(), TopicName);
		}

		private CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>> FindBroker(InetSocketAddress SocketAddress, bool Authoritative, TopicName TopicName)
		{
			CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>> AddressFuture = new CompletableFuture<Pair<InetSocketAddress, InetSocketAddress>>();

			client.CnxPool.getConnection(SocketAddress).thenAccept(clientCnx =>
			{
			long RequestId = client.NewRequestId();
			ByteBuf Request = Commands.newLookup(TopicName.ToString(), Authoritative, RequestId);
			clientCnx.newLookup(Request, RequestId).thenAccept(lookupDataResult =>
			{
				URI Uri = null;
				try
				{
					if (useTls)
					{
						Uri = new URI(lookupDataResult.brokerUrlTls);
					}
					else
					{
						string ServiceUrl = lookupDataResult.brokerUrl;
						Uri = new URI(ServiceUrl);
					}
					InetSocketAddress ResponseBrokerAddress = InetSocketAddress.createUnresolved(Uri.Host, Uri.Port);
					if (lookupDataResult.redirect)
					{
						FindBroker(ResponseBrokerAddress, lookupDataResult.authoritative, TopicName).thenAccept(addressPair =>
						{
							AddressFuture.complete(addressPair);
						}).exceptionally((lookupException) =>
						{
							log.warn("[{}] lookup failed : {}", TopicName.ToString(), lookupException.Message, lookupException);
							AddressFuture.completeExceptionally(lookupException);
							return null;
						});
					}
					else
					{
						if (lookupDataResult.proxyThroughServiceUrl)
						{
							AddressFuture.complete(Pair.of(ResponseBrokerAddress, SocketAddress));
						}
						else
						{
							AddressFuture.complete(Pair.of(ResponseBrokerAddress, ResponseBrokerAddress));
						}
					}
				}
				catch (Exception ParseUrlException)
				{
					log.warn("[{}] invalid url {} : {}", TopicName.ToString(), Uri, ParseUrlException.Message, ParseUrlException);
					AddressFuture.completeExceptionally(ParseUrlException);
				}
			}).exceptionally((sendException) =>
			{
				log.warn("[{}] failed to send lookup request : {}", TopicName.ToString(), sendException.Message);
				if (log.DebugEnabled)
				{
					log.warn("[{}] Lookup response exception: {}", TopicName.ToString(), sendException);
				}
				AddressFuture.completeExceptionally(sendException);
				return null;
			});
			}).exceptionally(connectionException =>
			{
			AddressFuture.completeExceptionally(connectionException);
			return null;
		});
			return AddressFuture;
		}

		private CompletableFuture<PartitionedTopicMetadata> GetPartitionedTopicMetadata(InetSocketAddress SocketAddress, TopicName TopicName)
		{

			CompletableFuture<PartitionedTopicMetadata> PartitionFuture = new CompletableFuture<PartitionedTopicMetadata>();

			client.CnxPool.getConnection(SocketAddress).thenAccept(clientCnx =>
			{
			long RequestId = client.NewRequestId();
			ByteBuf Request = Commands.newPartitionMetadataRequest(TopicName.ToString(), RequestId);
			clientCnx.newLookup(Request, RequestId).thenAccept(lookupDataResult =>
			{
				try
				{
					PartitionFuture.complete(new PartitionedTopicMetadata(lookupDataResult.partitions));
				}
				catch (Exception E)
				{
					PartitionFuture.completeExceptionally(new PulsarClientException.LookupException(format("Failed to parse partition-response redirect=%s, topic=%s, partitions with %s", lookupDataResult.redirect, TopicName.ToString(), lookupDataResult.partitions, E.Message)));
				}
			}).exceptionally((e) =>
			{
				log.warn("[{}] failed to get Partitioned metadata : {}", TopicName.ToString(), e.Cause.Message, e);
				PartitionFuture.completeExceptionally(e);
				return null;
			});
			}).exceptionally(connectionException =>
			{
			PartitionFuture.completeExceptionally(connectionException);
			return null;
		});

			return PartitionFuture;
		}

		public override CompletableFuture<Optional<SchemaInfo>> GetSchema(TopicName TopicName)
		{
			return GetSchema(TopicName, null);
		}


		public override CompletableFuture<Optional<SchemaInfo>> GetSchema(TopicName TopicName, sbyte[] Version)
		{
			return client.CnxPool.getConnection(serviceNameResolver.ResolveHost()).thenCompose(clientCnx =>
			{
			long RequestId = client.NewRequestId();
			ByteBuf Request = Commands.newGetSchema(RequestId, TopicName.ToString(), Optional.ofNullable(BytesSchemaVersion.of(Version)));
			return clientCnx.sendGetSchema(Request, RequestId);
			});
		}

		public virtual string ServiceUrl
		{
			get
			{
				return serviceNameResolver.ServiceUrl;
			}
		}

		public override CompletableFuture<IList<string>> GetTopicsUnderNamespace(NamespaceName Namespace, Mode Mode)
		{
			CompletableFuture<IList<string>> TopicsFuture = new CompletableFuture<IList<string>>();

			AtomicLong OpTimeoutMs = new AtomicLong(client.Configuration.OperationTimeoutMs);
			Backoff Backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMandatoryStop(OpTimeoutMs.get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).setMax(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).create();
			GetTopicsUnderNamespace(serviceNameResolver.ResolveHost(), Namespace, Backoff, OpTimeoutMs, TopicsFuture, Mode);
			return TopicsFuture;
		}

		private void GetTopicsUnderNamespace(InetSocketAddress SocketAddress, NamespaceName Namespace, Backoff Backoff, AtomicLong RemainingTime, CompletableFuture<IList<string>> TopicsFuture, Mode Mode)
		{
			client.CnxPool.getConnection(SocketAddress).thenAccept(clientCnx =>
			{
			long RequestId = client.NewRequestId();
			ByteBuf Request = Commands.newGetTopicsOfNamespaceRequest(Namespace.ToString(), RequestId, Mode);
			clientCnx.newGetTopicsOfNamespace(Request, RequestId).thenAccept(topicsList =>
			{
				if (log.DebugEnabled)
				{
					log.debug("[namespace: {}] Success get topics list in request: {}", Namespace.ToString(), RequestId);
				}
				IList<string> Result = Lists.newArrayList();
				topicsList.forEach(topic =>
				{
					string Filtered = TopicName.get(topic).PartitionedTopicName;
					if (!Result.Contains(Filtered))
					{
						Result.Add(Filtered);
					}
				});
				TopicsFuture.complete(Result);
			}).exceptionally((e) =>
			{
				TopicsFuture.completeExceptionally(e);
				return null;
			});
			}).exceptionally((e) =>
			{
			long NextDelay = Math.Min(Backoff.next(), RemainingTime.get());
			if (NextDelay <= 0)
			{
				TopicsFuture.completeExceptionally(new PulsarClientException.TimeoutException(format("Could not get topics of namespace %s within configured timeout", Namespace.ToString())));
				return null;
			}
			((ScheduledExecutorService) executor).schedule(() =>
			{
				log.warn("[namespace: {}] Could not get connection while getTopicsUnderNamespace -- Will try again in {} ms", Namespace, NextDelay);
				RemainingTime.addAndGet(-NextDelay);
				GetTopicsUnderNamespace(SocketAddress, Namespace, Backoff, RemainingTime, TopicsFuture, Mode);
			}, NextDelay, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS);
			return null;
		});
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws Exception
		public override void Close()
		{
			// no-op
		}

		public class LookupDataResult
		{

			public readonly string BrokerUrl;
			public readonly string BrokerUrlTls;
			public readonly int Partitions;
			public readonly bool Authoritative;
			public readonly bool ProxyThroughServiceUrl;
			public readonly bool Redirect;

			public LookupDataResult(CommandLookupTopicResponse Result)
			{
				this.BrokerUrl = Result.BrokerServiceUrl;
				this.BrokerUrlTls = Result.BrokerServiceUrlTls;
				this.Authoritative = Result.Authoritative;
				this.Redirect = Result.Response == CommandLookupTopicResponse.LookupType.Redirect;
				this.ProxyThroughServiceUrl = Result.ProxyThroughServiceUrl;
				this.Partitions = -1;
			}

			public LookupDataResult(int Partitions) : base()
			{
				this.Partitions = Partitions;
				this.BrokerUrl = null;
				this.BrokerUrlTls = null;
				this.Authoritative = false;
				this.ProxyThroughServiceUrl = false;
				this.Redirect = false;
			}

		}

		private static readonly Logger log = LoggerFactory.getLogger(typeof(BinaryProtoLookupService));
	}

}