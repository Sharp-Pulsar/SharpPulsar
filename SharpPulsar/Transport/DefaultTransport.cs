using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Buffers;
using Pipelines.Sockets.Unofficial;
using Pipelines.Sockets.Unofficial.Arenas;

namespace SharpPulsar.Transport
{
    public class DefaultTransport:ITransport
    {
        private readonly PipeReader _reader;
        private readonly PipeWriter _writer;
        private readonly Stream _stream;
        public DefaultTransport(PipeReader reader, PipeWriter writer)
        {
            _reader = reader;
            _writer = writer;
            _stream = StreamConnection.GetWriter(writer);
        }
        
        public async ValueTask Write(IByteBuffer message)
        {
            var sequence = new ReadOnlySequence<byte>(message.Array);
            foreach (var segment in sequence)
            {
                var data = segment.ToArray();
                await _stream.WriteAsync(data, 0, data.Length);
            }
        }
        public async ValueTask<IByteBuffer> Receive(CancellationToken token = default)
        {
            while (true)
            {
                var read = await _reader.ReadAsync(token);
                if (read.IsCanceled) throw new OperationCanceledException();

                // can we find a complete frame?
                var buffer = read.Buffer;
                if (TryParseFrame(buffer, out IByteBuffer nextMessage, out SequencePosition consumedTo))
                {
                    _reader.AdvanceTo(consumedTo);
                    return nextMessage;
                }
                _reader.AdvanceTo(buffer.Start, buffer.End);
                if (read.IsCompleted)
                    throw new InvalidOperationException("EOF");
            }
        }
        private static bool TryParseFrame(ReadOnlySequence<byte> buffer, out IByteBuffer nextMessage, out SequencePosition consumedTo)
        {
            // find the end-of-line marker
            var eol = buffer.PositionOf((byte)'\n');
            if (eol == null)
            {
                nextMessage = default;
                consumedTo = default;
                return false;
            }

            // read past the line-ending
            consumedTo = buffer.GetPosition(1, eol.Value);
            // consume the data
            var payload = buffer.Slice(0, eol.Value);
            nextMessage = Unpooled.WrappedBuffer(payload.ToArray());
            return true;
        }
        public ValueTask Dispose()
        {
           return new ValueTask(Task.CompletedTask);
        }
        private static ValueTask<bool> Flush(PipeWriter writer)
        {
            try
            {
                bool GetResult(FlushResult flush)
                    // tell the calling code whether any more messages
                    // should be written
                    => !(flush.IsCanceled || flush.IsCompleted);

                async ValueTask<bool> Awaited(ValueTask<FlushResult> incomplete)
                    => GetResult(await incomplete);

                // apply back-pressure etc
                var flushTask = writer.FlushAsync();

                return flushTask.IsCompletedSuccessfully
                    ? new ValueTask<bool>(GetResult(flushTask.Result))
                    : Awaited(flushTask);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ValueTask<bool>(false);
            }
        }
    }
}
