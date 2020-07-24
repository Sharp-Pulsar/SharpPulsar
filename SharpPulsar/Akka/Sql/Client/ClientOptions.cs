using System;
using System.Collections.Generic;
using SharpPulsar.Presto;

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
namespace SharpPulsar.Akka.Sql.Client
{

	public class ClientOptions
	{
		private static readonly Splitter NAME_VALUE_SPLITTER = Splitter.on('=').limit(2);
		private static readonly CharMatcher PRINTABLE_ASCII = CharMatcher.inRange((char) 0x21, (char) 0x7E); // spaces are not allowed
        public string server = "localhost:8080";

        public string krb5RemoteServiceName;

        public string krb5ConfigPath = "/etc/krb5.conf";

        public string krb5KeytabPath = "/etc/krb5.keytab";

        public string krb5CredentialCachePath = defaultCredentialCachePath().orElse(null);

        public string krb5Principal;

        public bool krb5DisableRemoteServiceHostnameCanonicalization;

        public string keystorePath;

        public string keystorePassword;

        public string truststorePath;

        public string truststorePassword;

        public string accessToken;

        public string user = System.getProperty("user.name");

        public bool password;

        public string source = "presto-cli";

        public string clientInfo;

        public string clientTags = "";

        public string catalog;

        public string schema;

        public string file;

        public bool debug;

        public string logLevelsFile;

        public string execute;

        public readonly IList<ClientResourceEstimate> resourceEstimates = new List<ClientResourceEstimate>();

        public readonly IList<ClientSessionProperty> sessionProperties = new List<ClientSessionProperty>();

        public readonly IList<ClientExtraCredential> extraCredentials = new List<ClientExtraCredential>();

        public HostAndPort socksProxy;

        public HostAndPort httpProxy;

        public Duration clientRequestTimeout = new Duration(2, MINUTES);

        public bool ignoreErrors;


		public virtual ClientSession toClientSession()
		{
			return new ClientSession(parseServer(server), user, source, null, parseClientTags(clientTags), clientInfo, catalog, schema, TimeZone.Default.ID, Locale.Default, toResourceEstimates(resourceEstimates), toProperties(sessionProperties), emptyMap(), emptyMap(), toExtraCredentials(extraCredentials), null, clientRequestTimeout);
		}

		public static URI parseServer(string server)
		{
			server = server.ToLower(ENGLISH);
			if (server.StartsWith("http://", StringComparison.Ordinal) || server.StartsWith("https://", StringComparison.Ordinal))
			{
				return URI.create(server);
			}

			HostAndPort host = HostAndPort.fromString(server);
			try
			{
				return new URI("http", null, host.Host, host.getPortOrDefault(80), null, null, null);
			}
			catch (URISyntaxException e)
			{
				throw new System.ArgumentException(e);
			}
		}

		public static ISet<string> parseClientTags(string clientTagsString)
		{
			Splitter splitter = Splitter.on(',').trimResults().omitEmptyStrings();
			return ImmutableSet.copyOf(splitter.split(nullToEmpty(clientTagsString)));
		}

		public static IDictionary<string, string> toProperties(IList<ClientSessionProperty> sessionProperties)
		{
			ImmutableMap.Builder<string, string> builder = ImmutableMap.builder();
			foreach (ClientSessionProperty sessionProperty in sessionProperties)
			{
				string name = sessionProperty.Name;
				if (sessionProperty.Catalog.Present)
				{
					name = sessionProperty.Catalog.get() + "." + name;
				}
				builder.put(name, sessionProperty.Value);
			}
			return builder.build();
		}

		public static IDictionary<string, string> toResourceEstimates(IList<ClientResourceEstimate> estimates)
		{
			ImmutableMap.Builder<string, string> builder = ImmutableMap.builder();
			foreach (ClientResourceEstimate estimate in estimates)
			{
				builder.put(estimate.Resource, estimate.Estimate);
			}
			return builder.build();
		}

		public static IDictionary<string, string> toExtraCredentials(IList<ClientExtraCredential> extraCredentials)
		{
			ImmutableMap.Builder<string, string> builder = ImmutableMap.builder();
			foreach (ClientExtraCredential credential in extraCredentials)
			{
				builder.put(credential.Name, credential.Value);
			}
			return builder.build();
		}

		public sealed class ClientResourceEstimate
		{
			internal readonly string resource;
			internal readonly string estimate;

			public ClientResourceEstimate(string resourceEstimate)
			{
				IList<string> nameValue = NAME_VALUE_SPLITTER.splitToList(resourceEstimate);
				checkArgument(nameValue.Count == 2, "Resource estimate: %s", resourceEstimate);

				this.resource = nameValue[0];
				this.estimate = nameValue[1];
				checkArgument(resource.Length > 0, "Resource name is empty");
				checkArgument(estimate.Length > 0, "Resource estimate is empty");
				checkArgument(PRINTABLE_ASCII.matchesAllOf(resource), "Resource contains spaces or is not US_ASCII: %s", resource);
				checkArgument(resource.IndexOf('=') < 0, "Resource must not contain '=': %s", resource);
				checkArgument(PRINTABLE_ASCII.matchesAllOf(estimate), "Resource estimate contains spaces or is not US_ASCII: %s", resource);
			}

			public ClientResourceEstimate(string resource, string estimate)
			{
				this.resource = requireNonNull(resource, "resource is null");
				this.estimate = estimate;
			}

			public string Resource
			{
				get
				{
					return resource;
				}
			}

			public string Estimate
			{
				get
				{
					return estimate;
				}
			}

			public override string ToString()
			{
				return resource + '=' + estimate;
			}

			public override bool Equals(object o)
			{
				if (this == o)
				{
					return true;
				}
				if (o == null || this.GetType() != o.GetType())
				{
					return false;
				}
				ClientResourceEstimate other = (ClientResourceEstimate) o;
				return Objects.equals(resource, other.resource) && Objects.equals(estimate, other.estimate);
			}

			public override int GetHashCode()
			{
				return Objects.hash(resource, estimate);
			}
		}

		public sealed class ClientSessionProperty
		{
			internal static readonly Splitter NAME_SPLITTER = Splitter.on('.');
			internal readonly Optional<string> catalog;
			internal readonly string name;
			internal readonly string value;

			public ClientSessionProperty(string property)
			{
				IList<string> nameValue = NAME_VALUE_SPLITTER.splitToList(property);
				checkArgument(nameValue.Count == 2, "Session property: %s", property);

				IList<string> nameParts = NAME_SPLITTER.splitToList(nameValue[0]);
				checkArgument(nameParts.Count == 1 || nameParts.Count == 2, "Invalid session property: %s", property);
				if (nameParts.Count == 1)
				{
					catalog = null;
					name = nameParts[0];
				}
				else
				{
					catalog = nameParts[0];
					name = nameParts[1];
				}
				value = nameValue[1];

				verifyProperty(catalog, name, value);
			}

			public ClientSessionProperty(Optional<string> catalog, string name, string value)
			{
				this.catalog = requireNonNull(catalog, "catalog is null");
				this.name = requireNonNull(name, "name is null");
				this.value = requireNonNull(value, "value is null");

				verifyProperty(catalog, name, value);
			}

			internal static void verifyProperty(Optional<string> catalog, string name, string value)
			{
				checkArgument(!catalog.Present || !catalog.get().Empty, "Invalid session property: %s.%s:%s", catalog, name, value);
				checkArgument(name.Length > 0, "Session property name is empty");
				checkArgument(catalog.orElse("").IndexOf('=') < 0, "Session property catalog must not contain '=': %s", name);
				checkArgument(PRINTABLE_ASCII.matchesAllOf(catalog.orElse("")), "Session property catalog contains spaces or is not US_ASCII: %s", name);
				checkArgument(name.IndexOf('=') < 0, "Session property name must not contain '=': %s", name);
				checkArgument(PRINTABLE_ASCII.matchesAllOf(name), "Session property name contains spaces or is not US_ASCII: %s", name);
				checkArgument(PRINTABLE_ASCII.matchesAllOf(value), "Session property value contains spaces or is not US_ASCII: %s", value);
			}

			public Optional<string> Catalog
			{
				get
				{
					return catalog;
				}
			}

			public string Name
			{
				get
				{
					return name;
				}
			}

			public string Value
			{
				get
				{
					return value;
				}
			}

			public override string ToString()
			{
				return (catalog.Present ? catalog.get() + '.' : "") + name + '=' + value;
			}

			public override int GetHashCode()
			{
				return Objects.hash(catalog, name, value);
			}

			public override bool Equals(object obj)
			{
				if (this == obj)
				{
					return true;
				}
				if (obj == null || this.GetType() != obj.GetType())
				{
					return false;
				}
				ClientSessionProperty other = (ClientSessionProperty) obj;
				return Objects.equals(this.catalog, other.catalog) && Objects.equals(this.name, other.name) && Objects.equals(this.value, other.value);
			}
		}

		public sealed class ClientExtraCredential
		{
			internal readonly string name;
			internal readonly string value;

			public ClientExtraCredential(string extraCredential)
			{
				IList<string> nameValue = NAME_VALUE_SPLITTER.splitToList(extraCredential);
				checkArgument(nameValue.Count == 2, "Extra credential: %s", extraCredential);

				this.name = nameValue[0];
				this.value = nameValue[1];
				checkArgument(name.Length > 0, "Credential name is empty");
				checkArgument(value.Length > 0, "Credential value is empty");
				checkArgument(PRINTABLE_ASCII.matchesAllOf(name), "Credential name contains spaces or is not US_ASCII: %s", name);
				checkArgument(name.IndexOf('=') < 0, "Credential name must not contain '=': %s", name);
				checkArgument(PRINTABLE_ASCII.matchesAllOf(value), "Credential value contains space or is not US_ASCII: %s", name);
			}

			public ClientExtraCredential(string name, string value)
			{
				this.name = requireNonNull(name, "name is null");
				this.value = value;
			}

			public string Name
			{
				get
				{
					return name;
				}
			}

			public string Value
			{
				get
				{
					return value;
				}
			}

			public override string ToString()
			{
				return name + '=' + value;
			}

			public override bool Equals(object o)
			{
				if (this == o)
				{
					return true;
				}
				if (o == null || this.GetType() != o.GetType())
				{
					return false;
				}
				ClientExtraCredential other = (ClientExtraCredential) o;
				return Objects.equals(name, other.name) && Objects.equals(value, other.value);
			}

			public override int GetHashCode()
			{
				return Objects.hash(name, value);
			}
		}
	}

}