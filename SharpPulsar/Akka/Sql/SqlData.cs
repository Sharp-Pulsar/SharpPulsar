using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Akka.Sql
{
    public sealed class SqlData
    {
        public SqlData(bool hasRow, int row, Dictionary<string, object> data, Dictionary<string, object> metadata, bool hasError = false, Exception exception = null)
        {
            HasRow = hasRow;
            Row = row;
            Data = data;
            Metadata = metadata;
            HasError = hasError;
            Exception = exception;
        }
        public bool HasError { get; }
        public Exception Exception { get; }
        public bool HasRow { get; }
        public int Row { get; }
        public Dictionary<string, object> Data { get; }
        public Dictionary<string, object> Metadata { get; }
    }
}
