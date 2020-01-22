using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IDequeue<T>
    {
        ValueTask<T> Dequeue(CancellationToken cancellationToken = default);
    }
}
