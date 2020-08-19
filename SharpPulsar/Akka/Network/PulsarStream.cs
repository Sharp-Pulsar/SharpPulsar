﻿/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Akka.Event;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Akka.Network
{
    public sealed class PulsarStream
    {
        private const long PauseAtMoreThan10Mb = 10485760;
        private const long ResumeAt5MbOrLess = 5242881;

        private readonly Stream _stream;
        private readonly PipeReader _reader;
        private readonly PipeWriter _writer;
        private int _isDisposed;
        private ILoggingAdapter _log;

        public PulsarStream(Stream stream, ILoggingAdapter log)
        {
            _log = log;
            _stream = stream;
            var options = new PipeOptions(pauseWriterThreshold: PauseAtMoreThan10Mb, resumeWriterThreshold: ResumeAt5MbOrLess);
            var pipe = new Pipe(options);
            _reader = pipe.Reader;
            _writer = pipe.Writer;
        }

        public async Task Send(ReadOnlySequence<byte> sequence)
        {
            ThrowIfDisposed();
            foreach (var segment in sequence)
            {
                await _stream.WriteAsync(segment);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
                return;
            await _stream.DisposeAsync();
        }

        private async Task FillPipe(CancellationToken cancellationToken)
        {
            await Task.Yield();

            try
            {
                while (true)
                {
                    var memory = _writer.GetMemory(84999); // LOH - 1 byte

                    var bytesRead = await _stream.ReadAsync(memory, cancellationToken);

                    if (bytesRead == 0)
                        break;

                    _writer.Advance(bytesRead);

                    var result = await _writer.FlushAsync(cancellationToken);
                    if (result.IsCompleted)
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            finally
            {
                _writer.Complete();
            }
        }

        public async IAsyncEnumerable<ReadOnlySequence<byte>> Frames([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _ = FillPipe(cancellationToken);

            try
            {
                while (true)
                {
                    var result = await _reader.ReadAsync(cancellationToken);
                    var buffer = result.Buffer;

                    while (true)
                    {
                        if (buffer.Length < 4)
                            break;

                        var frameSize = buffer.ReadUInt32(0, true);
                        var totalSize = frameSize + 4;
                        if (buffer.Length < totalSize)
                            break;

                        yield return new ReadOnlySequence<byte>(buffer.Slice(4, frameSize).ToArray());

                        buffer = buffer.Slice(totalSize);
                    }

                    if (result.IsCompleted)
                        break;

                    _reader.AdvanceTo(buffer.Start);
                }
            }
            finally
            {
                _reader.Complete();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed != 0)
                throw new ObjectDisposedException(nameof(PulsarStream));
        }
    }
}
