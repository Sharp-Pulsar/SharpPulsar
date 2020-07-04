using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.Function;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Akka.Sql.Live;
using TopicEntries = SharpPulsar.Akka.InternalCommands.Consumer.TopicEntries;

namespace SharpPulsar.Akka
{
    public class PulsarManagerState
    {

        public BlockingQueue<CreatedConsumer> ConsumerQueue { get; set; }
        public BlockingQueue<IEventMessage> EventQueue { get; set; }
        public BlockingQueue<CreatedProducer> ProducerQueue { get; set; }
        public BlockingQueue<SqlData> DataQueue { get; set; }
        public BlockingQueue<AdminResponse> AdminQueue { get; set; }
        public BlockingQueue<FunctionResponse> FunctionQueue { get; set; }
        public BlockingCollection<LiveSqlData> LiveDataQueue { get; set; }
        public BlockingQueue<TopicEntries> MaxQueue { get; set; }
        public BlockingQueue<GetOrCreateSchemaServerResponse> SchemaQueue { get; set; }
        public BlockingQueue<LastMessageIdReceived> MessageIdQueue { get; set; }
        public ConcurrentDictionary<string, BlockingCollection<ConsumedMessage>> MessageQueue { get; set; }
    }
}
