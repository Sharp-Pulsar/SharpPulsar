using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPulsar.Stole
{
    public sealed class SequenceBuilder where T : notnull
    {
        private readonly LinkedList<ReadOnlyMemory> _elements;

        public SequenceBuilder() => _elements = new LinkedList<ReadOnlyMemory>();

        public SequenceBuilder Prepend(ReadOnlyMemory memory)
        {
            _elements.AddFirst(memory);
            return this;
        }

        public SequenceBuilder Prepend(ReadOnlySequence sequence)
        {
            LinkedListNode<ReadOnlyMemory>? index = null;

            foreach (var memory in sequence)
            {
                if (index is null)
                    index = _elements.AddFirst(memory);
                else
                    index = _elements.AddAfter(index, memory);
            }

            return this;
        }

        public SequenceBuilder Append(ReadOnlyMemory memory)
        {
            _elements.AddLast(memory);
            return this;
        }

        public SequenceBuilder Append(ReadOnlySequence sequence)
        {
            foreach (var memory in sequence)
                _elements.AddLast(memory);

            return this;
        }

        public long Length => _elements.Sum(e => e.Length);

        public ReadOnlySequence Build()
        {
            if (_elements.Count == 0)
                return new ReadOnlySequence();

            Segment? start = null;
            Segment? current = null;

            foreach (var element in _elements)
            {
                if (current is null)
                {
                    current = new Segment(element);
                    start = current;
                }
                else
                    current = current.CreateNext(element);
            }

            return new ReadOnlySequence(start, 0, current, current!.Memory.Length);
        }

        private sealed class Segment : ReadOnlySequenceSegment
        {
            public Segment(ReadOnlyMemory memory, long runningIndex = 0)
            {
                Memory = memory;
                RunningIndex = runningIndex;
            }

            public Segment CreateNext(ReadOnlyMemory memory)
            {
                var segment = new Segment(memory, RunningIndex + Memory.Length);
                Next = segment;
                return segment;
            }
        }
    }
}
