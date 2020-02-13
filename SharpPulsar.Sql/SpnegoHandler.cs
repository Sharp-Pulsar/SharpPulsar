using System;

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
	using Splitter = com.google.common.@base.Splitter;
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



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.CharMatcher.whitespace;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Throwables.throwIfInstanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Throwables.throwIfUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.AUTHORIZATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.WWW_AUTHENTICATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Boolean.getBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.ietf.jgss.GSSContext.INDEFINITE_LIFETIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.ietf.jgss.GSSCredential.DEFAULT_LIFETIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.ietf.jgss.GSSCredential.INITIATE_ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.ietf.jgss.GSSName.NT_HOSTBASED_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.ietf.jgss.GSSName.NT_USER_NAME;

	// TODO: This class is similar to SpnegoAuthentication in Airlift. Consider extracting a library.
	public class SpnegoHandler : Interceptor, Authenticator
	{
		private const string NEGOTIATE = "Negotiate";
		private static readonly Duration MIN_CREDENTIAL_LIFETIME = new Duration(60, SECONDS);

		private static readonly GSSManager GSS_MANAGER = GSSManager.Instance;

		private static readonly Oid SPNEGO_OID = CreateOid("1.3.6.1.5.5.2");
		private static readonly Oid KERBEROS_OID = CreateOid("1.2.840.113554.1.2.2");

		private readonly string remoteServiceName;
		private readonly bool useCanonicalHostname;
		private readonly Optional<string> principal;
		private readonly Optional<File> keytab;
		private readonly Optional<File> credentialCache;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GuardedBy("this") private Session clientSession;
		private Session clientSession;

		public SpnegoHandler(string RemoteServiceName, bool UseCanonicalHostname, Optional<string> Principal, Optional<File> KerberosConfig, Optional<File> Keytab, Optional<File> CredentialCache)
		{
			this.remoteServiceName = requireNonNull(RemoteServiceName, "remoteServiceName is null");
			this.useCanonicalHostname = UseCanonicalHostname;
			this.principal = requireNonNull(Principal, "principal is null");
			this.keytab = requireNonNull(Keytab, "keytab is null");
			this.credentialCache = requireNonNull(CredentialCache, "credentialCache is null");

			KerberosConfig.ifPresent(file => System.setProperty("java.security.krb5.conf", file.AbsolutePath));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public okhttp3.Response intercept(Chain chain) throws java.io.IOException
		public override Response Intercept(Chain Chain)
		{
			// eagerly send authentication if possible
			try
			{
				return Chain.proceed(Authenticate(Chain.request()));
			}
			catch (ClientException)
			{
				return Chain.proceed(Chain.request());
			}
		}

		public override Request Authenticate(Route Route, Response Response)
		{
			// skip if we already tried or were not asked for Kerberos
			if (Response.request().headers(AUTHORIZATION).Any(SpnegoHandler.isNegotiate) || Response.headers(WWW_AUTHENTICATE).noneMatch(SpnegoHandler.isNegotiate))
			{
				return null;
			}

			return Authenticate(Response.request());
		}

		private static bool IsNegotiate(string Value)
		{
			return Splitter.on(whitespace()).Split(Value).GetEnumerator().next().equalsIgnoreCase(NEGOTIATE);
		}

		private Request Authenticate(Request Request)
		{
			string HostName = Request.url().host();
			string Principal = MakeServicePrincipal(remoteServiceName, HostName, useCanonicalHostname);
			sbyte[] Token = GenerateToken(Principal);

			string Credential = format("%s %s", NEGOTIATE, Base64.Encoder.encodeToString(Token));
			return Request.newBuilder().header(AUTHORIZATION, Credential).build();
		}

		private sbyte[] GenerateToken(string ServicePrincipal)
		{
			GSSContext Context = null;
			try
			{
				Session Session = GetSession();
				Context = DoAs(Session.LoginContext.Subject, () =>
				{
				GSSContext Result = GSS_MANAGER.createContext(GSS_MANAGER.createName(ServicePrincipal, NT_HOSTBASED_SERVICE), SPNEGO_OID, Session.ClientCredential, INDEFINITE_LIFETIME);
				Result.requestMutualAuth(true);
				Result.requestConf(true);
				Result.requestInteg(true);
				Result.requestCredDeleg(false);
				return Result;
				});

				sbyte[] Token = Context.initSecContext(new sbyte[0], 0, 0);
				if (Token == null)
				{
					throw new LoginException("No token generated from GSS context");
				}
				return Token;
			}
			catch (Exception e) when (e is GSSException || e is LoginException)
			{
				throw new ClientException(format("Kerberos error for [%s]: %s", ServicePrincipal, e.Message), e);
			}
			finally
			{
				try
				{
					if (Context != null)
					{
						Context.dispose();
					}
				}
				catch (GSSException)
				{
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized Session getSession() throws javax.security.auth.login.LoginException, org.ietf.jgss.GSSException
		private Session GetSession()
		{
			lock (this)
			{
				if ((clientSession == null) || clientSession.NeedsRefresh())
				{
					clientSession = CreateSession();
				}
				return clientSession;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Session createSession() throws javax.security.auth.login.LoginException, org.ietf.jgss.GSSException
		private Session CreateSession()
		{
			// TODO: do we need to call logout() on the LoginContext?

			LoginContext LoginContext = new LoginContext("", null, null, new ConfigurationAnonymousInnerClass(this));

			LoginContext.login();
			Subject Subject = LoginContext.Subject;
			Principal ClientPrincipal = Subject.Principals.GetEnumerator().next();
			GSSCredential ClientCredential = DoAs(Subject, () => GSS_MANAGER.createCredential(GSS_MANAGER.createName(ClientPrincipal.Name, NT_USER_NAME), DEFAULT_LIFETIME, KERBEROS_OID, INITIATE_ONLY));

			return new Session(LoginContext, ClientCredential);
		}

		public class ConfigurationAnonymousInnerClass : Configuration
		{
			private readonly SpnegoHandler outerInstance;

			public ConfigurationAnonymousInnerClass(SpnegoHandler OuterInstance)
			{
				this.outerInstance = OuterInstance;
			}

			public override AppConfigurationEntry[] getAppConfigurationEntry(string Name)
			{
				ImmutableMap.Builder<string, string> Options = ImmutableMap.builder();
				Options.put("refreshKrb5Config", "true");
				Options.put("doNotPrompt", "true");
				Options.put("useKeyTab", "true");

				if (getBoolean("presto.client.debugKerberos"))
				{
					Options.put("debug", "true");
				}

				outerInstance.keytab.ifPresent(file => Options.put("keyTab", file.AbsolutePath));

				outerInstance.credentialCache.ifPresent(file =>
				{
				Options.put("ticketCache", file.AbsolutePath);
				Options.put("useTicketCache", "true");
				Options.put("renewTGT", "true");
				});

				outerInstance.principal.ifPresent(value => Options.put("principal", value));

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				return new AppConfigurationEntry[] {new AppConfigurationEntry(typeof(Krb5LoginModule).FullName, REQUIRED, Options.build())};
			}
		}

		private static string MakeServicePrincipal(string ServiceName, string HostName, bool UseCanonicalHostname)
		{
			string ServiceHostName = HostName;
			if (UseCanonicalHostname)
			{
				ServiceHostName = CanonicalizeServiceHostName(HostName);
			}
			return format("%s@%s", ServiceName, ServiceHostName.ToLower(Locale.US));
		}

		private static string CanonicalizeServiceHostName(string HostName)
		{
			try
			{
				InetAddress Address = InetAddress.getByName(HostName);
				string FullHostName;
				if ("localhost".Equals(Address.HostName, StringComparison.OrdinalIgnoreCase))
				{
					FullHostName = InetAddress.LocalHost.CanonicalHostName;
				}
				else
				{
					FullHostName = Address.CanonicalHostName;
				}
				if (FullHostName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
				{
					throw new ClientException("Fully qualified name of localhost should not resolve to 'localhost'. System configuration error?");
				}
				return FullHostName;
			}
			catch (UnknownHostException E)
			{
				throw new ClientException("Failed to resolve host: " + HostName, E);
			}
		}

		public interface GssSupplier<T>
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T get() throws org.ietf.jgss.GSSException;
			T Get();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T> T doAs(javax.security.auth.Subject subject, GssSupplier<T> action) throws org.ietf.jgss.GSSException
		private static T DoAs<T>(Subject Subject, GssSupplier<T> Action)
		{
			try
			{
				return Subject.doAs(Subject, (PrivilegedExceptionAction<T>) Action.get);
			}
			catch (PrivilegedActionException E)
			{
				Exception T = E.InnerException;
				throwIfInstanceOf(T, typeof(GSSException));
				throwIfUnchecked(T);
				throw new Exception(T);
			}
		}

		private static Oid CreateOid(string Value)
		{
			try
			{
				return new Oid(Value);
			}
			catch (GSSException E)
			{
				throw new AssertionError(E);
			}
		}

		public class Session
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal readonly LoginContext LoginContextConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal readonly GSSCredential ClientCredentialConflict;

			public Session(LoginContext LoginContext, GSSCredential ClientCredential)
			{
				requireNonNull(LoginContext, "loginContext is null");
				requireNonNull(ClientCredential, "gssCredential is null");

				this.LoginContextConflict = LoginContext;
				this.ClientCredentialConflict = ClientCredential;
			}

			public virtual LoginContext LoginContext
			{
				get
				{
					return LoginContextConflict;
				}
			}

			public virtual GSSCredential ClientCredential
			{
				get
				{
					return ClientCredentialConflict;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean needsRefresh() throws org.ietf.jgss.GSSException
			public virtual bool NeedsRefresh()
			{
				return ClientCredentialConflict.RemainingLifetime < MIN_CREDENTIAL_LIFETIME.getValue(SECONDS);
			}
		}
	}

}