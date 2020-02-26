using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using SharpPulsar.Api;
using SharpPulsar.Impl.Conf;

namespace SharpPulsar.Akka.Consumer
{
    public class ConsumerManager<T>:ReceiveActor
    {
        private IConsumerBuilder<T> _consumerBuilder;
        private IMessageReceivedHandler _handler;
        private ConsumerConfigurationData<T> _conf;
        public ConsumerManager(IConsumerBuilder<T> consumerBuilder, ConsumerConfigurationData<T> conf, IMessageReceivedHandler handler)
        {
            _consumerBuilder = consumerBuilder;
            _handler = handler;
            _conf = conf;
        }

        public static Props Prop(IConsumerBuilder<T> consumerBuilder, ConsumerConfigurationData<T> conf, IMessageReceivedHandler handler)
        {
            return Props.Create(()=> new ConsumerManager<T>(consumerBuilder, conf, handler));
        }
    }
}
