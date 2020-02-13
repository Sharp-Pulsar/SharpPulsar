using System;
using System.Collections.Generic;
using System.IO;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace SharpPulsar.Sql
{
	using PemReader = com.facebook.airlift.security.pem.PemReader;
	using CharMatcher = com.google.common.@base.CharMatcher;
	using HostAndPort = com.google.common.net.HostAndPort;
	using Call = okhttp3.Call;
	using Callback = okhttp3.Callback;
	using Credentials = okhttp3.Credentials;
	using Interceptor = okhttp3.Interceptor;
	using JavaNetCookieJar = okhttp3.JavaNetCookieJar;
	using OkHttpClient = okhttp3.OkHttpClient;
	using Response = okhttp3.Response;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.AUTHORIZATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.USER_AGENT;

	public sealed class OkHttpUtil
	{
		private OkHttpUtil()
		{
		}

		public class NullCallback : Callback
		{
			public override void OnFailure(Call Call, IOException E)
			{
			}
			public override void OnResponse(Call Call, Response Response)
			{
			}
		}

		public static Interceptor UserAgent(string UserAgent)
		{
			return chain => chain.proceed(chain.request().newBuilder().header(USER_AGENT, UserAgent).build());
		}

		public static Interceptor BasicAuth(string User, string Password)
		{
			requireNonNull(User, "user is null");
			requireNonNull(Password, "password is null");
			if (User.Contains(":"))
			{
				throw new ClientException("Illegal character ':' found in username");
			}

			string Credential = Credentials.basic(User, Password);
			return chain => chain.proceed(chain.request().newBuilder().header(AUTHORIZATION, Credential).build());
		}

		public static Interceptor TokenAuth(string AccessToken)
		{
			requireNonNull(AccessToken, "accessToken is null");
			checkArgument(CharMatcher.inRange((char) 33, (char) 126).matchesAllOf(AccessToken));

			return chain => chain.proceed(chain.request().newBuilder().addHeader(AUTHORIZATION, "Bearer " + AccessToken).build());
		}

		public static void SetupTimeouts(OkHttpClient.Builder ClientBuilder, int Timeout, TimeUnit Unit)
		{
			ClientBuilder.connectTimeout(Timeout, Unit).readTimeout(Timeout, Unit).writeTimeout(Timeout, Unit);
		}

		public static void SetupCookieJar(OkHttpClient.Builder ClientBuilder)
		{
			ClientBuilder.cookieJar(new JavaNetCookieJar(new CookieManager()));
		}

		public static void SetupSocksProxy(OkHttpClient.Builder ClientBuilder, Optional<HostAndPort> SocksProxy)
		{
			SetupProxy(ClientBuilder, SocksProxy, SOCKS);
		}

		public static void SetupHttpProxy(OkHttpClient.Builder ClientBuilder, Optional<HostAndPort> HttpProxy)
		{
			SetupProxy(ClientBuilder, HttpProxy, HTTP);
		}

		public static void SetupProxy(OkHttpClient.Builder ClientBuilder, Optional<HostAndPort> Proxy, Proxy.Type Type)
		{
			Proxy.map(OkHttpUtil.toUnresolvedAddress).map(address => new Proxy(Type, address)).ifPresent(ClientBuilder.proxy);
		}

		private static InetSocketAddress ToUnresolvedAddress(HostAndPort Address)
		{
			return InetSocketAddress.createUnresolved(Address.Host, Address.Port);
		}

		public static void SetupSsl(OkHttpClient.Builder ClientBuilder, Optional<string> KeyStorePath, Optional<string> KeyStorePassword, Optional<string> TrustStorePath, Optional<string> TrustStorePassword)
		{
			if (!KeyStorePath.Present && !TrustStorePath.Present)
			{
				return;
			}

			try
			{
				// load KeyStore if configured and get KeyManagers
				KeyStore KeyStore = null;
				KeyManager[] KeyManagers = null;
				if (KeyStorePath.Present)
				{
					char[] KeyManagerPassword;
					try
					{
						// attempt to read the key store as a PEM file
						KeyStore = PemReader.loadKeyStore(new File(KeyStorePath.get()), new File(KeyStorePath.get()), KeyStorePassword);
						// for PEM encoded keys, the password is used to decrypt the specific key (and does not protect the keystore itself)
						KeyManagerPassword = new char[0];
					}
					catch (Exception ignored) when (ignored is IOException || ignored is GeneralSecurityException)
					{
						KeyManagerPassword = KeyStorePassword.map(string.ToCharArray).orElse(null);

						KeyStore = KeyStore.getInstance(KeyStore.DefaultType);
						using (Stream In = new FileStream(KeyStorePath.get(), FileMode.Open, FileAccess.Read))
						{
							KeyStore.load(In, KeyManagerPassword);
						}
					}
					ValidateCertificates(KeyStore);
					KeyManagerFactory KeyManagerFactory = KeyManagerFactory.getInstance(KeyManagerFactory.DefaultAlgorithm);
					KeyManagerFactory.init(KeyStore, KeyManagerPassword);
					KeyManagers = KeyManagerFactory.KeyManagers;
				}

				// load TrustStore if configured, otherwise use KeyStore
				KeyStore TrustStore = KeyStore;
				if (TrustStorePath.Present)
				{
					TrustStore = LoadTrustStore(new File(TrustStorePath.get()), TrustStorePassword);
				}

				// create TrustManagerFactory
				TrustManagerFactory TrustManagerFactory = TrustManagerFactory.getInstance(TrustManagerFactory.DefaultAlgorithm);
				TrustManagerFactory.init(TrustStore);

				// get X509TrustManager
				TrustManager[] TrustManagers = TrustManagerFactory.TrustManagers;
				if ((TrustManagers.Length != 1) || !(TrustManagers[0] is X509TrustManager))
				{
					throw new Exception("Unexpected default trust managers:" + Arrays.toString(TrustManagers));
				}
				X509TrustManager TrustManager = (X509TrustManager) TrustManagers[0];

				// create SSLContext
				SSLContext SslContext = SSLContext.getInstance("TLS");
				SslContext.init(KeyManagers, new TrustManager[] {TrustManager}, null);

				ClientBuilder.sslSocketFactory(SslContext.SocketFactory, TrustManager);
			}
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new ClientException("Error setting up SSL: " + e.Message, e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void validateCertificates(java.security.KeyStore keyStore) throws java.security.GeneralSecurityException
		private static void ValidateCertificates(KeyStore KeyStore)
		{
			foreach (string Alias in list(KeyStore.aliases()))
			{
				if (!KeyStore.isKeyEntry(Alias))
				{
					continue;
				}
				Certificate Certificate = KeyStore.getCertificate(Alias);
				if (!(Certificate is X509Certificate))
				{
					continue;
				}

				try
				{
					((X509Certificate) Certificate).checkValidity();
				}
				catch (CertificateExpiredException E)
				{
					throw new CertificateExpiredException("KeyStore certificate is expired: " + E.Message);
				}
				catch (CertificateNotYetValidException E)
				{
					throw new CertificateNotYetValidException("KeyStore certificate is not yet valid: " + E.Message);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.security.KeyStore loadTrustStore(java.io.File trustStorePath, java.util.Optional<String> trustStorePassword) throws java.io.IOException, java.security.GeneralSecurityException
		private static KeyStore LoadTrustStore(File TrustStorePath, Optional<string> TrustStorePassword)
		{
			KeyStore TrustStore = KeyStore.getInstance(KeyStore.DefaultType);
			try
			{
				// attempt to read the trust store as a PEM file
				IList<X509Certificate> CertificateChain = PemReader.readCertificateChain(TrustStorePath);
				if (CertificateChain.Count > 0)
				{
					TrustStore.load(null, null);
					foreach (X509Certificate Certificate in CertificateChain)
					{
						X500Principal Principal = Certificate.SubjectX500Principal;
						TrustStore.setCertificateEntry(Principal.Name, Certificate);
					}
					return TrustStore;
				}
			}
			catch (Exception ignored) when (ignored is IOException || ignored is GeneralSecurityException)
			{
			}

			using (Stream In = new FileStream(TrustStorePath, FileMode.Open, FileAccess.Read))
			{
				TrustStore.load(In, TrustStorePassword.map(string.ToCharArray).orElse(null));
			}
			return TrustStore;
		}

		public static void SetupKerberos(OkHttpClient.Builder ClientBuilder, string RemoteServiceName, bool UseCanonicalHostname, Optional<string> Principal, Optional<File> KerberosConfig, Optional<File> Keytab, Optional<File> CredentialCache)
		{
			SpnegoHandler Handler = new SpnegoHandler(RemoteServiceName, UseCanonicalHostname, Principal, KerberosConfig, Keytab, CredentialCache);
			ClientBuilder.addInterceptor(Handler);
			ClientBuilder.authenticator(Handler);
		}
	}

}