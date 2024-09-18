using Akka.Actor;
using SharpPulsar.Common.Naming;
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
using System.Threading.Tasks;
using SharpPulsar.Messages.Client;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.ServiceName;
using Mode = SharpPulsar.Protocol.Proto.CommandGetTopicsOfNamespace.Mode;
using PartitionedTopicMetadata = SharpPulsar.Common.Partition.PartitionedTopicMetadata;
using SharpPulsar.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DotNetty.Common.Utilities;

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
    internal class BinaryProtoLookupService : ReceiveActor, IWithUnboundedStash, IWithTimers
    {
        private readonly ServiceNameResolver _serviceNameResolver;
        private readonly bool _useTls;
        private readonly string _listenerName;
        private readonly int _maxLookupRedirects;
        private readonly TimeSpan _operationTimeout;
        private readonly TimeSpan _timeCnx;
        private TimeSpan _opTime;
        private readonly IActorRef _connectionPool;
        private readonly IActorRef _generator;
        private IActorRef _clientCnx;
        private readonly ILoggingAdapter _log;
        private long _requestId = -1;
        private bool _duringConnect = false;
        private int _randomKeyForSelectConnection;

        public BinaryProtoLookupService(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, TimeSpan operationTimeout, TimeSpan timeCnx)
        {
            _log = Context.GetLogger();
            _useTls = useTls;
            _maxLookupRedirects = maxLookupRedirects;
            _serviceNameResolver = new PulsarServiceNameResolver(_log);
            _listenerName = listenerName;
            _operationTimeout = operationTimeout;
            _connectionPool = connectionPool;
            _timeCnx = timeCnx;
            _generator = idGenerator;   
            UpdateServiceUrl(serviceUrl);
            /*
             LatencyHistogram histo = client.instrumentProvider().newLatencyHistogram("pulsar.client.lookup.duration",
                "Duration of lookup operations", null,
                Attributes.builder().put("pulsar.lookup.transport-type", "binary").build());
        histoGetBroker = histo.withAttributes(Attributes.builder().put("pulsar.lookup.type", "topic").build());
        histoGetTopicMetadata =
                histo.withAttributes(Attributes.builder().put("pulsar.lookup.type", "metadata").build());
        histoGetSchema = histo.withAttributes(Attributes.builder().put("pulsar.lookup.type", "schema").build());
        histoListTopics = histo.withAttributes(Attributes.builder().put("pulsar.lookup.type", "list-topics").build());
             */
        }
        private void UpdateServiceUrl(string serviceUrl)
        {
            _serviceNameResolver.UpdateServiceUrl(serviceUrl);
            //Sender.Tell(0);
            Become(PublicCommands);
        }
        private void PublicCommands()
        {

            Receive<SetClient>(c => { });
            Receive<UpdateServiceUrl>(u => UpdateServiceUrl(u.ServiceUrl));
            Receive<GetUpdateServiceUrl>(u =>
            {
                UpdateServiceUrl(u.ServiceUrl);
                Sender.Tell(GetServiceUrl());
            });
            Receive<GetServiceUrl>(_ => Sender.Tell(GetServiceUrl()));
            Receive<GetResolvedHost>(_ => Sender.Tell(ResolveHost()));
            ReceiveAsync<GetBroker>(async broke =>
            {
                try
                {
                    await GetCnxAndRequestId();
                    Become(PrivateCommands);
                    await GetBroker(broke);
                }
                catch (Exception ex)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                }
            });
            ReceiveAsync<GetPartitionedTopicMetadata>(async p =>
            {
                try
                {
                    await GetCnxAndRequestId();
                    await PartitionedTopicMetadata(p.TopicName, p.MetadataAutoCreationEnabled, p.UseFallbackForNonPIP344Brokers);
                }
                catch (Exception e)
                {
                    Sender.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
                    //Become(Awaiting);
                }
            });
            ReceiveAsync<GetSchema>(async s => await Schema(s));
            ReceiveAsync<GetTopicsUnderNamespace>(async t =>
            {
                await GetCnxAndRequestId();
                Become(PrivateCommands);
                await TopicsUnderNamespaceAsync(t);
            });
           

        }
        private void PrivateCommands()
        {
            ReceiveAsync<SetFindBroker>(async set =>
            {
                await GetCnxAndRequestId(set.Address);
                await FindBroker(set.Topic, set.RedirectCount, set.Address, set.Authoritative, set.Sender);
            });
            ReceiveAsync<SetTopicsUnderNamespace>(async set =>
            {
                await GetCnxAndRequestId();
                _log.Warning($"Retrying 'GetTopicsUnderNamespace' after {set.NextDelay} ms delay with requestid '{_requestId}'");
                await TopicsUnderNamespace(set.Ns, set.Backoff, set.Mode, set.TopicsPattern, set.TopicsHash, set.OpTimeout, set.Sender);
            });
            ReceiveAny(s => Stash.Stash());
        }
        public string GetServiceUrl()
        {
            return _serviceNameResolver.ServiceUrl;
        }
       
        public Uri ResolveHost()
        {
            return _serviceNameResolver.ResolveHost();
        }
        private async ValueTask Schema(GetSchema s)
        {
            try
            {
                await GetCnxAndRequestId();
                await GetSchema(s.TopicName, s.Version);
            }
            catch (Exception e)
            {
                Sender.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
            }
           
        }

        private async ValueTask TopicsUnderNamespaceAsync(GetTopicsUnderNamespace t)
        {
            try
            {
                var opTimeout = _operationTimeout;
                var backOff = new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(100)).SetMandatoryStop(opTimeout.Multiply(2)).SetMax(TimeSpan.FromMinutes(1)).Create();
                
                await TopicsUnderNamespace(t.Namespace, backOff, t.Mode, t.TopicsPattern, t.TopicsHash, opTimeout, Sender);                
            }
            catch (Exception e)
            {
                Sender.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
                Become(PublicCommands);
            }
        }
        private async ValueTask TopicsUnderNamespace(NamespaceName ns, Backoff backoff, Mode mode, string topicsPattern, string topicsHash, TimeSpan opTimeout, IActorRef sender)
        {
            try
            {
                var request = Commands.NewGetTopicsOfNamespaceRequest(ns.ToString(), _requestId, mode, topicsPattern, topicsHash);
                var payload = new Payload(request, _requestId, "NewGetTopicsOfNamespaceRequest");
                var askResponse = await _clientCnx.Ask<AskResponse>(payload, _timeCnx);
                var response = askResponse.ConvertTo<GetTopicsOfNamespaceResponse>();
                if (_log.IsDebugEnabled)
                {
                    _log.Debug($"[namespace: {ns}] Successfully got {response.Response.Topics.Count} topics list in request: {_requestId}");
                }
                var res = response.Response;
                var result = new List<string>();
                var tpics = res.Topics.Where(x => !x.Contains("__transaction")).ToArray();
                foreach (var topic in tpics)
                {
                    var filtered = TopicName.Get(topic).PartitionedTopicName;
                    if (!result.Contains(filtered))
                    {
                        result.Add(filtered);
                    }
                }
                //_replyTo.Tell(new AskResponse(new GetTopicsUnderNamespaceResponse(result)));
                _opTime = opTimeout;
                sender.Tell(new AskResponse(new GetTopicsUnderNamespaceResponse(result, res.TopicsHash, res.Changed, res.Filtered)));
            }
            catch (Exception ex)
            {
                if (ex.Message == "Unable to write data to the transport connection: An established connection was aborted by the software in your host machine..")
                {
                    sender.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                    UnstashAll();
                    return;
                }
                var nextDelay = Math.Min(backoff.Next(), opTimeout.TotalMilliseconds);

                if (nextDelay <= 0)
                {
                    _opTime = opTimeout;
                    sender.Tell(new AskResponse(PulsarClientException.Unwrap(new Exception($"TimeoutException: Could not get topics of namespace {ns} within configured timeout"))));
                }
                else
                {
                    _log.Warning($"[namespace: {ns}] Could not get connection while getTopicsUnderNamespace -- Will try again in {nextDelay} ms");

                    _opTime = opTimeout - TimeSpan.FromMilliseconds(nextDelay);
                    //await Task.Delay(TimeSpan.FromMilliseconds(nextDelay));

                    //var reqId = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);

                    //_requestId = reqId.Id;

                    
                    Timers.StartSingleTimer(SetTopicsUnderNamespace.Instance, 
                        new SetTopicsUnderNamespace(ns, backoff, mode, topicsPattern, topicsHash, _opTime, sender, nextDelay), TimeSpan.FromMilliseconds(nextDelay));
                    //_self.Tell(false);
                    return;
                }
            }
            UnstashAll();
        }
        private void UnstashAll()
        {
            Stash?.UnstashAll();
            Become(PublicCommands);
        }
        private async ValueTask GetBroker(GetBroker broker)
        {
            var socketAddress = _serviceNameResolver.ResolveHost().ToDnsEndPoint();
            await FindBroker(broker.TopicName, 0, socketAddress, false);
        }
        private async ValueTask FindBroker(TopicName topic, int redirectCount, DnsEndPoint address, bool authoritative, IActorRef sender = null)
        {
            IActorRef replyTo = null;
            if(sender == null)
                replyTo = Sender;   
            else
                replyTo = sender;

            var socketAddress = address ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
            if (_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
            {
                var err = new Exception("LookupException: Too many redirects: " + _maxLookupRedirects);
                _log.Error(err.ToString());
                
                replyTo.Tell(new AskResponse(new PulsarClientException(err)));
                UnstashAll();
                return;
            }
            var askResponse = await NewLookup(topic, authoritative);
            if (askResponse.Failed)
            {
                replyTo.Tell(askResponse);
                // lookup failed
                if (redirectCount > 0)
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug($"[{topic}] lookup redirection failed ({redirectCount}) : {askResponse.Exception.Message}");
                    }
                }
                else
                {
                    _log.Warning($"[{topic}] lookup failed : {askResponse.Exception.Message}");
                }
                UnstashAll();
                return;
            }
            var data = askResponse.ConvertTo<LookupDataResult>();
            if (data.Error != ServerError.UnknownError)
            {
                _log.Warning($"[{topic}] failed to send lookup request: {data.Error}:{data.ErrorMessage}");
                if (_log.IsDebugEnabled)
                {
                    _log.Warning($"[{topic}] Lookup response exception> {data.Error}:{data.ErrorMessage}");
                }
                replyTo.Tell(new AskResponse(new PulsarClientException(new Exception($"Lookup is not found: {data.Error}:{data.ErrorMessage}"))));

            }
            else
            {
                Uri uri = null;
                try
                {
                    // (1) build response broker-address
                    if (_useTls)
                    {
                        uri = new Uri(data.BrokerUrlTls);
                    }
                    else
                    {
                        var serviceUrl = data.BrokerUrl;
                        uri = new Uri(serviceUrl);
                    }
                    var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);

                    // (2) redirect to given address if response is: redirect
                    if (data.Redirect)
                    {                        
                        Self.Tell(new SetFindBroker(topic, redirectCount + 1, responseBrokerAddress, data.Authoritative, replyTo));
                        return;
                    }
                    else
                    {
                        var response = data.ProxyThroughServiceUrl ?
                            new GetBrokerResponse(responseBrokerAddress, socketAddress) :
                            new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
                        replyTo.Tell(new AskResponse(response));
                    }
                }
                catch (Exception parseUrlException)
                {
                    _log.Warning($"[{topic}] invalid url {uri}");
                    replyTo.Tell(new AskResponse(new PulsarClientException(parseUrlException)));
                }
            }
            UnstashAll();
        }
        private async ValueTask GetCnxAndRequestId()
        {
            _clientCnx = null;
            _requestId = -1;
            var address = _serviceNameResolver.ResolveHost().ToDnsEndPoint();
            var ask = await _connectionPool.Ask<AskResponse>(new GetConnection(address));
            if (ask.Failed)
                throw ask.Exception;
            var o = ask.ConvertTo<ConnectionOpened>();
            _clientCnx = o.ClientCnx;
            var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
            _requestId = id.Id;
        }
        private async ValueTask GetCnxAndRequestId(DnsEndPoint dnsEndPoint)
        {
            _clientCnx = null;
            _requestId = -1;
            var address = dnsEndPoint;
            var ask = await _connectionPool.Ask<AskResponse>(new GetConnection(address));
            if (ask.Failed)
                throw ask.Exception;
            var o = ask.ConvertTo<ConnectionOpened>();
            _clientCnx = o.ClientCnx;
            var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
            _requestId = id.Id;
        }

        /// <summary>
        /// Calls broker binaryProto-lookup api to find broker-service address which can serve a given topic.
        /// </summary>
        /// <param name="topicName">topic-name </param>
        /// <param name="authoritative"></param>
        /// <returns> broker-socket-address that serves given topic </returns>
        private async ValueTask<AskResponse> NewLookup(TopicName topicName, bool authoritative = false)
        {
            var request = Commands.NewLookup(topicName.ToString(), _listenerName, authoritative, _requestId);
            var payload = new Payload(request, _requestId, "NewLookup");
            return await _clientCnx.Ask<AskResponse>(payload);
        }

        /// <summary>
        /// calls broker binaryProto-lookup api to get metadata of partitioned-topic.
        /// 
        /// </summary>
        
        private async ValueTask PartitionedTopicMetadata(TopicName topicName, bool metadataAutoCreationEnabled, bool useFallbackForNonPIP344Brokers)
        {
            var finalAutoCreationEnabled = metadataAutoCreationEnabled;
            var autoCreation = await _clientCnx.Ask<bool>(IsSupportsGetPartitionedMetadataWithoutAutoCreation.Instance);

            if (!metadataAutoCreationEnabled && !autoCreation)
            {
                if (useFallbackForNonPIP344Brokers)
                {
                    _log.Info($"[{topicName}] Using original behavior of getPartitionedTopicMetadata(topic) in "
                            + "getPartitionedTopicMetadata(topic, false) "
                            + "since the target broker does not support PIP-344 and fallback is enabled.");
                    finalAutoCreationEnabled = true;
                }
                else
                {
                    Sender.Tell(new AskResponse(new NotSupportedException($"The feature of getting partitions without auto-creation is not supported by the broker  Please upgrade the broker to version that supports PIP-344 to resolve this "
                                    + $"issue.{autoCreation}")));
                    return;
                }
            }

            var request = Commands.NewPartitionMetadataRequest(topicName.ToString(), _requestId, metadataAutoCreationEnabled);
            var payload = new Payload(request, _requestId, "NewPartitionMetadataRequest");
            var askResponse = await _clientCnx.Ask<AskResponse>(payload, _timeCnx);
            if (askResponse.Failed)
            {
                Sender.Tell(askResponse);
                return;
            }
            var data = askResponse.ConvertTo<LookupDataResult>();

            if (data?.Error != ServerError.UnknownError)
            {
                _log.Warning($"[{topicName}] failed to get Partitioned metadata : {data.Error}:{data.ErrorMessage}");
                Sender.Tell(new AskResponse(new PartitionedTopicMetadata(0)));
            }
            else
            {
                Sender.Tell(new AskResponse(new PartitionedTopicMetadata(data.Partitions)));
            }
            _connectionPool.Tell(new ReleaseConnection(_clientCnx));
            //_getPartitionedTopicMetadataBackOff = null;
            
            //Stash?.UnstashAll();
            //Become(Awaiting);
        }
        private async ValueTask GetSchema(TopicName topicName, byte[] version)
		{
			var request = Commands.NewGetSchema(_requestId, topicName.ToString(), BytesSchemaVersion.Of(version));
			var payload = new Payload(request, _requestId, "SendGetRawSchema");
			var askResponse = await _clientCnx.Ask<AskResponse>(payload);

            if (askResponse.Failed)
            {
                Sender.Tell(askResponse);
                return;
            }

            var schemaResponse = askResponse.ConvertTo<Messages.GetSchemaResponse>();
            var err = schemaResponse.Response.ErrorCode;
            if (err != ServerError.UnknownError)
            {
                var e = $"{err}: {schemaResponse.Response.ErrorMessage}";
                _log.Error(e);
                Sender.Tell(new AskResponse(new PulsarClientException(new Exception(e))));
            }
            else
            {
                var schema = schemaResponse.Response.Schema;
                var info = new SchemaInfo
                {
                    Schema = schema.SchemaData,
                    Name = schema.Name,
                    Properties = schema.Properties.ToDictionary(k => k.Key, v => v.Value),
                    Type = SchemaType.ValueOf((int)schema.type)
                };
                Sender.Tell(new AskResponse(new GetSchemaInfoResponse(info)));
            }
            _connectionPool.Tell(new ReleaseConnection(_clientCnx));
        }

		public string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

        public IStash Stash { get; set; }
        public ITimerScheduler Timers { get; set; }

        
		protected override void Unhandled(object message)
        {
			_log.Info($"Unhandled {message.GetType().FullName} received");
            base.Unhandled(message);
        }
        protected override void PreStart()
        {
            base.PreStart();
        }
        public static Props Prop(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, TimeSpan operationTimeout, TimeSpan timeCnx)
        {
			return Props.Create(() => new BinaryProtoLookupService(connectionPool, idGenerator, serviceUrl, listenerName, useTls, maxLookupRedirects, operationTimeout, timeCnx));
        }
    }
    internal sealed class RetryGetTopicsUnderNamespace
	{
		public NamespaceName Namespace { get; }
		public Mode Mode { get; }
		public long OpTimeOutMs { get; }
		public RetryGetTopicsUnderNamespace(NamespaceName nsn, Mode mode, long opTimeout)
		{
			Mode = mode;
			Namespace = nsn;
			OpTimeOutMs = opTimeout;
		}
    }
    internal record struct SetFindBroker(TopicName Topic, int RedirectCount, DnsEndPoint Address, bool Authoritative, IActorRef Sender);
    internal record struct SetTopicsUnderNamespace(NamespaceName Ns, Backoff Backoff, Mode Mode, string TopicsPattern, string TopicsHash, TimeSpan OpTimeout, IActorRef Sender, double NextDelay)
    {
        internal static SetTopicsUnderNamespace Instance = new SetTopicsUnderNamespace();   
    }
}