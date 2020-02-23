using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

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
namespace SharpPulsar.Utility
{

	/// <summary>
	/// Auto refresher and builder of SSLContext.
	/// </summary>
	/// @param <T>
	///            type of SSLContext </param>
	public abstract class SslContextAutoRefreshBuilder<T>
	{
		protected internal readonly bool tlsAllowInsecureConnection;
		protected internal readonly FileModifiedTimeUpdater tlsTrustCertsFilePath, tlsCertificateFilePath, tlsKeyFilePath;
		protected internal readonly ISet<string> tlsCiphers;
		protected internal readonly ISet<string> tlsProtocols;
		protected internal readonly bool tlsRequireTrustedClientCertOnConnect;
		protected internal readonly long refreshTime;
		protected internal long lastRefreshTime;

		public SslContextAutoRefreshBuilder(bool allowInsecure, string trustCertsFilePath, string certificateFilePath, string keyFilePath, ISet<string> ciphers, ISet<string> protocols, bool requireTrustedClientCertOnConnect, long certRefreshInSec)
		{
			this.tlsAllowInsecureConnection = allowInsecure;
			this.tlsTrustCertsFilePath = new FileModifiedTimeUpdater(trustCertsFilePath);
			this.tlsCertificateFilePath = new FileModifiedTimeUpdater(certificateFilePath);
			this.tlsKeyFilePath = new FileModifiedTimeUpdater(keyFilePath);
			this.tlsCiphers = ciphers;
			this.tlsProtocols = protocols;
			this.tlsRequireTrustedClientCertOnConnect = requireTrustedClientCertOnConnect;
			this.refreshTime = BAMCIS.Util.Concurrent.TimeUnit.SECONDS.ToMillis(certRefreshInSec);
			this.lastRefreshTime = -1;

			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug("Certs will be refreshed every {} seconds", certRefreshInSec);
			}
		}

		/// <summary>
		/// updates and returns cached SSLContext.
		/// 
		/// @return </summary>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="IOException"> </exception>
		protected internal abstract T Update();

		/// <summary>
		/// Returns cached SSLContext.
		/// 
		/// @return
		/// </summary>
		protected internal abstract T SslContext {get;}

		/// <summary>
		/// It updates SSLContext at every configured refresh time and returns updated SSLContext.
		/// 
		/// @return
		/// </summary>
		public virtual T Get()
		{
			T ctx = SslContext;
			if (ctx == null)
			{
				try
				{
					Update();
					lastRefreshTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					return SslContext;
				}
				catch (Exception e) when (e is SecurityException || e is IOException)
				{
					log.LogError("Execption while trying to refresh ssl Context {}", e.Message, e);
				}
			}
			else
			{
				long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				if (refreshTime <= 0 || now > (lastRefreshTime + refreshTime))
				{
					if (tlsTrustCertsFilePath.CheckAndRefresh() || tlsCertificateFilePath.CheckAndRefresh() || tlsKeyFilePath.CheckAndRefresh())
					{
						try
						{
							ctx = Update();
							lastRefreshTime = now;
						}
						catch (Exception e) when (e is SecurityException || e is IOException)
						{
							log.LogError("Execption while trying to refresh ssl Context {} ", e.Message, e);
						}
					}
				}
			}
			return ctx;
		}

		private static readonly ILogger log = Utility.Log.Logger.CreateLogger<SslContextAutoRefreshBuilder<T>>();
	}

}