using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Producer.Actors;
using Sample.Command;

namespace Producer
{
    class Program
    {//https://stackoverflow.com/questions/58308230/orleans-akka-net-problem-with-understanding-the-actor-model
        static void Main(string[] args)
        {
            var config = File.ReadAllText("host.hocon");
            var actorSystem = ActorSystem.Create("SampleSystem", ConfigurationFactory.ParseString(config));
            var conct = new ConcurrentDictionary<Type, object>();
            var y = conct.Take(100);
            var sampleActor = actorSystem.ActorOf(SamplePersistentActor.Prop(), "utcreader-2");
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                sampleActor.Tell(new ReadSystemCurrentTimeUtc());
                
               // Console.WriteLine("Tell Actor");
            }
        }
    }
}
