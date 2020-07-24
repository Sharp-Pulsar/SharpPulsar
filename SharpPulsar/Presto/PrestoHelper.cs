using System;

namespace SharpPulsar.Presto
{
    public class PrestoHelper
    {
        public static Type MapType(string Type)
        {
            return Type.GetType();
        }
    }
}
