namespace SharpPulsar.Trino.Trino
{
    public class TrinoSqlException : Exception
    {
        public QueryError QueryError { get; internal set; }
        internal TrinoSqlException(QueryError error) : base(error.Message)
        {
            QueryError = error;
        }

        internal static TrinoSqlException Create(QueryError error)
        {
            switch (error.ErrorType)
            {
                case "USER_ERROR": return new TrinoSqlUserException(error);
                case "INTERNAL_ERROR": return new TrinoSqlInternalException(error);
                case "INSUFFICIENT_RESOURCES": return new TrinoSqlInsufficientResourceException(error);
                case "EXTERNAL": return new TrinoSqlExternalException(error);
                default:
                    return new TrinoSqlException(error);
            }
        }

        public string GetMessage() { return QueryError.Message; }
        public string GetSqlState() { return QueryError.SqlState; }
        public int GetErrorCode() { return QueryError.ErrorCode; }
        public string GetErrorName() { return QueryError.ErrorName; }
        public string GetErrorType() { return QueryError.ErrorType; }
        public ErrorLocation GetErrorLocation() { return QueryError.ErrorLocation; }
        public FailureInfo GetFailureInfo() { return QueryError.FailureInfo; }
    }


    public class TrinoSqlUserException : TrinoSqlException
    {
        internal TrinoSqlUserException(QueryError error) : base(error) { }
    }

    public class TrinoSqlInternalException : TrinoSqlException
    {
        internal TrinoSqlInternalException(QueryError error) : base(error) { }
    }

    public class TrinoSqlInsufficientResourceException : TrinoSqlException
    {
        internal TrinoSqlInsufficientResourceException(QueryError error) : base(error) { }
    }

    public class TrinoSqlExternalException : TrinoSqlException
    {
        internal TrinoSqlExternalException(QueryError error) : base(error) { }
    }
}
