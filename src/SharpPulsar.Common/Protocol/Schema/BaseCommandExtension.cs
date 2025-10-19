using Pulsar.Proto;

namespace SharpPulsar.Common.Protocol.Schema
{
    public static class BaseCommandExtension
    {
        public static BaseCommand ToBaseCommand(this BaseCommand.Types.Type type)
        {
            return new BaseCommand
            {
                Type = type
            };
        }        
    }
}
