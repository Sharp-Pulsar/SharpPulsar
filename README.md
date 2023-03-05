[![Build](https://github.com/eaba/SharpPulsar/workflows/Build/badge.svg)](https://github.com/eaba/SharpPulsar/actions?query=workflow%3ABuild)
[![Tests](https://github.com/eaba/SharpPulsar/workflows/Tests/badge.svg)](https://github.com/eaba/SharpPulsar/actions?query=workflow%3ATests)

# SharpPulsar
SharpPulsar is an [Apache Pulsar](https://github.com/apache/pulsar) Client built on top [Akka.net](https://github.com/akkadotnet/akka.net), which can handle millions of 
Apache Pulsar Producers/Consumers (in theory). 

# What Is Akka.NET?
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
- [x] Cluster-level Auto Failover

# Producer
- [x] Exclusive Producer
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
- [x] Broker Entry Metadata

# Reader
- [x] User-defined properties	
- [x] HasMessageAvailable
- [x] Schema (Primitive, Avro, Json, KeyValue, AutoSchema)
- [x] Seek (MessageID, Timestamp)
- [x] Multiple Topics		
- [x] End-to-end Encryption	
- [x] Interceptors

# TableView
- [x] Compacted Topics
- [x] Schema (All supported schema types)
- [x] Register Listener

# Extras
- [x] Pulsar SQL
- [x] Pulsar Admin REST API
- [x] Function REST API
- [x] EventSource(Reader/SQL)
- [x] OpenTelemetry (`ProducerOTelInterceptor`, `ConsumerOTelInterceptor`)



## Getting Started
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
			Thread.Sleep(TimeSpan.FromSeconds(5));
            for (var i = 0; i < 10; i++)
            {
                var message = (Message<sbyte[]>)consumer.Receive();
                consumer.Acknowledge(message);
                var res = Encoding.UTF8.GetString(message.Data.ToBytes());
                Console.WriteLine($"message '{res}' from topic: {message.TopicName}");
            }

````
## Logical Types
Avro Logical Types are supported. Message object MUST implement `ISpecificRecord`
```csharp
    AvroSchema<LogicalMessage> avroSchema = AvroSchema<LogicalMessage>.Of(ISchemaDefinition<LogicalMessage>.Builder().WithPojo(typeof(LogicalMessage)).WithJSR310ConversionEnabled(true).Build());

    public class LogicalMessage : ISpecificRecord
    {
        [LogicalType(LogicalTypeKind.Date)]
        public DateTime CreatedTime { get; set; }
		
        [LogicalType(LogicalTypeKind.TimestampMicrosecond)]
        public DateTime StampMicros { get; set; }

        [LogicalType(LogicalTypeKind.TimestampMillisecond)]
        public DateTime StampMillis { get; set; }
		
	[LogicalType(LogicalTypeKind.TimeMicrosecond)]
        public TimeSpan TimeMicros { get; set; }

        [LogicalType(LogicalTypeKind.TimeMillisecond)]
        public TimeSpan TimeMillis { get; set; }
        
        public AvroDecimal Size { get; set; }
		
        public string DayOfWeek { get; set; }

        [Ignore]
        public Avro.Schema Schema { get; set; }

        public object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0: return CreatedTime; 
	        case 1: return StampMicros;
                case 2: return StampMillis;
	        case 3: return TimeMicros;
                case 4: return TimeMillis;
                case 5: return Size;
                case 6: return DayOfWeek;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
            };
        }

        public void Put(int fieldPos, object fieldValue)
        {
            switch (fieldPos)
            {
                case 0: CreatedTime = (DateTime)fieldValue; break;
		case 1: StampMicros = (DateTime)fieldValue; break;
                case 2: StampMillis = (DateTime)fieldValue; break;
	        case 3: TimeMicros = (TimeSpan)fieldValue; break;
                case 4: TimeMillis = (TimeSpan)fieldValue; break;
                case 5: Size = (AvroDecimal)fieldValue; break;
                case 6: DayOfWeek = (String)fieldValue; break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            };
        }
    }
```
## KeyValue Schema ALERT!!!!

Because I have become lazy and a lover of "peace of mind":
- For schema type of **KEYVALUESCHEMA**:
  ```csharp 
  producer.NewMessage().Value<TK, TV>(data).Send();  
  ```
  OR
  ```csharp
  producer.Send<TK, TV>(data);
  ```
`TK, TV` represents the key and value types of the `KEYVALUESCHEMA` respectively. 

## TableView

```csharp
var topic = $"persistent://public/default/tableview-{DateTime.Now.Ticks}";
var count = 20;
var keys = await PublishMessages(topic, count, false);

var tv = await _client.NewTableViewBuilder(ISchema<string>.Bytes)
.Topic(topic)
.AutoUpdatePartitionsInterval(TimeSpan.FromSeconds(60))
.CreateAsync();
 
 Console.WriteLine($"start tv size: {tv.Size()}");
 tv.ForEachAndListen((k, v) => Console.WriteLine($"{k} -> {Encoding.UTF8.GetString(v)}"));
 await Task.Delay(5000);
 Console.WriteLine($"Current tv size: {tv.Size()}");

 tv.ForEachAndListen((k, v) => Console.WriteLine($"checkpoint {k} -> {Encoding.UTF8.GetString(v)}"));
```

## OpenTelemetry

```csharp
var exportedItems = new List<Activity>();
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
.AddSource("producer", "consumer")
.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("inmemory-test"))
.AddInMemoryExporter(exportedItems)
.Build();

 var producerBuilder = new ProducerConfigBuilder<byte[]>()
 .Intercept(new ProducerOTelInterceptor<byte[]>("producer", _client.Log))
 .Topic(topic);

 var consumerBuilder = new ConsumerConfigBuilder<byte[]>()
 .Intercept(new ConsumerOTelInterceptor<byte[]>("consumer", _client.Log))
 .Topic(topic);
```

## Cluster-level Auto Failover

```csharp
var config = new PulsarClientConfigBuilder();
var builder = AutoClusterFailover.Builder().Primary(serviceUrl)
.Secondary(new List<string> { secondary })
.FailoverDelay(TimeSpan.FromSeconds(failoverDelay))
.SwitchBackDelay(TimeSpan.FromSeconds(switchBackDelay))
.CheckInterval(TimeSpan.FromSeconds(checkInterval));
config.ServiceUrlProvider(new AutoClusterFailover((AutoClusterFailoverBuilder)builder));
```

## [Experimental]Running SharpPulsar Tests in docker container (the issue I have faced is how to create container from within a container)

You can run `SharpPulsar` tests in docker container. A `Dockerfile` and `docker-compose` file is provided at the root folder to help you run these tests in a docker container.
`docker-compose.yml`:
```yml
version: "2.4"

services:
  akka-test:
    image: sharp-pulsar-test
    build: 
      context: .
    cpu_count: 1
    mem_limit: 1g
    environment:
      run_count: 2
      # to filter tests, uncomment
      # test_filter: "--filter FullyQualifiedName=SharpPulsar.Test.MessageChunkingTest"
      test_file: Tests/SharpPulsar.Test/SharpPulsar.Test.csproj
```
`Dockerfile`:
```shell
FROM mcr.microsoft.com/dotnet/sdk:6.0 
ENV test_file="Tests/SharpPulsar.Test/SharpPulsar.Test.csproj"
ENV test_filter=""
ENV run_count=2
RUN mkdir sharppulsar
COPY . ./sharppulsar
RUN ls
WORKDIR /sharppulsar
CMD ["/bin/bash", "-c", "x=1; c=0; while [ $x -le 1 ] && [ $c -le ${run_count} ]; do dotnet test ${test_file} ${test_filter} --framework net6.0 --logger trx; c=$(( $c + 1 )); if [ $? -eq 0 ]; then x=1; else x=0; fi;  done"]
```
# How to:
`cd` into the root directory and execute `docker-compose up`
`run-count` is the number of times you want the test repeated.
`test_filter` is used when you need to test a specific test instead of running all the tests in the test suite.

## License

This project is licensed under the Apache License Version 2.0 - see the [LICENSE](LICENSE) file for details.
