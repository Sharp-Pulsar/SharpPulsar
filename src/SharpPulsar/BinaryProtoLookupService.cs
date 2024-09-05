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
    internal class BinaryProtoLookupService : ReceiveActor, IWithUnboundedStash
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
        private readonly IActorContext _context;
        private IActorRef _replyTo;
        private IActorRef _self;
        private long _requestId = -1;
        private TopicName _topicName;
        private Backoff _getTopicsUnderNamespaceBackOff;
        private Backoff _getPartitionedTopicMetadataBackOff;
        private GetTopicsUnderNamespace _getTopicsUnderNamespace;
        public BinaryProtoLookupService(IActorRef connectionPool, IActorRef idGenerator, string serviceUrl, string listenerName, bool useTls, int maxLookupRedirects, TimeSpan operationTimeout, TimeSpan timeCnx)
        {
            _self = Self;
            _generator = idGenerator;
            _context = Context;
            _log = Context.GetLogger();
            _useTls = useTls;
            _maxLookupRedirects = maxLookupRedirects;
            _serviceNameResolver = new PulsarServiceNameResolver(_log);
            _listenerName = listenerName;
            _operationTimeout = operationTimeout;
            _connectionPool = connectionPool;
            _timeCnx = timeCnx;

            Receive<SetClient>(c => { });
            Receive<UpdateServiceUrl>(u => UpdateServiceUrl(u.ServiceUrl));
            ReceiveAsync<GetBroker>(async broke => await GetBroker(broke));
            ReceiveAsync<GetPartitionedTopicMetadata>(async p => await PartitionedTopicMetadata(p));
            ReceiveAsync<GetSchema>(async s => await Schema(s));
            ReceiveAsync<GetTopicsUnderNamespace>(async t =>
            {
                _replyTo = Sender;
                _getTopicsUnderNamespace = t;
                await TopicsUnderNamespaceAsync();

                Become(GetTopicsUnderNamespace);
            });
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
            //Become(Awaiting);
        }
        
        private async ValueTask PartitionedTopicMetadata(GetPartitionedTopicMetadata p)
        {
            try
            {
                var opTimeout = _operationTimeout;
                _replyTo = Sender;
                _getPartitionedTopicMetadataBackOff = (new BackoffBuilder()).SetInitialTime(TimeSpan.FromMilliseconds(100)).SetMandatoryStop(opTimeout.Multiply(2)).SetMax(TimeSpan.FromMinutes(1)).Create();

                await GetCnxAndRequestId();
                await GetPartitionedTopicMetadata(p.TopicName, opTimeout);
            }
            catch (Exception e)
            {
                _replyTo.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
                //Become(Awaiting);
            }


        }
        private async ValueTask Schema(GetSchema s)
        {
            try
            {
                _replyTo = Sender;
                await GetCnxAndRequestId();
                await GetSchema(s.TopicName, s.Version);
            }
            catch (Exception e)
            {
                _replyTo.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
            }
            Become(Awaiting);
        }

        private async ValueTask TopicsUnderNamespaceAsync()
        {
            try
            {
                var t = _getTopicsUnderNamespace;
                var opTimeout = _operationTimeout;
                _getTopicsUnderNamespaceBackOff = new BackoffBuilder().SetInitialTime(TimeSpan.FromMilliseconds(100)).SetMandatoryStop(opTimeout.Multiply(2)).SetMax(TimeSpan.FromMinutes(1)).Create();
                await GetCnxAndRequestId();
                await TopicsUnderNamespace(t.Namespace, t.Mode, t.TopicsPattern, t.TopicsHash, opTimeout);

            }
            catch (Exception e)
            {
                _replyTo.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
                Become(Awaiting);

            }
        }
        private void Awaiting()
        {
            Receive<SetClient>(c => { });
            Receive<UpdateServiceUrl>(u => UpdateServiceUrl(u.ServiceUrl));
            ReceiveAsync<GetBroker>(async broke => await GetBroke(broke.TopicName));
            ReceiveAsync<GetPartitionedTopicMetadata>(async p => await PartitionedTopicMetadata(p));
            ReceiveAsync<GetSchema>(async s => await Schema(s));
            ReceiveAsync<GetTopicsUnderNamespace>(async t =>
            {
                _replyTo = Sender;
                _getTopicsUnderNamespace = t;
                await TopicsUnderNamespaceAsync();

                Become(GetTopicsUnderNamespace);
            });
            Receive<SetFindBroker>(async set => await FindBroker(set.Topic, set.RedirectCount, set.Address, set.Authoritative, set.Sender));
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
                        await GetCnxAndRequestId(responseBrokerAddress);
                        Self.Tell(new SetFindBroker(topic, redirectCount + 1, responseBrokerAddress, data.Authoritative, replyTo));
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

            _topicName = topicName;
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
                _replyTo.Tell(askResponse);
                return;
            }

            var schemaResponse = askResponse.ConvertTo<Messages.GetSchemaResponse>();
            var err = schemaResponse.Response.ErrorCode;
            if (err != ServerError.UnknownError)
            {
                var e = $"{err}: {schemaResponse.Response.ErrorMessage}";
                _log.Error(e);
                _replyTo.Tell(new AskResponse(new PulsarClientException(new Exception(e))));
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
                _replyTo.Tell(new AskResponse(new GetSchemaInfoResponse(info)));
            }
        }

		public string ServiceUrl
		{
			get
			{
				return _serviceNameResolver.ServiceUrl;
			}
		}

        public IStash Stash { get; set; }
        
        private void GetTopicsUnderNamespace()
        {
            
            Receive<bool>(async l => 
            {
                var t = _getTopicsUnderNamespace;
                await TopicsUnderNamespace(t.Namespace, t.Mode, t.TopicsPattern, t.TopicsHash, _opTime);
            });
            Receive<AskResponse>(l =>
            {
                _replyTo.Tell(l);

                Stash.UnstashAll();
                Become(Awaiting);
            });
            ReceiveAny(s => Stash.Stash());
        }
        private async ValueTask TopicsUnderNamespace(NamespaceName ns, Mode mode, string topicsPattern, string topicsHash, TimeSpan opTimeout)
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
                _self.Tell(new AskResponse(new GetTopicsUnderNamespaceResponse(result, res.TopicsHash, res.Changed, res.Filtered)));
            }
            catch(Exception ex)
            {
                if(ex.Message == "Unable to write data to the transport connection: An established connection was aborted by the software in your host machine..")
                {
                    _self.Tell(new AskResponse(PulsarClientException.Unwrap(ex)));
                    return;
                }
                var nextDelay = Math.Min(_getTopicsUnderNamespaceBackOff.Next(), opTimeout.TotalMilliseconds);
               
                if (nextDelay <= 0)
                {
                    _opTime = opTimeout;
                    _self.Tell(new AskResponse(PulsarClientException.Unwrap(new Exception($"TimeoutException: Could not get topics of namespace {ns} within configured timeout"))));
                }
                else
                {
                    _log.Warning($"[namespace: {ns}] Could not get connection while getTopicsUnderNamespace -- Will try again in {nextDelay} ms");
                   
                    _opTime = opTimeout - TimeSpan.FromMilliseconds(nextDelay);
                    await Task.Delay(TimeSpan.FromMilliseconds(nextDelay));

                    var reqId = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);

                    _requestId = reqId.Id;

                    _log.Warning($"Retrying 'GetTopicsUnderNamespace' after {nextDelay} ms delay with requestid '{reqId.Id}'");

                    _self.Tell(false);
                }
            }
        }
		protected override void Unhandled(object message)
        {
			_log.Info($"Unhandled {message.GetType().FullName} received");
            base.Unhandled(message);
        }
        protected override void PreStart()
        {
            base.PreStart();
            Become(Awaiting);
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
}