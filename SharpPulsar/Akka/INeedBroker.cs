
namespace SharpPulsar.Akka
{
    /// <summary>
    /// Used to Ask for broker
    /// </summary>
    public sealed class INeedBroker
    {
        public static INeedBroker Instance = new INeedBroker(); 
    }
}
