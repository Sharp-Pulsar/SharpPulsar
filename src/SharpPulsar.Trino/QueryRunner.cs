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

using Akka.Actor;
using Akka.Event;
using SharpPulsar.Trino.Precondition;
using SharpPulsar.Trino.Trino;

namespace SharpPulsar.Trino
{

    internal class QueryRunner
    {
        private ClientSession _session;
        private readonly HttpClient _httpClient;

        public QueryRunner(ClientSession session, string accessToken, string user, string password)
        {
            _session = Condition.RequireNonNull(session, "session is null");
            _httpClient = new HttpClient();
            _httpClient.SetupTimeouts(30000);
            SetupBasicAuth(_httpClient, session, user, password);
            SetupTokenAuth(_httpClient, session, accessToken);

            //SetupExternalAuth(builder, session, externalAuthentication, _sslSetup);
            //SetupNetworkLogging(builder);
        }

        public ClientSession Session
        {
            get => _session;
            set => _session = Condition.RequireNonNull(value, "session is null");
        }


        public Query StartQuery(string query, IActorRef output, ILoggingAdapter log)
        {
            return new Query(StartInternalQuery(_session, query), output, log);
        }

        public IStatementClient StartInternalQuery(string query)
        {
            return StartInternalQuery(ClientSession.StripTransactionId(_session), query);
        }

        private IStatementClient StartInternalQuery(ClientSession session, string query)
        {
            return StatementClientFactory.NewStatementClient(_httpClient, session, query);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
        /*
		private static void SetupExternalAuth(OkHttpClient.Builder builder, ClientSession session, bool enabled, System.Action<OkHttpClient.Builder> sslSetup)
		{
			if(!enabled)
			{
				return;
			}
			checkArgument(session.Server.Scheme.equalsIgnoreCase("https"), "Authentication using externalAuthentication requires HTTPS to be enabled");

			RedirectHandler redirectHandler = uri =>
			{
			@out.println("External authentication required. Please go to:");
			@out.println(uri.ToString());
			};
			TokenPoller poller = new HttpTokenPoller(builder.build(), sslSetup);

			ExternalAuthenticator authenticator = new ExternalAuthenticator(redirectHandler, poller, Duration.ofMinutes(10));

			builder.authenticator(authenticator);
			builder.addInterceptor(authenticator);
		}*/
        private static void SetupBasicAuth(HttpClient client, ClientSession session, string user, string password)
        {
            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
            {
                Condition.CheckArgument(session.Server.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase), "Authentication using username/password requires HTTPS to be enabled");
                client.BasicAuth(user, password);
            }
        }

        private static void SetupTokenAuth(HttpClient client, ClientSession session, string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                Condition.CheckArgument(session.Server.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase), "Authentication using an access token requires HTTPS to be enabled");
                client.TokenAuth(accessToken);
            }
        }
    }

}