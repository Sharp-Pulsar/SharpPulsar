using System;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using SharpPulsar.Configuration;
using SharpPulsar.Auth;
using System.IO;
using System.Net.Security;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Buffers;
using SharpPulsar.Protocol.Proto;
using ProtoBuf;
using SharpPulsar.Common;
using SharpPulsar.Extension;
using SharpPulsar.Protocol;
using SharpPulsar.Client.Internal.Help;

namespace SharpPulsar.Client.Internal
{
    internal sealed class SocketClientActor : ReceiveActor
    {
        private readonly  Socket _socket;
        private readonly X509Certificate2Collection _clientCertificates;
        private readonly X509Certificate2 _trustedCertificateAuthority;
        private readonly ClientConfigurationData _clientConfiguration;
        private readonly bool _encrypt;
        private readonly string _serviceUrl;
        private readonly string _targetServerName;

        private const int ChunkSize = 75000;


        private ChunkingPipeline _pipeline;

        private PipeReader _pipeReader;

        private PipeWriter _pipeWriter;

        private readonly ILoggingAdapter _logger;

        private readonly DnsEndPoint _server;
        private IActorRef _client;
        //private IActorRef _self;

        private readonly string _connectonId = string.Empty;
        private bool _start = true;
        //private ICancelable _socketkCancellable;
        //private readonly IScheduler _scheduler;

        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        public SocketClientActor(IActorRef client, ClientConfigurationData conf, DnsEndPoint server, string hostName)
        {
            //_scheduler = Context.System.Scheduler;
            _client = client;
            //_self = Self;
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket = clientSocket;
            _server = server;
            if (conf.ClientCertificates != null)
                _clientCertificates = conf.ClientCertificates;

            if (conf.Authentication is AuthenticationTls tls)
                _clientCertificates = tls.GetAuthData().TlsCertificates;

            if (conf.TrustedCertificateAuthority != null)
                _trustedCertificateAuthority = conf.TrustedCertificateAuthority;

            _encrypt = conf.UseTls;

            _serviceUrl = conf.ServiceUrl;

            _clientConfiguration = conf;

            _logger = Context.GetLogger();

            _targetServerName = hostName;
            ReceiveAsync<Connect>(async _ =>
            {
                var host = _server.Host;
                var networkStream = await GetStream(_server);

                if (_encrypt)
                    networkStream = await EncryptStream(networkStream, host);

                _pipeline = new ChunkingPipeline(networkStream, ChunkSize);
                _pipeReader = PipeReader.Create(networkStream);

                _pipeWriter = PipeWriter.Create(networkStream);
                var m = Context.ActorOf(SendMessageActor.Prop(_pipeline));
                Sender.Tell(m);

            });
            // THIS IS NOT FAST ENOUGH
            /*ReceiveAsync<ClientMessage>(async _ =>
            {
                try
                {
                    _logger.Info("Running on thread: " + Thread.CurrentThread.ManagedThreadId);

                    var result = await _pipeReader.ReadAsync().ConfigureAwait(false);

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
                                var calculatedCheckSum = CRC32C.Get(0u, stream, metadataLength + payloadLength);
                                var hasValidCheckSum = messageCheckSum == calculatedCheckSum;
                                _client.Tell(new Reader(command, metadata, brokerEntryMetadata, new ReadOnlySequence<byte>(payload), hasValidCheckSum, hasMagicNumber));
                                //|> invalidArgIf((<>) MagicNumber) "Invalid magicNumber" |> ignore
                            }
                            else
                            {
                                _client.Tell(new Reader(command, null, null, ReadOnlySequence<byte>.Empty, false, false));
                            }
                            if (result.IsCompleted)
                                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
                            else
                                _pipeReader.AdvanceTo(consumed);
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
                
            }); */
            /*Receive<Start>(_ =>
            {
                if(_start) return;  
                _socketkCancellable = _scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(100),
                        () => 
                        {
                            _self.Tell(ClientMessage.Instance);
                        });
                _start = true;
            });*/

            ReceiveAsync<Start>(async _ =>
            {
                try
                {
                    _logger.Info("Running on thread: " + Thread.CurrentThread.ManagedThreadId);

                    while (_start)
                    {                        
                        var result = await _pipeReader.ReadAsync().ConfigureAwait(false);

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
                                    _client.Tell(new Reader(command, metadata, brokerEntryMetadata, new ReadOnlySequence<byte>(payload), hasValidCheckSum, hasMagicNumber));
                                    //|> invalidArgIf((<>) MagicNumber) "Invalid magicNumber" |> ignore
                                }
                                else
                                {
                                    _client.Tell(new Reader(command, null, null, ReadOnlySequence<byte>.Empty, false, false));
                                }
                                if (result.IsCompleted)
                                    _pipeReader.AdvanceTo(buffer.Start, buffer.End);
                                else
                                    _pipeReader.AdvanceTo(consumed);
                            }

                        }
                        //await Task.Delay(TimeSpan.FromMilliseconds(100));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
                finally
                {
                    await _pipeReader.CompleteAsync().ConfigureAwait(false);
                }
            });
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
            catch (Exception)
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
                foreach (var address in serverAddresses)
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
        public static Props Prop(IActorRef client, ClientConfigurationData conf, DnsEndPoint server, string hostName)
        {
            return Props.Create(() => new SocketClientActor(client, conf, server, hostName));
        }
        protected override void PostStop()
        {
            try
            {
                //_pipeReader.CompleteAsync().ConfigureAwait(false).GetAwaiter();
                //_socketkCancellable.Cancel();   
                _start = false;
                cancellation?.Cancel();
                _pipeReader?.Complete();
                _pipeWriter?.Complete();
                _socket.Shutdown(SocketShutdown.Both);
                _socket?.Close(1000);
            }
            catch
            {
                // ignored
            }
            _logger.Info("connection close complete....");
            base.PostStop();
        }
        internal readonly record struct Connect
        {
            internal static Connect Instance = new Connect();
        }
        internal readonly record struct Start
        {
            internal static Start Instance = new Start();
        }
        private readonly record struct ClientMessage
        {
            internal static ClientMessage Instance = new ClientMessage();
        }
        internal readonly record struct SendMessage
        {
            public readonly ReadOnlySequence<byte> Message;
            internal SendMessage(ReadOnlySequence<byte> message)
            {
                Message = message;
            }
        }
        internal readonly record struct Reader(BaseCommand Command, MessageMetadata Metadata, BrokerEntryMetadata BrokerEntryMetadata, ReadOnlySequence<byte> Payload, bool HasValidcheckSum, bool HasMagicNumber);
    }
}
