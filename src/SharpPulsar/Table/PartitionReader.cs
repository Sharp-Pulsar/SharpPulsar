using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using App.Metrics.Concurrency;
using SharpPulsar.Table.Messages;

namespace SharpPulsar.Table
{
    internal class PartitionReader<T>: ReceiveActor
    {
        private readonly ILoggingAdapter _log;
        private readonly Reader<T> _reader;
        private readonly IActorRef _parent;
        private readonly AtomicLong _messagesRead;
        public PartitionReader(Reader<T> reader)
        {
            _parent = Context.Parent;
            _reader = reader;   
            _log = Context.GetLogger(); 
            var startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _messagesRead = new AtomicLong();    
            /*Akka.Dispatch.ActorTaskScheduler.RunTask(async() =>
            {
               await  ReadAllExistingMessages(reader, startTime, _messagesRead);                
            });*/
            ReadAllExistingMessages(reader, startTime, _messagesRead);
        }
        protected override void PostStop()
        {
            _reader.Close();    
            base.PostStop();
        }
        private async ValueTask ReadAllExistingMessages(Reader<T> reader, long startTime, AtomicLong messagesRead)
        {
            try
            {
                var hasMessage = await reader.HasMessageAvailableAsync();
                if (hasMessage)
                {
                    try
                    {
                        var msg = await reader.ReadNextAsync();
                        if(msg != null)
                        {
                            messagesRead.Increment();
                            _parent.Tell(new HandleMessage<T>(msg));
                        }
                        await ReadAllExistingMessages(reader, startTime, messagesRead);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.ToString());
                    }
                }
                else
                {
                    var endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    var durationMillis = endTime - startTime;
                    _log.Info("Started table view for topic {} - Replayed {} messages in {} seconds", reader.Topic, messagesRead, durationMillis / 1000.0);
                    
                    await ReadTailMessages(reader);
                }
            }
            catch(Exception ex) 
            {
                _log.Error(ex.ToString());
            }
        }

        private async ValueTask ReadTailMessages(Reader<T> reader)
        {
            var msg = await reader.ReadNextAsync();
            if(msg != null)
            {
                _parent.Tell(new HandleMessage<T>(msg));
            }
            await ReadTailMessages(reader);
        }
        public static Props Prop(Reader<T> reader)
        {
            return Props.Create(()=> new PartitionReader<T>(reader)); 
        }
    }
}
