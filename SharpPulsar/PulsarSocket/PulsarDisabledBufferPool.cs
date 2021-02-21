using Akka.Actor;
using Akka.Configuration;
using Akka.IO.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.PulsarSocket
{
    using ByteBuffer = ArraySegment<byte>;

    internal class PulsarDisabledBufferPool : IBufferPool
    {
        private readonly int _bufferSize;

        public PulsarDisabledBufferPool(ExtendedActorSystem system, Config config) : this(config.GetInt("buffer-size", 256))
        {
        }

        public PulsarDisabledBufferPool(int bufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentException("Buffer size must be positive number", nameof(bufferSize));

            _bufferSize = bufferSize;
        }

        public ByteBuffer Rent() => RentOfSize(_bufferSize);

        public IEnumerable<ByteBuffer> Rent(int minimumSize)
        {
            var bytesRequired = Math.Max(minimumSize, _bufferSize);
            return new[] { RentOfSize(bytesRequired) };
        }

        public void Release(ByteBuffer buf)
        {
            // Let GC to collect this
        }

        public void Release(IEnumerable<ByteBuffer> buffers)
        {
            foreach (var buf in buffers)
            {
                Release(buf);
            }
        }

        private ByteBuffer RentOfSize(int size)
        {
            var bytes = new byte[size];
            return new ByteBuffer(bytes, 0, size);
        }
    }
}
