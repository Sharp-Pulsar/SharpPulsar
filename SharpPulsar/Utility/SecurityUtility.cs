using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
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
                using var input = new FileStream(certFilePath, FileMode.Open, FileAccess.Read);
                using var stream = new StreamReader(input);
                var c = new X509Certificate2();
                var pemReader = new PemReader(stream);
                var obj = pemReader.ReadPemObject();
                c.Import(obj.Content);
                return new []{c};
            }
			catch (Exception e) when (e is GeneralSecurityException || e is IOException)
			{
				throw new SecurityException("Certificate loading error", e);
			}
		}
        /// <summary>
        /// Retrieves the certificate from Pem format.
        /// </summary>
        /// <param name="certificateText">Certificate in Pem format</param>
        /// <returns>An X509 certificate</returns>
        public X509Certificate ImportCertificate(string certificateText)
        {
            using var textReader = new StringReader(certificateText);
            var pemReader = new PemReader(textReader);
            var certificate = (X509Certificate)pemReader.ReadObject();
            return certificate;
        }
        /// <summary>
        /// Retrieves the key pair from PEM format
        /// </summary>
        /// <param name="keyPairPemText">Key pair in pem format</param>
        /// <returns>Key pair</returns>
        public AsymmetricCipherKeyPair ImportKeyPair(string keyPairPemText)
        {
            using var textReader = new StringReader(keyPairPemText);
            var pemReader = new PemReader(textReader);
            var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            return keyPair;
        }
        private RsaKeyParameters GenerateKeysFromPem(byte[] rawData)
        {
            var pem = new PemReader(new StreamReader(new MemoryStream(rawData)));
            var keyPair = (RsaKeyParameters)pem.ReadObject();
            return keyPair;
        }
        public Pkcs10CertificationRequest LoadCertificate(string pemFilenameCsr)
        {
            var textReader = File.OpenText(pemFilenameCsr);
            var reader = new PemReader(textReader);
            return reader.ReadObject() as Pkcs10CertificationRequest;
        }
        public static AsymmetricKeyParameter ImportPublicFromPem(string pub)
        {
            AsymmetricKeyParameter pubkey;

            using (var textReader = new StringReader(pub))
            {
                var pemReader = new PemReader(textReader);
                pubkey = (AsymmetricKeyParameter)pemReader.ReadObject();
            }

            return pubkey;
        }
        public static AsymmetricAlgorithm LoadPrivateKeyFromPemStream(Stream inStream)
        {
            PrivateKey privateKey = null;

            if (inStream == null)
            {
                return privateKey;
            }

            try
            {
                using (StreamReader reader = new StreamReader(inStream))
                {
                    if (inStream.markSupported())
                    {
                        inStream.reset();
                    }
                    StringBuilder sb = new StringBuilder();
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
        public static X509Certificate2[] LoadCertificatesFromPemStream(Stream inStream)
        {
            if (inStream == null)
            {
                return null;
            }
            CertificateFactory cf;
            try
            {
                if (inStream.markSupported())
                {
                    inStream.reset();
                }
                cf = CertificateFactory.getInstance("X.509");
                ICollection<X509Certificate> collection = (ICollection<X509Certificate>)cf.generateCertificates(inStream);
                return collection.ToArray();
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
        /// <summary>
        /// Reads the PEM key file and returns the object.
        /// </summary>
        /// <param name="fileName">Path to the pem file</param>
        /// <returns>the read object which may be of different key types</returns>
        /// <exception cref="FormatException">Thrown if the key is not in PEM format</exception>
        private static object ReadPem(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("The key file does not exist: " + fileName);

            using var file = new StreamReader(fileName);
            var pRd = new PemReader(file);

            var obj = pRd.ReadObject();
            pRd.Reader.Close();
            if (obj == null)
            {
                throw new FormatException("The key file " + fileName + " is no valid PEM format");
            }
            return obj;
        }
        public static byte[] Decrypt(byte[] buffer)
        {
            using TextReader sr = new StringReader(""/*PRIVATE_KEY*/);
            var pemReader = new PemReader(sr);
            var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            var privateKey = (RsaKeyParameters)keyPair.Private;
            IAsymmetricBlockCipher cipher = new Pkcs1Encoding(new RsaEngine());

            cipher.Init(false, privateKey);
            return cipher.ProcessBlock(buffer, 0, buffer.Length);
        }
        public static AsymmetricKeyParameter ReadRsaPrivateKey(string path)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName != null && fileName.Contains("_private"))
                {
                    AsymmetricCipherKeyPair key;
                    TextReader tr = new StreamReader(path);
                    var pr = new PemReader(tr);
                    key = (AsymmetricCipherKeyPair)pr.ReadObject();
                    pr.Reader.Close();
                    tr.Close();
                    return key.Private;
                }
                else
                {
                    return null;
                }
            }
            catch (InvalidCastException e)
            {
                return null;
            }
        }
        public static AsymmetricKeyParameter ReadRsaPublicKey(string path)
        {
            try
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (fileName != null && fileName.Contains("_public"))
                {
                    RsaKeyParameters rsaKey;
                    TextReader tr = new StreamReader(path);
                    var pr = new PemReader(tr);
                    rsaKey = (RsaKeyParameters)pr.ReadObject();
                    var key = (AsymmetricKeyParameter)rsaKey;
                    pr.Reader.Close();
                    tr.Close();
                    return key;
                }

                return null;
            }
            catch (InvalidCastException e)
            {
                return null;
            }
        }
        internal AsymmetricCipherKeyPair GetKeyPair(string key)
        {
            var reader = new StringReader(key);
            var pem = new PemReader(reader);
            var o = pem.ReadObject();

            return (AsymmetricCipherKeyPair)o;
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

		public static RSA GetPublicKeyFromPemFile(string filePath)
        {
            using TextReader publicKeyTextReader = new StringReader(File.ReadAllText(filePath));
            var publicKeyParam = (RsaKeyParameters)new PemReader(publicKeyTextReader).ReadObject();

            var rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParam);

            var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }
		
	}

}