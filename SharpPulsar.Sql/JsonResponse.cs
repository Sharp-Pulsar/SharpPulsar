using System;
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
	using Headers = okhttp3.Headers;
	using MediaType = okhttp3.MediaType;
	using OkHttpClient = okhttp3.OkHttpClient;
	using Request = okhttp3.Request;
	using Response = okhttp3.Response;
	using ResponseBody = okhttp3.ResponseBody;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.@base.MoreObjects.toStringHelper;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.google.common.net.HttpHeaders.LOCATION;

	public sealed class JsonResponse<T>
	{
		public virtual StatusCode {get;}
		public virtual StatusMessage {get;}
		public virtual Headers {get;}
		public virtual ResponseBody {get;}
		private readonly bool hasValue;
		private readonly T value;
		public virtual Exception {get;}

		private JsonResponse(int StatusCode, string StatusMessage, Headers Headers, string ResponseBody)
		{
			this.StatusCode = StatusCode;
			this.StatusMessage = StatusMessage;
			this.Headers = requireNonNull(Headers, "headers is null");
			this.ResponseBody = requireNonNull(ResponseBody, "responseBody is null");

			this.hasValue = false;
			this.value = default(T);
			this.Exception = null;
		}

		private JsonResponse(int StatusCode, string StatusMessage, Headers Headers, string ResponseBody, JsonCodec<T> JsonCodec)
		{
			this.StatusCode = StatusCode;
			this.StatusMessage = StatusMessage;
			this.Headers = requireNonNull(Headers, "headers is null");
			this.ResponseBody = requireNonNull(ResponseBody, "responseBody is null");

			T Value = default(T);
			System.ArgumentException Exception = null;
			try
			{
				Value = JsonCodec.fromJson(ResponseBody);
			}
			catch (System.ArgumentException E)
			{
				Exception = new System.ArgumentException(format("Unable to create %s from JSON response:\n[%s]", JsonCodec.Type, ResponseBody), E);
			}
			this.hasValue = (Exception == null);
			this.value = Value;
			this.Exception = Exception;
		}




		public bool HasValue()
		{
			return hasValue;
		}

		public T Value
		{
			get
			{
				if (!hasValue)
				{
					throw new System.InvalidOperationException("Response does not contain a JSON value", Exception);
				}
				return value;
			}
		}


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nullable public IllegalArgumentException getException()

		public override string ToString()
		{
			return toStringHelper(this).add("statusCode", StatusCode).add("statusMessage", StatusMessage).add("headers", Headers.toMultimap()).add("hasValue", hasValue).add("value", value).omitNullValues().ToString();
		}

		public static JsonResponse<T> Execute<T>(JsonCodec<T> Codec, OkHttpClient Client, Request Request)
		{
			try
			{
					using (Response Response = Client.newCall(Request).execute())
					{
					// TODO: fix in OkHttp: https://github.com/square/okhttp/issues/3111
					if ((Response.code() == 307) || (Response.code() == 308))
					{
						string Location = Response.header(LOCATION);
						if (!string.ReferenceEquals(Location, null))
						{
							Request = Request.newBuilder().url(Location).build();
							return Execute(Codec, Client, Request);
						}
					}
        
					ResponseBody ResponseBody = requireNonNull(Response.body());
					string Body = ResponseBody.@string();
					if (IsJson(ResponseBody.contentType()))
					{
						return new JsonResponse<T>(Response.code(), Response.message(), Response.headers(), Body, Codec);
					}
					return new JsonResponse<T>(Response.code(), Response.message(), Response.headers(), Body);
					}
			}
			catch (IOException E)
			{
				// OkHttp throws this after clearing the interrupt status
				// TODO: remove after updating to Okio 1.15.0+
				if ((E is InterruptedIOException) && "thread interrupted".Equals(E.Message))
				{
					Thread.CurrentThread.Interrupt();
				}
				throw new UncheckedIOException(E);
			}
		}

		private static bool IsJson(MediaType Type)
		{
			return (Type != null) && "application".Equals(Type.type()) && "json".Equals(Type.subtype());
		}
	}

}