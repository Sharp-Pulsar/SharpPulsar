using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.SocketImpl
{
    public static class SocketEx
    {
        public static byte[] ToMessageBuffer(this string source)
        {
            return Encoding.UTF8.GetBytes($"{source}\n");
        }

        public static string ToMessage(this ReadOnlySequence<byte> source)
        {
            return Encoding.UTF8.GetString(source.ToArray());
        }


        public static async ValueTask SendAsync(this PipeWriter writer, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await writer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }


        public static async ValueTask SendAsync(this PipeWriter writer, byte[] buffer, CancellationToken cancellationToken = default)
        {
            await writer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        public static ValueTask<FlushResult> SendMessageAsync(this PipeWriter writer, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default)
        {
            foreach(var seq in buffer)
                writer.WriteAsync(seq, cancellationToken);

            return new ValueTask<FlushResult>();
        }


    }
}
