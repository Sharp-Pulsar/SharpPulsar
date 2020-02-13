using System.Collections.Generic;

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
namespace Org.Apache.Pulsar.Client.Admin
{

	using Authentication = Org.Apache.Pulsar.Client.Api.Authentication;
	using PulsarClientException = Org.Apache.Pulsar.Client.Api.PulsarClientException;
	using UnsupportedAuthenticationException = Org.Apache.Pulsar.Client.Api.PulsarClientException.UnsupportedAuthenticationException;

	/// <summary>
	/// Builder class for a <seealso cref="PulsarAdmin"/> instance.
	/// 
	/// </summary>
	public interface PulsarAdminBuilder
	{

		/// <returns> the new <seealso cref="PulsarAdmin"/> instance </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PulsarAdmin build() throws org.apache.pulsar.client.api.PulsarClientException;
		PulsarAdmin Build();

		/// <summary>
		/// Create a copy of the current client builder.
		/// <para>
		/// Cloning the builder can be used to share an incomplete configuration and specialize it multiple times. For
		/// example:
		/// 
		/// <pre>
		/// PulsarAdminBuilder builder = PulsarAdmin.builder().allowTlsInsecureConnection(false);
		/// 
		/// PulsarAdmin client1 = builder.clone().serviceHttpUrl(URL_1).build();
		/// PulsarAdmin client2 = builder.clone().serviceHttpUrl(URL_2).build();
		/// </pre>
		/// </para>
		/// </summary>
		PulsarAdminBuilder Clone();

		/// <summary>
		/// Set the Pulsar service HTTP URL for the admin endpoint (eg. "http://my-broker.example.com:8080", or
		/// "https://my-broker.example.com:8443" for TLS)
		/// </summary>
		PulsarAdminBuilder ServiceHttpUrl(string ServiceHttpUrl);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar client instance.
		/// <para>
		/// Example:
		/// </para>
		/// <para>
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
		/// 
		/// </para>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParamsString">
		///            string which represents parameters for the Authentication-Plugin, e.g., "key1:val1,key2:val2" </param>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PulsarAdminBuilder authentication(String authPluginClassName, String authParamsString) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
		PulsarAdminBuilder Authentication(string AuthPluginClassName, string AuthParamsString);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar client instance.
		/// <para>
		/// Example:
		/// </para>
		/// <para>
		/// 
		/// <pre>
		/// <code>
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
		/// </code>
		/// 
		/// </para>
		/// </summary>
		/// <param name="authPluginClassName">
		///            name of the Authentication-Plugin you want to use </param>
		/// <param name="authParams">
		///            map which represents parameters for the Authentication-Plugin </param>
		/// <exception cref="UnsupportedAuthenticationException">
		///             failed to instantiate specified Authentication-Plugin </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PulsarAdminBuilder authentication(String authPluginClassName, java.util.Map<String, String> authParams) throws org.apache.pulsar.client.api.PulsarClientException.UnsupportedAuthenticationException;
		PulsarAdminBuilder Authentication(string AuthPluginClassName, IDictionary<string, string> AuthParams);

		/// <summary>
		/// Set the authentication provider to use in the Pulsar admin instance.
		/// <para>
		/// Example:
		/// </para>
		/// <para>
		/// 
		/// <pre>
		/// <code>
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
		/// </code>
		/// </pre>
		/// 
		/// </para>
		/// </summary>
		/// <param name="authentication">
		///            an instance of the <seealso cref="Authentication"/> provider already constructed </param>
		PulsarAdminBuilder Authentication(Authentication Authentication);

		/// <summary>
		/// Set the path to the trusted TLS certificate file
		/// </summary>
		/// <param name="tlsTrustCertsFilePath"> </param>
		PulsarAdminBuilder TlsTrustCertsFilePath(string TlsTrustCertsFilePath);

		/// <summary>
		/// Configure whether the Pulsar admin client accept untrusted TLS certificate from broker <i>(default: false)</i>
		/// </summary>
		/// <param name="allowTlsInsecureConnection"> </param>
		PulsarAdminBuilder AllowTlsInsecureConnection(bool AllowTlsInsecureConnection);

		/// <summary>
		/// It allows to validate hostname verification when client connects to broker over TLS. It validates incoming x509
		/// certificate and matches provided hostname(CN/SAN) with expected broker's host name. It follows RFC 2818, 3.1.
		/// Server Identity hostname verification.
		/// </summary>
		/// <seealso cref= <a href="https://tools.ietf.org/html/rfc2818">rfc2818</a>
		/// </seealso>
		/// <param name="enableTlsHostnameVerification"> </param>
		PulsarAdminBuilder EnableTlsHostnameVerification(bool EnableTlsHostnameVerification);

		/// <summary>
		/// This sets the connection time out for the pulsar admin client
		/// </summary>
		/// <param name="connectionTimeout"> </param>
		/// <param name="connectionTimeoutUnit"> </param>
		PulsarAdminBuilder ConnectionTimeout(int ConnectionTimeout, TimeUnit ConnectionTimeoutUnit);

		/// <summary>
		/// This sets the server response read time out for the pulsar admin client for any request.
		/// </summary>
		/// <param name="readTimeout"> </param>
		/// <param name="readTimeoutUnit"> </param>
		PulsarAdminBuilder ReadTimeout(int ReadTimeout, TimeUnit ReadTimeoutUnit);

		/// <summary>
		/// This sets the server request time out for the pulsar admin client for any request.
		/// </summary>
		/// <param name="requestTimeout"> </param>
		/// <param name="requestTimeoutUnit"> </param>
		PulsarAdminBuilder RequestTimeout(int RequestTimeout, TimeUnit RequestTimeoutUnit);

	}

}