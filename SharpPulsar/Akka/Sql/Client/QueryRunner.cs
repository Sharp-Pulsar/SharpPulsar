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

using System.Net.Http;
using SharpPulsar.Presto;

namespace SharpPulsar.Akka.Sql.Client
{

	public class QueryRunner : System.IDisposable
	{
		private readonly ClientSession session;
		private readonly bool debug;
		private readonly HttpClient httpClient;
		private readonly System.Action<HttpClient> sslSetup;

		public QueryRunner(ClientSession session, bool debug, Optional<HostAndPort> socksProxy, Optional<HostAndPort> httpProxy, Optional<string> keystorePath, Optional<string> keystorePassword, Optional<string> truststorePath, Optional<string> truststorePassword, Optional<string> accessToken, Optional<string> user, Optional<string> password, Optional<string> kerberosPrincipal, Optional<string> kerberosRemoteServiceName, Optional<string> kerberosConfigPath, Optional<string> kerberosKeytabPath, Optional<string> kerberosCredentialCachePath, bool kerberosUseCanonicalHostname)
		{
			this.session = new requireNonNull(session, "session is null"));
			this.debug = debug;

			this.sslSetup = builder => setupSsl(builder, keystorePath, keystorePassword, truststorePath, truststorePassword);

			OkHttpClient.Builder builder = new OkHttpClient.Builder();

			builder.socketFactory(new SocketChannelSocketFactory());

			setupTimeouts(builder, 30, SECONDS);
			setupCookieJar(builder);
			setupSocksProxy(builder, socksProxy);
			setupHttpProxy(builder, httpProxy);
			setupBasicAuth(builder, session, user, password);
			setupTokenAuth(builder, session, accessToken);

			if (kerberosRemoteServiceName.Present)
			{
				checkArgument(session.Server.Scheme.equalsIgnoreCase("https"), "Authentication using Kerberos requires HTTPS to be enabled");

                setupKerberos(builder, kerberosRemoteServiceName.get(), kerberosUseCanonicalHostname, kerberosPrincipal, kerberosConfigPath.map(File::new), kerberosKeytabPath.map(File::new), kerberosCredentialCachePath.map(File::new));
			}

			this.httpClient = builder.build();
		}

		public virtual ClientSession Session
		{
			get
			{
				return session.get();
			}
			set
			{
				this.session.set(requireNonNull(value, "session is null"));
			}
		}


		public virtual bool Debug
		{
			get
			{
				return debug;
			}
		}

		public virtual Query startQuery(string query)
		{
			return new Query(startInternalQuery(session.get(), query), debug);
		}

		public virtual IStatementClient startInternalQuery(string query)
		{
			return startInternalQuery(stripTransactionId(session.get()), query);
		}

		private IStatementClient startInternalQuery(ClientSession session, string query)
		{
			OkHttpClient.Builder builder = httpClient.newBuilder();
			sslSetup.accept(builder);
			OkHttpClient client = builder.build();

			return newStatementClient(client, session, query);
		}

		public virtual void Dispose()
		{
			httpClient.dispatcher().executorService().shutdown();
			httpClient.connectionPool().evictAll();
		}

		private static void setupBasicAuth(OkHttpClient.Builder clientBuilder, ClientSession session, Optional<string> user, Optional<string> password)
		{
			if (user.Present && password.Present)
			{
				checkArgument(session.Server.Scheme.equalsIgnoreCase("https"), "Authentication using username/password requires HTTPS to be enabled");
				clientBuilder.addInterceptor(basicAuth(user.get(), password.get()));
			}
		}

		private static void setupTokenAuth(OkHttpClient.Builder clientBuilder, ClientSession session, Optional<string> accessToken)
		{
			if (accessToken.Present)
			{
				checkArgument(session.Server.Scheme.equalsIgnoreCase("https"), "Authentication using an access token requires HTTPS to be enabled");
				clientBuilder.addInterceptor(tokenAuth(accessToken.get()));
			}
		}
	}

}