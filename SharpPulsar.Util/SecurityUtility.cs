using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Cryptography;
using DotNetty.Handlers.Tls;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
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
namespace SharpPulsar.Util
{
	using ClientAuth =  io.netty.handler.ssl.ClientAuth;
	using SslContext = DotNetty.Handlers.Tls.TlsHandler io.netty.handler.ssl.SslContext;
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
			//java.security.Security.addProvider(new org.bouncycastle.jce.provider.BouncyCastleProvider());
		}

		public static SslContext CreateSslContext(bool AllowInsecureConnection, X509Certificate[] TrustCertificates)
		{
			return CreateSslContext(AllowInsecureConnection, TrustCertificates, (X509Certificate[])null, (AsymmetricAlgorithm)null);
		}

		
		public static SslContext CreateNettySslContextForClient(bool AllowInsecureConnection, string TrustCertsFilePath)
		{
			return CreateNettySslContextForClient(AllowInsecureConnection, TrustCertsFilePath, (X509Certificate[])null, (PrivateKey)null);
		}

		
		public static SslContext CreateSslContext(bool AllowInsecureConnection, string TrustCertsFilePath, string CertFilePath, string KeyFilePath)
		{
			X509Certificate[] TrustCertificates = LoadCertificatesFromPemFile(TrustCertsFilePath);
			X509Certificate[] Certificates = LoadCertificatesFromPemFile(CertFilePath);
			var pri = LoadPrivateKeyFromPemFile(KeyFilePath);
			_publicKey = pub.ExportParameters(false);
			_privateKey = pri.ExportParameters(true);
			PrivateKey PrivateKey = LoadPrivateKeyFromPemFile(KeyFilePath);
			return CreateSslContext(AllowInsecureConnection, TrustCertificates, Certificates, PrivateKey);
		}

		
		public static SslContext CreateNettySslContextForClient(bool AllowInsecureConnection, string TrustCertsFilePath, string CertFilePath, string KeyFilePath)
		{
			X509Certificate[] Certificates = LoadCertificatesFromPemFile(CertFilePath);
			PrivateKey PrivateKey = LoadPrivateKeyFromPemFile(KeyFilePath);
			return CreateNettySslContextForClient(AllowInsecureConnection, TrustCertsFilePath, Certificates, PrivateKey);
		}

		
		public static SslContext CreateNettySslContextForClient(bool AllowInsecureConnection, string TrustCertsFilePath, X509Certificate[] Certificates, PrivateKey PrivateKey)
		{
			SslContextBuilder Builder = SslContextBuilder.forClient();
			SetupTrustCerts(Builder, AllowInsecureConnection, TrustCertsFilePath);
			SetupKeyManager(Builder, PrivateKey, (X509Certificate[])Certificates);
			return Builder.build();
		}

		public static SslContext CreateNettySslContextForServer(bool AllowInsecureConnection, string TrustCertsFilePath, string CertFilePath, string KeyFilePath, ISet<string> Ciphers, ISet<string> Protocols, bool RequireTrustedClientCertOnConnect)
		{
			X509Certificate[] Certificates = LoadCertificatesFromPemFile(CertFilePath);
			PrivateKey PrivateKey = LoadPrivateKeyFromPemFile(KeyFilePath).ex;

			SslContextBuilder Builder = SslContextBuilder.forServer(PrivateKey, (X509Certificate[])Certificates);
			SetupCiphers(Builder, Ciphers);
			SetupProtocols(Builder, Protocols);
			SetupTrustCerts(Builder, AllowInsecureConnection, TrustCertsFilePath);
			SetupKeyManager(Builder, PrivateKey, Certificates);
			SetupClientAuthentication(Builder, RequireTrustedClientCertOnConnect);
			return Builder.build();
		}

		
		public static SslContext CreateSslContext(bool AllowInsecureConnection, X509Certificate[] TrustCertficates, X509Certificate[] Certificates, PrivateKey PrivateKey)
		{
			KeyStoreHolder Ksh = new KeyStoreHolder();
			TrustManager[] TrustManagers = null;
			KeyManager[] KeyManagers = null;

			TrustManagers = SetupTrustCerts(Ksh, AllowInsecureConnection, TrustCertficates);
			KeyManagers = SetupKeyManager(Ksh, PrivateKey, Certificates);

			SSLContext SslCtx = SSLContext.getInstance("TLS");
			SslCtx.init(KeyManagers, TrustManagers, new SecureRandom());
			SslCtx.DefaultSSLParameters;
			return SslCtx;
		}

		
		private static KeyManager[] SetupKeyManager(KeyStoreHolder Ksh, PrivateKey PrivateKey, X509Certificate[] Certificates)
		{
			KeyManager[] KeyManagers = null;
			if (Certificates != null && PrivateKey != null)
			{
				Ksh.setPrivateKey("private", PrivateKey, Certificates);
				KeyManagerFactory Kmf = KeyManagerFactory.getInstance(KeyManagerFactory.DefaultAlgorithm);
				Kmf.init(Ksh.KeyStore, "".ToCharArray());
				KeyManagers = Kmf.KeyManagers;
			}
			return KeyManagers;
		}

		private static TrustManager[] SetupTrustCerts(KeyStoreHolder Ksh, bool AllowInsecureConnection, X509Certificate[] TrustCertficates)
		{
			TrustManager[] TrustManagers;
			if (AllowInsecureConnection)
			{
				TrustManagers = InsecureTrustManagerFactory.INSTANCE.TrustManagers;
			}
			else
			{
				TrustManagerFactory Tmf = TrustManagerFactory.getInstance(TrustManagerFactory.DefaultAlgorithm);

				if (TrustCertficates == null || TrustCertficates.Length == 0)
				{
					Tmf.init((KeyStore)null);
				}
				else
				{
					for (int I = 0; I < TrustCertficates.Length; I++)
					{
						Ksh.setCertificate("trust" + I, TrustCertficates[I]);
					}
					Tmf.init(Ksh.KeyStore);
				}

				TrustManagers = Tmf.TrustManagers;
			}
			return TrustManagers;
		}

		public static X509Certificate[] LoadCertificatesFromPemFile(string CertFilePath)
		{
			X509Certificate[] Certificates = null;

			if (string.ReferenceEquals(CertFilePath, null) || CertFilePath.Length == 0)
			{
				return Certificates;
			}

			try
			{
				using (FileStream Input = new FileStream(CertFilePath, FileMode.Open, FileAccess.Read))
				{
					Pkcs12Store store = new Pkcs12Store();
					CertificateFactory Cf = CertificateFactory.getInstance("X.509");
					ICollection<X509Certificate> Collection = (ICollection<X509Certificate>)Cf.generateCertificates(Input);
					Certificates = Collection.ToArray();
				}
			}
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new KeyManagementException("Certificate loading error", e);
			}

			return Certificates;
		}

		public static RSAParameters LoadPrivateKeyFromPemFile(string KeyFilePath)
		{
			if (string.ReferenceEquals(KeyFilePath, null) || KeyFilePath.Length == 0)
			{
				throw new Exception("File path cannot be empty");
			}

			try
			{
				var p = GetPrivateKeyFromPemFile(KeyFilePath);
				return p.ExportParameters(true);
			}
			catch (Exception e) when (e is SecurityException || e is IOException)
			{
				throw new Exception("Private key loading error", e);
			}
		}
		private static RSACryptoServiceProvider GetPrivateKeyFromPemFile(string KeyFilePath)
		{
			using (TextReader privateKeyTextReader = new StringReader(File.ReadAllText(KeyFilePath)))
			{
				AsymmetricCipherKeyPair readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();

				RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)readKeyPair.Private);
				RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
				csp.ImportParameters(rsaParams);
				return csp;
			}
		}

		public static RSACryptoServiceProvider GetPublicKeyFromPemFile(String filePath)
		{
			using (TextReader publicKeyTextReader = new StringReader(File.ReadAllText(filePath)))
			{
				RsaKeyParameters publicKeyParam = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

				RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKeyParam);

				RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
				csp.ImportParameters(rsaParams);
				return csp;
			}
		}
		
		private static void SetupTrustCerts(SslContextBuilder Builder, bool AllowInsecureConnection, string TrustCertsFilePath)
		{
			if (AllowInsecureConnection)
			{
				Builder.trustManager(InsecureTrustManagerFactory.INSTANCE);
			}
			else
			{
				if (!string.ReferenceEquals(TrustCertsFilePath, null) && TrustCertsFilePath.Length != 0)
				{
					using (FileStream Input = new FileStream(TrustCertsFilePath, FileMode.Open, FileAccess.Read))
					{
						Builder.trustManager(Input);
					}
				}
				else
				{
					Builder.trustManager((File)null);
				}
			}
		}

		private static void SetupKeyManager(SslContextBuilder Builder, PrivateKey PrivateKey, X509Certificate[] Certificates)
		{
			Builder.keyManager(PrivateKey, (X509Certificate[])Certificates);
		}

		private static void SetupCiphers(SslContextBuilder Builder, ISet<string> Ciphers)
		{
			if (Ciphers != null && Ciphers.Count > 0)
			{
				Builder.ciphers(Ciphers);
			}
		}

		private static void SetupProtocols(SslContextBuilder Builder, ISet<string> Protocols)
		{
			if (Protocols != null && Protocols.Count > 0)
			{
				Builder.protocols(Protocols.ToArray());
			}
		}

		private static void SetupClientAuthentication(SslContextBuilder Builder, bool RequireTrustedClientCertOnConnect)
		{
			if (RequireTrustedClientCertOnConnect)
			{
				Builder.clientAuth(ClientAuth.REQUIRE);
			}
			else
			{
				Builder.clientAuth(ClientAuth.OPTIONAL);
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public static org.eclipse.jetty.util.ssl.SslContextFactory createSslContextFactory(boolean tlsAllowInsecureConnection, String tlsTrustCertsFilePath, String tlsCertificateFilePath, String tlsKeyFilePath, boolean tlsRequireTrustedClientCertOnConnect, boolean autoRefresh, long certRefreshInSec) throws java.security.GeneralSecurityException, javax.net.ssl.SSLException, java.io.FileNotFoundException, java.io.IOException
		public static SslContextFactory CreateSslContextFactory(bool TlsAllowInsecureConnection, string TlsTrustCertsFilePath, string TlsCertificateFilePath, string TlsKeyFilePath, bool TlsRequireTrustedClientCertOnConnect, bool AutoRefresh, long CertRefreshInSec)
		{
			SslContextFactory SslCtxFactory = null;
			if (AutoRefresh)
			{
				SslCtxFactory = new SslContextFactoryWithAutoRefresh(TlsAllowInsecureConnection, TlsTrustCertsFilePath, TlsCertificateFilePath, TlsKeyFilePath, TlsRequireTrustedClientCertOnConnect, 0);
			}
			else
			{
				SslCtxFactory = new SslContextFactory();
				SSLContext SslCtx = CreateSslContext(TlsAllowInsecureConnection, TlsTrustCertsFilePath, TlsCertificateFilePath, TlsKeyFilePath);
				SslCtxFactory.SslContext = SslCtx;
			}
			if (TlsRequireTrustedClientCertOnConnect)
			{
				SslCtxFactory.NeedClientAuth = true;
			}
			else
			{
				SslCtxFactory.WantClientAuth = true;
			}
			SslCtxFactory.TrustAll = true;
			return SslCtxFactory;
		}

		/// <summary>
		/// <seealso cref="SslContextFactory"/> that auto-refresh SSLContext.
		/// </summary>
		public class SslContextFactoryWithAutoRefresh : SslContextFactory
		{

			internal readonly DefaultSslContextBuilder SslCtxRefresher;
			public SslContextFactoryWithAutoRefresh(bool TlsAllowInsecureConnection, string TlsTrustCertsFilePath, string TlsCertificateFilePath, string TlsKeyFilePath, bool TlsRequireTrustedClientCertOnConnect, long CertRefreshInSec) : base()
			{
				SslCtxRefresher = new DefaultSslContextBuilder(TlsAllowInsecureConnection, TlsTrustCertsFilePath, TlsCertificateFilePath, TlsKeyFilePath, TlsRequireTrustedClientCertOnConnect, CertRefreshInSec);
			}

			public override TlsHandler SslContext
			{
				get
				{
					return SslCtxRefresher.Get();

				}
			}
		}
	}

}