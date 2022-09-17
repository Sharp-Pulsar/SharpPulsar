
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpPulsar.Admin.Admin.Models;
using SharpPulsar.Admin.Model;
using BookieInfo = SharpPulsar.Admin.Model.BookieInfo;
using BookiesClusterInfo = SharpPulsar.Admin.Model.BookiesClusterInfo;

namespace SharpPulsar.Admin.interfaces
{
    /// <summary>
    /// Admin interface for bookies rack placement management.
    /// </summary>
    public interface IBookies
    {

        /// <summary>
        /// Gets the rack placement information for all the bookies in the cluster.
        /// </summary>

        IDictionary<string, IDictionary<string, BookieInfo>> GetBookiesRackInfo(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Gets the rack placement information for all the bookies in the cluster asynchronously.
        /// </summary>
        ValueTask<IDictionary<string, IDictionary<string, BookieInfo>>> GetBookiesRackInfoAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets discovery information for all the bookies in the cluster.
        /// </summary>

        BookiesClusterInfo GetBookies(Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Gets discovery information for all the bookies in the cluster asynchronously.
        /// </summary>
        ValueTask<BookiesClusterInfo> GetBookiesAsync(Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the rack placement information for a specific bookie in the cluster.
        /// </summary>
        BookieInfo GetBookieRackInfo(string bookieAddress, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Gets the rack placement information for a specific bookie in the cluster asynchronously.
        /// </summary>
        ValueTask<BookieInfo> GetBookieRackInfoAsync(string bookieAddress, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Remove rack placement information for a specific bookie in the cluster.
        /// </summary
        void DeleteBookieRackInfo(string bookieAddress, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Remove rack placement information for a specific bookie in the cluster asynchronously.
        /// </summary>
        ValueTask DeleteBookieRackInfoAsync(string bookieAddress, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Updates the rack placement information for a specific bookie in the cluster.
        /// </summary>
       
        void UpdateBookieRackInfo(string bookieAddress, string group, BookieInfo bookieInfo, Dictionary<string, List<string>> customHeaders = null);

        /// <summary>
        /// Updates the rack placement information for a specific bookie in the cluster asynchronously.
        /// </summary>
        ValueTask UpdateBookieRackInfoAsync(string bookieAddress, string group, BookieInfo bookieInfo, Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
