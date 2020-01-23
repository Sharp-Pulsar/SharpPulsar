namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IEnqueue<T>
    {
        void Enqueue(T item);
    }
}
