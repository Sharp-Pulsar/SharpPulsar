
namespace SharpPulsar.Api
{
    /// <summary>
    /// Protcol type to determine type of proxy routing when client connects to proxy using
    /// <seealso cref="ClientBuilder::proxyServiceUrl"/>.
    /// </summary>
    public enum ProxyProtocol
    {
        /// <summary>
        /// Follows SNI-routing
        /// https://docs.trafficserver.apache.org/en/latest/admin-guide/layer-4-routing.en.html#sni-routing.
        /// 
        /// </summary>
        SNI
    }
}
