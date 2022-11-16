using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Admin.v2;

namespace SharpPulsar.EventSource.Trino.Tagged
{
    public class CurrentEventsByTagActor : ReceiveActor
    {
        private readonly CurrentEventsByTag _message;
        private readonly PulsarAdminRESTAPIClient _admin;
        readonly BufferBlock<IEventEnvelope> _buffer;
        public CurrentEventsByTagActor(CurrentEventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri(message.AdminUrl)
            };
            _admin = new PulsarAdminRESTAPIClient(http);
            _buffer = buffer;
            _message = message;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata2Async(message.Tenant, message.Namespace, message.Topic, false, false).GetAwaiter().GetResult();
            Setup(partitions);
            Receive<Terminated>(t =>
            {
                var children = Context.GetChildren();
                if (!children.Any())
                {
                    Context.System.Log.Info($"All children exited, shutting down in 5 seconds :{Self.Path}");
                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
            });
        }

        private void Setup(PartitionedTopicMetadata p)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var child = Context.ActorOf(TrinoTaggedSourceActor.Prop(_buffer, false, _message, _message.Tag));
                    Context.Watch(child);
                }
            }
            else
            {
                var child = Context.ActorOf(TrinoTaggedSourceActor.Prop(_buffer, false, _message, _message.Tag));
                Context.Watch(child);
            }
        }

        public static Props Prop(CurrentEventsByTag message, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(() => new CurrentEventsByTagActor(message, buffer));
        }
    }
}