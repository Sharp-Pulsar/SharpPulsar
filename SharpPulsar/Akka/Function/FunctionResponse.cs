namespace SharpPulsar.Akka.Function
{
    public sealed class FunctionResponse
    {
        public FunctionResponse(object response)
        {
            Response = response;
        }

        public object Response { get; }
    }
}
