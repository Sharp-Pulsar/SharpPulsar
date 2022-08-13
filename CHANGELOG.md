## vNext
- Automatic failure recovery between primary and backup clusters. [PIP#13315]
- Topic map support added with new TableView type using key values in received messages. [PIP#12838]
- BREAKING CHANGES: the config builders have been moved to builder folder!
- Fixed potential `HasMessageAvailable` `deadlock`ing 
- OpenTelemetry 
- Akka.Persistence.Pulsar: BrokerEntryMetadata.BrokerTimestamp (docker run --name pulsar_local -it --env PULSAR_PREFIX_acknowledgmentAtBatchIndexLevelEnabled=true --env PULSAR_PREFIX_nettyMaxFrameSizeBytes=5253120 --env PULSAR_PREFIX_brokerDeleteInactiveTopicsEnabled=false --env PULSAR_PREFIX_transactionCoordinatorEnabled=true --env PULSAR_PREFIX_exposingBrokerEntryMetadataToClientEnabled=true --env PULSAR_PREFIX_brokerEntryMetadataInterceptors=org.apache.pulsar.common.intercept.AppendBrokerTimestampMetadataInterceptor,org.apache.pulsar.common.intercept.AppendIndexMetadataInterceptor -p 6650:6650 -p 8080:8080 -p 8081:8081 apachepulsar/pulsar-all:2.10.1 bash -c "bin/apply-config-from-env.py conf/standalone.conf && bin/pulsar standalone -nfw -nss && bin/pulsar initialize-transaction-coordinator-metadata -cs localhost:2181 -c standalone --initial-num-transaction-coordinators 2 && bin/pulsar-admin namespaces set-retention public/default --time -1 --size -1")

## [2.9.0] / 2022-02-21
- This release contains new feature and fixes found in Apache Pulsar 2.9.0 official java client/drive
- Apache Pulsar BrokerMetadata
- Apache Pulsar Exlusive Producer
- Replaced `Ask<T>` with `TaskCompletionSource<T>` to await creation of Pulsar client, producer, consumer and reader
- Fixed TimeZoneId KeyNotFound exception in SQL
- Deploy Apache Pulsar with TestContainer for esay testing

## [0.1.0] / 14 January 2022
- First release

[vNext]: https://github.com/eaba/SharpPulsar/compare/2.9.0...HEAD
[2.9.0]: https://github.com/eaba/SharpPulsar/compare/0.1.0...2.9.0
[0.1.0]: https://github.com/eaba/SharpPulsar/tree/0.1.0

