using System;
using System.Linq;
using System.Threading;
using Akka.Actor;
using SharpPulsar.Akka.InternalCommands.Consumer;
using SharpPulsar.Api;
using SharpPulsar.Batch;
using SharpPulsar.Common.Naming;
using SharpPulsar.Impl;
using SharpPulsar.Impl.Conf;
using SharpPulsar.Protocol.Proto;
using SharpPulsar.Utility;

namespace SharpPulsar.Akka.Reader
{
    public class Reader: ReceiveActor
    {
        public Reader(ClientConfigurationData clientConfiguration, ReaderConfigurationData readerConfiguration, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            var subscription = "reader-" + ConsumerName.Sha1Hex(Guid.NewGuid().ToString()).Substring(0, 10);
            if (!string.IsNullOrWhiteSpace(readerConfiguration.SubscriptionRolePrefix))
            {
                subscription = readerConfiguration.SubscriptionRolePrefix + "-" + subscription;
            }

            var readerListener = readerConfiguration.ReaderListener;
            var consumerConfiguration = new ConsumerConfigurationData();
            consumerConfiguration.TopicNames.Add(readerConfiguration.TopicName);
            consumerConfiguration.SubscriptionName = subscription;
            consumerConfiguration.SubscriptionType = CommandSubscribe.SubType.Exclusive;
            consumerConfiguration.ReceiverQueueSize = readerConfiguration.ReceiverQueueSize;
            consumerConfiguration.ReadCompacted = readerConfiguration.ReadCompacted;
            consumerConfiguration.Schema = readerConfiguration.Schema;
            consumerConfiguration.ConsumerEventListener = readerConfiguration.EventListener;
            
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

            if (readerConfiguration.KeyHashRanges != null)
            {
                consumerConfiguration.KeySharedPolicy = KeySharedPolicy.StickyHashRange().GetRanges(readerConfiguration.KeyHashRanges.ToArray());
            }
            var partitionIdx = TopicName.GetPartitionIndex(readerConfiguration.TopicName);
            Context.ActorOf(Consumer.Consumer.Prop(clientConfiguration, readerConfiguration.TopicName, consumerConfiguration, Interlocked.Increment(ref IdGenerators.ReaderId), network, true, partitionIdx, SubscriptionMode.NonDurable, seek, pulsarManager), readerConfiguration.ReaderName);
            Receive<ConsumedMessage>(m =>
            {
                var msgid = m.Message.MessageId;
                if (consumerConfiguration.ConsumptionType == ConsumptionType.Listener)
                    readerListener.Received(m.Message);
                else 
                    pulsarManager.Tell(new ConsumedMessage(m.Consumer, m.Message, m.AckSets, m.ConsumerName));
                // Acknowledge message immediately because the reader is based on non-durable subscription. When it reconnects,
                // it will specify the subscription position anyway
                m.Consumer.Tell(new AckMessages(msgid));
            });
            Receive<CloseConsumer>(c => { Context.Parent.Tell(c);});
        }

        public static Props Prop(ClientConfigurationData clientConfiguration, ReaderConfigurationData readerConfiguration, IActorRef network, Seek seek, IActorRef pulsarManager)
        {
            return Props.Create(()=> new Reader(clientConfiguration, readerConfiguration, network, seek, pulsarManager));
        }
    }
}
