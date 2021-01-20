using Akka.Actor;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class ActorEx
    {
        public static T AskFor<T>(this IActorRef actorRef, object message)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return Task.Run(() => actorRef.Ask<T>(message)).Result;
        }
        
        public static object AskFor(this IActorRef actorRef, object message)
        {
            //https://stackoverflow.com/questions/17248680/await-works-but-calling-task-result-hangs-deadlocks#answer-32429753
            return Task.Run(() => actorRef.Ask(message)).Result;
        }
    }
}
