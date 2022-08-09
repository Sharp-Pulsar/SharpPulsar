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

//COME BACK TO THIS AND USE https://github.com/dotnet/Kerberos.NET
namespace SharpPulsar.Sql.Presto
{
    /*
    using ImmutableMap = com.google.common.collect.ImmutableMap;
	using Krb5LoginModule = com.sun.security.auth.module.Krb5LoginModule;
	using Duration = io.airlift.units.Duration;
	using Authenticator = okhttp3.Authenticator;
	using Interceptor = okhttp3.Interceptor;
	using Request = okhttp3.Request;
	using Response = okhttp3.Response;
	using Route = okhttp3.Route;
	using GSSContext = org.ietf.jgss.GSSContext;
	using GSSCredential = org.ietf.jgss.GSSCredential;
	using GSSException = org.ietf.jgss.GSSException;
	using GSSManager = org.ietf.jgss.GSSManager;
	using Oid = org.ietf.jgss.Oid;


	// TODO: This class is similar to SpnegoAuthentication in Airlift. Consider extracting a library.
	public class SpnegoHandler : Interceptor, Authenticator
	{
		private const string NEGOTIATE = "Negotiate";
		private static readonly Duration MIN_CREDENTIAL_LIFETIME = new Duration(60, SECONDS);

		private static readonly GSSManager GSS_MANAGER = GSSManager.Instance;

		private static readonly Oid SPNEGO_OID = createOid("1.3.6.1.5.5.2");
		private static readonly Oid KERBEROS_OID = createOid("1.2.840.113554.1.2.2");

		private readonly string remoteServiceName;
		private readonly bool useCanonicalHostname;
		private readonly Optional<string> principal;
		private readonly Optional<File> keytab;
		private readonly Optional<File> credentialCache;

		private Session clientSession;

		public SpnegoHandler(string remoteServiceName, bool useCanonicalHostname, Optional<string> principal, Optional<File> kerberosConfig, Optional<File> keytab, Optional<File> credentialCache)
		{
			this.remoteServiceName = requireNonNull(remoteServiceName, "remoteServiceName is null");
			this.useCanonicalHostname = useCanonicalHostname;
			this.principal = requireNonNull(principal, "principal is null");
			this.keytab = requireNonNull(keytab, "keytab is null");
			this.credentialCache = requireNonNull(credentialCache, "credentialCache is null");

			kerberosConfig.ifPresent(file => System.setProperty("java.security.krb5.conf", file.AbsolutePath));
		}

		public override Response intercept(Chain chain)
		{
			// eagerly send authentication if possible
			try
			{
				return chain.proceed(authenticate(chain.request()));
			}
			catch (ClientException)
			{
				return chain.proceed(chain.request());
			}
		}

		public override Request authenticate(Route route, Response response)
		{
			// skip if we already tried or were not asked for Kerberos
			if (response.request().headers(AUTHORIZATION).Any(SpnegoHandler.isNegotiate) || response.headers(WWW_AUTHENTICATE).noneMatch(SpnegoHandler.isNegotiate))
			{
				return null;
			}

			return authenticate(response.request());
		}

		private static bool isNegotiate(string value)
		{
			return Splitter.on(whitespace()).Split(value).GetEnumerator().next().equalsIgnoreCase(NEGOTIATE);
		}

		private Request authenticate(Request request)
		{
			string hostName = request.url().host();
			string principal = makeServicePrincipal(remoteServiceName, hostName, useCanonicalHostname);
			byte[] token = generateToken(principal);

			string credential = format("%s %s", NEGOTIATE, Base64.Encoder.encodeToString(token));
			return request.newBuilder().header(AUTHORIZATION, credential).build();
		}

		private byte[] generateToken(string servicePrincipal)
		{
			GSSContext context = null;
			try
			{
				Session session = getSession();
				context = doAs(session.LoginContext.Subject, () =>
				{
				GSSContext result = GSS_MANAGER.createContext(GSS_MANAGER.createName(servicePrincipal, NT_HOSTBASED_SERVICE), SPNEGO_OID, session.ClientCredential, INDEFINITE_LIFETIME);
				result.requestMutualAuth(true);
				result.requestConf(true);
				result.requestInteg(true);
				result.requestCredDeleg(false);
				return result;
				});

				byte[] token = context.initSecContext(new byte[0], 0, 0);
				if (token == null)
				{
					throw new LoginException("No token generated from GSS context");
				}
				return token;
			}
			catch (Exception e) when (e is GSSException || e is LoginException)
			{
				throw new ClientException(format("Kerberos error for [%s]: %s", servicePrincipal, e.Message), e);
			}
			finally
			{
				try
				{
					if (context != null)
					{
						context.dispose();
					}
				}
				catch (GSSException)
				{
				}
			}
		}

		private Session getSession()
		{
			lock (this)
			{
				if ((clientSession == null) || clientSession.needsRefresh())
				{
					clientSession = createSession();
				}
				return clientSession;
			}
		}

		private Session createSession()
		{
			// TODO: do we need to call logout() on the LoginContext?

			LoginContext loginContext = new LoginContext("", null, null, new ConfigurationAnonymousInnerClass(this));

			loginContext.login();
			Subject subject = loginContext.Subject;
			Principal clientPrincipal = subject.Principals.GetEnumerator().next();
			GSSCredential clientCredential = doAs(subject, () => GSS_MANAGER.createCredential(GSS_MANAGER.createName(clientPrincipal.Name, NT_USER_NAME), DEFAULT_LIFETIME, KERBEROS_OID, INITIATE_ONLY));

			return new Session(loginContext, clientCredential);
		}

		public class ConfigurationAnonymousInnerClass : Configuration
		{
			private readonly SpnegoHandler outerInstance;

			public ConfigurationAnonymousInnerClass(SpnegoHandler outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override AppConfigurationEntry[] getAppConfigurationEntry(string name)
			{
				ImmutableMap.Builder<string, string> options = ImmutableMap.builder();
				options.put("refreshKrb5Config", "true");
				options.put("doNotPrompt", "true");
				options.put("useKeyTab", "true");

				if (getBoolean("presto.client.debugKerberos"))
				{
					options.put("debug", "true");
				}

				outerInstance.keytab.ifPresent(file => options.put("keyTab", file.AbsolutePath));

				outerInstance.credentialCache.ifPresent(file =>
				{
				options.put("ticketCache", file.AbsolutePath);
				options.put("useTicketCache", "true");
				options.put("renewTGT", "true");
				});

				outerInstance.principal.ifPresent(value => options.put("principal", value));
				return new AppConfigurationEntry[] {new AppConfigurationEntry(typeof(Krb5LoginModule).FullName, REQUIRED, options.build())};
			}
		}

		private static string makeServicePrincipal(string serviceName, string hostName, bool useCanonicalHostname)
		{
			string serviceHostName = hostName;
			if (useCanonicalHostname)
			{
				serviceHostName = canonicalizeServiceHostName(hostName);
			}
			return format("%s@%s", serviceName, serviceHostName.ToLower(Locale.US));
		}

		private static string canonicalizeServiceHostName(string hostName)
		{
			try
			{
				InetAddress address = InetAddress.getByName(hostName);
				string fullHostName;
				if ("localhost".Equals(address.HostName, StringComparison.OrdinalIgnoreCase))
				{
					fullHostName = InetAddress.LocalHost.CanonicalHostName;
				}
				else
				{
					fullHostName = address.CanonicalHostName;
				}
				if (fullHostName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
				{
					throw new ClientException("Fully qualified name of localhost should not resolve to 'localhost'. System configuration error?");
				}
				return fullHostName;
			}
			catch (UnknownHostException e)
			{
				throw new ClientException("Failed to resolve host: " + hostName, e);
			}
		}

		public interface GssSupplier<T>
		{
			T get();
		}

		private static T doAs<T>(Subject subject, GssSupplier<T> action)
		{
			try
			{
				return Subject.doAs(subject, (PrivilegedExceptionAction<T>) action.get);
			}
			catch (PrivilegedActionException e)
			{
				Exception t = e.InnerException;
				throwIfInstanceOf(t, typeof(GSSException));
				throwIfUnchecked(t);
				throw new Exception(t);
			}
		}

		private static Oid createOid(string value)
		{
			try
			{
				return new Oid(value);
			}
			catch (GSSException e)
			{
				throw new AssertionError(e);
			}
		}

		public class Session
		{
			internal readonly LoginContext loginContext;
			internal readonly GSSCredential clientCredential;

			public Session(LoginContext loginContext, GSSCredential clientCredential)
			{
				requireNonNull(loginContext, "loginContext is null");
				requireNonNull(clientCredential, "gssCredential is null");

				this.loginContext = loginContext;
				this.clientCredential = clientCredential;
			}

			public virtual LoginContext LoginContext
			{
				get
				{
					return loginContext;
				}
			}

			public virtual GSSCredential ClientCredential
			{
				get
				{
					return clientCredential;
				}
			}

			public virtual bool needsRefresh()
			{
				return clientCredential.RemainingLifetime < MIN_CREDENTIAL_LIFETIME.getValue(SECONDS);
			}
		}
	}
	*/

}