## vNext
- Package License

## [2.11.1] / 2023-03-18
- Update Akka.NET to 1.5.1

## [2.11.0] / 2023-03-05
- Automatic failure recovery between primary and backup clusters. [PIP#13315]
- Topic map support added with new TableView type using key values in received messages. [PIP#12838]
- BREAKING CHANGES: the config builders have been moved to builder folder!
- Fixed potential `HasMessageAvailable` `deadlock`ing 
- OpenTelemetry 
- Obsoletes
- Update Akka.NET to 1.5.0
- Update Apache Pulsar 2.11.0

## [2.9.0] / 2022-02-21
- This release contains new feature and fixes found in Apache Pulsar 2.9.0 official java client/drive
- Apache Pulsar BrokerMetadata
- Apache Pulsar Exlusive Producer
- Replaced `Ask<T>` with `TaskCompletionSource<T>` to await creation of Pulsar client, producer, consumer and reader
- Fixed TimeZoneId KeyNotFound exception in SQL
- Deploy Apache Pulsar with TestContainer for esay testing

## [0.1.0] / 14 January 2022
- First release

[vNext]: https://github.com/eaba/SharpPulsar/compare/2.11.1...HEAD
[2.11.1]: https://github.com/eaba/SharpPulsar/compare/2.11.0...2.11.1
[2.11.0]: https://github.com/eaba/SharpPulsar/compare/2.9.0...2.11.0
[2.9.0]: https://github.com/eaba/SharpPulsar/compare/0.1.0...2.9.0
[0.1.0]: https://github.com/eaba/SharpPulsar/tree/0.1.0

