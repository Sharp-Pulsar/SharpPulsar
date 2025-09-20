using Akka.Actor;
using Akka.Dispatch;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Proto;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class ActorEx
    {
        public static async Task<T> AskFo<T>(this IActorRef actorRef, object message, int timeoutInSeconds = 120)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            
            return await actorRef.Ask<T>(message, timeout: TimeSpan.FromSeconds(timeoutInSeconds));
        }
        
        public static async Task<object> AskFo(this IActorRef actorRef, object message, int timeoutInSeconds = 120)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return await actorRef.Ask(message, TimeSpan.FromSeconds(timeoutInSeconds));
        }
        public static ActorTaskScheduler InternalPinnedExecutor(this IActorRef actorRef, ActorTaskScheduler actorTaskScheduler = null)
        {
            if (actorTaskScheduler != null)
                return actorTaskScheduler;
            
            return null;
        }
        public static CommandSubscribe.InitialPosition ValueOf(this SubscriptionInitialPosition position)
        {
            if (position.Value == 0)
                return CommandSubscribe.InitialPosition.Latest;
            return CommandSubscribe.InitialPosition.Earliest;
        }
    }
}
