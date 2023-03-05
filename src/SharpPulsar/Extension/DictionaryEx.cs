using System.Collections.Generic;
using SharpPulsar.Tracker;

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
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newItems)
        {
            foreach (T item in newItems)
            {
                collection.Add(item);
            }
        }
        public static HashSet<UnackMessageIdWrapper> PutIfAbsent(this Dictionary<UnackMessageIdWrapper, HashSet<UnackMessageIdWrapper>> map, UnackMessageIdWrapper key, HashSet<UnackMessageIdWrapper> value)
        {
            if (!map.ContainsKey(key))
            {
                map.Add(key, value);
                return value;
            }
                
            else
                return map[key];
        }
    }
}
