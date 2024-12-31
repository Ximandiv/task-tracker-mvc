using StackExchange.Redis;
using System.Text.Json;
using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Use_Cases.Cache
{
    public class CacheHandler
        (IConnectionMultiplexer connectionMultiplexer,
        IConfiguration config)
        : ICacheHandler
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;
        private readonly TimeSpan _expirationTime = TimeSpan
                                                        .FromMinutes
                                                            (int.Parse
                                                                (config
                                                                    .GetSection("CacheSettings")
                                                                                ["ExpirationTimeInMinutes"]!
                                                                )
                                                            );

        public async Task Set<T>
            (CachePrefix prefixKey,
            string suffixKey,
            T value) where T : class
        {
            var jsonValue = JsonSerializer.Serialize(value);

            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync($"{prefixKey.ToString()}_{suffixKey}", jsonValue, _expirationTime);
        }

        public async Task SetList<T>
            (CachePrefix prefixKey,
            string suffixKey,
            IEnumerable<T> value) where T : class
        {
            var jsonValue = JsonSerializer.Serialize(value);

            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync($"{prefixKey.ToString()}_{suffixKey}", jsonValue, _expirationTime);
        }

        public async Task<T?> Get<T>
            (CachePrefix prefixKey,
            string suffixKey) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();

            if (await KeyExists(prefixKey, suffixKey))
            {
                var value = await db.StringGetAsync($"{prefixKey.ToString()}_{suffixKey}");

                return JsonSerializer.Deserialize<T>(value!);
            }

            return null;
        }

        public async Task<IEnumerable<T>?> GetList<T>
            (CachePrefix prefixKey,
            string suffixKey) where T : class
        {
            var db = _connectionMultiplexer.GetDatabase();

            if (await KeyExists(prefixKey, suffixKey))
            {
                var value = await db.StringGetAsync($"{prefixKey.ToString()}_{suffixKey}");

                return JsonSerializer.Deserialize<IEnumerable<T>>(value!);
            }

            return null;
        }

        public async Task<bool> KeyExists(CachePrefix prefixKey, string suffixKey)
            => await _connectionMultiplexer
                .GetDatabase()
                    .KeyExistsAsync($"{prefixKey.ToString()}_{suffixKey}");

        public async Task Remove(CachePrefix prefixKey, string suffixKey)
        {
            var db = _connectionMultiplexer.GetDatabase();

            await db.KeyDeleteAsync($"{prefixKey.ToString()}_{suffixKey}");
        }
    }
}
