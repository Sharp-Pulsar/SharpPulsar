﻿using Akka.Actor;
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
            UpdateServiceUrl(serviceUrl);
        }
        private void UpdateServiceUrl(string serviceUrl)
        {
            _serviceNameResolver.UpdateServiceUrl(serviceUrl);
            //Sender.Tell(0);
            Become(Awaiting);
        }
        private async ValueTask Broke(GetBroker broker)
        {
            try
            {
                _replyTo = Sender;
                await GetCnxAndRequestId();
                await GetBroker(broker);
            }
            catch (Exception e)
            {
                _replyTo.Tell(new AskResponse(PulsarClientException.Unwrap(e)));
            }
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
            ReceiveAsync<GetBroker>(async broke => await Broke(broke));
            ReceiveAsync<GetPartitionedTopicMetadata>(async p => await PartitionedTopicMetadata(p));
            ReceiveAsync<GetSchema>(async s => await Schema(s));
            ReceiveAsync<GetTopicsUnderNamespace>(async t =>
            {
                _replyTo = Sender;
                _getTopicsUnderNamespace = t;
                await TopicsUnderNamespaceAsync();

                Become(GetTopicsUnderNamespace);
            });
        }
        private async ValueTask GetBroker(GetBroker broker)
        {
            var socketAddress = _serviceNameResolver.ResolveHost().ToDnsEndPoint();
            var askResponse = await NewLookup(broker.TopicName);
            if (askResponse.Failed)
            {
                _replyTo.Tell(askResponse);
                return;
            }

            var data = askResponse.ConvertTo<LookupDataResult>();
            var br = broker;
            if (data.Error != ServerError.UnknownError)
            {
                _log.Warning($"[{br.TopicName}] failed to send lookup request: {data.Error}:{data.ErrorMessage}");
                if (_log.IsDebugEnabled)
                {
                    _log.Warning($"[{br.TopicName}] Lookup response exception> {data.Error}:{data.ErrorMessage}");
                }
                _replyTo.Tell(new AskResponse(new PulsarClientException(new Exception($"Lookup is not found: {data.Error}:{data.ErrorMessage}"))));
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
                        var serviceUrl = data.BrokerUrl;
                        uri = new Uri(serviceUrl);
                    }
                    var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);
                    if (data.Redirect)
                    {
                        await GetCnxAndRequestId(responseBrokerAddress);
                        await RedirectedGetBroker(br.TopicName, 1, responseBrokerAddress, data.Authoritative);
                    }
                    else
                    {
                        var response = data.ProxyThroughServiceUrl ?
                            new GetBrokerResponse(responseBrokerAddress, socketAddress) :
                            new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
                        _replyTo.Tell(new AskResponse(response));
                    }
                }
                catch (Exception parseUrlException)
                {
                    _log.Warning($"[{br.TopicName}] invalid url {uri}");
                    _replyTo.Tell(new AskResponse(new PulsarClientException(parseUrlException)));
                }
            }

        }
        private async ValueTask RedirectedGetBroker(TopicName topic, int redirectCount, DnsEndPoint address, bool authoritative)
        {
            var socketAddress = address ?? _serviceNameResolver.ResolveHost().ToDnsEndPoint();
            if (_maxLookupRedirects > 0 && redirectCount > _maxLookupRedirects)
            {
                var err = new Exception("LookupException: Too many redirects: " + _maxLookupRedirects);
                _log.Error(err.ToString());
                _replyTo.Tell(new AskResponse(new PulsarClientException(err)));
                return;
            }
            var askResponse = await NewLookup(topic, authoritative);
            if (askResponse.Failed)
            {
                _replyTo.Tell(askResponse);
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
                _replyTo.Tell(new AskResponse(new PulsarClientException(new Exception($"Lookup is not found: {data.Error}:{data.ErrorMessage}"))));

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
                        var serviceUrl = data.BrokerUrl;
                        uri = new Uri(serviceUrl);
                    }
                    var responseBrokerAddress = new DnsEndPoint(uri.Host, uri.Port);
                    if (data.Redirect)
                    {
                        await GetCnxAndRequestId(responseBrokerAddress);
                        await RedirectedGetBroker(topic, redirectCount + 1, responseBrokerAddress, data.Authoritative);
                    }
                    else
                    {
                        var response = data.ProxyThroughServiceUrl ?
                            new GetBrokerResponse(responseBrokerAddress, socketAddress) :
                            new GetBrokerResponse(responseBrokerAddress, responseBrokerAddress);
                        _replyTo.Tell(new AskResponse(response));
                    }
                }
                catch (Exception parseUrlException)
                {
                    _log.Warning($"[{topic}] invalid url {uri}");
                    _replyTo.Tell(new AskResponse(new PulsarClientException(parseUrlException)));
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
        private async ValueTask GetPartitionedTopicMetadata(TopicName topicName, TimeSpan opTimeout)
        {
            _topicName = topicName;
            await PartitionedTopicMetadata(topicName, opTimeout);
        }
    
        
        private void GetPartitionedTopicMetadata()
        {
            ReceiveAsync<bool>(async l =>
            {
                await PartitionedTopicMetadata(_topicName, _opTime);
            });
            Receive<AskResponse>(l =>
            {
                _replyTo.Tell(l);

                Stash.UnstashAll();
                Become(Awaiting);
            });
            ReceiveAny(s => Stash.Stash());
        }
        private async ValueTask PartitionedTopicMetadata(TopicName topicName, TimeSpan opTimeout)
        {
            var request = Commands.NewPartitionMetadataRequest(topicName.ToString(), _requestId);
            var payload = new Payload(request, _requestId, "NewPartitionMetadataRequest");
            var askResponse = await _clientCnx.Ask<AskResponse>(payload, _timeCnx);
            if (askResponse.Failed)
            {

                Become(GetPartitionedTopicMetadata);
                var e = askResponse.Exception;
                var nextDelay = Math.Min(_getPartitionedTopicMetadataBackOff.Next(), opTimeout.TotalMilliseconds);

                var isLookupThrottling = !PulsarClientException.IsRetriableError(e) || e is PulsarClientException.TooManyRequestsException || e is PulsarClientException.AuthenticationException;
                if (nextDelay <= 0 || isLookupThrottling)
                {
                    _self.Tell(new AskResponse(new PulsarClientException.InvalidConfigurationException(e)));
                    _log.Error(e.ToString());
                    _getPartitionedTopicMetadataBackOff = null;
                }
                else
                {
                    _log.Warning($"[topic: {topicName}] Could not get connection while getPartitionedTopicMetadata -- Will try again in {nextDelay} ms: {e?.Message}");
                    _opTime = opTimeout - TimeSpan.FromMilliseconds(nextDelay);
                    var id = await _generator.Ask<NewRequestIdResponse>(NewRequestId.Instance);
                    _requestId = id.Id;
                    _self.Tell(askResponse.Failed);
                }
                return;
            }
            var data = askResponse.ConvertTo<LookupDataResult>();

            if (data?.Error != ServerError.UnknownError)
            {
                _log.Warning($"[{topicName}] failed to get Partitioned metadata : {data.Error}:{data.ErrorMessage}");
                _replyTo.Tell(new AskResponse(new PartitionedTopicMetadata(0)));
            }
            else
            {
                _replyTo.Tell(new AskResponse(new PartitionedTopicMetadata(data.Partitions)));
            }
            _getPartitionedTopicMetadataBackOff = null;
            
            Stash?.UnstashAll();
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
}