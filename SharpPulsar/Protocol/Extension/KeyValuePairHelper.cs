using System.Collections.Generic;

namespace SharpPulsar.Protocol.Extension
{
	internal static class KeyValuePairHelper
    {
        public static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var entries = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var keyValuePair in dictionary)
            {
                entries.Add(keyValuePair);
            }
            return entries;
        }

        public static TValue GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out var ret);
            return ret;
        }
    }
}
