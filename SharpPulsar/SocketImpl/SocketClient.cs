using Akka.Event;
using SharpPulsar.Impl.Conf;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    //https://github.com/fzf003/TinyService/blob/242c38c06ef7cb685934ba653041d07c64a6f806/Src/TinyServer.ReactiveSocket/SocketAcceptClient.cs
    public sealed class SocketClient: ISocketClient
    {
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2? _trustedCertificateAuthority;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private string _targetServerName;
        private readonly Socket _socket;

        private readonly Stream _networkstream;

        private readonly PipeReader _pipeReader;

        private readonly PipeWriter _pipeWriter;

        private readonly TimeSpan heartbeatttimespan = TimeSpan.FromMilliseconds(800);

        private readonly ILoggingAdapter _logger;

        private readonly Heartbeat _heartbeat;

        private readonly string _connectonId = string.Empty;

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        public static ISocketClient CreateClient(ClientConfigurationData conf, Uri server, string hostName, ILoggingAdapter logger)
        {
            return new SocketClient(conf, server, hostName, logger);
        }
        internal SocketClient(ClientConfigurationData conf, Uri server, string hostName, ILoggingAdapter logger)
        {
            if (conf.ClientCertificates != null)
                _clientCertificates = conf.ClientCertificates;

            if (conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;

            _encrypt = conf.UseTls;

            _serviceUrl = conf.ServiceUrl;

            _clientConfiguration = conf;

            _logger = logger;

            var host = server.Host;

            _targetServerName = hostName;

            var networkStream = GetStream(server);

            if (_encrypt)
                networkStream = EncryptStream(networkStream, host);

            var addresses = Dns.GetHostAddresses(server.Host);
            var endPoint = new IPEndPoint(addresses[0], server.Port);
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            _heartbeat = new Heartbeat(_socket, logger);

            _heartbeat.OnDisconnect += HeartbeatProcess;

            _networkstream = networkStream;

            _pipeReader = PipeReader.Create(_networkstream);

            _pipeWriter = PipeWriter.Create(_networkstream);

            _heartbeat.Start();

            _connectonId = $"{_socket.RemoteEndPoint}";

        }

        public string RemoteConnectionId
        {
            get
            {
                return _connectonId;
            }
        }

        void HeartbeatProcess(HeartbeatEvent @event)
        {
            _logger.Debug($"{DateTime.Now}----{RemoteConnectionId} Disconnect....");
        }


        public IObservable<ReadOnlySequence<byte>> RevicedObservable =>
               Observable.Create<ReadOnlySequence<byte>>((observer) => ReaderSchedule(observer, cancellation.Token));


        IDisposable ReaderSchedule(IObserver<ReadOnlySequence<byte>> observer, CancellationToken cancellationToken = default)
        {
            return NewThreadScheduler.Default.Schedule(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {

                        var readresult = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                        var buffer = readresult.Buffer;

                        if (buffer.Length <= 0)
                        {
                            continue;
                        }

                        while (ContainsLine(ref buffer, out ReadOnlySequence<byte> line))
                        {
                            ProcessLine(line, observer);

                        }
                        //Move the pointer to the head position of the next data.
                        _pipeReader.AdvanceTo(buffer.Start, buffer.End);


                        if (readresult.IsCompleted)
                        {
                            break;
                        }

                    }
                    catch
                    {
                        break;
                    }
                }

                _pipeReader?.Complete();
                observer.OnCompleted();

            });
        }

        public Task SendMessageAsync(byte[] message)
        {
            return _pipeWriter.SendMessageAsync(message).AsTask();
        }

        public Task SendMessageAsync(string message)
        {
            return _pipeWriter.SendAsync(message.ToMessageBuffer()).AsTask();
        }


        public void Dispose()
        {
            cancellation?.Cancel();
            _heartbeat?.Dispose();
            _pipeReader?.Complete();
            _pipeWriter?.Complete();
            ShutDownSocket(_socket);
        }

        void ShutDownSocket(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close(1000);
            _logger.Info("Shutting down socket client....");
        }
        static bool ContainsLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;
                return false;
            }


            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            return true;
        }


        static void ProcessLine(in ReadOnlySequence<byte> buffer, IObserver<ReadOnlySequence<byte>> observer)
        {
            observer.OnNext(buffer);
        }
        private Stream GetStream(Uri endPoint)
        {
            var tcpClient = new TcpClient();
            Socket socket = null;
            try
            {
                if (SniProxy)
                    endPoint = new Uri(_clientConfiguration.ProxyServiceUrl);

                if (!_encrypt)
                {
                    tcpClient.Connect(endPoint.Host, endPoint.Port);
                    return tcpClient.GetStream();
                }

                Dns.GetHostAddressesAsync(endPoint.Host).ContinueWith(async task => {
                    if (!task.IsFaulted)
                    {
                        socket = await ConnectAsync(task.Result, endPoint.Port);
                    }
                    else
                        _logger.Error(task.Exception.ToString());
                });
                return new NetworkStream(socket, true);
            }
            catch (Exception ex)
            {
                tcpClient.Dispose();
                _logger.Error(ex.ToString());
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
        private async Task<Socket> ConnectAsync(IPAddress[] serverAddresses, int port)
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
