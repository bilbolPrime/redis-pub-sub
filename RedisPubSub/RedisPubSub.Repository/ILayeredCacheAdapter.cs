﻿namespace BilbolStack.RedisPubSub.Repository
{
    public interface ILayeredCacheAdapter
    {
        T Get<T>(string cacheKey);

        T Get<T>(string cacheKey, Func<T> action);
        T Get<T>(string cacheKey, int cacheTimeInMinutes, Func<T> action);
        void Set(string cacheKey, object obj);

        void Set(string cacheKey, object obj, int minutes);
        void Clear(string cacheKey);
        void ClearByPattern(string pattern);
        void ClearAll();
        bool IsSet(string cacheKey);
    }
}
