using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Model;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using SharpPulsar.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SharpPulsar.Messages.Client;
using SharpPulsar.Exceptions;
using SharpPulsar.ServiceName;

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
    public class BinaryProtoLookupService : ReceiveActor
	{
		private IActorRef _pulsarClient;
		private readonly ServiceNameResolver _serviceNameResolver;
		private readonly bool _useTls;
		private readonly string _listenerName;
		private readonly int _maxLookupRedirects;
		private readonly long _operationTimeoutMs;
		private readonly IActorRef _connectionPool;
		private readonly IActorRef _generator;
		private readonly ILoggingAdapter _log;
		private IAdvancedScheduler _executor;
		private IActorContext _context;

		public BinaryProtoLookupService(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs)
		{
			_generator = idGenerator;
			_context = Context;
			_executor = Context.System.Scheduler.Advanced;
			_log = Context.GetLogger();
			_useTls = useTls;
			_maxLookupRedirects = maxLookupRedirects;
			_serviceNameResolver = new PulsarServiceNameResolver(_log);
			_listenerName = listenerName;
			_operationTimeoutMs = operationTimeoutMs;
			_connectionPool = connectionPool;
			UpdateServiceUrl(serviceUrl);
			Receives();
		}
		private void UpdateServiceUrl(string serviceUrl)
		{
			_serviceNameResolver.UpdateServiceUrl(serviceUrl);
		}
		private void Receives()
        {
			Receive<SetClient>(c =>
			{
				_pulsarClient = c.Client;

			});
			Receive<UpdateServiceUrl>(u =>
			{
				UpdateServiceUrl(u.ServiceUrl);

			});
			ReceiveAsync<GetBroker>(async b => 
			{
				var task = new TaskCompletionSource<GetBrokerResponse>();
				var pool = _connectionPool;
				var address = _serviceNameResolver.ResolveHost().ToDnsEndPoint();
				var xtion = await pool.Ask<GetConnectionResponse>(new GetConnection(address));
				var connection = xtion.ClientCnx;
				var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
				var requestid = id.Id;
				await GetBroker(b.TopicName, requestid, connection, task);
                try
                {
					var result = await task.Task;
					Sender.Tell(result);
				}
				catch(Exception ex)
                {
					Sender.Tell(new ClientExceptions((PulsarClientException)ex.InnerException));
				}
			});
			ReceiveAsync<GetPartitionedTopicMetadata>(async p =>
			{
				var pool = _connectionPool;
				var xtion = await pool.Ask<GetConnectionResponse>(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				var connection = xtion.ClientCnx;
				var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
				var requestid = id.Id;
				await GetPartitionedTopicMetadata(p.TopicName, requestid, connection);
			});
			ReceiveAsync<GetSchema>(async s => 
			{
				var sender = Sender;
				var pool = _connectionPool;
				var xtion = await pool.Ask<GetConnectionResponse>(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				var connection = xtion.ClientCnx;
				var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
				var requestid = id.Id;
				await GetSchema(s.TopicName, s.Version, requestid, connection, sender);
			});
			ReceiveAsync<GetTopicsUnderNamespace>( async t => 
			{
				var sender = Sender;
				var pool = _connectionPool;
				var xtion = await pool.Ask<GetConnectionResponse>(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				var connection = xtion.ClientCnx;
				var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
				var requestid = id.Id;
				await GetTopicsUnderNamespace(t, requestid, connection, sender);
			});
		}
		
		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		private async ValueTask GetBroker(TopicName topicName, long requestId, IActorRef clientCnx, TaskCompletionSource<GetBrokerResponse> task, int redirectCount = 0, DnsEndPoint address = null, bool authoritative = false)
		{
			var socketAddress = address ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
			if (_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
			{
				var err = new Exception("LookupException: Too many redirects: " + _maxLookupRedirects);
				_log.Error(err.ToString());
				task.SetException(err);
				return;
			}
			var request = new Commands().NewLookup(topicName.ToString(), _listenerName, authoritative, requestId);
			var payload = new Payload(request, requestId, "NewLookup");
			var lk = await clientCnx.Ask(payload);
			if(lk is LookupDataResult lookup)
            {

				if (Enum.IsDefined(typeof(ServerError), lookup.Error) && !string.IsNullOrWhiteSpace(lookup.ErrorMessage))
				{
					_log.Warning($"[{topicName}] failed to send lookup request: {lookup.Error}:{lookup.ErrorMessage}");
					if (_log.IsDebugEnabled)
					{
						_log.Warning($"[{topicName}] Lookup response exception> {lookup.Error}:{lookup.ErrorMessage}");
					}
					task.SetException(new Exception($"Lookup is not found: {lookup.Error}:{lookup.ErrorMessage}"));
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
							var pool = _connectionPool;
							var xtion = await pool.Ask<GetConnectionResponse>(new GetConnection(responseBrokerAddress));
							var connection = xtion.ClientCnx;
							var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
							requestId = id.Id;
							await GetBroker(topicName, requestId, connection, task, redirectCount + 1, responseBrokerAddress, lookup.Authoritative);
						}
						else
						{
							if (lookup.ProxyThroughServiceUrl)
							{
								var response = new GetBrokerResponse(responseBrokerAddress, socketAddress);
								task.SetResult(response);
							}
							else
							{
								var response = new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
								task.SetResult(response);
							}
						}
					}
					catch (Exception parseUrlException)
					{
						_log.Warning($"[{topicName}] invalid url {uri}");
						task.SetException(parseUrlException);
					}
				}
			}
            else
            {
				var e = lk as ClientExceptions;
				task.SetException(e.Exception);
            }
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		private async ValueTask GetPartitionedTopicMetadata(TopicName topicName, long requestId, IActorRef clientCnx)
		{
			var request = new Commands().NewPartitionMetadataRequest(topicName.ToString(), requestId);
			var payload = new Payload(request, requestId, "NewPartitionMetadataRequest");
			var lk = await clientCnx.Ask(payload);
			if(lk is LookupDataResult lookup)
            {
				if (Enum.IsDefined(typeof(ServerError), lookup.Error) && lookup.ErrorMessage != null)
				{
					_log.Warning($"[{topicName}] failed to get Partitioned metadata : {lookup.Error}:{lookup.ErrorMessage}");
					Sender.Tell(new PartitionedTopicMetadata(0));
					return;
				}
				else
				{
					Sender.Tell(new PartitionedTopicMetadata(lookup.Partitions));
				}
			}
            else
            {
				var ex = lk as ClientExceptions;
				Sender.Tell(ex);
            }
		}


		private async ValueTask GetSchema(TopicName topicName, sbyte[] version, long requestId, IActorRef clientCnx, IActorRef sender)
		{
			var request = new Commands().NewGetSchema(requestId, topicName.ToString(), BytesSchemaVersion.Of(version));
			var payload = new Payload(request, requestId, "SendGetRawSchema");
			var schemaResponse = await clientCnx.Ask<Messages.GetSchemaResponse>(payload);
			var err = schemaResponse.Response.ErrorCode;
			if (Enum.IsDefined(typeof(ServerError), err))
			{
				var e = $"{err}: {schemaResponse.Response.ErrorMessage}";
				_log.Error(e);
				sender.Tell(new Failure { Exception = new Exception(e) });
			}
			else
			{
				var schema = schemaResponse.Response.Schema;
				var info = new SchemaInfo
				{
					Schema = schema.SchemaData.ToSBytes(),
					Name = schema.Name,
					Properties = schema.Properties.ToDictionary(k => k.Key, v => v.Value),
					Type = SchemaType.ValueOf((int)schema.type)
				};
				sender.Tell(new GetSchemaInfoResponse(info));
			}
		}

		public string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

        private async ValueTask GetTopicsUnderNamespace(GetTopicsUnderNamespace nsn, long requestid, IActorRef clientCnx, IActorRef sender)
		{
			var opTimeoutMs = _operationTimeoutMs;
			var backoff = new BackoffBuilder().SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
			
			var request = new Commands().NewGetTopicsOfNamespaceRequest(nsn.Namespace.ToString(), requestid, nsn.Mode);
			var payload = new Payload(request, requestid, "NewGetTopicsOfNamespaceRequest");
			var topics = await clientCnx.Ask(payload);

			while(!(topics is GetTopicsOfNamespaceResponse))
            {
				var ns = nsn.Namespace;
				var bkOff = backoff;
				var mde = nsn.Mode;
				var nextDelay = Math.Min(backoff.Next(), opTimeoutMs);
				var reply = Sender;
				if (nextDelay <= 0)
				{
					reply.Tell(new Failure { Exception = new Exception($"TimeoutException: Could not get topics of namespace {ns} within configured timeout") });
					break;
				}
				else
				{
					_log.Warning($"[namespace: {ns}] Could not get connection while getTopicsUnderNamespace -- Will try again in {nextDelay} ms");
					opTimeoutMs -= nextDelay;
					var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(nextDelay)));
					var reqid = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance); 
					request = new Commands().NewGetTopicsOfNamespaceRequest(nsn.Namespace.ToString(), reqid.Id, nsn.Mode);
					payload = new Payload(request, reqid.Id, "NewGetTopicsOfNamespaceRequest");
					topics = await clientCnx.Ask(payload);
				}
			}
			if (topics is GetTopicsOfNamespaceResponse t)
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[namespace: {nsn.Namespace}] Success get topics list in request: {requestid}");
				}
				var result = new List<string>();
				t.Response.Topics.ForEach(topic =>
				{
					var filtered = TopicName.Get(topic).PartitionedTopicName;
					if (!result.Contains(filtered))
					{
						result.Add(filtered);
					}
				});
				sender.Tell(new GetTopicsUnderNamespaceResponse(result));
			}
			else
				sender.Tell(new GetTopicsUnderNamespaceResponse(new List<string>()));
		}

		protected override void Unhandled(object message)
        {
			_log.Info($"Unhandled {message.GetType().FullName} received");
            base.Unhandled(message);
        }
		public static Props Prop(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs)
        {
			return Props.Create(() => new BinaryProtoLookupService(connectionPool, idGenerator, serviceUrl, listenerName, useTls, maxLookupRedirects, operationTimeoutMs));
        }
    }

}