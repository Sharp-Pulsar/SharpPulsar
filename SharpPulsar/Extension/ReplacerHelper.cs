using System.Text.RegularExpressions;

namespace SharpPulsar.Extension
{
    public static class ReplacerHelper
    {
        public static string ToAkkaNaming(this string topic)
        {
            return Regex.Replace(topic, @"[^\w\d]", "");
        }
    }
}
