## vNext

## [2.15.1] / 2024-02-15
- [FIX][ServiceUrlProvider] ClientConfigurationData
- [Updated] Nager.PublicSuffix v3.0.0

## [2.15.0] / 2024-02-03
- [FIX][ConnectionHandler] Akka.NET Version 1.5.16: `error AK1002`
- [Added][ProducerConfigBuilder] `DefaultCryptoKeyReader`*
- [Added] Token Test 
- [Added][Consumer] `Unsubscribe(bool force)`
- [Changed][`Actors`] Add folder for internal `Client`, `Producer`, `Consumer` etc 
- [FIXED] OAuth Test 
- [Removed] Deletes all the Obsoletes in PulsarSystem
- [Updated] Apache Pulsar v3.1.2
- [Upgraded] .NET 7 to .NET 8

## [2.14.1] / 2023-09-22
- [UPDATE] NUKE v7.0.5
- [FIX][SQL] Presto to Trino [APACHE PULSAR v3.1.0]
- [UPDATES] Akka.NET v1.5.13, TestContainer v3.5.0
- [TESTS][Skip] SharpPulsar.Test `OAuth`s
- [ADD] `Tutorials` to `Launchsetting`
- [FIX][Testcontainers][PULSAR] Set retention policy `defaultRetentionTimeInMinutes=-1`, `defaultRetentionSizeInMB=-1`. 
- [REMOVED][TRINO] `HttpRequest<T> - WebRequest.Create(Url)`
- [FIXED][Obsolete] `SHA256Managed`: Changed to `SHA256.Create()`
- [REMOVED][IHash] `IHash.MakeHash(string)`

## [2.14.0] / 2023-07-12
- Update `TableView`
- Update `Transaction`
- Improve `Admin` HttpClient
- Update NUKE to `7.0.2`

## [2.13.0] / 2023-05-20
- Improved `ClientCnx`
  - `SocketClient`: changed to `SocketClientActor`
  - Added `SendMessageActor`
  - Removed `IObservable`, `NewThreadScheduler.Default.Schedule`	

## [2.12.1] / 2023-05-10
- Fixed `GetStats*`

## [2.12.0] / 2023-05-01
- Update Akka.NET to 1.5.4
- Update Apache Pulsar 2.11.1
- Improve `ClientCnx`

## [2.11.2] / 2023-03-19
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

[vNext]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.15.1...HEAD
[2.15.1]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.15.0...2.15.1
[2.15.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.14.1...2.15.0
[2.14.1]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.14.0...2.14.1
[2.14.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.13.0...2.14.0
[2.13.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.12.1...2.13.0
[2.12.1]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.12.0...2.12.1
[2.12.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.11.2...2.12.0
[2.11.2]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.11.1...2.11.2
[2.11.1]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.11.0...2.11.1
[2.11.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/2.9.0...2.11.0
[2.9.0]: https://github.com/Sharp-Pulsar/SharpPulsar/compare/0.1.0...2.9.0
[0.1.0]: https://github.com/Sharp-Pulsar/SharpPulsar/tree/0.1.0
