using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Akka.Util;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
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
namespace SharpPulsar.Presto
{
	public class StatementClientV1 : StatementClient
	{
		private static readonly MediaType MediaTypeText = MediaType.parse("text/plain; charset=utf-8");
		private static readonly JsonCodec<QueryResults> QueryResultsCodec = jsonCodec(typeof(QueryResults));

		private static readonly Splitter SessionHeaderSplitter = Splitter.on('=').limit(2).trimResults();
		private static readonly string UserAgentValue = typeof(StatementClientV1).Name + "/" + firstNonNull(typeof(StatementClientV1).Assembly.ImplementationVersion, "unknown");

		private readonly OkHttpClient _httpClient;
		public virtual Query {get;}
		private readonly AtomicReference<QueryResults> _currentResults = new AtomicReference<QueryResults>();
		private readonly AtomicReference<string> _setCatalog = new AtomicReference<string>();
		private readonly AtomicReference<string> _setSchema = new AtomicReference<string>();
		private readonly IDictionary<string, string> _setSessionProperties = new ConcurrentDictionary<string, string>();
		private readonly ISet<string> _resetSessionProperties = Sets.newConcurrentHashSet();
		private readonly IDictionary<string, SelectedRole> _setRoles = new ConcurrentDictionary<string, SelectedRole>();
		private readonly IDictionary<string, string> _addedPreparedStatements = new ConcurrentDictionary<string, string>();
		private readonly ISet<string> _deallocatedPreparedStatements = Sets.newConcurrentHashSet();
		private readonly AtomicReference<string> _startedTransactionId = new AtomicReference<string>();
		private readonly AtomicBoolean _clearTransactionId = new AtomicBoolean();
		public virtual TimeZone {get;}
		private readonly Duration _requestTimeoutNanos;
		private readonly string _user;

		private readonly AtomicReference<State> _state = new AtomicReference<State>(State.Running);

		public StatementClientV1(HttpClient httpClient, ClientSession session, string query)
		{
			requireNonNull(httpClient, "httpClient is null");
			requireNonNull(session, "session is null");
			requireNonNull(query, "query is null");

			this._httpClient = httpClient;
			this.TimeZone = session.TimeZone;
			this.Query = query;
			this._requestTimeoutNanos = session.ClientRequestTimeout;
			this._user = session.User;

			Request request = buildQueryRequest(session, query);

			JsonResponse<QueryResults> response = JsonResponse.execute(QueryResultsCodec, httpClient, request);
			if ((response.StatusCode != HTTP_OK) || !response.hasValue())
			{
				_state.compareAndSet(State.Running, State.ClientError);
				throw requestFailedException("starting query", request, response);
			}

			processResponse(response.Headers, response.Value);
		}

		private Request buildQueryRequest(ClientSession session, string query)
		{
			HttpUrl url = HttpUrl.get(session.Server);
			if (url == null)
			{
				throw new ClientException("Invalid server URL: " + session.Server);
			}
			url = url.newBuilder().encodedPath("/v1/statement").build();

			Request.Builder builder = prepareRequest(url).post(RequestBody.create(MediaTypeText, query));

			if (!string.ReferenceEquals(session.Source, null))
			{
				builder.addHeader(PRESTO_SOURCE, session.Source);
			}

			session.TraceToken.ifPresent(token => builder.addHeader(PRESTO_TRACE_TOKEN, token));

			if (session.ClientTags != null && session.ClientTags.Count > 0)
			{
				builder.addHeader(PRESTO_CLIENT_TAGS, Joiner.on(",").join(session.ClientTags));
			}
			if (!string.ReferenceEquals(session.ClientInfo, null))
			{
				builder.addHeader(PRESTO_CLIENT_INFO, session.ClientInfo);
			}
			if (!string.ReferenceEquals(session.Catalog, null))
			{
				builder.addHeader(PRESTO_CATALOG, session.Catalog);
			}
			if (!string.ReferenceEquals(session.Schema, null))
			{
				builder.addHeader(PRESTO_SCHEMA, session.Schema);
			}
			builder.addHeader(PRESTO_TIME_ZONE, session.TimeZone.Id);
			if (session.Locale != null)
			{
				builder.addHeader(PRESTO_LANGUAGE, session.Locale.toLanguageTag());
			}

			IDictionary<string, string> property = session.Properties;
			foreach (KeyValuePair<string, string> entry in property.SetOfKeyValuePairs())
			{
				builder.addHeader(PRESTO_SESSION, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, string> resourceEstimates = session.ResourceEstimates;
			foreach (KeyValuePair<string, string> entry in resourceEstimates.SetOfKeyValuePairs())
			{
				builder.addHeader(PRESTO_RESOURCE_ESTIMATE, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, SelectedRole> roles = session.Roles;
			foreach (KeyValuePair<string, SelectedRole> entry in roles.SetOfKeyValuePairs())
			{
				builder.addHeader(PrestoHeaders.PrestoRole, entry.Key + '=' + urlEncode(entry.Value.ToString()));
			}

			IDictionary<string, string> extraCredentials = session.ExtraCredentials;
			foreach (KeyValuePair<string, string> entry in extraCredentials.SetOfKeyValuePairs())
			{
				builder.addHeader(PRESTO_EXTRA_CREDENTIAL, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, string> statements = session.PreparedStatements;
			foreach (KeyValuePair<string, string> entry in statements.SetOfKeyValuePairs())
			{
				builder.addHeader(PRESTO_PREPARED_STATEMENT, urlEncode(entry.Key) + "=" + urlEncode(entry.Value));
			}

			builder.addHeader(PRESTO_TRANSACTION_ID, string.ReferenceEquals(session.TransactionId, null) ? "NONE" : session.TransactionId);

			return builder.build();
		}



		public virtual bool Running
		{
			get
			{
				return _state.get() == State.Running;
			}
		}

		public virtual bool ClientAborted
		{
			get
			{
				return _state.get() == State.ClientAborted;
			}
		}

		public virtual bool ClientError
		{
			get
			{
				return _state.get() == State.ClientError;
			}
		}

		public virtual bool Finished
		{
			get
			{
				return _state.get() == State.Finished;
			}
		}

		public virtual StatementStats Stats
		{
			get
			{
				return _currentResults.get().Stats;
			}
		}

		public virtual QueryStatusInfo currentStatusInfo()
		{
			checkState(Running, "current position is not valid (cursor past end)");
			return _currentResults.get();
		}

		public virtual QueryData currentData()
		{
			checkState(Running, "current position is not valid (cursor past end)");
			return _currentResults.get();
		}

		public virtual QueryStatusInfo finalStatusInfo()
		{
			checkState(!Running, "current position is still valid");
			return _currentResults.get();
		}

		public virtual Optional<string> SetCatalog
		{
			get
			{
				return Optional.ofNullable(_setCatalog.get());
			}
		}

		public virtual Optional<string> SetSchema
		{
			get
			{
				return Optional.ofNullable(_setSchema.get());
			}
		}

		public virtual IDictionary<string, string> SetSessionProperties
		{
			get
			{
				return ImmutableMap.copyOf(_setSessionProperties);
			}
		}

		public virtual ISet<string> ResetSessionProperties
		{
			get
			{
				return ImmutableSet.copyOf(_resetSessionProperties);
			}
		}

		public virtual IDictionary<string, SelectedRole> SetRoles
		{
			get
			{
				return ImmutableMap.copyOf(_setRoles);
			}
		}

		public virtual IDictionary<string, string> AddedPreparedStatements
		{
			get
			{
				return ImmutableMap.copyOf(_addedPreparedStatements);
			}
		}

		public virtual ISet<string> DeallocatedPreparedStatements
		{
			get
			{
				return ImmutableSet.copyOf(_deallocatedPreparedStatements);
			}
		}

		public virtual string StartedTransactionId
		{
			get
			{
				return _startedTransactionId.get();
			}
		}

		public virtual bool ClearTransactionId
		{
			get
			{
				return _clearTransactionId.get();
			}
		}

		private Request.Builder PrepareRequest(HttpUrl url)
		{
			return (new Request.Builder()).addHeader(PRESTO_USER, _user).addHeader(USER_AGENT, UserAgentValue).url(url);
		}

		public virtual bool Advance()
		{
			if (!Running)
			{
				return false;
			}

			URI nextUri = currentStatusInfo().NextUri;
			if (nextUri == null)
			{
				_state.compareAndSet(State.Running, State.Finished);
				return false;
			}

			Request request = prepareRequest(HttpUrl.get(nextUri)).build();

			Exception cause = null;
			long start = System.nanoTime();
			long attempts = 0;

			while (true)
			{
				if (ClientAborted)
				{
					return false;
				}

				Duration sinceStart = Duration.nanosSince(start);
				if (attempts > 0 && sinceStart.compareTo(_requestTimeoutNanos) > 0)
				{
					_state.compareAndSet(State.Running, State.ClientError);
					throw new Exception(format("Error fetching next (attempts: %s, duration: %s)", attempts, sinceStart), cause);
				}

				if (attempts > 0)
				{
					// back-off on retry
					try
					{
						MILLISECONDS.sleep(attempts * 100);
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
						_state.compareAndSet(State.Running, State.ClientError);
						throw new Exception("StatementClient thread was interrupted");
					}
				}
				attempts++;

				JsonResponse<QueryResults> response;
				try
				{
					response = JsonResponse.execute(QueryResultsCodec, _httpClient, request);
				}
				catch (Exception e)
				{
					cause = e;
					continue;
				}

				if ((response.StatusCode == HTTP_OK) && response.hasValue())
				{
					processResponse(response.Headers, response.Value);
					return true;
				}

				if (response.StatusCode != HTTP_UNAVAILABLE)
				{
					_state.compareAndSet(State.Running, State.ClientError);
					throw requestFailedException("fetching next", request, response);
				}
			}
		}

		private void ProcessResponse(Headers headers, QueryResults results)
		{
			_setCatalog.set(headers.get(PRESTO_SET_CATALOG));
			_setSchema.set(headers.get(PRESTO_SET_SCHEMA));

			foreach (string setSession in headers.values(PRESTO_SET_SESSION))
			{
				IList<string> keyValue = SessionHeaderSplitter.splitToList(setSession);
				if (keyValue.Count != 2)
				{
					continue;
				}
				_setSessionProperties[keyValue[0]] = keyValue[1];
			}
			_resetSessionProperties.addAll(headers.values(PRESTO_CLEAR_SESSION));

			foreach (string setRole in headers.values(PRESTO_SET_ROLE))
			{
				IList<string> keyValue = SessionHeaderSplitter.splitToList(setRole);
				if (keyValue.Count != 2)
				{
					continue;
				}
				_setRoles[keyValue[0]] = SelectedRole.valueOf(urlDecode(keyValue[1]));
			}

			foreach (string entry in headers.values(PRESTO_ADDED_PREPARE))
			{
				IList<string> keyValue = SessionHeaderSplitter.splitToList(entry);
				if (keyValue.Count != 2)
				{
					continue;
				}
				_addedPreparedStatements[urlDecode(keyValue[0])] = urlDecode(keyValue[1]);
			}
			foreach (string entry in headers.values(PRESTO_DEALLOCATED_PREPARE))
			{
				_deallocatedPreparedStatements.Add(urlDecode(entry));
			}

			string startedTransactionId = headers.get(PRESTO_STARTED_TRANSACTION_ID);
			if (!string.ReferenceEquals(startedTransactionId, null))
			{
				this._startedTransactionId.set(startedTransactionId);
			}
			if (headers.get(PRESTO_CLEAR_TRANSACTION_ID) != null)
			{
				_clearTransactionId.set(true);
			}

			_currentResults.set(results);
		}

		private Exception RequestFailedException(string task, Request request, JsonResponse<QueryResults> response)
		{
			if (!response.hasValue())
			{
				if (response.StatusCode == HTTP_UNAUTHORIZED)
				{
					return new ClientException("Authentication failed" + Optional.ofNullable(response.StatusMessage).map(message => ": " + message).orElse(""));
				}
				return new Exception(format("Error %s at %s returned an invalid response: %s [Error: %s]", task, request.url(), response, response.ResponseBody), response.Exception);
			}
			return new Exception(format("Error %s at %s returned HTTP %s", task, request.url(), response.StatusCode));
		}

		public virtual void CancelLeafStage()
		{
			checkState(!ClientAborted, "client is closed");

			URI uri = currentStatusInfo().PartialCancelUri;
			if (uri != null)
			{
				httpDelete(uri);
			}
		}

		public virtual void Dispose()
		{
			// If the query is not done, abort the query.
			if (_state.compareAndSet(State.Running, State.ClientAborted))
			{
				URI uri = _currentResults.get().NextUri;
				if (uri != null)
				{
					httpDelete(uri);
				}
			}
		}

		private void httpDelete(URI uri)
		{
			Request request = prepareRequest(HttpUrl.get(uri)).delete().build();
			_httpClient.newCall(request).enqueue(new NullCallback());
		}

		private static string urlEncode(string value)
		{
			try
			{
				return URLEncoder.encode(value, "UTF-8");
			}
			catch (UnsupportedEncodingException e)
			{
				throw new AssertionError(e);
			}
		}

		private static string urlDecode(string value)
		{
			try
			{
				return URLDecoder.decode(value, "UTF-8");
			}
			catch (UnsupportedEncodingException e)
			{
				throw new AssertionError(e);
			}
		}

		public enum State
		{
			/// <summary>
			/// submitted to server, not in terminal state (including planning, queued, running, etc)
			/// </summary>
			Running,
			ClientError,
			ClientAborted,
			/// <summary>
			/// finished on remote Presto server (including failed and successfully completed)
			/// </summary>
			Finished,
		}
	}

}