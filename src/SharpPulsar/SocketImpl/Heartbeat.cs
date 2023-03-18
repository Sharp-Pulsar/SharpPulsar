using System;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    internal class HeartbeatEvent
    {
        public HeartbeatEvent(Socket socket, bool state)
        {
            Socket = socket;
            State = state;
        }

        public Socket Socket { get; }
        public bool State { get; }
    }



    internal class Heartbeat : IDisposable
    {
        private readonly IObservable<long> _heartbeatStream;

        private IDisposable _disp = Disposable.Empty;

        private readonly ILoggingAdapter _logger;

        private readonly Socket _socket;

        public event Action<HeartbeatEvent> OnDisconnect;

        public Heartbeat(Socket socket, ILoggingAdapter logger)
        {
            _socket = socket;
            _heartbeatStream = Observable.Timer(HeartbeatInterval, HeartbeatInterval);
            _logger = logger;
        }

        TimeSpan HeartbeatInterval => TimeSpan.FromSeconds(1);

        public void Dispose()
        {
            _disp.Dispose();
            _logger.Debug($"Stop heartbeat detection {_socket.RemoteEndPoint}");
        }

        public Task Start()
        {
            _disp = _heartbeatStream.Subscribe(HeartbeatProcess);

            _logger.Debug($"Start heartbeat detection {_socket.RemoteEndPoint}");

            return Task.CompletedTask;
        }

        void HeartbeatProcess(long timer)
        {
            var socketstate = _socket.Poll(100, SelectMode.SelectRead);
            if (socketstate)
            {
                _logger.Debug($"{DateTime.Now}-Endpoint:{_socket.RemoteEndPoint} Dropped:{socketstate}");

                var handler = OnDisconnect;

                handler?.Invoke(new HeartbeatEvent(_socket, socketstate));
            }

        }
    }
}
