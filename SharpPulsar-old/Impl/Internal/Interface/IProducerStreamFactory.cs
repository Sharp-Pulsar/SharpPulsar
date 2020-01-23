using System.Threading;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal.Interface
{
    public interface IProducerStreamFactory
    {
        Task<IProducerStream> CreateStream(IProducerProxy proxy, CancellationToken cancellationToken = default);
    }
}
