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
using ProtoBuf;
using Serializer = ProtoBuf.Serializer;

namespace SharpPulsar.SocketImpl
{
    //https://github.com/fzf003/TinyService/blob/242c38c06ef7cb685934ba653041d07c64a6f806/Src/TinyServer.ReactiveSocket/SocketAcceptClient.cs
    public sealed class SocketClient: ISocketClient
    {
        private readonly Socket _socket;
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2 _trustedCertificateAuthority;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private readonly string _targetServerName;

        private const int ChunkSize = 75000;


        private ChunkingPipeline _pipeline;

        public event Action OnConnect;
        public event Action OnDisconnect;

        private PipeReader _pipeReader;

        private PipeWriter _pipeWriter;

        private readonly ILoggingAdapter _logger;

        private readonly DnsEndPoint _server;

        private readonly string _connectonId = string.Empty;

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        public static ISocketClient CreateClient(ClientConfigurationData conf, DnsEndPoint server, string hostName, ILoggingAdapter logger)
        {
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            //clientSocket.Connect(server.Host, server.Port);

            return new SocketClient(conf, clientSocket, hostName, logger, server);
        }
        internal SocketClient(ClientConfigurationData conf, Socket server, string hostName, ILoggingAdapter logger, DnsEndPoint dnsEndPoint)
        {
            _socket = server;
            _server = dnsEndPoint;
            if (conf.ClientCertificates != null)
                _clientCertificates = conf.ClientCertificates;

            if (conf.Authentication is AuthenticationTls tls)
                _clientCertificates = tls.GetAuthData().TlsCertificates;

            if (conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;

            _encrypt = conf.UseTls;

            _serviceUrl = conf.ServiceUrl;

            _clientConfiguration = conf;

            _logger = logger;

            _targetServerName = hostName;

        }
        public async ValueTask Connect()
        {
            var host = _server.Host;
            var networkStream = await GetStream(_server);

            if (_encrypt)
                networkStream = await EncryptStream(networkStream, host);

            _pipeline = new ChunkingPipeline(networkStream, ChunkSize);
            _pipeReader = PipeReader.Create(networkStream);

            _pipeWriter = PipeWriter.Create(networkStream);
        }
        public string RemoteConnectionId
        {
            get
            {
                return _connectonId;
            }
        }

        public IObservable<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> ReceiveMessageObservable =>
               Observable.Create<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)>((observer) => ReaderSchedule(observer, cancellation.Token));


        IDisposable ReaderSchedule(IObserver<(BaseCommand command, MessageMetadata metadata, BrokerEntryMetadata brokerEntryMetadata, ReadOnlySequence<byte> payload, bool hasValidcheckSum, bool hasMagicNumber)> observer, CancellationToken cancellationToken = default)
        {
            return NewThreadScheduler.Default.Schedule(async() =>
            {
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var result = await _pipeReader.ReadAsync(cancellationToken).ConfigureAwait(false);

                        var buffer = result.Buffer;
                        var length = (int)buffer.Length;
                        if (length >= 8)
                        {
                            using var stream = new MemoryStream(buffer.ToArray());
                            using var reader = new BinaryReader(stream);
                            // Wire format
                            // [TOTAL_SIZE] [CMD_SIZE] [CMD] [BROKER_ENTRY_METADATA_MAGIC_NUMBER] [BROKER_ENTRY_METADATA_SIZE] [BROKER_ENTRY_METADATA] [MAGIC_NUMBER][CHECKSUM] [METADATA_SIZE][METADATA] [PAYLOAD]
                            // | 4 bytes  | |4 bytes | |CMD_SIZE|

                            var frameSize = reader.ReadInt32().IntFromBigEndian();
                            var totalSize = frameSize + 4;
                            if (length >= totalSize)
                            {
                                var consumed = buffer.GetPosition(totalSize);
                                var command = Serializer.DeserializeWithLengthPrefix<BaseCommand>(stream, PrefixStyle.Fixed32BigEndian);
                                if (command.type == BaseCommand.Type.Message)
                                {
                                    BrokerEntryMetadata brokerEntryMetadata = null;
                                    var brokerEntryMetadataPosition = stream.Position;
                                    var brokerEntryMetadataMagicNumber = reader.ReadInt16().Int16FromBigEndian();
                                    if (brokerEntryMetadataMagicNumber == Commands.MagicBrokerEntryMetadata)
                                    {
                                        brokerEntryMetadata = Serializer.DeserializeWithLengthPrefix<BrokerEntryMetadata>(stream, PrefixStyle.Fixed32BigEndian);
                                    }
                                    else
                                        //we need to rewind to the brokerEntryMetadataPosition
                                        stream.Seek(brokerEntryMetadataPosition, SeekOrigin.Begin);

                                    var magicNumber = (uint)reader.ReadInt16().Int16FromBigEndian();
                                    var hasMagicNumber = magicNumber == 3585;
                                    var messageCheckSum = (uint)reader.ReadInt32().IntFromBigEndian();

                                    var metadataOffset = stream.Position;
                                    var metadata = Serializer.DeserializeWithLengthPrefix<MessageMetadata>(stream, PrefixStyle.Fixed32BigEndian);
                                    var payloadOffset = stream.Position;
                                    var metadataLength = (int)(payloadOffset - metadataOffset);
                                    var payloadLength = totalSize - (int)payloadOffset;
                                    var payload = reader.ReadBytes(payloadLength);
                                    stream.Seek(metadataOffset, SeekOrigin.Begin);
                                    var calculatedCheckSum = (uint)CRC32C.Get(0u, stream, metadataLength + payloadLength);
                                    var hasValidCheckSum = messageCheckSum == calculatedCheckSum;
                                    observer.OnNext((command, metadata, brokerEntryMetadata, new ReadOnlySequence<byte>(payload), hasValidCheckSum, hasMagicNumber));
                                    //|> invalidArgIf((<>) MagicNumber) "Invalid magicNumber" |> ignore
                                }
                                else
                                {
                                    observer.OnNext((command, null, null, ReadOnlySequence<byte>.Empty, false, false));
                                }
                                if (result.IsCompleted)
                                    _pipeReader.AdvanceTo(buffer.Start, buffer.End);
                                else
                                    _pipeReader.AdvanceTo(consumed);
                            }

                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
                finally
                {
                    await _pipeReader.CompleteAsync().ConfigureAwait(false);
                    observer.OnCompleted();
                }                           
                
            });
        }

        public async ValueTask SendMessage(ReadOnlySequence<byte> message)
        {
            await _pipeline.Send(message);
        }

        public void Dispose()
        {
            try
            {
                cancellation?.Cancel();
                _pipeReader?.Complete();
                _pipeWriter?.Complete();
                ShutDownSocket(_socket);
            }
            catch
            {
                // ignored
            }

            if (OnDisconnect != null) OnDisconnect();
        }
        void ShutDownSocket(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket?.Close(1000);
            }
            catch { }
            _logger.Info("connection close complete....");
        }

        private async Task<Stream> GetStream(DnsEndPoint endPoint)
        {
            try
            {
                
                if (SniProxy)
                {
                    var url = new Uri(_clientConfiguration.ProxyServiceUrl);
                    endPoint = new DnsEndPoint(url.Host, url.Port);
                }
                if (!_encrypt)
                {           
                    await _socket.ConnectAsync(endPoint);
                    return new NetworkStream(_socket);
                }

                var addr = await Dns.GetHostAddressesAsync(endPoint.Host).ConfigureAwait(false); 
                await ConnectAsync(addr, endPoint.Port).ConfigureAwait(false);
                return new NetworkStream(_socket, true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw new Exception("Failed to connect.");
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
            catch(Exception)
            {
                if (sslStream is null)
                    stream.Dispose();
                else
                    sslStream.Dispose();

                throw;
            }
        }
        //https://github.com/LukeInkster/CSharpCorpus/blob/919b7525a61eb6b475fbcba0d87fd3cb44ef3b38/corefx/src/System.Data.SqlClient/src/System/Data/SqlClient/SNI/SNITcpHandle.cs
        private async Task ConnectAsync(IPAddress[] serverAddresses, int port)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await _socket.ConnectAsync(serverAddresses, port).ConfigureAwait(false);
            }
            else
            {
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
                    try
                    {
                        await _socket.ConnectAsync(address, port).ConfigureAwait(false);
                        return;
                    }
                    catch (Exception exc)
                    {
                        _socket.Dispose();
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
