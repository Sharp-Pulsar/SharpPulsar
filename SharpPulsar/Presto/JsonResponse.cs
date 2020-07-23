using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Org.BouncyCastle.Asn1.Ocsp;
using SharpPulsar.Precondition;

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
    public sealed class JsonResponse<T>
	{
		public int StatusCode {get;}
		public string StatusMessage {get;}
		public object Headers {get;}
		public string ResponseBody {get;}
		private readonly bool _hasValue;
		private readonly T _value;
		public Exception Exception {get;}
		private JsonResponse(int statusCode, string statusMessage, string responseBody, string method, string url, string request = "", WebHeaderCollection headers = null)
		{
			StatusCode = statusCode;
			StatusMessage = statusMessage;
			Headers = Condition.RequireNonNull(headers, "headers is null");
			ResponseBody = Condition.RequireNonNull(responseBody, "responseBody is null");

			_hasValue = false;
			_value = default(T);
			Exception = null;
		}

		private JsonResponse(int statusCode, string statusMessage, HttpHeaders headers, string responseBody, JsonCodec<T> jsonCodec)
		{
			StatusCode = statusCode;
			StatusMessage = statusMessage;
			Headers = Condition.RequireNonNull(headers, "headers is null");
			ResponseBody = Condition.RequireNonNull(responseBody, "responseBody is null");

			T value = default(T);
			ArgumentException exception = null;
			try
			{
				value = jsonCodec.fromJson(responseBody);
			}
			catch (ArgumentException e)
			{
				exception = new ArgumentException(format("Unable to create %s from JSON response:\n[%s]", jsonCodec.Type, responseBody), e);
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
			return StringHelper.Build(this).Add("statusCode", StatusCode).Add("statusMessage", StatusMessage).Add("headers", Headers.ToMultimap()).Add("hasValue", _hasValue).Add("value", _value).ToString();
		}

		public static JsonResponse<T> Execute<T>(JsonCodec<T> codec, HttpClient client, Request request)
		{
			try
			{
					using (var response = client.GetAsync(request))
					{
					    if ((response.Result.StatusCode == HttpStatusCode.RedirectKeepVerb) || (response.Result.StatusCode == HttpStatusCode.PermanentRedirect))
					    {
						    string location = response.Result.Headers.Location.AbsoluteUri;
						    if (!ReferenceEquals(location, null))
						    {
							    request = request.newBuilder().url(location).build();
							    return Execute(codec, client, request);
						    }
					    }
        
					ResponseBody responseBody = requireNonNull(response.body());
					string body = responseBody.@string();
					if (IsJson(responseBody.contentType()))
					{
						return new JsonResponse<T>(response.code(), response.message(), response.headers(), body, codec);
					}
					return new JsonResponse<T>(response.code(), response.message(), response.headers(), body);
					}
			}
			catch (IOException e)
            {
                throw;
            }
		}

		private static bool IsJson(MediaType type)
		{
			return (type != null) && "application".Equals(type) && "json".Equals(type.subtype());
		}
	}

}