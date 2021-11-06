using System;
using System.Linq;
using System.Net.Http;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.Common.Naming;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.EventSource.Messages;
using SharpPulsar.Utils;

namespace SharpPulsar.EventSource.Presto
{
    public class CurrentEventsByTopicActor : ReceiveActor
    {
        private readonly CurrentEventsByTopic _message;
        private readonly Admin.Public.Admin _admin;
        private readonly BufferBlock<IEventEnvelope> _buffer;
        public CurrentEventsByTopicActor(CurrentEventsByTopic message, BufferBlock<IEventEnvelope> buffer)
        {
            _admin = new Admin.Public.Admin(message.AdminUrl, new HttpClient());
            _buffer = buffer;
            _message = message;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
            Receive<Terminated>(t =>
            {
                var children =  Context.GetChildren();
                if (!children.Any())
                {
                    Context.System.Log.Info($"All children exited, shutting down in 5 seconds :{Self.Path}");
                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
            });
        }

        private void Setup(PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var msgId = GetMessageId();
                    var child = Context.ActorOf(PrestoSourceActor.Prop(_buffer,msgId.Start, msgId.End, false, _httpClient, _message));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageId();
                var child = Context.ActorOf(PrestoSourceActor.Prop(_buffer, msgId.Start, msgId.End, false, _httpClient, _message));
                Context.Watch(child);
            }
        }

        private MessageId GetMessageId()
        {
            return (MessageId)MessageIdUtils.GetMessageId(_message.FromMessageId);
        }
        public static Props Prop(CurrentEventsByTopic message, HttpClient httpClient, BufferBlock<IEventEnvelope> buffer)
        {
            return Props.Create(()=> new CurrentEventsByTopicActor(message, httpClient, buffer));
        }
    }
}