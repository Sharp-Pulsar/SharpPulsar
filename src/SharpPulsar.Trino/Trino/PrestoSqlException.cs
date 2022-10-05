namespace SharpPulsar.Trino.Trino
{
    public class PrestoSqlException : Exception
    {
        public QueryError QueryError { get; internal set; }
        internal PrestoSqlException(QueryError error) : base(error.Message)
        {
            QueryError = error;
        }

        internal static PrestoSqlException Create(QueryError error)
        {
            switch (error.ErrorType)
            {
                case "USER_ERROR": return new PrestoSqlUserException(error);
                case "INTERNAL_ERROR": return new PrestoSqlInternalException(error);
                case "INSUFFICIENT_RESOURCES": return new PrestoSqlInsufficientResourceException(error);
                case "EXTERNAL": return new PrestoSqlExternalException(error);
                default:
                    return new PrestoSqlException(error);
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


    public class PrestoSqlUserException : PrestoSqlException
    {
        internal PrestoSqlUserException(QueryError error) : base(error) { }
    }

    public class PrestoSqlInternalException : PrestoSqlException
    {
        internal PrestoSqlInternalException(QueryError error) : base(error) { }
    }

    public class PrestoSqlInsufficientResourceException : PrestoSqlException
    {
        internal PrestoSqlInsufficientResourceException(QueryError error) : base(error) { }
    }

    public class PrestoSqlExternalException : PrestoSqlException
    {
        internal PrestoSqlExternalException(QueryError error) : base(error) { }
    }
}
