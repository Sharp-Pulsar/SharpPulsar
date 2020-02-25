using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;

namespace SharpPulsar.Transport
{
    public interface ITransport
    {
        ValueTask Write(IByteBuffer message);
        ValueTask<IByteBuffer> Receive(CancellationToken token);
        ValueTask Dispose();
    }
}
