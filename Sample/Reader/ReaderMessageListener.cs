using System;
using Producer;
using SharpPulsar.Api;

namespace Samples.Reader
{
    public class ReaderMessageListener:IReaderListener
    {
        public void Received(IMessage msg)
        {
            if (msg.Value is Students students)
                Console.WriteLine($"{msg.TopicName} >> {students.Name}");
        }
    }
}
