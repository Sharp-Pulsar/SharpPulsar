using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpPulsar.Common;

namespace SharpPulsar.Utility
{
    //https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
    //http://www.idc-online.com/technical_references/pdfs/information_technology/Bouncy_Castle_Net_Implementation_RSA_Algorithm.pdf
    //https://gist.github.com/saldoukhov/59cbf22745bb45c17461dfb5986fca62
    //https://gist.github.com/dziwoki/cc41b523c2bd43ee646b957f0aa91943
    //https://stackoverflow.com/questions/41607885/the-input-data-is-not-a-complete-block-when-decrypting-using-aes
    public static class CryptoHelper
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] encodingParam)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString(key).Trim()));
            var keys = (RsaKeyParameters)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            var eng = new OaepEncoding(new RsaEngine(), new Sha256Digest(), encodingParam);
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
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString(key).Trim()));
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

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] encodingParam)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString(key).Trim()));
            var keys = (AsymmetricCipherKeyPair)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            var eng = new OaepEncoding(new RsaEngine(),new Sha256Digest(), encodingParam);
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
        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            var pr = new PemReader(new StringReader(StringHelper.NewString(key).Trim()));
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
        public static byte[] Encrypt(byte[] key, byte[] data, byte[] iv, int keySize)
        {
            byte[] output;
            using (var aes = Aes.Create())
            {
                //BlockSize = 128,
                aes.KeySize = keySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.ISO10126;

                using var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                output = encrypt.TransformFinalBlock(data, 0, data.Length);
                return output;
            };
        }

        public static byte[] Decrypt(byte[] key, byte[] data, byte[] iv, int keySize)
        {
            byte[] output;
            using (var aes = Aes.Create())
            {
                //BlockSize=128,
                aes.KeySize = keySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.ISO10126;
                using var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
                output = decrypt.TransformFinalBlock(data, 0, data.Length);
                return output;
            };
        }
    }
}
