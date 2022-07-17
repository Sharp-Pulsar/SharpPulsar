using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;

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
	/// Resolves OAuth 2.0 authorization server metadata as described in RFC 8414.
	/// </summary>
	public class DefaultMetadataResolver : MetadataResolver
	{

		protected internal const int DefaultConnectTimeoutInSeconds = 10;
		protected internal const int DefaultReadTimeoutInSeconds = 30;

		private readonly Uri _metadataUrl;
		private TimeSpan _connectTimeout;
		private TimeSpan _readTimeout;

		public DefaultMetadataResolver(Uri metadataUrl)
		{
            _metadataUrl = metadataUrl;
            // set a default timeout to ensure that this doesn't block
            _connectTimeout = TimeSpan.FromSeconds(DefaultConnectTimeoutInSeconds);
            _readTimeout = TimeSpan.FromSeconds(DefaultReadTimeoutInSeconds);
		}

		public virtual DefaultMetadataResolver WithConnectTimeout(TimeSpan connectTimeout)
		{
            _connectTimeout = connectTimeout;
			return this;
		}

		public virtual DefaultMetadataResolver WithReadTimeout(TimeSpan readTimeout)
		{
            _readTimeout = readTimeout;
			return this;
		}

		/// <summary>
		/// Resolves the authorization metadata. </summary>
		/// <returns> metadata </returns>
		/// <exception cref="IOException"> if the metadata could not be resolved. </exception>
		public async Task<Metadata> Resolve()
		{
			try
			{
                var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(mediaType);
                client.Timeout = TimeSpan.FromSeconds(DefaultConnectTimeoutInSeconds);
				var c = _metadataUrl;
                var metadataDataUrl = GetWellKnownMetadataUrl(_metadataUrl);
                var response = await client.GetStreamAsync(metadataDataUrl);
                return await JsonSerializer.DeserializeAsync<Metadata>(response);                

			}
			catch (IOException E)
			{
				throw new IOException("Cannot obtain authorization metadata from " + _metadataUrl.ToString(), E);
			}
		}

		/// <summary>
		/// Gets a well-known metadata URL for the given OAuth issuer URL. </summary>
		/// <param name="issuerUrl"> The authorization server's issuer identifier </param>
		/// <returns> a resolver </returns>
		public static DefaultMetadataResolver FromIssuerUrl(Uri issuerUrl)
		{
			return new DefaultMetadataResolver(GetWellKnownMetadataUrl(issuerUrl));
		}

		/// <summary>
		/// Gets a well-known metadata URL for the given OAuth issuer URL. </summary>
		/// <seealso cref="<a href="https://tools.ietf.org/id/draft-ietf-oauth-discovery-08.html.ASConfig">"
		///     OAuth Discovery: Obtaining Authorization Server Metadata</a>/>
		/// <param name="issuerUrl"> The authorization server's issuer identifier </param>
		/// <returns> a URL </returns>
		public static Uri GetWellKnownMetadataUrl(Uri issuerUrl)
		{
			try
			{
                return new Uri(issuerUrl.AbsoluteUri + ".well-known/openid-configuration");

            }
			catch (Exception e)
			{
				throw e;
			}
		}
	}

}