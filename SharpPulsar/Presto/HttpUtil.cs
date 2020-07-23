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
namespace SharpPulsar.Presto
{
    using HostAndPort = com.google.common.net.HostAndPort;
	using Call = okhttp3.Call;
	using Callback = okhttp3.Callback;
    using Interceptor = okhttp3.Interceptor;
	using JavaNetCookieJar = okhttp3.JavaNetCookieJar;
	using OkHttpClient = okhttp3.OkHttpClient;
	using Response = okhttp3.Response;



	public sealed class HttpUtil
	{
		private HttpUtil()
		{
		}
		
		public static Interceptor userAgent(string userAgent)
		{
			return chain => chain.proceed(chain.request().newBuilder().header(USER_AGENT, userAgent).build());
		}

		public static Interceptor BasicAuth(string user, string password)
		{
			requireNonNull(user, "user is null");
			requireNonNull(password, "password is null");
			if (user.Contains(":"))
			{
				throw new ClientException("Illegal character ':' found in username");
			}

			string credential = Credentials.basic(user, password);
			return chain => chain.proceed(chain.request().newBuilder().header(AUTHORIZATION, credential).build());
		}

		public static Interceptor TokenAuth(string accessToken)
		{
			requireNonNull(accessToken, "accessToken is null");
			checkArgument(CharMatcher.inRange((char) 33, (char) 126).matchesAllOf(accessToken));

			return chain => chain.proceed(chain.request().newBuilder().addHeader(AUTHORIZATION, "Bearer " + accessToken).build());
		}

		public static void SetupTimeouts(OkHttpClient.Builder clientBuilder, int timeout, TimeUnit unit)
		{
			clientBuilder.connectTimeout(timeout, unit).readTimeout(timeout, unit).writeTimeout(timeout, unit);
		}

		public static void SetupSocksProxy(OkHttpClient.Builder clientBuilder, Optional<HostAndPort> socksProxy)
		{
			setupProxy(clientBuilder, socksProxy, SOCKS);
		}

		public static void SetupHttpProxy(OkHttpClient.Builder clientBuilder, Optional<HostAndPort> httpProxy)
		{
			setupProxy(clientBuilder, httpProxy, HTTP);
		}

		public static void SetupProxy(OkHttpClient.Builder clientBuilder, Optional<HostAndPort> proxy, Proxy.Type type)
		{
			proxy.map(HttpUtil.toUnresolvedAddress).map(address => new Proxy(type, address)).ifPresent(clientBuilder.proxy);
		}

		private static InetSocketAddress ToUnresolvedAddress(HostAndPort address)
		{
			return InetSocketAddress.createUnresolved(address.Host, address.Port);
		}


		public static void SetupKerberos(OkHttpClient.Builder clientBuilder, string remoteServiceName, bool useCanonicalHostname, Optional<string> principal, Optional<File> kerberosConfig, Optional<File> keytab, Optional<File> credentialCache)
		{
			SpnegoHandler handler = new SpnegoHandler(remoteServiceName, useCanonicalHostname, principal, kerberosConfig, keytab, credentialCache);
			clientBuilder.addInterceptor(handler);
			clientBuilder.authenticator(handler);
		}
	}

}