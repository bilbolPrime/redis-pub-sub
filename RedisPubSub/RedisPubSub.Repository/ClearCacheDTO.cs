namespace BilbolStack.RedisPubSub.Repository
{
    public class ClearCacheDTO
    {
        public Guid Source { get; set; }
        public string Key { get; set; }
        public bool ByPattern { get; set; }
    }
}
