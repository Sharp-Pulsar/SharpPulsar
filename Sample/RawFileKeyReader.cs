using System;
using System.Collections.Generic;
using System.IO;
using SharpPulsar.Api;
using SharpPulsar.Shared;

namespace Samples
{
    public class RawFileKeyReader: ICryptoKeyReader
    {
        private string _publicKeyFile;
        private string _privateKeyFile;

        public RawFileKeyReader(string publicKeyFile, string privateKeyFile)
        {
            _publicKeyFile = publicKeyFile;
            _privateKeyFile = privateKeyFile;
        }

        public EncryptionKeyInfo GetPublicKey(string keyName, IDictionary<string, string> metadata)
        {
            EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
            try
            {
                keyInfo.Key = (sbyte[]) (object) File.ReadAllBytes(Path.GetFullPath(_publicKeyFile));
                keyInfo.Metadata = metadata;
            }
            catch (IOException e)
            {
                Console.WriteLine($"ERROR: Failed to read public key from file {_publicKeyFile}");
            }
            return keyInfo;
        }

        public EncryptionKeyInfo GetPrivateKey(string keyName, IDictionary<string, string> metadata)
        {
            EncryptionKeyInfo keyInfo = new EncryptionKeyInfo();
            try
            {
                keyInfo.Key = (sbyte[])(object)File.ReadAllBytes(Path.GetFullPath(_privateKeyFile));
                keyInfo.Metadata = metadata;
            }
            catch (IOException e)
            {
                Console.WriteLine($"ERROR: Failed to read public key from file {_publicKeyFile}");
            }
            return keyInfo;
        }
    }
}
