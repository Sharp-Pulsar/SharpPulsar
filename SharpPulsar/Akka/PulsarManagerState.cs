using System.Collections.Concurrent;
using SharpPulsar.Akka.Admin;
using SharpPulsar.Akka.EventSource.Messages;
using SharpPulsar.Akka.Function;
using SharpPulsar.Messages;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Akka.Sql.Live;

namespace SharpPulsar.Akka
{
    public class PulsarManagerState
    {

        public BlockingQueue<CreatedConsumer> ConsumerQueue { get; set; }
        public BlockingQueue<IEventEnvelope> PrestoEventQueue { get; set; }
        public BlockingQueue<EventMessage> PulsarEventQueue { get; set; }
        public BlockingQueue<ActiveTopics> ActiveTopicsQueue { get; set; }
        public BlockingQueue<CreatedProducer> ProducerQueue { get; set; }
        public BlockingQueue<SqlData> DataQueue { get; set; }
        public BlockingQueue<SentReceipt>SentReceiptQueue { get; set; }
        public BlockingQueue<AdminResponse> AdminQueue { get; set; }
        public BlockingQueue<FunctionResponse> FunctionQueue { get; set; }
        public BlockingCollection<LiveSqlData> LiveDataQueue { get; set; }
        public BlockingQueue<GetOrCreateSchemaServerResponse> SchemaQueue { get; set; }
        public BlockingQueue<LastMessageIdReceived> MessageIdQueue { get; set; }
        public ConcurrentDictionary<string, BlockingCollection<ConsumedMessage>> MessageQueue { get; set; }
    }
}
