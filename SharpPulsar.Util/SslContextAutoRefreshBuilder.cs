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
namespace SharpPulsar.Util
{

	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;

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
			this.refreshTime = TimeUnit.SECONDS.toMillis(certRefreshInSec);
			this.lastRefreshTime = -1;

			if (LOG.DebugEnabled)
			{
				LOG.debug("Certs will be refreshed every {} seconds", certRefreshInSec);
			}
		}

		/// <summary>
		/// updates and returns cached SSLContext.
		/// 
		/// @return </summary>
		/// <exception cref="GeneralSecurityException"> </exception>
		/// <exception cref="IOException"> </exception>
		protected internal abstract T update();

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
		public virtual T get()
		{
			T ctx = SslContext;
			if (ctx == null)
			{
				try
				{
					update();
					lastRefreshTime = DateTimeHelper.CurrentUnixTimeMillis();
					return SslContext;
				}
				catch (Exception e) when (e is GeneralSecurityException || e is IOException)
				{
					LOG.error("Execption while trying to refresh ssl Context {}", e.Message, e);
				}
			}
			else
			{
				long now = DateTimeHelper.CurrentUnixTimeMillis();
				if (refreshTime <= 0 || now > (lastRefreshTime + refreshTime))
				{
					if (tlsTrustCertsFilePath.checkAndRefresh() || tlsCertificateFilePath.checkAndRefresh() || tlsKeyFilePath.checkAndRefresh())
					{
						try
						{
							ctx = update();
							lastRefreshTime = now;
						}
						catch (Exception e) when (e is GeneralSecurityException || e is IOException)
						{
							LOG.error("Execption while trying to refresh ssl Context {} ", e.Message, e);
						}
					}
				}
			}
			return ctx;
		}

		private static readonly Logger LOG = LoggerFactory.getLogger(typeof(SslContextAutoRefreshBuilder));
	}

}