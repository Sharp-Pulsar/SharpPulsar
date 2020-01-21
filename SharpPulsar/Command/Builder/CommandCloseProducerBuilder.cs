using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandCloseProducerBuilder
    {
        private readonly CommandCloseProducer _producer;
        public CommandCloseProducerBuilder()
        {
            _producer = new CommandCloseProducer();
        }
        
        public CommandCloseProducerBuilder SetProducerId(long producerid)
        {
            _producer.ProducerId = (ulong)producerid;
            return this;
        }
        public CommandCloseProducerBuilder SetRequestId(long requestid)
        {
            _producer.RequestId = (ulong)requestid;
            return this;
        }
        public CommandCloseProducer Build()
        {
            return _producer;
        }
    }
}
