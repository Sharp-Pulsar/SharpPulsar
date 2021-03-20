using System.Collections.Generic;

namespace SharpPulsar.Sql.Message
{
    public sealed class DataResponse : IQueryResponse
    {
        public DataResponse(Dictionary<string, object> data, Dictionary<string, object> metadata)
        {
            Data = data;
            Metadata = metadata;
        }

        public Dictionary<string, object> Data { get; }
        public Dictionary<string, object> Metadata { get; }
    }
}
