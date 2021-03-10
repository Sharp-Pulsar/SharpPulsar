using Akka.Actor;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Proto;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class ActorEx
    {
        public static async Task<T> AskFor<T>(this IActorRef actorRef, object message, int timeoutInSeconds = 60)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            /*return Task.Run(async () => 
            {
                var re = await actorRef.Ask<T>(message, timeout: TimeSpan.FromSeconds(timeoutInSeconds));
                return re;
            }).Result;*/
            return await actorRef.Ask<T>(message, timeout: TimeSpan.FromSeconds(timeoutInSeconds)).ConfigureAwait(false);
        }
        
        public static async Task<object> AskFor(this IActorRef actorRef, object message, int timeoutInSeconds = 60)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return await actorRef.Ask(message, TimeSpan.FromSeconds(timeoutInSeconds)).ConfigureAwait(false);
        }
        public static CommandSubscribe.InitialPosition ValueOf(this SubscriptionInitialPosition position)
        {
            if (position.Value == 0)
                return CommandSubscribe.InitialPosition.Latest;
            return CommandSubscribe.InitialPosition.Earliest;
        }
    }
}
