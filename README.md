# SharpPulsar
SharpPulsar is [Apache Pulsar](https://github.com/apache/pulsar) Client built using [Akka.net](https://github.com/akkadotnet/akka.net). 

The goal is to match java client features so that .Net developers lack nothing!

# What Is Akka.Net?
**Akka.NET** is a professional-grade port of the popular Java/Scala framework [Akka](http://akka.io) distributed actor framework to .NET.

# What Is Apache Pulsar?
Pulsar is a distributed pub-sub messaging platform with a very flexible messaging model and an intuitive client API.

## Note
JsonSchema is basically AvroSchema underneath!

Supported pulsar cluster versions: 2.5+

## Supported features
- [X] Service discovery
- [X] Automatic reconnect
- [X] Producer
- [X] Consumer
- [X] Reader
- [X] Schema Registration
- [X] End-To-End Message Encryption
- [X] Partitioned Producer
- [X] MultiTopics Consumer
- [x] Pattern Multi-Topics Consumer
- [x] Bulk Publishing - for simplicity sake!
- [x] Proxy
- [x] Seek (MessageId, Timestamp)
- [x] Pulsar SQL (Presto)[How to use cmd 20 and 21 in [Sample](https://github.com/eaba/SharpPulsar/tree/master/Sample)]


### Getting Started
Install the NuGet package [SharpPulsar](https://www.nuget.org/packages/SharpPulsar) and follow the [Sample](https://github.com/eaba/SharpPulsar/tree/master/Sample).

## Usage
1 - Ready your Schema:
````
 var jsonSchema = JsonSchema.Of(ISchemaDefinition.Builder().WithPojo(typeof(Students)).WithAlwaysAllowNull(false).Build());
 or
 var jsonSchem = JsonSchema.Of(typeof(Students));
````
2 - Make ready your Listeners for Producer, Consumer and Reader. Due to the async nature of Actors' `Tell`, listeners are a way 
    for you to hear what they have got to report back!:
    
````
var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, (s, p) => Producers.Add(s, p), s =>
            {
                Receipts.Add(s);
            });
            var consumerListener = new DefaultConsumerEventListener(Console.WriteLine, (s, c) =>
            {
                if(!Consumers.ContainsKey(s))
                    Consumers.Add(s, c);
            }, (s, response) => LastMessageId.Add(s, response));
            //var jsonSchem = JsonSchema.Of(typeof(Students));

            #region messageListener

            var messageListener = new DefaultMessageListener((a, m) =>
            {
                var students = m.ToTypeOf<Students>();
                var s = JsonSerializer.Serialize(students);
                Messages.Add(s);
                Console.WriteLine(s);
                if (m.MessageId is MessageId mi)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(mi.LedgerId, mi.EntryId, -1, mi.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {mi.PartitionIndex}");
                }
                else if (m.MessageId is BatchMessageId b)
                {
                    a.Tell(new AckMessage(new MessageIdReceived(b.LedgerId, b.EntryId, b.BatchIndex, b.PartitionIndex)));
                    Console.WriteLine($"Consumer >> {students.Name}- partition: {b.PartitionIndex}");
                }
                else
                    Console.WriteLine($"Unknown messageid: {m.MessageId.GetType().Name}");
            }, message =>
            {
                var students = message.ToTypeOf<Students>();
                Console.WriteLine(JsonSerializer.Serialize(students));
            });

````
3 - Instantiate `PulsarSystem` with Client Configuration:
````
var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl("pulsar://localhost:6650")
                .ConnectionsPerBroker(1)
                .ClientConfigurationData;

var pulsarSystem = new PulsarSystem(clientConfig);
````
3.1 - If Broker is behind proxy, set that in the Client Configuration:
`.UseProxy(true)`

4 - Create a Producer with Producer Configuration:
````
var producerConfig = new ProducerConfigBuilder()
                .ProducerName("producer")
                .Topic("test-topic")
                .Schema(jsonSchema)
                .EventListener(producerListener)
                .ProducerConfigurationData;

  var topic = pulsarSystem.CreateProducer(new CreateProducer(jsonSchema, producerConfig));

````

5 - Create a Consumer with Consumer Configuration:
````
var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName("topic")
                .ForceTopicCreation(true)
                .SubscriptionName("pattern-Subscription")
                .Topic(topic)
                .ConsumerEventListener(consumerListener)
                .Schema(jsonSchema)
                .MessageListener(messageListener)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Latest)
                .ConsumerConfigurationData;
 pulsarSystem.CreateConsumer(new CreateConsumer(jsonSchema, consumerConfig, ConsumerType.Single));
````
6 - Create a Reader with Reader Configuration:
````
  var readerConfig = new ReaderConfigBuilder()
                .ReaderName("partitioned-topic")
                .Schema(jsonSchema)
                .EventListener(consumerListener)
                .ReaderListener(messageListener)
                .Topic(topic)
                .StartMessageId(MessageIdFields.Latest)
                .ReaderConfigurationData;
  pulsarSystem.CreateReader(new CreateReader(jsonSchema, readerConfig));
````
7 - Publish your messages either with `pulsarSystem.BulkSend` or `pulsarSystem.Send`
