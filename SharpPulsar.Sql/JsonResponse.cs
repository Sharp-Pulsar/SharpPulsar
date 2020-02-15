using System;
using System.IO;
using System.Threading;
using SharpPulsar.Sql.Facebook.Type;
using System.Net.Http.Headers;

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
	using Headers = okhttp3.Headers;
	using MediaType = okhttp3.MediaType;
	using OkHttpClient = okhttp3.OkHttpClient;
	using Request = okhttp3.Request;
	using Response = okhttp3.Response;
	using ResponseBody = okhttp3.ResponseBody;

	public sealed class JsonResponse<T>
	{
		public int StatusCode {get;}
		public string StatusMessage {get;}
		public object Headers {get;}
		public string ResponseBody {get;}
		private readonly bool _hasValue;
		private readonly T _value;
		public Exception Exception {get;}

		private JsonResponse(int statusCode, string statusMessage, Headers headers, string responseBody)
		{
			this.StatusCode = statusCode;
			this.StatusMessage = statusMessage;
			this.Headers = requireNonNull(headers, "headers is null");
			this.ResponseBody = requireNonNull(responseBody, "responseBody is null");

			this._hasValue = false;
			this._value = default(T);
			this.Exception = null;
		}

		private JsonResponse(int statusCode, string statusMessage, Headers headers, string responseBody, JsonCodec<T> jsonCodec)
		{
			this.StatusCode = statusCode;
			this.StatusMessage = statusMessage;
			this.Headers = requireNonNull(headers, "headers is null");
			this.ResponseBody = requireNonNull(responseBody, "responseBody is null");

			T value = default(T);
			System.ArgumentException exception = null;
			try
			{
				value = jsonCodec.fromJson(responseBody);
			}
			catch (System.ArgumentException e)
			{
				exception = new System.ArgumentException(format("Unable to create %s from JSON response:\n[%s]", jsonCodec.Type, responseBody), e);
			}
			this._hasValue = (exception == null);
			this._value = value;
			this.Exception = exception;
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
					throw new System.InvalidOperationException("Response does not contain a JSON value", Exception);
				}
				return _value;
			}
		}

		
		public override string ToString()
		{
			return StringHelper.Build(this).Add("statusCode", StatusCode).Add("statusMessage", StatusMessage).Add("headers", Headers.ToMultimap()).Add("hasValue", _hasValue).Add("value", _value).ToString();
		}

		public static JsonResponse<T> Execute<T>(JsonCodec<T> codec, OkHttpClient client, Request request)
		{
			try
			{
					using (Response response = client.newCall(request).execute())
					{
					// TODO: fix in OkHttp: https://github.com/square/okhttp/issues/3111
					if ((response.code() == 307) || (response.code() == 308))
					{
						string location = response.header(LOCATION);
						if (!string.ReferenceEquals(location, null))
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
				// OkHttp throws this after clearing the interrupt status
				// TODO: remove after updating to Okio 1.15.0+
				if ((e is InterruptedIOException) && "thread interrupted".Equals(e.Message))
				{
					Thread.CurrentThread.Interrupt();
				}
				throw new UncheckedIOException(e);
			}
		}

		private static bool IsJson(MediaType type)
		{
			return (type != null) && "application".Equals(type.type()) && "json".Equals(type.subtype());
		}
	}

}