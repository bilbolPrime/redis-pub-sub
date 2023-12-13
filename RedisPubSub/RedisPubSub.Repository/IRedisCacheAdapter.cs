namespace BilbolStack.RedisPubSub.Repository
{
    public interface IRedisCacheAdapter : ICacheAdapter
    {
        void AssignClearCacheAction(Action<string, bool> action);
    }
}
