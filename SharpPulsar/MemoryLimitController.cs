using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using App.Metrics.Concurrency;

namespace SharpPulsar
{
    public class MemoryLimitController
    {
        private readonly bool InstanceFieldsInitialized = false;
        private static readonly string _mutex = "lock";
        private void InitializeInstanceFields()
        {
            //condition = mutex.newCondition();
        }


        private readonly long _memoryLimit;
        private readonly AtomicLong _currentUsage = new AtomicLong();

        public MemoryLimitController(long memoryLimitBytes)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            _memoryLimit = memoryLimitBytes;
        }

        public virtual bool TryReserveMemory(long size)
        {
            while (true)
            {
                var current = _currentUsage.GetValue();
                var newUsage = current + size;

                // We allow one request to go over the limit, to make the notification
                // path simpler and more efficient
                if (current > _memoryLimit && _memoryLimit > 0)
                {
                    return false;
                }

                if (_currentUsage.CompareAndSwap(current, newUsage))
                {
                    return true;
                }
            }
        }

        public virtual void ReserveMemory(long size)
        {
            if (!TryReserveMemory(size))
            {
                Monitor.TryEnter(_mutex);
                try
                {
                    while (!TryReserveMemory(size))
                    {
                        Monitor.Wait(_mutex);
                    }
                }
                finally
                {
                    Monitor.Exit(_mutex);
                }
            }
        }

        public virtual void ReleaseMemory(long size)
        {
            var newUsage = _currentUsage.Add(-size);
            if (newUsage + size > _memoryLimit && newUsage <= _memoryLimit)
            {
                // We just crossed the limit. Now we have more space
                Monitor.TryEnter(_mutex);
                try
                {
                    Monitor.PulseAll(_mutex);
                }
                finally
                {
                    Monitor.Exit(_mutex);
                }
            }
        }

        public virtual long CurrentUsage()
        {
            return _currentUsage.GetValue();
        }
    }

}
