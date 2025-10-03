//---------------------------------------------------------------------------------------------------------
//
//	This class is used to replace calls to some Java HashMap or Hashtable methods.
//---------------------------------------------------------------------------------------------------------
namespace SharpPulsar.Shared
{
    public static class HashMapHelper
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
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue ret;
            if (dictionary.TryGetValue(key, out ret))
                return ret;
            else
                return defaultValue;
        }

        public static void PutAll<TKey, TValue>(this IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2)
        {
            if (d2 is null)
                throw new NullReferenceException();

            foreach (TKey key in d2.Keys)
            {
                d1[key] = d2[key];
            }
        }
    }
}
