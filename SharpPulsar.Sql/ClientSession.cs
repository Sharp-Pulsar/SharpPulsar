using System.Collections.Generic;

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
	using SelectedRole = com.facebook.presto.spi.security.SelectedRole;
	using TimeZoneKey = com.facebook.presto.spi.type.TimeZoneKey;
	using ImmutableMap = com.google.common.collect.ImmutableMap;
	using ImmutableSet = com.google.common.collect.ImmutableSet;
	using Duration = io.airlift.units.Duration;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.toStringHelper;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkArgument;

	public class ClientSession
	{
		public virtual  string Server {get;}
		public virtual string User {get;}
		public virtual string Source {get;}
		private readonly string _traceToken;
		private readonly ISet<string> _clientTags;
		public virtual string ClientInfo {get;}
		public virtual string Catalog {get;}
		public virtual string Schema {get;}
		public virtual string TimeZone {get;}
		public virtual string Locale {get;}
		private readonly IDictionary<string, string> _resourceEstimates;
		private readonly IDictionary<string, string> _properties;
		private readonly IDictionary<string, string> _preparedStatements;
		private readonly IDictionary<string, SelectedRole> _roles;
		private readonly IDictionary<string, string> _extraCredentials;
		public virtual string TransactionId {get;}
		public virtual string ClientRequestTimeout {get;}

		public static Builder Builder(ClientSession clientSession)
		{
			return new Builder(clientSession);
		}

		public static ClientSession StripTransactionId(ClientSession session)
		{
			return ClientSession.Builder(session).withoutTransactionId().build();
		}

		public ClientSession(URI server, string user, string source, Optional<string> traceToken, ISet<string> clientTags, string clientInfo, string catalog, string schema, string timeZoneId, Locale locale, IDictionary<string, string> resourceEstimates, IDictionary<string, string> properties, IDictionary<string, string> preparedStatements, IDictionary<string, SelectedRole> roles, IDictionary<string, string> extraCredentials, string transactionId, Duration clientRequestTimeout)
		{
			this.Server = requireNonNull(server, "server is null");
			this.User = user;
			this.Source = source;
			this._traceToken = requireNonNull(traceToken, "traceToken is null");
			this._clientTags = ImmutableSet.copyOf(requireNonNull(clientTags, "clientTags is null"));
			this.ClientInfo = clientInfo;
			this.Catalog = catalog;
			this.Schema = schema;
			this.Locale = locale;
			this.TimeZone = TimeZoneKey.getTimeZoneKey(timeZoneId);
			this.TransactionId = transactionId;
			this._resourceEstimates = ImmutableMap.copyOf(requireNonNull(resourceEstimates, "resourceEstimates is null"));
			this._properties = ImmutableMap.copyOf(requireNonNull(properties, "properties is null"));
			this._preparedStatements = ImmutableMap.copyOf(requireNonNull(preparedStatements, "preparedStatements is null"));
			this._roles = ImmutableMap.copyOf(requireNonNull(roles, "roles is null"));
			this._extraCredentials = ImmutableMap.copyOf(requireNonNull(extraCredentials, "extraCredentials is null"));
			this.ClientRequestTimeout = clientRequestTimeout;

			foreach (string clientTag in clientTags)
			{
				checkArgument(!clientTag.Contains(","), "client tag cannot contain ','");
			}

			// verify that resource estimates are valid
			CharsetEncoder charsetEncoder = US_ASCII.newEncoder();
			foreach (KeyValuePair<string, string> entry in resourceEstimates.SetOfKeyValuePairs())
			{
				checkArgument(!entry.Key.Empty, "Resource name is empty");
				checkArgument(entry.Key.IndexOf('=') < 0, "Resource name must not contain '=': %s", entry.Key);
				checkArgument(charsetEncoder.canEncode(entry.Key), "Resource name is not US_ASCII: %s", entry.Key);
			}

			// verify the properties are valid
			foreach (KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs())
			{
				checkArgument(!entry.Key.Empty, "Session property name is empty");
				checkArgument(entry.Key.IndexOf('=') < 0, "Session property name must not contain '=': %s", entry.Key);
				checkArgument(charsetEncoder.canEncode(entry.Key), "Session property name is not US_ASCII: %s", entry.Key);
				checkArgument(charsetEncoder.canEncode(entry.Value), "Session property value is not US_ASCII: %s", entry.Value);
			}

			// verify the extra credentials are valid
			foreach (KeyValuePair<string, string> entry in extraCredentials.SetOfKeyValuePairs())
			{
				checkArgument(!entry.Key.Empty, "Credential name is empty");
				checkArgument(entry.Key.IndexOf('=') < 0, "Credential name must not contain '=': %s", entry.Key);
				checkArgument(charsetEncoder.canEncode(entry.Key), "Credential name is not US_ASCII: %s", entry.Key);
				checkArgument(charsetEncoder.canEncode(entry.Value), "Credential value is not US_ASCII: %s", entry.Value);
			}
		}




		public virtual Optional<string> TraceToken
		{
			get
			{
				return _traceToken;
			}
		}

		public virtual ISet<string> ClientTags
		{
			get
			{
				return _clientTags;
			}
		}






		public virtual IDictionary<string, string> ResourceEstimates
		{
			get
			{
				return _resourceEstimates;
			}
		}

		public virtual IDictionary<string, string> Properties
		{
			get
			{
				return _properties;
			}
		}

		public virtual IDictionary<string, string> PreparedStatements
		{
			get
			{
				return _preparedStatements;
			}
		}

		/// <summary>
		/// Returns the map of catalog name -> selected role
		/// </summary>
		public virtual IDictionary<string, SelectedRole> Roles
		{
			get
			{
				return _roles;
			}
		}

		public virtual IDictionary<string, string> ExtraCredentials
		{
			get
			{
				return _extraCredentials;
			}
		}


		public virtual bool Debug
		{
			get
			{
				return false;
			}
		}


		public override string ToString()
		{
			return toStringHelper(this).add("server", Server).add("user", User).add("clientTags", _clientTags).add("clientInfo", ClientInfo).add("catalog", Catalog).add("schema", Schema).add("traceToken", _traceToken.orElse(null)).add("timeZone", TimeZone).add("locale", Locale).add("properties", _properties).add("transactionId", TransactionId).omitNullValues().ToString();
		}

		public sealed class Builder
		{
			internal URI Server;
			internal string User;
			internal string Source;
			internal Optional<string> TraceToken;
			internal ISet<string> ClientTags;
			internal string ClientInfo;
			internal string Catalog;
			internal string Schema;
			internal TimeZoneKey TimeZone;
			internal Locale Locale;
			internal IDictionary<string, string> ResourceEstimates;
			internal IDictionary<string, string> Properties;
			internal IDictionary<string, string> PreparedStatements;
			internal IDictionary<string, SelectedRole> Roles;
			internal IDictionary<string, string> Credentials;
			internal string TransactionId;
			internal Duration ClientRequestTimeout;

			public Builder(ClientSession clientSession)
			{
				requireNonNull(clientSession, "clientSession is null");
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
				this.Catalog = requireNonNull(catalog, "catalog is null");
				return this;
			}

			public Builder WithSchema(string schema)
			{
				this.Schema = requireNonNull(schema, "schema is null");
				return this;
			}

			public Builder WithProperties(IDictionary<string, string> properties)
			{
				this.Properties = requireNonNull(properties, "properties is null");
				return this;
			}

			public Builder WithRoles(IDictionary<string, SelectedRole> roles)
			{
				this.Roles = roles;
				return this;
			}

			public Builder WithCredentials(IDictionary<string, string> credentials)
			{
				this.Credentials = requireNonNull(credentials, "extraCredentials is null");
				return this;
			}

			public Builder WithPreparedStatements(IDictionary<string, string> preparedStatements)
			{
				this.PreparedStatements = requireNonNull(preparedStatements, "preparedStatements is null");
				return this;
			}

			public Builder WithTransactionId(string transactionId)
			{
				this.TransactionId = requireNonNull(transactionId, "transactionId is null");
				return this;
			}

			public Builder WithoutTransactionId()
			{
				this.TransactionId = null;
				return this;
			}

			public ClientSession Build()
			{
				return new ClientSession(Server, User, Source, TraceToken, ClientTags, ClientInfo, Catalog, Schema, TimeZone.Id, Locale, ResourceEstimates, Properties, PreparedStatements, Roles, Credentials, TransactionId, ClientRequestTimeout);
			}
		}
	}

}