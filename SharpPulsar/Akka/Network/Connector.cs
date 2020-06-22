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
using System.Linq;
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
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _verifyCertificateAuthority;
        private readonly bool _verifyCertificateName;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private string _serviceSniName;

        public Connector(ClientConfigurationData conf)
        {
            if(conf.TlsTrustCerts != null)
                _clientCertificates = conf.TlsTrustCerts;
            if(conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;
            _verifyCertificateAuthority = conf.VerifyCertificateAuthority;
            _verifyCertificateName = conf.VerifyCertificateName;
            _encrypt = conf.UseTls;
            _serviceUrl = conf.ServiceUrl;
            _clientConfiguration = conf;
        }

        public Stream Connect(Uri endPoint, string hostName)
        {
            var host = endPoint.Host;
            _serviceSniName = hostName;
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
                if (!SniProxy)
                    tcpClient.Connect(endPoint.Host, endPoint.Port);
                else
                {
                    var proxy = new Uri(_clientConfiguration.ProxyServiceUrl);
                    tcpClient.Connect(proxy.Host, proxy.Port);
                }
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
                sslStream = new SslStream(stream, false, ValidateServerCertificate, null);
                sslStream.AuthenticateAsClient(host, _clientCertificates, SslProtocols.Tls12, false);
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
        private bool SniProxy => _encrypt && _clientConfiguration.ProxyProtocol != null && !string.IsNullOrWhiteSpace(_clientConfiguration.ProxyServiceUrl);

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool result;
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.None:
                    result = true;
                    break;
                case SslPolicyErrors.RemoteCertificateChainErrors:

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
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    var cert = new X509Certificate2(certificate);
                    var cn = cert.GetNameInfo(X509NameType.SimpleName, false);
                    var cleanName = cn?.Substring(cn.LastIndexOf('*') + 1);
                    string[] addresses = { _serviceUrl, _serviceSniName };

                    // if the ending of the sni and servername do match the common name of the cert, fail
                    result = addresses.Count(item => cleanName != null && item.EndsWith(cleanName)) == addresses.Count();
                    break;

                default:
                    result = false;
                    break;
            }

            return result;
        }
    }
}
