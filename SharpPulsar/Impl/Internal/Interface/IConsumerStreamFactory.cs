using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IConsumerStreamFactory
    {
        Task<IConsumerStream> CreateStream(IConsumerProxy proxy, CancellationToken cancellationToken = default);
    }
}
