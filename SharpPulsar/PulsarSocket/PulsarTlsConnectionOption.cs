using System.Security.Cryptography.X509Certificates;
using static Akka.IO.Inet;

namespace SharpPulsar.PulsarSocket
{
    public class PulsarTlsConnectionOption : SocketOption
    {
        public X509Certificate2 Certificate { get; }
        public bool SuppressValidation { get; }
        public bool ClientAsServer { get; set; }

        public PulsarTlsConnectionOption(X509Certificate2 cert, bool suppressValidation)
        {
            Certificate = cert;
            SuppressValidation = suppressValidation;
        }
    }
}
