
namespace SharpPulsar.Messages.Consumer
{
    public sealed class HandlerStateResponse
    {
        public HandlerState.State State { get; }
        public HandlerStateResponse(HandlerState.State state)
        {
            State = state;
        }
    }
}
