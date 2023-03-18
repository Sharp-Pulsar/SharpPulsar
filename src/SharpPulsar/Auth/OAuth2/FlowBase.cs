using System;
using System.Collections.Generic;
using SharpPulsar.Auth.OAuth2.Protocol;
using SharpPulsar.Exceptions;

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
namespace SharpPulsar.Auth.OAuth2
{


    /// <summary>
    /// An abstract OAuth 2.0 authorization flow.
    /// </summary>
    /// 

    [Serializable]
	internal abstract class FlowBase : IFlow
	{
		public abstract void Close();
		public abstract TokenResult Authenticate();

		protected internal readonly Uri IssuerUrl;

		[NonSerialized]
		protected internal Protocol.Metadata Metadata;

		protected internal FlowBase(Uri issuerUrl)
		{
			this.IssuerUrl = issuerUrl;
		}
		public virtual void Initialize()
		{
			try
			{
                var resolve = CreateMetadataResolver();
                var result = resolve.Resolve().Result;
                Metadata = result;
			}
			catch
			{
				//log.error("Unable to retrieve OAuth 2.0 server metadata", E);
				throw new PulsarClientException.AuthenticationException("Unable to retrieve OAuth 2.0 server metadata");
			}
		}

		protected internal virtual MetadataResolver CreateMetadataResolver()
		{
			return DefaultMetadataResolver.FromIssuerUrl(IssuerUrl);
		}

		internal static string ParseParameterString(IDictionary<string, string> prams, string name)
		{
			var s = prams[name];
			if (string.IsNullOrEmpty(s))
			{
				throw new System.ArgumentException("Required configuration parameter: " + name);
			}
			return s;
		}

		internal static Uri ParseParameterUrl(IDictionary<string, string> prams, string name)
		{
			var s = prams[name];
			if (string.IsNullOrEmpty(s))
			{
				throw new System.ArgumentException("Required configuration parameter: " + name);
			}
			try
			{
				return new Uri(s);
			}
			catch (Exception)
			{
				throw new System.ArgumentException("Malformed configuration parameter: " + name);
			}
		}
	}

}