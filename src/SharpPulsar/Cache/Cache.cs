using System;
using System.Collections.Concurrent;

namespace SharpPulsar.Cache
{
    //https://stackoverflow.com/questions/47874173/dictionary-cache-with-expiration-time
    public class Cache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
        private readonly TimeSpan _expiration;
        private readonly int? _maximumSize;

        public Cache(TimeSpan expiration, int? maxsize = null)
        {
            _expiration = expiration;
            _maximumSize = maxsize;
        }

        public void Put(TKey key, TValue value)
        {
            var item = new CacheItem<TValue>(value);
            if(_maximumSize.HasValue && item.Size > _maximumSize)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} has exceeded the max allowed: [{item.Size.Value}>{_maximumSize.Value}]");
            
            _cache[key] = new CacheItem<TValue>(value);
        }
        public TValue Get(TKey key)
        {
            if (!_cache.ContainsKey(key))
                return default;
            var cached = _cache[key];
            if (cached.Accessed)
            {
                if (DateTimeOffset.Now - cached.AccessedTime >= cached.ExpiresAfter)
                {
                    _cache.TryRemove(key, out _);
                    return default;
                }
            }
            else
            {
                _cache[key].Accessed = true;
                _cache[key].AccessedTime = DateTimeOffset.Now;
                _cache[key].ExpiresAfter = _expiration;
            }
            return cached.Value;
        }
        public TValue Get(TKey key, Func<TKey, TValue> func)
        {
            if (!_cache.ContainsKey(key))
            {
                var value = func(key);
                var item = new CacheItem<TValue>(value);
                if (_maximumSize.HasValue && item.Size > _maximumSize)
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(value)} has exceeded the max allowed: [{item.Size.Value}>{_maximumSize.Value}]");

                _cache[key] = new CacheItem<TValue>(value);
                return _cache[key].Value;
            }
            var cached = _cache[key];
            if (cached.Accessed)
            {
                if (DateTimeOffset.Now - cached.AccessedTime >= cached.ExpiresAfter)
                {
                    _cache.TryRemove(key, out _);
                    return default;
                }
            }
            else
            {
                _cache[key].Accessed = true;
                _cache[key].AccessedTime = DateTimeOffset.Now;
                _cache[key].ExpiresAfter = _expiration;
            }
            return cached.Value;
        }
    }
}
