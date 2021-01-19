using BAMCIS.Util.Concurrent;
using SharpPulsar.Interfaces;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.User
{
    public class Reader<T> : IReader<T>
    {
        public string Topic => throw new NotImplementedException();

        public bool Connected => throw new NotImplementedException();

        public ValueTask CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool HasMessageAvailable()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> HasMessageAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public bool HasReachedEndOfTopic()
        {
            throw new NotImplementedException();
        }

        public IMessage<T> ReadNext()
        {
            throw new NotImplementedException();
        }

        public IMessage<T> ReadNext(int timeout, TimeUnit unit)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IMessage<T>> ReadNextAsync()
        {
            throw new NotImplementedException();
        }

        public void Seek(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public void Seek(long timestamp)
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(IMessageId messageId)
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(long timestamp)
        {
            throw new NotImplementedException();
        }
    }
}
