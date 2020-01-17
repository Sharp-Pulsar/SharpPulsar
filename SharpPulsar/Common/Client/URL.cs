using SharpPulsar.Common.Client;
using System;

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
	using System;
	/// <summary>
	/// Wrapper around {@code java.net.URL} to improve usability.
	/// </summary>
	public class URL
	{
		private static readonly URLStreamHandlerFactory urlStreamHandlerFactory = new PulsarURLStreamHandlerFactory();
		private readonly Uri url;

		//public URL(String spec) throws java.net.MalformedURLException, java.net.URISyntaxException, InstantiationException, IllegalAccessException
		public URL(string spec)
		{
			string scheme = (new Uri(spec)).Scheme;
			if (string.ReferenceEquals(scheme, null))
			{
				this.url = new Uri(null, "file:" + spec);
			}
			else
			{
				this.url = new Uri(null, spec, urlStreamHandlerFactory.createURLStreamHandler(scheme));
			}
		}
		public virtual URLConnection OpenConnection()
		{
			return this.url.openConnection();
		}
		public virtual object Content
		{
			get
			{
				return this.url.Content;
			}
		}

		public virtual object GetContent(Type[] classes)
		{
			return this.url.GetContent(classes);
		}

	}

}