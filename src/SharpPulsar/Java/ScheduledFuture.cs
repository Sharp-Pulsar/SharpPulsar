using System;
using System.Threading;

namespace SharpPulsar.Java
{
    public class ScheduledFuture : IDisposable//線程
    {
        Thread thread;
        Action<ScheduledFuture> thisPool;

        public TimeSpan waitTime;
        public TimeSpan cycleTime;
        public bool isCycle;

        event Action thisEvent;
        public void Run()
        {
            Thread.Sleep(waitTime);
            if (thisEvent != null)
            {
                if (isCycle)
                    while (true)
                    {
                        Thread.Sleep(cycleTime);
                        thisEvent();
                    }
                else
                    thisEvent();
            }
        }

        public ScheduledFuture(Action<ScheduledFuture> pool)
        {
            waitTime = new TimeSpan(0);
            cycleTime = new TimeSpan(0);
            isCycle = false;
            thisPool = pool;
            thread = new Thread(Run);
            thread.IsBackground = true;
        }
        ~ScheduledFuture()
        {
            thread.Abort();
        }
        public void Reset(Runnable ra, int waitTime, long timeSpan)
        {
            Reset(ra, waitTime, -1, timeSpan);
        }
        public void Reset(Runnable ra, int waitTime, int cycleTime, long timeSpan)
        {
            thisEvent = ra.GetEvent();
            this.waitTime = new TimeSpan(waitTime * timeSpan);
            if (cycleTime >= 0)
            {
                this.cycleTime = new TimeSpan(cycleTime * timeSpan);
                this.isCycle = true;
            }
            else
                this.isCycle = false;
            thread.Start();
        }
        public bool Cancel()
        {
            if (thread.IsAlive)
            {
                thread.Join();
                thisPool(this);
            }
            return !thread.IsAlive;
        }
        public bool IsCancel
        {
            get { return !thread.IsAlive; }
        }
        public void Dispose()
        {
            thread.Abort();
        }
    }
}
