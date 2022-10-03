using System;
using System.Collections.Generic;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar.Admin.Interfaces
{
	/// <summary>
	/// Builder class for a <seealso cref="IPulsarAdmin"/> instance.
	/// 
	/// </summary>
	public interface IPulsarAdminBuilder
	{

		/// <returns> the new <seealso cref="IPulsarAdmin"/> instance </returns>
		IPulsarAdmin Build();

		/// <summary>
		/// Load the configuration from provided <tt>config</tt> map.
		/// 
		/// <para>Example:
		/// 
		/// <pre>
		/// {@code
		/// Map<String, Object> config = new HashMap<>();
		/// config.put("serviceHttpUrl", "http://localhost:6650");
		/// 
		/// PulsarAdminBuilder builder = ...;
		/// builder = builder.loadConf(config);
		/// 
		/// PulsarAdmin client = builder.build();
		/// }
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="config">
		///            configuration to load </param>
		/// <returns> the client builder instance </returns>
		IPulsarAdminBuilder LoadConf(IDictionary<string, object> config);

		/// <summary>
		/// Create a copy of the current client builder.
		/// <p/>
		/// Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
		/// example:
		/// 
		/// <pre>
		/// PulsarAdminBuilder builder = PulsarAdmin.builder().allowTlsInsecureConnection(false);
		/// 
		/// PulsarAdmin client1 = builder.clone().serviceHttpUrl(URL_1).build();
		/// PulsarAdmin client2 = builder.clone().serviceHttpUrl(URL_2).build();
		/// </pre>
		/// </summary>
		IPulsarAdminBuilder Clone();

		/// <summary>
		/// Set the Pulsar service HTTP URL for the admin endpoint (eg. "http://my-broker.example.com:8080", or
		/// "https://my-broker.example.com:8443" for TLS)
		/// </summary>
		IPulsarAdminBuilder ServiceHttpUrl(string serviceHttpUrl);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar client instance.
		/// <p/>
		/// Example:
		/// <p/>
		/// 
		/// <pre>
		/// <code>
		/// String AUTH_CLASS = "org.apache.pulsar.client.impl.auth.AuthenticationTls";
		/// String AUTH_PARAMS = "tlsCertFile:/my/cert/file,tlsKeyFile:/my/key/file";
		/// 
		/// PulsarAdmin client = PulsarAdmin.builder()
		///          .serviceHttpUrl(SERVICE_HTTP_URL)
		///          .authentication(AUTH_CLASS, AUTH_PARAMS)
		///          .build();
		/// ....
		/// </code>
		/// </pre>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParamsString">
		///            string which represents parameters for the Authentication-Plugin, e.g., "key1:val1,key2:val2" </param>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>
		IPulsarAdminBuilder Authentication(string authPluginClassName, string authParamsString);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar client instance.
		/// <p/>
		/// Example:
		/// <p/>
		/// 
		/// <pre>{@code
		/// String AUTH_CLASS = "org.apache.pulsar.client.impl.auth.AuthenticationTls";
		/// 
		/// Map<String, String> conf = new TreeMap<>();
		/// conf.put("tlsCertFile", "/my/cert/file");
		/// conf.put("tlsKeyFile", "/my/key/file");
		/// 
		/// PulsarAdmin client = PulsarAdmin.builder()
		///          .serviceHttpUrl(SERVICE_HTTP_URL)
		///          .authentication(AUTH_CLASS, conf)
		///          .build();
		/// ....
		/// }
		/// </pre>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParams">
		///            map which represents parameters for the Authentication-Plugin </param>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>

		IPulsarAdminBuilder Authentication(string authPluginClassName, IDictionary<string, string> authParams);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar admin instance.
		/// <p/>
		/// Example:
		/// <p/>
		/// 
		/// <pre>{@code
		/// String AUTH_CLASS = "org.apache.pulsar.client.impl.auth.AuthenticationTls";
		/// 
		/// Map<String, String> conf = new TreeMap<>();
		/// conf.put("tlsCertFile", "/my/cert/file");
		/// conf.put("tlsKeyFile", "/my/key/file");
		/// 
		/// Authentication auth = AuthenticationFactor.create(AUTH_CLASS, conf);
		/// 
		/// PulsarAdmin admin = PulsarAdmin.builder()
		///          .serviceHttpUrl(SERVICE_URL)
		///          .authentication(auth)
		///          .build();
		/// ....
		/// }
		/// </pre>
		/// </summary>
		/// <param name="authentication">
		///            an instance of the <seealso cref="Authentication"/> provider already constructed </param>
		IPulsarAdminBuilder Authentication(IAuthentication authentication);

		/// <summary>
		/// Set the path to the TLS key file.
		/// </summary>
		/// <param name="tlsKeyFilePath"> </param>
		/// <returns> the admin builder instance </returns>
		IPulsarAdminBuilder TlsKeyFilePath(string tlsKeyFilePath);

		/// <summary>
		/// Set the path to the TLS certificate file.
		/// </summary>
		/// <param name="tlsCertificateFilePath"> </param>
		/// <returns> the admin builder instance </returns>
		IPulsarAdminBuilder TlsCertificateFilePath(string tlsCertificateFilePath);

		/// <summary>
		/// Set the path to the trusted TLS certificate file.
		/// </summary>
		/// <param name="tlsTrustCertsFilePath"> </param>
		IPulsarAdminBuilder TlsTrustCertsFilePath(string tlsTrustCertsFilePath);

		/// <summary>
		/// Configure whether the Pulsar admin client accept untrusted TLS certificate from broker <i>(default: false)</i>.
		/// </summary>
		/// <param name="allowTlsInsecureConnection"> </param>
		IPulsarAdminBuilder AllowTlsInsecureConnection(bool allowTlsInsecureConnection);

		/// <summary>
		/// It allows to validate hostname verification when client connects to broker over TLS. It validates incoming x509
		/// certificate and matches provided hostname(CN/SAN) with expected broker's host name. It follows RFC 2818, 3.1.
		/// Server Identity hostname verification.
		/// </summary>
		/// <seealso cref="<a href="https://tools.ietf.org/html/rfc2818">rfc2818</a>"
		/// />
		/// <param name="enableTlsHostnameVerification"> </param>
		IPulsarAdminBuilder EnableTlsHostnameVerification(bool enableTlsHostnameVerification);

		/// <summary>
		/// If Tls is enabled, whether use KeyStore type as tls configuration parameter.
		/// False means use default pem type configuration.
		/// </summary>
		/// <param name="useKeyStoreTls"> </param>
		IPulsarAdminBuilder UseKeyStoreTls(bool useKeyStoreTls);

		/// <summary>
		/// The name of the security provider used for SSL connections.
		/// Default value is the default security provider of the JVM.
		/// </summary>
		/// <param name="sslProvider"> </param>
		IPulsarAdminBuilder SslProvider(string sslProvider);

		/// <summary>
		/// The file format of the key store file.
		/// </summary>
		/// <param name="tlsKeyStoreType"> </param>
		/// <returns> the admin builder instance </returns>
		IPulsarAdminBuilder TlsKeyStoreType(string tlsKeyStoreType);

		/// <summary>
		/// The location of the key store file.
		/// </summary>
		/// <param name="tlsTrustStorePath"> </param>
		/// <returns> the admin builder instance </returns>
		IPulsarAdminBuilder TlsKeyStorePath(string tlsTrustStorePath);

		/// <summary>
		/// The store password for the key store file.
		/// </summary>
		/// <param name="tlsKeyStorePassword"> </param>
		/// <returns> the admin builder instance </returns>
		IPulsarAdminBuilder TlsKeyStorePassword(string tlsKeyStorePassword);

		/// <summary>
		/// The file format of the trust store file.
		/// </summary>
		/// <param name="tlsTrustStoreType"> </param>
		IPulsarAdminBuilder TlsTrustStoreType(string tlsTrustStoreType);

		/// <summary>
		/// The location of the trust store file.
		/// </summary>
		/// <param name="tlsTrustStorePath"> </param>
		IPulsarAdminBuilder TlsTrustStorePath(string tlsTrustStorePath);

		/// <summary>
		/// The store password for the key store file.
		/// </summary>
		/// <param name="tlsTrustStorePassword"> </param>
		/// <returns> the client builder instance </returns>
		IPulsarAdminBuilder TlsTrustStorePassword(string tlsTrustStorePassword);

		/// <summary>
		/// A list of cipher suites.
		/// This is a named combination of authentication, encryption, MAC and key exchange algorithm
		/// used to negotiate the security settings for a network connection using TLS or SSL network protocol.
		/// By default all the available cipher suites are supported.
		/// </summary>
		/// <param name="tlsCiphers"> </param>
		IPulsarAdminBuilder TlsCiphers(ISet<string> tlsCiphers);

		/// <summary>
		/// The SSL protocol used to generate the SSLContext.
		/// Default setting is TLS, which is fine for most cases.
		/// Allowed values in recent JVMs are TLS, TLSv1.3, TLSv1.2 and TLSv1.1.
		/// </summary>
		/// <param name="tlsProtocols"> </param>
		IPulsarAdminBuilder TlsProtocols(ISet<string> tlsProtocols);

		/// <summary>
		/// This sets the connection time out for the pulsar admin client.
		/// </summary>
		/// <param name="connectionTimeout"> </param>
		/// <param name="connectionTimeoutUnit"> </param>
		IPulsarAdminBuilder ConnectionTimeout(int connectionTimeout, TimeSpan connectionTimeoutUnit);

		/// <summary>
		/// This sets the server response read time out for the pulsar admin client for any request.
		/// </summary>
		/// <param name="readTimeout"> </param>
		/// <param name="readTimeoutUnit"> </param>
		IPulsarAdminBuilder ReadTimeout(int readTimeout, TimeSpan readTimeoutUnit);

		/// <summary>
		/// This sets the server request time out for the pulsar admin client for any request.
		/// </summary>
		/// <param name="requestTimeout"> </param>
		/// <param name="requestTimeoutUnit"> </param>
		IPulsarAdminBuilder RequestTimeout(int requestTimeout, TimeSpan requestTimeoutUnit);

		/// <summary>
		/// This sets auto cert refresh time if Pulsar admin uses tls authentication.
		/// </summary>
		/// <param name="autoCertRefreshTime"> </param>
		/// <param name="autoCertRefreshTimeSpan"> </param>
		IPulsarAdminBuilder AutoCertRefreshTime(int autoCertRefreshTime, TimeSpan autoCertRefreshTimeSpan);
		
	}

}