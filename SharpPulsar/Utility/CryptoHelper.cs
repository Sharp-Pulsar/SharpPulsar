using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace SharpPulsar.Utility
{
    internal static class CryptoHelper
    {

        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString((sbyte[])(object)key).Trim()));
            var keys = (RsaKeyParameters)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            var eng = new OaepEncoding(new RsaEngine());
            eng.Init(true, keys);

            var length = data.Length;
            
            var blockSize = eng.GetInputBlockSize();
            var encryptedData = new List<byte>();
            for (var chunkPosition = 0; chunkPosition < length; chunkPosition += blockSize)
            {
                var chunkSize = Math.Min(blockSize, length - chunkPosition);
                encryptedData.AddRange(eng.ProcessBlock(data, chunkPosition, chunkSize));
            }
            return encryptedData.ToArray();
        }

        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString((sbyte[])(object)key).Trim()));
            var keys = (AsymmetricCipherKeyPair)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            var eng = new OaepEncoding(new RsaEngine());
            eng.Init(false, keys.Private);

            var length = data.Length;
            var blockSize = eng.GetInputBlockSize();
            var decryptedData = new List<byte>();
            for (var chunkPosition = 0; chunkPosition < length; chunkPosition += blockSize)
            {
                var chunkSize = Math.Min(blockSize, length - chunkPosition);
                decryptedData.AddRange(eng.ProcessBlock(data, chunkPosition, chunkSize));
            }
            return decryptedData.ToArray();
        }
        public static RSACryptoServiceProvider GetRsaProviderFromPem(string pemstr)
        {
            var rsaKey = new RSACryptoServiceProvider();

            RSACryptoServiceProvider MakePublicRcsp(RSACryptoServiceProvider rcsp, RsaKeyParameters rkp)
            {
                var rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            }

            Func<RSACryptoServiceProvider, RsaPrivateCrtKeyParameters, RSACryptoServiceProvider> MakePrivateRCSP = (rcsp, rkp) =>
            {
                var rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
                rcsp.ImportParameters(rsaParameters);
                return rsaKey;
            };

            var reader = new PemReader(new StringReader(pemstr));
            var kp = reader.ReadObject();

            // If object has Private/Public property, we have a Private PEM
            return (kp.GetType() == typeof(RsaPrivateCrtKeyParameters)) ? MakePrivateRCSP(rsaKey, (RsaPrivateCrtKeyParameters)kp) : MakePublicRcsp(rsaKey, (RsaKeyParameters)kp);
        }
        public static byte[] Encrypt(byte[] key, byte[] payload, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 256;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(payload);
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var output = new MemoryStream();
            cryptoStream.CopyTo(output);
            return output.ToArray();
        }

        public static byte[] Decrypt(byte[] key, byte[] payload, byte[] iv)
        {
            //byte[] iv = new byte[16];
            //byte[] buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 256;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(payload);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var output = new MemoryStream();
            cryptoStream.CopyTo(output);
            return output.ToArray();
        }
    }
}
