using System;
using SharpPulsar.Api;

namespace Samples.Reader
{
    public class ReaderMessageListener:IReaderListener
    {
        public void Received(IMessage msg)
        {
            var students = msg.ToTypeOf<Students>();
            Console.WriteLine($"Reader >> {students.Name}");
        }
    }
}
