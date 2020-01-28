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

	public class DefaultSslContextBuilder : SslContextAutoRefreshBuilder<SSLContext>
	{
		private volatile SSLContext sslContext;
		public DefaultSslContextBuilder(bool allowInsecure, string trustCertsFilePath, string certificateFilePath, string keyFilePath, bool requireTrustedClientCertOnConnect, long certRefreshInSec) : base(allowInsecure, trustCertsFilePath, certificateFilePath, keyFilePath, null, null, requireTrustedClientCertOnConnect, certRefreshInSec)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public synchronized javax.net.ssl.SSLContext update() throws java.security.GeneralSecurityException
		public override SSLContext update()
		{
			lock (this)
			{
				this.sslContext = SecurityUtility.createSslContext(tlsAllowInsecureConnection, tlsTrustCertsFilePath.FileName, tlsCertificateFilePath.FileName, tlsKeyFilePath.FileName);
				return this.sslContext;
			}
		}

		public override SSLContext SslContext
		{
			get
			{
				return this.sslContext;
			}
		}

	}

}