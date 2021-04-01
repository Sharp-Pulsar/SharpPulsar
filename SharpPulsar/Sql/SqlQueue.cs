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
            _buffer.TryReceive(out var msg);
            return msg;
        }
        public IList<T> All()
        {
           _ = _buffer.TryReceiveAll(out var messages);
            return messages;
        }
    }
}
