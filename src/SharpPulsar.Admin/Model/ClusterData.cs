using System.Collections.Generic;
using System.Text.Json.Serialization;
/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Admin.Model
{
	
	public class ClusterData
	{

        [JsonPropertyName("serviceUrl")]
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the HTTPS rest service URL (for admin operations)
        /// </summary>
        [JsonPropertyName("serviceUrlTls")]
        public string ServiceUrlTls { get; set; }

        /// <summary>
        /// Gets or sets the broker service url (for produce and consume
        /// operations)
        /// </summary>
        [JsonPropertyName("brokerServiceUrl")]
        public string BrokerServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the secured broker service url (for produce and
        /// consume operations)
        /// </summary>
        [JsonPropertyName("brokerServiceUrlTls")]
        public string BrokerServiceUrlTls { get; set; }

        /// <summary>
        /// Gets or sets proxy-service url when client would like to connect to
        /// broker via proxy.
        /// </summary>
        [JsonPropertyName("proxyServiceUrl")]
        public string ProxyServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets authentication plugin when client would like to
        /// connect to cluster.
        /// </summary>
        [JsonPropertyName("authenticationPlugin")]
        public string AuthenticationPlugin { get; set; }

        /// <summary>
        /// Gets or sets authentication parameters when client would like to
        /// connect to cluster.
        /// </summary>
        [JsonPropertyName("authenticationParameters")]
        public string AuthenticationParameters { get; set; }

        /// <summary>
        /// Gets or sets protocol to decide type of proxy routing eg:
        /// SNI-routing. Possible values include: 'SNI'
        /// </summary>
        [JsonPropertyName("proxyProtocol")]
        public ProxyProtocol ProxyProtocol { get; set; }

        /// <summary>
        /// Gets or sets a set of peer cluster names
        /// </summary>
        [JsonPropertyName("peerClusterNames")]
        public IList<string> PeerClusterNames { get; set; }

        /// <summary>
        /// Gets or sets enable TLS when talking with other brokers in the same
        /// cluster (admin operation) or different clusters (replication)
        /// </summary>
        [JsonPropertyName("brokerClientTlsEnabled")]
        public bool? BrokerClientTlsEnabled { get; set; }

        /// <summary>
        /// Gets or sets allow TLS connections to servers whose certificate
        /// cannot be be verified to have been signed by a trusted certificate
        /// authority.
        /// </summary>
        [JsonPropertyName("tlsAllowInsecureConnection")]
        public bool? TlsAllowInsecureConnection { get; set; }

        /// <summary>
        /// Gets or sets whether internal client use KeyStore type to
        /// authenticate with other Pulsar brokers
        /// </summary>
        [JsonPropertyName("brokerClientTlsEnabledWithKeyStore")]
        public bool? BrokerClientTlsEnabledWithKeyStore { get; set; }

        /// <summary>
        /// Gets or sets TLS TrustStore type configuration for internal client:
        /// JKS, PKCS12 used by the internal client to authenticate with Pulsar
        /// brokers
        /// </summary>
        [JsonPropertyName("brokerClientTlsTrustStoreType")]
        public string BrokerClientTlsTrustStoreType { get; set; }

        /// <summary>
        /// Gets or sets TLS TrustStore path for internal client used by the
        /// internal client to authenticate with Pulsar brokers
        /// </summary>
        [JsonPropertyName("brokerClientTlsTrustStore")]
        public string BrokerClientTlsTrustStore { get; set; }

        /// <summary>
        /// Gets or sets TLS TrustStore password for internal client used by
        /// the internal client to authenticate with Pulsar brokers
        /// </summary>
        [JsonPropertyName("brokerClientTlsTrustStorePassword")]
        public string BrokerClientTlsTrustStorePassword { get; set; }

        /// <summary>
        /// Gets or sets path for the trusted TLS certificate file for outgoing
        /// connection to a server (broker)
        /// </summary>
        [JsonPropertyName("brokerClientTrustCertsFilePath")]
        public string BrokerClientTrustCertsFilePath { get; set; }

        /// <summary>
        /// Gets or sets listenerName when client would like to connect to
        /// cluster
        /// </summary>
        [JsonPropertyName("listenerName")]
        public string ListenerName { get; set; }

    }



}