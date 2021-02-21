﻿using Akka.Actor;
using Akka.IO;
using SharpPulsar.Configuration;
using SharpPulsar.PulsarSocket;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SharpPulsar.Tcps
{
    public class SocketActor : UntypedActor, IWithUnboundedStash
    {
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2 _trustedCertificateAuthority;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private string _targetServerName;
        public SocketActor(ClientConfigurationData conf, DnsEndPoint server, string hostName)
        {
            _clientConfiguration = conf;
            _targetServerName = hostName;
            Context.System.TcpPulsar().Tell(new PulsarTcp.Connect(server));
        }

        public IStash Stash { get; set; }

        public static Props Prop(ClientConfigurationData conf, DnsEndPoint server, string hostName)
        {
            return Props.Create(() => new SocketActor(conf, server, hostName));
        }
        protected override void OnReceive(object message)
        {
            if (message is PulsarTcp.Connected)
            {
                var connected = message as PulsarTcp.Connected;
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);

                // Register self as connection handler
                Sender.Tell(new PulsarTcp.Register(Self));
                Context.Parent.Tell(connected);
                Stash.UnstashAll();
                Become(Connected(Sender));
            }
            else if (message is PulsarTcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else
            {
                Stash.Stash();
            }
        }

        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is PulsarTcp.Received)  // data received from network
                {
                    var received = message as PulsarTcp.Received;
                    var data = new ReadOnlySequence<byte>(received.Data.ToArray());
                    Context.Parent.Tell(new SocketPayload(data));
                }
                else if (message is SocketPayload p)   // data received from console
                {
                    connection.Tell(PulsarTcp.Write.Create(PulsarByteString.CopyFrom(p.Payload.ToArray())));
                }
                else if (message is PulsarTcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }
                else Unhandled(message);
            };
        }
    }
}
