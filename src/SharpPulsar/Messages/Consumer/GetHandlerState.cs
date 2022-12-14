
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct GetHandlerState
    {
        public static GetHandlerState Instance = new GetHandlerState();
    }
}
