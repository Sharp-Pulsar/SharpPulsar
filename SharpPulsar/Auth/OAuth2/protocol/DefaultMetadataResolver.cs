using System.IO;

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
	using ObjectMapper = com.fasterxml.jackson.databind.ObjectMapper;
	using ObjectReader = com.fasterxml.jackson.databind.ObjectReader;

	/// <summary>
	/// Resolves OAuth 2.0 authorization server metadata as described in RFC 8414.
	/// </summary>
	public class DefaultMetadataResolver : MetadataResolver
	{

		protected internal const int DefaultConnectTimeoutInSeconds = 10;
		protected internal const int DefaultReadTimeoutInSeconds = 30;

		private readonly URL metadataUrl;
		private readonly ObjectReader objectReader;
		private Duration connectTimeout;
		private Duration readTimeout;

		public DefaultMetadataResolver(URL MetadataUrl)
		{
			this.metadataUrl = MetadataUrl;
			this.objectReader = (new ObjectMapper()).readerFor(typeof(Metadata));
			// set a default timeout to ensure that this doesn't block
			this.connectTimeout = Duration.ofSeconds(DefaultConnectTimeoutInSeconds);
			this.readTimeout = Duration.ofSeconds(DefaultReadTimeoutInSeconds);
		}

		public virtual DefaultMetadataResolver WithConnectTimeout(Duration ConnectTimeout)
		{
			this.connectTimeout = ConnectTimeout;
			return this;
		}

		public virtual DefaultMetadataResolver WithReadTimeout(Duration ReadTimeout)
		{
			this.readTimeout = ReadTimeout;
			return this;
		}

		/// <summary>
		/// Resolves the authorization metadata. </summary>
		/// <returns> metadata </returns>
		/// <exception cref="IOException"> if the metadata could not be resolved. </exception>
		public virtual Metadata Resolve()
		{
			try
			{
				URLConnection C = this.metadataUrl.openConnection();
				if (connectTimeout != null)
				{
					C.setConnectTimeout((int) connectTimeout.toMillis());
				}
				if (readTimeout != null)
				{
					C.setReadTimeout((int) readTimeout.toMillis());
				}
				C.setRequestProperty("Accept", "application/json");

				Metadata Metadata;
				using (Stream InputStream = C.getInputStream())
				{
					Metadata = this.objectReader.readValue(InputStream);
				}
				return Metadata;

			}
			catch (IOException E)
			{
				throw new IOException("Cannot obtain authorization metadata from " + metadataUrl.ToString(), E);
			}
		}

		/// <summary>
		/// Gets a well-known metadata URL for the given OAuth issuer URL. </summary>
		/// <param name="issuerUrl"> The authorization server's issuer identifier </param>
		/// <returns> a resolver </returns>
		public static DefaultMetadataResolver FromIssuerUrl(URL IssuerUrl)
		{
			return new DefaultMetadataResolver(GetWellKnownMetadataUrl(IssuerUrl));
		}

		/// <summary>
		/// Gets a well-known metadata URL for the given OAuth issuer URL. </summary>
		/// <seealso cref="<a href="https://tools.ietf.org/id/draft-ietf-oauth-discovery-08.html.ASConfig">"
		///     OAuth Discovery: Obtaining Authorization Server Metadata</a>/>
		/// <param name="issuerUrl"> The authorization server's issuer identifier </param>
		/// <returns> a URL </returns>
		public static URL GetWellKnownMetadataUrl(URL IssuerUrl)
		{
			try
			{
				return URI.create(IssuerUrl.toExternalForm() + "/.well-known/openid-configuration").normalize().toURL();
			}
			catch (MalformedURLException E)
			{
				throw new System.ArgumentException(E);
			}
		}
	}

}