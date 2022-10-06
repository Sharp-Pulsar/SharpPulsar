using Org.BouncyCastle.Security;
using SharpPulsar.Utility;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace SharpPulsar.Test.API
{
    public class CipherHelper
    {
        private readonly ITestOutputHelper _output;

        public CipherHelper(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void EncryDecrpt()
        {
            var t = "Hello Word!";
            var keyGen = new AesManaged();
            var rand = new SecureRandom();
            var ivLen = 256;
            //var iv = 16;
            var tag = 256;
            //keyGen.KeySize = tag;
            keyGen.GenerateIV();
            keyGen.GenerateKey();
            var iv = keyGen.IV;
            var key = keyGen.Key;
            var bytes = Encoding.UTF8.GetBytes(t);
            var encBytes = CryptoHelper.Encrypt(key, bytes, iv, tag);
            var deBytes = CryptoHelper.Decrypt(key, encBytes, iv, tag);
            for (var i = 0; i < bytes.Length; i++)
            {
                _output.WriteLine($"{bytes[i]} : {deBytes[i]}");
                Assert.Equal(bytes[i], deBytes[i]);
            }
            _output.WriteLine(Convert.ToBase64String(deBytes));
        }
    }
}
