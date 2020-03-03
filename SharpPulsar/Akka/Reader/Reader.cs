using System;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.Reader
{
    public class Reader: ReceiveActor
    {
        private long _readerid;
        private IReaderListener _readerListener;
        public Reader(ClientConfigurationData clientConfiguration, ReaderConfigurationData readerConfiguration, IActorRef network)
        {
			var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }

            _readerListener = readerConfiguration.ReaderListener;
            var consumerConfiguration = new ConsumerConfigurationData();
            consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = CommandSubscribe.SubType.Exclusive;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;
            consumerConfiguration.Schema = readerConfiguration.Schema;
            
            if(readerConfiguration.StartMessageId != null)
                consumerConfiguration.StartMessageId = (BatchMessageId)readerConfiguration.StartMessageId;

            if (readerConfiguration.ReaderName != null)
            {
                consumerConfiguration.ConsumerName = readerConfiguration.ReaderName;
            }

            if (readerConfiguration.ResetIncludeHead)
            {
                consumerConfiguration.ResetIncludeHead = true;
            }
            
            consumerConfiguration.CryptoFailureAction = readerConfiguration.CryptoFailureAction;
            if (readerConfiguration.CryptoKeyReader != null)
            {
                consumerConfiguration.CryptoKeyReader = readerConfiguration.CryptoKeyReader;
            }

            var partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
            Context.ActorOf(Consumer.Consumer.Prop(clientConfiguration, readerConfiguration.TopicName,
                consumerConfiguration, _readerid++, network, true, partitionIdx, SubscriptionMode.NonDurable));
            Receive<ConsumedMessage>(m =>
            {
                _readerListener.Received(m.Message);
                // Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
                // it will specify the subscription position anyway
                m.Consumer.Tell(new AckMessages(m.Message.ReceivedId));
            });
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ReaderConfigurationData readerConfiguration, IActorRef network)
        {
            return Props.Create(()=> new Reader(clientConfiguration, readerConfiguration, network));
        }
    }
}
