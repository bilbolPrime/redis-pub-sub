namespace BilbolStack.RedisPubSub.Repository
{
    public class LayeredCacheAdapter : ILayeredCacheAdapter
    {
        private const string CLEAR_ALL = "CLEAR_ALL";
        private const int CACHE_TIME_MINUTES = 30;

        private readonly IMemoryCacheAdapter _memoryCacheAdapter;
        private readonly IRedisCacheAdapter _redisCacheAdapter;

        public LayeredCacheAdapter(IMemoryCacheAdapter memoryCacheAdapter, IRedisCacheAdapter redisCacheAdapter)
        {
            _memoryCacheAdapter = memoryCacheAdapter;
            _redisCacheAdapter = redisCacheAdapter;
            _redisCacheAdapter.AssignClearCacheAction(ProcessAction);
        }

        public T Get<T>(string cacheKey)
        {
            var cachedResult = _memoryCacheAdapter.Get<T>(cacheKey);

            // If not in memory, check redis
            if (EqualityComparer<T>.Default.Equals(cachedResult, default(T)))
            {
                cachedResult = _redisCacheAdapter.Get<T>(cacheKey);

                // Put it in memory cache
                if (!EqualityComparer<T>.Default.Equals(cachedResult, default(T)))
                {
                    _memoryCacheAdapter.Set(cacheKey, cachedResult);
                }
            }

            return cachedResult;
        }

        public T Get<T>(string cacheKey, Func<T> action)
        {
            return Get<T>(cacheKey, CACHE_TIME_MINUTES, action);
        }


        public T Get<T>(string cacheKey, int cacheTimeInMinutes, Func<T> action)
        {
            var cachedResult = Get<T>(cacheKey);
            if (!EqualityComparer<T>.Default.Equals(cachedResult, default(T)))
            {
                return cachedResult;
            }

            var result = action.Invoke();

            Set(cacheKey, result, cacheTimeInMinutes);

            return result;
        }

        public void Set(string cacheKey, object obj)
        {
            Set(cacheKey, obj, CACHE_TIME_MINUTES);
        }

        public void Set(string cacheKey, object obj, int minutes)
        {
            _redisCacheAdapter.Set(cacheKey, obj, minutes);
        }

        private void Clear(string cacheKey, bool memoryOnly)
        {
            _memoryCacheAdapter.Clear(cacheKey);

            if (memoryOnly)
            {
                return;
            }

            _redisCacheAdapter.Clear(cacheKey);
        }

        public void Clear(string cacheKey)
        {
            Clear(cacheKey, false);
        }

        private void ClearByPattern(string pattern, bool memoryOnly)
        {
            _memoryCacheAdapter.ClearByPattern(pattern);

            if (memoryOnly)
            {
                return;
            }

            _redisCacheAdapter.ClearByPattern(pattern);
        }

        public void ClearByPattern(string pattern)
        {
            ClearByPattern(pattern, false);
        }

        private void ClearAll(bool memoryOnly)
        {
            _memoryCacheAdapter.ClearAll();

            if (memoryOnly)
            {
                return;
            }

            _redisCacheAdapter.ClearAll();
        }

        public void ClearAll()
        {
            ClearAll(false);
        }

        private void ProcessAction(string key, bool byPattern)
        {
            if (key == CLEAR_ALL)
            {
                ClearAll(true);
                return;
            }

            if (byPattern)
            {
                ClearByPattern(key, true);
                return;
            }

            Clear(key, true);
        }

        public bool IsSet(string cacheKey)
        {
            return _redisCacheAdapter.IsSet(cacheKey);
        }
    }
}
