using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpPulsar.Util
{
    //https://blog.adamfurmanek.pl/2018/08/18/trivial-scheduledthreadpoolexecutor-in-c/
    public class ScheduledThreadPoolExecutor
    {
        public int ThreadCount => _threads.Length;
        public EventHandler<System.Exception> OnException;

        private readonly ManualResetEvent _waiter;
        private readonly Thread[] _threads;
        private readonly SortedSet<(DateTime dateTime, Action action)> _queue;

        public ScheduledThreadPoolExecutor(int threadCount)
        {
            _waiter = new ManualResetEvent(false);
            _queue = new SortedSet<(DateTime dateTime, Action action)>();
            OnException += (o, e) => { };
            _threads = Enumerable.Range(0, threadCount).Select(i => new Thread(RunLoop)).ToArray();
            foreach (var thread in _threads)
            {
                thread.Start();
            }
        }

        private void RunLoop()
        {
            while (true)
            {
                var sleepingTime = TimeSpan.MaxValue;
                var needToSleep = true;
                Action task = null;

                try
                {
                    lock (_waiter)
                    {
                        if (_queue.Any())
                        {
                            if (_queue.First().dateTime.Millisecond <= DateTime.Now.Millisecond)
                            {
                                task = _queue.First().action;
                                _queue.Remove(_queue.First());
                                needToSleep = false;
                            }
                            else
                            {
                                sleepingTime = _queue.First().dateTime - DateTime.Now;
                            }
                        }
                    }

                    if (needToSleep)
                    {
                        _waiter.WaitOne((int)sleepingTime.TotalMilliseconds);
                    }
                    else
                    {
                        task();
                    }
                }
                catch (System.Exception e)
                {
                    OnException(task, e);
                }
            }
        }
        public void Schedule(Action action, TimeSpan initialDelay)
        {
            Schedule(action, DateTime.Now + initialDelay);
        }
        private void Schedule(Action action, DateTime time)
        {
            lock (_waiter)
            {
                _queue.Add((time, action));
            }

            _waiter.Set();
        }
        public void ScheduleWithFixedDelay(Action action, TimeSpan initialDelay, TimeSpan delay)
        {
            Schedule(() =>
                {
                    action();
                    ScheduleWithFixedDelay(action, delay, delay);
                }, DateTime.Now + initialDelay);
        }
        public void ScheduleAtFixedRate(Action action, TimeSpan initialDelay, TimeSpan delay)
        {
            var scheduleTime = DateTime.Now + initialDelay;

            void RegisterTask()
            {
                Schedule(() =>
                {
                    action();
                    scheduleTime += delay;
                    RegisterTask();
                }, scheduleTime);
            }

            RegisterTask();
        }
    }
}
