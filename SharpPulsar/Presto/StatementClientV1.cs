using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web;
using Nito.AsyncEx;
using SharpPulsar.Precondition;
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
	public class StatementClientV1 : IStatementClient
	{
        private readonly string _userAgentValue;

		private readonly HttpClient _httpClient;
		public string Query { get; }
		private QueryResults _currentResults;

		public TimeZoneKey TimeZone { get; }
		private readonly long _requestTimeoutNanos;
		private readonly string _user;

		private State _state = State.Running;

		public StatementClientV1(HttpClient httpClient, ClientSession session, string query)
		{
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            _userAgentValue = typeof(StatementClientV1).Name + "/" + fvi;
			Condition.RequireNonNull(httpClient, "httpClient is null");
			Condition.RequireNonNull(session, "session is null");
			Condition.RequireNonNull(query, "query is null");

			_httpClient = httpClient;
			TimeZone = session.TimeZone;
			Query = query;
			_requestTimeoutNanos = session.ClientRequestTimeout;
			_user = session.User;

			var request = BuildQueryRequest(session, query);

			var responseTask = JsonResponse<QueryResults>.Execute(httpClient, request);
			var response = SynchronizationContextSwitcher.NoContext(async () => await responseTask).Result;
			if ((response.ResponseMessage.StatusCode != HttpStatusCode.Accepted) || !response.HasValue())
			{
				if (_state == State.Running)
					_state = State.ClientError;
				throw RequestFailedException("starting query", request, response);
			}

			ProcessResponse(response.Headers, response.Value);
		}

		private HttpRequestMessage BuildQueryRequest(ClientSession session, string query)
		{
			var url = new Uri($"{session.Server.TrimEnd('/')}/v1/statement");
			if (url == null)
			{
				throw new ClientException("Invalid server URL: " + session.Server);
			}
			var builder = PrepareRequest(HttpMethod.Post, url, query, "text/plain");

			if (!string.IsNullOrWhiteSpace(session.Source))
			{
				builder.Headers.Add(PrestoHeaders.PrestoSource, session.Source);
			}
			if (!string.IsNullOrWhiteSpace(session.TraceToken))
			{
				builder.Headers.Add(PrestoHeaders.PrestoTraceToken, session.TraceToken);
			}

			if (session.ClientTags != null && session.ClientTags.Count > 0)
			{
				builder.Headers.Add(PrestoHeaders.PrestoClientTags, string.Join(",", session.ClientTags));
			}
			if (!string.IsNullOrWhiteSpace(session.ClientInfo))
			{
				builder.Headers.Add(PrestoHeaders.PrestoClientInfo, session.ClientInfo);
			}
			if (!ReferenceEquals(session.Catalog, null))
			{
				builder.Headers.Add(PrestoHeaders.PrestoCatalog, session.Catalog);
			}
			if (!ReferenceEquals(session.Schema, null))
			{
				builder.Headers.Add(PrestoHeaders.PrestoSchema, session.Schema);
			}
			builder.Headers.Add(PrestoHeaders.PrestoTimeZone, session.TimeZone.Id);
			if (session.Locale != null)
			{
				builder.Headers.Add(PrestoHeaders.PrestoLanguage, session.Locale.Name);
			}

			IDictionary<string, string> property = session.Properties;
			foreach (KeyValuePair<string, string> entry in property.SetOfKeyValuePairs())
			{
				builder.Headers.Add(PrestoHeaders.PrestoSession, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, string> resourceEstimates = session.ResourceEstimates;
			foreach (KeyValuePair<string, string> entry in resourceEstimates.SetOfKeyValuePairs())
			{
				builder.Headers.Add(PrestoHeaders.PrestoResourceEstimate, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, SelectedRole> roles = session.Roles;
			foreach (KeyValuePair<string, SelectedRole> entry in roles.SetOfKeyValuePairs())
			{
				builder.Headers.Add(PrestoHeaders.PrestoRole, entry.Key + '=' + UrlEncode(entry.Value.ToString()));
			}

			IDictionary<string, string> extraCredentials = session.ExtraCredentials;
			foreach (KeyValuePair<string, string> entry in extraCredentials.SetOfKeyValuePairs())
			{
				builder.Headers.Add(PrestoHeaders.PrestoExtraCredential, entry.Key + "=" + entry.Value);
			}

			IDictionary<string, string> statements = session.PreparedStatements;
			foreach (KeyValuePair<string, string> entry in statements.SetOfKeyValuePairs())
			{
				builder.Headers.Add(PrestoHeaders.PrestoPreparedStatement, UrlEncode(entry.Key) + "=" + UrlEncode(entry.Value));
			}

			builder.Headers.Add(PrestoHeaders.PrestoTransactionId, string.IsNullOrWhiteSpace(session.TransactionId) ? "NONE" : session.TransactionId);

			return builder;
		}



		public bool Running => _state == State.Running;

		public bool ClientAborted => _state == State.ClientAborted;

		public bool ClientError => _state == State.ClientError;

		public bool Finished => _state == State.Finished;

		public StatementStats Stats => _currentResults.Stats;

		public QueryStatusInfo CurrentStatusInfo()
		{
			Condition.CheckArgument(Running, "current position is not valid (cursor past end)");
			return _currentResults;
		}

		public IQueryData CurrentData()
		{
			Condition.CheckArgument(Running, "current position is not valid (cursor past end)");
			return _currentResults;
		}

		public QueryStatusInfo FinalStatusInfo()
		{
			Condition.CheckArgument(!Running, "current position is still valid");
			return _currentResults;
		}

		public string SetCatalog { get; set; }

        public string SetSchema { get; set; }

        public IDictionary<string, string> SetSessionProperties { get; set; } = new ConcurrentDictionary<string, string>();

        public ISet<string> ResetSessionProperties { get; set; } = new HashSet<string>();

        public IDictionary<string, SelectedRole> SetRoles { get; set; } = new ConcurrentDictionary<string, SelectedRole>();

        public IDictionary<string, string> AddedPreparedStatements { get; set; } = new ConcurrentDictionary<string, string>();

        public ISet<string> DeallocatedPreparedStatements { get; set; } = new HashSet<string>();

        public string StartedTransactionId { get; private set; }

        public bool ClearTransactionId { get; private set; }

        private HttpRequestMessage PrepareRequest(HttpMethod mode, Uri uri, string data = "", string mediaType = "")
		{
			var request = new HttpRequestMessage(mode, uri);
			request.Headers.Add(PrestoHeaders.PrestoUser, _user);
			request.Headers.Add("User-Agent", _userAgentValue);
			if (!string.IsNullOrWhiteSpace(data))
				request.Content = new StringContent(data, Encoding.UTF8, mediaType);
			return request;
		}

		public bool Advance()
		{
			if (!Running)
			{
				return false;
			}

			var nextUri = CurrentStatusInfo().NextUri;
			if (nextUri == null)
			{
				_state = State.Finished;
				return false;
			}

			var request = PrepareRequest(HttpMethod.Get, nextUri);

			Exception cause = null;
			var start = DateTime.Now;
			long attempts = 0;

			while (true)
			{
				if (ClientAborted)
				{
					return false;
				}

				var sinceStart = (DateTime.Now - start).Ticks;
				if (attempts > 0 && sinceStart.CompareTo(_requestTimeoutNanos) > 0)
				{
					_state = State.ClientError;
					throw new Exception($"Error fetching next (attempts:{attempts}, duration: {sinceStart})", cause);
				}

				if (attempts > 0)
				{
					// back-off on retry
					try
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(attempts * 100));
					}
					catch (Exception)
					{
						try
						{
							Dispose();
						}
						finally
						{
							Thread.CurrentThread.Interrupt();
						}
						_state = State.ClientError;
						throw new Exception("StatementClient thread was interrupted");
					}
				}
				attempts++;

				JsonResponse<QueryResults> response;
				try
				{
					var responseTask = JsonResponse<QueryResults>.Execute(_httpClient, request);
					response = SynchronizationContextSwitcher.NoContext(async () => await responseTask).Result;
				}
				catch (Exception e)
				{
					cause = e;
					continue;
				}

				if ((response.ResponseMessage.StatusCode == HttpStatusCode.Accepted) && response.HasValue())
				{
					ProcessResponse(response.Headers, response.Value);
					return true;
				}

				if (response.ResponseMessage.StatusCode != HttpStatusCode.ServiceUnavailable)
				{
					if (_state == State.Running)
						_state = State.ClientError;
					throw RequestFailedException("fetching next", request, response);
				}
			}
		}

		private void ProcessResponse(HttpResponseHeaders headers, QueryResults results)
		{
			SetCatalog = headers.GetValues(PrestoHeaders.PrestoSetCatalog).First();
			SetSchema = headers.GetValues(PrestoHeaders.PrestoSetSchema).First();
			var sessions = headers.GetValues(PrestoHeaders.PrestoSetSession);
			foreach (string setSession in sessions)
			{
				IList<string> keyValue = setSession.Split('=').Take(2).Select(x => x.Trim()).ToList();
				if (keyValue.Count != 2)
				{
					continue;
				}
				SetSessionProperties[keyValue[0]] = keyValue[1];
			}
			headers.GetValues(PrestoHeaders.PrestoClearSession).ToList().ForEach(x => ResetSessionProperties.Add(x));
			var roles = headers.GetValues(PrestoHeaders.PrestoSetRole);
			foreach (string setRole in roles)
			{
				IList<string> keyValue = setRole.Split('=').Take(2).Select(x => x.Trim()).ToList();
				if (keyValue.Count != 2)
				{
					continue;
				}
				SetRoles[keyValue[0]] = SelectedRole.ValueOf(UrlDecode(keyValue[1]));
			}
			var prepares = headers.GetValues(PrestoHeaders.PrestoAddedPrepare);
			foreach (string entry in prepares)
			{
				IList<string> keyValue = entry.Split('=').Take(2).Select(x => x.Trim()).ToList();
				if (keyValue.Count != 2)
				{
					continue;
				}
				AddedPreparedStatements[UrlDecode(keyValue[0])] = UrlDecode(keyValue[1]);
			}
			var deAllocs = headers.GetValues(PrestoHeaders.PrestoDeallocatedPrepare);
			foreach (string entry in deAllocs)
			{
				DeallocatedPreparedStatements.Add(UrlDecode(entry));
			}

			string startedTransactionId = headers.GetValues(PrestoHeaders.PrestoStartedTransactionId).First();
			if (!string.IsNullOrWhiteSpace(startedTransactionId))
			{
				StartedTransactionId = startedTransactionId;
			}
			string clearedTransactionId = headers.GetValues(PrestoHeaders.PrestoClearTransactionId).First();
			if (clearedTransactionId != null)
			{
				ClearTransactionId = true;
			}

			_currentResults = results;
		}

		private Exception RequestFailedException(string task, HttpRequestMessage request, JsonResponse<QueryResults> response)
		{
			if (!response.HasValue())
			{
				if (response.ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
				{
					return new ClientException("Authentication failed: " + response.ResponseMessage.ReasonPhrase);
				}
				return new Exception($"Error [{task}] at [{response.ResponseMessage.RequestMessage.RequestUri}] returned an invalid response: [{response.ResponseBody}] [Error: {response.Exception}]");
			}
			return new Exception($"Error [{task}] at [{response.ResponseMessage.RequestMessage.RequestUri}] returned HTTP [{response.ResponseMessage.StatusCode}]");
		}

		public void CancelLeafStage()
		{
			Condition.CheckArgument(!ClientAborted, "client is closed");

			var uri = CurrentStatusInfo().PartialCancelUri;
			if (uri != null)
			{
				HttpDelete(uri);
			}
		}

		public void Dispose()
		{
			// If the query is not done, abort the query.
			if (_state == State.Running)
			{
				var uri = _currentResults.NextUri;
				if (uri != null)
				{
					HttpDelete(uri);
				}

				_state = State.ClientAborted;
			}
		}

		private void HttpDelete(Uri uri)
		{
			_httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, uri));
		}

		private static string UrlEncode(string value)
		{
			return HttpUtility.UrlEncode(value, Encoding.UTF8);
			;
		}

		private static string UrlDecode(string value)
		{
			return HttpUtility.UrlDecode(value, Encoding.UTF8);
		}

		public void Close()
		{
			throw new NotImplementedException();
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