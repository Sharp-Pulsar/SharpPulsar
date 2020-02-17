using Microsoft.Extensions.Logging;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpPulsar.Common.Schema;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Utility.Atomic;
using SharpPulsar.Utils;
using PulsarClientException = SharpPulsar.Exceptions.PulsarClientException;

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
	public sealed class BinaryProtoLookupService : ILookupService
	{

		private readonly PulsarClientImpl _client;
		private readonly ServiceNameResolver _serviceNameResolver;
		private readonly bool _useTls;
		private readonly ScheduledThreadPoolExecutor _executor;
		public BinaryProtoLookupService(PulsarClientImpl client, string serviceUrl, bool useTls, ScheduledThreadPoolExecutor executor)
		{
			_executor = executor;
			_client = client;
			_useTls = useTls;
			_serviceNameResolver = new PulsarServiceNameResolver();
			UpdateServiceUrl(serviceUrl);
		}

		public void UpdateServiceUrl(string serviceUrl)
		{
			_serviceNameResolver.UpdateServiceUrl(serviceUrl);
		}

		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		public ValueTask<KeyValuePair<EndPoint, EndPoint>> GetBroker(TopicName topicName)
		{
			return FindBroker(_serviceNameResolver.ResolveHost(), false, topicName);
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		public ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(TopicName topicName)
		{
			return GetPartitionedTopicMetadata(_serviceNameResolver.ResolveHost(), topicName);
		}

		private ValueTask<KeyValuePair<EndPoint, EndPoint>> FindBroker(IPEndPoint socketAddress, bool authoritative, TopicName topicName)
		{
			var addressTask = new TaskCompletionSource<KeyValuePair<EndPoint, EndPoint>>();

			_client.CnxPool.GetConnection(socketAddress).AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    addressTask.SetException(task.Exception ?? throw new InvalidOperationException());
                    return;
				}
                var clientCnx = task.Result;
				var requestId = _client.NewRequestId();
				var request = Commands.NewLookup(topicName.ToString(), authoritative, requestId);
				clientCnx.NewLookup(request, requestId).Task.ContinueWith(tsk =>
                {
                    if (tsk.IsFaulted)
                    {
                        if (tsk.Exception != null)
                            Log.LogWarning("[{}] failed to send lookup request : {}", topicName.ToString(),
                                tsk.Exception.Message);
                        if (Log.IsEnabled(LogLevel.Debug))
                        {
                            Log.LogWarning("[{}] Lookup response exception: {}", topicName.ToString(), tsk.Exception);
                        }
                        addressTask.SetException(tsk.Exception ?? throw new InvalidOperationException());
                        return;
					}
                    var lookupDataResult = tsk.Result;

					Uri uri = null;
				    try
				    {
					    if (_useTls)
					    {
						    uri = new Uri(lookupDataResult.BrokerUrlTls);
					    }
					    else
					    {
						    string serviceUrl = lookupDataResult.BrokerUrl;
						    uri = new Uri(serviceUrl);
					    }
					    var responseBrokerAddress = new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port); 
					    if (lookupDataResult.Redirect)
					    {
						    FindBroker(responseBrokerAddress, lookupDataResult.Authoritative, topicName).AsTask().ContinueWith(taskAddr =>
                            {
                                if (taskAddr.IsFaulted)
                                {
                                    if (taskAddr.Exception != null)
                                    {
                                        Log.LogWarning("[{}] lookup failed : {}", topicName.ToString(),
                                            taskAddr.Exception.Message, taskAddr.Exception);
                                        addressTask.SetException(taskAddr.Exception);
                                    }

                                    return;
								}
                                var addressPair = taskAddr.Result;
                                addressTask.SetResult(addressPair);
						    });
					    }
					    else
					    {
						    if (lookupDataResult.ProxyThroughServiceUrl)
						    {
							    addressTask.SetResult(new KeyValuePair<EndPoint, EndPoint>(responseBrokerAddress, socketAddress));
						    }
						    else
						    {
								addressTask.SetResult(new KeyValuePair<EndPoint, EndPoint>(responseBrokerAddress, responseBrokerAddress));
							}
					    }
				    }
				    catch (System.Exception parseUrlException)
				    {
					    Log.LogWarning("[{}] invalid url {} : {}", topicName.ToString(), uri, parseUrlException.Message, parseUrlException);
					    addressTask.SetException(parseUrlException);
				    }
			    });
			});
			return new ValueTask<KeyValuePair<EndPoint, EndPoint>>(addressTask.Task);
		}

		private ValueTask<PartitionedTopicMetadata> GetPartitionedTopicMetadata(IPEndPoint socketAddress, TopicName topicName)
		{

			var partitionTask = new TaskCompletionSource<PartitionedTopicMetadata>();

			_client.CnxPool.GetConnection(socketAddress).AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    partitionTask.SetException(task.Exception ?? throw new InvalidOperationException());
                    return;
				}
                var clientCnx = task.Result;

				var requestId = _client.NewRequestId();
			    var request = Commands.NewPartitionMetadataRequest(topicName.ToString(), requestId);
			    clientCnx.NewLookup(request, requestId).Task.ContinueWith(tskNew =>
                {
                    if (tskNew.IsFaulted)
                    {
                        Log.LogWarning("[{}] failed to get Partitioned metadata : {}", topicName.ToString(), tskNew.Exception.Message, tskNew.Exception);
                        partitionTask.SetException(tskNew.Exception ?? throw new InvalidOperationException());
                        return;
					}
                    var lookupDataResult = tskNew.Result;

					try
				    {
					    partitionTask.SetResult(new PartitionedTopicMetadata(lookupDataResult.Partitions));
				    }
				    catch (System.Exception e)
				    {
					    partitionTask.SetException(new PulsarClientException.LookupException(string.Format("Failed to parse partition-response redirect=%s, topic=%s, partitions with %s", lookupDataResult.Redirect, topicName.ToString(), lookupDataResult.Partitions, e.Message)));
				    }
			    });
			});

			return new ValueTask<PartitionedTopicMetadata>(partitionTask.Task);
		}

		public ValueTask<SchemaInfo> GetSchema(TopicName topicName)
		{
			return GetSchema(topicName, null);
		}


		public ValueTask<SchemaInfo> GetSchema(TopicName topicName, sbyte[] version)
		{
			var r = _client.CnxPool.GetConnection(_serviceNameResolver.ResolveHost()).AsTask().ContinueWith(task =>
            {
                var clientCnx = task.Result;

				var requestId = _client.NewRequestId();
			    var request = Commands.NewGetSchema(requestId, topicName.ToString(), BytesSchemaVersion.Of(version));
			    return clientCnx.SendGetSchema(request, requestId);
			});
			return r.Result;
		}

		public string ServiceUrl => _serviceNameResolver.ServiceUrl;

        public ValueTask<IList<string>> GetTopicsUnderNamespace(NamespaceName @namespace, CommandGetTopicsOfNamespace.Types.Mode mode)
		{
			var topicsTask = new TaskCompletionSource<IList<string>>();

			var opTimeoutMs = new AtomicLong(_client.Configuration.OperationTimeoutMs);
			var backoff = (new BackoffBuilder()).SetInitialTime(100, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs.Get() * 2, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).SetMax(0, BAMCIS.Util.Concurrent.TimeUnit.MILLISECONDS).Create();
			GetTopicsUnderNamespace(_serviceNameResolver.ResolveHost(), @namespace, backoff, opTimeoutMs, topicsTask, mode);
			return new ValueTask<IList<string>>(topicsTask.Task);
		}

		private void GetTopicsUnderNamespace(IPEndPoint socketAddress, NamespaceName @namespace, Backoff backoff, AtomicLong remainingTime, TaskCompletionSource<IList<string>> topicsTask, CommandGetTopicsOfNamespace.Types.Mode mode)
		{
			_client.CnxPool.GetConnection(socketAddress).AsTask().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    var nextDelay = Math.Min(backoff.Next(), remainingTime.Get());
                    if (nextDelay <= 0)
                    {
                        topicsTask.SetException(new PulsarClientException.TimeoutException(string.Format("Could not get topics of namespace %s within configured timeout", @namespace.ToString())));
                        return;
                    }
                    _executor.Schedule(() =>
                    {
                        Log.LogWarning("[namespace: {}] Could not get connection while getTopicsUnderNamespace -- Will try again in {} ms", @namespace, nextDelay);
                        remainingTime.AddAndGet(-nextDelay);
                        GetTopicsUnderNamespace(socketAddress, @namespace, backoff, remainingTime, topicsTask, mode);
                    }, TimeSpan.FromMilliseconds(nextDelay));
                    return;
				}
                var clientCnx = task.Result;

				var requestId = _client.NewRequestId();
			    var request = Commands.NewGetTopicsOfNamespaceRequest(@namespace.ToString(), requestId, mode);
			    clientCnx.NewGetTopicsOfNamespace(request, requestId).Task.ContinueWith(tsk =>
                {
                    var topicsList = tsk.Result;
                    if (tsk.IsFaulted)
                    {
                        topicsTask.SetException(tsk.Exception ?? throw new InvalidOperationException());
                        return;
					}
					if (Log.IsEnabled(LogLevel.Debug))
				    {
					    Log.LogDebug("[namespace: {}] Success get topics list in request: {}", @namespace.ToString(), requestId);
				    }
				    IList<string> result = new List<string>();
				    topicsList.ToList().ForEach(topic =>
				    {
					    var filtered = TopicName.Get(topic).PartitionedTopicName;
					    if (!result.Contains(filtered))
					    {
						    result.Add(filtered);
					    }
				    });
				    topicsTask.SetResult(result);
			    });
			});
		}

		public void Close()
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

			public LookupDataResult(CommandLookupTopicResponse result)
			{
				BrokerUrl = result.BrokerServiceUrl;
				BrokerUrlTls = result.BrokerServiceUrlTls;
				Authoritative = result.Authoritative;
				Redirect = result.Response == CommandLookupTopicResponse.Types.LookupType.Redirect;
				ProxyThroughServiceUrl = result.ProxyThroughServiceUrl;
				Partitions = -1;
			}

			public LookupDataResult(int partitions) : base()
			{
				Partitions = partitions;
				BrokerUrl = null;
				BrokerUrlTls = null;
				Authoritative = false;
				ProxyThroughServiceUrl = false;
				Redirect = false;
			}

		}
		private static readonly ILogger Log = new LoggerFactory().CreateLogger<BinaryProtoLookupService>();
        public void Dispose()
        {
            Close();
        }
    }

}