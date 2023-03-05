
namespace SharpPulsar.Messages.Requests
{
    public readonly record struct GetLastSequenceId
    {
        public static GetLastSequenceId Instance = new GetLastSequenceId();
    }
    
}
