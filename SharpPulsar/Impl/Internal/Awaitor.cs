﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal
{
    public sealed class Awaitor<T, Result> : IDisposable
    {
        private readonly Dictionary<T, TaskCompletionSource<Result>> _items;

        public Awaitor() => _items = new Dictionary<T, TaskCompletionSource<Result>>();

        public Task<Result> CreateTask(T item)
        {
            var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
            _items.Add(item, tcs);
            return tcs.Task;
        }

        public void SetResult(T item, Result result)
        {
#if NETSTANDARD2_0
            var tcs = _items[item];
            _items.Remove(item);
#else
            _items.Remove(item, out var tcs);
#endif
            tcs.SetResult(result);
        }

        public void Dispose()
        {
            foreach (var item in _items.Values)
            {
                item.SetCanceled();
            }

            _items.Clear();
        }
    }
}
