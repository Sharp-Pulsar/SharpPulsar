using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;


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
	/// Helper class for the security domain.
	/// </summary>
	public class SecurityUtility
	{

		public static X509Certificate2[] LoadCertificatesFromPemFile(string certFilePath)
		{
			if (ReferenceEquals(certFilePath, null) || certFilePath.Length == 0)
			{
				return null;
			}

			try
            {
                var cert = new X509Certificate2(File.ReadAllBytes(certFilePath));
                return new [] {cert};
            }
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new SecurityException("Certificate loading error", e);
			}
		}
        
        public static RSACryptoServiceProvider LoadPrivateKeyFromPemStream(Stream inStream)
        {
            if (inStream == null)
            {
                return null;
            }

            try
            {
                using var reader = new StreamReader(inStream);
                var sb = new StringBuilder();
                string currentLine = null;

                // Jump to the first line after -----BEGIN [RSA] PRIVATE KEY-----
                while (!reader.ReadLine().StartsWith("-----BEGIN"))
                {
                    reader.ReadLine();
                }

                // Stop (and skip) at the last line that has, say, -----END [RSA] PRIVATE KEY-----
                while (!string.ReferenceEquals((currentLine = reader.ReadLine()), null) && !currentLine.StartsWith("-----END", StringComparison.Ordinal))
                {
                    sb.Append(currentLine);
                }

                using TextReader privateKeyTextReader = new StringReader(sb.ToString());
                var readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();

                var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)readKeyPair.Private);
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(rsaParams);
                return csp;
            }
            catch (Exception e) when (e is GeneralSecurityException || e is IOException || e is OutOfMemoryException)
            {
                throw new Exception("Private key loading error", e);
            }
        }
        public static X509Certificate2[] LoadCertificatesFromPemStream(Stream inStream)
        {
            if (inStream == null)
            {
                return null;
            }
            try
            {
                using var stream = new StreamReader(inStream);
                var pemReader = new PemReader(stream);
                var obj = pemReader.ReadPemObject();
                var c = new X509Certificate2(obj.Content);
                return new[] { c };
            }
            catch (Exception e) when (e is CertificateException || e is IOException)
            {
                throw new CertificateException("Certificate loading error", e);
            }
        }
        public static AsymmetricAlgorithm LoadPrivateKeyFromPemFile(string keyFilePath)
		{
			if (string.ReferenceEquals(keyFilePath, null) || keyFilePath.Length == 0)
			{
				throw new Exception("File path cannot be empty");
			}

			try
			{
				var p = (AsymmetricAlgorithm)GetPrivateKeyFromPemFile(keyFilePath);
				//return p.ExportParameters(true);
                return p;
            }
			catch (Exception e) when (e is SecurityException || e is IOException)
			{
				throw new Exception("Private key loading error", e);
			}
		}
        public static AsymmetricAlgorithm LoadPrivateKeyFromFile(string keyFilePath)
		{
			if (string.ReferenceEquals(keyFilePath, null) || keyFilePath.Length == 0)
			{
				throw new Exception("File path cannot be empty");
			}

			try
            {
                var privateKeyBytes = LoadPrivateKeyBytes(keyFilePath);

                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                return rsa;
            }
			catch (Exception e) when (e is SecurityException || e is IOException)
			{
				throw new Exception("Private key loading error", e);
			}
		}
        private static byte[] LoadPrivateKeyBytes(string keyFile)
        {
            // remove these lines
            // -----BEGIN RSA PRIVATE KEY-----
            // -----END RSA PRIVATE KEY-----
            var pemFileData = File.ReadAllLines(keyFile).Where(x => !x.StartsWith("-"));

            // Join it all together, convert from base64
            var binaryEncoding = Convert.FromBase64String(string.Join(null, pemFileData));

            // this is the private key byte data
            return binaryEncoding;
        }
        private static RSACryptoServiceProvider GetPrivateKeyFromPemFile(string keyFilePath)
        {
            using TextReader privateKeyTextReader = new StringReader(File.ReadAllText(keyFilePath));
            var readKeyPair = (AsymmetricCipherKeyPair)new PemReader(privateKeyTextReader).ReadObject();

            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)readKeyPair.Private);
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);
            return csp;
        }
        
		
	}

}