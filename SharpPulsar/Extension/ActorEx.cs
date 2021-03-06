using Akka.Actor;
using SharpPulsar.Common;
using SharpPulsar.Protocol.Proto;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class ActorEx
    {
        public static T AskFor<T>(this IActorRef actorRef, object message)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return Task.Run(async () => 
            {
                var re = await actorRef.Ask<T>(message);
                return re;
            }).Result;
        }
        public static T AskForState<T>(this IActorRef actorRef, object message)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return Task.Run(async () => 
            {
                var re = await actorRef.Ask<T>(message);
                return re;
            }).Result;
        }
        
        public static object AskFor(this IActorRef actorRef, object message)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return Task.Run(async () => await actorRef.Ask(message)).Result;
        }
        public static CommandSubscribe.InitialPosition ValueOf(this SubscriptionInitialPosition position)
        {
            if (position.Value == 0)
                return CommandSubscribe.InitialPosition.Latest;
            return CommandSubscribe.InitialPosition.Earliest;
        }
    }
}
