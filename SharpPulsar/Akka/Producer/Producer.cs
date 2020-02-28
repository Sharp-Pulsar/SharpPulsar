using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Akka.Actor;
using Org.BouncyCastle.Crypto;
using SharpPulsar.Akka.InternalCommands;
using SharpPulsar.Akka.Network;
using SharpPulsar.Api;
using SharpPulsar.Api.Interceptor;
using SharpPulsar.Common.Compression;
using SharpPulsar.Common.Naming;
using SharpPulsar.Common.Schema;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol;
using SharpPulsar.Protocol.Schema;

namespace SharpPulsar.Akka.Producer
{
    public class Producer: ReceiveActor, IWithUnboundedStash
    {
        private IActorRef _broker;
        private IActorRef _network;
        private ProducerConfigurationData _configuration;
        private long _producerId;
        public string _producerName;
        private readonly bool _userProvidedProducerName = false;

        private string _connectionId;
        private string _connectedSince;

        private readonly CompressionCodec _compressor;

        private readonly MessageCrypto _msgCrypto = null;

        private readonly IDictionary<string, string> _metadata;
        private sbyte[] _schemaVersion;
        private long _sequenceId;
        private long _requestId;
        private MultiSchemaMode _multiSchemaMode;
        private ISchema _schema;
        private ClientConfigurationData _clientConfiguration;
        private readonly List<IProducerInterceptor> _producerInterceptor;
        private readonly Dictionary<long, Payload> _pendingLookupRequests = new Dictionary<long, Payload>();
        private Dictionary<SchemaHash, byte[]> _schemaCache = new Dictionary<SchemaHash, byte[]>();
        private bool _isPartitioned;
        private int _partitionIndex;
        public Producer(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            _isPartitioned = isPartitioned;
            _clientConfiguration = clientConfiguration;
            _producerInterceptor = configuration.Interceptors;
            _schema = configuration.Schema;
            _configuration = configuration;
            _producerId = producerid;
            _network = network;
			_producerId = producerid;
			 _producerName= configuration.ProducerName;
             if (!configuration.MultiSchema)
             {
                 _multiSchemaMode = MultiSchemaMode.Disabled;
             }
            if (!string.IsNullOrWhiteSpace(_producerName) || isPartitioned)
			{
				_userProvidedProducerName = true;
			}
			_partitionIndex = configuration.Partitions;

			_compressor = CompressionCodecProvider.GetCompressionCodec(configuration.CompressionType);
			if (configuration.InitialSequenceId != null)
			{
				var initialSequenceId = (long)configuration.InitialSequenceId;
				_sequenceId = initialSequenceId;
			}
			else
			{
				_sequenceId = -1L;
			}

			if (configuration.EncryptionEnabled)
			{
				var logCtx = "[" + configuration.TopicName + "] [" + _producerName + "] [" + _producerId + "]";
				_msgCrypto = new MessageCrypto(logCtx, true);

				// Regenerate data key cipher at fixed interval
				Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30), Self, new AddPublicKeyCipher(), ActorRefs.NoSender);

            }
			//batching comes later
			if (!configuration.Properties.Any())
			{
				_metadata = new Dictionary<string, string>();
			}
			else
			{
				_metadata = new SortedDictionary<string, string>(configuration.Properties);
			}
            Become(LookUpBroker);
		}

        public static Props Prop(ClientConfigurationData clientConfiguration, ProducerConfigurationData configuration, long producerid, IActorRef network, bool isPartitioned = false)
        {
            return Props.Create(()=> new Producer(clientConfiguration, configuration, producerid, network, isPartitioned));
        }

        private void Init()
        {
            Receive<TcpSuccess>(s =>
            {
                Console.WriteLine($"Pulsar handshake completed with {s.Name}");
                Become(CreateProducer);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(_=> Stash.Stash());
        }

        public void Ready()
        {
            Context.Parent.Tell(new RegisteredProducer(_producerId, _producerName, _configuration.TopicName));
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            Receive<TcpClosed>(_ =>
            {
                Become(LookUpBroker);
            });
            Stash.UnstashAll();
        }
        public void CreateProducer()
        {
            Receive<ProducerCreated>(p =>
            {
                _pendingLookupRequests.Remove(p.RequestId);
                if (string.IsNullOrWhiteSpace(_producerName))
                    _producerName = p.Name;
                _sequenceId = p.LastSequenceId;
                var schemaVersion = p.SchemaVersion;
                if (schemaVersion != null)
                {
                    _schemaCache.TryAdd(SchemaHash.Of(_configuration.Schema), schemaVersion);
                }
                Become(Ready);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(x => Stash.Stash());
            if (_isPartitioned)
            {
                var index = int.Parse(Self.Path.Name);
                _producerId = index;
               _producerName = TopicName.Get(_configuration.TopicName).GetPartition(index).ToString();
            }
            SendNewProducerCommand();
        }
        private void SendNewProducerCommand()
        {
            var requestid = _requestId++;
            var schemaInfo = (SchemaInfo)_configuration.Schema.SchemaInfo;
            var request = Commands.NewProducer(_configuration.TopicName, _producerId, requestid, _producerName, _configuration.EncryptionEnabled, _metadata, schemaInfo, DateTime.Now.Millisecond, _userProvidedProducerName);
            var payload = new Payload(request.Array, requestid, "CommandProducer");
            _pendingLookupRequests.Add(requestid, payload);
            _network.Tell(payload);
        }
        private void LookUpBroker()
        {
            Receive<BrokerLookUp>(l =>
            {
                _pendingLookupRequests.Remove(l.RequestId);
                var uri = _configuration.UseTls ? new Uri(l.BrokerServiceUrlTls) : new Uri(l.BrokerServiceUrl);

                var address = new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port);
                _broker = Context.ActorOf(ClientConnection.Prop(address, _clientConfiguration, Sender));
                Become(Init);
            });
            Receive<AddPublicKeyCipher>(a =>
            {
                AddKey();
            });
            ReceiveAny(_ => Stash.Stash());
            SendBrokerLookUpCommand();
        }

        private void SendBrokerLookUpCommand()
        {
            var requestid = _requestId++;
            var request = Commands.NewLookup(_configuration.TopicName, false, requestid);
            var load = new Payload(request.Array, requestid, "BrokerLookUp");
            _network.Tell(load);
            _pendingLookupRequests.Add(requestid, load);
        }
        private void AddKey()
        {
            try
            {
                _msgCrypto.AddPublicKeyCipher(_configuration.EncryptionKeys, _configuration.CryptoKeyReader);
            }
            catch (CryptoException e)
            {
                Context.System.Log.Error(e.ToString());
            }
		}
		public class AddPublicKeyCipher
		{
            
        }

        public IStash Stash { get; set; }
    }
}
