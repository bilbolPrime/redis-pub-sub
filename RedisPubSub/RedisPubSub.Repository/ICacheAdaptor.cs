namespace BilbolStack.RedisPubSub.Repository
{
    public interface ICacheAdapter
    {
        T Get<T>(string cacheKey);

        T Get<T>(string cacheKey, Func<T> action);
        void Set(string cacheKey, object obj);

        void Set(string cacheKey, object obj, int minutes);
        void Clear(string cacheKey);
        void ClearByPattern(string pattern);
        void ClearAll();
        bool IsSet(string cacheKey);
    }
}
