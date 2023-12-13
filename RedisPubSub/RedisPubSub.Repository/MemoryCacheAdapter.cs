using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace BilbolStack.RedisPubSub.Repository
{
    public class MemoryCacheAdapter : IMemoryCacheAdapter
    {
        private const int CACHE_TIME_MINUTES = 1;
        private IMemoryCache _cache;
        private HashSet<string> _keys;

        public MemoryCacheAdapter(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _keys = new HashSet<string>();
        }

        public void Clear(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        public void ClearAll()
        {
            var oldCache = _cache;
            _cache = new MemoryCache(new MemoryCacheOptions());
            oldCache.Dispose();
        }

        public void ClearByPattern(string pattern)
        {
            Regex reg = new Regex(pattern);
            foreach (var key in _keys)
            {
                if (reg.IsMatch(key))
                {
                    Clear(key);
                }
            }
        }

        public T Get<T>(string cacheKey)
        {
            _cache.TryGetValue(cacheKey, out T cacheEntry);
            return cacheEntry;
        }

        public T Get<T>(string cacheKey, Func<T> action)
        {
            if (_cache.TryGetValue(cacheKey, out T cacheEntry))
            {
                return cacheEntry;
            }

            var result = action.Invoke();

            Set(cacheKey, result);

            return result;
        }

        public bool IsSet(string cacheKey)
        {
            return _cache.TryGetValue(cacheKey, out _);
        }

        public void Set(string cacheKey, object obj)
        {
            Set(cacheKey, obj, CACHE_TIME_MINUTES);
        }

        public void Set(string cacheKey, object obj, int minutes)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(minutes));
            _cache.Set(cacheKey, obj, cacheEntryOptions);
            _keys.Add(cacheKey);
        }
    }
}
