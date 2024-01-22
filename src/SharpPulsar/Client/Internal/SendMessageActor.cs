using Akka.Actor;
using SharpPulsar.Client.Internal.Help;
using static SharpPulsar.Client.Internal.SocketClientActor;

namespace SharpPulsar.Client.Internal
{
    internal sealed class SendMessageActor : ReceiveActor
    {
        private ChunkingPipeline _pipeline;
        public SendMessageActor(ChunkingPipeline pipeline)
        {
            _pipeline = pipeline;
            ReceiveAsync<SendMessage>(async message =>
            {
                await _pipeline.Send(message.Message);
                Sender.Tell(message);
            });
        }
        public static Props Prop(ChunkingPipeline pipeline)
        {
            return Props.Create(() => new SendMessageActor(pipeline));
        }
        protected override void PostStop()
        {
            base.PostStop();
        }
    }
}
