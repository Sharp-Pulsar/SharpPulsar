using SharpPulsar.Interfaces;

namespace SharpPulsar.Extension
{
    public static class EnumValue
    {
        public static object GetCompressionTypeValue(this ICompressionType compression)
        {
            //return Enum.GetValues(typeof(object)).Cast().ToList()[(int) compression];
            return null;
        }
    }
}
