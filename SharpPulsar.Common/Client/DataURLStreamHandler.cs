using System;
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
namespace SharpPulsar.Common.Client
{

	/// <summary>
	/// Extension of the {@code URLStreamHandler} class to handle all stream protocol handlers.
	/// </summary>
	public class DataURLStreamHandler : URLStreamHandler
	{

		/// <summary>
		/// Representation of a communications link between the application and a URL.
		/// </summary>
		internal class DataURLConnection : URLConnection
		{
			internal bool parsed = false;
			internal string contentType;
			internal sbyte[] data;
			internal Uri uri;

			internal static readonly Pattern pattern = Pattern.compile("(?<mimeType>[^;,]+)?(;(?<charset>charset=[^;,]+))?(;(?<base64>base64))?,(?<data>.+)", Pattern.DOTALL);

			protected internal DataURLConnection(URL url) : base(url)
			{
				try
				{
					this.uri = this.url.toURI();
				}
				catch (URISyntaxException)
				{
					this.uri = null;
				}
			}

			public void Connect()
			{
				if (this.parsed)
				{
					return;
				}

				if (this.uri == null)
				{
					throw new IOException();
				}

				Matcher matcher = pattern.matcher(this.uri.SchemeSpecificPart);
				if (matcher.matches())
				{
					this.contentType = matcher.group("mimeType");
					if (string.ReferenceEquals(contentType, null))
					{
						this.contentType = "application/data";
					}

					if (matcher.group("base64") == null)
					{
						// Support Urlencode but not decode here because already decoded by URI class.
						this.data = matcher.group("data").Bytes;
					}
					else
					{
						this.data = Base64.Decoder.decode(matcher.group("data"));
					}
				}
				else
				{
					throw new MalformedURLException();
				}
				parsed = true;
			}

			public long ContentLengthLong
			{
				get
				{
					long length;
					try
					{
						this.Connect();
						length = this.data.Length;
					}
					catch (IOException)
					{
						length = -1;
					}
					return length;
				}
			}

			public string ContentType
			{
				get
				{
					string contentType;
					try
					{
						this.connect();
						contentType = this.contentType;
					}
					catch (IOException)
					{
						contentType = null;
					}
					return contentType;
				}
			}

			public string ContentEncoding
			{
				get
				{
					return "identity";
				}
			}

			public virtual Stream InputStream
			{
				get
				{
					this.connect();
					return new MemoryStream(this.data);
				}
			}
		}

		protected internal URLConnection OpenConnection(URL u)
		{
			return new DataURLConnection(u);
		}

	}

}