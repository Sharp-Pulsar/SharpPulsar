using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Sql.Precondition;
using SharpPulsar.Sql.Presto.Facebook.Type;

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
namespace SharpPulsar.Sql.Presto
{
    public sealed class JsonResponse<T>
	{
		public HttpResponseMessage ResponseMessage { get; }
		public HttpResponseHeaders Headers {get;}
		public string ResponseBody {get;}
		private readonly bool _hasValue;
		private readonly T _value;
		public Exception Exception {get;}
		private JsonResponse(string responseBody, HttpResponseMessage responseMessage)
		{
			Headers = Condition.RequireNonNull(responseMessage?.Headers, "headers is null");
			ResponseBody = Condition.RequireNonNull(responseBody, "responseBody is null");
            ResponseMessage = responseMessage;
			_hasValue = false;
			_value = default(T);
			Exception = null;
		}

		private JsonResponse(HttpResponseMessage responseMessage, string responseBody)
		{
			Headers = Condition.RequireNonNull(responseMessage?.Headers, "headers is null");
			ResponseBody = Condition.RequireNonNull(responseBody, "responseBody is null");
            ResponseMessage = responseMessage;
			T value = default(T);
			ArgumentException exception = null;
			try
			{
				value = JsonSerializer.Deserialize<T>(responseBody);
			}
			catch (ArgumentException e)
			{
				exception = new ArgumentException($"Unable to create {typeof(T).Name} from JSON response:\n[{responseBody}]", e);
			}
			_hasValue = (exception == null);
			_value = value;
			Exception = exception;
		}




		public bool HasValue()
		{
			return _hasValue;
		}

		public T Value
		{
			get
			{
				if (!_hasValue)
				{
					throw new InvalidOperationException("Response does not contain a JSON value", Exception);
				}
				return _value;
			}
		}

		
		public override string ToString()
        {
            return StringHelper.Build(this).Add("statusCode", ResponseMessage.StatusCode).Add("statusMessage", ResponseMessage.ReasonPhrase).Add("headers", Headers).Add("hasValue", _hasValue).Add("value", _value).ToString();
		}

		public static async Task<JsonResponse<T>> Execute(HttpClient client, HttpRequestMessage request)
		{
            var req = request;
            using var response = await client.SendAsync(req).ConfigureAwait(false);
            if ((response.StatusCode == HttpStatusCode.RedirectKeepVerb) || (response.StatusCode == HttpStatusCode.PermanentRedirect))
            {
                var location = response.Headers.Location;
                if (!ReferenceEquals(location, null))
                {
                    req.RequestUri = location;
                    return await Execute(client, req).ConfigureAwait(false);
                }
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            if (stream != null)
            {
                using var sr = new StreamReader(stream);
                var stringBody = sr.ReadToEnd();
                var responseBody = Condition.RequireNonNull(stringBody, "content is null");
                if (IsJson(response.Content.Headers.ContentType.MediaType))
                {
                    return new JsonResponse<T>(response, responseBody);
                }
                return new JsonResponse<T>(responseBody, response);
            }
            throw new NullReferenceException();
        }

		private static bool IsJson(string type)
		{
			return (type != null) && type.StartsWith("application") && type.EndsWith("json");
		}
	}

}