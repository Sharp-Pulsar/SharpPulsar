using Akka.Actor;
using Akka.Event;
using BAMCIS.Util.Concurrent;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Partition;
using SharpPulsar.Impl;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Requests;
using SharpPulsar.Model;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Protocol.Schema;
using SharpPulsar.Schemas;
using SharpPulsar.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
	public class BinaryProtoLookupService : ReceiveActor, IWithUnboundedStash
	{
		private readonly IActorRef _client;
		private readonly ServiceNameResolver _serviceNameResolver;
		private readonly bool _useTls;
		private readonly string _listenerName;
		private readonly int _maxLookupRedirects;
		private readonly long _operationTimeoutMs;
		private readonly IActorRef _connectionPool;
		private readonly ILoggingAdapter _log;
		private IAdvancedScheduler _executor;
		private IActorContext _context;

		public BinaryProtoLookupService(IActorRef client, IActorRef connectionPool, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs)
		{
			_context = Context;
			_executor = Context.System.Scheduler.Advanced;
			_log = Context.GetLogger();
			_client = client;
			_useTls = useTls;
			_maxLookupRedirects = maxLookupRedirects;
			_serviceNameResolver = new PulsarServiceNameResolver(_log);
			_listenerName = listenerName;
			_operationTimeoutMs = operationTimeoutMs;
			_connectionPool = connectionPool;
			UpdateServiceUrl(serviceUrl);
			Normal();
		}
		private void UpdateServiceUrl(string serviceUrl)
		{
			_serviceNameResolver.UpdateServiceUrl(serviceUrl);
		}
		private void Normal()
        {
			Receive<UpdateService>(u =>
			{
				UpdateServiceUrl(u.Service);

			});
			Receive<GetBroker>(b => 
			{
				var pool = _connectionPool;
				pool.Tell(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				Become(()=>GetConnection(b));			
			});
			Receive<GetPartitionedTopicMetadata>(p => 
			{
				var pool = _connectionPool;
				pool.Tell(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				Become(() => GetConnection(p));
			});
			Receive<GetSchema>(s => 
			{
				var pool = _connectionPool;
				pool.Tell(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				Become(() => GetConnection(s));
			});
			Receive<GetTopicsUnderNamespace>(t => 
			{
				var pool = _connectionPool;
				pool.Tell(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				Become(() => GetConnection(t));
			});
		}
		private void GetConnection(object message)
        {
			Receive<GetConnectionResponse>(c => 
			{
				var client = _client;
				client.Tell(NewRequestId.Instance);

				var msg = message;
				Become(()=> GetRequestId(msg, c.ClientCnx));
			});
			ReceiveAny(a => Stash.Stash());
        }
		private void GetRequestId(object message, IActorRef cnx)
        {
			Receive<NewRequestIdResponse>(r => 
			{
				var obj = message;
				var clientCnx = cnx;
				switch(obj)
                {
					case GetBroker broker:
                        {
							Become(() => GetBroker(broker.TopicName, r.Id, clientCnx, broker.ReplyTo));
						}
						break;
					case GetBrokerRedirect broker:
                        {
							Become(() => GetBroker(broker.TopicName, r.Id, clientCnx, broker.ReplyTo, broker.RedirectCount, broker.BrokerAddress, broker.Authoritative));
						}
						break;
					case GetPartitionedTopicMetadata metadata:
						Become(() => GetPartitionedTopicMetadata(metadata.TopicName, r.Id, clientCnx, metadata.ReplyTo));
						break;
					case GetTopicsUnderNamespace @namespace:
						Become(() => GetTopicsUnderNamespace(@namespace, r.Id, clientCnx, @namespace.ReplyTo));
						break;
					case GetTopicsOfNamespaceRetry tr:
						Become(() => GetTopicsUnderNamespace(r.Id, tr.Namespace, tr.Backoff, tr.RemainingTime, tr.Mode, clientCnx, tr.ReplyTo));
						break;
					case GetSchema s:
						Become(() => GetSchema(s.TopicName, s.Version, r.Id, clientCnx, s.ReplyTo));
						break;
					default:
						_log.Debug($"Unknown Request : {obj.GetType().FullName}");
						break;

				}
			});
			ReceiveAny(a => Stash.Stash());
		}
		/// <summary>
		/// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
		/// </summary>
		/// <param name="topicName">
		///            topic-name </param>
		/// <returns> broker-socket-address that serves given topic </returns>
		private void GetBroker(TopicName topicName, long requestId, IActorRef clientCnx, IActorRef replyTo, int redirectCount = 0, DnsEndPoint address = null, bool authoritative = false)
		{
			var socketAddress = address ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
			FindBroker(authoritative, topicName, redirectCount, requestId, clientCnx, replyTo);

			Receive<LookupDataResult>(lookup => 
			{

				if (Enum.IsDefined(typeof(ServerError), lookup.Error))
				{
					_log.Warning($"[{topicName}] failed to send lookup request: {lookup.Error}:{lookup.ErrorMessage}");
					if (_log.IsDebugEnabled)
					{
						_log.Warning($"[{topicName}] Lookup response exception> {lookup.Error}:{lookup.ErrorMessage}");
					}
					replyTo.Tell(new Failure { Exception = new Exception($"Lookup is not found: {lookup.Error}:{lookup.ErrorMessage}") });
					Become(Normal);
					Stash.Unstash();
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
							var redirectBroker = new GetBrokerRedirect(topicName, replyTo, redirectCount + 1, responseBrokerAddress, lookup.Authoritative);
							Self.Tell(redirectBroker);
							return;
						}
						else
						{
							if (lookup.ProxyThroughServiceUrl)
							{
								var response = new GetBrokerResponse(responseBrokerAddress, socketAddress);
								replyTo.Tell(response);
							}
							else
							{
								var response = new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
								replyTo.Tell(response);
							}
						}
					}
					catch (Exception parseUrlException)
					{
						_log.Warning($"[{topicName}] invalid url {uri}");
						replyTo.Tell(new Failure { Exception = parseUrlException });
					}
                    finally
                    {
						Become(Normal);
						Stash.Unstash();
					}
				}

			});
			Receive<GetBrokerRedirect>(r => 
			{
				_connectionPool.Tell(new GetConnection(r.BrokerAddress));
				Become(() => GetConnection(r));
			});
			ReceiveAny(a => Stash.Stash());
		}

		public void FindBroker(bool authoritative, TopicName topicName, int redirectCount, long requestId, IActorRef clientCnx, IActorRef replyTo)
		{
			if(_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
			{
				var err = new Exception("LookupException: Too many redirects: " + _maxLookupRedirects);
				_log.Error(err.ToString());
				replyTo.Tell(new Failure { Exception = err});

				Become(Normal);
				Stash.Unstash();
				return;
			}
			var request = Commands.NewLookup(topicName.ToString(), _listenerName, authoritative, requestId);
			var payload = new Payload(request, requestId, "NewLookup");
			clientCnx.Tell(payload);
		}

		/// <summary>
		/// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
		/// 
		/// </summary>
		private void GetPartitionedTopicMetadata(TopicName topicName, long requestId, IActorRef clientCnx, IActorRef reply)
		{
			var request = Commands.NewPartitionMetadataRequest(topicName.ToString(), requestId);
			var payload = new Payload(request, requestId, "NewPartitionMetadataRequest");
			clientCnx.Tell(payload);
			Receive<LookupDataResult>(m =>
			{
				if (Enum.IsDefined(typeof(ServerError), m.Error))
				{
					_log.Warning($"[{topicName}] failed to get Partitioned metadata : {m.Error}:{m.ErrorMessage}");
					reply.Tell(new Failure { Exception = new Exception($"[{topicName}] failed to get Partitioned metadata. {m.Error}:{m.ErrorMessage}")});
					return;
				}
				else
				{
					reply.Tell(new PartitionedTopicMetadata(m.Partitions));
				}

				Become(Normal);
				Stash.Unstash();

			});
			ReceiveAny(a => Stash.Stash());
		}


		private void GetSchema(TopicName topicName, sbyte[] version, long requestId, IActorRef clientCnx, IActorRef replyTo)
		{
			var request = Commands.NewGetSchema(requestId, topicName.ToString(), BytesSchemaVersion.Of(version));
			var payload = new Payload(request, requestId, "SendGetRawSchema");
			clientCnx.Tell(payload);
			Receive<Messages.GetSchemaResponse>(s=> 
			{
				var err = s.Response.ErrorCode;
				if(Enum.IsDefined(typeof(ServerError), err))
                {
					var e = $"{err}: {s.Response.ErrorMessage}";
					_log.Error(e);
					replyTo.Tell(new Failure { Exception = new Exception(e) });
                }
                else
                {
					var schema = s.Response.Schema;
					var info = new SchemaInfo
					{
						Schema = (sbyte[])(object)schema.SchemaData,
						Name = schema.Name,
						Properties = schema.Properties.ToDictionary(k => k.Key, v => v.Value),
						Type = SchemaType.ValueOf((int)schema.type)
					};
					replyTo.Tell(new GetSchemaInfoResponse(info));
				}
				Become(Normal);
				Stash.Unstash();
			});
			ReceiveAny(a => Stash.Stash());

		}

		public string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

        public IStash Stash { get; set; }

        private void GetTopicsUnderNamespace(GetTopicsUnderNamespace nsn, long requestid, IActorRef clientCnx, IActorRef reply)
		{
			var opTimeoutMs = _operationTimeoutMs;
			var backoff = new BackoffBuilder().SetInitialTime(100, TimeUnit.MILLISECONDS).SetMandatoryStop(opTimeoutMs * 2, TimeUnit.MILLISECONDS).SetMax(1, TimeUnit.MINUTES).Create();
			GetTopicsUnderNamespace(requestid, nsn.Namespace, backoff, opTimeoutMs, nsn.Mode, clientCnx, reply);			
		}

		private void GetTopicsUnderNamespace(long requestId, NamespaceName @namespace, Backoff backoff, long remainingTime, Mode mode, IActorRef clientCnx, IActorRef replyTo)
		{
			var request = Commands.NewGetTopicsOfNamespaceRequest(@namespace.ToString(), requestId, mode);
			var payload = new Payload(request, requestId, "NewGetTopicsOfNamespaceRequest");
			_context.SetReceiveTimeout(TimeSpan.FromMilliseconds(_operationTimeoutMs));
			clientCnx.Tell(payload);
			Receive<GetTopicsOfNamespaceResponse>(n => 
			{
				_context.SetReceiveTimeout(null);
				if (_log.IsDebugEnabled)
				{
					_log.Debug($"[namespace: {@namespace}] Success get topics list in request: {requestId}");
				}
				var result = new List<string>();
				n.Response.Topics.ForEach(topic =>
				{
					var filtered = TopicName.Get(topic).PartitionedTopicName;
					if (!result.Contains(filtered))
					{
						result.Add(filtered);
					}
				});
				replyTo.Tell(new GetTopicsUnderNamespaceResponse(result));
				Become(Normal);
				Stash.Unstash();
			});
			Receive<GetTopicsOfNamespaceRetry>(r =>
			{
				_context.SetReceiveTimeout(null);
				_connectionPool.Tell(new GetConnection(_serviceNameResolver.ResolveHost().ToDnsEndPoint()));
				Become(() => GetConnection(r));

			});
			Receive<ReceiveTimeout>(t =>
			{
				var ns = @namespace;
				var bkOff = backoff;
				var mde = mode;
				var remaining = remainingTime;
				var nextDelay = Math.Min(backoff.Next(), remaining);
				var reply = replyTo;
				if (nextDelay <= 0)
				{
					replyTo.Tell(new Failure { Exception = new Exception($"TimeoutException: Could not get topics of namespace {ns} within configured timeout") });

					_context.SetReceiveTimeout(null);
				}
                else
                {
					_log.Warning($"[namespace: {ns}] Could not get connection while getTopicsUnderNamespace -- Will try again in {nextDelay} ms");
					remaining -= nextDelay;
					var retry = new GetTopicsOfNamespaceRetry(ns, bkOff, remaining, mode, reply);
					_executor.ScheduleOnce(TimeSpan.FromMilliseconds(TimeUnit.MILLISECONDS.ToMilliseconds(nextDelay)), () => 
					{
						var rt = retry;
						var self = _context.Self;
						_context.SetReceiveTimeout(TimeSpan.FromMilliseconds(_operationTimeoutMs));
						self.Tell(rt);
					});
				}
			});
			ReceiveAny(a => Stash.Stash());
		}
        protected override void Unhandled(object message)
        {
			_log.Info($"Unhandled {message.GetType().FullName} received");
            base.Unhandled(message);
        }
		public static Props Prop(IActorRef client, IActorRef connectionPool, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, long operationTimeoutMs)
        {
			return Props.Create(() => new BinaryProtoLookupService(client, connectionPool, serviceUrl, listenerName, useTls, maxLookupRedirects, operationTimeoutMs));
        }
    }

}