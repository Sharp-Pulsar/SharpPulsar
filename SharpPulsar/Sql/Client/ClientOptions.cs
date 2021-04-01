using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using NodaTime;
using SharpPulsar.Precondition;
using SharpPulsar.Presto;
using SharpPulsar.Presto.Facebook.Type;

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
namespace SharpPulsar.Sql.Client
{

    public class ClientOptions
    {
        public string Server = "localhost:8080";

        public string AccessToken;

        public string User = Environment.UserName;
        public string Principal = Environment.UserName;

        public string Password;

        public string Source = "sharp-pulsar";

        public string ClientInfo;

        public string ClientTags = "";

        public string Catalog;

        public string Schema;

        public string File;

        public string Execute;

        public string SessionUser;

        public readonly IList<ClientResourceEstimate> ResourceEstimates = new List<ClientResourceEstimate>();

        public readonly IList<ClientSessionProperty> SessionProperties = new List<ClientSessionProperty>();

        public readonly IList<ClientExtraCredential> ExtraCredentials = new List<ClientExtraCredential>();

        public Uri SocksProxy;

        public Uri HttpProxy;

        public TimeSpan ClientRequestTimeout = TimeSpan.FromMinutes(2);

        public bool ExternalAuthentication;
        public bool DisableCompression;


        public virtual ClientSession ToClientSession()
        {
            var timeZoneId = DateTimeZoneProviders.Tzdb.GetSystemDefault().ToString();
            return new ClientSession(ParseServer(Server), SessionUser, User, Source, null, ParseClientTags(ClientTags), ClientInfo, Catalog, Schema, null, timeZoneId, CultureInfo.CurrentCulture, ToResourceEstimates(ResourceEstimates), ToProperties(SessionProperties), new Dictionary<string, string>(), new Dictionary<string, SelectedRole>(), ToExtraCredentials(ExtraCredentials), null, ClientRequestTimeout, DisableCompression);
        }

        public static Uri ParseServer(string server)
        {
            server = server.ToLower();
            if (server.StartsWith("http://", StringComparison.Ordinal) || server.StartsWith("https://", StringComparison.Ordinal))
            {
                return new Uri(server);
            }

            var host = Dns.GetHostEntry(Dns.GetHostName());
            //port = host.getPortOrDefault(80)
            return new Uri($"http://{host.HostName}:{80}");
        }

        public static ISet<string> ParseClientTags(string clientTagsString)
        {
            return clientTagsString.Split(',').ToHashSet();
        }

        public static IDictionary<string, string> ToProperties(IList<ClientSessionProperty> sessionProperties)
        {
            var builder = new Dictionary<string, string>();
            foreach (var sessionProperty in sessionProperties)
            {
                var name = sessionProperty.Name;
                if (!string.IsNullOrWhiteSpace(sessionProperty.Catalog))
                {
                    name = sessionProperty.Catalog + "." + name;
                }
                builder.Add(name, sessionProperty.Value);
            }
            return builder;
        }

        public static IDictionary<string, string> ToResourceEstimates(IList<ClientResourceEstimate> estimates)
        {
            var builder = new Dictionary<string, string>();
            foreach (var estimate in estimates)
            {
                builder.Add(estimate.Resource, estimate.Estimate);
            }
            return builder;
        }

        public static IDictionary<string, string> ToExtraCredentials(IList<ClientExtraCredential> extraCredentials)
        {
            var builder = new Dictionary<string, string>();
            foreach (var credential in extraCredentials)
            {
                builder.Add(credential.Name, credential.Value);
            }
            return builder;
        }

        public sealed class ClientResourceEstimate
        {
            public ClientResourceEstimate(string resourceEstimate)
            {
                IList<string> nameValue = resourceEstimate.Split('=');
                Condition.CheckArgument(nameValue.Count == 2, $"Resource estimate: {resourceEstimate}");

                Resource = nameValue[0];
                Estimate = nameValue[1];
                Condition.CheckArgument(Resource.Length > 0, "Resource name is empty");
                Condition.CheckArgument(Estimate.Length > 0, "Resource estimate is empty");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(Resource), "Resource contains spaces or is not US_ASCII: %s", Resource);
                Condition.CheckArgument(Resource.IndexOf('=') < 0, $"Resource must not contain '=': {Resource}");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(Estimate), "Resource estimate contains spaces or is not US_ASCII: %s", Resource);
            }

            public ClientResourceEstimate(string resource, string estimate)
            {
                Resource = Condition.RequireNonNull(resource, "resource is null");
                Estimate = estimate;
            }

            public string Resource { get; }

            public string Estimate { get; }

            public override string ToString()
            {
                return Resource + '=' + Estimate;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }
                var other = (ClientResourceEstimate)o;
                return Equals(Resource, other.Resource) && Equals(Estimate, other.Estimate);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Resource, Estimate);
            }
        }

        public sealed class ClientSessionProperty
        {
            public ClientSessionProperty(string property)
            {
                IList<string> nameValue = property.Split('=');
                Condition.CheckArgument(nameValue.Count == 2, $"Session property: {property}");

                IList<string> nameParts = nameValue[0].Split('.');
                Condition.CheckArgument(nameParts.Count == 1 || nameParts.Count == 2, $"Invalid session property: {property}");
                if (nameParts.Count == 1)
                {
                    Catalog = null;
                    Name = nameParts[0];
                }
                else
                {
                    Catalog = nameParts[0];
                    Name = nameParts[1];
                }
                Value = nameValue[1];

                VerifyProperty(Catalog, Name, Value);
            }

            public ClientSessionProperty(string catalog, string name, string value)
            {
                Catalog = Condition.RequireNonNull(catalog, "catalog is null");
                Name = Condition.RequireNonNull(name, "name is null");
                Value = Condition.RequireNonNull(value, "value is null");

                VerifyProperty(catalog, name, value);
            }

            internal static void VerifyProperty(string catalog, string name, string value)
            {
                Condition.CheckArgument(!string.IsNullOrWhiteSpace(catalog), $"Invalid session property: {catalog}.{name}:{value}", catalog, name, value);
                Condition.CheckArgument(name.Length > 0, "Session property name is empty");
                Condition.CheckArgument(catalog?.IndexOf('=') < 0, $"Session property catalog must not contain '=': {name}");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(catalog.orElse("")), "Session property catalog contains spaces or is not US_ASCII: %s", name);
                Condition.CheckArgument(name.IndexOf('=') < 0, $"Session property name must not contain '=': {name}");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(name), "Session property name contains spaces or is not US_ASCII: %s", name);
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(value), "Session property value contains spaces or is not US_ASCII: %s", value);
            }

            public string Catalog { get; }

            public string Name { get; }

            public string Value { get; }

            public override string ToString()
            {
                return (!string.IsNullOrWhiteSpace(Catalog) ? Catalog + '.' : "") + Name + '=' + Value;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Catalog, Name, Value);
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                var other = (ClientSessionProperty)obj;
                return Equals(Catalog, other.Catalog) && Equals(Name, other.Name) && Equals(Value, other.Value);
            }
        }

        public sealed class ClientExtraCredential
        {
            public ClientExtraCredential(string extraCredential)
            {
                IList<string> nameValue = extraCredential.Split('=');
                Condition.CheckArgument(nameValue.Count == 2, $"Extra credential: {extraCredential}");

                Name = nameValue[0];
                Value = nameValue[1];
                Condition.CheckArgument(Name.Length > 0, "Credential name is empty");
                Condition.CheckArgument(Value.Length > 0, "Credential value is empty");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(Name), "Credential name contains spaces or is not US_ASCII: %s", Name);
                Condition.CheckArgument(Name.IndexOf('=') < 0, $"Credential name must not contain '=': {Name}");
                //Condition.CheckArgument(PRINTABLE_ASCII.matchesAllOf(Value), "Credential value contains space or is not US_ASCII: %s", Name);
            }

            public ClientExtraCredential(string name, string value)
            {
                Name = Condition.RequireNonNull(name, "name is null");
                Value = value;
            }

            public string Name { get; }

            public string Value { get; }

            public override string ToString()
            {
                return Name + '=' + Value;
            }

            public override bool Equals(object o)
            {
                if (this == o)
                {
                    return true;
                }
                if (o == null || GetType() != o.GetType())
                {
                    return false;
                }
                var other = (ClientExtraCredential)o;
                return Equals(Name, other.Name) && Equals(Value, other.Value);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, Value);
            }
        }
    }

}