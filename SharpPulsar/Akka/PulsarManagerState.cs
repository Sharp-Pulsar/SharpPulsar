﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Akka.InternalCommands.Producer;
using SharpPulsar.Akka.Sql;
using SharpPulsar.Akka.Sql.Live;

namespace SharpPulsar.Akka
{
    public class PulsarManagerState
    {

        public BlockingQueue<CreatedConsumer> ConsumerQueue { get; set; }
        public BlockingQueue<IEventMessage> EventQueue { get; set; }
        public BlockingQueue<CreatedProducer> ProducerQueue { get; set; }
        public BlockingQueue<SqlData> DataQueue { get; set; }
        public BlockingCollection<LiveSqlData> LiveDataQueue { get; set; }
        public BlockingQueue<NumberOfEntries> MaxQueue { get; set; }
        public BlockingQueue<GetOrCreateSchemaServerResponse> SchemaQueue { get; set; }
        public BlockingQueue<LastMessageIdReceived> MessageIdQueue { get; set; }
        public BlockingCollection<ConsumedMessage> MessageQueue { get; set; }
    }
}