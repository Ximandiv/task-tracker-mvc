using Microsoft.Extensions.Caching.Memory;
using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Cache
{
    public class MemoryCacheHandler
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options;

        private readonly int slidingExpMins = 30;
        private readonly int absoluteExpHours = 1;

        public MemoryCacheHandler(IMemoryCache cache)
        {
            _cache = cache;
            _options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpMins))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(absoluteExpHours));
        }

        public void Set<T>(CachePrefix prefixKey, int userId, T value) => _cache.Set(getPrefix(prefixKey) + userId, value, _options);

        public bool Get<T>(CachePrefix prefixKey, int userId, out T? value)
        {
            if(!_cache.TryGetValue(getPrefix(prefixKey) + userId, out value))
                return false;

            return true;
        }

        public void Remove(CachePrefix prefixKey, int userId) => _cache.Remove(getPrefix(prefixKey) + userId);

        private string getPrefix(CachePrefix prefix)
        {
            return prefix switch
            {
                CachePrefix.UserTasks => "UserTasks_",
                _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, null)
            };
        }
    }
}
