using System.Collections.Generic;

namespace SharpPulsar.Extension
{
    public static class DictionaryEx
    {
        public static string GetOrDefault(this IDictionary<string, string> dict, string key, string defalt)
        {
            if (dict.TryGetValue(key, out var value))
                return value;
            else
                return defalt;
        }
        internal static ChunkedMessageCtx RemoveEx(this IDictionary<string, ChunkedMessageCtx> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                dict.Remove(key);
                return value;
            }
                
           return null;
        }
    }
}
