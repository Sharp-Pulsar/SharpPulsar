using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using App.Metrics.Concurrency;
using SharpPulsar.Table.Messages;
using SharpPulsar.User;

namespace SharpPulsar.Table
{
    internal class PartitionReader<T>: ReceiveActor
    {
        private ILoggingAdapter _log;
        private Reader<T> _reader;
        private IActorRef _parent;
        public PartitionReader(Reader<T> reader)
        {
            _parent = Context.Parent;
            _reader = reader;   
            _log = Context.GetLogger();
            Akka.Dispatch.ActorTaskScheduler.RunTask(async () => 
            {
                var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var messagesRead = new AtomicLong();

                var future = new TaskCompletionSource<Reader<T>>();
                ReadAllExistingMessages(reader, future, startTime, messagesRead);
                await future.Task;
            });
        }
        protected override void PostStop()
        {
            _reader.Close();    
            base.PostStop();
        }
        private void ReadAllExistingMessages(Reader<T> reader, TaskCompletionSource<Reader<T>> future, long startTime, AtomicLong messagesRead)
        {
            var hasMessage = reader.HasMessageAvailableAsync()
                .AsTask()
                .ContinueWith(task => 
                {
                    if (!task.IsFaulted)
                    {
                        var hasMessage = task.Result;

                        if (hasMessage)
                        {
                            reader.ReadNextAsync()
                            .AsTask()
                            .ContinueWith(t => 
                            { 
                                if(!t.IsFaulted)
                                {
                                    var msg = t.Result;
                                    messagesRead.Increment();
                                    _parent.Tell(new HandleMessage<T>(msg));
                                    ReadAllExistingMessages(reader, future, startTime, messagesRead);
                                }
                                else
                                    future.SetException(t.Exception);   
                            });
                        }
                        else
                        {
                            var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            var durationMillis = endTime - startTime;
                            _log.Info("Started table view for topic {} - Replayed {} messages in {} seconds", reader.Topic, messagesRead, durationMillis / 1000.0);
                            future.SetResult(reader);
                            ReadTailMessages(reader);
                        }
                    }
                });
            
        }

        private void ReadTailMessages(Reader<T> reader)
        {
            reader.ReadNextAsync().AsTask()
                .ContinueWith(task => 
                {
                    if (!task.IsFaulted)
                    {
                        var msg = task.Result;
                        _parent.Tell(new HandleMessage<T>(msg));
                        ReadTailMessages(reader);
                    }
                    else
                    {
                        _log.Info($"Reader {reader.Topic} was interrupted");
                    }
                });

        }
        public static Props Prop(Reader<T> reader)
        {
            return Props.Create(()=> new PartitionReader<T>(reader)); 
        }
    }
}
