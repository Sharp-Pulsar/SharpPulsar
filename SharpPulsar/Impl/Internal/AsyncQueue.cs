﻿using SharpPulsar.Impl.Internal.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal
{
    public sealed class AsyncQueue<T> : IEnqueue<T>, IDequeue<T>, IDisposable
    {
        private readonly object _lock;
        private readonly Queue<T> _queue;
        private readonly LinkedList<CancelableCompletionSource<T>> _pendingDequeues;
        private bool _isDisposed;

        public AsyncQueue()
        {
            _lock = new object();
            _queue = new Queue<T>();
            _pendingDequeues = new LinkedList<CancelableCompletionSource<T>>();
            _isDisposed = false;
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(AsyncQueue<T>));

                if (_pendingDequeues.Count > 0)
                {
                    var tcs = _pendingDequeues.First;
                    _pendingDequeues.RemoveFirst();
                    tcs.Value.SetResult(item);
                }
                else
                    _queue.Enqueue(item);
            }
        }

        public ValueTask<T> Dequeue(CancellationToken cancellationToken = default)
        {
            LinkedListNode<CancelableCompletionSource<T>>? node = null;

            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(AsyncQueue<T>));

                if (_queue.Count > 0)
                    return new ValueTask<T>(_queue.Dequeue());

                node = _pendingDequeues.AddLast(new CancelableCompletionSource<T>());
            }

            node.Value.SetupCancellation(() => Cancel(node), cancellationToken);
            return new ValueTask<T>(node.Value.Task);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;

                foreach (var pendingDequeue in _pendingDequeues)
                    pendingDequeue.Dispose();

                _pendingDequeues.Clear();
                _queue.Clear();
            }
        }

        private void Cancel(LinkedListNode<CancelableCompletionSource<T>> node)
        {
            lock (_lock)
            {
                try
                {
                    node.Value.Dispose();
                    _pendingDequeues.Remove(node);
                }
                catch { }
            }
        }
    }
}
