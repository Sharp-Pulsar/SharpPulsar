using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Akka.Actor;
using SharpPulsar.EventSource.Messages.Presto;
using SharpPulsar.Common.Naming;
using System.Threading.Tasks.Dataflow;
using SharpPulsar.EventSource.Pulsar;

namespace SharpPulsar.EventSource.Presto
{
    public class CurrentEventsByTopicActor : ReceiveActor
    {
        private readonly CurrentEventsByTopic _message;
        private readonly HttpClient _httpClient;
        private readonly User.Admin _admin;
        private readonly BufferBlock<object> _buffer;
        public CurrentEventsByTopicActor(CurrentEventsByTopic message, HttpClient httpClient, BufferBlock<object> buffer)
        {
            _admin = new User.Admin(message.AdminUrl, httpClient);
            _buffer = buffer;
            _message = message;
            _httpClient = httpClient;
            var topic = $"persistent://{message.Tenant}/{message.Namespace}/{message.Topic}";
            var partitions = _admin.GetPartitionedMetadata(message.Tenant, message.Namespace, message.Topic);
            Setup(partitions.Body, topic);
            Receive<Akka.Actor.Terminated>(t =>
            {
                var children =  Context.GetChildren();
                if (!children.Any())
                {
                    Context.System.Log.Info($"All children exited, shutting down in 5 seconds :{Self.Path}");
                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                }
            });
        }

        private void Setup(Admin.Models.PartitionedTopicMetadata p, string topic)
        {
            if (p.Partitions > 0)
            {
                for (var i = 0; i < p.Partitions; i++)
                {
                    var partitionTopic = TopicName.Get(topic).GetPartition(i);
                    var msgId = GetMessageIds(partitionTopic);
                    var child = Context.ActorOf(PrestoSourceActor.Prop(_buffer,msgId.Start, msgId.End, false, _httpClient, _message));
                    Context.Watch(child);
                }
            }
            else
            {
                var msgId = GetMessageIds(TopicName.Get(topic));
                var child = Context.ActorOf(PrestoSourceActor.Prop(_buffer, msgId.Start, msgId.End, false, _httpClient, _message));
                Context.Watch(child);
            }
        }
        private (EventMessageId Start, EventMessageId End) GetMessageIds(TopicName topic)
        {
            var stats = _admin.GetInternalStats(topic.NamespaceObject.Tenant, topic.NamespaceObject.LocalName, topic.LocalName);
            var start = MessageIdHelper.Calculate(_message.FromSequenceId, stats.Body);
            var startMessageId = new EventMessageId(start.Ledger, start.Entry, start.Index);
            var end = MessageIdHelper.Calculate(_message.ToSequenceId, stats.Body);
            var endMessageId = new EventMessageId(end.Ledger, end.Entry, end.Index);
            return (startMessageId, endMessageId);
        }
        public static Props Prop(CurrentEventsByTopic message, HttpClient httpClient, BufferBlock<object> buffer)
        {
            return Props.Create(()=> new CurrentEventsByTopicActor(message, httpClient, buffer));
        }
    }
}