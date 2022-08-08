using Akka.Actor;
using SharpPulsar.Messages.Client;
using SharpPulsar.Messages.Consumer;
using SharpPulsar.Messages.Producer;

namespace SharpPulsar
{
    public class IdGeneratorActor:ReceiveActor
    {
        private long _requestIdGenerator;
        private long _consumerIdGenerator;
        private long _producerIdGenerator;
        public IdGeneratorActor()
        {
			Receive<NewRequestId>(_ => Sender.Tell(new NewRequestIdResponse(NewRequestId())));
			Receive<NewConsumerId>(_ => Sender.Tell(NewConsumerId()));
			Receive<NewProducerId>(_ => Sender.Tell(NewProducerId()));
		}
		public static Props Prop()
        {
			return Props.Create(() => new IdGeneratorActor());
        }
		private long NewProducerId()
		{
			return _producerIdGenerator++;
		}

		private long NewConsumerId()
		{
			return _consumerIdGenerator++;
		}

		private long NewRequestId()
		{
			return _requestIdGenerator++;
		}
	}
}
