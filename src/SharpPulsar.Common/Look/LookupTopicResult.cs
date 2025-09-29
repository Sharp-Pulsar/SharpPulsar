
using System.Net;

namespace SharpPulsar.Common.Look
{
    public class LookupTopicResult
    {
        public IPEndPoint LogicalAddress {  get; set; }
        public IPEndPoint PhysicalAddress {  get; set; }
        public bool IsUseProxy { get; set; }
    }

}
