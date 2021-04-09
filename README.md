[![Build](https://github.com/eaba/SharpPulsar/workflows/Build/badge.svg)](https://github.com/eaba/SharpPulsar/actions?query=workflow%3ABuild)
[![Tests](https://github.com/eaba/SharpPulsar/workflows/Tests/badge.svg)](https://github.com/eaba/SharpPulsar/actions?query=workflow%3ATests)

# SharpPulsar
SharpPulsar is an [Apache Pulsar](https://github.com/apache/pulsar) Client built on top [Akka.net](https://github.com/akkadotnet/akka.net), which can handle millions of 
Apache Pulsar Producers/Consumers (in theory). 

# What Is Akka.Net?
**Akka.NET** is a toolkit and runtime for building highly concurrent, distributed, and fault tolerant event-driven applications on .NET & Mono that is able to support up to 50 million msg/sec on a single machine,
with small memory footprint and ~2.5 million actors(or Apache Pulsar Producers/Consumers) per GB of heap.

# What Is Apache Pulsar?
**Apache Pulsar** is a cloud-native, distributed messaging and streaming platform that is able to support millions of topics while delivering high-throughput and low-latency performance.

## Supported features

# Client
- [x] TLS
- [x] Authentication (token, tls, OAuth2)
- [x] Multi-Hosts Service URL	
- [x] Proxy
- [x] SNI Routing	
- [x] Transactions	
- [x] Subscription(Durable, Non-durable)	

# Producer
- [x] Partitioned Topics
- [x] Batching
- [x] Compression (LZ4, ZLIB, ZSTD, SNAPPY)
- [x] Schema (Primitive, Avro, Json, KeyValue, AutoSchema)
- [x] User-defined properties	
- [x] Key-based batcher	
- [x] Delayed/Scheduled messages	
- [x] Interceptors	
- [x] Message Router (RoundRobin, ConsistentHashing, Broadcast, Random)
- [x] End-to-end Encryption
- [x] Chunking	
- [x] Transactions

# Consumer
- [x] User-defined properties	
- [x] HasMessageAvailable	
- [x] Subscription Type (Exclusive, Failover, Shared, Key_Shared)
- [x] Subscription Mode (Durable, Non-durable)
- [x] Interceptors	
- [x] Ack (Ack Individual, Ack Commulative, Batch-Index Ack)
- [x] Ack Timeout	
- [x] Negative Ack	
- [x] Dead Letter Policy	
- [x] End-to-end Encryption	
- [x] SubscriptionInitialPosition	
- [x] Partitioned Topics	
- [x] Batching	
- [x] Compression (LZ4, ZLIB, ZSTD, SNAPPY)
- [x] Schema (Primitive, Avro, Json, KeyValue, AutoSchema)
- [x] Compacted Topics	
- [x] Multiple Topics	
- [x] Regex Consumer

# Reader
- [x] User-defined properties	
- [x] HasMessageAvailable
- [x] Schema (Primitive, Avro, Json, KeyValue, AutoSchema)
- [x] Seek (MessageID, Timestamp)
- [x] Multiple Topics		
- [x] End-to-end Encryption	
- [x] Interceptors

# Extras
- [x] Pulsar SQL
- [x] Pulsar Admin REST API
- [x] Function REST API
- [x] EventSource(Reader/SQL)



### Getting Started
Install the NuGet package [SharpPulsar](https://www.nuget.org/packages/SharpPulsar) and follow the [Tutorials](https://github.com/eaba/SharpPulsar/tree/dev/Tutorials).

````csharp
//pulsar client settings builder
            var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650");

            //pulsar actor system
            var pulsarSystem = PulsarSystem.GetInstance(clientConfig);

            var pulsarClient = pulsarSystem.NewClient();

            var consumer = pulsarClient.NewConsumer(new ConsumerConfigBuilder<sbyte[]>()
                .Topic(myTopic)
                .ForceTopicCreation(true)
                .SubscriptionName("myTopic-sub"));

            var producer = pulsarClient.NewProducer(new ProducerConfigBuilder<sbyte[]>()
                .Topic(myTopic));

            for (var i = 0; i < 10; i++)
            {
                var data = Encoding.UTF8.GetBytes($"tuts-{i}").ToSBytes();
                producer.NewMessage().Value(data).Send();
            }
            for (var i = 0; i < 10; i++)
            {
                var message = (Message<sbyte[]>)consumer.Receive(TimeSpan.FromSeconds(30));
                consumer.Acknowledge(message);
                var res = Encoding.UTF8.GetString(message.Data.ToBytes());
                Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
            }

````


## License

This project is licensed under the Apache License Version 2.0 - see the [LICENSE](LICENSE) file for details.
