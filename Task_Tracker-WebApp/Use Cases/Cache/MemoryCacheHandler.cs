using Microsoft.Extensions.Caching.Memory;
using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Use_Cases.Cache
{
    public class MemoryCacheHandler : IMemoryCacheHandler
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options;

        public MemoryCacheHandler
            (IMemoryCache cache,
            IConfiguration config)
        {
            _cache = cache;

            int slidingExpMins;
            if(!int.TryParse(config.GetSection("CacheSettings")["SlidingTimeInMinutes"], out slidingExpMins))
                slidingExpMins = 30;

            int absoluteExpHours;
            if (!int.TryParse(config.GetSection("CacheSettings")["AbsoluteTimeInHours"], out absoluteExpHours))
                absoluteExpHours = 1;

            _options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpMins))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(absoluteExpHours));
        }

        public void Set<T>(CachePrefix prefixKey, string suffixKey, T value)
            => _cache.Set(getPrefix(prefixKey) + suffixKey, value, _options);

        public bool Get<T>(CachePrefix prefixKey, string suffixKey, out T? value)
        {
            if(!_cache.TryGetValue(getPrefix(prefixKey) + suffixKey, out value))
                return false;

            return true;
        }

        public void Remove(CachePrefix prefixKey, string suffixKey)
            => _cache.Remove(getPrefix(prefixKey) + suffixKey);

        private string getPrefix(CachePrefix prefix)
        {
            return prefix switch
            {
                CachePrefix.UserTaskList => "UserTaskList_",
                CachePrefix.UserTask => "UserTask_",
                _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, null)
            };
        }
    }
}
