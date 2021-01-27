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
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SharpPulsar.Configuration;

namespace SharpPulsar.Akka.Network
{
    public sealed class Connector
    {
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2? _trustedCertificateAuthority;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private string _targetServerName;

        public Connector(ClientConfigurationData conf)
        {
            if (conf.ClientCertificates != null)
                _clientCertificates = conf.ClientCertificates;
            if (conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;
            _encrypt = conf.UseTls;
            _serviceUrl = conf.ServiceUrl;
            _clientConfiguration = conf;
        }

        public Stream Connect(Uri endPoint, string hostName)
        {
            var host = endPoint.Host;
            _targetServerName = hostName;
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
                if (SniProxy)
                    endPoint = new Uri(_clientConfiguration.ProxyServiceUrl);

                if (!_encrypt)
                {
                    tcpClient.Connect(endPoint.Host, endPoint.Port);
                    return tcpClient.GetStream();
                }
                
                var addressesTask = Dns.GetHostAddressesAsync(endPoint.Host);
                var addresses = SynchronizationContextSwitcher.NoContext(async () => await addressesTask).Result;
                var socketTask = ConnectAsync(addresses, endPoint.Port);
                var socket = SynchronizationContextSwitcher.NoContext(async () => await socketTask).Result;
                return new NetworkStream(socket, true);
            }
            catch (Exception ex)
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
                sslStream = new SslStream(stream, true, ValidateServerCertificate, null);
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
        //https://github.com/LukeInkster/CSharpCorpus/blob/919b7525a61eb6b475fbcba0d87fd3cb44ef3b38/corefx/src/System.Data.SqlClient/src/System/Data/SqlClient/SNI/SNITcpHandle.cs
        private static async Task<Socket> ConnectAsync(IPAddress[] serverAddresses, int port)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(serverAddresses, port);
                return socket;
            }

            // On unix we can't use the instance Socket methods that take multiple endpoints

            if (serverAddresses == null)
            {
                throw new ArgumentNullException(nameof(serverAddresses));
            }
            if (serverAddresses.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(serverAddresses));
            }

            // Try each address in turn, and return the socket opened for the first one that works.
            ExceptionDispatchInfo lastException = null;
            foreach (IPAddress address in serverAddresses)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(address, port).ConfigureAwait(false);
                    return socket;
                }
                catch (Exception exc)
                {
                    socket.Dispose();
                    lastException = ExceptionDispatchInfo.Capture(exc);
                }
            }

            // Propagate the last failure that occurred
            if (lastException != null)
            {
                lastException.Throw();
            }

            // Should never get here.  Either there will have been no addresses and we'll have thrown
            // at the beginning, or one of the addresses will have worked and we'll have returned, or
            // at least one of the addresses will failed, in which case we will have propagated that.
            throw new ArgumentException();
        }

        private bool SniProxy => _clientConfiguration.ProxyProtocol != null && !string.IsNullOrWhiteSpace(_clientConfiguration.ProxyServiceUrl);

        /*private bool ValidateServerCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                var certServerName = cert.Subject.Substring(cert.Subject.IndexOf('=') + 1);

                // Verify that target server name matches subject in the certificate
                if (_targetServerName.Length > certServerName.Length)
                {
                    return false;
                }
                if (_targetServerName.Length == certServerName.Length)
                {
                    // Both strings have the same Length, so targetServerName must be a FQDN
                    if (!_targetServerName.Equals(certServerName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    if (string.Compare(_targetServerName, 0, certServerName, 0, _targetServerName.Length, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return false;
                    }

                    // Server name matches cert name for its whole Length, so ensure that the
                    // character following the server name is a '.'. This will avoid
                    // having server name "ab" match "abc.corp.company.com"
                    // (Names have different lengths, so the target server can't be a FQDN.)
                    if (certServerName[_targetServerName.Length] != '.')
                    {
                        return false;
                    }
                }
            }
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                if (_trustedCertificateAuthority is null)
                    return false;

                chain.ChainPolicy.ExtraStore.Add(_trustedCertificateAuthority);
                _ = chain.Build((X509Certificate2)cert);
                for (var i = 0; i < chain.ChainElements.Count; i++)
                {
                    if (chain.ChainElements[i].Certificate.Thumbprint == _trustedCertificateAuthority.Thumbprint)
                        return true;
                }
                return false;
            }
            else
            {
                // Fail all other SslPolicy cases besides RemoteCertificateNameMismatch
                return false;
            }
            return true;
        }*/
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
                    string[] addresses = { _serviceUrl, _targetServerName };

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
