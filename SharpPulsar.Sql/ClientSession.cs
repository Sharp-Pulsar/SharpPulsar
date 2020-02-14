using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SharpPulsar.Sql.Facebook;
using SharpPulsar.Sql.Facebook.Type;
using SharpPulsar.Sql.Precondition;

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
	public class ClientSession
	{
		public  string Server {get;}
		public string User {get;}
		public string Source {get;}
		private readonly string _traceToken;
		private readonly ISet<string> _clientTags;
		public string ClientInfo {get;}
		public string Catalog {get;}
		public string Schema {get;}
		public TimeZoneKey TimeZone {get;}
		public CultureInfo Locale {get;}
		private readonly IDictionary<string, string> _resourceEstimates;
		private readonly IDictionary<string, string> _properties;
        public  string TransactionId {get;}
		public TimeSpan ClientRequestTimeout {get;}

		public static Builder NewBuilder(ClientSession clientSession)
		{
			return new Builder(clientSession);
		}

		public static ClientSession StripTransactionId(ClientSession session)
		{
			return NewBuilder(session).WithoutTransactionId().Build();
		}

		public ClientSession(string server, string user, string source, string traceToken, ISet<string> clientTags, string clientInfo, string catalog, string schema, string timeZoneId, CultureInfo locale, IDictionary<string, string> resourceEstimates, IDictionary<string, string> properties, IDictionary<string, string> preparedStatements, IDictionary<string, SelectedRole> roles, IDictionary<string, string> extraCredentials, string transactionId, TimeSpan clientRequestTimeout)
		{
			Server = ParameterCondition.RequireNonNull(server,"Server", "server is null");
			User = user;
			Source = source;
			_traceToken = ParameterCondition.RequireNonNull(traceToken,"TraceToken", "traceToken is null");
			_clientTags = new HashSet<string>(ParameterCondition.RequireNonNull(clientTags, "ClientTags", "clientTags is null"));
			ClientInfo = clientInfo;
			Catalog = catalog;
			Schema = schema;
			Locale = locale;
			TimeZone = TimeZoneKey.GetTimeZoneKey(timeZoneId);
			TransactionId = transactionId;
			_resourceEstimates = new Dictionary<string, string>(ParameterCondition.RequireNonNull(resourceEstimates, "ResourceEstimates", "resourceEstimates is null"));
			_properties = new Dictionary<string, string>(ParameterCondition.RequireNonNull(properties, "properties is null"));
			PreparedStatements = new Dictionary<string, string>(ParameterCondition.RequireNonNull(preparedStatements, "preparedStatements is null"));
			Roles = new Dictionary<string, SelectedRole>(ParameterCondition.RequireNonNull(roles, "roles is null"));
			ExtraCredentials = new Dictionary<string, string>(ParameterCondition.RequireNonNull(extraCredentials, "extraCredentials is null"));
			ClientRequestTimeout = clientRequestTimeout;

			foreach (string clientTag in clientTags)
			{
				ParameterCondition.CheckArgument(!clientTag.Contains(","), "client tag cannot contain ','");
			}

			// verify that resource estimates are valid
			var charsetEncoder = new ASCIIEncoding();
			foreach (KeyValuePair<string, string> entry in resourceEstimates.SetOfKeyValuePairs())
			{
                ParameterCondition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Resource name is empty");
                ParameterCondition.CheckArgument(entry.Key.IndexOf('=') < 0, "Resource name must not contain '=': %s", entry.Key);
                //ParameterCondition.CheckArgument(charsetEncoder.canEncode(entry.Key), "Resource name is not US_ASCII: %s", entry.Key);
			}

			// verify the properties are valid
			foreach (KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs())
			{
				ParameterCondition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Session property name is empty");
                ParameterCondition.CheckArgument(entry.Key.IndexOf('=') < 0, "Session property name must not contain '=': %s", entry.Key);
                ParameterCondition.CheckArgument(ParameterCondition.CanEncode(entry.Key), "Session property name is not US_ASCII: %s", entry.Key);
                ParameterCondition.CheckArgument(ParameterCondition.CanEncode(entry.Value), "Session property value is not US_ASCII: %s", entry.Value);
			}

			// verify the extra credentials are valid
			foreach (KeyValuePair<string, string> entry in extraCredentials.SetOfKeyValuePairs())
			{
                ParameterCondition.CheckArgument(!string.IsNullOrWhiteSpace(entry.Key), "Credential name is empty");
                ParameterCondition.CheckArgument(entry.Key.IndexOf('=') < 0, "Credential name must not contain '=': %s", entry.Key);
                ParameterCondition.CheckArgument(ParameterCondition.CanEncode(entry.Key), "Credential name is not US_ASCII: %s", entry.Key);
                ParameterCondition.CheckArgument(ParameterCondition.CanEncode(entry.Value), "Credential value is not US_ASCII: %s", entry.Value);
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
		public virtual IDictionary<string, SelectedRole> Roles { get; }

        public virtual IDictionary<string, string> ExtraCredentials { get; }


        public virtual bool Debug => false;


		public String toString()
        {
            return StringHelper.Build(this)
                .Add("server", Server)
                .Add("user", User)
                .Add("clientTags", ClientTags)
                .Add("clientInfo", ClientInfo)
                .Add("catalog", Catalog)
                .Add("schema", Schema)
                .Add("traceToken", TraceToken)
                .Add("timeZone", TimeZone)
                .Add("locale", Locale.Name)
                .Add("properties", Properties)
                .Add("transactionId", TransactionId)
                .ToString();
        }

		public sealed class Builder
		{
			internal string Server;
			internal string User;
			internal string Source;
			internal string TraceToken;
			internal ISet<string> ClientTags;
			internal string ClientInfo;
			internal string Catalog;
			internal string Schema;
			internal TimeZoneKey TimeZone;
			internal CultureInfo Locale;
			internal IDictionary<string, string> ResourceEstimates;
			internal IDictionary<string, string> Properties;
			internal IDictionary<string, string> PreparedStatements;
			internal IDictionary<string, SelectedRole> Roles;
			internal IDictionary<string, string> Credentials;
			internal string TransactionId;
			internal TimeSpan ClientRequestTimeout;

			public Builder(ClientSession clientSession)
			{
				ParameterCondition.RequireNonNull(clientSession, "clientSession", "clientSession is null");
				Server = clientSession.Server;
				User = clientSession.User;
				Source = clientSession.Source;
				TraceToken = clientSession.TraceToken;
				ClientTags = clientSession.ClientTags;
				ClientInfo = clientSession.ClientInfo;
				Catalog = clientSession.Catalog;
				Schema = clientSession.Schema;
				TimeZone = clientSession.TimeZone;
				Locale = clientSession.Locale;
				ResourceEstimates = clientSession.ResourceEstimates;
				Properties = clientSession.Properties;
				PreparedStatements = clientSession.PreparedStatements;
				Roles = clientSession.Roles;
				Credentials = clientSession.ExtraCredentials;
				TransactionId = clientSession.TransactionId;
				ClientRequestTimeout = clientSession.ClientRequestTimeout;
			}

			public Builder WithCatalog(string catalog)
			{
				Catalog = ParameterCondition.RequireNonNull(catalog, "catalog", "catalog is null");
				return this;
			}

			public Builder WithSchema(string schema)
			{
				Schema = ParameterCondition.RequireNonNull(schema, "schema", "schema is null");
				return this;
			}

			public Builder WithProperties(IDictionary<string, string> properties)
			{
				Properties = ParameterCondition.RequireNonNull(properties, "properties", "properties is null");
				return this;
			}

			public Builder WithRoles(IDictionary<string, SelectedRole> roles)
			{
				Roles = roles;
				return this;
			}

			public Builder WithCredentials(IDictionary<string, string> credentials)
			{
				Credentials = ParameterCondition.RequireNonNull(credentials, "credentialsS", "extraCredentials is null");
				return this;
			}

			public Builder WithPreparedStatements(IDictionary<string, string> preparedStatements)
			{
				PreparedStatements = ParameterCondition.RequireNonNull(preparedStatements, "preparedStatements", "preparedStatements is null");
				return this;
			}

			public Builder WithTransactionId(string transactionId)
			{
				TransactionId = ParameterCondition.RequireNonNull(transactionId, "transactionId", "transactionId is null");
				return this;
			}

			public Builder WithoutTransactionId()
			{
				TransactionId = null;
				return this;
			}

			public ClientSession Build()
			{
				return new ClientSession(Server, User, Source, TraceToken, ClientTags, ClientInfo, Catalog, Schema, TimeZone.Id, Locale, ResourceEstimates, Properties, PreparedStatements, Roles, Credentials, TransactionId, ClientRequestTimeout);
			}

		}
	}

}