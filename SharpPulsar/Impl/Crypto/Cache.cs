using System;
using System.Collections.Generic;

namespace SharpPulsar.Impl.Crypto
{
    //https://stackoverflow.com/questions/47874173/dictionary-cache-with-expiration-time
    public class Cache<TKey, TValue>
    {
        private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();
        private readonly int _expiration;
        
        public Cache(int expiration)
        {
            _expiration = expiration;
        }

        public void Put(TKey key, TValue value)
        {
            _cache[key] = new CacheItem<TValue>(value);
        }
        public TValue Get(TKey key)
        {
            if (!_cache.ContainsKey(key)) 
                return default(TValue);
            var cached = _cache[key];
            if (cached.Accessed)
            {
                if (DateTimeOffset.Now - cached.AccessedTime >= cached.ExpiresAfter)
                {
                    _cache.Remove(key);
                    return default(TValue);
                }
            }
            else
            {
                _cache[key].Accessed = true;
                _cache[key].AccessedTime = DateTimeOffset.Now;
                _cache[key].ExpiresAfter = TimeSpan.FromHours(_expiration);
            }
            return cached.Value;
        }
    }
}
