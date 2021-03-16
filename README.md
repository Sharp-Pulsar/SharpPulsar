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



### Getting Started (for 2.0 coming soon)
Install the NuGet package [SharpPulsar](https://www.nuget.org/packages/SharpPulsar) and follow the [Sample](https://github.com/eaba/SharpPulsar/tree/master/Sample).



## License

This project is licensed under the Apache License Version 2.0 - see the [LICENSE](LICENSE) file for details.
