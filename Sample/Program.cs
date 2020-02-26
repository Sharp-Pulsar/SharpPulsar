using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;

namespace Sample
{
    class Program
    {
        private static AutoResetEvent ChannelInitilizedEvent = new AutoResetEvent(false);

        static async Task Main(string[] args)
        {
            //InternalLoggerFactory.DefaultFactory = SharpPulsar.Utility.Log.Logger;//.AddProvider(new ConsoleLoggerProvider(new OptionsMonitor<ConsoleLoggerOptions>(null, null, null)));

            var system = ActorSystem.Create("Yyy");
            system.ActorOf(Act.Prop());
            while (true)
            {
                
            }
            //system.Tcp().Tell(new Tcp.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650)));
        }
    }

    public class Act:UntypedActor
    {
        public Act()
        {
            Context.System.Tcp().Tell(new Tcp.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6650)));
        }
        protected override void OnReceive(object message)
        {
            if (message is Tcp.Connected)
            {
                var connected = message as Tcp.Connected;
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);

                // Register self as connection handler
                Sender.Tell(new Tcp.Register(Self));
                ReadConsoleAsync();
                Become(Connected(Sender));
            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else Unhandled(message);
        }

        public static Props Prop()
        {
            return Props.Create(() => new Act());
        }
        private void ReadConsoleAsync()
        {
            Task.Factory.StartNew(self => Console.In.ReadLineAsync().PipeTo((ICanTell)self), Self);
        }
        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is Tcp.Received received)  // data received from network
                {
                    Console.WriteLine(Encoding.ASCII.GetString(received.Data.ToArray()));
                }
                else if (message is string)   // data received from console
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromString((string)message + "\n")));
                    ReadConsoleAsync();
                }
                else if (message is Tcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }
                else Unhandled(message);
            };
        }
    }
    
}
