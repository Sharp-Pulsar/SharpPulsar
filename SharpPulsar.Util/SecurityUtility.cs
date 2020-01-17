using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

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
namespace org.apache.pulsar.common.util
{
	using ClientAuth = io.netty.handler.ssl.ClientAuth;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslContextBuilder = io.netty.handler.ssl.SslContextBuilder;
	using InsecureTrustManagerFactory = io.netty.handler.ssl.util.InsecureTrustManagerFactory;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;

	/// <summary>
	/// Helper class for the security domain.
	/// </summary>
	public class SecurityUtility
	{

		static SecurityUtility()
		{
			// Fixes loading PKCS8Key file: https://stackoverflow.com/a/18912362
			java.security.Security.addProvider(new org.bouncycastle.jce.provider.BouncyCastleProvider());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static javax.net.ssl.SSLContext createSslContext(boolean allowInsecureConnection, java.security.cert.Certificate[] trustCertificates) throws java.security.GeneralSecurityException
		public static SSLContext createSslContext(bool allowInsecureConnection, Certificate[] trustCertificates)
		{
			return createSslContext(allowInsecureConnection, trustCertificates, (Certificate[]) null, (PrivateKey) null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.handler.ssl.SslContext createNettySslContextForClient(boolean allowInsecureConnection, String trustCertsFilePath) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContext createNettySslContextForClient(bool allowInsecureConnection, string trustCertsFilePath)
		{
			return createNettySslContextForClient(allowInsecureConnection, trustCertsFilePath, (Certificate[]) null, (PrivateKey) null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static javax.net.ssl.SSLContext createSslContext(boolean allowInsecureConnection, String trustCertsFilePath, String certFilePath, String keyFilePath) throws java.security.GeneralSecurityException
		public static SSLContext createSslContext(bool allowInsecureConnection, string trustCertsFilePath, string certFilePath, string keyFilePath)
		{
			X509Certificate[] trustCertificates = loadCertificatesFromPemFile(trustCertsFilePath);
			X509Certificate[] certificates = loadCertificatesFromPemFile(certFilePath);
			PrivateKey privateKey = loadPrivateKeyFromPemFile(keyFilePath);
			return createSslContext(allowInsecureConnection, trustCertificates, certificates, privateKey);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.handler.ssl.SslContext createNettySslContextForClient(boolean allowInsecureConnection, String trustCertsFilePath, String certFilePath, String keyFilePath) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContext createNettySslContextForClient(bool allowInsecureConnection, string trustCertsFilePath, string certFilePath, string keyFilePath)
		{
			X509Certificate[] certificates = loadCertificatesFromPemFile(certFilePath);
			PrivateKey privateKey = loadPrivateKeyFromPemFile(keyFilePath);
			return createNettySslContextForClient(allowInsecureConnection, trustCertsFilePath, certificates, privateKey);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.handler.ssl.SslContext createNettySslContextForClient(boolean allowInsecureConnection, String trustCertsFilePath, java.security.cert.Certificate[] certificates, java.security.PrivateKey privateKey) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContext createNettySslContextForClient(bool allowInsecureConnection, string trustCertsFilePath, Certificate[] certificates, PrivateKey privateKey)
		{
			SslContextBuilder builder = SslContextBuilder.forClient();
			setupTrustCerts(builder, allowInsecureConnection, trustCertsFilePath);
			setupKeyManager(builder, privateKey, (X509Certificate[]) certificates);
			return builder.build();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static io.netty.handler.ssl.SslContext createNettySslContextForServer(boolean allowInsecureConnection, String trustCertsFilePath, String certFilePath, String keyFilePath, java.util.Set<String> ciphers, java.util.Set<String> protocols, boolean requireTrustedClientCertOnConnect) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContext createNettySslContextForServer(bool allowInsecureConnection, string trustCertsFilePath, string certFilePath, string keyFilePath, ISet<string> ciphers, ISet<string> protocols, bool requireTrustedClientCertOnConnect)
		{
			X509Certificate[] certificates = loadCertificatesFromPemFile(certFilePath);
			PrivateKey privateKey = loadPrivateKeyFromPemFile(keyFilePath);

			SslContextBuilder builder = SslContextBuilder.forServer(privateKey, (X509Certificate[]) certificates);
			setupCiphers(builder, ciphers);
			setupProtocols(builder, protocols);
			setupTrustCerts(builder, allowInsecureConnection, trustCertsFilePath);
			setupKeyManager(builder, privateKey, certificates);
			setupClientAuthentication(builder, requireTrustedClientCertOnConnect);
			return builder.build();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static javax.net.ssl.SSLContext createSslContext(boolean allowInsecureConnection, java.security.cert.Certificate[] trustCertficates, java.security.cert.Certificate[] certificates, java.security.PrivateKey privateKey) throws java.security.GeneralSecurityException
		public static SSLContext createSslContext(bool allowInsecureConnection, Certificate[] trustCertficates, Certificate[] certificates, PrivateKey privateKey)
		{
			KeyStoreHolder ksh = new KeyStoreHolder();
			TrustManager[] trustManagers = null;
			KeyManager[] keyManagers = null;

			trustManagers = setupTrustCerts(ksh, allowInsecureConnection, trustCertficates);
			keyManagers = setupKeyManager(ksh, privateKey, certificates);

			SSLContext sslCtx = SSLContext.getInstance("TLS");
			sslCtx.init(keyManagers, trustManagers, new SecureRandom());
			sslCtx.DefaultSSLParameters;
			return sslCtx;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static javax.net.ssl.KeyManager[] setupKeyManager(KeyStoreHolder ksh, java.security.PrivateKey privateKey, java.security.cert.Certificate[] certificates) throws java.security.KeyStoreException, java.security.NoSuchAlgorithmException, java.security.UnrecoverableKeyException
		private static KeyManager[] setupKeyManager(KeyStoreHolder ksh, PrivateKey privateKey, Certificate[] certificates)
		{
			KeyManager[] keyManagers = null;
			if (certificates != null && privateKey != null)
			{
				ksh.setPrivateKey("private", privateKey, certificates);
				KeyManagerFactory kmf = KeyManagerFactory.getInstance(KeyManagerFactory.DefaultAlgorithm);
				kmf.init(ksh.KeyStore, "".ToCharArray());
				keyManagers = kmf.KeyManagers;
			}
			return keyManagers;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static javax.net.ssl.TrustManager[] setupTrustCerts(KeyStoreHolder ksh, boolean allowInsecureConnection, java.security.cert.Certificate[] trustCertficates) throws java.security.NoSuchAlgorithmException, java.security.KeyStoreException
		private static TrustManager[] setupTrustCerts(KeyStoreHolder ksh, bool allowInsecureConnection, Certificate[] trustCertficates)
		{
			TrustManager[] trustManagers;
			if (allowInsecureConnection)
			{
				trustManagers = InsecureTrustManagerFactory.INSTANCE.TrustManagers;
			}
			else
			{
				TrustManagerFactory tmf = TrustManagerFactory.getInstance(TrustManagerFactory.DefaultAlgorithm);

				if (trustCertficates == null || trustCertficates.Length == 0)
				{
					tmf.init((KeyStore) null);
				}
				else
				{
					for (int i = 0; i < trustCertficates.Length; i++)
					{
						ksh.setCertificate("trust" + i, trustCertficates[i]);
					}
					tmf.init(ksh.KeyStore);
				}

				trustManagers = tmf.TrustManagers;
			}
			return trustManagers;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.security.cert.X509Certificate[] loadCertificatesFromPemFile(String certFilePath) throws java.security.KeyManagementException
		public static X509Certificate[] loadCertificatesFromPemFile(string certFilePath)
		{
			X509Certificate[] certificates = null;

			if (string.ReferenceEquals(certFilePath, null) || certFilePath.Length == 0)
			{
				return certificates;
			}

			try
			{
					using (FileStream input = new FileStream(certFilePath, FileMode.Open, FileAccess.Read))
					{
					CertificateFactory cf = CertificateFactory.getInstance("X.509");
					ICollection<X509Certificate> collection = (ICollection<X509Certificate>) cf.generateCertificates(input);
					certificates = collection.ToArray();
					}
			}
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new KeyManagementException("Certificate loading error", e);
			}

			return certificates;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.security.PrivateKey loadPrivateKeyFromPemFile(String keyFilePath) throws java.security.KeyManagementException
		public static PrivateKey loadPrivateKeyFromPemFile(string keyFilePath)
		{
			PrivateKey privateKey = null;

			if (string.ReferenceEquals(keyFilePath, null) || keyFilePath.Length == 0)
			{
				return privateKey;
			}

			try
			{
					using (StreamReader reader = new StreamReader(keyFilePath))
					{
					StringBuilder sb = new StringBuilder();
					string previousLine = "";
					string currentLine = null;
        
					// Skip the first line (-----BEGIN RSA PRIVATE KEY-----)
					reader.ReadLine();
					while (!string.ReferenceEquals((currentLine = reader.ReadLine()), null))
					{
						sb.Append(previousLine);
						previousLine = currentLine;
					}
					// Skip the last line (-----END RSA PRIVATE KEY-----)
        
					KeyFactory kf = KeyFactory.getInstance("RSA");
					KeySpec keySpec = new PKCS8EncodedKeySpec(Base64.Decoder.decode(sb.ToString()));
					privateKey = kf.generatePrivate(keySpec);
					}
			}
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new KeyManagementException("Private key loading error", e);
			}

			return privateKey;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void setupTrustCerts(io.netty.handler.ssl.SslContextBuilder builder, boolean allowInsecureConnection, String trustCertsFilePath) throws java.io.IOException, java.io.FileNotFoundException
		private static void setupTrustCerts(SslContextBuilder builder, bool allowInsecureConnection, string trustCertsFilePath)
		{
			if (allowInsecureConnection)
			{
				builder.trustManager(InsecureTrustManagerFactory.INSTANCE);
			}
			else
			{
				if (!string.ReferenceEquals(trustCertsFilePath, null) && trustCertsFilePath.Length != 0)
				{
					using (FileStream input = new FileStream(trustCertsFilePath, FileMode.Open, FileAccess.Read))
					{
						builder.trustManager(input);
					}
				}
				else
				{
					builder.trustManager((File) null);
				}
			}
		}

		private static void setupKeyManager(SslContextBuilder builder, PrivateKey privateKey, X509Certificate[] certificates)
		{
			builder.keyManager(privateKey, (X509Certificate[]) certificates);
		}

		private static void setupCiphers(SslContextBuilder builder, ISet<string> ciphers)
		{
			if (ciphers != null && ciphers.Count > 0)
			{
				builder.ciphers(ciphers);
			}
		}

		private static void setupProtocols(SslContextBuilder builder, ISet<string> protocols)
		{
			if (protocols != null && protocols.Count > 0)
			{
				builder.protocols(protocols.ToArray());
			}
		}

		private static void setupClientAuthentication(SslContextBuilder builder, bool requireTrustedClientCertOnConnect)
		{
			if (requireTrustedClientCertOnConnect)
			{
				builder.clientAuth(ClientAuth.REQUIRE);
			}
			else
			{
				builder.clientAuth(ClientAuth.OPTIONAL);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.eclipse.jetty.util.ssl.SslContextFactory createSslContextFactory(boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath, String tlsCertificateFilePath, String tlsKeyFilePath, boolean tlsRequireTrustedClientCertOnConnect, boolean autoRefresh, long certRefreshInSec) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContextFactory createSslContextFactory(bool tlsAllowInsecureConnection, string tlsTrustCertsFilePath, string tlsCertificateFilePath, string tlsKeyFilePath, bool tlsRequireTrustedClientCertOnConnect, bool autoRefresh, long certRefreshInSec)
		{
			SslContextFactory sslCtxFactory = null;
			if (autoRefresh)
			{
				sslCtxFactory = new SslContextFactoryWithAutoRefresh(tlsAllowInsecureConnection, tlsTrustCertsFilePath, tlsCertificateFilePath, tlsKeyFilePath, tlsRequireTrustedClientCertOnConnect, 0);
			}
			else
			{
				sslCtxFactory = new SslContextFactory();
				SSLContext sslCtx = createSslContext(tlsAllowInsecureConnection, tlsTrustCertsFilePath, tlsCertificateFilePath, tlsKeyFilePath);
				sslCtxFactory.SslContext = sslCtx;
			}
			if (tlsRequireTrustedClientCertOnConnect)
			{
				sslCtxFactory.NeedClientAuth = true;
			}
			else
			{
				sslCtxFactory.WantClientAuth = true;
			}
			sslCtxFactory.TrustAll = true;
			return sslCtxFactory;
		}

		/// <summary>
		/// <seealso cref="SslContextFactory"/> that auto-refresh SSLContext.
		/// </summary>
		internal class SslContextFactoryWithAutoRefresh : SslContextFactory
		{

			internal readonly DefaultSslContextBuilder sslCtxRefresher;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SslContextFactoryWithAutoRefresh(boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath, String tlsCertificateFilePath, String tlsKeyFilePath, boolean tlsRequireTrustedClientCertOnConnect, long certRefreshInSec) throws javax.net.ssl.SSLException, java.io.FileNotFoundException, java.security.GeneralSecurityException, java.io.IOException
			public SslContextFactoryWithAutoRefresh(bool tlsAllowInsecureConnection, string tlsTrustCertsFilePath, string tlsCertificateFilePath, string tlsKeyFilePath, bool tlsRequireTrustedClientCertOnConnect, long certRefreshInSec) : base()
			{
				sslCtxRefresher = new DefaultSslContextBuilder(tlsAllowInsecureConnection, tlsTrustCertsFilePath, tlsCertificateFilePath, tlsKeyFilePath, tlsRequireTrustedClientCertOnConnect, certRefreshInSec);
			}

			public override SSLContext SslContext
			{
				get
				{
					return sslCtxRefresher.get();
				}
			}
		}
	}

}