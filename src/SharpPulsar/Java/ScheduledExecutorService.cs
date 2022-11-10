using System.Collections.Generic;
using System.Threading;

namespace SharpPulsar.Java
{
    public class ScheduledExecutorService//線程池
    {
        const int futureAutoScale = 3;//自動備用線程數量
        public List<ScheduledFuture> futureList;
        public Queue<ScheduledFuture> handleVoid;
        Thread thread;
        public ScheduledExecutorService()
        {
            futureList = new List<ScheduledFuture>();
            handleVoid = new Queue<ScheduledFuture>();
            AutoScaleFuture();
            thread = new Thread(() => { while (true) AutoScaleFuture(); });
            thread.IsBackground = true;
            thread.Start();
        }
        ~ScheduledExecutorService()
        {
            foreach (ScheduledFuture fu in futureList)
                fu.Dispose();
            while (handleVoid.Count > 0)
                handleVoid.Dequeue().Dispose();
        }
        void AutoScaleFuture()
        {//自動調整備用線程數量
            if (handleVoid.Count < 2)
            {
                int i = futureAutoScale;
                while (i-- > 0)
                {
                    ScheduledFuture future = new ScheduledFuture(AddHandleVoid);
                    handleVoid.Enqueue(future);
                }
            }
            while (handleVoid.Count > futureAutoScale + 1)
            {
                handleVoid.Dequeue().Dispose();
            }
        }
        void AddHandleVoid(ScheduledFuture future)//將被關閉的線程轉移到備用線程中
        {
            if (futureList.Contains(future))
            {
                futureList.Remove(future);
                handleVoid.Enqueue(future);
            }
        }

        public ScheduledFuture Schedule(Runnable runnable, int time, long timeSpan)
        {
            return ScheduleAtFixedRate(runnable, time, -1, timeSpan);
        }
        public ScheduledFuture ScheduleAtFixedRate(Runnable runnable, int waitTime, int cycleTime, long timeSpan)
        {
            while (handleVoid.Count < 1) { }//備用線程庫沒有足夠的線程則等待線程分配
            ScheduledFuture future = handleVoid.Dequeue();
            future.Reset(runnable, waitTime, cycleTime, timeSpan);
            futureList.Add(future);
            return future;
        }
        public void ShutdownNow()//關閉所有正在使用的線程
        {
            int i = futureList.Count;
            while (i-- > 0)
                futureList[i].Cancel();
        }
        public bool IsShutdown()
        {
            if (futureList.Count == 0)
                return true;
            return false;
        }
    }
}
