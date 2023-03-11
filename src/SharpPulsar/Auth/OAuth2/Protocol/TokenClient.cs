using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SharpPulsar.Extension;

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
	public class TokenClient : IClientCredentialsExchanger
	{

		protected internal const int DefaultConnectTimeoutInSeconds = 10;
		protected internal const int DefaultReadTimeoutInSeconds = 30;

		private readonly Uri _tokenUrl;
		private readonly HttpClient _httpClient;

		public TokenClient(Uri tokenUrl) : this(tokenUrl, null)
		{
		}

		internal TokenClient(Uri tokenUrl, HttpClient httpClient)
		{
			if (httpClient == null)
			{				
                _httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(DefaultConnectTimeoutInSeconds * 1000)
                };
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "SharpPulsar");
            }
			else
			{
				_httpClient = httpClient;
			}
			_tokenUrl = tokenUrl;
		}

		public void Close()
		{
			_httpClient.Dispose();
		}

		/// <summary>
		/// Constructing http request parameters. </summary>
		/// <param name="req"> object with relevant request parameters </param>
		/// <returns> Generate the final request body from a map. </returns>
		internal virtual IDictionary<string, string> BuildClientCredentialsBody(ClientCredentialsExchangeRequest req)
		{
			IDictionary<string, string> bodyMap = new SortedDictionary<string, string>();
			bodyMap["grant_type"] = "client_credentials";
			bodyMap["client_id"] = req.ClientId;
			bodyMap["client_secret"] = req.ClientSecret;
			// Only set audience and scope if they are non-empty.
			if (!string.IsNullOrWhiteSpace(req.Audience))
			{
				bodyMap["audience"] = req.Audience;
			}
			if (!string.IsNullOrWhiteSpace(req.Scope))
			{
				bodyMap["scope"] = req.Scope;
			}
            var map = bodyMap.SetOfKeyValuePairs().Select(e =>
            {
                try
                {
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(e.Key)) + '=' + Encoding.UTF8.GetString(Encoding.Default.GetBytes(e.Value));
                }
                catch
                {
                    throw;
                }
            });//.Collect(Collectors.joining("&"));
            return bodyMap;// string.Join("&", map);
		}


		/// <summary>
		/// Performs a token exchange using client credentials. </summary>
		/// <param name="req"> the client credentials request details. </param>
		/// <returns> a token result </returns>
		/// <exception cref="TokenExchangeException"> </exception>
		public virtual async Task<TokenResult> ExchangeClientCredentials(ClientCredentialsExchangeRequest req)
		{
			var body = BuildClientCredentialsBody(req);

			try
			{
                var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
                var res = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
                res.Headers.Accept.Add(mediaType);
                res.Content = new FormUrlEncodedContent(body);

                var response = await _httpClient.SendAsync(res).ConfigureAwait(false);
                var resultContent = await response.Content.ReadAsStreamAsync();

                switch (response.StatusCode)
				{
				    case HttpStatusCode.OK:
                            {
                                var result = await JsonSerializer.DeserializeAsync<TokenResult>(resultContent,
                                    new JsonSerializerOptions 
                                    {
                                       PropertyNameCaseInsensitive = true,
                                    });
                                return result;
                             }

				    case HttpStatusCode.BadRequest: // Bad request
				    case HttpStatusCode.Unauthorized: // Unauthorized
                            {
                                resultContent = await response.Content.ReadAsStreamAsync();
                                var result = await JsonSerializer.DeserializeAsync<TokenError>(resultContent);
                                throw new TokenExchangeException(result);
                            }
					
				    default:
					    throw new IOException("Failed to perform HTTP request. res: " + response.StatusCode + " " + response.RequestMessage);
				}



			}
			catch  
			{
				throw;
			}
		}
	}

}