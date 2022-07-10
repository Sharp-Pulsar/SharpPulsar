using System;
using System.Collections.Generic;

/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Auth.OAuth2.Protocol
{

	/// <summary>
	/// A client for an OAuth 2.0 token endpoint.
	/// </summary>
	public class TokenClient : ClientCredentialsExchanger
	{

		protected internal const int DefaultConnectTimeoutInSeconds = 10;
		protected internal const int DefaultReadTimeoutInSeconds = 30;

		private readonly URL tokenUrl;
		private readonly AsyncHttpClient httpClient;

		public TokenClient(URL TokenUrl) : this(TokenUrl, null)
		{
		}

		internal TokenClient(URL TokenUrl, AsyncHttpClient HttpClient)
		{
			if (HttpClient == null)
			{
				DefaultAsyncHttpClientConfig.Builder ConfBuilder = new DefaultAsyncHttpClientConfig.Builder();
				ConfBuilder.setFollowRedirect(true);
				ConfBuilder.setConnectTimeout(DefaultConnectTimeoutInSeconds * 1000);
				ConfBuilder.setReadTimeout(DefaultReadTimeoutInSeconds * 1000);
				ConfBuilder.setUserAgent(string.Format("Pulsar-Java-v{0}", PulsarVersion.Version));
				AsyncHttpClientConfig Config = ConfBuilder.build();
				this.httpClient = new DefaultAsyncHttpClient(Config);
			}
			else
			{
				this.httpClient = HttpClient;
			}
			this.tokenUrl = TokenUrl;
		}

		public override void close()
		{
			httpClient.close();
		}

		/// <summary>
		/// Constructing http request parameters. </summary>
		/// <param name="req"> object with relevant request parameters </param>
		/// <returns> Generate the final request body from a map. </returns>
		internal virtual string BuildClientCredentialsBody(ClientCredentialsExchangeRequest Req)
		{
			IDictionary<string, string> BodyMap = new SortedDictionary<string, string>();
			BodyMap["grant_type"] = "client_credentials";
			BodyMap["client_id"] = Req.getClientId();
			BodyMap["client_secret"] = Req.getClientSecret();
			// Only set audience and scope if they are non-empty.
			if (!StringUtils.isBlank(Req.getAudience()))
			{
				BodyMap["audience"] = Req.getAudience();
			}
			if (!StringUtils.isBlank(Req.getScope()))
			{
				BodyMap["scope"] = Req.getScope();
			}
			return BodyMap.SetOfKeyValuePairs().Select(e =>
			{
			try
			{
				return URLEncoder.encode(e.getKey(), "UTF-8") + '=' + URLEncoder.encode(e.getValue(), "UTF-8");
			}
			catch (UnsupportedEncodingException E1)
			{
				throw new Exception(E1);
			}
			}).collect(Collectors.joining("&"));
		}

		/// <summary>
		/// Performs a token exchange using client credentials. </summary>
		/// <param name="req"> the client credentials request details. </param>
		/// <returns> a token result </returns>
		/// <exception cref="TokenExchangeException"> </exception>
		public virtual TokenResult ExchangeClientCredentials(ClientCredentialsExchangeRequest Req)
		{
			string Body = BuildClientCredentialsBody(Req);

			try
			{

				Response Res = httpClient.preparePost(tokenUrl.ToString()).setHeader("Accept", "application/json").setHeader("Content-Type", "application/x-www-form-urlencoded").setBody(Body).execute().get();

				switch (Res.getStatusCode())
				{
				case 200:
					return ObjectMapperFactory.ThreadLocal.reader().readValue(Res.getResponseBodyAsBytes(), typeof(TokenResult));

				case 400: // Bad request
				case 401: // Unauthorized
					throw new TokenExchangeException(ObjectMapperFactory.ThreadLocal.reader().readValue(Res.getResponseBodyAsBytes(), typeof(TokenError)));

				default:
					throw new IOException("Failed to perform HTTP request. res: " + Res.getStatusCode() + " " + Res.getStatusText());
				}



			}
			catch (Exception e1) when (e1 is InterruptedException || e1 is ExecutionException)
			{
				throw new IOException(e1);
			}
		}
	}

}