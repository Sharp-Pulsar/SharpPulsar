using System.Globalization;
using System.Text;
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
    public class ClientSession
    {
        public Uri Server { get; }
        public string User { get; }
        public string Principal { get; }
        public string Source { get; }
        private readonly string _traceToken;
        public string Path { get; }
        public bool CompressionDisabled { get; }
        private readonly ISet<string> _clientTags;
        public string ClientInfo { get; }
        public string Catalog { get; }
        public string Schema { get; }
        public TimeZoneKey TimeZone { get; }
        public CultureInfo Locale { get; }
        private readonly IDictionary<string, string> _resourceEstimates;
        private readonly IDictionary<string, string> _properties;
        public string TransactionId { get; }
        public TimeSpan ClientRequestTimeout { get; }

        public static Builder NewBuilder(ClientSession clientSession)
        {
            return new Builder(clientSession);
        }
        public static ClientSession StripTransactionId(ClientSession session)
        {
            return NewBuilder(session).TransactionId().Build();
        }
        public ClientSession(Uri server, string principal, string user, string source, string traceToken, ISet<string> clientTags, string clientInfo, string catalog, string schema, string path, string timeZoneId, CultureInfo locale, IDictionary<string, string> resourceEstimates, IDictionary<string, string> properties, IDictionary<string, string> preparedStatements, IDictionary<string, ClientSelectedRole> roles, IDictionary<string, string> extraCredentials, string transactionId, TimeSpan clientRequestTimeout, bool compressionDisabled)
        {
            Server = Condition.RequireNonNull(server, "Server", "server is null");
            Principal = Condition.RequireNonNull(principal, "principal", "principal is null");
            User = Condition.RequireNonNull(user, "user", "user is null");
            Path = path;
            Source = source;
            _traceToken = Condition.RequireNonNull(traceToken, "traceToken", "traceToken is null");
            _clientTags = Condition.RequireNonNull(clientTags, "clientTags", "clientTags is null") ;
            ClientInfo = clientInfo;
            Catalog = catalog; 
            Schema = schema;
            Locale = locale;
            TimeZone = Condition.RequireNonNull(TimeZoneKey.GetTimeZoneKey(timeZoneId), "timeZoneId", "timeZoneId is null") ;
            TransactionId = transactionId;
            _resourceEstimates = Condition.RequireNonNull(resourceEstimates, "resourceEstimates", "resourceEstimates is null");
            _properties = Condition.RequireNonNull(properties, "properties", "properties is null");
            PreparedStatements = Condition.RequireNonNull(preparedStatements, "preparedStatements", "preparedStatements is null");
            Roles = Condition.RequireNonNull(roles, "roles", "roles is null");
            ExtraCredentials = Condition.RequireNonNull(extraCredentials, "credentialsS", "extraCredentials is null");
            ClientRequestTimeout = clientRequestTimeout;
            CompressionDisabled = compressionDisabled;

            foreach (var clientTag in clientTags)
            {
                Condition.CheckArgument(!clientTag.Contains(","), "client tag cannot contain ','");
            }

            if (resourceEstimates != null)
            {
                // verify that resource estimates are valid
                var charsetEncoder = new ASCIIEncoding();
                foreach (var entry in resourceEstimates.SetOfKeyValuePairs())
                {
                    Condition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Resource name is empty");
                    Condition.CheckArgument(entry.Key.IndexOf('=') < 0, "Resource name must not contain '=': %s", entry.Key);
                    //Condition.CheckArgument(charsetEncoder.canEncode(entry.Key), "Resource name is not US_ASCII: %s", entry.Key);
                }
            }

            if (properties != null)
            {
                // verify the properties are valid
                foreach (var entry in properties.SetOfKeyValuePairs())
                {
                    Condition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Session property name is empty");
                    Condition.CheckArgument(entry.Key.IndexOf('=') < 0, "Session property name must not contain '=': %s", entry.Key);
                    Condition.CheckArgument(Condition.CanEncode(entry.Key), "Session property name is not US_ASCII: %s", entry.Key);
                    Condition.CheckArgument(Condition.CanEncode(entry.Value), "Session property value is not US_ASCII: %s", entry.Value);
                }
            }

            if (extraCredentials != null)
            {
                // verify the extra credentials are valid
                foreach (var entry in extraCredentials.SetOfKeyValuePairs())
                {
                    Condition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Credential name is empty");
                    Condition.CheckArgument(entry.Key.IndexOf('=') < 0, "Credential name must not contain '=': %s", entry.Key);
                    Condition.CheckArgument(Condition.CanEncode(entry.Key), "Credential name is not US_ASCII: %s", entry.Key);
                    Condition.CheckArgument(Condition.CanEncode(entry.Value), "Credential value is not US_ASCII: %s", entry.Value);
                }
            }
        }




        public virtual string TraceToken => _traceToken;

        public virtual ISet<string> ClientTags => _clientTags;


        public virtual IDictionary<string, string> ResourceEstimates => _resourceEstimates;

        public virtual IDictionary<string, string> Properties => _properties;

        public virtual IDictionary<string, string> PreparedStatements { get; }

        /// <summary>
        /// Returns the map of catalog name -> selected role
        /// </summary>
        public virtual IDictionary<string, ClientSelectedRole> Roles { get; }

        public virtual IDictionary<string, string> ExtraCredentials { get; }


        public virtual bool Debug => false;


        public override string ToString()
        {
            return StringHelper
            .Build(this)
            .Add("server", Server)
            .Add("principal", Principal)
            .Add("user", User)
            .Add("clientTags", ClientTags)
            .Add("clientInfo", ClientInfo)
            .Add("catalog", Catalog)
            .Add("schema", Schema)
            .Add("path", Path)
            .Add("traceToken", TraceToken)
            .Add("timeZone", TimeZone)
            .Add("locale", Locale.Name)
            .Add("properties", Properties)
            .Add("transactionId", TransactionId)
            .ToString();
        }

        public sealed class Builder
        {
            internal Uri server;
            internal string user;
            internal string principal;
            internal string path;
            internal string source;
            internal string traceToken;
            internal ISet<string> clientTags;
            internal string clientInfo;
            internal string catalog;
            internal string schema;
            internal TimeZoneKey timeZone;
            internal CultureInfo locale;
            internal IDictionary<string, string> resourceEstimates;
            internal IDictionary<string, string> properties;
            internal IDictionary<string, string> preparedStatements;
            internal IDictionary<string, ClientSelectedRole> roles;
            internal IDictionary<string, string> credentials;
            internal string transactionId;
            internal TimeSpan clientRequestTimeout;
            internal bool compressionDisabled;

            public Builder(ClientSession clientSession)
            {
                Condition.RequireNonNull(clientSession, "clientSession", "clientSession is null");
                server = clientSession.Server;
                principal = clientSession.Principal;
                compressionDisabled = clientSession.CompressionDisabled;
                user = clientSession.User;
                path = clientSession.Path;
                source = clientSession.Source;
                traceToken = clientSession.TraceToken;
                clientTags = clientSession.ClientTags;
                clientInfo = clientSession.ClientInfo;
                catalog = clientSession.Catalog;
                schema = clientSession.Schema;
                timeZone = clientSession.TimeZone;
                locale = clientSession.Locale;
                resourceEstimates = clientSession.ResourceEstimates;
                properties = clientSession.Properties;
                preparedStatements = clientSession.PreparedStatements;
                roles = clientSession.Roles;
                credentials = clientSession.ExtraCredentials;
                transactionId = clientSession.TransactionId;
                clientRequestTimeout = clientSession.ClientRequestTimeout;
            }


            public Builder Server(Uri server)
            {
                this.server = server;
                return this;
            }

            public Builder User(string user)
            {
                this.user = user;
                return this;
            }

            public Builder Principal(string principal)
            {
                this.principal = principal;
                return this;
            }

            public Builder Source(string source)
            {
                this.source = source;
                return this;
            }

            public Builder TraceToken(string traceToken)
            {
                this.traceToken = traceToken;
                return this;
            }

            public Builder ClientTags(ISet<string> clientTags)
            {
                this.clientTags = clientTags;
                return this;
            }

            public Builder ClientInfo(string clientInfo)
            {
                this.clientInfo = clientInfo;
                return this;
            }
            public Builder Catalog(string catalog)
            {
                this.catalog = catalog;
                return this;
            }

            public Builder Schema(string schema)
            {
                this.schema = schema;
                return this;
            }

            public Builder Path(string path)
            {
                this.path = path;
                return this;
            }

            public Builder TimeZone(string timeZone)
            {
                this.timeZone = TimeZoneKey.GetTimeZoneKey(timeZone);
                return this;
            }

            public Builder Locale(CultureInfo locale)
            {
                this.locale = locale;
                return this;
            }

            public Builder ResourceEstimates(IDictionary<string, string> resourceEstimates)
            {
                this.resourceEstimates = resourceEstimates;
                return this;
            }
            public Builder Properties(IDictionary<string, string> properties)
            {
                this.properties = properties;
                return this;
            }

            public Builder Roles(IDictionary<string, ClientSelectedRole> roles)
            {
                this.roles = roles;
                return this;
            }

            public Builder Credentials(IDictionary<string, string> credentials)
            {
                this.credentials = credentials ;
                return this;
            }

            public Builder PreparedStatements(IDictionary<string, string> preparedStatements)
            {
                this.preparedStatements = preparedStatements;
                return this;
            }

            public Builder TransactionId(string transactionId = "")
            {
                this.transactionId = transactionId;
                return this;
            }

            public Builder ClientRequestTimeout(TimeSpan clientRequestTimeout)
            {
                this.clientRequestTimeout = clientRequestTimeout;
                return this;
            }

            public Builder CompressionDisabled(bool compressionDisabled)
            {
                this.compressionDisabled = compressionDisabled;
                return this;
            }

            public ClientSession Build()
            {
                return new ClientSession(server, principal, user, source, traceToken, clientTags, clientInfo, catalog, schema, path, timeZone.Id, locale, resourceEstimates, properties, preparedStatements, roles, credentials, transactionId, clientRequestTimeout, compressionDisabled);
            }
        }
    }

}