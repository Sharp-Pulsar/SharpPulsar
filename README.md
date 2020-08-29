# SharpPulsar
SharpPulsar is [Apache Pulsar](https://github.com/apache/pulsar) Client built using [Akka.net](https://github.com/akkadotnet/akka.net). 

# What Is Akka.Net?
**Akka.NET** is a professional-grade port of the popular Java/Scala framework [Akka](http://akka.io) distributed actor framework to .NET.

# What Is Apache Pulsar?
Pulsar is a distributed pub-sub messaging platform with a very flexible messaging model and an intuitive client API.
Supported pulsar cluster versions: 2.5+

## Supported features
- [x] Basic Producer/Consumer API
- [x] Partitioned Topics Producer
- [x] Producer Broadcast Group - broadcast single message to multiple topics including partitioned topics
- [x] Bulk Message Producer
- [x] Batching
- [x] Multi-topics Consumer
- [x] Topics Regex Consumer
- [x] Reader API
- [x] Schema
- [x] Large Message Chunking
- [X] End-To-End Message Encryption
- [x] Proxy support(SNI supporteed)
- [x] Consumer seek
- [x] Compression
- [x] TLS
- [x] Authentication (token, tls, OAuth2)
- [x] Key_shared
- [x] key based batcher
- [x] Negative Acknowledge
- [x] Delayed Delivery Messages
- [x] User defined properties producer/consumer(or message tagging)
- [x] Interceptors
- [x] Routing (RoundRobin, ConsistentHashing, Broadcast, Random)
- [x] Pulsar SQL
- [x] Pulsar Admin/Function API
- [x] EventSource(Reader API/Presto SQL)
- [x] [More...](https://github.com/eaba/SharpPulsar/blob/master/Sample/Program.cs)



### Getting Started
Install the NuGet package [SharpPulsar](https://www.nuget.org/packages/SharpPulsar) and follow the [Sample](https://github.com/eaba/SharpPulsar/tree/master/Sample).

## Usage
1 - Startup SharpPulsar:
```csharp
 var clientConfig = new PulsarClientConfigBuilder()
                .ServiceUrl(endPoint)
                .ConnectionsPerBroker(1)
                //If Broker is behind proxy, set that in the Client Configuration
                .UseProxy(true)
                .OperationTimeout(opto)
                .AllowTlsInsecureConnection(false)
                .ProxyServiceUrl(url, ProxyProtocol.SNI)
                .Authentication( new AuthenticationDisabled())                             //.Authentication(AuthenticationFactory.Token("eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJzaGFycHB1bHNhci1jbGllbnQtNWU3NzY5OWM2M2Y5MCJ9.lbwoSdOdBoUn3yPz16j3V7zvkUx-Xbiq0_vlSvklj45Bo7zgpLOXgLDYvY34h4MX8yHB4ynBAZEKG1ySIv76DPjn6MIH2FTP_bpI4lSvJxF5KsuPlFHsj8HWTmk57TeUgZ1IOgQn0muGLK1LhrRzKOkdOU6VBV_Hu0Sas0z9jTZL7Xnj1pTmGAn1hueC-6NgkxaZ-7dKqF4BQrr7zNt63_rPZi0ev47vcTV3ga68NUYLH5PfS8XIqJ_OV7ylouw1qDrE9SVN8a5KRrz8V3AokjThcsJvsMQ8C1MhbEm88QICdNKF5nu7kPYR6SsOfJJ1HYY-QBX3wf6YO3VAF_fPpQ"))
                .ClientConfigurationData;

var pulsarSystem = PulsarSystem.GetInstance(clientConfig);
```
2 - Create Producer and Send Messages:
    
```csharp
           var jsonSchem = AvroSchema.Of(typeof(Students));
            var producerListener = new DefaultProducerListener((o) =>
            {
                Console.WriteLine(o.ToString());
            }, s =>
            {
                Receipts.Add(s);
            });
            var producerConfig = new ProducerConfigBuilder()
                .ProducerName($"Test-{topic}-{Guid.NewGuid()}")
                .Topic(topic)
                .Schema(jsonSchem)
                .EnableChunking(true)
                .EventListener(producerListener)
                .ProducerConfigurationData;

            var t = pulsarSystem.PulsarProducer(new CreateProducer(jsonSchem, producerConfig));            
            Console.WriteLine($"Acquired producer for topic: {t.Topic}");
            SendMessages(system, topic, t.Producer, key, value, tag);
```
3 - You can Consume messages in one of two ways(Listener or Queue). `ConsumptionType.Queue` let you pull messages forever or with limit by supplying a `takeCount` of -1 or `{number of messages}` to pull, respectively:
```csharp
           var consumerListener = new DefaultConsumerEventListener(Console.WriteLine);
            var messageListener = new DefaultMessageListener(StudentHandler, null);
            var jsonSchem = AvroSchema.Of(typeof(Students));
            var topicLast = topic.Split("/").Last();
            var consumerConfig = new ConsumerConfigBuilder()
                .ConsumerName(topicLast)
                .ForceTopicCreation(true)
                .SubscriptionName($"{topicLast}-Subscription")
                .Topic(topic)
                //.AckTimeout(10000)
                .AcknowledgmentGroupTime(0)
                .ConsumerEventListener(consumerListener)
                .SubscriptionType(CommandSubscribe.SubType.Exclusive)
                .Schema(jsonSchem)
                .SetConsumptionType(ConsumptionType.Queue)
                .MessageListener(messageListener)
                .StartMessageId(MessageIdFields.Earliest)
                .SubscriptionInitialPosition(SubscriptionInitialPosition.Earliest)
                .ConsumerConfigurationData;
            system.PulsarConsumer(new CreateConsumer(jsonSchem, consumerConfig));
```
4 - [Explore more](https://github.com/eaba/SharpPulsar/blob/master/Sample/Program.cs)
5 - It is a big sin not to report any difficulties or issues being experienced using SharpPulsar
## License

This project is licensed under the Apache License Version 2.0 - see the [LICENSE](LICENSE) file for details.
