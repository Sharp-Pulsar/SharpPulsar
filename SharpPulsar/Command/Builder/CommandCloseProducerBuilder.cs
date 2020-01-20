using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandCloseProducerBuilder
    {
        private readonly CommandCloseProducer _producer;
        public CommandCloseProducerBuilder()
        {
            _producer = new CommandCloseProducer();
        }
        private CommandCloseProducerBuilder(CommandCloseProducer producer)
        {
            _producer = producer;
        }
        public CommandCloseProducerBuilder SetProducerId(long producerid)
        {
            _producer.ProducerId = (ulong)producerid;
            return new CommandCloseProducerBuilder(_producer);
        }
        public CommandCloseProducerBuilder SetRequestId(long requestid)
        {
            _producer.RequestId = (ulong)requestid;
            return new CommandCloseProducerBuilder(_producer);
        }
        public CommandCloseProducer Build()
        {
            return _producer;
        }
    }
}
