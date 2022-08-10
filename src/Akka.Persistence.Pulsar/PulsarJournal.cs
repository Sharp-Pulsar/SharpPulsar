#region copyright

// -----------------------------------------------------------------------
//  <copyright file="PulsarJournal.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.Journal;
using SharpPulsar;
using SharpPulsar.Auth;
using SharpPulsar.Builder;
using SharpPulsar.Schemas;
using SharpPulsar.User;

namespace Akka.Persistence.Pulsar
{
    public sealed class PulsarJournal : AsyncWriteJournal
    {
        private readonly PulsarSettings settings;
        private readonly PulsarClient client;
        private Akka.Serialization.Serialization serialization;

        public Akka.Serialization.Serialization Serialization => serialization ??= Context.System.Serialization;
        public PulsarJournal() : this(PulsarPersistence.Get(Context.System).JournalSettings)
        {
        }
        public PulsarJournal(PulsarSettings settings)
        {
           var builder = new PulsarClientConfigBuilder()
                .ServiceUrl(settings.ServiceUrl)
                .ConnectionsPerBroker(1)
                .OperationTimeout(settings.OperationTimeOut)
                .Authentication(AuthenticationFactory.Create(settings.AuthClass, settings.AuthParam));

            if (!(settings.TrustedCertificateAuthority is null))
            {
                builder = builder.AddTrustedAuthCert(this.settings.TrustedCertificateAuthority);
            }

            if (!(settings.ClientCertificate is null))
            {
                builder = builder.AddTlsCerts(new X509Certificate2Collection { settings.ClientCertificate });
            }
            var pulsarSystem = PulsarSystem.GetInstance(Context.System);
            this.settings = settings;
            client = pulsarSystem.NewClient(builder).AsTask().Result;
        }       

        /// <summary>
        /// This method replays existing event stream (identified by <paramref name="persistenceId"/>) asynchronously.
        /// It doesn't replay the whole stream, but only a window of it (described by range of [<paramref name="fromSequenceNr"/>, <paramref name="toSequenceNr"/>),
        /// with a limiter of up to <paramref name="max"/> elements - therefore it's possible that it will complete
        /// before the whole window is replayed.
        ///
        /// For every replayed message we need to construct a corresponding <see cref="Persistent"/> instance, that will
        /// be send back to a journal by calling a <paramref name="recoveryCallback"/>.
        /// </summary>
        public override async Task ReplayMessagesAsync(IActorContext context, string persistenceId, long fromSequenceNr,
            long toSequenceNr, long max,
            Action<IPersistentRepresentation> recoveryCallback)
        {
            var startMessageId = new ReaderConfigBuilder<byte[]>()
            .StartMessageId(settings.LedgerId, settings.EntryId, settings.Partition, settings.BatchIndex)
            .Topic(persistenceId);
            var reader = await client.NewReaderAsync(startMessageId);
            async IAsyncEnumerable<Message<byte[]>> message()
            {
                var msg = await reader.ReadNextAsync(TimeSpan.FromMilliseconds(1000));
                while (true)
                {
                    yield return (Message<byte[]>)msg;
                    msg = await reader.ReadNextAsync(TimeSpan.FromMilliseconds(1000));
                }
            };
            await foreach (var msg in message())
            {
                var persistent = new Persistent(msg.Data, msg.SequenceId, persistenceId/*, manifest, sender*/);
                recoveryCallback(persistent);
            }
        }

        /// <summary>
        /// This method is called at the very beginning of the replay procedure to define a possible boundary of replay:
        /// In akka persistence every persistent actor starts from the replay phase, where it replays state from all of
        /// the events emitted so far before being marked as ready for command processing.
        /// </summary>
        public override async Task<long> ReadHighestSequenceNrAsync(string persistenceId, long fromSequenceNr)
        {
            //TODO: how to read the latest known sequence nr in pulsar? Theoretically it's the last element in virtual
            // topic belonging to that persistenceId.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a batch of messages. Each <see cref="AtomicWrite"/> can have one or many <see cref="IPersistentRepresentation"/>
        /// events inside its payload, and they all should be written in atomic fashion (in one transaction, all-or-none).
        ///
        /// In case of failure of a single <see cref="AtomicWrite"/> we don't fail right away. Instead we try to write
        /// remaining writes and gather all exceptions produced in the process: they will be returned at the end.
        /// </summary>
        protected override async Task<IImmutableList<Exception>> WriteMessagesAsync(IEnumerable<AtomicWrite> messages)
        {
            //TODO: creating producer for every single write is not feasible. We should be able to create or cache topics and use them ad-hoc when necessary.

            Producer<byte[]> producer = null;
            var failures = ImmutableArray.CreateBuilder<Exception>(0);
            foreach (var write in messages)
            {
                try
                {
                    var persistentMessages = (IImmutableList<IPersistentRepresentation>) write.Payload;
                    foreach (var message in persistentMessages)
                    {
                        var data = serialization.Serialize(write.Payload);
                        if(producer == null) 
                        {

                            var producerConfig = new ProducerConfigBuilder<byte[]>()
                                .Topic(message.PersistenceId)
                                .SendTimeout(TimeSpan.FromMilliseconds(10000));

                            producer = await client.NewProducerAsync(producerConfig);
                        }
                        await producer.NewMessage()
                        //.Properties(properties)
                        .SequenceId(message.SequenceNr)
                        .Key(message.PersistenceId)
                        .Value(data)
                        .SendAsync();
                    }
                }
                catch (Exception e)
                {
                    failures.Add(e);
                }
            }

            return failures.ToImmutable();
        }
        
        /// <summary>
        /// Deletes all events stored in a single logical event stream (pulsar virtual topic), starting from the
        /// beginning of stream up to <paramref name="toSequenceNr"/> (inclusive?).
        /// </summary>
        protected override async Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            throw new NotImplementedException();
        }
    }
}