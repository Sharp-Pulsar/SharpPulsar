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

### Getting Started
Install the NuGet package [SharpPulsar](https://www.nuget.org/packages/SharpPulsar/0.3.0) and follow the [Sample](https://github.com/eaba/SharpPulsar/tree/master/Sample).

Features:
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

