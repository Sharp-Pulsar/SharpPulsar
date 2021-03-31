using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace SharpPulsar.Sql
{
    public class SqlQueue<T>
    {
        private BufferBlock<T> _buffer;
        public SqlQueue()
        {
            _buffer = new BufferBlock<T>();
        }
        public bool Post(T data)
        {
            return _buffer.Post(data);
        }
        public T Receive()
        {
            return _buffer.Receive();
        }
        public IList<T> All()
        {
           _ = _buffer.TryReceiveAll(out var messages);
            return messages;
        }
    }
}
