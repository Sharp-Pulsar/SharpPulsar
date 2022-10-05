using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MihaZupan;
using SharpPulsar.Trino.Precondition;

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
namespace SharpPulsar.Trino.Trino
{
    public static class HttpUtil
    {

        public static void UserAgent(this HttpClient httpclient, string userAgent)
        {
            httpclient.DefaultRequestHeaders.Add("User-Agent", userAgent); ;
        }

        public static void BasicAuth(this HttpClient client, string user, string password)
        {
            Condition.RequireNonNull(user, "user is null");
            Condition.RequireNonNull(password, "password is null");
            if (user.Contains(":"))
            {
                throw new ClientException("Illegal character ':' found in username");
            }

            var authToken = Encoding.ASCII.GetBytes($"{user}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(authToken));

        }

        public static void TokenAuth(this HttpClient client, string accessToken)
        {
            Condition.RequireNonNull(accessToken, "accessToken is null");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public static void SetupTimeouts(this HttpClient client, int timeoutMilis)
        {
            client.Timeout = TimeSpan.FromMilliseconds(timeoutMilis);
        }

        public static HttpClient SetupSocksProxy(string host, int port)
        {
            var proxy = new HttpToSocks5Proxy(host, port);
            var handler = new HttpClientHandler { Proxy = proxy };
            return new HttpClient(handler, true);
        }

        public static HttpClient SetupHttpProxy(string proxyHost, int proxyPort, string proxyUserName, string proxyPassword, bool needServerAuthentication = false, string serverUserName = "", string serverPassword = "")
        {
            // First create a proxy object
            var proxy = new WebProxy
            {
                Address = new Uri($"http://{proxyHost}:{proxyPort}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,

                // *** These creds are given to the proxy server, not the web server ***
                Credentials = new NetworkCredential(
                    userName: proxyUserName,
                    password: proxyPassword)
            };

            // Now create a client handler which uses that proxy
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
            };
            if (needServerAuthentication)
            {
                httpClientHandler.PreAuthenticate = true;
                httpClientHandler.UseDefaultCredentials = false;

                // *** These creds are given to the web server, not the proxy server ***
                httpClientHandler.Credentials = new NetworkCredential(
                    userName: serverUserName,
                    password: serverPassword);
            }
            /*var handler = new HttpClientHandler();
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            */
            return new HttpClient(httpClientHandler, disposeHandler: true);
        }
        //COME BACK TO THIS AND USE https://github.com/dotnet/Kerberos.NET
        /*public static void SetupKerberos(OkHttpClient.Builder clientBuilder, string remoteServiceName, bool useCanonicalHostname, Optional<string> principal, Optional<File> kerberosConfig, Optional<File> keytab, Optional<File> credentialCache)
		{
			SpnegoHandler handler = new SpnegoHandler(remoteServiceName, useCanonicalHostname, principal, kerberosConfig, keytab, credentialCache);
			clientBuilder.addInterceptor(handler);
			clientBuilder.authenticator(handler);
		}*/
    }

}