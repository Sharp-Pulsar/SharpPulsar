using Akka.Event;
using SharpPulsar.Common;
using SharpPulsar.Configuration;
using SharpPulsar.Extension;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.SocketImpl.Help;
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
using SharpPulsar.Auth;
using SharpPulsar.Protocol;
using SharpPulsar.Helpers;

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

        private const int ChunkSize = 75000;


        private ChunkingPipeline _pipeline;

        public event Action OnConnect;
        public event Action OnDisconnect;

        private Stream _networkstream;

        private PipeReader _pipeReader;

        private PipeWriter _pipeWriter;

        private readonly ILoggingAdapter _logger;

        private readonly DnsEndPoint _server;

        private readonly string _connectonId = string.Empty;

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        public static ISocketClient CreateClient(ClientConfigurationData conf, DnsEndPoint server, string hostName, ILoggingAdapter logger)
        {            
            return new SocketClient(conf, server, hostName, logger);
        }
        internal SocketClient(ClientConfigurationData conf, DnsEndPoint server, string hostName, ILoggingAdapter logger)
        {
            _server = server;
            if (conf.ClientCertificates != null)
                _clientCertificates = conf.ClientCertificates;

            if (conf.Authentication is AuthenticationTls tls)
                _clientCertificates = tls.AuthData.TlsCertificates;

            if (conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;

            _encrypt = conf.UseTls;

            _serviceUrl = conf.ServiceUrl;

            _clientConfiguration = conf;

            _logger = logger;

            _targetServerName = hostName;
            //_heartbeat.Start();

            //_connectonId = $"{_networkstream.}";

        }
        public async ValueTask Connect()
        {
            var host = _server.Host;
            var networkStream = await GetStream(_server);

            if (_encrypt)
                networkStream = await EncryptStream(networkStream, host);

            _networkstream = networkStream;

            _pipeline = new ChunkingPipeline(networkStream, ChunkSize);
            _pipeReader = PipeReader.Create(_networkstream);

            _pipeWriter = PipeWriter.Create(_networkstream);
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


        public IObservable<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> ReceiveMessageObservable =>
               Observable.Create<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)>((observer) => ReaderSchedule(observer, cancellation.Token));


        IDisposable ReaderSchedule(IObserver<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> observer, CancellationToken cancellationToken = default)
        {
            return NewThreadScheduler.Default.Schedule(async() =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                        var buffer = result.Buffer;

                        if (buffer.Length < 4)
                            break;
                        // Wire format
                        // [TOTAL_SIZE] [CMD_SIZE] [CMD] [BROKER_ENTRY_METADATA_MAGIC_NUMBER] [BROKER_ENTRY_METADATA_SIZE] [BROKER_ENTRY_METADATA] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
                        // | 4 bytes  | |4 bytes | |CMD_SIZE|

                        var frameSize = buffer.ReadUInt32(0, true);
                        var totalSize = frameSize + 4;

                        if (buffer.Length < totalSize)
                            break;
                        var frame = buffer.Slice(4, frameSize);
                        var commandSize = frame.ReadUInt32(0, true);
                        var command = Serializer.Deserialize<BaseCommand>(frame.Slice(4, commandSize));
                        if (command.type == BaseCommand.Type.Message)
                        {
                            var b = frame.Slice(commandSize + 4);
                            BrokerEntryMetadata brokerEntryMetadata = null;
                            var brokerEntryMetadataMagicNumber = b.ReadUInt32(2, true);
                            if (brokerEntryMetadataMagicNumber == Commands.MagicBrokerEntryMetadata)
                            {
                                var brokerEntrySlice = b.Slice(2);
                                var brokerEntryMetadataSize = brokerEntrySlice.ReadUInt32(0, true);
                                brokerEntryMetadata = Serializer.Deserialize<BrokerEntryMetadata>(brokerEntrySlice.Slice(4, brokerEntryMetadataSize));
                                b = brokerEntrySlice.Slice(brokerEntryMetadataSize + 4);
                            }
                            var hasMagicNumber = b.StartsWith(Constants.MagicNumber);
                            b = b.Slice(Constants.MagicNumber.Length);
                            var messageCheckSum = b.ReadUInt32(0, true);
                            b = b.Slice(4);
                            var metadataSize = b.ReadUInt32(0, true);
                            var metadata = Serializer.Deserialize<MessageMetadata>(b.Slice(4, metadataSize));

                            var hasValidCheckSum = messageCheckSum == CRC32C.Calculate(b);

                            var payload = new ReadOnlySequence<byte>(b.Slice(metadataSize + 4).ToArray()); 
                            observer.OnNext((command, metadata, brokerEntryMetadata, payload, hasValidCheckSum, hasMagicNumber));
                            //|> invalidArgIf((<>) MagicNumber) "Invalid magicNumber" |> ignore
                        }
                        else
                        {
                            observer.OnNext((command, null, null, ReadOnlySequence<byte>.Empty, false, false));
                        }
                        buffer = buffer.Slice(totalSize);
                        if (result.IsCompleted)
                            break;

                        _pipeReader.AdvanceTo(buffer.Start);
                    }

                    _pipeReader?.Complete();
                    observer.OnCompleted();

                }
                catch(Exception ex) 
                {
                    _logger.Error(ex.ToString());
                }
                
            });
        }

        public void SendMessage(ReadOnlySequence<byte> message)
        {
            _ = _pipeline.Send(message);
        }

        public void Dispose()
        {
            try
            {
                cancellation?.Cancel();
                _pipeReader?.Complete();
                _pipeWriter?.Complete();
                _networkstream?.Close();
                _networkstream?.Dispose();
            }
            catch
            {
                // ignored
            }

            if (OnDisconnect != null) OnDisconnect();
        }
        private async Task<Stream> GetStream(DnsEndPoint endPoint)
        {
            var tcpClient = new TcpClient();
            try
            {
                if (SniProxy)
                {
                    var url = new Uri(_clientConfiguration.ProxyServiceUrl);
                    endPoint = new DnsEndPoint(url.Host, url.Port);
                }                        

                if (!_encrypt)
                {
                    await tcpClient.ConnectAsync(endPoint.Host, endPoint.Port).ConfigureAwait(false);
                    
                    return tcpClient.GetStream();
                }

                var addr = await Dns.GetHostAddressesAsync(endPoint.Host).ConfigureAwait(false); 
                var socket = await ConnectAsync(addr, endPoint.Port).ConfigureAwait(false);
                return new NetworkStream(socket, true);
            }
            catch (Exception ex)
            {
                tcpClient.Dispose();
                _logger.Error(ex.ToString());
                throw;
            }
        }
        private async ValueTask<Stream> EncryptStream(Stream stream, string host)
        {
            SslStream sslStream = null;

            try
            {
                sslStream = new SslStream(stream, false, ValidateServerCertificate, null);
                await sslStream.AuthenticateAsClientAsync(host, _clientCertificates, SslProtocols.Tls12, true);
                return sslStream;
            }
            catch(Exception ex)
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
            var error = sslPolicyErrors.ToString();
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
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

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                var cert = new X509Certificate2(certificate);
                var cn = cert.GetNameInfo(X509NameType.SimpleName, false);
                var cleanName = cn?.Substring(cn.LastIndexOf('*') + 1);
                string[] addresses = { _serviceUrl, _targetServerName };

                // if the ending of the sni and servername do match the common name of the cert, fail
                return addresses.Count(item => cleanName != null && item.EndsWith(cleanName)) == addresses.Count();

            }
            

            return false;
        }

        public void Connected()
        {
            throw new NotImplementedException();
        }

        public void Disconnected()
        {
            throw new NotImplementedException();
        }

    }
}
