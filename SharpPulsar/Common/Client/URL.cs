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
namespace org.apache.pulsar.client.api.url
{

	/// <summary>
	/// Wrapper around {@code java.net.URL} to improve usability.
	/// </summary>
	public class URL
	{
		private static readonly URLStreamHandlerFactory urlStreamHandlerFactory = new PulsarURLStreamHandlerFactory();
		private readonly java.net.URL url;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public URL(String spec) throws java.net.MalformedURLException, java.net.URISyntaxException, InstantiationException, IllegalAccessException
		public URL(string spec)
		{
			string scheme = (new URI(spec)).Scheme;
			if (string.ReferenceEquals(scheme, null))
			{
				this.url = new java.net.URL(null, "file:" + spec);
			}
			else
			{
				this.url = new java.net.URL(null, spec, urlStreamHandlerFactory.createURLStreamHandler(scheme));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URLConnection openConnection() throws java.io.IOException
		public virtual URLConnection openConnection()
		{
			return this.url.openConnection();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object getContent() throws java.io.IOException
		public virtual object Content
		{
			get
			{
				return this.url.Content;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object getContent(Class[] classes) throws java.io.IOException
		public virtual object getContent(Type[] classes)
		{
			return this.url.getContent(classes);
		}

	}

}