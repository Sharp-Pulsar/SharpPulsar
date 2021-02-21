using static Akka.IO.Inet;

namespace SharpPulsar.PulsarSocket
{
    public class PulsarWorkerDispatcher : SocketOption
    {
        public string Dispatcher { get; }

        public PulsarWorkerDispatcher(string dispatcher)
        {
            Dispatcher = dispatcher;
        }
    }
}
