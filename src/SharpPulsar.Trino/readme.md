## Apache Pulsar SQL - Trino

# pulsar.properties:

```
#
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
#

# name of the connector to be displayed in the catalog
connector.name=pulsar
# the url of Pulsar broker service
# DEPRECATED
pulsar.broker-service-url=http://localhost:8080
# the url of Pulsar broker web service
pulsar.web-service-url=http://localhost:8080
# the url of Pulsar broker binary service
pulsar.broker-binary-service-url=pulsar://localhost:6650
# URI of Zookeeper cluster
pulsar.zookeeper-uri=127.0.0.1:2181
# minimum number of entries to read at a single time
pulsar.max-entry-read-batch-size=100
# default number of splits to use per query
pulsar.target-num-splits=2
# max message queue size
pulsar.max-split-message-queue-size=10000
# max entry queue size
pulsar.max-split-entry-queue-size=1000
# half of this value is used as max entry queue size bytes and the left is used as max message queue size bytes,
# the queue size bytes shouldn't exceed this value, but it's not strict, the default value -1 indicate no limit.
pulsar.max-split-queue-cache-size=-1
# Rewrite namespace delimiter
# Warn: avoid using symbols allowed by Namespace (a-zA-Z_0-9 -=:%)
# to prevent erroneous rewriting
pulsar.namespace-delimiter-rewrite-enable=false
pulsar.rewrite-namespace-delimiter=/
# max size of one batch message (default value is 5MB)
# pulsar.max-message-size=5242880

####### TIERED STORAGE OFFLOADER CONFIGS #######

## Driver to use to offload old data to long term storage
#pulsar.managed-ledger-offload-driver = aws-s3

## The directory to locate offloaders
#pulsar.offloaders-directory = /pulsar/offloaders

## Maximum number of thread pool threads for ledger offloading
#pulsar.managed-ledger-offload-max-threads = 2

## Properties and configurations related to specific offloader implementation
#pulsar.offloader-properties = \
#  {"s3ManagedLedgerOffloadBucket": "offload-bucket", \
#  "s3ManagedLedgerOffloadRegion": "us-west-2", \
#  "s3ManagedLedgerOffloadServiceEndpoint": "http://s3.amazonaws.com"}


####### AUTHENTICATION CONFIGS #######

## the authentication plugin to be used to authenticate to Pulsar cluster
#pulsar.auth-plugin=

## the authentication parameter to be used to authenticate to Pulsar cluster
#pulsar.auth-params=

## Accept untrusted TLS certificate
#pulsar.tls-allow-insecure-connection =

## Whether to enable hostname verification on TLS connections
#pulsar.tls-hostname-verification-enable =

## Path for the trusted TLS certificate file
#pulsar.tls-trust-cert-file-path =

####### PULSAR AUTHORIZATION CONFIGS #######

## Whether to enable pulsar authorization
pulsar.authorization-enabled=false

####### BOOKKEEPER CONFIGS #######

# Entries read count throttling-limit per seconds, 0 is represents disable the throttle, default is 0.
pulsar.bookkeeper-throttle-value = 0

# The number of threads used by Netty to handle TCP connections,
# default is 2 * Runtime.getRuntime().availableProcessors().
# pulsar.bookkeeper-num-io-threads =

# The number of worker threads used by bookkeeper client to submit operations,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.bookkeeper-num-worker-threads =

# Whether the bookkeeper client use v2 protocol or v3 protocol.
# Default is the v2 protocol which the LAC is piggy back lac. Otherwise the client
# will use v3 protocol and use explicit lac.
pulsar.bookkeeper-use-v2-protocol=true
pulsar.bookkeeper-explicit-interval=0

####### MANAGED LEDGER CONFIGS #######

# Amount of memory to use for caching data payload in managed ledger. This memory
# is allocated from JVM direct memory and it's shared across all the managed ledgers
# running in same sql worker. 0 is represents disable the cache, default is 0.
pulsar.managed-ledger-cache-size-MB = 0

# Number of threads to be used for managed ledger tasks dispatching,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-worker-threads =

# Number of threads to be used for managed ledger scheduled tasks,
# default is Runtime.getRuntime().availableProcessors().
# pulsar.managed-ledger-num-scheduler-threads =

####### PROMETHEUS CONFIGS #######

# pulsar.stats-provider=org.apache.bookkeeper.stats.prometheus.PrometheusMetricsProvider
# pulsar.stats-provider-configs={"httpServerEnabled":"false", "prometheusStatsHttpPort":"9092", "prometheusStatsHttpEnable":"true"}

```

# docker-compose.yml:

```
sql1:
    hostname: sql1
    container_name: sql1
    image: apachepulsar/pulsar-all:latest
    restart: on-failure
    command: >
      bash -c "bin/apply-config-from-env-with-prefix.py SQL_PREFIX_ trino/conf/catalog/pulsar.properties && \
               bin/apply-config-from-env.py conf/pulsar_env.sh && \
               bin/watch-znode.py -z $$zookeeperServers -p /initialized-$$clusterName -w && \
               exec bin/pulsar sql-worker run"
    environment:
      clusterName: test
      zookeeperServers: zk1:2181,zk2:2181,zk3:2181
      configurationStoreServers: zk1:2181,zk2:2181,zk3:2181
      pulsar.zookeeper-uri: zk1:2181,zk2:2181,zk3:2181
      coordinator: "true"
    volumes:
      - ./../../docker/pulsar/scripts/apply-config-from-env-with-prefix.py:/pulsar/bin/apply-config-from-env-with-prefix.py
      - ./../../docker/pulsar/scripts/apply-config-from-env.py:/pulsar/bin/apply-config-from-env.py
    depends_on:
      - zk1
      - zk2
      - zk3
      - pulsar-init
      - bk1
      - bk2
      - bk3
      - broker1
      - proxy1
    ports:
      - "8081:8081"
    networks:
      pulsar:
```
# docker

- docker run -it -p 6650:6650  -p 8080:8080 --mount source=pulsardata,target=/pulsar/data --mount source=pulsarconf,target=/pulsar/conf apachepulsar/pulsar:2.10.1 bin/pulsar standalone
- docker exec -it name bin/pulsar sql-worker run

## Pulsar SQL REST APIs

# Request for Trino services

All requests for Trino services should use Trino REST API v1 version. 

To request services, use the explicit URL `http://trino.service:8081/v1``. You need to update `trino.service:8081` with your real Trino address before sending requests.

`POST` requests require the `X-Trino-User` header. If you use authentication, you must use the same `username` that is specified in the authentication configuration. If you do not use authentication, you can specify anything for `username`.

```http
X-Trino-User: username
```

For more information about headers, refer to [client request headers](https://trino.io/docs/363/develop/client-protocol.html#client-request-headers).

# Schema

You can use statement in the HTTP body. All data is received as JSON document that might contain a `nextUri` link. If the received JSON document contains a `nextUri` link, the request continues with the `nextUri` link until the received data does not contain a `nextUri` link. If no error is returned, the query completes successfully. If an `error` field is displayed in `stats`, it means the query fails.

The following is an example of `show catalogs`. The query continues until the received JSON document does not contain a `nextUri` link. Since no `error` is displayed in `stats`, it means that the query completes successfully.

```bash
curl --header "X-Trino-User: test-user" --request POST --data 'show catalogs' http://localhost:8081/v1/statement
```

Output:

```json
{
   "infoUri" : "http://localhost:8081/ui/query.html?20191113_033653_00006_dg6hb",
   "stats" : {
      "queued" : true,
      "nodes" : 0,
      "userTimeMillis" : 0,
      "cpuTimeMillis" : 0,
      "wallTimeMillis" : 0,
      "processedBytes" : 0,
      "processedRows" : 0,
      "runningSplits" : 0,
      "queuedTimeMillis" : 0,
      "queuedSplits" : 0,
      "completedSplits" : 0,
      "totalSplits" : 0,
      "scheduled" : false,
      "peakMemoryBytes" : 0,
      "state" : "QUEUED",
      "elapsedTimeMillis" : 0
   },
   "id" : "20191113_033653_00006_dg6hb",
   "nextUri" : "http://localhost:8081/v1/statement/20191113_033653_00006_dg6hb/1"
}
```

```bash
curl http://localhost:8081/v1/statement/20191113_033653_00006_dg6hb/1
```

Output:

```json
{
   "infoUri" : "http://localhost:8081/ui/query.html?20191113_033653_00006_dg6hb",
   "nextUri" : "http://localhost:8081/v1/statement/20191113_033653_00006_dg6hb/2",
   "id" : "20191113_033653_00006_dg6hb",
   "stats" : {
      "state" : "PLANNING",
      "totalSplits" : 0,
      "queued" : false,
      "userTimeMillis" : 0,
      "completedSplits" : 0,
      "scheduled" : false,
      "wallTimeMillis" : 0,
      "runningSplits" : 0,
      "queuedSplits" : 0,
      "cpuTimeMillis" : 0,
      "processedRows" : 0,
      "processedBytes" : 0,
      "nodes" : 0,
      "queuedTimeMillis" : 1,
      "elapsedTimeMillis" : 2,
      "peakMemoryBytes" : 0
   }
}
```

```bash
curl http://localhost:8081/v1/statement/20191113_033653_00006_dg6hb/2
```

Output:

```json
{
   "id" : "20191113_033653_00006_dg6hb",
   "data" : [
      [
         "pulsar"
      ],
      [
         "system"
      ]
   ],
   "infoUri" : "http://localhost:8081/ui/query.html?20191113_033653_00006_dg6hb",
   "columns" : [
      {
         "typeSignature" : {
            "rawType" : "varchar",
            "arguments" : [
               {
                  "kind" : "LONG_LITERAL",
                  "value" : 6
               }
            ],
            "literalArguments" : [],
            "typeArguments" : []
         },
         "name" : "Catalog",
         "type" : "varchar(6)"
      }
   ],
   "stats" : {
      "wallTimeMillis" : 104,
      "scheduled" : true,
      "userTimeMillis" : 14,
      "progressPercentage" : 100,
      "totalSplits" : 19,
      "nodes" : 1,
      "cpuTimeMillis" : 16,
      "queued" : false,
      "queuedTimeMillis" : 1,
      "state" : "FINISHED",
      "peakMemoryBytes" : 0,
      "elapsedTimeMillis" : 111,
      "processedBytes" : 0,
      "processedRows" : 0,
      "queuedSplits" : 0,
      "rootStage" : {
         "cpuTimeMillis" : 1,
         "runningSplits" : 0,
         "state" : "FINISHED",
         "completedSplits" : 1,
         "subStages" : [
            {
               "cpuTimeMillis" : 14,
               "runningSplits" : 0,
               "state" : "FINISHED",
               "completedSplits" : 17,
               "subStages" : [
                  {
                     "wallTimeMillis" : 7,
                     "subStages" : [],
                     "stageId" : "2",
                     "done" : true,
                     "nodes" : 1,
                     "totalSplits" : 1,
                     "processedBytes" : 22,
                     "processedRows" : 2,
                     "queuedSplits" : 0,
                     "userTimeMillis" : 1,
                     "cpuTimeMillis" : 1,
                     "runningSplits" : 0,
                     "state" : "FINISHED",
                     "completedSplits" : 1
                  }
               ],
               "wallTimeMillis" : 92,
               "nodes" : 1,
               "done" : true,
               "stageId" : "1",
               "userTimeMillis" : 12,
               "processedRows" : 2,
               "processedBytes" : 51,
               "queuedSplits" : 0,
               "totalSplits" : 17
            }
         ],
         "wallTimeMillis" : 5,
         "done" : true,
         "nodes" : 1,
         "stageId" : "0",
         "userTimeMillis" : 1,
         "processedRows" : 2,
         "processedBytes" : 22,
         "totalSplits" : 1,
         "queuedSplits" : 0
      },
      "runningSplits" : 0,
      "completedSplits" : 19
   }
}
```

:::note

Since the response data is not in sync with the query state from the perspective of clients, you cannot rely on the response data to determine whether the query completes.

:::

For more information about Trino REST API, refer to [Trino client REST API](https://trino.io/docs/363/develop/client-protocol.html).
