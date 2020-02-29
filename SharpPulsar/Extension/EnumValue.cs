using System;
using System.Linq;

namespace SharpPulsar.Extension
{
    public static class EnumValue
    {
        public static T GetCompressionTypeValue(this Api.ICompressionType compression)
        {
            return Enum.GetValues(typeof(T)).Cast().ToList()[(int) compression];
        }
    }
}
