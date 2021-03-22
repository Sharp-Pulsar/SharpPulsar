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
    public class BinaryProtoLookupService : ReceiveActor, IWithUnboundedStash
	{
		private readonly ServiceNameResolver _serviceNameResolver;
		private readonly bool _useTls;
		private readonly string _listenerName;
		private readonly int _maxLookupRedirects;
		private readonly long _operationTimeoutMs;
		private readonly IActorRef _connectionPool;
		private readonly IActorRef _generator;
		private IActorRef _replyTo;
		private long _requestId;
		private IActorRef _clientCnx;
		private readonly ILoggingAdapter _log;
		private IActorContext _context;
		private Action<object[]> _nextBecome;
		private object[] _invokeArg;
		private Backoff _backOff;

		public BinaryProtoLookupService(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs)
		{
			_generator = idGenerator;
			_context = Context;
			_log = Context.GetLogger();
			_useTls = useTls;
			_maxLookupRedirects = maxLookupRedirects;
			_serviceNameResolver = new PulsarServiceNameResolver(_log);
			_listenerName = listenerName;
			_operationTimeoutMs = operationTimeoutMs;
			_connectionPool = connectionPool;
			UpdateServiceUrl(serviceUrl);
			Awaiting();
		}
		private void UpdateServiceUrl(string serviceUrl)
		{
			_serviceNameResolver.UpdateServiceUrl(serviceUrl);
		}
		private void Awaiting()
        {
			Receive<SetClient>(c =>
			{
				//_pulsarClient = c.Client;

			});
			Receive<UpdateServiceUrl>(u =>
			{
				UpdateServiceUrl(u.ServiceUrl);

			});
			Receive<GetBroker>(b => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { b};
				_nextBecome = GetBroker;
				Become(()=> GetCnxAndRequestId());			
			});
			Receive<GetPartitionedTopicMetadata>(p =>
			{
				_replyTo = Sender;
				_invokeArg = new object[] { p.TopicName };
				_nextBecome = GetPartitionedTopicMetadata;
				Become(() => GetCnxAndRequestId());
			});
			Receive<GetSchema>(s => 
			{
				_replyTo = Sender;
				_invokeArg = new object[] { s.TopicName, s.Version};
				_nextBecome = GetSchema;
				Become(() => GetCnxAndRequestId());
			});
			Receive<GetTopicsUnderNamespace>(t => 
			{
				var opTimeoutMs = _operationTimeoutMs;
				_backOff = new BackoffBuilder().SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
				_replyTo = Sender;
				_invokeArg = new object[] { t, opTimeoutMs };
				_nextBecome = GetTopicsUnderNamespace;
				Become(() => GetCnxAndRequestId());
			});
			Stash.Unstash();
		}
		private void GetBroker(object[] b)
        {
			var broker = b[0] as GetBroker;
			var socketAddress = _serviceNameResolver.ResolveHost().ToDnsEndPoint();
			NewLookup(broker.TopicName);
			Receive<LookupDataResult>(data => 
			{
				var br = broker;
				if (Enum.IsDefined(typeof(ServerError), data.Error) && !string.IsNullOrWhiteSpace(data.ErrorMessage))
				{
					_log.Warning($"[{br.TopicName}] failed to send lookup request: {data.Error}:{data.ErrorMessage}");
					if (_log.IsDebugEnabled)
					{
						_log.Warning($"[{br.TopicName}] Lookup response exception> {data.Error}:{data.ErrorMessage}");
					}
					_replyTo.Tell(new ClientExceptions(new PulsarClientException(new Exception($"Lookup is not found: {data.Error}:{data.ErrorMessage}"))));
					Become(Awaiting);
				}
				else
				{
					Uri uri = null;
					try
					{
						if (_useTls)
						{
							uri = new Uri(data.BrokerUrlTls);
						}
						else
						{
							string serviceUrl = data.BrokerUrl;
							uri = new Uri(serviceUrl);
						}
						var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);
						if (data.Redirect)
						{
							_invokeArg = new object[]{br.TopicName, 1 , responseBrokerAddress, data.Authoritative};
							_nextBecome = RedirectedGetBroker;
							Become(() => GetCnxAndRequestId(responseBrokerAddress));
						}
						else
						{
							var response = data.ProxyThroughServiceUrl ? 
							new GetBrokerResponse(responseBrokerAddress, socketAddress): 
							new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
							_replyTo.Tell(response);
							Become(Awaiting);
						}
					}
					catch (Exception parseUrlException)
					{
						_log.Warning($"[{br.TopicName}] invalid url {uri}");
						_replyTo.Tell(new ClientExceptions(new PulsarClientException(parseUrlException)));
						Become(Awaiting);
					}
				}
			});
			Receive<ClientExceptions>(m => _replyTo.Tell(m));
			ReceiveAny(_ => Stash.Stash());
		}
		private void RedirectedGetBroker(object[] args)
        {
			var topic = (TopicName)args[0];
			var redirectCount = (int)args[1];
			var authoritative = (bool)args[3];
			var socketAddress = (DnsEndPoint)args[2];
			Receive<LookupDataResult>(data =>
			{
				if (Enum.IsDefined(typeof(ServerError), data.Error) && !string.IsNullOrWhiteSpace(data.ErrorMessage))
				{
					_log.Warning($"[{topic}] failed to send lookup request: {data.Error}:{data.ErrorMessage}");
					if (_log.IsDebugEnabled)
					{
						_log.Warning($"[{topic}] Lookup response exception> {data.Error}:{data.ErrorMessage}");
					}
					_replyTo.Tell(new ClientExceptions(new PulsarClientException(new Exception($"Lookup is not found: {data.Error}:{data.ErrorMessage}"))));
					Become(Awaiting);
				}
				else
				{
					Uri uri = null;
					try
					{
						if (_useTls)
						{
							uri = new Uri(data.BrokerUrlTls);
						}
						else
						{
							string serviceUrl = data.BrokerUrl;
							uri = new Uri(serviceUrl);
						}
						var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);
						if (data.Redirect)
						{
							_invokeArg = _invokeArg = new object[] { topic, redirectCount + 1, responseBrokerAddress, data.Authoritative };
							_nextBecome = RedirectedGetBroker;
							Become(() => GetCnxAndRequestId(responseBrokerAddress));
						}
						else
						{
							var response = data.ProxyThroughServiceUrl ?
							new GetBrokerResponse(responseBrokerAddress, socketAddress) :
							new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
							_replyTo.Tell(response);
							Become(Awaiting);
						}
					}
					catch (Exception parseUrlException)
					{
						_log.Warning($"[{topic}] invalid url {uri}");
						_replyTo.Tell(new ClientExceptions(new PulsarClientException(parseUrlException)));
						Become(Awaiting);
					}
				}
			});
			Receive<ClientExceptions>(m => _replyTo.Tell(m));
			ReceiveAny(_ => Stash.Stash());
			NewLookup(topic, redirectCount, socketAddress, authoritative);
		}
		private void GetCnxAndRequestId(DnsEndPoint dnsEndPoint = null)
        {
			_clientCnx = null;
			_requestId = -1;
			var address = dnsEndPoint ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
			Receive<GetConnectionResponse>(m => 
			{
				_clientCnx = m.ClientCnx;
				if (_requestId > -1)
					Become(()=>_nextBecome.Invoke(_invokeArg));
			});
			Receive<NewRequestIdResponse>(m => 
			{
				_requestId = m.Id;
				if (_clientCnx != null)
					Become(() => _nextBecome.Invoke(_invokeArg));
			});
			ReceiveAny(_=> Stash.Stash());
			_connectionPool.Tell(new GetConnection(address));
			_generator.Tell(NewRequestId.Instance);
		}
		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		private void NewLookup(TopicName topicName, int redirectCount = 0, DnsEndPoint address = null, bool authoritative = false)
		{
			var socketAddress = address ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
			if (_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
			{
				var err = new Exception("LookupException: Too many redirects: " + _maxLookupRedirects);
				_log.Error(err.ToString());
				_replyTo.Tell(new ClientExceptions(new PulsarClientException(err)));
				Become(Awaiting);
				return;
			}
			var request = new Commands().NewLookup(topicName.ToString(), _listenerName, authoritative, _requestId);
			var payload = new Payload(request, _requestId, "NewLookup");
			_clientCnx.Tell(payload);

		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		private void GetPartitionedTopicMetadata(object[] args)
		{
			var topicName = (TopicName)args[0];
			Receive<LookupDataResult>(data=> 
			{
				if (Enum.IsDefined(typeof(ServerError), data.Error) && data.ErrorMessage != null)
				{
					_log.Warning($"[{topicName}] failed to get Partitioned metadata : {data.Error}:{data.ErrorMessage}");
					_replyTo.Tell(new PartitionedTopicMetadata(0));
					return;
				}
				else
				{
					_replyTo.Tell(new PartitionedTopicMetadata(data.Partitions));
				}
				Become(Awaiting);
			});
			Receive<ClientExceptions>(ex => 
			{
				_replyTo.Tell(ex);
				Become(Awaiting);
			});
			ReceiveAny(_ => Stash.Stash());
			var request = new Commands().NewPartitionMetadataRequest(topicName.ToString(), _requestId);
			var payload = new Payload(request, _requestId, "NewPartitionMetadataRequest");
			 _clientCnx.Tell(payload);
		}


		private void GetSchema(object[] args)
		{
			
			Receive<Messages.GetSchemaResponse>(schemaResponse =>
			{
				var err = schemaResponse.Response.ErrorCode;
				if (Enum.IsDefined(typeof(ServerError), err) && schemaResponse.Response.ErrorMessage != null)
				{
					var e = $"{err}: {schemaResponse.Response.ErrorMessage}";
					_log.Error(e);
					_replyTo.Tell(new Failure { Exception = new Exception(e) });
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
					_replyTo.Tell(new GetSchemaInfoResponse(info));
				}
				Become(Awaiting);
			});
			ReceiveAny(_ => Stash.Stash());
			var topicName = (TopicName)args[0];
			var version = (sbyte[])args[1];
			var request = new Commands().NewGetSchema(_requestId, topicName.ToString(), BytesSchemaVersion.Of(version));
			var payload = new Payload(request, _requestId, "SendGetRawSchema");
			_clientCnx.Tell(payload);
		}

		public string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

        public IStash Stash { get; set; }

        private void GetTopicsUnderNamespace(object[] args)
		{
			var nsn = (GetTopicsUnderNamespace)args[0];
			var opTimeoutMs = (long)args[1];
			Receive<GetTopicsOfNamespaceResponse>(response=> 
			{
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[namespace: {nsn.Namespace}] Success get topics list in request: {_requestId}");
				}
				var result = new List<string>();
				foreach(var topic in response.Response.Topics)
                {
					var filtered = TopicName.Get(topic).PartitionedTopicName;
					if (!result.Contains(filtered))
					{
						result.Add(filtered);
					}
				}
				_replyTo.Tell(new GetTopicsUnderNamespaceResponse(result));
				Become(Awaiting);
			});
			Receive<ClientExceptions>(ex => 
			{
				var ns = nsn.Namespace;
				var mde = nsn.Mode;
				var nextDelay = Math.Min(_backOff.Next(), _operationTimeoutMs);
				var reply = _replyTo;
				if (nextDelay <= 0)
				{
					reply.Tell(new Failure { Exception = new Exception($"TimeoutException: Could not get topics of namespace {ns} within configured timeout") });
					Become(Awaiting);
				}
				else
				{
					_log.Warning($"[namespace: {ns}] Could not get connection while getTopicsUnderNamespace -- Will try again in {nextDelay} ms");
					opTimeoutMs -= nextDelay;
					var task = Task.Run(() => Task.Delay(TimeSpan.FromMilliseconds(nextDelay)));
					_invokeArg = new object[] { ns, opTimeoutMs };
					_nextBecome = GetTopicsUnderNamespace;
					Become(() => GetCnxAndRequestId());
				}
			});
			ReceiveAny(_ => Stash.Stash());
			var request = new Commands().NewGetTopicsOfNamespaceRequest(nsn.Namespace.ToString(), _requestId, nsn.Mode);
			var payload = new Payload(request, _requestId, "NewGetTopicsOfNamespaceRequest");
			_clientCnx.Tell(payload);

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