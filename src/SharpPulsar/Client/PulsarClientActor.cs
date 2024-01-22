using Akka.Actor;
using SharpPulsar.Auth;
using SharpPulsar.Configuration;
using SharpPulsar.Exceptions;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Requests;
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
namespace SharpPulsar.Client
{
    internal class PulsarClientActor : ReceiveActor
    {

        private readonly ClientConfigurationData _conf;
        private readonly IActorRef _lookup;
        private readonly IActorRef _cnxPool;
        private readonly ILoggingAdapter _log;

        public enum State
        {
            Open = 0,
            Closing = 1,
            Closed = 2
        }

        private readonly State _state;
        private readonly ISet<IActorRef> _producers;
        private readonly ISet<IActorRef> _consumers;
        private readonly DateTime _clientClock;

        private readonly IActorRef _tcClient;
        public PulsarClientActor(ClientConfigurationData conf, IActorRef cnxPool, IActorRef txnCoordinator, IActorRef lookup, IActorRef idGenerator)
        {
            if (conf == null || string.IsNullOrWhiteSpace(conf.ServiceUrl))
            {
                throw new PulsarClientException.InvalidConfigurationException("Invalid client configuration");
            }
            _tcClient = txnCoordinator;
            _log = Context.GetLogger();
            Auth = conf;
            _conf = conf;
            _clientClock = conf.Clock;
            conf.Authentication.Start();
            _cnxPool = cnxPool;
            _lookup = lookup;
            _producers = new HashSet<IActorRef>();
            _consumers = new HashSet<IActorRef>();
            _state = State.Open;
            Receive<AddProducer>(m => _producers.Add(m.Producer));
            Receive<UpdateServiceUrl>(m => UpdateServiceUrl(m.ServiceUrl));
            Receive<AddConsumer>(m =>
            {
                _consumers.Add(m.Consumer);
            });
            Receive<GetClientState>(_ => Sender.Tell((int)_state));
            Receive<CleanupConsumer>(m => _consumers.Remove(m.Consumer));
            Receive<CleanupProducer>(m => _producers.Remove(m.Producer));
            Receive<GetTcClient>(_ =>
            {
                Sender.Tell(new TcClient(_tcClient));
            });
        }

        private ClientConfigurationData Auth
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value.AuthPluginClassName) || string.IsNullOrWhiteSpace(value.AuthParams) && value.AuthParamMap == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(value.AuthParams))
                {
                    value.Authentication = AuthenticationFactory.Create(value.AuthPluginClassName, value.AuthParams);
                }
                else if (value.AuthParamMap != null)
                {
                    value.Authentication = AuthenticationFactory.Create(value.AuthPluginClassName, value.AuthParamMap);
                }
            }
        }

        public virtual ClientConfigurationData Configuration
        {
            get
            {
                return _conf;
            }
        }
        protected override void PostStop()
        {
            _lookup.GracefulStop(TimeSpan.FromSeconds(1));
            _cnxPool.GracefulStop(TimeSpan.FromSeconds(1));
            _conf.Authentication = null;
            base.PostStop();
        }

        public virtual void UpdateServiceUrl(string serviceUrl)
        {
            _log.Info($"Updating service URL to {serviceUrl}");

            _conf.ServiceUrl = serviceUrl;
            var asked = _lookup.Ask<int>(new UpdateServiceUrl(serviceUrl)).GetAwaiter().GetResult();
            _cnxPool.Tell(CloseAllConnections.Instance);
        }

        internal virtual int ProducersCount()
        {
            lock (_producers)
            {
                return _producers.Count;
            }
        }

        internal virtual int ConsumersCount()
        {
            lock (_consumers)
            {
                return _consumers.Count;
            }
        }


    }

}