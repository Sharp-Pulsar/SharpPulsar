using System;

namespace SharpPulsar.Akka.Handlers
{
    public class ProducerHandler:IHandler
    {
        public void Capture(object data)
        {
            Console.WriteLine(data);
        }

        public void MessageId(object messageid)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine(messageid);
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}
