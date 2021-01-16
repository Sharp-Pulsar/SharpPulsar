using Akka.Actor;
using Akka.Event;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Impl;
using SharpPulsar.Interfaces.ISchema;
using SharpPulsar.Messages;
using SharpPulsar.Model;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace;

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
namespace SharpPulsar
{ 
	public class BinaryProtoLookupService
	{

		private readonly IActorRef _client;
		private readonly ServiceNameResolver _serviceNameResolver;
		private readonly bool _useTls;
		private readonly string _listenerName;
		private readonly int _maxLookupRedirects;
		private readonly long _operationTimeoutMs;
		private readonly ConnectionPool _connectionPool;
		private readonly ILoggingAdapter _log;

		public BinaryProtoLookupService(IActorRef client, ConnectionPool connectionPool, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs, ILoggingAdapter log)
		{
			_log = log;
			_client = client;
			_useTls = useTls;
			_maxLookupRedirects = maxLookupRedirects;
			_serviceNameResolver = new PulsarServiceNameResolver(log);
			_listenerName = listenerName;
			_operationTimeoutMs = operationTimeoutMs;
			_connectionPool = connectionPool;
			UpdateServiceUrl(serviceUrl);
		}
		public virtual void UpdateServiceUrl(string serviceUrl)
		{
			_serviceNameResolver.UpdateServiceUrl(serviceUrl);
		}

		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		public (DnsEndPoint logicalAddress, DnsEndPoint physicalAddress) GetBroker(TopicName topicName)
		{
			return FindBroker(_serviceNameResolver.ResolveHost().ToDnsEndPoint(), false, topicName, 0);
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		private PartitionedTopicMetadata GetPartitionedTopicMetadata(TopicName topicName)
		{
			return GetPartitionedTopicMetadata(_serviceNameResolver.ResolveHost().ToDnsEndPoint(), topicName);
		}

		public (DnsEndPoint logicalAddress, DnsEndPoint physicalAddress) FindBroker(DnsEndPoint socketAddress, bool authoritative, TopicName topicName, int redirectCount)
		{
			if(_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
			{
				throw new PulsarClientException.LookupException("Too many redirects: " + _maxLookupRedirects);
			}
			var clientCnx = _connectionPool.GetConnection(socketAddress);
			long requestId = _client.NewRequestId();
			var request = Commands.NewLookup(topicName.ToString(), _listenerName, authoritative, requestId);
			var payload = new Payload(request, requestId, "NewLookup");
			var lookup = clientCnx.Ask<LookupDataResult>(payload).Result;
			if (lookup == null)
			{
				_log.Warning($"[{topicName}] failed to send lookup request");
				if (_log.IsDebugEnabled)
				{
					_log.Warning($"[{topicName}] Lookup response exception");
				}
				throw new Exception("Lookup is not found");
			}
			else
			{
				Uri uri = null;
				try
				{
					if (_useTls)
					{
						uri = new Uri(lookup.BrokerUrlTls);
					}
					else
					{
						string serviceUrl = lookup.BrokerUrl;
						uri = new Uri(serviceUrl);
					}
					var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);
					if (lookup.Redirect)
					{
						return FindBroker(responseBrokerAddress, lookup.Authoritative, topicName, redirectCount + 1);
					}
					else
					{
						if (lookup.ProxyThroughServiceUrl)
						{
							return (responseBrokerAddress, socketAddress);
						}
						else
						{
							return (responseBrokerAddress, responseBrokerAddress);
						}
					}
				}
				catch (Exception parseUrlException)
				{
					_log.Warning($"[{topicName}] invalid url {uri}");
					addressFuture.completeExceptionally(parseUrlException);
				}
			}
			_connectionPool.ReleaseConnection(clientCnx);
			return null;
		}

		private async Task<PartitionedTopicMetadata> GetPartitionedTopicMetadata(DnsEndPoint socketAddress, TopicName topicName)
		{

			_client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = _client.NewRequestId();
			ByteBuf request = Commands.NewPartitionMetadataRequest(topicName.ToString(), requestId);
			clientCnx.newLookup(request, requestId).whenComplete((r, t) =>
			{
				if(t != null)
				{
					_log.warn("[{}] failed to get Partitioned metadata : {}", topicName.ToString(), t.Message, t);
					partitionFuture.completeExceptionally(t);
				}
				else
				{
					try
					{
						partitionFuture.complete(new PartitionedTopicMetadata(r.partitions));
					}
					catch(Exception e)
					{
						partitionFuture.completeExceptionally(new PulsarClientException.LookupException(format("Failed to parse partition-response redirect=%s, topic=%s, partitions with %s", r.redirect, topicName.ToString(), r.partitions, e.Message)));
					}
				}
				_client.CnxPool.releaseConnection(clientCnx);
			});
			}).exceptionally(connectionException =>
			{
			partitionFuture.completeExceptionally(connectionException);
			return null;
		});

			return partitionFuture;
		}

		public virtual ISchemaInfo GetSchema(TopicName topicName)
		{
			return GetSchema(topicName, null);
		}


		public virtual ISchemaInfo GetSchema(TopicName topicName, sbyte[] version)
		{
			InetSocketAddress socketAddress = _serviceNameResolver.ResolveHost();
			CompletableFuture<Optional<SchemaInfo>> schemaFuture = new CompletableFuture<Optional<SchemaInfo>>();

			_client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = _client.NewRequestId();
			ByteBuf request = Commands.NewGetSchema(requestId, topicName.ToString(), Optional.ofNullable(BytesSchemaVersion.Of(version)));
			clientCnx.sendGetSchema(request, requestId).whenComplete((r, t) =>
			{
				if(t != null)
				{
					_log.warn("[{}] failed to get schema : {}", topicName.ToString(), t.Message, t);
					schemaFuture.completeExceptionally(t);
				}
				else
				{
					schemaFuture.complete(r);
				}
				_client.CnxPool.releaseConnection(clientCnx);
			});
			}).exceptionally(ex =>
			{
			schemaFuture.completeExceptionally(ex);
			return null;
		});

			return schemaFuture;
		}

		public virtual string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

		public virtual IList<string> GetTopicsUnderNamespace(NamespaceName @namespace, Mode mode)
		{
			CompletableFuture<IList<string>> topicsFuture = new CompletableFuture<IList<string>>();

			AtomicLong opTimeoutMs = new AtomicLong(_client.Configuration.OperationTimeoutMs);
			Backoff backoff = (new BackoffBuilder()).SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs.get() * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
			GetTopicsUnderNamespace(_serviceNameResolver.ResolveHost(), @namespace, backoff, opTimeoutMs, topicsFuture, mode);
			return topicsFuture;
		}

		private void GetTopicsUnderNamespace(InetSocketAddress socketAddress, NamespaceName @namespace, Backoff backoff, AtomicLong remainingTime, CompletableFuture<IList<string>> topicsFuture, Mode mode)
		{
			_client.CnxPool.getConnection(socketAddress).thenAccept(clientCnx =>
			{
			long requestId = _client.NewRequestId();
			ByteBuf request = Commands.NewGetTopicsOfNamespaceRequest(@namespace.ToString(), requestId, mode);
			clientCnx.newGetTopicsOfNamespace(request, requestId).whenComplete((r, t) =>
			{
				if(t != null)
				{
					topicsFuture.completeExceptionally(t);
				}
				else
				{
					if(_log.DebugEnabled)
					{
						_log.debug("[namespace: {}] Success get topics list in request: {}", @namespace.ToString(), requestId);
					}
					IList<string> result = Lists.newArrayList();
					r.forEach(topic =>
					{
						string filtered = TopicName.Get(topic).PartitionedTopicName;
						if(!result.Contains(filtered))
						{
							result.Add(filtered);
						}
					});
					topicsFuture.complete(result);
				}
				_client.CnxPool.releaseConnection(clientCnx);
			});
			}).exceptionally((e) =>
			{
			long nextDelay = Math.Min(backoff.Next(), remainingTime.get());
			if(nextDelay <= 0)
			{
				topicsFuture.completeExceptionally(new PulsarClientException.TimeoutException(format("Could not get topics of namespace %s within configured timeout", @namespace.ToString())));
				return null;
			}
			((ScheduledExecutorService) _executor).schedule(() =>
			{
				_log.warn("[namespace: {}] Could not get connection while getTopicsUnderNamespace -- Will try again in {} ms", @namespace, nextDelay);
				remainingTime.addAndGet(-nextDelay);
				GetTopicsUnderNamespace(socketAddress, @namespace, backoff, remainingTime, topicsFuture, mode);
			}, nextDelay, TimeUnit.MILLISECONDS);
			return null;
		});
		}

		public override void close()
		{
			// no-op
		}


	}

}