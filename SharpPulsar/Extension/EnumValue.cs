using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPulsar.Extension
{
    public static class EnumValue
    {
        public static T GetCompressionTypeValue<T>(this Api.ICompressionType compression)
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList()[(int) compression];
        }
    }
}
