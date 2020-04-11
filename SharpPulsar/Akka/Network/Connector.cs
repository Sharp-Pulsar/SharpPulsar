/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
//https://docs.microsoft.com/en-us/azure/aks/ingress-basic
//https://docs.microsoft.com/en-us/azure/aks/ingress-static-ip
//https://docs.microsoft.com/en-us/azure/dns/dns-getstarted-powershell
//https://docs.microsoft.com/en-us/azure/dns/dns-getstarted-cli
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Network
{
    public sealed class Connector
    {
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2? _trustedCertificateAuthority;
        private readonly bool _verifyCertificateAuthority;
        private readonly bool _verifyCertificateName;
        private bool _encrypt;

        public Connector(ClientConfigurationData conf)
        {
            if(conf.TlsTrustCerts != null)
                _clientCertificates = conf.TlsTrustCerts;
            if(conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;
            _verifyCertificateAuthority = conf.VerifyCertificateAuthority;
            _verifyCertificateName = conf.VerifyCertificateName;
            _encrypt = conf.UseTls;
        }

        public Stream Connect(Uri endPoint)
        {
            var host = endPoint.Host;
            var stream = GetStream(endPoint);

            if (_encrypt)
                stream = EncryptStream(stream, host);

            return stream;
        }

        private Stream GetStream(Uri endPoint)
        {
            var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(endPoint.Host, endPoint.Port);
                return tcpClient.GetStream();
            }
            catch(Exception ex)
            {
                tcpClient.Dispose();
                throw;
            }
        }

        private Stream EncryptStream(Stream stream, string host)
        {
            SslStream sslStream = null;

            try
            {
                sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                sslStream.AuthenticateAsClient(host, _clientCertificates, SslProtocols.None, true);
                return sslStream;
            }
            catch
            {
                if (sslStream is null)
                    stream.Dispose();
                else
                    sslStream.Dispose();

                throw;
            }
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
                return false;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch) && _verifyCertificateName)
                return false;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) && _verifyCertificateAuthority)
            {
                if (_trustedCertificateAuthority is null)
                    return false;

                chain.ChainPolicy.ExtraStore.Add(_trustedCertificateAuthority);
                _ = chain.Build((X509Certificate2)certificate);
                for (var i = 0; i < chain.ChainElements.Count; i++)
                {
                    if (chain.ChainElements[i].Certificate.Thumbprint == _trustedCertificateAuthority.Thumbprint)
                        return true;
                }

                return false;
            }

            return true;
        }
    }
}
