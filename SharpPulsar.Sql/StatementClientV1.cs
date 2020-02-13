using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
	using JsonCodec = com.facebook.airlift.json.JsonCodec;
	using NullCallback = Com.Facebook.Presto.Client.OkHttpUtil.NullCallback;
	using SelectedRole = com.facebook.presto.spi.security.SelectedRole;
	using TimeZoneKey = com.facebook.presto.spi.type.TimeZoneKey;
	using Joiner = com.google.common.@base.Joiner;
	using Splitter = com.google.common.@base.Splitter;
	using ImmutableMap = com.google.common.collect.ImmutableMap;
	using ImmutableSet = com.google.common.collect.ImmutableSet;
	using Sets = com.google.common.collect.Sets;
	using Duration = io.airlift.units.Duration;
	using Headers = okhttp3.Headers;
	using HttpUrl = okhttp3.HttpUrl;
	using MediaType = okhttp3.MediaType;
	using OkHttpClient = okhttp3.OkHttpClient;
	using Request = okhttp3.Request;
	using RequestBody = okhttp3.RequestBody;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.airlift.json.JsonCodec.jsonCodec;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_ADDED_PREPARE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_CATALOG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_CLEAR_SESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_CLEAR_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_CLIENT_INFO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_CLIENT_TAGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_DEALLOCATED_PREPARE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_EXTRA_CREDENTIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_LANGUAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_PREPARED_STATEMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_RESOURCE_ESTIMATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SET_CATALOG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SET_ROLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SET_SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SET_SESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_SOURCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_STARTED_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_TIME_ZONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_TRACE_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_TRANSACTION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.facebook.presto.client.PrestoHeaders.PRESTO_USER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.firstNonNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.USER_AGENT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ThreadSafe class StatementClientV1 implements StatementClient
	public class StatementClientV1 : StatementClient
	{
		private static readonly MediaType MEDIA_TYPE_TEXT = MediaType.parse("text/plain; charset=utf-8");
		private static readonly JsonCodec<QueryResults> QUERY_RESULTS_CODEC = jsonCodec(typeof(QueryResults));

		private static readonly Splitter SESSION_HEADER_SPLITTER = Splitter.on('=').limit(2).trimResults();
		private static readonly string USER_AGENT_VALUE = typeof(StatementClientV1).Name + "/" + firstNonNull(typeof(StatementClientV1).Assembly.ImplementationVersion, "unknown");

		private readonly OkHttpClient httpClient;
		public virtual Query {get;}
		private readonly AtomicReference<QueryResults> currentResults = new AtomicReference<QueryResults>();
		private readonly AtomicReference<string> setCatalog = new AtomicReference<string>();
		private readonly AtomicReference<string> setSchema = new AtomicReference<string>();
		private readonly IDictionary<string, string> setSessionProperties = new ConcurrentDictionary<string, string>();
		private readonly ISet<string> resetSessionProperties = Sets.newConcurrentHashSet();
		private readonly IDictionary<string, SelectedRole> setRoles = new ConcurrentDictionary<string, SelectedRole>();
		private readonly IDictionary<string, string> addedPreparedStatements = new ConcurrentDictionary<string, string>();
		private readonly ISet<string> deallocatedPreparedStatements = Sets.newConcurrentHashSet();
		private readonly AtomicReference<string> startedTransactionId = new AtomicReference<string>();
		private readonly AtomicBoolean clearTransactionId = new AtomicBoolean();
		public virtual TimeZone {get;}
		private readonly Duration requestTimeoutNanos;
		private readonly string user;

		private readonly AtomicReference<State> state = new AtomicReference<State>(State.RUNNING);

		public StatementClientV1(OkHttpClient HttpClient, ClientSession Session, string Query)
		{
			requireNonNull(HttpClient, "httpClient is null");
			requireNonNull(Session, "session is null");
			requireNonNull(Query, "query is null");

			this.httpClient = HttpClient;
			this.TimeZone = Session.TimeZone;
			this.Query = Query;
			this.requestTimeoutNanos = Session.ClientRequestTimeout;
			this.user = Session.User;

			Request Request = BuildQueryRequest(Session, Query);

			JsonResponse<QueryResults> Response = JsonResponse.Execute(QUERY_RESULTS_CODEC, HttpClient, Request);
			if ((Response.StatusCode != HTTP_OK) || !Response.hasValue())
			{
				state.compareAndSet(State.RUNNING, State.ClientError);
				throw RequestFailedException("starting query", Request, Response);
			}

			ProcessResponse(Response.Headers, Response.Value);
		}

		private Request BuildQueryRequest(ClientSession Session, string Query)
		{
			HttpUrl Url = HttpUrl.get(Session.Server);
			if (Url == null)
			{
				throw new ClientException("Invalid server URL: " + Session.Server);
			}
			Url = Url.newBuilder().encodedPath("/v1/statement").build();

			Request.Builder Builder = PrepareRequest(Url).post(RequestBody.create(MEDIA_TYPE_TEXT, Query));

			if (!string.ReferenceEquals(Session.Source, null))
			{
				Builder.addHeader(PRESTO_SOURCE, Session.Source);
			}

			Session.TraceToken.ifPresent(token => Builder.addHeader(PRESTO_TRACE_TOKEN, token));

			if (Session.ClientTags != null && Session.ClientTags.Count > 0)
			{
				Builder.addHeader(PRESTO_CLIENT_TAGS, Joiner.on(",").join(Session.ClientTags));
			}
			if (!string.ReferenceEquals(Session.ClientInfo, null))
			{
				Builder.addHeader(PRESTO_CLIENT_INFO, Session.ClientInfo);
			}
			if (!string.ReferenceEquals(Session.Catalog, null))
			{
				Builder.addHeader(PRESTO_CATALOG, Session.Catalog);
			}
			if (!string.ReferenceEquals(Session.Schema, null))
			{
				Builder.addHeader(PRESTO_SCHEMA, Session.Schema);
			}
			Builder.addHeader(PRESTO_TIME_ZONE, Session.TimeZone.Id);
			if (Session.Locale != null)
			{
				Builder.addHeader(PRESTO_LANGUAGE, Session.Locale.toLanguageTag());
			}

			IDictionary<string, string> Property = Session.Properties;
			foreach (KeyValuePair<string, string> Entry in Property.SetOfKeyValuePairs())
			{
				Builder.addHeader(PRESTO_SESSION, Entry.Key + "=" + Entry.Value);
			}

			IDictionary<string, string> ResourceEstimates = Session.ResourceEstimates;
			foreach (KeyValuePair<string, string> Entry in ResourceEstimates.SetOfKeyValuePairs())
			{
				Builder.addHeader(PRESTO_RESOURCE_ESTIMATE, Entry.Key + "=" + Entry.Value);
			}

			IDictionary<string, SelectedRole> Roles = Session.Roles;
			foreach (KeyValuePair<string, SelectedRole> Entry in Roles.SetOfKeyValuePairs())
			{
				Builder.addHeader(PrestoHeaders.PrestoRole, Entry.Key + '=' + UrlEncode(Entry.Value.ToString()));
			}

			IDictionary<string, string> ExtraCredentials = Session.ExtraCredentials;
			foreach (KeyValuePair<string, string> Entry in ExtraCredentials.SetOfKeyValuePairs())
			{
				Builder.addHeader(PRESTO_EXTRA_CREDENTIAL, Entry.Key + "=" + Entry.Value);
			}

			IDictionary<string, string> Statements = Session.PreparedStatements;
			foreach (KeyValuePair<string, string> Entry in Statements.SetOfKeyValuePairs())
			{
				Builder.addHeader(PRESTO_PREPARED_STATEMENT, UrlEncode(Entry.Key) + "=" + UrlEncode(Entry.Value));
			}

			Builder.addHeader(PRESTO_TRANSACTION_ID, string.ReferenceEquals(Session.TransactionId, null) ? "NONE" : Session.TransactionId);

			return Builder.build();
		}



		public virtual bool Running
		{
			get
			{
				return state.get() == State.RUNNING;
			}
		}

		public virtual bool ClientAborted
		{
			get
			{
				return state.get() == State.ClientAborted;
			}
		}

		public virtual bool ClientError
		{
			get
			{
				return state.get() == State.ClientError;
			}
		}

		public virtual bool Finished
		{
			get
			{
				return state.get() == State.FINISHED;
			}
		}

		public virtual StatementStats Stats
		{
			get
			{
				return currentResults.get().Stats;
			}
		}

		public override QueryStatusInfo CurrentStatusInfo()
		{
			checkState(Running, "current position is not valid (cursor past end)");
			return currentResults.get();
		}

		public override QueryData CurrentData()
		{
			checkState(Running, "current position is not valid (cursor past end)");
			return currentResults.get();
		}

		public override QueryStatusInfo FinalStatusInfo()
		{
			checkState(!Running, "current position is still valid");
			return currentResults.get();
		}

		public virtual Optional<string> SetCatalog
		{
			get
			{
				return Optional.ofNullable(setCatalog.get());
			}
		}

		public virtual Optional<string> SetSchema
		{
			get
			{
				return Optional.ofNullable(setSchema.get());
			}
		}

		public virtual IDictionary<string, string> SetSessionProperties
		{
			get
			{
				return ImmutableMap.copyOf(setSessionProperties);
			}
		}

		public virtual ISet<string> ResetSessionProperties
		{
			get
			{
				return ImmutableSet.copyOf(resetSessionProperties);
			}
		}

		public virtual IDictionary<string, SelectedRole> SetRoles
		{
			get
			{
				return ImmutableMap.copyOf(setRoles);
			}
		}

		public virtual IDictionary<string, string> AddedPreparedStatements
		{
			get
			{
				return ImmutableMap.copyOf(addedPreparedStatements);
			}
		}

		public virtual ISet<string> DeallocatedPreparedStatements
		{
			get
			{
				return ImmutableSet.copyOf(deallocatedPreparedStatements);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nullable public String getStartedTransactionId()
		public virtual string StartedTransactionId
		{
			get
			{
				return startedTransactionId.get();
			}
		}

		public virtual bool ClearTransactionId
		{
			get
			{
				return clearTransactionId.get();
			}
		}

		private Request.Builder PrepareRequest(HttpUrl Url)
		{
			return (new Request.Builder()).addHeader(PRESTO_USER, user).addHeader(USER_AGENT, USER_AGENT_VALUE).url(Url);
		}

		public override bool Advance()
		{
			if (!Running)
			{
				return false;
			}

			URI NextUri = CurrentStatusInfo().NextUri;
			if (NextUri == null)
			{
				state.compareAndSet(State.RUNNING, State.FINISHED);
				return false;
			}

			Request Request = PrepareRequest(HttpUrl.get(NextUri)).build();

			Exception Cause = null;
			long Start = System.nanoTime();
			long Attempts = 0;

			while (true)
			{
				if (ClientAborted)
				{
					return false;
				}

				Duration SinceStart = Duration.nanosSince(Start);
				if (Attempts > 0 && SinceStart.compareTo(requestTimeoutNanos) > 0)
				{
					state.compareAndSet(State.RUNNING, State.ClientError);
					throw new Exception(format("Error fetching next (attempts: %s, duration: %s)", Attempts, SinceStart), Cause);
				}

				if (Attempts > 0)
				{
					// back-off on retry
					try
					{
						MILLISECONDS.sleep(Attempts * 100);
					}
					catch (InterruptedException)
					{
						try
						{
							Dispose();
						}
						finally
						{
							Thread.CurrentThread.Interrupt();
						}
						state.compareAndSet(State.RUNNING, State.ClientError);
						throw new Exception("StatementClient thread was interrupted");
					}
				}
				Attempts++;

				JsonResponse<QueryResults> Response;
				try
				{
					Response = JsonResponse.Execute(QUERY_RESULTS_CODEC, httpClient, Request);
				}
				catch (Exception E)
				{
					Cause = E;
					continue;
				}

				if ((Response.StatusCode == HTTP_OK) && Response.hasValue())
				{
					ProcessResponse(Response.Headers, Response.Value);
					return true;
				}

				if (Response.StatusCode != HTTP_UNAVAILABLE)
				{
					state.compareAndSet(State.RUNNING, State.ClientError);
					throw RequestFailedException("fetching next", Request, Response);
				}
			}
		}

		private void ProcessResponse(Headers Headers, QueryResults Results)
		{
			setCatalog.set(Headers.get(PRESTO_SET_CATALOG));
			setSchema.set(Headers.get(PRESTO_SET_SCHEMA));

			foreach (string SetSession in Headers.values(PRESTO_SET_SESSION))
			{
				IList<string> KeyValue = SESSION_HEADER_SPLITTER.splitToList(SetSession);
				if (KeyValue.Count != 2)
				{
					continue;
				}
				setSessionProperties[KeyValue[0]] = KeyValue[1];
			}
			resetSessionProperties.addAll(Headers.values(PRESTO_CLEAR_SESSION));

			foreach (string SetRole in Headers.values(PRESTO_SET_ROLE))
			{
				IList<string> KeyValue = SESSION_HEADER_SPLITTER.splitToList(SetRole);
				if (KeyValue.Count != 2)
				{
					continue;
				}
				setRoles[KeyValue[0]] = SelectedRole.valueOf(UrlDecode(KeyValue[1]));
			}

			foreach (string Entry in Headers.values(PRESTO_ADDED_PREPARE))
			{
				IList<string> KeyValue = SESSION_HEADER_SPLITTER.splitToList(Entry);
				if (KeyValue.Count != 2)
				{
					continue;
				}
				addedPreparedStatements[UrlDecode(KeyValue[0])] = UrlDecode(KeyValue[1]);
			}
			foreach (string Entry in Headers.values(PRESTO_DEALLOCATED_PREPARE))
			{
				deallocatedPreparedStatements.Add(UrlDecode(Entry));
			}

			string StartedTransactionId = Headers.get(PRESTO_STARTED_TRANSACTION_ID);
			if (!string.ReferenceEquals(StartedTransactionId, null))
			{
				this.startedTransactionId.set(StartedTransactionId);
			}
			if (Headers.get(PRESTO_CLEAR_TRANSACTION_ID) != null)
			{
				clearTransactionId.set(true);
			}

			currentResults.set(Results);
		}

		private Exception RequestFailedException(string Task, Request Request, JsonResponse<QueryResults> Response)
		{
			if (!Response.hasValue())
			{
				if (Response.StatusCode == HTTP_UNAUTHORIZED)
				{
					return new ClientException("Authentication failed" + Optional.ofNullable(Response.StatusMessage).map(message => ": " + message).orElse(""));
				}
				return new Exception(format("Error %s at %s returned an invalid response: %s [Error: %s]", Task, Request.url(), Response, Response.ResponseBody), Response.Exception);
			}
			return new Exception(format("Error %s at %s returned HTTP %s", Task, Request.url(), Response.StatusCode));
		}

		public override void CancelLeafStage()
		{
			checkState(!ClientAborted, "client is closed");

			URI Uri = CurrentStatusInfo().PartialCancelUri;
			if (Uri != null)
			{
				HttpDelete(Uri);
			}
		}

		public override void Close()
		{
			// If the query is not done, abort the query.
			if (state.compareAndSet(State.RUNNING, State.ClientAborted))
			{
				URI Uri = currentResults.get().NextUri;
				if (Uri != null)
				{
					HttpDelete(Uri);
				}
			}
		}

		private void HttpDelete(URI Uri)
		{
			Request Request = PrepareRequest(HttpUrl.get(Uri)).delete().build();
			httpClient.newCall(Request).enqueue(new NullCallback());
		}

		private static string UrlEncode(string Value)
		{
			try
			{
				return URLEncoder.encode(Value, "UTF-8");
			}
			catch (UnsupportedEncodingException E)
			{
				throw new AssertionError(E);
			}
		}

		private static string UrlDecode(string Value)
		{
			try
			{
				return URLDecoder.decode(Value, "UTF-8");
			}
			catch (UnsupportedEncodingException E)
			{
				throw new AssertionError(E);
			}
		}

		public enum State
		{
			/// <summary>
			/// submitted to server, not in terminal state (including planning, queued, running, etc)
			/// </summary>
			RUNNING,
			ClientError,
			ClientAborted,
			/// <summary>
			/// finished on remote Presto server (including failed and successfully completed)
			/// </summary>
			FINISHED,
		}
	}

}