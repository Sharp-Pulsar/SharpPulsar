using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avro;
using DotNetty.Codecs.Protobuf;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SharpPulsar.Api;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Utility;

namespace Sample
{
    class Program
    {
        private static AutoResetEvent ChannelInitilizedEvent = new AutoResetEvent(false);
        private static Bootstrap SocketBootstrap = new Bootstrap();
        private static MultithreadEventLoopGroup WorkGroup = new MultithreadEventLoopGroup();
        private static volatile bool Connected = false;
        private static IChannel Channel;
        static async Task Main(string[] args)
        {
            //InternalLoggerFactory.DefaultFactory = SharpPulsar.Utility.Log.Logger;//.AddProvider(new ConsoleLoggerProvider(new OptionsMonitor<ConsoleLoggerOptions>(null, null, null)));
            ILogger logger = Log.Logger.CreateLogger<Program>();
            
            logger.LogInformation("Example log message");
            /*var serviceNameResolver = new PulsarServiceNameResolver();
            serviceNameResolver.UpdateServiceUrl("pulsar://localhost:6650");
            var conf = new ClientConfigurationData();
            var pool = new ConnectionPool(conf, new MultithreadEventLoopGroup(conf.NumIoThreads), serviceNameResolver);
            var boot = pool.GetBootstrap();
            var resolver = pool.GetDefaultNameResolver();
            var addresses = pool.GetAddresses();
            foreach (var s in addresses)
            {
                var service = s;
                if (!resolver.IsResolved(s))
                    service = (IPEndPoint)await resolver.ResolveAsync(s);
                var host = Dns.GetHostEntry(service.Address).HostName;
                logger.LogInformation($"Creating connection to {host}");
                for (var i = 0; i < conf.ConnectionsPerBroker; i++)
                {
                    var channel = await boot.ConnectAsync("127.0.0.1", 6650);
                    var cnx = (ClientCnx)channel.Pipeline.Get("handler");
                    pool.GetOrAddConnection(host, cnx);
                }
            }*/
            //await pool.CreateConnections();

            var client = await IPulsarClient.Builder().ServiceUrl("pulsar://localhost:6650").Build();
            var producer = await client.NewProducer().Topic("persistent://my-tenant/my-ns/my-topic").CreateAsync();

            for (var i = 0; i < 10; i++)
            {
                var m =producer.Send("my-message".GetBytes());
                Console.WriteLine(m);
            }

            client.Dispose();
            InitBootstrap();
            await DoConnect();
            await WorkGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        }
        private static void InitBootstrap()
        {
            SocketBootstrap.Group(WorkGroup)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new ProtobufVarint32FrameDecoder());
                    //pipeline.AddLast(new ProtobufDecoder(Message.Parser));

                    pipeline.AddLast(new ProtobufVarint32LengthFieldPrepender());
                    pipeline.AddLast(new ProtobufEncoder());

                    pipeline.AddLast(new DotHandler());
                }));
        }
        private static async Task DoConnect()
        {
            Connected = false;
            var connected = false;
            do
            {
                try
                {
                    var clientChannel = await SocketBootstrap.ConnectAsync("127.0.0.1", 6650);
                    Channel = clientChannel;
                    ChannelInitilizedEvent.Set();
                    connected = true;
                }
                catch (Exception /*ce*/)
                {
                    //Console.WriteLine(ce.StackTrace);
                    Console.WriteLine("Reconnect server after 5 seconds...");
                    Thread.Sleep(5000);
                }
            } while (!connected);
        }
    }

    public class DotHandler : ChannelHandlerAdapter
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Console.WriteLine(message);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            context.Channel.DisconnectAsync();
            Console.WriteLine("=================Channel Inactive");
        }
        public override void ChannelActive(IChannelHandlerContext context)
        {
            Console.WriteLine($"================={context.Channel} Channel Is Active");
        }

    }
}
