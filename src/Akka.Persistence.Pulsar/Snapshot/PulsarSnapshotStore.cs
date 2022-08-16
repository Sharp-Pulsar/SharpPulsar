using Akka.Event;
using Akka.Persistence.Snapshot;
using Akka.Serialization;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Akka.Configuration;
using IdentityModel;
using SharpPulsar.Schemas;
using SharpPulsar.Configuration;
using SharpPulsar;
using SharpPulsar.User;
using SharpPulsar.Sql;
using SharpPulsar.Messages;
using SharpPulsar.Sql.Client;
using SharpPulsar.Sql.Message;
using SharpPulsar.Builder;
using SharpPulsar.Common.Naming;
using SharpPulsar.Auth;
using System.Security.Cryptography.X509Certificates;
using Akka.Actor;
using SharpPulsar.Sql.Public;

namespace Akka.Persistence.Pulsar.Snapshot
{
    /// <summary>
    ///     Pulsar-backed snapshot store for Akka.Persistence.
    /// </summary>
    /// 
    
    public class PulsarSnapshotStore : SnapshotStore
    {
        private readonly CancellationTokenSource _pendingRequestsCancellation;
        private readonly PulsarSettings _settings;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly PulsarSystem _pulsarSystem;
        private readonly PulsarClient _client;
        private readonly SqlInstance _sql;
        private readonly ClientOptions _sqlClientOptions;
        public static readonly ConcurrentDictionary<string, Producer<SnapshotEntry>> _producers = new ConcurrentDictionary<string, Producer<SnapshotEntry>>();
        private static readonly Type SnapshotType = typeof(Serialization.Snapshot);
        private readonly Serializer _serializer;
        private readonly AvroSchema<SnapshotEntry> _snapshotEntrySchema;
        private readonly TopicName _topicName;


        //public Akka.Serialization.Serialization Serialization => _serialization ??= Context.System.Serialization;

        public PulsarSnapshotStore(Config config) : this(new PulsarSettings(config))
        {

        }

        public PulsarSnapshotStore(PulsarSettings settings)
        {
            _topicName = TopicName.Get($"{settings.Topic}");
            _pendingRequestsCancellation = new CancellationTokenSource();
            _snapshotEntrySchema = AvroSchema<SnapshotEntry>.Of(typeof(SnapshotEntry));
            _serializer = Context.System.Serialization.FindSerializerForType(SnapshotType);
            _settings = settings;
            _sqlClientOptions = new ClientOptions 
            { 
                Server = settings.TrinoServer,
                Catalog = "pulsar",
                Schema = $"{_topicName.Tenant}/{_topicName.NamespaceObject.LocalName}"
            };
            var builder = new PulsarClientConfigBuilder()
                 .ServiceUrl(_settings.ServiceUrl)
                 .ConnectionsPerBroker(1)
                 .OperationTimeout(_settings.OperationTimeOut)
                 .Authentication(AuthenticationFactory.Create(_settings.AuthClass, _settings.AuthParam));

            if (!(_settings.TrustedCertificateAuthority is null))
            {
                builder = builder.AddTrustedAuthCert(_settings.TrustedCertificateAuthority);
            }

            if (!(_settings.ClientCertificate is null))
            {
                builder = builder.AddTlsCerts(new X509Certificate2Collection { _settings.ClientCertificate });
            }
            var pulsarSystem = PulsarStatic.System ?? PulsarSystem.GetInstance(Context.System);

            if (PulsarStatic.Client == null)
                PulsarStatic.Client = pulsarSystem.NewClient(builder).AsTask().Result;

            _client = PulsarStatic.Client;
            _sql = PulsarSystem.Sql(Context.System, _sqlClientOptions);
        }
        
        protected override async Task DeleteAsync(SnapshotMetadata metadata)
        {
            //use admin api to implement - maybe
            await Task.CompletedTask;
        }

        //use admin api to implement - maybe
        protected override async Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            await Task.CompletedTask;
        }

        protected override async Task<SelectedSnapshot> LoadAsync(string persistenceId, SnapshotSelectionCriteria criteria)
        {
            SelectedSnapshot shot = null;
            _sqlClientOptions.Execute = $"select Id, PersistenceId, SequenceNr, Timestamp, Snapshot from snapshot WHERE PersistenceId = {persistenceId} AND SequenceNr <= {criteria.MaxSequenceNr} AND Timestamp <= {criteria.MaxTimeStamp.ToEpochTime()} ORDER BY SequenceNr DESC, __publish_time__ DESC LIMIT 1";
            var response = await _sql.ExecuteAsync(TimeSpan.FromSeconds(5));
            var data = response.Response;
            switch (data)
            {
                case DataResponse dr:
                    return ToSelectedSnapshot(JsonSerializer.Deserialize<SnapshotEntry>(JsonSerializer.Serialize(dr.Data)));
                case StatsResponse sr:
                    _log.Info(JsonSerializer.Serialize(sr, new JsonSerializerOptions { WriteIndented = true }));
                    return shot;
                case ErrorResponse er:
                    _log.Error(er.Error.FailureInfo.Message);
                    return shot;
                default:
                    return shot;
            }
        }

        protected override async Task SaveAsync(SnapshotMetadata metadata, object snapshot)
        {
            var producer = await GetProducer(metadata.PersistenceId);
            var snapshotEntry = ToSnapshotEntry(metadata, snapshot);
            await producer.SendAsync(snapshotEntry);
        }

        private async ValueTask<Producer<SnapshotEntry>> CreateSnapshotProducer(string topic, string persistenceid)
        {
            var producerConfig = new ProducerConfigBuilder<SnapshotEntry>()
                   .ProducerName($"snapshot-{persistenceid}")
                   .Topic(topic)
                   .Schema(_snapshotEntrySchema)
                   .SendTimeout(TimeSpan.FromMilliseconds(10000));
            var producer = await _client.NewProducerAsync(_snapshotEntrySchema, producerConfig);
            _producers[persistenceid] = producer;
            return producer;
        }
        private async ValueTask<Producer<SnapshotEntry>> GetProducer(string persistenceid)
        {
            if (_producers.TryGetValue(persistenceid, out var producer))
            {
                return producer;
            }
            else
            {
                return await CreateSnapshotProducer(_settings.Topic, persistenceid);
            }
        }
        protected override void PostStop()
        {
            base.PostStop();

            // stop all operations executed in the background
            _pendingRequestsCancellation.Cancel();
            _client.Shutdown();
        }
        
        private object Deserialize(byte[] bytes)
        {
            return ((Serialization.Snapshot)_serializer.FromBinary(bytes, SnapshotType)).Data;
        }

        private byte[] Serialize(object snapshotData)
        {
            return _serializer.ToBinary(new Serialization.Snapshot(snapshotData));
        }
        private SnapshotEntry ToSnapshotEntry(SnapshotMetadata metadata, object snapshot)
        {
            var binary = Serialize(snapshot);

            return new SnapshotEntry
            {
                Id = metadata.PersistenceId + "_" + metadata.SequenceNr,
                PersistenceId = metadata.PersistenceId,
                SequenceNr = metadata.SequenceNr,
                Snapshot = Convert.ToBase64String(binary),
                Timestamp = metadata.Timestamp.ToEpochTime()
            };
        }

        private SelectedSnapshot ToSelectedSnapshot(SnapshotEntry entry)
        {
            var snapshot = Deserialize(Convert.FromBase64String(entry.Snapshot));
            return new SelectedSnapshot(new SnapshotMetadata(entry.PersistenceId, entry.SequenceNr), snapshot);

        }
    }
}
