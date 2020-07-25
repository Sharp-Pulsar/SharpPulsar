using System;
using System.Collections.Generic;
using System.Text;
using SharpPulsar.Akka.Sql.Message;

namespace SharpPulsar.Akka.Sql
{
    public sealed class SqlData
    {
        public SqlData(IQueryResponse response)
        {
            Response = response;
        }
        public IQueryResponse Response { get; }
    }
}
