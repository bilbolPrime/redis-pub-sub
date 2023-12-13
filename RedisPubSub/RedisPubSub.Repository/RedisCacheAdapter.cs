using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace BilbolStack.RedisPubSub.Repository
{
    public class RedisCacheAdapter : IRedisCacheAdapter
    {
        private const string CHANNEL_NAME = "CHANNEL-NAME";
        private const string CLEAR_ALL = "CLEAR_ALL";
        private const int CACHE_TIME_MINUTES = 30;

        private readonly ConfigurationOptions _configurationOptions;
        private readonly Guid _serverGuid;
        private IConnectionMultiplexer _connection;
        private ISubscriber _subscriber;
        private Action<string, bool> ClearCacheAction;

        public RedisCacheAdapter(IOptions<RedisSettings> chainSettings)
        {
            var tokens = chainSettings.Value.ConnectionString.Split(':', '@');
            _configurationOptions =
                !tokens[3].Contains("localhost") ?
                ConfigurationOptions.Parse(string.Format("{0}:{1},password={2}", tokens[3], tokens[4], tokens[2]))
                : ConfigurationOptions.Parse(string.Format("{0},password={1}", tokens[3], tokens[2]));
            _configurationOptions.AllowAdmin = true;
            _configurationOptions.AbortOnConnectFail = true;
            _configurationOptions.Ssl = !tokens[3].Contains("localhost");
            _serverGuid = Guid.NewGuid();
            InitPubSub();
        }

        public void Clear(string cacheKey)
        {
            var db = GetConnection().GetDatabase();
            db.KeyDelete(cacheKey);
            PublishClearCache(cacheKey, false);
        }

        public void ClearAll()
        {
            foreach (var endpoint in GetConnection().GetEndPoints())
            {
                var server = GetConnection().GetServer(endpoint);
                server.FlushAllDatabases();
            }
            PublishClearCache(CLEAR_ALL, false);
        }

        public void ClearByPattern(string pattern)
        {
            foreach (var endPoint in GetConnection().GetEndPoints())
            {
                var server = GetConnection().GetServer(endPoint);
                var db = GetConnection().GetDatabase();
                foreach (var key in server.Keys(pattern: pattern))
                {
                    db.KeyDelete(key);
                }
            }

            PublishClearCache(pattern, true);
        }

        public T Get<T>(string cacheKey)
        {
            var db = GetConnection().GetDatabase();
            if (!db.KeyExists(cacheKey))
            {
                return default;
            }

            var cacheEntry = db.StringGet(cacheKey);

            // Extra protection for that one nano second...
            if (string.IsNullOrEmpty(cacheEntry))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cacheEntry);
        }

        public T Get<T>(string cacheKey, Func<T> action)
        {
            var cachedResult = Get<T>(cacheKey);
            if (!EqualityComparer<T>.Default.Equals(cachedResult, default(T)))
            {
                return cachedResult;
            }

            var result = action.Invoke();

            Set(cacheKey, result);

            return result;
        }

        public void Set(string cacheKey, object obj)
        {
            Set(cacheKey, obj, CACHE_TIME_MINUTES);
        }

        public void Set(string cacheKey, object obj, int minutes)
        {
            var db = GetConnection().GetDatabase();
            db.StringSet(cacheKey, JsonSerializer.Serialize(obj), TimeSpan.FromMinutes(minutes));

            PublishClearCache(cacheKey, false);
        }

        private void InitPubSub()
        {
            _subscriber = GetConnection().GetSubscriber();
            _subscriber.Subscribe(CHANNEL_NAME, (channel, message) =>
            {
                ReceivePublish(message);
            });
        }

        private void PublishClearCache(string key, bool byPattern)
        {
            var message = JsonSerializer.Serialize(new ClearCacheDTO() { Source = _serverGuid, Key = key, ByPattern = byPattern });
            _subscriber.Publish(CHANNEL_NAME, message);
        }

        private void ReceivePublish(string message)
        {
            var decodedMessage = JsonSerializer.Deserialize<ClearCacheDTO>(message);
            if (decodedMessage.Source == _serverGuid)
            {
                return;
            }

            if (ClearCacheAction != null)
            {
                ClearCacheAction(decodedMessage.Key, decodedMessage.ByPattern);
            }
        }

        public void AssignClearCacheAction(Action<string, bool> action)
        {
            ClearCacheAction = action;
        }

        private static object _lockObj = new object();
        private IConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected) return _connection;
            lock (_lockObj)
            {
                if (_connection != null && _connection.IsConnected)
                {
                    return _connection;
                }

                if (_connection != null)
                {
                    _connection.Dispose();
                }

                _connection = ConnectionMultiplexer.Connect(_configurationOptions);
                return _connection;
            }
        }

        public bool IsSet(string cacheKey)
        {
            var db = GetConnection().GetDatabase();
            return db.KeyExists(cacheKey);
        }
    }
}
